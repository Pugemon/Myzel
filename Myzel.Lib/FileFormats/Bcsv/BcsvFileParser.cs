using System.Collections;
using System.Reflection;
using Myzel.Lib.FileFormats.Bcsv.Attributes;
using Myzel.Lib.FileFormats.Bcsv.Converters;
using Myzel.Lib.Utils;

namespace Myzel.Lib.FileFormats.Bcsv;

/// <summary>
/// A class for parsing BCSV files.
/// </summary>
public class BcsvFileParser : IFileParser<BcsvFile>
{
    #region private members
    private readonly Dictionary<string, BcsvHeaderInfo> _headerInfo = new(StringComparer.OrdinalIgnoreCase);
    #endregion

    #region constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="BcsvFileParser"/> class without header information.
    /// </summary>
    public BcsvFileParser()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BcsvFileParser"/> class with additional header information.
    /// </summary>
    /// <param name="headerInfo">The header information to use.</param>
    public BcsvFileParser(IEnumerable<BcsvHeaderInfo>? headerInfo)
    {
        if (headerInfo is null) return;
        foreach (BcsvHeaderInfo info in headerInfo) _headerInfo[info.HeaderName] = info;
    }
    #endregion

    #region IFileParser interface
    /// <inheritdoc/>
    public bool CanParse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        return CanParse(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public BcsvFile Parse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        FileReader reader = new(fileStream);
        if (!CanParse(reader)) throw new InvalidDataException("File is not a BCSV file.");

        //parse file metadata and header
        GetMetaData(reader, out int rowCount, out int colCount, out int dataSize);
        GetHeaderInfo(reader, colCount, out string[] headerStrings, out int[] offsets);
        GetDataSizes(colCount, dataSize, offsets, out int[] sizes);

        //build header cache
        BcsvHeaderInfo[] header = new BcsvHeaderInfo[headerStrings.Length];
        for (int i = 0; i < headerStrings.Length; ++i)
        {
            if (_headerInfo.TryGetValue(headerStrings[i], out BcsvHeaderInfo? value))
            {
                BcsvHeaderInfo val = value;
                if (val.DataType == BcsvDataType.Default) val = new BcsvHeaderInfo(val.HeaderName, val.NewHeaderName, GetBestFitType(sizes[i]), val.Converter);
                header[i] = val;
            }
            else header[i] = new BcsvHeaderInfo(headerStrings[i], GetBestFitType(sizes[i]));
        }

        //parse data
        object?[][] cells = new object?[rowCount][];
        for (int i = 0; i < rowCount; ++i)
        {
            //get current row offset in stream (28 = offset from meta data)
            int offset = 28 + colCount * 8 + i * dataSize;
            object?[] row = new object?[colCount];

            for (int j = 0; j < colCount; ++j)
            {
                //get current column/data offset
                int from = offset + offsets[j];

                //read value
                BcsvHeaderInfo info = header[j];
                row[j] = info.Converter is not null ? info.Converter.Convert(reader.ReadBytesAt(from, sizes[j]), info.DataType.GetSystemType()) : ReadByType(reader, info.DataType, from, sizes[j]);
            }

            cells[i] = row;
        }

        return new BcsvFile(header, cells);
    }
    #endregion

    #region public methods
    /// <summary>
    /// Deserializes a BCSV file into a given type.
    /// Type must have a public default constructor.
    /// Returns a list of instances, where each instance represents a data row.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize into.</typeparam>
    /// <param name="filePath">The filepath of the BCSV file.</param>
    /// <param name="converters">A list of <see cref="IBcsvConverter"/> instances to use for deserialization.</param>
    /// <returns>A list of new instances.</returns>
    public static IList<T> Deserialize<T>(string filePath, IEnumerable<IBcsvConverter>? converters = null) where T : new()
    {
        ArgumentNullException.ThrowIfNull(filePath);

        return Deserialize<T>(File.OpenRead(filePath), false, converters);
    }

    /// <summary>
    /// Deserializes a BCSV file into a given type.
    /// Type must have a public default constructor.
    /// Returns a list of instances, where each instance represents a data row.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize into.</typeparam>
    /// <param name="data">The content of the BCSV file.</param>
    /// <param name="converters">A list of <see cref="IBcsvConverter"/> instances to use for deserialization.</param>
    /// <returns>A list of new instances.</returns>
    public static IList<T> Deserialize<T>(byte[] data, IEnumerable<IBcsvConverter>? converters = null) where T : new()
    {
        ArgumentNullException.ThrowIfNull(data);

        return Deserialize<T>(new MemoryStream(data), false, converters);
    }

    /// <summary>
    /// Deserializes a BCSV file into a given type.
    /// Type must have a public default constructor.
    /// Returns a list of instances, where each instance represents a data row.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize into.</typeparam>
    /// <param name="fileStream">The BCSV file stream to read from.</param>
    /// <param name="converters">A list of <see cref="IBcsvConverter"/> instances to use for deserialization.</param>
    /// <returns>A list of new instances.</returns>
    public static IList<T> Deserialize<T>(Stream fileStream, IEnumerable<IBcsvConverter>? converters = null) where T : new() => Deserialize<T>(fileStream, false, converters);

    /// <summary>
    /// Deserializes a BCSV file into a given type.
    /// Type must have a public default constructor.
    /// Returns a list of instances, where each instance represents a data row.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize into.</typeparam>
    /// <param name="filePath">The filepath of the BCSV file.</param>
    /// <param name="printUnusedHeaders">Whether to print unused headers to the console.</param>
    /// <param name="converters">A list of <see cref="IBcsvConverter"/> instances to use for deserialization.</param>
    /// <returns>A list of new instances.</returns>
    public static IList<T> Deserialize<T>(string filePath, bool printUnusedHeaders, IEnumerable<IBcsvConverter>? converters = null) where T : new()
    {
        ArgumentNullException.ThrowIfNull(filePath);

        return Deserialize<T>(File.OpenRead(filePath), printUnusedHeaders, converters);
    }

    /// <summary>
    /// Deserializes a BCSV file into a given type.
    /// Type must have a public default constructor.
    /// Returns a list of instances, where each instance represents a data row.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize into.</typeparam>
    /// <param name="data">The content of the BCSV file.</param>
    /// <param name="printUnusedHeaders">Whether to print unused headers to the console.</param>
    /// <param name="converters">A list of <see cref="IBcsvConverter"/> instances to use for deserialization.</param>
    /// <returns>A list of new instances.</returns>
    public static IList<T> Deserialize<T>(byte[] data, bool printUnusedHeaders, IEnumerable<IBcsvConverter>? converters = null) where T : new()
    {
        ArgumentNullException.ThrowIfNull(data);

        return Deserialize<T>(new MemoryStream(data), printUnusedHeaders, converters);
    }

    /// <summary>
    /// Deserializes a BCSV file into a given type.
    /// Type must have a public default constructor.
    /// Returns a list of instances, where each instance represents a data row.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize into.</typeparam>
    /// <param name="fileStream">The BCSV file stream to read from.</param>
    /// <param name="printUnusedHeaders">Whether to print unused headers to the console.</param>
    /// <param name="converters">A list of <see cref="IBcsvConverter"/> instances to use for deserialization.</param>
    /// <returns>A list of new instances.</returns>
    private static IList<T> Deserialize<T>(Stream fileStream, bool printUnusedHeaders, IEnumerable<IBcsvConverter>? converters = null) where T : new() => (IList<T>) Deserialize(typeof(T), fileStream, printUnusedHeaders, converters);

    /// <summary>
    /// Deserializes a BCSV file into a given type.
    /// Type must have a public default constructor.
    /// Returns a list of instances, where each instance represents a data row.
    /// </summary>
    /// <param name="targetType">The target type to deserialize into.</param>
    /// <param name="filePath">The filepath of the BCSV file.</param>
    /// <param name="converters">A list of <see cref="IBcsvConverter"/> instances to use for deserialization.</param>
    /// <returns>A list of new instances.</returns>
    public static IList Deserialize(Type targetType, string filePath, IEnumerable<IBcsvConverter>? converters = null)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        return Deserialize(targetType, File.OpenRead(filePath), false, converters);
    }

    /// <summary>
    /// Deserializes a BCSV file into a given type.
    /// Type must have a public default constructor.
    /// Returns a list of instances, where each instance represents a data row.
    /// </summary>
    /// <param name="targetType">The target type to deserialize into.</param>
    /// <param name="data">The content of the BCSV file.</param>
    /// <param name="converters">A list of <see cref="IBcsvConverter"/> instances to use for deserialization.</param>
    /// <returns>A list of new instances.</returns>
    public static IList Deserialize(Type targetType, byte[] data, IEnumerable<IBcsvConverter>? converters = null)
    {
        ArgumentNullException.ThrowIfNull(data);

        return Deserialize(targetType, new MemoryStream(data), false, converters);
    }

    /// <summary>
    /// Deserializes a BCSV file into a given type.
    /// Type must have a public default constructor.
    /// Returns a list of instances, where each instance represents a data row.
    /// </summary>
    /// <param name="targetType">The target type to deserialize into.</param>
    /// <param name="fileStream">The BCSV file stream to read from.</param>
    /// <param name="converters">A list of <see cref="IBcsvConverter"/> instances to use for deserialization.</param>
    /// <returns>A list of new instances.</returns>
    public static IList Deserialize(Type targetType, Stream fileStream, IEnumerable<IBcsvConverter>? converters = null) => Deserialize(targetType, fileStream, false, converters);

    /// <summary>
    /// Deserializes a BCSV file into a given type.
    /// Type must have a public default constructor.
    /// Returns a list of instances, where each instance represents a data row.
    /// </summary>
    /// <param name="targetType">The target type to deserialize into.</param>
    /// <param name="filePath">The filepath of the BCSV file.</param>
    /// <param name="printUnusedHeaders">Whether to print unused headers to the console.</param>
    /// <param name="converters">A list of <see cref="IBcsvConverter"/> instances to use for deserialization.</param>
    /// <returns>A list of new instances.</returns>
    public static IList Deserialize(Type targetType, string filePath, bool printUnusedHeaders, IEnumerable<IBcsvConverter>? converters = null)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        return Deserialize(targetType, File.OpenRead(filePath), printUnusedHeaders, converters);
    }

    /// <summary>
    /// Deserializes a BCSV file into a given type.
    /// Type must have a public default constructor.
    /// Returns a list of instances, where each instance represents a data row.
    /// </summary>
    /// <param name="targetType">The target type to deserialize into.</param>
    /// <param name="data">The content of the BCSV file.</param>
    /// <param name="printUnusedHeaders">Whether to print unused headers to the console.</param>
    /// <param name="converters">A list of <see cref="IBcsvConverter"/> instances to use for deserialization.</param>
    /// <returns>A list of new instances.</returns>
    public static IList Deserialize(Type targetType, byte[] data, bool printUnusedHeaders, IEnumerable<IBcsvConverter>? converters = null)
    {
        ArgumentNullException.ThrowIfNull(data);

        return Deserialize(targetType, new MemoryStream(data), printUnusedHeaders, converters);
    }

    /// <summary>
    /// Deserializes a BCSV file into a given type.
    /// Type must have a public default constructor.
    /// Returns a list of instances, where each instance represents a data row.
    /// </summary>
    /// <param name="targetType">The target type to deserialize into.</param>
    /// <param name="fileStream">The BCSV file stream to read from.</param>
    /// <param name="printUnusedHeaders">Whether to print unused headers to the console.</param>
    /// <param name="converters">A list of <see cref="IBcsvConverter"/> instances to use for deserialization.</param>
    /// <returns>A list of new instances.</returns>
    private static IList Deserialize(Type targetType, Stream fileStream, bool printUnusedHeaders, IEnumerable<IBcsvConverter>? converters = null)
    {
        if (!targetType.IsClass) throw new ArgumentException("Target type is not a class.");
        if (targetType.IsAbstract) throw new ArgumentException("Target type is abstract.");
        if (targetType.GetConstructor(Type.EmptyTypes) is null) throw new ArgumentException("Target type doesn't have a default or parameter-less constructor.");
        ArgumentNullException.ThrowIfNull(fileStream);
        converters ??= Array.Empty<IBcsvConverter>();

        FileReader reader = new(fileStream);
        if (!CanParse(reader)) throw new InvalidDataException("File is not a BCSV file.");

        //parse file metadata and header
        GetMetaData(reader, out int rowCount, out int colCount, out int dataSize);
        GetHeaderInfo(reader, colCount, out string[] header, out int[] offsets);
        GetDataSizes(colCount, dataSize, offsets, out int[] sizes);

        //get type cache
        HeaderInfo?[] typeCache = BuildTypeCache(targetType, header, converters, printUnusedHeaders);
        if (printUnusedHeaders)
        {
            for (int i = 0; i < typeCache.Length; ++i)
            {
                if (typeCache[i] is not null) continue;
                if (i > 0) Console.WriteLine("Unused header after " + header[i - 1].ToLower() + ": " + header[i].ToLower() + " [" + sizes[i] + " bytes]");
                else Console.WriteLine("Unused header at the start of file: " + header[i].ToLower() + " [" + sizes[i] + " bytes]");
            }
        }

        //parse entries
        IList entries = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType))!;
        for (int i = 0; i < rowCount; ++i)
        {
            object? entry = Activator.CreateInstance(targetType);

            //get current row offset in stream (28 = offset from meta data)
            int offset = 28 + colCount * 8 + i * dataSize;

            for (int j = 0; j < colCount; ++j)
            {
                HeaderInfo? info = typeCache[j];
                if (info is null) continue;

                //get current column/data offset
                int from = offset + offsets[j];

                Type type = info.Property.PropertyType;
                object? value;

                //read value
                if (info.Converter is not null) value = info.Converter.Convert(reader.ReadBytesAt(from, sizes[j]), type);
                else if (info.DataType == BcsvDataType.Default) value = ReadByType(reader, type, from, sizes[j]);
                else value = ReadByType(reader, info.DataType, from, sizes[j]);
                if (value is null) continue;

                //convert if target type does not match
                if (value.GetType() != type) value = type.IsEnum ? Enum.ToObject(type, value) : Convert.ChangeType(value, type);

                //set property value
                info.Property.SetValue(entry, value);
            }

            entries.Add(entry);
        }

        return entries;
    }

    /// <summary>
    /// Gets the list of headers from a BCSV file.
    /// </summary>
    /// <param name="filePath">The filepath of the BCSV file.</param>
    /// <returns>A list of column names.</returns>
    public static string[] GetHeaders(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        return GetHeaders(File.OpenRead(filePath));
    }

    /// <summary>
    /// Gets the list of headers from a BCSV file.
    /// </summary>
    /// <param name="data">The content of the BCSV file.</param>
    /// <returns>A list of column names.</returns>
    public static string[] GetHeaders(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return GetHeaders(new MemoryStream(data, false));
    }

    /// <summary>
    /// Gets the list of headers from a BCSV file.
    /// </summary>
    /// <param name="fileStream">The BCSV file stream to read from.</param>
    /// <returns>A list of column names.</returns>
    private static string[] GetHeaders(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        FileReader reader = new FileReader(fileStream);
        if (!CanParse(reader)) throw new FileLoadException("File is not a BCSV file.");

        //parse file metadata and header
        short colCount = reader.ReadInt16At(8);
        GetHeaderInfo(reader, colCount, out var header, out _);

        return header;
    }
    #endregion

    #region private methods
    //verifies that the file is a BCSV file
    private static bool CanParse(FileReader reader) => reader.ReadStringAt(12, 4) == "VSCB";

    //parses meta data
    private static void GetMetaData(FileReader reader, out int rowCount, out int colCount, out int dataSize /*out int revision*/)
    {
        rowCount = reader.ReadInt32At(0);
        colCount = reader.ReadInt16At(8);
        dataSize = reader.ReadInt32At(4);
        //revision = reader.ReadInt16At(16);
    }

    //parses header information
    private static void GetHeaderInfo(FileReader reader, int colCount, out string[] header, out int[] offsets)
    {
        header = new string[colCount];
        offsets = new int[colCount];

        for (int i = 0; i < colCount; ++i)
        {
            header[i] = reader.ReadHexStringAt(28 + i * 8, 4); //28 = offset from meta data
            offsets[i] = reader.ReadInt32At(32 + i * 8);
        }
    }

    //gets data block sizes
    private static void GetDataSizes(int colCount, int dataSize, IReadOnlyList<int> offsets, out int[] sizes)
    {
        sizes = new int[colCount];
        for (int i = 0; i < colCount; ++i)
        {
            sizes[i] = (i + 1 < colCount ? offsets[i + 1] : dataSize) - offsets[i];
        }
    }

    //gets the best fitting data type for a given data size
    private static BcsvDataType GetBestFitType(int size)
    {
        switch (size)
        {
            case 0:
                return BcsvDataType.Default;
            case 1:
            case 2:
                return BcsvDataType.SignedInt32;
            case 4:
            case 5:
            case 16:
                return BcsvDataType.Crc32;
            default:
                return BcsvDataType.String;
        }
    }

    //reads a value from stream as a given data-type
    private static object? ReadByType(FileReader reader, BcsvDataType dataType, int from, int length)
    {
        if (length == 0) return null;

        return dataType switch
        {
            BcsvDataType.SignedInt8    => reader.ReadSByteAt(from, length),
            BcsvDataType.UnsignedInt8  => reader.ReadByteAt(from, length),
            BcsvDataType.SignedInt16   => reader.ReadInt16At(from, length),
            BcsvDataType.UnsignedInt16 => reader.ReadUInt16At(from, length),
            BcsvDataType.SignedInt32   => reader.ReadInt32At(from, length),
            BcsvDataType.UnsignedInt32 => reader.ReadUInt32At(from, length),
            BcsvDataType.SignedInt64   => reader.ReadInt64At(from, length),
            BcsvDataType.UnsignedInt64 => reader.ReadUInt64At(from, length),
            BcsvDataType.Float32       => reader.ReadSingleAt(from, length),
            BcsvDataType.Float64       => reader.ReadDoubleAt(from, length),
            BcsvDataType.Crc32         => reader.ReadHexStringAt(from, length),
            BcsvDataType.Mmh3          => reader.ReadHexStringAt(from, length),
            BcsvDataType.String        => reader.ReadStringAt(from, length),
            _ => null
        };
    }

    //reads a value from stream as a given data-type
    private static object? ReadByType(FileReader reader, Type dataType, int from, int length)
    {
        if (length == 0) return null;

        return dataType.Name switch
        {
            nameof(SByte)  => reader.ReadSByteAt(from, length),
            nameof(Byte)   => reader.ReadByteAt(from, length),
            nameof(Int16)  => reader.ReadInt16At(from, length),
            nameof(UInt16) => reader.ReadUInt16At(from, length),
            nameof(Int32)  => reader.ReadInt32At(from, length),
            nameof(UInt32) => reader.ReadUInt32At(from, length),
            nameof(Int64)  => reader.ReadInt64At(from, length),
            nameof(UInt64) => reader.ReadUInt64At(from, length),
            nameof(Single) => reader.ReadSingleAt(from, length),
            nameof(Double) => reader.ReadDoubleAt(from, length),
            nameof(String) => length == 16 ? reader.ReadHexStringAt(from, length) : reader.ReadStringAt(from, length),
            _ => dataType.IsEnum ? reader.ReadInt32At(from, length) : null
        };
    }

    //builds a lookup table for BCSV headers to type properties
    private static HeaderInfo?[] BuildTypeCache(Type fileType, IReadOnlyList<string> header, IEnumerable<IBcsvConverter> converters, bool printUnusedHeaders)
    {
        //build converter cache from instances
        Dictionary<Type, IBcsvConverter> converterCache = new();
        foreach (IBcsvConverter converter in converters)
        {
            Type converterType = converter.GetType();
            converterCache.TryAdd(converterType, converter);

        }

        HeaderInfo?[] typeCache = new HeaderInfo?[header.Count];

        //build type cache from type properties
        foreach (PropertyInfo prop in fileType.GetProperties())
        {
            //ignore non-writable properties and properties with BcsvIgnore attribute
            if (!prop.CanWrite || Attribute.IsDefined(prop, typeof(BcsvIgnoreAttribute))) continue;

            int index = -1;
            BcsvDataType dataType = BcsvDataType.Default;
            IBcsvConverter? converter = null;

            //check for BcsvHeader attribute
            BcsvHeaderAttribute? bcsvHeader = prop.GetCustomAttribute<BcsvHeaderAttribute>(true);
            if (bcsvHeader is not null)
            {
                dataType = bcsvHeader.DataType;
                if (bcsvHeader.Name is not null) index = IndexOfCleaned(header, bcsvHeader.Name);

                //check if instance of converter already exists or create new instance
                if (bcsvHeader.Converter is not null)
                {
                    if (converterCache.TryGetValue(bcsvHeader.Converter, out IBcsvConverter? value)) converter = value;
                    else
                    {
                        converter = (IBcsvConverter) Activator.CreateInstance(bcsvHeader.Converter)!;
                        converterCache.Add(converter.GetType(), converter);
                    }
                }

                if (printUnusedHeaders && index == -1) Console.WriteLine("Header not found: " + prop.Name + " (" + bcsvHeader.Name + ")");
            }

            //do normal property name check
            if (index == -1) index = IndexOfCleaned(header, prop.Name);

            if (index == -1) continue;

            typeCache[index] = new HeaderInfo {Property = prop, DataType = dataType, Converter = converter};
        }

        return typeCache;
    }

    //gets the index of a value within a string array but compares case-insensitive
    private static int IndexOfCleaned(IReadOnlyList<string> array, string value)
    {
        value = CleanString(value);

        for (int i = 0; i < array.Count; ++i)
        {
            string val = CleanString(array[i]);
            if (value == val) return i;
        }

        return -1;
    }

    //normalizes a string for comparison (lower-case, remove _-)
    private static string CleanString(string str) => str.ToLowerInvariant().Trim().Replace("_", "").Replace("-", "");
    #endregion

    #region helper class
    private class HeaderInfo
    {
        public PropertyInfo Property { get; init; } = null!;

        public BcsvDataType DataType { get; init; }

        public IBcsvConverter? Converter { get; init; }
    }
    #endregion
}
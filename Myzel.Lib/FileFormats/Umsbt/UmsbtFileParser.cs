using Myzel.Lib.FileFormats.Msbt;
using Myzel.Lib.Utils;

namespace Myzel.Lib.FileFormats.Umsbt;

/// <summary>
/// A class for parsing UMSBT archives.
/// </summary>
public class UmsbtFileParser : IFileParser<IList<MsbtFile>>
{
    #region private members
    private static readonly MsbtFileParser MsbtParser = new();
    private readonly string[] _languages;
    #endregion

    #region constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="UmsbtFileParser"/> class without any language mappings.
    /// </summary>
    public UmsbtFileParser() : this(null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UmsbtFileParser"/> class with the given languages.
    /// Each language maps to the MSBT file within the archive in the same order.
    /// </summary>
    /// <param name="languages">The list of languages found in the UMSBT archive.</param>
    public UmsbtFileParser(IEnumerable<string>? languages) => _languages = languages is null ? Array.Empty<string>() : languages.ToArray();
    #endregion

    #region IFileParser interface
    /// <inheritdoc/>
    public bool CanParse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        return CanParse(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public IList<MsbtFile> Parse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        FileReader reader = new(fileStream);

        //find file offsets and sizes
        GetMetaData(reader, out uint[] offsets, out uint[] sizes);

        //read UMSBT files
        List<MsbtFile> result = [];
        for (int i = 0; i < offsets.Length; ++i)
        {
            string? language = i < _languages.Length ? _languages[i] : null;
            MsbtFileParser parser = new(language);
            StreamSpan stream = new(reader.BaseStream, offsets[i], sizes[i]);
            try
            {
                result.Add(parser.Parse(stream));
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("File is not an UMSBT file.", ex);
            }
        }

        return result;
    }
    #endregion

    #region private methods
    //verifies that the file is a UMSBT file
    private static bool CanParse(FileReader reader)
    {
        GetMetaData(reader, out uint[] offsets, out uint[] sizes);
        return offsets.Length != 0 && offsets.Select((t, i) => new StreamSpan(reader.BaseStream, t, sizes[i])).All(stream => MsbtParser.CanParse(stream));

    }

    //parses meta data
    private static void GetMetaData(FileReader reader, out uint[] offsets, out uint[] sizes)
    {
        List<uint> offsetList = [];
        List<uint> sizeList = [];
        offsets = [];
        sizes = [];

        try
        {
            uint dataStart = reader.ReadUInt32At(0);
            reader.Position = 0;

            while (reader.Position < dataStart)
            {
                uint offset = reader.ReadUInt32();
                uint size = reader.ReadUInt32();

                if (offset + size > reader.BaseStream.Length) return;
                if (offset == 0 || size == 0) break;

                offsetList.Add(offset);
                sizeList.Add(size);
            }

            offsets = [..offsetList];
            sizes = [..sizeList];
        }
        catch
        {
            //ignored
        }
    }
    #endregion
}
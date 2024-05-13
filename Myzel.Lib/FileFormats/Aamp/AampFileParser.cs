using System.Text;
using Myzel.Lib.FileFormats.Aamp.Parameters;
using Myzel.Lib.Utils;

namespace Myzel.Lib.FileFormats.Aamp;

/// <summary>
/// A class for parsing AAMP files.
/// </summary>
public class AampFileParser : IFileParser<AampFile>
{
    #region IFileParser interface
    /// <inheritdoc/>
    public bool CanParse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        return CanParse(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public AampFile Parse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        FileReader reader = new FileReader(fileStream);
        if (!CanParse(reader)) throw new InvalidDataException("File is not an AAMP file.");

        AampFile aampFile = new AampFile {Version = reader.ReadInt32At(0x04)};

        uint flags = reader.ReadUInt32();
        reader.BigEndian = (flags & 1 << 0) == 0;
        Encoding encoding = (flags & 1 << 1) == 0 ? Encoding.ASCII : Encoding.UTF8;
        //var fileSize = reader.ReadUInt32();

        //var rootVersion = reader.ReadUInt32At(0x10);
        uint rootOffset = reader.ReadUInt32At(0x14);
        //var listCount = reader.ReadUInt32At(0x18);
        //var objCount = reader.ReadUInt32At(0x1c);
        //var paramCount = reader.ReadUInt32At(0x20);
        //var dataSize = reader.ReadUInt32(0x24);
        //var stringSize = reader.ReadUInt32(0x28);
        //var dataType = reader.ReadStringAt(0x30, (int) rootOffset);

        reader.JumpTo(0x30 + rootOffset);
        aampFile.Root = ReadList(reader, encoding);

        return aampFile;
    }
    #endregion

    #region private methods
    //verifies that the file is an AAMP file
    private static bool CanParse(FileReader reader) => reader.ReadStringAt(0, 4) == "AAMP";

    //read data as list node
    private static ParameterList ReadList(FileReader reader, Encoding encoding)
    {
        long offset = reader.Position;
        ParameterList list = new() {Name = reader.ReadHexString(4)};
        int listOffset = reader.ReadUInt16() * 4;
        ushort listCount = reader.ReadUInt16();
        int objOffset = reader.ReadUInt16() * 4;
        ushort objCount = reader.ReadUInt16();

        for (int i = 0; i < listCount; ++i)
        {
            reader.JumpTo(offset + listOffset + i * 12);
            list.Lists.Add(ReadList(reader, encoding));
        }

        for (int i = 0; i < objCount; ++i)
        {
            reader.JumpTo(offset + objOffset + i * 8);
            list.Objects.Add(ReadObject(reader, encoding));
        }

        return list;
    }

    //read data as object node
    private static ParameterObject ReadObject(FileReader reader, Encoding encoding)
    {
        long offset = reader.Position;
        ParameterObject obj = new() {Name = reader.ReadHexString(4)};
        int paramOffset = reader.ReadUInt16() * 4;
        ushort paramCount = reader.ReadUInt16();

        for (int i = 0; i < paramCount; ++i)
        {
            reader.JumpTo(offset + paramOffset + i * 8);
            obj.Parameters.Add(ReadParameter(reader, encoding));
        }

        return obj;
    }

    //read data as parameter
    private static Parameter ReadParameter(FileReader reader, Encoding encoding)
    {
        long offset = reader.Position;
        string name = reader.ReadHexString(4);
        uint dataOffset = reader.ReadUInt32(3) * 4;
        byte type = reader.ReadByte();

        Parameter parameter;
        switch (type)
        {
            case ParameterTypes.Bool:
                parameter = new ValueParameter
                {
                    Value = reader.ReadUInt32At(offset + dataOffset) != 0
                };
                break;
            case ParameterTypes.Int32:
                parameter = new ValueParameter
                {
                    Value = reader.ReadInt32At(offset + dataOffset)
                };
                break;
            case ParameterTypes.UInt32:
                parameter = new ValueParameter
                {
                    Value = reader.ReadUInt32At(offset + dataOffset)
                };
                break;
            case ParameterTypes.Float32:
                parameter = new ValueParameter
                {
                    Value = reader.ReadSingleAt(offset + dataOffset)
                };
                break;
            case ParameterTypes.String32:
                parameter = new ValueParameter
                {
                    Value = reader.ReadTerminatedStringAt(offset + dataOffset, encoding, 32)
                };
                break;
            case ParameterTypes.String64:
                parameter = new ValueParameter
                {
                    Value = reader.ReadTerminatedStringAt(offset + dataOffset, encoding, 64)
                };
                break;
            case ParameterTypes.String256:
                parameter = new ValueParameter
                {
                    Value = reader.ReadTerminatedStringAt(offset + dataOffset, encoding, 256)
                };
                break;
            case ParameterTypes.StringReference:
                parameter = new ValueParameter
                {
                    Value = reader.ReadHexStringAt(offset + dataOffset, 4)
                };
                break;
            case ParameterTypes.BinaryBuffer:
                parameter = new ValueParameter
                {
                    Value = reader.ReadBytes((int) reader.ReadUInt32At(dataOffset - 4))
                };
                break;
            case ParameterTypes.Int32Buffer:
                parameter = new ValueParameter
                {
                    Value = BuildArray(() => reader.ReadInt32(), (int) reader.ReadUInt32At(dataOffset - 4))
                };
                break;
            case ParameterTypes.UInt32Buffer:
                parameter = new ValueParameter
                {
                    Value = BuildArray(() => reader.ReadUInt32(), (int) reader.ReadUInt32At(dataOffset - 4))
                };
                break;
            case ParameterTypes.Float32Buffer:
                parameter = new ValueParameter
                {
                    Value = BuildArray(() => reader.ReadSingle(), (int) reader.ReadUInt32At(dataOffset - 4))
                };
                break;
            case ParameterTypes.Color:
                parameter = new ColorParameter
                {
                    Red = reader.ReadSingle(),
                    Green = reader.ReadSingle(),
                    Blue = reader.ReadSingle(),
                    Alpha = reader.ReadSingle()
                };
                break;
            case ParameterTypes.Vector2:
            case ParameterTypes.Vector3:
            case ParameterTypes.Vector4:
            case ParameterTypes.Quat:
                parameter = new ValueParameter
                {
                    Value = BuildArray(() => reader.ReadSingle(), GetValueArraySize(type))
                };
                break;
            case ParameterTypes.Curve1:
            case ParameterTypes.Curve2:
            case ParameterTypes.Curve3:
            case ParameterTypes.Curve4:
                parameter = new CurveParameter
                {
                    Curves = BuildList(() => new CurveValue
                    {
                        IntValues = BuildArray(() => reader.ReadUInt32(), 2),
                        FloatValues = BuildArray(() => reader.ReadSingle(), 30)
                    }, GetValueArraySize(type))
                };
                break;
            default:
                parameter = new ValueParameter
                {
                    Value = type
                };
                break;
        }

        parameter.Name = name;
        parameter.Type = type;
        return parameter;
    }

    private static int GetValueArraySize(byte type) => type switch
    {
        ParameterTypes.Curve1  => 1,
        ParameterTypes.Vector2 => 2,
        ParameterTypes.Curve2  => 2,
        ParameterTypes.Vector3 => 3,
        ParameterTypes.Curve3  => 3,
        ParameterTypes.Vector4 => 4,
        ParameterTypes.Quat    => 4,
        ParameterTypes.Curve4  => 4,
        _ => 0
    };

    private static T[] BuildArray<T>(Func<T> read, int length)
    {
        var data = new T[length];
        for (var i = 0; i < length; ++i) data[i] = read.Invoke();
        return data;
    }

    private static List<T> BuildList<T>(Func<T> read, int length)
    {
        List<T> data = [];
        for (int i = 0; i < length; ++i) data.Add(read.Invoke());
        return data;
    }
    #endregion
}
using Myzel.Lib.FileFormats.Byml.Nodes;
using Myzel.Lib.Utils;

namespace Myzel.Lib.FileFormats.Byml;

/// <summary>
/// A class for parsing BYML files.
/// </summary>
public class BymlFileParser : IFileParser<BymlFile>
{
    #region IFileParser interface
    /// <inheritdoc/>
    public bool CanParse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        return CanParse(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public BymlFile Parse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        FileReader reader = new(fileStream);
        if (!CanParse(reader)) throw new InvalidDataException("File is not a BYML file.");

        BymlFile bymlFile = new() {Version = reader.ReadInt16At(0x02)};

        //read header
        ReadHeader(reader, out Tables tables, out long rootNodeOffset);
        if (rootNodeOffset == 0)
        {
            bymlFile.RootNode = new NullNode();
            return bymlFile;
        }

        bymlFile.RootNode = ReadNode(reader, rootNodeOffset, reader.ReadByteAt(rootNodeOffset), tables);

        return bymlFile;
    }
    #endregion

    #region private methods
    //verifies that the file is a BYML file
    private static bool CanParse(FileReader reader)
    {
        switch (reader.ReadStringAt(0, 2))
        {
            case "BY":
                reader.BigEndian = true;
                return true;
            case "YB":
                reader.BigEndian = false;
                return true;
            default:
                return false;
        }
    }

    //parses header and tables
    private static void ReadHeader(FileReader reader, out Tables tables, out long rootNodeOffset)
    {
        tables = new Tables
        {
            Names = Array.Empty<string>(),
            Strings = Array.Empty<string>()
        };

        int nameTableOffset = reader.ReadInt32At(0x04);
        int stringTableOffset = reader.ReadInt32();
        int pathTableOffset = reader.ReadInt32();
        bool hasPaths = false;

        if (nameTableOffset > 0)
        {
            byte type = reader.ReadByteAt(nameTableOffset);
            if (type == NodeTypes.StringTable) tables.Names = ReadStringTable(reader, reader.Position);
        }

        if (stringTableOffset > 0)
        {
            byte type = reader.ReadByteAt(stringTableOffset);
            if (type == NodeTypes.StringTable) tables.Strings = ReadStringTable(reader, reader.Position);
        }

        if (pathTableOffset > 0) //only appears to exist in MarioKart 8
        {
            byte type = reader.ReadByteAt(pathTableOffset);
            if (type == NodeTypes.PathTable)
            {
                hasPaths = true;
                int length = reader.ReadInt32(3);

                uint[] offsets = new uint[length + 1];
                for (int i = 0; i < offsets.Length; ++i) offsets[i] = reader.ReadUInt32();

                tables.Paths = new PathNode[length];
                for (int i = 0; i < length; ++i)
                {
                    tables.Paths[i] = new PathNode
                    {
                        PositionX = reader.ReadSingleAt(offsets[i]),
                        PositionY = reader.ReadSingle(),
                        PositionZ = reader.ReadSingle(),
                        NormalX = reader.ReadSingle(),
                        NormalY = reader.ReadSingle(),
                        NormalZ = reader.ReadSingle()
                    };
                }
            }
        }

        rootNodeOffset = reader.ReadUInt32At(hasPaths ? 0x10 : 0x0c);
    }

    //parses a string table
    private static string[] ReadStringTable(FileReader reader, long offset)
    {
        uint length = reader.ReadUInt32At(offset, 3);
        string[] strings = new string[length];

        uint[] offsets = new uint[length + 1];
        for (int i = 0; i < offsets.Length; ++i) offsets[i] = reader.ReadUInt32();

        for (int i = 0; i < length; ++i) strings[i] = reader.ReadString((int) (offsets[i + 1] - offsets[i]));

        return strings;
    }

    //parses a generic node
    private static INode ReadNode(FileReader reader, long offset, byte type, Tables tables) => type switch
    {
        NodeTypes.HashDictionary      => ReadHashDictionaryNode(reader, offset, tables),
        NodeTypes.HashValueDictionary => ReadHashValueDictionaryNode(reader, offset, tables),

        NodeTypes.String            => ReadStringNode(reader, offset, tables),
        NodeTypes.BinaryData        => tables.Paths is null ? ReadBinaryNode(reader, offset) : ReadPathNode(reader, offset, tables),
        NodeTypes.AlignedBinaryData => ReadAlignedBinaryDataNode(reader, offset),

        NodeTypes.Array            => ReadArrayNode(reader, type, offset, tables),
        NodeTypes.Dictionary       => ReadDictionaryNode(reader, offset, tables),
        NodeTypes.SingleTypedArray => ReadArrayNode(reader, type, offset, tables),

        NodeTypes.Bool => ReadBoolNode(reader, offset),
        NodeTypes.S32  => ReadIntNode(reader, offset),
        NodeTypes.F32  => ReadFloatNode(reader, offset),
        NodeTypes.U32  => ReadUIntNode(reader, offset),
        NodeTypes.S64  => ReadLongNode(reader, offset),
        NodeTypes.U64  => ReadULongNode(reader, offset),
        NodeTypes.F64  => ReadDoubleNode(reader, offset),

        NodeTypes.Null => new NullNode(),
        _ => throw new FileLoadException($"Unknown node type: 0x{type:x2}")
    };

    //parses an array node
    private static ArrayNode ReadArrayNode(FileReader reader, byte type, long offset, Tables tables)
    {
        uint length = reader.ReadUInt32At(offset + 1, 3);
        byte[] types = reader.ReadBytes((int) length);
        reader.Align(4);

        long valueOffset = reader.Position;

        ArrayNode node = new ArrayNode(type);
        for (uint i = 0; i < length; ++i)
        {
            long nodeOffset = valueOffset + i * 4;

            long value = IsContainer(types[i]) ? reader.ReadUInt32At(nodeOffset) : nodeOffset;

            INode childNode = ReadNode(reader, value, types[i], tables);
            node.Add(childNode);
        }

        return node;
    }

    //parses a dictionary node
    private static DictionaryNode ReadDictionaryNode(FileReader reader, long offset, Tables tables)
    {
        uint length = reader.ReadUInt32At(offset + 1, 3);

        DictionaryNode node = new();
        for (uint i = 0; i < length; ++i)
        {
            long nodeOffset = offset + 4 + i * 8;

            string name = tables.Names[reader.ReadInt32At(nodeOffset, 3)];
            byte type = reader.ReadByte();
            long value = IsContainer(type) ? reader.ReadUInt32() : nodeOffset + 4;

            INode childNode = ReadNode(reader, value, type, tables);
            node.Add(name, childNode);
        }

        return node;
    }

    //parses a hash dictionary node
    private static HashDictionaryNode ReadHashDictionaryNode(FileReader reader, long offset, Tables tables)
    {
        uint length = reader.ReadUInt32At(offset + 1, 3);
        byte[] types = reader.ReadBytesAt(offset + 4 + 8 * length, (int) length);

        HashDictionaryNode node = new();
        for (uint i = 0; i < length; ++i)
        {
            long nodeOffset = offset + 4 + i * 8;
            byte nodeType = types[i];

            string name = "0x" + reader.ReadHexStringAt(nodeOffset, 4);
            long value = IsContainer(nodeType) ? reader.ReadUInt32() : nodeOffset + 4;

            INode childNode = ReadNode(reader, value, nodeType, tables);
            node.Add(name, childNode);
        }

        return node;
    }

    //parses a hash value dictionary node
    private static HashValueDictionaryNode ReadHashValueDictionaryNode(FileReader reader, long offset, Tables tables)
    {
        uint length = reader.ReadUInt32At(offset + 1, 3);
        byte[] types = reader.ReadBytesAt(offset + 4 + 12 * length, (int) length);

        HashValueDictionaryNode node = new();
        for (uint i = 0; i < length; ++i)
        {
            long nodeOffset = offset + 4 + i * 8;
            byte nodeType = types[i];

            long value = IsContainer(nodeType) ? reader.ReadUInt32() : nodeOffset;
            string name = "0x" + reader.ReadHexStringAt(nodeOffset + 4, 4);

            INode childNode = ReadNode(reader, value, nodeType, tables);
            node.Add(name, childNode);
        }

        return node;
    }

    //parses a string value node
    private static ValueNode<string> ReadStringNode(FileReader reader, long offset, Tables tables)
    {
        return new ValueNode<string>(NodeTypes.String) {Value = tables.Strings[reader.ReadInt32At(offset)]};
    }

    //parses a path value node
    private static PathNode ReadPathNode(FileReader reader, long offset, Tables tables)
    {
        return tables.Paths![reader.ReadInt32At(offset)];
    }

    //parses a binary value node
    private static BinaryDataNode ReadBinaryNode(FileReader reader, long offset)
    {
        int size = reader.ReadInt32At(reader.ReadInt32At(offset));

        return new BinaryDataNode
        {
            Size = size,
            Data = reader.ReadBytes(size)
        };
    }

    //parses a binary param value node
    private static AlignedBinaryDataNode ReadAlignedBinaryDataNode(FileReader reader, long offset)
    {
        int size = reader.ReadInt32At(reader.ReadInt32At(offset));

        return new AlignedBinaryDataNode
        {
            Size = size,
            Alignment = reader.ReadInt32(),
            Data = reader.ReadBytes(size)
        };
    }

    //parses a bool value node
    private static ValueNode<bool> ReadBoolNode(FileReader reader, long offset)
    {
        return new ValueNode<bool>(NodeTypes.Bool) {Value = reader.ReadUInt32At(offset) == 1};
    }

    //parses an int value node
    private static ValueNode<int> ReadIntNode(FileReader reader, long offset)
    {
        return new ValueNode<int>(NodeTypes.S32) {Value = reader.ReadInt32At(offset)};
    }

    //parses a float value node
    private static ValueNode<float> ReadFloatNode(FileReader reader, long offset)
    {
        return new ValueNode<float>(NodeTypes.F32) {Value = reader.ReadSingleAt(offset)};
    }

    //parses an uint value node
    private static ValueNode<uint> ReadUIntNode(FileReader reader, long offset)
    {
        return new ValueNode<uint>(NodeTypes.U32) {Value = reader.ReadUInt32At(offset)};
    }

    //parses an long value node
    private static ValueNode<long> ReadLongNode(FileReader reader, long offset)
    {
        return new ValueNode<long>(NodeTypes.S64) {Value = reader.ReadInt64At(reader.ReadUInt32At(offset))};
    }

    //parses an ulong value node
    private static ValueNode<ulong> ReadULongNode(FileReader reader, long offset)
    {
        return new ValueNode<ulong>(NodeTypes.U64) {Value = reader.ReadUInt64At(reader.ReadUInt32At(offset))};
    }

    //parses an double value node
    private static ValueNode<double> ReadDoubleNode(FileReader reader, long offset)
    {
        return new ValueNode<double>(NodeTypes.F64) {Value = reader.ReadDoubleAt(reader.ReadUInt32At(offset))};
    }

    //checks whether a node type is a data structure/container
    private static bool IsContainer(byte type) => type is NodeTypes.HashDictionary or NodeTypes.HashValueDictionary or NodeTypes.Array or NodeTypes.Dictionary or NodeTypes.SingleTypedArray;
    #endregion

    #region helper classes
    private class Tables
    {
        public string[] Names { get; set; } = null!;
        public string[] Strings { get; set; } = null!;
        public PathNode[]? Paths { get; set; }
    }
    #endregion
}
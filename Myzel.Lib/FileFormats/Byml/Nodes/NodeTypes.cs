namespace Myzel.Lib.FileFormats.Byml.Nodes;

/// <summary>
/// Class that holds BYML node types.
/// </summary>
public static class NodeTypes
{
    public const byte HashDictionary       = 0x20;
    public const byte HashValueDictionary  = 0x21;

    public const byte String            = 0xa0;
    public const byte BinaryData        = 0xa1;
    public const byte AlignedBinaryData = 0xa2;

    public const byte Array                = 0xc0;
    public const byte Dictionary           = 0xc1;
    public const byte StringTable          = 0xc2;
    public const byte PathTable            = 0xc3;
    public const byte RelocatedStringTable = 0xc5;
    public const byte SingleTypedArray     = 0xc8;

    public const byte Bool = 0xd0;
    public const byte S32  = 0xd1;
    public const byte F32  = 0xd2;
    public const byte U32  = 0xd3;
    public const byte S64  = 0xd4;
    public const byte U64  = 0xd5;
    public const byte F64  = 0xd6;

    public const byte Null = 0xff;
}
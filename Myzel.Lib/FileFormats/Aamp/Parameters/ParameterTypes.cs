namespace Myzel.Lib.FileFormats.Aamp.Parameters;

/// <summary>
/// Class that holds AAMP parameter types.
/// </summary>
public static class ParameterTypes
{
    /// <summary>
    /// A boolean parameter.
    /// </summary>
    public const byte Bool = 0x00;

    /// <summary>
    /// A floating point parameter (32 bit).
    /// </summary>
    public const byte Float32 = 0x01;

    /// <summary>
    /// A signed integer parameter (32 bit).
    /// </summary>
    public const byte Int32 = 0x02;

    /// <summary>
    /// A 2-dimensional floating point vector parameter (32 bit).
    /// </summary>
    public const byte Vector2 = 0x03;

    /// <summary>
    /// A 3-dimensional floating point vector parameter (32 bit).
    /// </summary>
    public const byte Vector3 = 0x04;

    /// <summary>
    /// A 4-dimensional floating point vector parameter (32 bit).
    /// </summary>
    public const byte Vector4 = 0x05;

    /// <summary>
    /// A color parameter.
    /// </summary>
    public const byte Color = 0x06;

    /// <summary>
    /// A string parameter (max 32 chars long).
    /// </summary>
    public const byte String32 = 0x07;

    /// <summary>
    /// A string parameter (max 64 chars long).
    /// </summary>
    public const byte String64 = 0x08;

    /// <summary>
    /// A 1-dimensional curve parameter (32 bit).
    /// </summary>
    public const byte Curve1 = 0x09;

    /// <summary>
    /// A 2-dimensional curve parameter (32 bit).
    /// </summary>
    public const byte Curve2 = 0x0A;

    /// <summary>
    /// A 3-dimensional curve parameter (32 bit).
    /// </summary>
    public const byte Curve3 = 0x0B;

    /// <summary>
    /// A 4-dimensional curve parameter (32 bit).
    /// </summary>
    public const byte Curve4 = 0x0C;

    /// <summary>
    /// A signed integer buffer parameter (32 bit).
    /// </summary>
    public const byte Int32Buffer = 0x0D;

    /// <summary>
    /// A floating point buffer parameter (32 bit).
    /// </summary>
    public const byte Float32Buffer = 0x0E;

    /// <summary>
    /// A string parameter value (max 256 chars long).
    /// </summary>
    public const byte String256 = 0x0F;

    /// <summary>
    /// A quat parameter (32 bit).
    /// </summary>
    public const byte Quat = 0x10;

    /// <summary>
    /// An unsigned integer parameter (32 bit).
    /// </summary>
    public const byte UInt32 = 0x11;

    /// <summary>
    /// A unsigned integer buffer parameter (32 bit).
    /// </summary>
    public const byte UInt32Buffer = 0x12;

    /// <summary>
    /// A binary buffer parameter.
    /// </summary>
    public const byte BinaryBuffer = 0x13;

    /// <summary>
    /// A hashed string reference parameter (32 bit).
    /// </summary>
    public const byte StringReference = 0x14;

    /// <summary>
    /// An unknown/special parameter.
    /// </summary>
    public const byte None = 0x15;
}
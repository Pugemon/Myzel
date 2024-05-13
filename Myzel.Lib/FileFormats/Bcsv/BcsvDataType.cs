namespace Myzel.Lib.FileFormats.Bcsv;

/// <summary>
/// An <see langword="enum"/> of BCSV data-types.
/// </summary>
public enum BcsvDataType
{
    /// <summary>
    /// Signed 8-bit integer.
    /// </summary>
    SignedInt8,
    /// <inheritdoc cref="SignedInt8"/>
    S8 = SignedInt8,

    /// <summary>
    /// Unsigned 8-bit integer.
    /// </summary>
    UnsignedInt8,
    /// <inheritdoc cref="UnsignedInt8"/>
    U8 = UnsignedInt8,

    /// <summary>
    /// Signed 16-bit integer.
    /// </summary>
    SignedInt16,
    /// <inheritdoc cref="SignedInt16"/>
    S16 = SignedInt16,

    /// <summary>
    /// Unsigned 16-bit integer.
    /// </summary>
    UnsignedInt16,
    /// <inheritdoc cref="UnsignedInt16"/>
    U16 = UnsignedInt16,

    /// <summary>
    /// Signed 32-bit integer.
    /// </summary>
    SignedInt32,
    /// <inheritdoc cref="SignedInt32"/>
    S32 = SignedInt32,

    /// <summary>
    /// Unsigned 32-bit integer.
    /// </summary>
    UnsignedInt32,
    /// <inheritdoc cref="UnsignedInt32"/>
    U32 = UnsignedInt32,

    /// <summary>
    /// Signed 64-bit integer.
    /// </summary>
    SignedInt64,
    /// <inheritdoc cref="SignedInt64"/>
    S64 = SignedInt64,

    /// <summary>
    /// Unsigned 64-bit integer.
    /// </summary>
    UnsignedInt64,
    /// <inheritdoc cref="UnsignedInt64"/>
    U64 = UnsignedInt64,

    /// <summary>
    /// 32-bit floating point number.
    /// </summary>
    Float32,
    /// <inheritdoc cref="Float32"/>
    F32 = Float32,
    /// <inheritdoc cref="Float32"/>
    Single = Float32,

    /// <summary>
    /// 64-bit floating point number.
    /// </summary>
    Float64,
    /// <inheritdoc cref="Float64"/>
    F64 = Float64,
    /// <inheritdoc cref="Float64"/>
    Double = Float64,

    /// <summary>
    /// Hex bytes as string.
    /// </summary>
    Crc32,
    /// <inheritdoc cref="Crc32"/>
    Crc32Hash = Crc32,

    /// <summary>
    /// Murmur hash.
    /// </summary>
    Mmh3,
    /// <inheritdoc cref="Mmh3"/>
    MurmurHash3 = Mmh3,

    /// <summary>
    /// A string.
    /// </summary>
    String,
    /// <inheritdoc cref="String"/>
    Str = String,

    /// <summary>
    /// Derive from class property type.
    /// </summary>
    Default
}

/// <summary>
/// An extension class for <see cref="BcsvDataType"/>.
/// </summary>
public static class BcsvDataTypeExtensions
{
    /// <summary>
    /// Gets the defined system type for a given <see cref="BcsvDataType"/> value.
    /// </summary>
    /// <param name="dataType">The <see cref="BcsvDataType"/> value.</param>
    /// <returns>The system type that corresponds to the <see cref="BcsvDataType"/> value.</returns>
    public static Type GetSystemType(this BcsvDataType dataType) => dataType switch
    {
        BcsvDataType.SignedInt8    => typeof(sbyte),
        BcsvDataType.UnsignedInt8  => typeof(byte),
        BcsvDataType.SignedInt16   => typeof(short),
        BcsvDataType.UnsignedInt16 => typeof(ushort),
        BcsvDataType.SignedInt32   => typeof(int),
        BcsvDataType.UnsignedInt32 => typeof(uint),
        BcsvDataType.Mmh3          => typeof(uint),
        BcsvDataType.SignedInt64   => typeof(long),
        BcsvDataType.UnsignedInt64 => typeof(ulong),
        BcsvDataType.Float32       => typeof(float),
        BcsvDataType.Float64       => typeof(double),
        BcsvDataType.Crc32         => typeof(string),
        BcsvDataType.String        => typeof(string),
        _                          => typeof(object)
    };
}
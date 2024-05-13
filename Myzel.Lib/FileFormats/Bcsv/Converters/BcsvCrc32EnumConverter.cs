using System.Reflection;
using Myzel.Lib.FileFormats.Bcsv.Attributes;
using Myzel.Lib.Hashing;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Lib.FileFormats.Bcsv.Converters;

/// <summary>
/// A converter to convert hex strings to CRC32-hashed <see langword="enum"/> values.
/// Uses the name of the <see langword="enum"/> value or a <see cref="BcsvCrc32EnumNameAttribute"/> value to compute the CRC32 hash.
/// </summary>
public class BcsvCrc32EnumConverter : IBcsvConverter
{
    #region private members
    private Dictionary<string, Enum>? _valueCache;
    private readonly IHashAlgorithm _hashAlgorithm = new Crc32Hash();
    #endregion

    #region IBcsvConverter interface
    /// <inheritdoc/>
    public object? Convert(byte[] data, Type targetType)
    {
        if (!targetType.IsEnum) throw new InvalidCastException("Cannot convert non-enum type to enum.");
        _valueCache ??= BuildCache(targetType);

        Array.Reverse(data);
        string hash = data.ToHexString();
        return _valueCache.GetValueOrDefault(hash);
    }
    #endregion

    #region private methods
    private Dictionary<string, Enum> BuildCache(Type enumType)
    {
        Dictionary<string, Enum> cache = new();

        foreach (Enum value in Enum.GetValues(enumType))
        {
            string name = value.ToString();

            MemberInfo[] info = enumType.GetMember(name);
            object[] attr = info[0].GetCustomAttributes(typeof(BcsvCrc32EnumNameAttribute), true);
            if (attr.Length > 0 && attr[0] is BcsvCrc32EnumNameAttribute crc32Name) name = crc32Name.Name;

            cache.Add(_hashAlgorithm.Compute(name), value);
        }

        return cache;
    }
    #endregion
}
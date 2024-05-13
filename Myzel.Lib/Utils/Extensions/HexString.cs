namespace Myzel.Lib.Utils.Extensions;

/// <summary>
/// A class to convert bytes to hex strings.
/// </summary>
public static class HexStringExtensions
{
    /// <summary>
    /// Converts a <see cref="byte"/> array to a hex <see cref="string"/>.
    /// </summary>
    /// <param name="data">The <see cref="byte"/> array to convert.</param>
    /// <returns>The converted hex <see cref="string"/>.</returns>
    public static string ToHexString(this byte[]? data)
    {
        return data.ToHexString(false);
    }

    /// <summary>
    /// Converts a <see cref="byte"/> array to a hex <see cref="string"/>.
    /// </summary>
    /// <param name="data">The <see cref="byte"/> array to convert.</param>
    /// <param name="prefix">Whether to prepend the "0x" hex prefix to the string.</param>
    /// <returns>The converted hex <see cref="string"/>.</returns>
    public static string ToHexString(this byte[]? data, bool prefix)
    {
        if (data is null) return string.Empty;

        string result = BitConverter.ToString(data).Replace("-", "");
        if (prefix && result.Length > 0) result = $"0x{result}";
        return result;
    }
}
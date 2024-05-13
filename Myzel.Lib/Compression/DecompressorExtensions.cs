using Myzel.Lib.Utils.Extensions;

namespace Myzel.Lib.Compression;

/// <summary>
/// An extension class for <see cref="IDecompressor"/> types.
/// </summary>
public static class DecompressorExtensions
{
    /// <summary>
    /// Validates whether the given data can be decompressed with this decompressor instance.
    /// </summary>
    /// <param name="decompressor">The <see cref="IDecompressor"/> instance to use.</param>
    /// <param name="data">The data to check.</param>
    /// <returns><see langword="true"/> if can be decompressed; otherwise <see langword="false"/>.</returns>
    public static bool CanDecompress(this IDecompressor decompressor, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(decompressor);
        ArgumentNullException.ThrowIfNull(data);

        using MemoryStream stream = new(data, false);
        return decompressor.CanDecompress(stream);
    }

    /// <summary>
    /// Decompresses a byte array.
    /// </summary>
    /// <param name="decompressor">The <see cref="IDecompressor"/> instance to use.</param>
    /// <param name="data">The data to decompress.</param>
    /// <returns>The decompressed data.</returns>
    public static byte[] Decompress(this IDecompressor decompressor, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(decompressor);
        ArgumentNullException.ThrowIfNull(data);

        using MemoryStream stream = new(data, false);
        Stream result = decompressor.Decompress(stream);
        return result.ToArray();
    }
}
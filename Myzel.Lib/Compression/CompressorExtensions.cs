using Myzel.Lib.Utils.Extensions;

namespace Myzel.Lib.Compression;

/// <summary>
/// An extension class for <see cref="ICompressor"/> types.
/// </summary>
public static class CompressorExtensions
{
    /// <summary>
    /// Compresses a byte array.
    /// </summary>
    /// <param name="compressor">The <see cref="ICompressor"/> instance to use.</param>
    /// <param name="data">The data to compress.</param>
    /// <returns>The compressed data.</returns>
    public static byte[] Compress(this ICompressor compressor, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(compressor);
        ArgumentNullException.ThrowIfNull(data);

        using MemoryStream stream = new(data, false);
        Stream result = compressor.Compress(stream);
        return result.ToArray();
    }
}
namespace Myzel.Lib.Compression;

/// <summary>
/// The interface for decompressor types.
/// </summary>
public interface IDecompressor
{
    /// <summary>
    /// Validates whether the given stream can be decompressed with this decompressor instance.
    /// </summary>
    /// <param name="fileStream">The stream to check.</param>
    /// <returns><see langword="true"/> if can be decompressed; otherwise <see langword="false"/>.</returns>
    public bool CanDecompress(Stream fileStream);

    /// <summary>
    /// Decompresses a stream.
    /// </summary>
    /// <param name="fileStream">The stream to decompress.</param>
    /// <returns>A decompressed stream.</returns>
    public Stream Decompress(Stream fileStream);
}
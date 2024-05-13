namespace Myzel.Lib.Compression;

/// <summary>
/// The interface for decompressor types.
/// </summary>
public interface ICompressor
{
    /// <summary>
    /// Validates whether the given stream can be decompressed with this decompressor instance.
    /// </summary>
    /// <param name="fileStream">The stream to check.</param>
    /// <returns><see langword="true"/> if can be decompressed; otherwise <see langword="false"/>.</returns>
    public bool CanCompress(Stream fileStream);
    
    /// <summary>
    /// Compresses a stream.
    /// </summary>
    /// <param name="fileStream">The stream to compress.</param>
    /// <returns>A compressed stream.</returns>
    public Stream Compress(Stream fileStream);
}
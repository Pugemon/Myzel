using Myzel.Lib.Utils.Extensions;
using ZstdSharp;

namespace Myzel.Lib.Compression.ZSTD;

/// <summary>
/// A class for Zstandard compression.
/// </summary>
public class ZstdCompressor : ICompressor
{

    #region private members

    private readonly int _compressionLevel;

    #endregion

    #region constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Decompressor"/> class.
    /// </summary>
    public ZstdCompressor() : this(0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Decompressor"/> class.
    /// </summary>
    /// <param name="compressionLevel">The level of data compression</param>
    public ZstdCompressor(int compressionLevel)
    {
        _compressionLevel = compressionLevel;
    }

    #endregion

    #region public properties

    /// <summary>
    /// Gets the minimum supported compression level.
    /// </summary>
    public static int MinCompressionLevel => Compressor.MinCompressionLevel;

    /// <summary>
    /// Gets the maximum supported compression level.
    /// </summary>
    public static int MaxCompressionLevel => Compressor.MaxCompressionLevel;

    #endregion

    #region ICompressor interface

    public bool CanCompress(Stream fileStream) => true;
    /// <inheritdoc/>
    public Stream Compress(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        using Compressor compressor = new(_compressionLevel);

        fileStream.Position = 0;
        byte[] data = fileStream.ToArray();
        Span<byte> compressed = compressor.Wrap(data);

        return new MemoryStream(compressed.ToArray());
    }

    #endregion

}
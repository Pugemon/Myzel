using Myzel.Lib.Utils;

namespace Myzel.Lib.Compression.ZSTD;

/// <summary>
/// A class for Zstandard decompression.
/// </summary>
public class ZstdDecompressor : IDecompressor
{

    #region private members

    private readonly byte[]? _dict;

    #endregion

    #region constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="Decompressor"/> class.
    /// </summary>
    public ZstdDecompressor()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Decompressor"/> class with a given decompression dictionary.
    /// </summary>
    /// <param name="dict">The compression dictionary to use.</param>
    public ZstdDecompressor(byte[] dict)
    {
        _dict = dict;
    }

    #endregion

    #region IDecompressor interface

    /// <inheritdoc/>
    public bool CanDecompress(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        FileReader reader = new(fileStream, true);
        return reader.ReadBytesAt(0, 4) is [0x28, 0xb5, 0x2f, 0xfd];
    }

    /// <inheritdoc/>
    public Stream Decompress(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        if (!CanDecompress(fileStream)) throw new InvalidDataException("Data is not ZSTD compressed.");

        using ZstdSharp.Decompressor decompressor = new();
        if (_dict is not null) decompressor.LoadDictionary(_dict);

        fileStream.Position = 0;
        MemoryStream resultStream = new();
        using ZstdSharp.DecompressionStream decompressorStream = new(fileStream, decompressor);
        decompressorStream.CopyTo(resultStream);

        return resultStream;
    }

    #endregion

}
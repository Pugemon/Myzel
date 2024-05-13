using Myzel.Core.Settings;
using Myzel.Lib.Compression.ZSTD;
using Myzel.Lib.Utils;

namespace Myzel.Core.FileTypes.StorageProviders;

internal class ZstdCompressionWrapper(StorageProvider provider, IZstdSettings settings) : StorageProvider
{
    #region public properties
    public override bool IsVirtual => provider.IsVirtual;

    public override bool IsCompressed => true;
    #endregion

    #region public methods
    public override Func<Stream> GetReadStream => () =>
    {
        var decompressor = settings.ZstdDict is null ? new ZstdDecompressor() : new ZstdDecompressor(settings.ZstdDict);

        using var readStream = provider.GetReadStream();
        return decompressor.Decompress(readStream);
    };

    public override Func<Stream> GetWriteStream => () => new WrappedMemoryStream(dataStream =>
    {
        var compressor = new ZstdCompressor(settings.ZstdCompressionLevel);
        using var stream = compressor.Compress(dataStream);

        using var writeStream = provider.GetWriteStream();
        stream.Position = 0;
        stream.CopyTo(writeStream);
    });
    #endregion
}
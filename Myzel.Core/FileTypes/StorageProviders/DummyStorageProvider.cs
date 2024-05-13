namespace Myzel.Core.FileTypes.StorageProviders;

internal class DummyStorageProvider(bool isVirtual = false, bool isCompressed = false) : StorageProvider
{
    public override bool IsVirtual => isVirtual;

    public override bool IsCompressed => isCompressed;

    public override Func<Stream> GetReadStream => () => new MemoryStream();

    public override Func<Stream> GetWriteStream => () => new MemoryStream();
}
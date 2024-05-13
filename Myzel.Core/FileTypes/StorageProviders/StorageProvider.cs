namespace Myzel.Core.FileTypes.StorageProviders;

public abstract class StorageProvider
{
    public abstract bool IsVirtual { get; }

    public abstract bool IsCompressed { get; }

    public abstract Func<Stream> GetReadStream { get; }

    public abstract Func<Stream> GetWriteStream { get; }
}
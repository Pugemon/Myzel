using Myzel.Lib.Utils;

namespace Myzel.Core.FileTypes.StorageProviders;

internal class VirtualStorageProvider(Func<byte[]> contentGetter, Action<byte[]> contentSetter) : StorageProvider
{
    public override bool IsVirtual => true;

    public override bool IsCompressed => false;

    public override Func<Stream> GetReadStream => () => new MemoryStream(contentGetter(), false);

    public override Func<Stream> GetWriteStream => () => new WrappedMemoryStream(stream => contentSetter(stream.ToArray()));
}
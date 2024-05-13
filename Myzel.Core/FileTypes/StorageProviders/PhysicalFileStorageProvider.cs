using Myzel.Lib.Utils;

namespace Myzel.Core.FileTypes.StorageProviders;

public class PhysicalFileStorageProvider(string filePath) : StorageProvider
{
    public override bool IsVirtual => false;

    public override bool IsCompressed => false;

    public override Func<Stream> GetReadStream => () => File.OpenRead(filePath);

    public override Func<Stream> GetWriteStream => () => new WrappedMemoryStream(stream =>
    {
        if (stream.Length == 0) return; //something went wrong, don't write the file

        using var writeStream = File.Create(filePath);
        stream.Position = 0;
        stream.CopyTo(writeStream);
    });
}
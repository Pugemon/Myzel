using Myzel.Lib.FileFormats;
using Myzel.Lib.Utils;

namespace Myzel.Core.FileTypes.StorageProviders;

internal class DirectAccessStorageProvider<T>(IFileParser<T> parser, Func<T> contentGetter, IFileCompiler<T> compiler, Action<T> contentSetter) : StorageProvider where T : class
{
    public override bool IsVirtual => true;

    public override bool IsCompressed => false;

    public override Func<Stream> GetReadStream => () =>
    {
        var stream = new MemoryStream();
        var model = contentGetter();
        compiler.Compile(model, stream);
        stream.Position = 0;
        return stream;
    };

    public override Func<Stream> GetWriteStream => () => new WrappedMemoryStream(stream => contentSetter(parser.Parse(stream)));
}
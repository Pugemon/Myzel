using Myzel.Core.FileTypes.StorageProviders;
using Myzel.Lib.FileFormats.Msbt;
using Myzel.Lib.FileFormats.Umsbt;

namespace Myzel.Core.FileTypes;

internal class UmsbtFileData(StorageProvider storageProvider, FileDataFactory fileFactory) : FileData(storageProvider)
{
    #region private members
    private static readonly UmsbtFileParser Parser = new();
    private static readonly UmsbtFileCompiler Compiler = new();
    private static readonly MsbtFileParser MsbtParser = new();
    private static readonly MsbtFileCompiler MsbtCompiler = new();
    #endregion

    #region public properties
    public override string Type => "umsbt";

    public override bool IsContainer => true;

    public IList<MsbtFile>? Model { get; private set; }
    #endregion

    #region protected methods
    protected override Task InternalLoad(Stream stream) => Task.Run(() =>
    {
        Model = Parser.Parse(stream);

        var files = new FileData[Model.Count];
        Parallel.For(0, Model.Count, i =>
        {
            var provider = new DirectAccessStorageProvider<MsbtFile>(MsbtParser, () => Model[i], MsbtCompiler, model => Model[i] = model);
            var filePath = FilePath + "/" + i + "/" + Path.GetFileNameWithoutExtension(Name) + ".msbt";
            files[i] = fileFactory.Create(provider, filePath);
        });

        Children = files;
    });

    protected override Task InternalSave(Stream stream) => Task.Run(() =>
    {
        if (Model is null) return;

        Compiler.Compile(Model, stream);
    });
    #endregion
}
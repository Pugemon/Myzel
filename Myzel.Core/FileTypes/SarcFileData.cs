using Myzel.Core.FileTypes.StorageProviders;
using Myzel.Core.Utils;
using Myzel.Lib.FileFormats.Sarc;

namespace Myzel.Core.FileTypes;

internal class SarcFileData(StorageProvider storageProvider, FileDataFactory fileFactory) : FileData(storageProvider)
{
    #region private members
    private static readonly SarcFileParser Parser = new();
    private static readonly SarcFileCompiler Compiler = new();
    #endregion

    #region public properties
    public override string Type => "sarc";

    public override bool IsContainer => true;

    public SarcFile? Model { get; private set; }
    #endregion

    #region protected methods
    protected override Task InternalLoad(Stream stream) => Task.Run(() =>
    {
        Model = Parser.Parse(stream);

        var files = new FileData[Model.Files.Count];
        Parallel.For((long)0, files.Length, i =>
        {
            var content = Model.Files[(int)i];
            var provider = new VirtualStorageProvider(() => content.Data, data => content.Data = data);
            files[i] = fileFactory.Create(provider, FilePath + "/" + content.Name);
        });

        Children = FolderBuilder.WrapVirtual(files, FilePath);
    });

    protected override Task InternalSave(Stream stream) => Task.Run(() =>
    {
        if (Model is null) return;

        Compiler.Compile(Model, stream);
    });
    #endregion
}
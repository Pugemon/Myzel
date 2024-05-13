using Myzel.Core.FileTypes.StorageProviders;
using Myzel.Core.Utils;

namespace Myzel.Core.FileTypes;

internal class FolderFileData : FileData
{
    #region private members
    private static readonly StorageProvider DummyProvider = new DummyStorageProvider();
    private readonly FileDataFactory? _fileFactory;
    #endregion

    #region constructors
    public FolderFileData(FileDataFactory fileFactory) : base(DummyProvider)
    {
        _fileFactory = fileFactory;
    }

    public FolderFileData(ICollection<FileData> children) : base(DummyProvider)
    {
        Children = children;
        IsInitialized = true;
    }
    #endregion

    #region public properties
    public override string Type => "folder";

    public override bool IsContainer => true;
    #endregion

    #region protected properties
    protected override bool AutoResetModifiedState => true;
    #endregion

    #region protected methods
    protected override Task InternalLoad(Stream stream) => Task.Run(() =>
    {
        if (_fileFactory is null) return;

        var filePaths = Directory.GetFiles(FilePath, "*", SearchOption.AllDirectories);
        var files = new FileData[filePaths.Length];
        Parallel.For(0, files.Length, i =>
        {
            var fixedFilePath = filePaths[i].Replace('\\', '/');
            var provider = new PhysicalFileStorageProvider(fixedFilePath);
            files[i] = _fileFactory.Create(provider, fixedFilePath);
        });

        Children = FolderBuilder.WrapPhysical(files, FilePath);
    });

    protected override Task InternalSave(Stream stream) => Task.CompletedTask;
    #endregion
}
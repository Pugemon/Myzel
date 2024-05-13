using Myzel.Core.FileTypes.StorageProviders;

namespace Myzel.Core.FileTypes;

internal class VirtualFolderFileData : FileData
{
    #region private members
    private static readonly StorageProvider DummyProvider = new DummyStorageProvider(true);
    #endregion

    #region constructor
    public VirtualFolderFileData(ICollection<FileData> files) : base(DummyProvider)
    {
        Children = files;
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
    protected override Task InternalLoad(Stream stream) => Task.CompletedTask;

    protected override Task InternalSave(Stream stream) => Task.CompletedTask;
    #endregion
}
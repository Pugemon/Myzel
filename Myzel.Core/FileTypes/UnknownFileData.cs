using Myzel.Core.FileTypes.StorageProviders;

namespace Myzel.Core.FileTypes;

internal class UnknownFileData : FileData
{
    #region constructor
    public UnknownFileData(StorageProvider storageProvider) : base(storageProvider) => IsInitialized = true;
    #endregion

    #region public properties
    public override string Type => "unknown";
    #endregion

    #region protected methods
    protected override Task InternalLoad(Stream stream) => Task.CompletedTask;

    protected override Task InternalSave(Stream stream) => Task.CompletedTask;
    #endregion
}
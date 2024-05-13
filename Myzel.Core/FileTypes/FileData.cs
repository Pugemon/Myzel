using Myzel.Core.FileTypes.StorageProviders;

namespace Myzel.Core.FileTypes;

public abstract class FileData(StorageProvider storageProvider)
{
    #region private members
    private bool _isModified;
    #endregion

    #region public properties
    public virtual string Name => Path.GetFileName(FilePath);

    public required string FilePath { get; set; }

    public abstract string Type { get; }

    public bool IsInitialized { get; protected set; }

    public bool IsVirtual => storageProvider.IsVirtual;

    public bool IsCompressed => storageProvider.IsCompressed;

    public virtual bool IsContainer => false;

    public bool IsModified
    {
        get => _isModified;
        set
        {
            if (value == _isModified) return;
            _isModified = value;
            Modified?.Invoke(this, EventArgs.Empty);

            if (Parent is null) return;
            if (value) Parent.IsModified = true;
            else if (Parent.AutoResetModifiedState)
            {
                bool hasModified = Parent.Children.Any(file => file.IsModified);
                if (!hasModified) Parent.IsModified = false;
            }
        }
    }

    public FileData? Parent { get; set; }

    public ICollection<FileData> Children { get; protected set; } = Array.Empty<FileData>();
    #endregion

    #region public events
    public event EventHandler? Modified;
    #endregion

    #region public methods
    public async Task Load(bool recursive = false)
    {
        await using Stream stream = storageProvider.GetReadStream();
        await Load(stream, recursive);
    }

    public async Task Load(Stream stream, bool recursive = false)
    {
        await InternalLoad(stream);
        stream.Close();

        foreach (FileData file in Children)
        {
            file.Parent = this;
            if (recursive) await file.Load(recursive);
        }

        bool needsNotification = !IsInitialized && !IsModified;
        IsInitialized = true;
        IsModified = false;
        if (needsNotification) Modified?.Invoke(this, EventArgs.Empty);
    }

    public async Task Save(bool recursive = false)
    {
        if (!IsInitialized || !IsModified) return;

        await using var stream = storageProvider.GetWriteStream();
        await Save(stream, recursive);
    }

    public async Task Save(Stream stream, bool recursive = false)
    {
        if (!IsInitialized) return;

        if (recursive)
        {
            foreach (FileData file in Children) await file.Save(recursive);
        }

        await InternalSave(stream);
        stream.Close();

        IsModified = false;
    }

    public bool Contains(FileData file) => Find<FileData>(f => f == file) is not null;

    public bool Contains(Func<FileData, bool> predicate) => Find<FileData>(predicate) is not null;

    public bool Contains<T>(Func<T, bool> predicate) where T : FileData => Find(predicate) is not null;

    public FileData? Find(Func<FileData, bool> predicate) => Find<FileData>(predicate);

    public T? Find<T>(Func<T, bool> predicate) where T : FileData
    {
        if (this is T thisFile && predicate(thisFile)) return thisFile;
        foreach (var file in Children)
        {
            if (file is T typedFile && predicate(typedFile)) return typedFile;

            var foundFile = file.Find(predicate);
            if (foundFile is not null) return foundFile;
        }
        return null;
    }

    public IEnumerable<FileData> FindAll(Func<FileData, bool> predicate) => FindAll<FileData>(predicate);

    public IEnumerable<T> FindAll<T>(Func<T, bool> predicate) where T : FileData
    {
        var files = new List<T>();
        FindAllChildren(this);
        return files;

        void FindAllChildren(FileData file)
        {
            if (file is T typedFile && predicate(typedFile)) files.Add(typedFile);
            foreach (var childFile in file.Children)
            {
                FindAllChildren(childFile);
            }
        }
    }
    #endregion

    #region protected properties
    protected virtual bool AutoResetModifiedState { get; set; }
    #endregion

    #region protected methods
    protected abstract Task InternalLoad(Stream stream);

    protected abstract Task InternalSave(Stream stream);
    #endregion
}
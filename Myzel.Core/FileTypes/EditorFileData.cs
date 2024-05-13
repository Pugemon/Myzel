using Myzel.Core.FileTypes.StorageProviders;
using Myzel.Core.Utils;

namespace Myzel.Core.FileTypes;

public abstract class EditorFileData(StorageProvider storageProvider) : FileData(storageProvider)
{
    #region private members
    private IEnumerable<char>? _initialText;
    private bool _isTextInitialized;
    #endregion

    #region public properties
    public abstract string EditorLanguage { get; }

    public IEnumerable<char>? InitialText
    {
        get => _initialText;
        set
        {
            _initialText = value;
            _isTextInitialized = false;
        }
    }
    #endregion

    #region public methods
    public async Task LoadEditorContent(bool reload = false)
    {
        //if (InitialText is null || !reload && _isTextInitialized) return;

        if (!IsInitialized) await Load();

        string content = await InternalLoadEditorContent();
        Console.WriteLine("String byuld");
        InitialText = content.ToCharArray().AsEnumerable();

        _isTextInitialized = true;
    }
    #endregion

    #region protected methods
    protected abstract Task<string> InternalLoadEditorContent();

    protected abstract Task InternalParseEditorContent(string? content);

    protected override async Task InternalSave(Stream stream)
    {
        if (InitialText is null || !_isTextInitialized) return;
        
        await InternalParseEditorContent(InitialText.ToString()!);
    }
    #endregion
}
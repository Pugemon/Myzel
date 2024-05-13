using Myzel.Core.FileTypes.StorageProviders;

namespace Myzel.Core.FileTypes;

public class FunctionMapFileData(StorageProvider storageProvider) : EditorFileData(storageProvider)
{
    #region private members
    private string? _editorContent;
    #endregion

    #region public properties
    public override string Name => Path.GetFileNameWithoutExtension(FilePath);

    public override string Type => "mfm";

    public override string EditorLanguage => "mfm";

    public FunctionMap.FunctionMap? Model { get; private set; }
    #endregion

    #region protected methods
    protected override async Task InternalLoad(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        _editorContent = content;
        Model = FunctionMap.FunctionMap.Parse(content);
    }

    protected override async Task InternalSave(Stream stream)
    {
        if (_editorContent is null) return;

        await base.InternalSave(stream);
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(_editorContent);
    }

    protected override Task<string> InternalLoadEditorContent() => Task.FromResult(_editorContent!);

    protected override Task InternalParseEditorContent(string? content) => Task.Run(() =>
    {
        Model = FunctionMap.FunctionMap.Parse(content);
        _editorContent = content;
    });
    #endregion
}
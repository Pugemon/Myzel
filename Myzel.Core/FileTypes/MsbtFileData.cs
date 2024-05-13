using Myzel.Lib.FileFormats.Msbt;
using Myzel.Core.FileTypes.StorageProviders;
using Myzel.Core.Settings;
using Myzel.Core.Utils;

namespace Myzel.Core.FileTypes;

internal class MsbtFileData(StorageProvider storageProvider, IMsbtSettings settings) : EditorFileData(storageProvider)
{
    #region private members
    private static readonly MsbtFileParser Parser = new();
    private static readonly MsbtFileCompiler Compiler = new();
    #endregion

    #region public properties
    public override string Type => "msbt";

    public override string EditorLanguage => "msbt";

    public MsbtFile? Model { get; private set; }
    #endregion

    #region protected methods
    protected override Task InternalLoad(Stream stream)
    {
        Model = Parser.Parse(stream);
        return Task.CompletedTask;
    }

    protected override Task InternalSave(Stream stream) => Task.Run(async () =>
    {
        if (Model is null) return;

        await base.InternalSave(stream);
        Compiler.Compile(Model, stream);
    });

    protected override Task<string> InternalLoadEditorContent() => Task.Run(() => TextConverter.SerializeMsbt(Model!, settings.FunctionMap));

    protected override Task InternalParseEditorContent(string? content) => Task.Run(() => Model = TextConverter.DeserializeMsbt(content, settings.FunctionMap));
    #endregion
}
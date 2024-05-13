using Myzel.Core.FileTypes.StorageProviders;
using Myzel.Core.Settings;
using Myzel.Core.Utils;
using Myzel.Lib.FileFormats.Bmg;

namespace Myzel.Core.FileTypes;

internal class BmgFileData(StorageProvider storageProvider, IMsbtSettings settings) : EditorFileData(storageProvider)
{
    #region private members
    private static readonly BmgFileParser Parser = new();
    private static readonly BmgFileCompiler Compiler = new();
    #endregion

    #region public properties
    public override string Type => "bmg";

    public override string EditorLanguage => "msbt";

    public BmgFile? Model { get; private set; }
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

    protected override Task<string> InternalLoadEditorContent() => Task.Run(() => TextConverter.SerializeBmg(Model!, settings.FunctionMap));

    protected override Task InternalParseEditorContent(string? content) => Task.Run(() => Model = TextConverter.DeserializeBmg(content, settings.FunctionMap));
    #endregion
}
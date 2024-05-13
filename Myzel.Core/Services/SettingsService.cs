using System.ComponentModel;
using System.Net.Mime;
using Myzel.Core.Settings;

namespace Myzel.Core.Services;

public class SettingsService : IZstdSettings, IMsbtSettings
{
    #region private members
    private static readonly string SettingsPath = Path.Combine(Path.GetDirectoryName(MediaTypeNames.Application.Json)!, "settings.json");

    private string? _zstdDictPath;
    private byte[]? _zstdDict;
    private int _zstdCompressionLevel;

    private FunctionMap.FunctionMap _functionMap = Core.FunctionMap.FunctionMap.Parse(string.Empty);

    private bool _editorWordWrap;
    private bool _editorNewLine;
    private bool _editorWhitespace;
    #endregion

    #region public properties
    #region zszd compression
    public string? ZstdDictPath
    {
        get => _zstdDictPath;
        set
        {
            if (_zstdDictPath == value) return;
            _zstdDictPath = value;
            SettingsChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SettingsService.ZstdDictPath)));
        }
    }

    public byte[]? ZstdDict
    {
        get => _zstdDict;
        set
        {
            if (_zstdDict == value) return;
            _zstdDict = value;
            SettingsChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SettingsService.ZstdDict)));
        }
    }

    public int ZstdCompressionLevel
    {
        get => _zstdCompressionLevel;
        set
        {
            if ( _zstdCompressionLevel == value) return;
            _zstdCompressionLevel = value;
            SettingsChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SettingsService.ZstdCompressionLevel)));
        }
    }
    #endregion

    #region msbt settings
    public FunctionMap.FunctionMap FunctionMap
    {
        get => _functionMap;
        set
        {
            if (_functionMap == value) return;
            _functionMap = value;
            SettingsChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SettingsService.FunctionMap)));
        }
    }
    #endregion

    #region editor settings
    public bool EditorWordWrap
    {
        get => _editorWordWrap;
        set
        {
            if ( _editorWordWrap == value) return;
            _editorWordWrap = value;
            SettingsChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SettingsService.EditorWordWrap)));
        }
    }

    public bool EditorNewLine
    {
        get => _editorNewLine;
        set
        {
            if ( _editorNewLine == value) return;
            _editorNewLine = value;
            SettingsChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SettingsService.EditorNewLine)));
        }
    }

    public bool EditorWhitespace
    {
        get => _editorWhitespace;
        set
        {
            if ( _editorWhitespace == value) return;
            _editorWhitespace = value;
            SettingsChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SettingsService.EditorWhitespace)));
        }
    }
    #endregion
    #endregion

    #region public events
    public PropertyChangedEventHandler? SettingsChanged;
    #endregion

    #region public methods
    public void Load()
    {
        //todo: add load settings from SettingsPath

        ZstdDict = null;
        ZstdCompressionLevel = 19;

        FunctionMap = Core.FunctionMap.FunctionMap.Parse(string.Empty);

        EditorWhitespace = true;
    }
    #endregion
}
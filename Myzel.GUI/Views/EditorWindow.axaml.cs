using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Indentation.CSharp;
using AvaloniaEdit.TextMate;
using Myzel.Core.FileTypes;
using Myzel.Core.FileTypes.StorageProviders;
using Myzel.Core.Services;

namespace Myzel.GUI.Views
{
    public partial class EditorWindow : Window
    {
        private readonly TextEditor? _textEditor;
        private readonly TextMate.Installation _textMateInstallation;
        private TextBlock? _statusTextBlock;
        private EditorFileData _fileData;
        public EditorWindow()
        {
            InitializeComponent();
            _textEditor = this.FindControl<TextEditor>("EditorText");
            if (_textEditor == null) return;

            LoadFile();
            
            
            _textEditor.HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible;
            _textEditor.ShowLineNumbers = true;
            _textEditor.TextArea.IndentationStrategy = new CSharpIndentationStrategy();
            //_textEditor.Document = new TextDocument(_fileData?.InitialText);
            #region ContextMenu

            _textEditor.ContextMenu = new ContextMenu
            {
                ItemsSource = new List<MenuItem>
                {
                    new() { Header = "Copy", InputGesture = new KeyGesture(Key.C, KeyModifiers.Control) },
                    new() { Header = "Paste", InputGesture = new KeyGesture(Key.V, KeyModifiers.Control) },
                    new() { Header = "Cut", InputGesture = new KeyGesture(Key.X, KeyModifiers.Control) }
                }
            };

            #endregion
            
            _textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged!;
            _statusTextBlock = this.Find<TextBlock>("StatusText");
            
        }

        private async void LoadFile()
        {
            var files = new List<FileData>();
            try
            {
                var filePath = $@"C:\Users\islav\Downloads\0002_3456.msbt";
                var fixedFilePath = filePath.Replace('\\', '/');
                var provider = new PhysicalFileStorageProvider(fixedFilePath);
                var settings = new SettingsService();
                settings.Load();
                FileDataFactory _fileDataFactory = new FileDataFactory(settings);
                var file = _fileDataFactory.Create(provider, fixedFilePath);
                await file.Load();
                EditorFileData editorFileData;
                editorFileData = (EditorFileData)file;
                await editorFileData.LoadEditorContent();
                _fileData = editorFileData;
                _textEditor.Document = new TextDocument(_fileData.InitialText);
                Console.WriteLine("Debugim!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            if (_textEditor == null) return;
            if (_statusTextBlock != null)
                _statusTextBlock.Text = $"Line {_textEditor.TextArea.Caret.Line} Column {_textEditor.TextArea.Caret.Column}";
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _textMateInstallation?.Dispose();

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
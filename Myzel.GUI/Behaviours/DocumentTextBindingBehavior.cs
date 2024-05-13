using System;
using Avalonia;
using AvaloniaEdit;
using Avalonia.Xaml.Interactivity;


namespace Myzel.GUI.Behaviours;

public class DocumentTextBindingBehavior : Behavior<TextEditor>
{
    private TextEditor _textEditor = null!;

    private static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<DocumentTextBindingBehavior, string>(nameof(DocumentTextBindingBehavior.Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not { } textEditor) return;
        _textEditor = textEditor;
        _textEditor.TextChanged += TextChanged;
        this.GetObservable(TextProperty).Subscribe(TextPropertyChanged);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        _textEditor.TextChanged -= TextChanged;
    }

    private void TextChanged(object? sender, EventArgs eventArgs)
    {
        if (_textEditor.Document != null)
        {
            Text = _textEditor.Document.Text;
        }
    }

    private void TextPropertyChanged(string? text)
    {
        if (_textEditor.Document == null || text == null) return;
        int caretOffset = _textEditor.CaretOffset;
        _textEditor.Document.Text = text;
        _textEditor.CaretOffset = caretOffset;
    }
}
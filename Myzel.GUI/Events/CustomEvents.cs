using System;

namespace Myzel.GUI.Events;

public class CustomEvents
{
    public class TextEventArgs(string text) : EventArgs
    {
        public string Text { get; } = text;
    }
}
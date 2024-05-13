namespace Myzel.Lib.Utils.YamlTextWriter;

internal class YamlTextWriterException : Exception
{
    public YamlTextWriterException()
    {
    }

    public YamlTextWriterException(string message) : base(message)
    {
    }

    public YamlTextWriterException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
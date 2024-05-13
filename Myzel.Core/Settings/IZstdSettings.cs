namespace Myzel.Core.Settings;

public interface IZstdSettings
{
    byte[]? ZstdDict { get; }

    int ZstdCompressionLevel { get; }

}
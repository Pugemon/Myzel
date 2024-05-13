namespace Myzel.Lib.FileFormats.Bmg.Serializers;

/// <summary>
/// A collection of extension methods for <see cref="IBmgSerializer"/> classes.
/// </summary>
public static class BmgSerializerExtensions
{
    /// <summary>
    /// Serializes a collection of <see cref="BmgFile"/> objects from multiple languages.
    /// </summary>
    /// <param name="serializer">The <see cref="IBmgSerializer"/> instance to use.</param>
    /// <param name="files">The collection of <see cref="BmgFile"/> objects to serialize.</param>
    /// <returns>The serialized string.</returns>
    public static string Serialize(this IBmgSerializer serializer, IEnumerable<BmgFile> files)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        ArgumentNullException.ThrowIfNull(files);

        using StringWriter writer = new();
        serializer.Serialize(writer, files);
        return writer.ToString();
    }
}
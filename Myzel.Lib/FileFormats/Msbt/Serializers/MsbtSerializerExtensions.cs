namespace Myzel.Lib.FileFormats.Msbt.Serializers;

/// <summary>
/// A collection of extension methods for <see cref="IMsbtSerializer"/> classes.
/// </summary>
public static class MsbtSerializerExtensions
{
    /// <summary>
    /// Serializes a collection of <see cref="MsbtFile"/> objects from multiple languages.
    /// </summary>
    /// <param name="serializer">The <see cref="IMsbtSerializer"/> instance to use.</param>
    /// <param name="files">The collection of <see cref="MsbtFile"/> objects to serialize.</param>
    /// <returns>The serialized string.</returns>
    public static string Serialize(this IMsbtSerializer serializer, IEnumerable<MsbtFile> files)
    {
        if (serializer is null) throw new ArgumentNullException(nameof(serializer));
        if (files is null) throw new ArgumentNullException(nameof(files));

        using var writer = new StringWriter();
        serializer.Serialize(writer, files);
        return writer.ToString();
    }
}
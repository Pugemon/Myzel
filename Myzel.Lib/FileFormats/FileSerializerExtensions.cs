namespace Myzel.Lib.FileFormats;

/// <summary>
/// A collection of extension methods for <see cref="IFileSerializer{T}"/> classes.
/// </summary>
public static class FileSerializerExtensions
{
    /// <summary>
    /// Serializes a file object.
    /// </summary>
    /// <param name="serializer">The <see cref="IFileSerializer{T}"/> instance to use.</param>
    /// <param name="file">The file to serialize.</param>
    /// <returns>The serialized string.</returns>
    public static string Serialize<T>(this IFileSerializer<T> serializer, T file) where T : class
    {
        ArgumentNullException.ThrowIfNull(serializer);
        ArgumentNullException.ThrowIfNull(file);

        using StringWriter writer = new();
        serializer.Serialize(writer, file);
        return writer.ToString();
    }
}
namespace Myzel.Lib.FileFormats;

/// <summary>
/// An extension class for <see cref="IFileParser"/> types.
/// </summary>
public static class FileParserExtensions
{
    /// <summary>
    /// Validates whether the given file can be parsed with this parser instance.
    /// </summary>
    /// <param name="parser">The <see cref="IFileParser"/> instance to use.</param>
    /// <param name="filePath">The path of the file to check.</param>
    /// <returns><see langword="true"/> if can be parsed; otherwise <see langword="false"/>.</returns>
    public static bool CanParse(this IFileParser parser, string filePath)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(filePath);

        using FileStream stream = File.OpenRead(filePath);
        return parser.CanParse(stream);
    }

    /// <summary>
    /// Validates whether the given data can be parsed with this parser instance.
    /// </summary>
    /// <param name="parser">The <see cref="IFileParser"/> instance to use.</param>
    /// <param name="data">The data to check.</param>
    /// <returns><see langword="true"/> if can be parsed; otherwise <see langword="false"/>.</returns>
    public static bool CanParse(this IFileParser parser, byte[] data)
    {
        if (parser is null) throw new ArgumentNullException(nameof(parser));
        if (data is null) throw new ArgumentNullException(nameof(data));

        using var stream = new MemoryStream(data, false);
        return parser.CanParse(stream);
    }

    /// <summary>
    /// Parses a stream to a file format.
    /// </summary>
    /// <param name="parser">The <see cref="IFileParser"/> instance to use.</param>
    /// <param name="filePath">The path of the file to parse.</param>
    /// <returns>The parsed file format.</returns>
    public static T Parse<T>(this IFileParser<T> parser, string filePath) where T : class
    {
        if (parser is null) throw new ArgumentNullException(nameof(parser));
        if (filePath is null) throw new ArgumentNullException(nameof(filePath));

        using var stream = File.OpenRead(filePath);
        return parser.Parse(stream);
    }

    /// <summary>
    /// Parses a stream to a file format.
    /// </summary>
    /// <param name="parser">The <see cref="IFileParser"/> instance to use.</param>
    /// <param name="data">The data to parse.</param>
    /// <returns>The parsed file format.</returns>
    public static T Parse<T>(this IFileParser<T> parser, byte[] data) where T : class
    {
        if (parser is null) throw new ArgumentNullException(nameof(parser));
        if (data is null) throw new ArgumentNullException(nameof(data));

        using var stream = new MemoryStream(data, false);
        return parser.Parse(stream);
    }
}
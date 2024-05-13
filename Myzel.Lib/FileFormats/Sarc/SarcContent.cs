namespace Myzel.Lib.FileFormats.Sarc;

/// <summary>
/// A class holding information about a file inside a SARC file.
/// </summary>
public class SarcContent
{
    /// <summary>
    /// The full name and path of the file.
    /// Defaults to the file name hash if the SARC file doesn't contain a SFNT entry.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The content of the file as <see cref="byte"/> array.
    /// </summary>
    public byte[] Data { get; set; } = null!;
}
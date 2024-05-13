namespace Myzel.Lib.FileFormats.Sarc;

/// <summary>
/// A class holding information about a SARC archive file.
/// </summary>
public class SarcFile
{
    /// <summary>
    /// Whether the file is encoded in big endian.
    /// </summary>
    public bool BigEndian { get; init; }

    /// <summary>
    /// Gets the version of the SARC file.
    /// </summary>
    public int Version { get; init; }

    /// <summary>
    /// Gets the hash key used for hashing file names.
    /// </summary>
    public int HashKey { get; set; }

    /// <summary>
    /// The list of files contained in the SARC file.
    /// </summary>
    public IList<SarcContent> Files { get; set; } = new List<SarcContent>();
}
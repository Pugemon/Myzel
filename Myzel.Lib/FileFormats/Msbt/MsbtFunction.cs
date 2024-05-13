namespace Myzel.Lib.FileFormats.Msbt;

/// <summary>
/// A class holding information about a MSBT control tag/function.
/// </summary>
public class MsbtFunction
{
    /// <summary>
    /// Gets or sets the group of the tag.
    /// </summary>
    public ushort Group { get; set; }

    /// <summary>
    /// Gets or sets the type of the tag.
    /// </summary>
    public ushort Type { get; set; }

    /// <summary>
    /// Gets or sets the arguments of the tag.
    /// </summary>
    public byte[] Args { get; set; } = [];
}
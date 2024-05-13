using System.Text;
using Myzel.Lib.FileFormats.Msbt;
using Myzel.Lib.FileFormats.Msbt.FormatProvider;
using Myzel.Lib.FileFormats.Msbt.FunctionTable;

namespace Myzel.Lib.FileFormats.Bmg;

/// <summary>
/// A class holding information about a BMG file.
/// </summary>
public class BmgFile
{
    /// <summary>
    /// Whether the file is encoded in big endian.
    /// </summary>
    public bool BigEndian { get; set; }

    /// <summary>
    /// The encoding used for the BMG file.
    /// </summary>
    public Encoding Encoding { get; set; } = null!;

    /// <summary>
    /// The ID of the BMG file. Usually unused.
    /// </summary>
    public int FileId { get; set; }

    /// <summary>
    /// The ID of the default text color.
    /// </summary>
    public int DefaultColor { get; set; }

    /// <summary>
    /// Whether the file contains a MID1 section.
    /// </summary>
    public bool HasMid1 { get; set; }

    /// <summary>
    /// Special MID1 format data.
    /// </summary>
    public byte[] Mid1Format { get; set; } = [];

    /// <summary>
    /// The language of the BMG file.
    /// This property is not set during parsing and must be set manually.
    /// If set, <see cref="IMsbtFunctionTable"/> and <see cref="IMsbtFormatProvider"/> implementation can use it to further improve parsing.
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// The list of messages in the BMG file.
    /// </summary>
    public List<MsbtMessage> Messages { get; set; } = [];
}
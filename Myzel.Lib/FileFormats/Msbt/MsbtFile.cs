using System.Text;
using Myzel.Lib.FileFormats.Msbt.FormatProvider;
using Myzel.Lib.FileFormats.Msbt.FunctionTable;

namespace Myzel.Lib.FileFormats.Msbt;

/// <summary>
/// A class holding information about a MSBT file.
/// </summary>
public class MsbtFile
{
    /// <summary>
    /// Whether the file is encoded in big endian.
    /// </summary>
    public bool BigEndian { get; set; }

    /// <summary>
    /// Gets the version of the MSBT file.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// The encoding used for the MSBT file.
    /// </summary>
    public required Encoding Encoding { get; set; }

    /// <summary>
    /// Whether the file contains a NLI1 section.
    /// </summary>
    public bool HasNli1 { get; set; }

    /// <summary>
    /// Whether the file contains a LBL1 section.
    /// </summary>
    public bool HasLbl1 { get; set; }

    /// <summary>
    /// The number of LBL1 groups used in the hash table.
    /// </summary>
    public int LabelGroups { get; set; }

    /// <summary>
    /// Whether the file contains an ATR1 section.
    /// </summary>
    public bool HasAtr1 { get; set; }

    /// <summary>
    /// Whether the file contains attribute data as text.
    /// </summary>
    public bool HasAttributeText { get; set; }

    /// <summary>
    /// Additional data in the ATR1 section that was not identified.
    /// </summary>
    public byte[] AdditionalAttributeData { get; set; } = [];

    /// <summary>
    /// Whether the file contains an ATO1 section.
    /// </summary>
    public bool HasAto1 { get; set; }

    /// <summary>
    /// The data from the ATO1 section.
    /// Only present if the file has an ATO1 section.
    /// </summary>
    public byte[] Ato1Data { get; set; } = [];

    /// <summary>
    /// Whether the file contains a TSY1 section.
    /// </summary>
    public bool HasTsy1 { get; set; }

    /// <summary>
    /// The language of the MSBT file.
    /// This property is not set during parsing and must be set manually.
    /// If set, <see cref="IMsbtFunctionTable"/> and <see cref="IMsbtFormatProvider"/> implementation can use it to further improve parsing.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// The list of messages in the MSBT file.
    /// </summary>
    public List<MsbtMessage> Messages { get; set; } = [];
}

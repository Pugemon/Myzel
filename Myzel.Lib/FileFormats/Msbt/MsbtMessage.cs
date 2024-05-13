using System.Text;
using System.Text.RegularExpressions;
using Myzel.Lib.FileFormats.Msbt.FormatProvider;
using Myzel.Lib.FileFormats.Msbt.FunctionTable;

namespace Myzel.Lib.FileFormats.Msbt;

/// <summary>
/// A class holding information about a MSBT message.
/// </summary>
public class MsbtMessage
{
    #region private members
    private static readonly IMsbtFunctionTable DefaultTable = new MsbtDefaultFunctionTable();
    private static readonly IMsbtFormatProvider DefaultFormatProvider = new MsbtDefaultFormatProvider();
    #endregion

    #region public properties
    /// <summary>
    /// Additional message index.
    /// Only present if the file has a NLI1 section.
    /// </summary>
    public int? Index { get; set; }

    /// <summary>
    /// The label of the message.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Additional attribute data of the message.
    /// Only present if the file has an ATR1 section.
    /// </summary>
    public byte[]? Attribute { get; set; }

    /// <summary>
    /// Additional attribute data of the message as text.
    /// Only present if the encoded ATR1 section had a string table.
    /// </summary>
    public string? AttributeText { get; set; }

    /// <summary>
    /// Additional text style index into a MSBP file.
    /// Only present if the file has a TSY1 section.
    /// </summary>
    public int? StyleIndex { get; set; }

    /// <summary>
    /// The message text.
    /// </summary>
    public string Text { get; set; } = null!;

    /// <summary>
    /// A list of <see cref="MsbtFunction"/> instances found in the text of the message.
    /// </summary>
    public IList<MsbtFunction> Functions { get; set; } = [];
    #endregion

    #region public methods
    /// <inheritdoc/>
    public override string ToString() => Text;

    /// <summary>
    /// Converts the message text to a clean string. All function call templates are removed.
    /// </summary>
    /// <returns>A cleaned string.</returns>
    public string ToCleanString() => Regex.Replace(Text, @"{{\d+}}", string.Empty, RegexOptions.Compiled);

    /// <summary>
    /// Converts the message text, adding function call declarations and values.
    /// Uses instances of the <see cref="MsbtDefaultFunctionTable"/> and <see cref="MsbtDefaultFormatProvider"/> classes to convert and format.
    /// </summary>
    /// <param name="bigEndian">Whether to use big endian for parsing argument values.</param>
    /// <param name="encoding">The encoding to use for string values in function arguments.</param>
    /// <returns>A converted string.</returns>
    public string ToCompiledString(bool bigEndian, Encoding encoding) => ToCompiledString(DefaultTable, DefaultFormatProvider, bigEndian, encoding);

    /// <summary>
    /// Converts the message text, adding function call declarations and values.
    /// </summary>
    /// <param name="table">The function table to use for function lookup.</param>
    /// <param name="formatProvider">The format provider to use for string formatting.</param>
    /// <param name="bigEndian">Whether to use big endian for parsing argument values.</param>
    /// <param name="encoding">The encoding to use for string values in function arguments.</param>
    /// <returns>A converted string.</returns>
    public string ToCompiledString(IMsbtFunctionTable table, IMsbtFormatProvider formatProvider, bool bigEndian, Encoding encoding)
    {
        if (table is null) throw new ArgumentNullException(nameof(table));
        if (formatProvider is null) throw new ArgumentNullException(nameof(formatProvider));

        var result = new StringBuilder(formatProvider.FormatMessage(this, Text));

        for (var i = 0; i < Functions.Count; ++i)
        {
            table.GetFunction(Functions[i], bigEndian, encoding, out var functionName, out var functionArgs);
            result = result.Replace("{{" + i + "}}", formatProvider.FormatFunction(this, functionName, functionArgs));
        }

        return result.ToString();
    }
    #endregion
}

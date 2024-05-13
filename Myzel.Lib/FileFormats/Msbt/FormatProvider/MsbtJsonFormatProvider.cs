using System.Text;

namespace Myzel.Lib.FileFormats.Msbt.FormatProvider;

/// <summary>
/// A JSON implementation of a <see cref="IMsbtFormatProvider"/>.<br/>
/// Message format:<br/>
/// - Encodes non-JSON compliant characters.<br/>
/// Function format:<br/>
/// - Empty function name: <c>string.Empty</c><br/>
/// - Without arguments: &lt;<c>functionName</c>/&gt;<br/>
/// - With arguments: &lt;<c>functionName</c> <c>arg.Name</c>="<c>arg.Value</c>"/&gt;
/// </summary>
public class MsbtJsonFormatProvider : IMsbtFormatProvider
{
    #region IMsbtFormatProvider interface
    /// <inheritdoc />
    public string FormatMessage(MsbtMessage message, string rawText) => rawText ?? throw new ArgumentNullException(nameof(rawText));

    /// <inheritdoc />
    public string FormatFunction(MsbtMessage message, string functionName, IEnumerable<MsbtFunctionArgument> arguments)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (string.IsNullOrEmpty(functionName)) return string.Empty;

        StringBuilder sb = new();
        sb.Append("{{").Append(functionName);

        foreach (MsbtFunctionArgument arg in arguments)
        {
            sb.Append(' ').Append(arg.Name).Append("=\"").Append(arg.Value).Append('\"');
        }

        sb.Append("}}");
        return sb.ToString();
    }
    #endregion
}
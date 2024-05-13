using System.Text;
using Myzel.Lib.FileFormats.Msbt;
using Myzel.Lib.FileFormats.Msbt.FormatProvider;

namespace Myzel.Core.Utils;

internal class MessageFormatProvider : IMsbtFormatProvider
{
    public string FormatMessage(MsbtMessage message, string rawText) => rawText;

    public string FormatFunction(MsbtMessage message, string functionName, IEnumerable<MsbtFunctionArgument> arguments)
    {
        var sb = new StringBuilder();
        sb.Append("{{").Append(functionName);

        foreach (MsbtFunctionArgument arg in arguments)
        {
            sb.Append(' ').Append(arg.Name).Append("=\"");
            if (arg.Value is Array arr)
            {
                sb.Append('[');
                for (int i = 0; i < arr.Length; ++i)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append(arr.GetValue(i));
                }
                sb.Append(']');
            }
            else sb.Append(arg.Value);
            sb.Append('\"');
        }

        sb.Append("}}");
        return sb.ToString();
    }
}
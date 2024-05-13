using System.Text;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Lib.FileFormats.Msbt.FunctionTable;

/// <summary>
/// Default implementation of a <see cref="IMsbtFunctionTable"/>.
/// Returns function name as fun_<c>hash</c> and argument data as single argument converted to hex string.
/// </summary>
public class MsbtDefaultFunctionTable : IMsbtFunctionTable
{
    /// <inheritdoc/>
    public void GetFunction(MsbtFunction function, bool bigEndian, Encoding encoding, out string functionName, out IEnumerable<MsbtFunctionArgument> functionArgs)
    {
        functionName = $"fun_{function.Group:X4}_{function.Group:X4}";
        List<MsbtFunctionArgument> argList = [];
        if (function.Args.Length > 0) argList.Add(new MsbtFunctionArgument("arg", function.Args.ToHexString(true)));
        functionArgs = argList;
    }
}
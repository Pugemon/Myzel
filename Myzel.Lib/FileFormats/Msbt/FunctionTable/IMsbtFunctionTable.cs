using System.Text;

namespace Myzel.Lib.FileFormats.Msbt.FunctionTable;

/// <summary>
/// An interface for the MSBT function lookup during message formatting.
/// </summary>
public interface IMsbtFunctionTable
{
    /// <summary>
    /// Gets the name and argument list of a MSBT function.
    /// </summary>
    /// <param name="function">The MSBT function.</param>
    /// <param name="bigEndian"></param>
    /// <param name="encoding">The encoding to use for string values.</param>
    /// <param name="functionName">The name of the function.</param>
    /// <param name="functionArgs">A list of function arguments.</param>
    public void GetFunction(MsbtFunction function, bool bigEndian, Encoding encoding, out string functionName, out IEnumerable<MsbtFunctionArgument> functionArgs);
}
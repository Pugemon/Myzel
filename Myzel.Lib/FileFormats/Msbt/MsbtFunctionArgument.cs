namespace Myzel.Lib.FileFormats.Msbt;

/// <summary>
/// A class holding information about a MSBT function argument.
/// </summary>
public class MsbtFunctionArgument
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsbtFunctionArgument"/> class.
    /// </summary>
    /// <param name="name">The name of the function argument.</param>
    /// <param name="value">The value of the function argument.</param>
    public MsbtFunctionArgument(string name, object? value = null)
    {
        Name = name;
        Value = value;
    }

    /// <summary>
    /// Gets or sets the name of the function argument.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value of the function argument.
    /// </summary>
    public object? Value { get; set; }
}
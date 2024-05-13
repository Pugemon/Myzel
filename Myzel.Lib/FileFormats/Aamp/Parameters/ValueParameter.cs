namespace Myzel.Lib.FileFormats.Aamp.Parameters;

/// <summary>
/// A class representing a value parameter in an AAMP file.
/// </summary>
public class ValueParameter : Parameter
{
    /// <summary>
    /// Gets or sets the value of the parameter.
    /// </summary>
    public object Value { get; init; } = default(object)!;
}
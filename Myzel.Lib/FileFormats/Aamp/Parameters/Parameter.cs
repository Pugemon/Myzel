namespace Myzel.Lib.FileFormats.Aamp.Parameters;

/// <summary>
/// A class representing a parameter in an AAMP file.
/// </summary>
public abstract class Parameter
{
    /// <summary>
    /// Gets or sets the name of the parameter.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the type of the parameter.
    /// </summary>
    public virtual byte Type { get; set; } = ParameterTypes.None;
}
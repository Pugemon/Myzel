namespace Myzel.Lib.FileFormats.Aamp.Parameters;

/// <summary>
/// A class representing a curve parameter in an AAMP file.
/// </summary>
public class CurveParameter : Parameter
{
    /// <summary>
    /// Gets or sets a list of <see cref="CurveValue"/> items for this curve parameter.
    /// </summary>
    public IList<CurveValue> Curves { get; init; } = new List<CurveValue>();
}
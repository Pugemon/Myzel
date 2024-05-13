namespace Myzel.Lib.FileFormats.Aamp.Parameters;

/// <summary>
/// A class representing a curve parameter value in an AAMP file.
/// </summary>
public class CurveValue
{
    //???
    public uint[] IntValues { get; init; } = null!;

    //???
    public float[] FloatValues { get; init; } = null!;
}
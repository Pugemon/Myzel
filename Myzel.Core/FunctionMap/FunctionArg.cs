namespace Myzel.Core.FunctionMap;

public class FunctionArg
{
    public required string Name { get; init; }

    public string Description { get; init; } = string.Empty;

    public required DataType DataType { get; set; }

    public bool IsPadding { get; init; }

    public bool IsDiscard { get; init; }

    public int ArrayLength { get; init; }

    public FunctionValue[]? ValueMap { get; set; }
}
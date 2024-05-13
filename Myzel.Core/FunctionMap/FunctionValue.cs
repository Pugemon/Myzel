namespace Myzel.Core.FunctionMap;

public class FunctionValue
{
    public required string Value { get; init; }

    public required string Name { get; init; }

    public string Description { get; init; } = string.Empty;
}
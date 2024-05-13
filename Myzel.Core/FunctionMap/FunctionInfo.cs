namespace Myzel.Core.FunctionMap;

public class FunctionInfo
{
    public required ushort Group { get; init; }

    public required ushort Type { get; init; }

    public ushort[] TypeList { get; init; } = [];

    public bool IsDiscardType { get; init; }

    public FunctionValue[]? TypeMap { get; set; }

    public required string Name { get; init; }

    public string Description { get; init; } = string.Empty;

    public FunctionArg[] Args { get; set; } = [];
}
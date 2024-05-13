namespace Myzel.Lib.FileFormats.Byml.Nodes;

/// <summary>
/// A class for a null-type node.
/// </summary>
public class NullNode : INode
{
    /// <inheritdoc/>
    public byte Type => NodeTypes.Null;
}
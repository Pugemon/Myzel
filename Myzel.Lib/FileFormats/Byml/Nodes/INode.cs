namespace Myzel.Lib.FileFormats.Byml.Nodes;

/// <summary>
/// The base interface for all nodes.
/// </summary>
public interface INode
{
    /// <summary>
    /// Gets the type of the node.
    /// </summary>
    public byte Type { get; }
}
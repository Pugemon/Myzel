namespace Myzel.Lib.FileFormats.Byml.Nodes;

/// <summary>
/// A class for a node containing binary data.
/// </summary>
public class BinaryDataNode : INode
{
    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryDataNode"/> class.
    /// </summary>
    public BinaryDataNode(byte type = NodeTypes.BinaryData) => Type = type;
    #endregion

    #region public properties
    /// <inheritdoc/>
    public byte Type { get; }

    /// <summary>
    /// Gets or sets the size of the binary data.
    /// </summary>
    public int Size { get; init; }

    /// <summary>
    /// Gets or sets the binary data of the node.
    /// </summary>
    public byte[] Data { get; init; } = null!;
    #endregion
}
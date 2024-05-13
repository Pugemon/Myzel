namespace Myzel.Lib.FileFormats.Byml.Nodes;

/// <summary>
/// A class for a node containing aligned binary data.
/// </summary>
public class AlignedBinaryDataNode : BinaryDataNode
{
    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="AlignedBinaryDataNode"/> class.
    /// </summary>
    public AlignedBinaryDataNode() : base(NodeTypes.AlignedBinaryData)
    { }
    #endregion

    #region public properties
    /// <summary>
    /// Gets or sets the alignment value of the node.
    /// </summary>
    public int Alignment { get; init; }
    #endregion
}
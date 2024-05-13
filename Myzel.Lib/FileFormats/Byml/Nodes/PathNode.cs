namespace Myzel.Lib.FileFormats.Byml.Nodes;

/// <summary>
/// A class for a node containing binary data serialized as path data.
/// </summary>
public class PathNode : INode
{
    /// <inheritdoc/>
    public byte Type => NodeTypes.BinaryData;

    /// <summary>
    /// Gets or sets the X coordinate of the position.
    /// </summary>
    public float PositionX { get; init; }

    /// <summary>
    /// Gets or sets the Y coordinate of the position.
    /// </summary>
    public float PositionY { get; init; }

    /// <summary>
    /// Gets or sets the Z coordinate of the position.
    /// </summary>
    public float PositionZ { get; init; }

    /// <summary>
    /// Gets or sets the X coordinate of the normal.
    /// </summary>
    public float NormalX { get; init; }

    /// <summary>
    /// Gets or sets the Y coordinate of the normal.
    /// </summary>
    public float NormalY { get; init; }

    /// <summary>
    /// Gets or sets the Z coordinate of the normal.
    /// </summary>
    public float NormalZ { get; init; }
}
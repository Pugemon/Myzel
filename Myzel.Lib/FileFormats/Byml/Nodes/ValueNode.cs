namespace Myzel.Lib.FileFormats.Byml.Nodes;

/// <summary>
/// The base interface for a value-type node.
/// </summary>
public interface IValueNode : INode
{
    /// <summary>
    /// Gets the type of the value.
    /// </summary>
    public Type ValueType { get; }

    /// <summary>
    /// Gets the value of the node.
    /// </summary>
    public object? GetValue();
}

/// <summary>
/// A class for a value-type node.
/// </summary>
public class ValueNode<T> : IValueNode
{
    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueNode{T}"/> class.
    /// </summary>
    public ValueNode(byte type) => Type = type;
    #endregion

    #region public properties
    /// <inheritdoc/>
    public byte Type { get; }

    /// <inheritdoc/>
    public Type ValueType => typeof(T);

    /// <summary>
    /// Gets or sets the value of the node.
    /// </summary>
    public T? Value { get; init; }
    #endregion

    #region public methods
    /// <inheritdoc/>
    public object? GetValue() => Value;
    #endregion
}
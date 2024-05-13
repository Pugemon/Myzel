using System.Collections;

namespace Myzel.Lib.FileFormats.Byml.Nodes;

/// <summary>
/// A class for an array-type node.
/// </summary>
public class ArrayNode : INode, IEnumerable<INode>
{
    #region private members
    private readonly List<INode> _nodes;
    #endregion

    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayNode"/> class.
    /// </summary>
    public ArrayNode(byte type = NodeTypes.Array)
    {
        _nodes = new List<INode>();
        Type = type;
    }

    #endregion

    #region public properties
    /// <inheritdoc/>
    public byte Type { get; }

    /// <summary>
    /// Gets the number of <see cref="INode"/> objects in this array.
    /// </summary>
    public int Count => _nodes.Count;

    /// <summary>
    /// Retrieves a <see cref="INode"/> object with a given index from the array.
    /// </summary>
    /// <param name="index">The zero-based index of the <see cref="INode"/> object to retrieve.</param>
    /// <returns>The <see cref="INode"/> object with the given index.</returns>
    public INode this[int index] => _nodes[index];
    #endregion

    #region public methods
    /// <summary>
    /// Adds a new <see cref="INode"/> object to the end of the array.
    /// </summary>
    /// <param name="node">The <see cref="INode"/> object to add.</param>
    public void Add(INode node) => _nodes.Add(node);

    /// <summary>
    /// Removes the <see cref="INode"/> object at the given index from the array.
    /// </summary>
    /// <param name="index">The index of the <see cref="INode"/> object to remove.</param>
    public void RemoveAt(int index) => _nodes.RemoveAt(index);

    /// <summary>
    /// Inserts the <see cref="INode"/> object at the given index in the array.
    /// </summary>
    /// <param name="node">The <see cref="INode"/> object to insert.</param>
    /// <param name="index">The index at which the <see cref="INode"/> object should be inserted.</param>
    public void InsertAt(INode node, int index) => _nodes.Insert(index, node);

    /// <summary>
    /// Removes all <see cref="INode"/> objects from array.
    /// </summary>
    public void Clear() => _nodes.Clear();
    #endregion

    #region IEnumerable interface
    /// <inheritdoc/>
    public IEnumerator<INode> GetEnumerator() => _nodes.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}
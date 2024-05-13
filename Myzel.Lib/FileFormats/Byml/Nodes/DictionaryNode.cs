using System.Collections;

namespace Myzel.Lib.FileFormats.Byml.Nodes;

/// <summary>
/// A class for an dictionary-type node.
/// </summary>
public class DictionaryNode : INode, IEnumerable<KeyValuePair<string, INode>>
{
    #region private members
    private readonly Dictionary<string, INode> _nodes = new();
    #endregion

    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryNode"/> class.
    /// </summary>
    public DictionaryNode(byte type = NodeTypes.Dictionary) => Type = type;
    #endregion

    #region public properties
    /// <inheritdoc/>
    public byte Type { get; }

    /// <summary>
    /// Gets the number of <see cref="INode"/> objects in this dictionary.
    /// </summary>
    public int Count => _nodes.Count;

    /// <summary>
    /// Retrieves a <see cref="INode"/> object with a given name/key from the dictionary.
    /// </summary>
    /// <param name="key">The name/key of the <see cref="INode"/> object to retrieve.</param>
    /// <returns>The <see cref="INode"/> object with the given index.</returns>
    public INode this[string key] => _nodes[key];
    #endregion

    #region public methods
    /// <summary>
    /// Determines whether a <see cref="INode"/> object with a given name/key exists in the dictionary.
    /// </summary>
    /// <param name="key">The name/key of the <see cref="INode"/> object to find.</param>
    /// <returns>A value indicating whether the <see cref="INode"/> object was found.</returns>
    public bool Contains(string key) => _nodes.ContainsKey(key);

    /// <summary>
    /// Adds a new <see cref="INode"/> object to the dictionary.
    /// </summary>
    /// <param name="key">The name/key of the <see cref="INode"/> object to add.</param>
    /// <param name="node">The <see cref="INode"/> object to add.</param>
    public void Add(string key, INode node) => _nodes.Add(key, node);

    /// <summary>
    /// Removes a <see cref="INode"/> object from the dictionary.
    /// </summary>
    /// <param name="key">The name/key of the <see cref="INode"/> object to remove.</param>
    public void Remove(string key) => _nodes.Remove(key);

    /// <summary>
    /// Removes all <see cref="INode"/> objects from dictionary.
    /// </summary>
    public void Clear() => _nodes.Clear();

    /// <summary>
    /// Changes the name/key of a <see cref="INode"/> object in the dictionary.
    /// </summary>
    /// <param name="oldKey">The old name/key of the <see cref="INode"/> object.</param>
    /// <param name="newKey">The new name/key of the <see cref="INode"/> object.</param>
    public void Rename(string oldKey, string newKey)
    {
        var node = _nodes[oldKey];
        Remove(oldKey);
        Add(newKey, node);
    }
    #endregion

    #region IEnumerable interface
    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, INode>> GetEnumerator() => _nodes.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}
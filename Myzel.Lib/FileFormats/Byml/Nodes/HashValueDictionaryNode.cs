namespace Myzel.Lib.FileFormats.Byml.Nodes;

/// <summary>
/// A class for an dictionary-type node with hash keys.
/// </summary>
public class HashValueDictionaryNode : DictionaryNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HashDictionaryNode"/> class.
    /// </summary>
    public HashValueDictionaryNode() : base(NodeTypes.HashValueDictionary)
    { }
}
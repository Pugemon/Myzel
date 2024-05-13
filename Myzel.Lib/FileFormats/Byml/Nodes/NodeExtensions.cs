namespace Myzel.Lib.FileFormats.Byml.Nodes;

/// <summary>
/// An extension class for <see cref="IFileParser"/> types.
/// </summary>
public static class NodeExtensions
{
    /// <summary>
    /// Finds a child <see cref="INode"/> element from a given path.
    /// Path elements have to be separated by a '/'.
    /// </summary>
    /// <param name="node">The <see cref="INode"/> object to browse.</param>
    /// <param name="path">The path to browse.</param>
    /// <returns>The <see cref="INode"/> object from the given path; returns <see langword="null"/> if no node was found.</returns>
    public static INode? Find(this INode node, string path)
    {
        string[] parts = path.Split('/');
        INode? browseNode = node;

        foreach (string element in parts)
        {
            if (browseNode is null) break;

            switch (browseNode)
            {
                case DictionaryNode dict:
                    browseNode = dict.Contains(element) ? dict[element] : null;
                    break;
                case ArrayNode array:
                    int index = int.Parse(element);
                    browseNode = array.Count > index ? array[index] : null;
                    break;
                default:
                    browseNode = null;
                    break;
            }
        }

        return browseNode;
    }

    /// <summary>
    /// Finds a child <see cref="INode"/> element from a given path.
    /// Path elements have to be separated by a '/'.
    /// </summary>
    /// <param name="node">The <see cref="INode"/> object to browse.</param>
    /// <param name="path">The path to browse.</param>
    /// <returns>The <see cref="INode"/> object from the given path; returns <see langword="null"/> if no node was found.</returns>
    public static T? Find<T>(this INode node, string path) where T : class, INode => node.Find(path) as T;
}

using Myzel.Lib.FileFormats.Byml.Nodes;

namespace Myzel.Lib.FileFormats.Byml;

/// <summary>
/// A class holding information about a BYML file.
/// </summary>
public class BymlFile
{
    #region public properties
    /// <summary>
    /// Gets the version of the BYML file.
    /// </summary>
    public int Version { get; init; }

    /// <summary>
    /// Gets the root <see cref="INode"/> object.
    /// </summary>
    public INode RootNode { get; set; } = null!;
    #endregion

    #region public methods
    /// <inheritdoc cref="NodeExtensions.Find"/>
    public INode? Find(string path) => RootNode.Find(path);

    /// <inheritdoc cref="NodeExtensions.Find{T}"/>
    public T? Find<T>(string path) where T : class, INode => RootNode.Find<T>(path);
    #endregion
}
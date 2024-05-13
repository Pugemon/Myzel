using System.Collections;

namespace Myzel.Lib.FileFormats.Aamp.Parameters;

/// <summary>
/// A class representing a parameter object in an AAMP file.
/// </summary>
public class ParameterObject : IEnumerable<Parameter>
{
    #region public properties
    /// <summary>
    /// Gets or sets the name of the object.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets a list of <see cref="Parameter"/> instances.
    /// </summary>
    public IList<Parameter> Parameters { get; } = new List<Parameter>();
    #endregion

    #region IEnumerable interface
    /// <inheritdoc/>
    public IEnumerator<Parameter> GetEnumerator() => Parameters.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}
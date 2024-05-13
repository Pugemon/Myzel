namespace Myzel.Lib.FileFormats.Aamp.Parameters;

/// <summary>
/// A class representing a parameter list in an AAMP file.
/// </summary>
public class ParameterList
{
    #region public properties
    /// <summary>
    /// Gets or sets the name of the list.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets a list of <see cref="ParameterObject"/> instances.
    /// </summary>
    public IList<ParameterObject> Objects { get; } = new List<ParameterObject>();

    /// <summary>
    /// Gets a list of <see cref="ParameterList"/> instances.
    /// </summary>
    public IList<ParameterList> Lists { get; } = new List<ParameterList>();
    #endregion
}
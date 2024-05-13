using Myzel.Lib.FileFormats.Bcsv.Converters;

namespace Myzel.Lib.FileFormats.Bcsv.Attributes;

/// <summary>
/// Provides the <see cref="BcsvCrc32EnumConverter"/> with an explicit name for an <see langword="enum"/> value.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class BcsvCrc32EnumNameAttribute : Attribute
{
    #region constructor
    /// <summary>
    /// Provides the <see cref="BcsvCrc32EnumConverter"/> with an explicit name for an <see langword="enum"/> value.
    /// </summary>
    /// <param name="name">The name of the <see langword="enum"/> value to use for hashing.</param>
    public BcsvCrc32EnumNameAttribute(string name) => Name = name;
    #endregion

    #region public properties
    /// <summary>
    /// The name of the <see langword="enum"/> value to use for hashing.
    /// </summary>
    public string Name { get; }
    #endregion
}
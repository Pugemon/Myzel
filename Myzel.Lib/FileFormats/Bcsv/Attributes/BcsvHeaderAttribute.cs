using Myzel.Lib.FileFormats.Bcsv.Converters;

namespace Myzel.Lib.FileFormats.Bcsv.Attributes;

/// <summary>
/// Provides the BCSV serializer with value information.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class BcsvHeaderAttribute : Attribute
{
    #region constructors
    /// <summary>
    /// Provides the BCSV serializer with value information.
    /// </summary>
    /// <param name="name">The name of the BCSV column to use for this property.</param>
    public BcsvHeaderAttribute(string name) : this(name, BcsvDataType.Default)
    { }

    /// <summary>
    /// Provides the BCSV serializer with value information.
    /// </summary>
    /// <param name="dataType">The data-type of the BCSV value.</param>
    public BcsvHeaderAttribute(BcsvDataType dataType) : this(null, dataType)
    { }

    /// <summary>
    /// Provides the BCSV serializer with value information.
    /// </summary>
    /// <param name="converter">A custom <see cref="IBcsvConverter"/> converter for the BCSV value.</param>
    public BcsvHeaderAttribute(Type converter) : this(null, converter)
    { }

    /// <summary>
    /// Provides the BCSV serializer with value information.
    /// </summary>
    /// <param name="name">The name of the BCSV column to use for this property.</param>
    /// <param name="dataType">The data-type of the BCSV value.</param>
    public BcsvHeaderAttribute(string? name, BcsvDataType dataType)
    {
        Name = name;
        DataType = dataType;
    }

    /// <summary>
    /// Provides the BCSV serializer with value information.
    /// </summary>
    /// <param name="name">The name of the BCSV column to use for this property.</param>
    /// <param name="converter">A custom <see cref="IBcsvConverter"/> converter for the BCSV value.</param>
    public BcsvHeaderAttribute(string? name, Type converter)
    {
        if (!typeof(IBcsvConverter).IsAssignableFrom(converter)) throw new ArgumentException("Convert does not implement " + nameof(IBcsvConverter) + " interface.");

        Name = name;
        DataType = BcsvDataType.Default;
        Converter = converter;
    }
    #endregion

    #region public properties
    /// <summary>
    /// The name of the BCSV column to use for this property.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// The data-type of the BCSV value.
    /// </summary>
    public BcsvDataType DataType { get; }

    /// <summary>
    /// A custom <see cref="IBcsvConverter"/> converter for the BCSV value.
    /// </summary>
    public Type? Converter { get; }
    #endregion
}
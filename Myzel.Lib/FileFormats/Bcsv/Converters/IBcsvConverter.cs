namespace Myzel.Lib.FileFormats.Bcsv.Converters;

/// <summary>
/// An interface for custom BCSV data converters.
/// </summary>
public interface IBcsvConverter
{
    /// <summary>
    /// Converts the given data to a specific target type.
    /// The result is written to the class property.
    /// </summary>
    /// <param name="data">The value data as <see cref="byte"/> array.</param>
    /// <param name="targetType">The target type of the class property.</param>
    /// <returns>A converted representation of the value data.</returns>
    public object? Convert(byte[] data, Type targetType);
}
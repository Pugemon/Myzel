using Myzel.Lib.FileFormats.Msbt.FormatProvider;
using Myzel.Lib.FileFormats.Msbt.FunctionTable;

namespace Myzel.Lib.FileFormats.Bmg.Serializers;

/// <summary>
/// An interface for serializing <see cref="BmgFile"/> objects.
/// </summary>
public interface IBmgSerializer : IFileSerializer<BmgFile>
{
    #region properties
    /// <summary>
    /// Gets or sets the function table to use.
    /// </summary>
    public IMsbtFunctionTable FunctionTable { get; set; }

    /// <summary>
    /// Gets or sets the message format provider to use.
    /// </summary>
    public IMsbtFormatProvider FormatProvider { get; set; }
    #endregion

    #region methods
    /// <summary>
    /// Serializes a collection of <see cref="BmgFile"/> objects from multiple languages.
    /// </summary>
    /// <param name="writer">A <see cref="TextWriter"/> to use for the serialization.</param>
    /// <param name="files">The collection of <see cref="BmgFile"/> objects to serialize.</param>
    public void Serialize(TextWriter writer, IEnumerable<BmgFile> files);
    #endregion
}
using Newtonsoft.Json;

namespace Myzel.Lib.FileFormats.Bcsv.Serializers;

/// <summary>
/// A class for serializing <see cref="BcsvFile"/> objects to JSON.
/// </summary>
public class BcsvJsonSerializer : IFileSerializer<BcsvFile>
{
    #region public properties
    /// <summary>
    /// Gets or sets number of indentation characters that should be used.
    /// '<c>0</c>' disables indentation.
    /// The default value is <c>2</c>.
    /// </summary>
    public int Indentation { get; set; } = 2;

    /// <summary>
    /// Gets or sets the indentation character that should be used.
    /// The default value is '<c> </c>'.
    /// </summary>
    public char IndentChar { get; set; } = ' ';
    #endregion

    #region IFileSerializer interface
    /// <inheritdoc />
    public void Serialize(TextWriter writer, BcsvFile file)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(file);

        using JsonTextWriter jsonWriter = new(writer);

        if (Indentation > 0)
        {
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.Indentation = Indentation;
            jsonWriter.IndentChar = IndentChar;
        }
        else jsonWriter.Formatting = Formatting.None;

        jsonWriter.WriteStartArray();

        foreach (object?[] entry in file)
        {
            jsonWriter.WriteStartObject();

            for (int i = 0; i < entry.Length; ++i)
            {
                jsonWriter.WritePropertyName(file.HeaderInfo[i].NewHeaderName);
                jsonWriter.WriteValue(entry[i] ?? string.Empty);
            }

            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndArray();
    }
    #endregion
}
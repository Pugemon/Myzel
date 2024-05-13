using Myzel.Lib.FileFormats.Byml.Nodes;
using Myzel.Lib.Utils.Extensions;
using Newtonsoft.Json;

namespace Myzel.Lib.FileFormats.Byml.Serializers;

/// <summary>
/// A class for serializing <see cref="BymlFile"/> objects to JSON.
/// </summary>
public class BymlJsonSerializer : IFileSerializer<BymlFile>
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
    public void Serialize(TextWriter writer, BymlFile file)
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

        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("version");
        jsonWriter.WriteValue(file.Version);

        jsonWriter.WritePropertyName("rootNode");
        WriteNode(jsonWriter, file.RootNode, jsonWriter.Formatting);

        jsonWriter.WriteEndObject();
    }
    #endregion

    #region private methods
    //writes the JSON elements for a given node
    private static void WriteNode(JsonWriter writer, INode node, Formatting defaultFormatting)
    {
        switch (node)
        {
            case DictionaryNode dict:
                writer.Formatting = defaultFormatting;
                writer.WriteStartObject();
                foreach (KeyValuePair<string, INode> item in dict)
                {
                    writer.WritePropertyName(item.Key);
                    WriteNode(writer, item.Value, defaultFormatting);
                }
                writer.WriteEndObject();
                break;
            case ArrayNode array:
                writer.Formatting = defaultFormatting;
                writer.WriteStartArray();
                writer.Formatting = Formatting.None;
                foreach (INode item in array) WriteNode(writer, item, defaultFormatting);
                writer.WriteEndArray();
                writer.Formatting = defaultFormatting;
                break;
            case IValueNode value:
                writer.WriteValue(value.GetValue());
                break;
            case PathNode path:
                writer.Formatting = defaultFormatting;
                writer.WriteStartObject();
                writer.WritePropertyName("positionX");
                writer.WriteValue(path.PositionX);
                writer.WritePropertyName("positionY");
                writer.WriteValue(path.PositionY);
                writer.WritePropertyName("positionZ");
                writer.WriteValue(path.PositionZ);
                writer.WritePropertyName("normalX");
                writer.WriteValue(path.NormalX);
                writer.WritePropertyName("normalY");
                writer.WriteValue(path.NormalY);
                writer.WritePropertyName("normalZ");
                writer.WriteValue(path.NormalZ);
                writer.WriteEndObject();
                break;
            case BinaryDataNode binary:
                writer.WriteStartObject();
                writer.WritePropertyName("size");
                writer.WriteValue(binary.Size);
                if (binary is AlignedBinaryDataNode alignedBinary)
                {
                    writer.WritePropertyName("alignment");
                    writer.WriteValue(alignedBinary.Alignment);
                }
                writer.WritePropertyName("data");
                writer.WriteValue(binary.Data.ToHexString(true));
                writer.WriteEndObject();
                break;
            case NullNode:
                writer.WriteNull();
                break;
        }
    }
    #endregion
}
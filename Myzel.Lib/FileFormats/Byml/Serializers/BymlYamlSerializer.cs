using Myzel.Lib.FileFormats.Byml.Nodes;
using Myzel.Lib.Utils.Extensions;
using Myzel.Lib.Utils.YamlTextWriter;

namespace Myzel.Lib.FileFormats.Byml.Serializers;

/// <summary>
/// A class for serializing <see cref="BymlFile"/> objects to YAML.
/// </summary>
public class BymlYamlSerializer : IFileSerializer<BymlFile>
{
    #region IFileSerializer interface
    /// <inheritdoc />
    public void Serialize(TextWriter writer, BymlFile file)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(file);

        using YamlTextWriter yamlWriter = new(writer);

        yamlWriter.WriteStartDocument();
        yamlWriter.WritePropertyName("version");
        yamlWriter.WriteValue(file.Version);

        yamlWriter.WritePropertyName("rootNode");
        WriteNode(yamlWriter, file.RootNode);
    }
    #endregion

    #region private methods
    //writes the YAML elements for a given node
    private static void WriteNode(YamlTextWriter writer, INode node)
    {
        switch (node)
        {
            case DictionaryNode dict:
                writer.WriteStartDictionary();
                foreach (KeyValuePair<string, INode> item in dict)
                {
                    writer.WritePropertyName(item.Key);
                    WriteNode(writer, item.Value);
                }
                writer.WriteEndDictionary();
                break;
            case ArrayNode array:
                writer.WriteStartArray();
                foreach (INode item in array) WriteNode(writer, item);
                writer.WriteEndArray();
                break;
            case IValueNode value:
                writer.WriteValue(value.GetValue());
                break;
            case PathNode path:
                writer.WriteStartDictionary();
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
                writer.WriteEndDictionary();
                break;
            case BinaryDataNode binary:
                writer.WriteStartDictionary();
                writer.WritePropertyName("size");
                writer.WriteValue(binary.Size);
                if (binary is AlignedBinaryDataNode alignedBinary)
                {
                    writer.WritePropertyName("alignment");
                    writer.WriteValue(alignedBinary.Alignment);
                }
                writer.WritePropertyName("data");
                writer.WriteValue(binary.Data.ToHexString(true));
                writer.WriteEndDictionary();
                break;
            case NullNode:
                writer.WriteNull();
                break;
        }
    }
    #endregion
}
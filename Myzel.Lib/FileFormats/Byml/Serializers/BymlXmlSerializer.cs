using System.Xml;
using Myzel.Lib.FileFormats.Byml.Nodes;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Lib.FileFormats.Byml.Serializers;

/// <summary>
/// A class for serializing <see cref="BymlFile"/> objects to XML.
/// </summary>
public class BymlXmlSerializer : IFileSerializer<BymlFile>
{
    #region public properties
    /// <summary>
    /// Gets or sets number of indentation characters that should be used.
    /// '<c>0</c>' disables indentation.
    /// The default value is <c>2</c>.
    /// </summary>
    private int Indentation { get; set; } = 2;

    /// <summary>
    /// Gets or sets the indentation character that should be used.
    /// The default value is '<c> </c>'.
    /// </summary>
    private char IndentChar { get; set; } = ' ';
    #endregion

    #region IFileSerializer interface
    /// <inheritdoc />
    public void Serialize(TextWriter writer, BymlFile file)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(file);

        using XmlTextWriter xmlWriter = new(writer);

        if (Indentation > 0)
        {
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.Indentation = Indentation;
            xmlWriter.IndentChar = IndentChar;
        }
        else xmlWriter.Formatting = Formatting.None;

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("byml");

        xmlWriter.WriteStartElement("version");
        xmlWriter.WriteValue(file.Version);
        xmlWriter.WriteEndElement();

        WriteNode(xmlWriter, file.RootNode);

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
    }
    #endregion

    #region private methods
    //writes the XML elements for a given node
    private static void WriteNode(XmlWriter writer, INode node)
    {
        switch (node)
        {
            case DictionaryNode dict:
                writer.WriteStartElement("dict");
                foreach (KeyValuePair<string, INode> item in dict)
                {
                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("name", item.Key);
                    WriteNode(writer, item.Value);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                break;
            case ArrayNode array:
                writer.WriteStartElement("array");
                for (var i = 0; i < array.Count; ++i)
                {
                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("index", i.ToString());
                    WriteNode(writer, array[i]);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                break;
            case IValueNode value:
                writer.WriteStartElement("value");
                writer.WriteAttributeString("type", $"xs:{GetType(value)}");
                if (value.GetValue() is { } val) writer.WriteValue(val);
                writer.WriteEndElement();
                break;
            case PathNode path:
                writer.WriteStartElement("path");
                writer.WriteStartElement("positionX");
                writer.WriteValue(path.PositionX);
                writer.WriteEndElement();
                writer.WriteStartElement("positionY");
                writer.WriteValue(path.PositionY);
                writer.WriteEndElement();
                writer.WriteStartElement("positionZ");
                writer.WriteValue(path.PositionZ);
                writer.WriteEndElement();
                writer.WriteStartElement("normalX");
                writer.WriteValue(path.NormalX);
                writer.WriteEndElement();
                writer.WriteStartElement("normalY");
                writer.WriteValue(path.NormalY);
                writer.WriteEndElement();
                writer.WriteStartElement("normalZ");
                writer.WriteValue(path.NormalZ);
                writer.WriteEndElement();
                writer.WriteEndElement();
                break;
            case BinaryDataNode binary:
                writer.WriteStartElement("binary");
                writer.WriteAttributeString("size", binary.Size.ToString());
                if (binary is AlignedBinaryDataNode alignedBinary)
                {
                    writer.WriteAttributeString("alignment", alignedBinary.Alignment.ToString());
                }
                writer.WriteValue(binary.Data.ToHexString(true));
                writer.WriteEndElement();
                break;
            case NullNode:
                writer.WriteStartElement("null");
                writer.WriteEndElement();
                break;
        }
    }

    //gets XML standard data type from value
    private static string GetType(IValueNode node)
    {
        return node.GetValue() switch
        {
            bool => "boolean",
            sbyte => "byte",
            byte => "unsignedByte",
            short => "short",
            ushort => "unsignedShort",
            int => "int",
            uint => "unsignedInt",
            long => "long",
            ulong => "unsignedLong",
            float => "float",
            double => "double",
            string => "string",
            null => "null",
            _ => "undefined"
        };
    }
    #endregion
}
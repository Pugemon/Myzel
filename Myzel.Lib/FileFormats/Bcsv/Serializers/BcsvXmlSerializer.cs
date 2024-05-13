using System.Xml;

namespace Myzel.Lib.FileFormats.Bcsv.Serializers;

/// <summary>
/// A class for serializing <see cref="BcsvFile"/> objects to XML.
/// </summary>
public class BcsvXmlSerializer : IFileSerializer<BcsvFile>
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

        using XmlTextWriter xmlWriter = new(writer);

        if (Indentation > 0)
        {
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.Indentation = Indentation;
            xmlWriter.IndentChar = IndentChar;
        }
        else xmlWriter.Formatting = Formatting.None;

        xmlWriter.WriteStartDocument();

        foreach (object?[] entry in file)
        {
            xmlWriter.WriteStartElement("entry");

            for (int i = 0; i < entry.Length; ++i)
            {
                xmlWriter.WriteStartElement(file.HeaderInfo[i].NewHeaderName);
                xmlWriter.WriteRaw(entry[i]?.ToString() ?? string.Empty);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndDocument();
    }
    #endregion
}
using System.Text;
using System.Xml;
using Myzel.Lib.FileFormats.Msbt.FormatProvider;
using Myzel.Lib.FileFormats.Msbt.FunctionTable;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Lib.FileFormats.Msbt.Serializers;

/// <summary>
/// A class for serializing a <see cref="MsbtFile"/> object to XML.
/// </summary>
public class MsbtXmlSerializer : IMsbtSerializer
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

    /// <summary>
    /// Gets or sets the name of the root node.
    /// </summary>
    public string RootNode { get; set; } = "msbt";

    /// <summary>
    /// Determines whether to skip MSBT metadata like version and encoding.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool SkipMetadata { get; set; }

    /// <summary>
    /// Determines whether to ignore message metadata in the output.
    /// This includes message index, attribute data, and style index.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool SkipMessageMetadata { get; set; }
    #endregion

    #region IMsbtSerializer interface
    /// <inheritdoc />
    public IMsbtFunctionTable FunctionTable { get; set; } = new MsbtDefaultFunctionTable();

    /// <inheritdoc />
    public IMsbtFormatProvider FormatProvider { get; set; } = new MsbtXmlFormatProvider();

    /// <inheritdoc />
    public void Serialize(TextWriter writer, MsbtFile file)
    {
        if (FunctionTable is null) throw new ArgumentNullException(nameof(FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(FormatProvider));
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (file is null) throw new ArgumentNullException(nameof(file));

        using var xmlWriter = new XmlTextWriter(writer);

        if (Indentation > 0)
        {
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.Indentation = Indentation;
            xmlWriter.IndentChar = IndentChar;
        }
        else xmlWriter.Formatting = Formatting.None;

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement(RootNode);

        if (!SkipMetadata) //write meta data
        {
            xmlWriter.WriteAttributeString("bigEndian", file.BigEndian.ToString());
            xmlWriter.WriteAttributeString("version", file.Version.ToString());
            xmlWriter.WriteAttributeString("encoding", file.Encoding.WebName);
            xmlWriter.WriteAttributeString("hasNli1", file.HasNli1.ToString());
            xmlWriter.WriteAttributeString("hasLbl1", file.HasLbl1.ToString());
            if (file.HasLbl1) xmlWriter.WriteAttributeString("labelGroups", file.LabelGroups.ToString());
            xmlWriter.WriteAttributeString("hasAtr1", file.HasAtr1.ToString());
            if (file.HasAtr1) xmlWriter.WriteAttributeString("hasAttributeText", file.HasAttributeText.ToString());
            if (file.AdditionalAttributeData.Length > 0) xmlWriter.WriteAttributeString("additionalAttributeData", file.AdditionalAttributeData.ToHexString(true));
            xmlWriter.WriteAttributeString("hasTsy1", file.HasTsy1.ToString());
        }

        foreach (var message in file.Messages)
        {
            xmlWriter.WriteStartElement("message");
            xmlWriter.WriteAttributeString("label", message.Label);
            if (!SkipMessageMetadata)
            {
                if (file.HasTsy1)
                {
                    xmlWriter.WriteAttributeString("index", message.Index?.ToString() ?? string.Empty);
                }
                if (file.HasAtr1)
                {
                    xmlWriter.WriteAttributeString("attribute",
                        (file.HasAttributeText
                            ? message.AttributeText
                            : message.Attribute.ToHexString(true)) ?? string.Empty);
                }
                if (file.HasTsy1)
                {
                    xmlWriter.WriteAttributeString("styleIndex", message.StyleIndex?.ToString() ?? string.Empty);
                }
            }
            xmlWriter.WriteRaw(message.ToCompiledString(FunctionTable, FormatProvider, file.BigEndian, file.Encoding));
            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        xmlWriter.Flush();
        xmlWriter.Close();
    }

    /// <inheritdoc />
    public void Serialize(TextWriter writer, IEnumerable<MsbtFile> files)
    {
        if (FunctionTable is null) throw new ArgumentNullException(nameof(FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(FormatProvider));
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (files is null) throw new ArgumentNullException(nameof(files));

        var fileArr = files as MsbtFile[] ?? files.ToArray();
        var languages = new string[fileArr.Length];

        //merge messages by label
        var remappedMessages = new Dictionary<string, (MsbtMessage Message, bool BigEndian, Encoding Encoding)?[]>();
        for (var i = 0; i < fileArr.Length; ++i)
        {
            languages[i] = fileArr[i].Language ?? i.ToString();
            var bigEndian = fileArr[i].BigEndian;
            var encoding = fileArr[i].Encoding;

            foreach (var message in fileArr[i].Messages)
            {
                remappedMessages.TryAdd(message.Label, new (MsbtMessage, bool, Encoding)?[fileArr.Length]);
                remappedMessages[message.Label][i] = (message, bigEndian, encoding);
            }
        }

        using var xmlWriter = new XmlTextWriter(writer);

        if (Indentation > 0)
        {
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.Indentation = Indentation;
            xmlWriter.IndentChar = IndentChar;
        }
        else xmlWriter.Formatting = Formatting.None;

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement(RootNode);

        if (!SkipMetadata && fileArr.Length > 0) //write meta data
        {
            xmlWriter.WriteAttributeString("bigEndian", fileArr[0].BigEndian.ToString());
            xmlWriter.WriteAttributeString("version", fileArr[0].Version.ToString());
            xmlWriter.WriteAttributeString("encoding", fileArr[0].Encoding.WebName);
            xmlWriter.WriteAttributeString("hasNli1", fileArr[0].HasNli1.ToString());
            xmlWriter.WriteAttributeString("hasLbl1", fileArr[0].HasLbl1.ToString());
            if (fileArr[0].HasLbl1) xmlWriter.WriteAttributeString("labelGroups", fileArr[0].LabelGroups.ToString());
            xmlWriter.WriteAttributeString("hasAtr1", fileArr[0].HasAtr1.ToString());
            if (fileArr[0].HasAtr1) xmlWriter.WriteAttributeString("hasAttributeText", fileArr[0].HasAttributeText.ToString());
            if (fileArr[0].AdditionalAttributeData.Length > 0) xmlWriter.WriteAttributeString("additionalAttributeData", fileArr[0].AdditionalAttributeData.ToHexString(true));
            xmlWriter.WriteAttributeString("hasTsy1", fileArr[0].HasTsy1.ToString());
        }

        //ensure original sort order persists
        foreach (var orderMessage in fileArr.FirstOrDefault()?.Messages ?? [])
        {
            xmlWriter.WriteStartElement("message");
            xmlWriter.WriteAttributeString("label", orderMessage.Label);

            if (!SkipMessageMetadata)
            {
                if (fileArr[0].HasTsy1)
                {
                    xmlWriter.WriteAttributeString("index", orderMessage.Index?.ToString() ?? string.Empty);
                }
                if (fileArr[0].HasAtr1)
                {
                    xmlWriter.WriteAttributeString("attribute",
                        (fileArr[0].HasAttributeText
                            ? orderMessage.AttributeText
                            : orderMessage.Attribute.ToHexString(true)) ?? string.Empty);
                }
                if (fileArr[0].HasTsy1)
                {
                    xmlWriter.WriteAttributeString("styleIndex", orderMessage.StyleIndex?.ToString() ?? string.Empty);
                }
            }

            for (var i = 0; i < remappedMessages[orderMessage.Label].Length; ++i)
            {
                xmlWriter.WriteStartElement("language");
                xmlWriter.WriteAttributeString("type", languages[i]);
                var messageData = remappedMessages[orderMessage.Label][i];
                if (messageData is not null) xmlWriter.WriteRaw(messageData.Value.Message.ToCompiledString(FunctionTable, FormatProvider, messageData.Value.BigEndian, messageData.Value.Encoding));
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        xmlWriter.Flush();
        xmlWriter.Close();
    }
    #endregion
}

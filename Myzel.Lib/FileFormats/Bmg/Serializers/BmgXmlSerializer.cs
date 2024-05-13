using System.Text;
using System.Xml;
using Myzel.Lib.FileFormats.Msbt;
using Myzel.Lib.FileFormats.Msbt.FormatProvider;
using Myzel.Lib.FileFormats.Msbt.FunctionTable;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Lib.FileFormats.Bmg.Serializers;

/// <summary>
/// A class for serializing a <see cref="BmgFile"/> object to XML.
/// </summary>
public class BmgXmlSerializer : IBmgSerializer
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
    public string RootNode { get; set; } = "bmg";

    /// <summary>
    /// Determines whether to skip BMG metadata like version and encoding.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool SkipMetadata { get; set; }

    /// <summary>
    /// Determines whether to ignore attribute values in the output.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool IgnoreAttributes { get; set; }
    #endregion

    #region IMsbtSerializer interface
    /// <inheritdoc />
    public IMsbtFunctionTable FunctionTable { get; set; } = new MsbtDefaultFunctionTable();

    /// <inheritdoc />
    public IMsbtFormatProvider FormatProvider { get; set; } = new MsbtXmlFormatProvider();

    /// <inheritdoc />
    public void Serialize(TextWriter writer, BmgFile file)
    {
        if (FunctionTable is null) throw new ArgumentNullException(nameof(BmgXmlSerializer.FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(BmgXmlSerializer.FormatProvider));
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(file);

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
            xmlWriter.WriteAttributeString("encoding", file.Encoding.WebName);
            xmlWriter.WriteAttributeString("fileId", file.FileId.ToString());
            xmlWriter.WriteAttributeString("defaultColor", file.DefaultColor.ToString());
            xmlWriter.WriteAttributeString("hasMid1", file.HasMid1.ToString());
            if (file.HasMid1) xmlWriter.WriteAttributeString("mid1Format", file.Mid1Format.ToHexString(true));
        }

        foreach (var message in file.Messages)
        {
            xmlWriter.WriteStartElement("message");
            xmlWriter.WriteAttributeString("label", message.Label);
            if (!IgnoreAttributes)
            {
                xmlWriter.WriteAttributeString("attribute", message.Attribute.ToHexString(true));
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
    public void Serialize(TextWriter writer, IEnumerable<BmgFile> files)
    {
        if (FunctionTable is null) throw new ArgumentNullException(nameof(BmgXmlSerializer.FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(BmgXmlSerializer.FormatProvider));
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(files);

        var fileArr = files as BmgFile[] ?? files.ToArray();
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
            xmlWriter.WriteAttributeString("encoding", fileArr[0].Encoding.WebName);
            xmlWriter.WriteAttributeString("fileId", fileArr[0].FileId.ToString());
            xmlWriter.WriteAttributeString("defaultColor", fileArr[0].DefaultColor.ToString());
            xmlWriter.WriteAttributeString("hasMid1", fileArr[0].HasMid1.ToString());
            if (fileArr[0].HasMid1) xmlWriter.WriteAttributeString("mid1Format", fileArr[0].Mid1Format.ToHexString(true));
        }

        //ensure original sort order persists
        foreach (var orderMessage in fileArr.FirstOrDefault()?.Messages ?? [])
        {
            xmlWriter.WriteStartElement("message");
            xmlWriter.WriteAttributeString("label", orderMessage.Label);

            if (!IgnoreAttributes)
            {
                xmlWriter.WriteAttributeString("attribute", orderMessage.Attribute.ToHexString(true));
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
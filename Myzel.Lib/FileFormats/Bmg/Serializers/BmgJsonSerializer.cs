using System.Text;
using Myzel.Lib.FileFormats.Msbt;
using Myzel.Lib.FileFormats.Msbt.FormatProvider;
using Myzel.Lib.FileFormats.Msbt.FunctionTable;
using Myzel.Lib.Utils.Extensions;
using Newtonsoft.Json;

namespace Myzel.Lib.FileFormats.Bmg.Serializers;

/// <summary>
/// A class for serializing a <see cref="BmgFile"/> object to JSON.
/// </summary>
public class BmgJsonSerializer : IBmgSerializer
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
    /// Determines whether to skip BMG metadata like version and encoding.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool SkipMetadata { get; set; }

    /// <summary>
    /// Determines whether the serialized result should be an object where each message is a property instead of an array of message objects.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool WriteAsObject { get; set; }

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
    public IMsbtFormatProvider FormatProvider { get; set; } = new MsbtJsonFormatProvider();

    /// <inheritdoc />
    public void Serialize(TextWriter writer, BmgFile file)
    {
        if (FunctionTable is null) throw new ArgumentNullException(nameof(BmgJsonSerializer.FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(BmgJsonSerializer.FormatProvider));
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

        if (!SkipMetadata) //write meta data
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("bigEndian");
            jsonWriter.WriteValue(file.BigEndian);
            jsonWriter.WritePropertyName("encoding");
            jsonWriter.WriteValue(file.Encoding.WebName);
            jsonWriter.WritePropertyName("fileId");
            jsonWriter.WriteValue(file.FileId);
            jsonWriter.WritePropertyName("defaultColor");
            jsonWriter.WriteValue(file.DefaultColor);
            jsonWriter.WritePropertyName("hasMid1");
            jsonWriter.WriteValue(file.HasMid1);
            if (file.HasMid1)
            {
                jsonWriter.WritePropertyName("mid1Format");
                jsonWriter.WriteValue(file.Mid1Format.ToHexString(true));
            }
            jsonWriter.WritePropertyName("messages");
        }

        if (WriteAsObject) //write one big object, only containing labels and texts
        {
            jsonWriter.WriteStartObject();

            foreach (MsbtMessage message in file.Messages)
            {
                jsonWriter.WritePropertyName(message.Label);
                jsonWriter.WriteValue(message.ToCompiledString(FunctionTable, FormatProvider, file.BigEndian, file.Encoding));
            }

            jsonWriter.WriteEndObject();
        }
        else //write array of full message objects
        {
            jsonWriter.WriteStartArray();

            foreach (var message in file.Messages)
            {
                jsonWriter.WriteStartObject();

                jsonWriter.WritePropertyName("label");
                jsonWriter.WriteValue(message.Label);

                if (!IgnoreAttributes)
                {
                    jsonWriter.WritePropertyName("attribute");
                    jsonWriter.WriteValue(message.Attribute.ToHexString(true));
                }

                jsonWriter.WritePropertyName("text");
                jsonWriter.WriteValue(message.ToCompiledString(FunctionTable, FormatProvider, file.BigEndian, file.Encoding));

                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
        }

        if (!SkipMetadata) jsonWriter.WriteEndObject();

        jsonWriter.Flush();
        jsonWriter.Close();
    }

    /// <inheritdoc />
    public void Serialize(TextWriter writer, IEnumerable<BmgFile> files)
    {
        if (FunctionTable is null) throw new ArgumentNullException(nameof(BmgJsonSerializer.FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(BmgJsonSerializer.FormatProvider));
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(files);

        BmgFile[] fileArr = files as BmgFile[] ?? files.ToArray();
        string[] languages = new string[fileArr.Length];

        //merge messages by label
        Dictionary<string, (MsbtMessage Message, bool BigEndian, Encoding Encoding)?[]> remappedMessages = new();
        for (int i = 0; i < fileArr.Length; ++i)
        {
            languages[i] = fileArr[i].Language ?? i.ToString();
            bool bigEndian = fileArr[i].BigEndian;
            Encoding encoding = fileArr[i].Encoding;

            foreach (MsbtMessage message in fileArr[i].Messages)
            {
                remappedMessages.TryAdd(message.Label, new (MsbtMessage, bool, Encoding)?[fileArr.Length]);
                remappedMessages[message.Label][i] = (message, bigEndian, encoding);
            }
        }

        using JsonTextWriter jsonWriter = new(writer);

        if (Indentation > 0)
        {
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.Indentation = Indentation;
            jsonWriter.IndentChar = IndentChar;
        }
        else jsonWriter.Formatting = Formatting.None;

        if (!SkipMetadata && fileArr.Length > 0) //write meta data
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("bigEndian");
            jsonWriter.WriteValue(fileArr[0].BigEndian);
            jsonWriter.WritePropertyName("encoding");
            jsonWriter.WriteValue(fileArr[0].Encoding.WebName);
            jsonWriter.WritePropertyName("fileId");
            jsonWriter.WriteValue(fileArr[0].FileId);
            jsonWriter.WritePropertyName("defaultColor");
            jsonWriter.WriteValue(fileArr[0].DefaultColor);
            jsonWriter.WritePropertyName("hasMid1");
            jsonWriter.WriteValue(fileArr[0].HasMid1);
            if (fileArr[0].HasMid1)
            {
                jsonWriter.WritePropertyName("mid1Format");
                jsonWriter.WriteValue(fileArr[0].Mid1Format.ToHexString(true));
            }
            jsonWriter.WritePropertyName("messages");
        }

        if (WriteAsObject)
        {
            jsonWriter.WriteStartObject();

            //ensure original sort order persists
            foreach (MsbtMessage orderMessage in fileArr.FirstOrDefault()?.Messages ?? [])
            {
                jsonWriter.WritePropertyName(orderMessage.Label);
                jsonWriter.WriteStartObject();

                if (!IgnoreAttributes)
                {
                    jsonWriter.WritePropertyName("attribute");
                    jsonWriter.WriteValue(orderMessage.Attribute.ToHexString(true));
                    jsonWriter.WritePropertyName("locale");
                    jsonWriter.WriteStartObject();
                }

                for (var i = 0; i < remappedMessages[orderMessage.Label].Length; ++i)
                {
                    jsonWriter.WritePropertyName(languages[i]);
                    var messageData = remappedMessages[orderMessage.Label][i];
                    jsonWriter.WriteValue(messageData is null ? string.Empty : messageData.Value.Message.ToCompiledString(FunctionTable, FormatProvider, messageData.Value.BigEndian, messageData.Value.Encoding));
                }

                if (!IgnoreAttributes) jsonWriter.WriteEndObject();

                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndObject();
        }
        else
        {
            jsonWriter.WriteStartArray();

            //ensure original sort order persists
            foreach (MsbtMessage orderMessage in fileArr.FirstOrDefault()?.Messages ?? [])
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("label");
                jsonWriter.WriteValue(orderMessage.Label);

                if (!IgnoreAttributes)
                {
                    jsonWriter.WritePropertyName("attribute");
                    jsonWriter.WriteValue(orderMessage.Attribute.ToHexString(true));
                }

                jsonWriter.WritePropertyName("locale");
                jsonWriter.WriteStartObject();
                for (int i = 0; i < remappedMessages[orderMessage.Label].Length; ++i)
                {
                    jsonWriter.WritePropertyName(languages[i]);
                    var messageData = remappedMessages[orderMessage.Label][i];
                    jsonWriter.WriteValue(messageData is null ? string.Empty : messageData.Value.Message.ToCompiledString(FunctionTable, FormatProvider, messageData.Value.BigEndian, messageData.Value.Encoding));
                }
                jsonWriter.WriteEndObject();

                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
        }

        if (!SkipMetadata) jsonWriter.WriteEndObject();

        jsonWriter.Flush();
        jsonWriter.Close();
    }
    #endregion
}
using System.Text;
using Myzel.Lib.FileFormats.Msbt.FormatProvider;
using Myzel.Lib.FileFormats.Msbt.FunctionTable;
using Myzel.Lib.Utils.Extensions;
using Newtonsoft.Json;

namespace Myzel.Lib.FileFormats.Msbt.Serializers;

/// <summary>
/// A class for serializing a <see cref="MsbtFile"/> object to JSON.
/// </summary>
public class MsbtJsonSerializer : IMsbtSerializer
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
    /// Determines whether to skip MSBT metadata like version and encoding.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool SkipMetadata { get; set; }

    /// <summary>
    /// Determines whether the serialized result should be an object where each message is a property instead of an array of message objects.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool WriteAsObject { get; set; }

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
    public IMsbtFormatProvider FormatProvider { get; set; } = new MsbtJsonFormatProvider();

    /// <inheritdoc />
    public void Serialize(TextWriter writer, MsbtFile file)
    {
        if (FunctionTable is null) throw new ArgumentNullException(nameof(FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(FormatProvider));
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (file is null) throw new ArgumentNullException(nameof(file));

        using var jsonWriter = new JsonTextWriter(writer);

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
            jsonWriter.WritePropertyName("version");
            jsonWriter.WriteValue(file.Version);
            jsonWriter.WritePropertyName("encoding");
            jsonWriter.WriteValue(file.Encoding.WebName);
            jsonWriter.WritePropertyName("hasNli1");
            jsonWriter.WriteValue(file.HasNli1);
            jsonWriter.WritePropertyName("hasLbl1");
            jsonWriter.WriteValue(file.HasLbl1);
            if (file.HasLbl1)
            {
                jsonWriter.WritePropertyName("labelGroups");
                jsonWriter.WriteValue(file.LabelGroups);
            }
            jsonWriter.WritePropertyName("hasAtr1");
            jsonWriter.WriteValue(file.HasAtr1);
            if (file.HasAtr1)
            {
                jsonWriter.WritePropertyName("hasAttributeText");
                jsonWriter.WriteValue(file.HasAttributeText);
            }
            if (file.AdditionalAttributeData.Length > 0)
            {
                jsonWriter.WritePropertyName("additionalAttributeData");
                jsonWriter.WriteValue(file.AdditionalAttributeData.ToHexString(true));
            }
            jsonWriter.WritePropertyName("hasTsy1");
            jsonWriter.WriteValue(file.HasTsy1);
            jsonWriter.WritePropertyName("messages");
        }

        if (WriteAsObject) //write one big object, only containing labels and texts
        {
            jsonWriter.WriteStartObject();

            foreach (var message in file.Messages)
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

                if (!SkipMessageMetadata)
                {
                    if (file.HasNli1)
                    {
                        jsonWriter.WritePropertyName("index");
                        if (message.Index.HasValue) jsonWriter.WriteValue(message.Index.Value);
                        else jsonWriter.WriteNull();
                    }
                    if (file.HasAtr1)
                    {
                        jsonWriter.WritePropertyName("attribute");
                        jsonWriter.WriteValue(file.HasAttributeText ? message.AttributeText : message.Attribute.ToHexString(true));
                    }
                    if (file.HasTsy1)
                    {
                        jsonWriter.WritePropertyName("styleIndex");
                        if (message.StyleIndex.HasValue) jsonWriter.WriteValue(message.StyleIndex.Value);
                        else jsonWriter.WriteNull();
                    }
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
    public void Serialize(TextWriter writer, IEnumerable<MsbtFile> files)
    {
        if (FunctionTable is null) throw new ArgumentNullException(nameof(FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(FormatProvider));
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(files);

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

        using var jsonWriter = new JsonTextWriter(writer);

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
            jsonWriter.WritePropertyName("version");
            jsonWriter.WriteValue(fileArr[0].Version);
            jsonWriter.WritePropertyName("encoding");
            jsonWriter.WriteValue(fileArr[0].Encoding.WebName);
            jsonWriter.WritePropertyName("hasNli1");
            jsonWriter.WriteValue(fileArr[0].HasNli1);
            jsonWriter.WritePropertyName("hasLbl1");
            jsonWriter.WriteValue(fileArr[0].HasLbl1);
            if (fileArr[0].HasLbl1)
            {
                jsonWriter.WritePropertyName("labelGroups");
                jsonWriter.WriteValue(fileArr[0].LabelGroups);
            }
            jsonWriter.WritePropertyName("hasAtr1");
            jsonWriter.WriteValue(fileArr[0].HasAtr1);
            if (fileArr[0].HasAtr1)
            {
                jsonWriter.WritePropertyName("hasAttributeText");
                jsonWriter.WriteValue(fileArr[0].HasAttributeText);
            }
            if (fileArr[0].AdditionalAttributeData.Length > 0)
            {
                jsonWriter.WritePropertyName("additionalAttributeData");
                jsonWriter.WriteValue(fileArr[0].AdditionalAttributeData.ToHexString(true));
            }
            jsonWriter.WritePropertyName("hasTsy1");
            jsonWriter.WriteValue(fileArr[0].HasTsy1);
            jsonWriter.WritePropertyName("messages");
        }

        if (WriteAsObject)
        {
            jsonWriter.WriteStartObject();

            //ensure original sort order persists
            foreach (var orderMessage in fileArr.FirstOrDefault()?.Messages ?? [])
            {
                jsonWriter.WritePropertyName(orderMessage.Label);
                jsonWriter.WriteStartObject();

                if (!SkipMetadata)
                {
                    if (fileArr[0].HasNli1)
                    {
                        jsonWriter.WritePropertyName("index");
                        if (orderMessage.Index.HasValue) jsonWriter.WriteValue(orderMessage.Index.Value);
                        else jsonWriter.WriteNull();
                    }
                    if (fileArr[0].HasAtr1)
                    {
                        jsonWriter.WritePropertyName("attribute");
                        jsonWriter.WriteValue(fileArr[0].HasAttributeText ? orderMessage.AttributeText : orderMessage.Attribute.ToHexString(true));
                    }
                    if (fileArr[0].HasTsy1)
                    {
                        jsonWriter.WritePropertyName("styleIndex");
                        if (orderMessage.StyleIndex.HasValue) jsonWriter.WriteValue(orderMessage.StyleIndex.Value);
                        else jsonWriter.WriteNull();
                    }
                }

                if (!SkipMetadata)
                {
                    jsonWriter.WritePropertyName("locale");
                    jsonWriter.WriteStartObject();
                }

                for (var i = 0; i < remappedMessages[orderMessage.Label].Length; ++i)
                {
                    jsonWriter.WritePropertyName(languages[i]);
                    var messageData = remappedMessages[orderMessage.Label][i];
                    jsonWriter.WriteValue(messageData is null ? string.Empty : messageData.Value.Message.ToCompiledString(FunctionTable, FormatProvider, messageData.Value.BigEndian, messageData.Value.Encoding));
                }

                if (!SkipMetadata) jsonWriter.WriteEndObject();

                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndObject();
        }
        else
        {
            jsonWriter.WriteStartArray();

            //ensure original sort order persists
            foreach (var orderMessage in fileArr.FirstOrDefault()?.Messages ?? [])
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("label");
                jsonWriter.WriteValue(orderMessage.Label);

                if (!SkipMetadata)
                {
                    if (fileArr[0].HasNli1)
                    {
                        jsonWriter.WritePropertyName("index");
                        if (orderMessage.Index.HasValue) jsonWriter.WriteValue(orderMessage.Index.Value);
                        else jsonWriter.WriteNull();
                    }
                    if (fileArr[0].HasAtr1)
                    {
                        jsonWriter.WritePropertyName("attribute");
                        jsonWriter.WriteValue(fileArr[0].HasAttributeText ? orderMessage.AttributeText : orderMessage.Attribute.ToHexString(true));
                    }
                    if (fileArr[0].HasTsy1)
                    {
                        jsonWriter.WritePropertyName("styleIndex");
                        if (orderMessage.StyleIndex.HasValue) jsonWriter.WriteValue(orderMessage.StyleIndex.Value);
                        else jsonWriter.WriteNull();
                    }
                }

                jsonWriter.WritePropertyName("locale");
                jsonWriter.WriteStartObject();
                for (var i = 0; i < remappedMessages[orderMessage.Label].Length; ++i)
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

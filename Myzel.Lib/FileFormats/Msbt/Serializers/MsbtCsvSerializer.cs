using System.Text;
using Myzel.Lib.FileFormats.Msbt.FormatProvider;
using Myzel.Lib.FileFormats.Msbt.FunctionTable;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Lib.FileFormats.Msbt.Serializers;

/// <summary>
/// A class for serializing a <see cref="MsbtFile"/> object to CSV.
/// </summary>
public class MsbtCsvSerializer : IMsbtSerializer
{
    #region public properties
    /// <summary>
    /// Gets or sets the separator character that should be used.
    /// The default value is '<c>,</c>'.
    /// </summary>
    public string Separator { get; set; } = ",";

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
    public IMsbtFormatProvider FormatProvider { get; set; } = new MsbtDefaultFormatProvider();

    /// <inheritdoc />
    public void Serialize(TextWriter writer, MsbtFile file)
    {
        if (string.IsNullOrEmpty(Separator)) throw new FormatException("CSV separator cannot be empty.");
        if (Separator.Contains('=')) throw new FormatException($"\"{Separator}\" cannot be used as CSV separator.");
        if (FunctionTable is null) throw new ArgumentNullException(nameof(FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(FormatProvider));
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (file is null) throw new ArgumentNullException(nameof(file));

        //write header
        writer.Write("Label");
        if (!SkipMessageMetadata)
        {
            if (file.HasNli1)
            {
                writer.Write(Separator);
                writer.Write("Index");
            }
            if (file.HasAtr1)
            {
                writer.Write(Separator);
                writer.Write("Attribute");
            }
            if (file.HasTsy1)
            {
                writer.Write(Separator);
                writer.Write("StyleIndex");
            }
        }
        writer.Write(Separator);
        writer.WriteLine("Text");

        //write messages
        foreach (var message in file.Messages)
        {
            writer.Write(message.Label);

            if (!SkipMessageMetadata)
            {
                if (file.HasNli1)
                {
                    writer.Write(Separator);
                    writer.Write(message.Index?.ToString() ?? string.Empty);
                }
                if (file.HasAtr1)
                {
                    writer.Write(Separator);
                    writer.Write(file.HasAttributeText ? message.AttributeText : message.Attribute.ToHexString(true));
                }
                if (file.HasTsy1)
                {
                    writer.Write(Separator);
                    writer.Write(message.StyleIndex?.ToString() ?? string.Empty);
                }
            }

            writer.Write(Separator);
            var text = message.ToCompiledString(FunctionTable, FormatProvider, file.BigEndian, file.Encoding);
            var wrapText = text.Contains(Separator) || text.Contains('\n');
            if (wrapText && text.Contains('"')) text = text.Replace("\"", "\"\"");
            writer.WriteLine(wrapText ? '"' + text + '"' : text);
        }

        writer.Flush();
        writer.Close();
    }

    /// <inheritdoc />
    public void Serialize(TextWriter writer, IEnumerable<MsbtFile> files)
    {
        if (string.IsNullOrEmpty(Separator)) throw new FormatException("CSV separator cannot be empty.");
        if (Separator.Contains('=')) throw new FormatException($"\"{Separator}\" cannot be used as CSV separator.");
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

        //write header
        writer.Write("Label");
        if (!SkipMessageMetadata)
        {
            if (fileArr[0].HasNli1)
            {
                writer.Write(Separator);
                writer.Write("Index");
            }
            if (fileArr[0].HasAtr1)
            {
                writer.Write(Separator);
                writer.Write("Attribute");
            }
            if (fileArr[0].HasTsy1)
            {
                writer.Write(Separator);
                writer.Write("StyleIndex");
            }
        }
        foreach (var language in languages)
        {
            writer.Write(Separator);
            writer.Write(language);
        }
        writer.WriteLine();

        //ensure original sort order persists
        foreach (var orderMessage in fileArr.FirstOrDefault()?.Messages ?? [])
        {
            writer.Write(orderMessage.Label);

            if (!SkipMessageMetadata)
            {
                if (fileArr[0].HasNli1)
                {
                    writer.Write(Separator);
                    writer.Write(orderMessage.Index?.ToString() ?? string.Empty);
                }
                if (fileArr[0].HasAtr1)
                {
                    writer.Write(Separator);
                    writer.Write(fileArr[0].HasAttributeText ? orderMessage.AttributeText : orderMessage.Attribute.ToHexString(true));
                }
                if (fileArr[0].HasTsy1)
                {
                    writer.Write(Separator);
                    writer.Write(orderMessage.StyleIndex?.ToString() ?? string.Empty);
                }
            }

            foreach (var messageData in remappedMessages[orderMessage.Label])
            {
                writer.Write(Separator);
                if (messageData is null) continue;

                var text = messageData.Value.Message.ToCompiledString(FunctionTable, FormatProvider, messageData.Value.BigEndian, messageData.Value.Encoding);
                var wrapText = text.Contains(Separator) || text.Contains('\n');
                if (wrapText && text.Contains('"')) text = text.Replace("\"", "\"\"");
                writer.Write(wrapText ? '"' + text + '"' : text);
            }

            writer.WriteLine();
        }

        writer.Flush();
        writer.Close();
    }
    #endregion
}

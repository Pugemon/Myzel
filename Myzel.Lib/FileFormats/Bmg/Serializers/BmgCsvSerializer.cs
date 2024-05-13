using System.Text;
using Myzel.Lib.FileFormats.Msbt;
using Myzel.Lib.FileFormats.Msbt.FormatProvider;
using Myzel.Lib.FileFormats.Msbt.FunctionTable;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Lib.FileFormats.Bmg.Serializers;

/// <summary>
/// A class for serializing a <see cref="BmgFile"/> object to CSV.
/// </summary>
public class BmgCsvSerializer : IBmgSerializer
{
    #region public properties
    /// <summary>
    /// Gets or sets the separator character that should be used.
    /// The default value is '<c>,</c>'.
    /// </summary>
    public string Separator { get; set; } = ",";

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
    public IMsbtFormatProvider FormatProvider { get; set; } = new MsbtDefaultFormatProvider();

    /// <inheritdoc />
    public void Serialize(TextWriter writer, BmgFile file)
    {
        if (string.IsNullOrEmpty(Separator)) throw new FormatException("CSV separator cannot be empty.");
        if (Separator.Contains('=')) throw new FormatException($"\"{Separator}\" cannot be used as CSV separator.");
        if (FunctionTable is null) throw new ArgumentNullException(nameof(BmgCsvSerializer.FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(BmgCsvSerializer.FormatProvider));
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(file);

        //write header
        writer.Write("Label");
        if (!IgnoreAttributes)
        {
            writer.Write(Separator);
            writer.Write("Attribute");
        }
        writer.Write(Separator);
        writer.WriteLine("Text");

        //write messages
        foreach (MsbtMessage message in file.Messages)
        {
            writer.Write(message.Label);

            if (!IgnoreAttributes)
            {
                writer.Write(Separator);
                writer.Write(message.Attribute.ToHexString(true));
            }

            writer.Write(Separator);
            string text = message.ToCompiledString(FunctionTable, FormatProvider, file.BigEndian, file.Encoding);
            bool wrapText = text.Contains(Separator) || text.Contains('\n');
            if (wrapText && text.Contains('"')) text = text.Replace("\"", "\"\"");
            writer.WriteLine(wrapText ? '"' + text + '"' : text);
        }

        writer.Flush();
        writer.Close();
    }

    /// <inheritdoc />
    public void Serialize(TextWriter writer, IEnumerable<BmgFile> files)
    {
        if (string.IsNullOrEmpty(Separator)) throw new FormatException("CSV separator cannot be empty.");
        if (Separator.Contains('=')) throw new FormatException($"\"{Separator}\" cannot be used as CSV separator.");
        if (FunctionTable is null) throw new ArgumentNullException(nameof(BmgCsvSerializer.FunctionTable));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(BmgCsvSerializer.FormatProvider));
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

        //write header
        writer.Write("Label");
        if (!IgnoreAttributes)
        {
            writer.Write(Separator);
            writer.Write("Attribute");
        }
        foreach (string language in languages)
        {
            writer.Write(Separator);
            writer.Write(language);
        }
        writer.WriteLine();

        //ensure original sort order persists
        foreach (MsbtMessage orderMessage in fileArr.FirstOrDefault()?.Messages ?? [])
        {
            writer.Write(orderMessage.Label);

            if (!IgnoreAttributes)
            {
                writer.Write(Separator);
                writer.Write(orderMessage.Attribute.ToHexString(true));
            }

            foreach ((MsbtMessage Message, bool BigEndian, Encoding Encoding)? messageData in remappedMessages[orderMessage.Label])
            {
                writer.Write(Separator);
                if (messageData is null) continue;

                string text = messageData.Value.Message.ToCompiledString(FunctionTable, FormatProvider, messageData.Value.BigEndian, messageData.Value.Encoding);
                bool wrapText = text.Contains(Separator) || text.Contains('\n');
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
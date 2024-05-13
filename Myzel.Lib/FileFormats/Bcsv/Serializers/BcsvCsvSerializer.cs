namespace Myzel.Lib.FileFormats.Bcsv.Serializers;

/// <summary>
/// A class for serializing <see cref="BcsvFile"/> objects to CSV.
/// </summary>
public class BcsvCsvSerializer : IFileSerializer<BcsvFile>
{
    #region public properties
    /// <summary>
    /// Gets or sets the separator character that should be used.
    /// The default value is '<c>,</c>'.
    /// </summary>
    public string Separator { get; set; } = ",";
    #endregion

    #region IFileSerializer interface
    /// <inheritdoc />
    public void Serialize(TextWriter writer, BcsvFile file)
    {
        if (string.IsNullOrEmpty(Separator)) throw new FormatException("CSV separator cannot be empty.");
        if (Separator.Contains('=')) throw new FormatException($"\"{Separator}\" cannot be used as CSV separator.");
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(file);

        for (int i = 0; i < file.Columns; ++i)
        {
            if (i > 0) writer.Write(Separator);
            writer.Write(file.HeaderInfo[i].NewHeaderName);
        }
        writer.WriteLine();

        foreach (object?[] entry in file)
        {
            for (int i = 0; i < entry.Length; ++i)
            {
                if (i > 0) writer.Write(Separator);
                if (entry[i] is null) continue;

                string text = entry[i]?.ToString() ?? string.Empty;
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
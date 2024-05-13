using System.Text;
using Myzel.Lib.Utils;

namespace Myzel.Lib.FileFormats.Msbt;

/// <summary>
/// A class for compiling MSBT files.
/// </summary>
public class MsbtFileCompiler : IFileCompiler<MsbtFile>
{
    #region IFileCompiler interface
    /// <inheritdoc/>
    public void Compile(MsbtFile file, Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(fileStream);

        FileWriter writer = new(fileStream, true);
        writer.BigEndian = file.BigEndian;

        WriteHeader(writer, file);
        if (file.HasLbl1) WriteLbl1(writer, file);
        if (file.HasAtr1) WriteAtr1(writer, file);
        WriteTxt2(writer, file);

        uint size = (uint) writer.Position;
        writer.JumpTo(0x12);
        writer.Write(size);
    }
    #endregion

    #region private methods
    private static void WriteHeader(FileWriter writer, MsbtFile file)
    {
        writer.Write("MsgStdBn", Encoding.ASCII);
        writer.Write((ushort) 0xFEFF);
        writer.Pad(2);

        if (file.Encoding.Equals(Encoding.UTF8)) writer.Write((byte) 0x00);
        else if (file.Encoding.Equals(Encoding.Unicode) || file.Encoding.Equals(Encoding.BigEndianUnicode)) writer.Write((byte) 0x01);
        else if (file.Encoding.Equals(Encoding.UTF32)) writer.Write((byte) 0x02);
        else throw new InvalidDataException("Invalid text encoding format.");

        writer.Write((byte) file.Version);

        int sections = 1;
        if (file.HasLbl1) ++sections;
        if (file.HasAtr1) ++sections;
        writer.Write((ushort) sections);

        writer.Pad(16);
    }

    private static void WriteLbl1(FileWriter writer, MsbtFile file)
    {
        writer.Write("LBL1", Encoding.ASCII);
        long sizePosition = writer.Position;
        writer.Pad(12);

        IList<(string, int)>[] table = new IList<(string,int)>[file.LabelGroups];
        for (int i = 0; i < table.Length; ++i) table[i] = new List<(string,int)>();
        for (int i = 0; i < file.Messages.Count; ++i)
        {
            MsbtMessage message = file.Messages[i];
            int hash = GetLabelHash(message.Label, file.LabelGroups);
            table[hash].Add((message.Label, i));
        }

        int labelOffset = 4 + file.LabelGroups * 8;
        writer.Write(file.LabelGroups);
        foreach (IList<(string, int)> list in table)
        {
            writer.Write(list.Count);
            writer.Write(labelOffset);
            labelOffset += list.Sum(label => 5 + label.Item1.Length);
        }

        foreach (IList<(string, int)> list in table)
        {
            foreach ((string, int) label in list)
            {
                writer.Write((byte) label.Item1.Length);
                writer.Write(label.Item1, Encoding.ASCII);
                writer.Write(label.Item2);
            }
        }

        long endPos = writer.Position;
        writer.JumpTo(sizePosition);
        writer.Write(labelOffset);

        writer.JumpTo(endPos);
        writer.Align(16, 0xAB);
    }

    private static int GetLabelHash(string label, int groups)
    {
        int hash = label.Aggregate(0, (current, c) => current * 0x492 + c);
        return (int) ((hash & 0xFFFFFFFF) % groups);
    }

    private static void WriteAtr1(FileWriter writer, MsbtFile file)
    {
        writer.Write("ATR1", Encoding.ASCII);
        long sizePosition = writer.Position;
        writer.Pad(12);

        writer.Write(file.Messages.Count);
        int attributeSize = file.HasAttributeText ? 4 : file.Messages.FirstOrDefault()?.Attribute?.Length ?? 0;
        writer.Write(attributeSize);

        if (file.HasAttributeText)
        {
            long offsetPosition = writer.Position;
            writer.Pad(file.Messages.Count * 4);
            for (int i = 0; i < file.Messages.Count; ++i)
            {
                long startPos = writer.Position;
                writer.JumpTo(offsetPosition + i * 4);
                writer.Write((int) (startPos - offsetPosition + 8));
                writer.JumpTo(startPos);

                if (string.IsNullOrEmpty(file.Messages[i].AttributeText)) writer.Write("\0", file.Encoding);
                else writer.Write(file.Messages[i].AttributeText + '\0', file.Encoding);
            }
        }
        else
        {
            foreach (MsbtMessage message in file.Messages)
            {
                writer.Write(message.Attribute ?? new byte[attributeSize]);
            }
        }

        writer.Write(file.AdditionalAttributeData);

        long endPos = writer.Position;
        writer.JumpTo(sizePosition);
        writer.Write((int) (endPos - sizePosition - 12));

        writer.JumpTo(endPos);
        writer.Align(16, 0xAB);
    }

    private static void WriteTxt2(FileWriter writer, MsbtFile file)
    {
        writer.Write("TXT2", Encoding.ASCII);
        long sizePosition = writer.Position;
        writer.Pad(12);

        writer.Write(file.Messages.Count);
        long offsetPosition = writer.Position;
        writer.Pad(file.Messages.Count * 4);

        for (int i = 0; i < file.Messages.Count; ++i)
        {
            long startPos = writer.Position;
            writer.JumpTo(offsetPosition + i * 4);
            writer.Write((int) (startPos - offsetPosition + 4));
            writer.JumpTo(startPos);

            MsbtMessage message = file.Messages[i];
            string text = message.Text;
            for (int j = 0; j < message.Functions.Count; ++j)
            {
                string[] split = text.Split("{{" + j + "}}");
                writer.Write(split[0], file.Encoding);

                MsbtFunction function = message.Functions[j];
                if (function is { Group: 0x0F, Type: 0x00 })
                {
                    writer.Write((ushort) 0x0F);
                    writer.Write(function.Args);
                }
                else
                {
                    writer.Write((ushort) 0x0E);
                    writer.Write(function.Group);
                    writer.Write(function.Type);
                    writer.Write((ushort) function.Args.Length);
                    writer.Write(function.Args);
                }

                text = split[1];
            }
            writer.Write(text + '\0', file.Encoding);
        }

        var endPos = writer.Position;
        writer.JumpTo(sizePosition);
        writer.Write((int) (endPos - sizePosition - 12));

        writer.JumpTo(endPos);
        writer.Align(16, 0xAB);
    }
    #endregion
}
using System.Text;
using Myzel.Lib.FileFormats.Msbt;
using Myzel.Lib.Utils;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Lib.FileFormats.Bmg;

/// <summary>
/// A class for compiling BMG files.
/// </summary>
public class BmgFileCompiler : IFileCompiler<BmgFile>
{
    #region IFileCompiler interface
    /// <inheritdoc/>
    public void Compile(BmgFile file, Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(fileStream);

        var writer = new FileWriter(fileStream, true);
        writer.BigEndian = file.BigEndian;

        WriteHeader(writer, file);
        WriteInf1(writer, file);
        WriteDat1(writer, file, out var offsets);
        WriteInf1Data(writer, file, offsets);
        if (file.HasMid1) WriteMid1(writer, file);

        int size = (int) writer.Position;
        writer.JumpTo(0x08);
        writer.Write(size);
    }
    #endregion

    #region private methods
    private static void WriteHeader(FileWriter writer, BmgFile file)
    {
        writer.Write("MESGbmg1", Encoding.ASCII);
        writer.Pad(4);
        writer.Write(file.HasMid1 ? 3 : 2);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        if (file.Encoding.Equals(Encoding.GetEncoding(1252))) writer.Write((byte) 0x01);
        else if (file.Encoding.Equals(Encoding.Unicode)) writer.Write((byte) 0x02);
        else if (file.Encoding.Equals(Encoding.BigEndianUnicode)) writer.Write((byte) 0x02);
        else if (file.Encoding.Equals(Encoding.GetEncoding("Shift-JIS"))) writer.Write((byte) 0x03);
        else if (file.Encoding.Equals(Encoding.UTF8)) writer.Write((byte) 0x04);
        else throw new InvalidDataException("Invalid text encoding format.");

        writer.Align(16);
    }

    private static void WriteInf1(FileWriter writer, BmgFile file)
    {
        long startPos = writer.Position;
        writer.Write("INF1", Encoding.ASCII);
        long sizePosition = writer.Position;
        writer.Pad(4);

        int attributeLength = (file.Messages.FirstOrDefault()?.Attribute?.Length ?? 0) + 4;

        writer.Write((ushort) file.Messages.Count);
        writer.Write((ushort) attributeLength);
        writer.Write((ushort) file.FileId);
        writer.Write((byte) file.DefaultColor);
        writer.Pad(1);

        writer.Pad(file.Messages.Count * attributeLength);

        writer.Align(32); //section size includes padding
        long endPos = writer.Position;
        writer.JumpTo(sizePosition);
        writer.Write((uint) (endPos - startPos));
        writer.JumpTo(endPos);
    }

    private static void WriteInf1Data(FileWriter writer, BmgFile file, IReadOnlyList<uint> offsets)
    {
        long startPos = writer.Position;
        writer.JumpTo(0x30);

        for (int i = 0; i < offsets.Count; ++i)
        {
            writer.Write(offsets[i]);
            if (file.Messages[i].Attribute is not null) writer.Write(file.Messages[i].Attribute!);
        }

        writer.JumpTo(startPos);
    }

    private static void WriteDat1(FileWriter writer, BmgFile file, out uint[] offsets)
    {
        offsets = new uint[file.Messages.Count];

        long startPos = writer.Position;
        writer.Write("DAT1", Encoding.ASCII);
        long sizePosition = writer.Position;
        writer.Pad(4);

        long offsetStart = writer.Position;
        int encodingSize = file.Encoding.GetMinByteCount();
        writer.Pad(encodingSize);

        for (int i = 0; i < file.Messages.Count; ++i)
        {
            offsets[i] = (uint) (writer.Position - offsetStart);

            MsbtMessage message = file.Messages[i];
            string text = message.Text;
            for (int j = 0; j < message.Functions.Count; ++j)
            {
                string[] split = text.Split("{{" + j + "}}");
                writer.Write(split[0], file.Encoding);

                MsbtFunction function = message.Functions[j];
                if (encodingSize == 1)
                {
                    writer.Write((byte) 0x1A);
                    writer.Write((byte) (function.Args.Length + 5));
                }
                else
                {
                    writer.Write((ushort) 0x1A);
                    writer.Write((byte) (function.Args.Length + 6));
                }

                writer.Write((byte) function.Group);
                writer.Write(function.Type);
                writer.Write(function.Args);

                text = split[1];
            }
            writer.Write(text + '\0', file.Encoding);
        }

        writer.Align(32); //section size includes padding
        long endPos = writer.Position;
        writer.JumpTo(sizePosition);
        writer.Write((uint) (endPos - startPos));
        writer.JumpTo(endPos);
    }

    private static void WriteMid1(FileWriter writer, BmgFile file)
    {
        long startPos = writer.Position;
        writer.Write("MID1", Encoding.ASCII);
        long sizePosition = writer.Position;
        writer.Pad(4);

        writer.Write((ushort) file.Messages.Count);
        writer.Write(file.Mid1Format);
        writer.Pad(4);

        foreach (MsbtMessage message in file.Messages)
        {
            writer.Write(uint.Parse(message.Label));
        }

        writer.Align(32); //section size includes padding
        long endPos = writer.Position;
        writer.JumpTo(sizePosition);
        writer.Write((uint) (endPos - startPos));
        writer.JumpTo(endPos);
    }
    #endregion
}
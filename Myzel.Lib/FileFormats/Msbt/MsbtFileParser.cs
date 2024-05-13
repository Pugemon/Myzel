using System.Text;
using Myzel.Lib.Utils;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Lib.FileFormats.Msbt;

/// <summary>
/// A class for parsing MSBT files.
/// </summary>
public class MsbtFileParser : IFileParser<MsbtFile>
{
    #region private members
    private readonly string? _language;
    #endregion

    #region constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="MsbtFileParser"/> class without a language.
    /// </summary>
    public MsbtFileParser() : this(null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MsbtFileParser"/> class with the given language.
    /// Each parsed <see cref="MsbtFile"/> object will have the given language assigned once parsed.
    /// </summary>
    /// <param name="language">The language to use.</param>
    public MsbtFileParser(string? language) => _language = language;
    #endregion

    #region IFileParser interface
    /// <inheritdoc/>
    public bool CanParse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        return CanParse(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public MsbtFile Parse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        FileReader reader = new(fileStream);
        if (!CanParse(reader)) throw new InvalidDataException("File is not a MSBT file.");

        //parse file metadata and header
        GetMetaData(reader, out int sectionCount, out _, out int version, out Encoding encoding);

        //parse messages
        MsbtFile msbtFile = new()
        {
            BigEndian = reader.BigEndian,
            Version = version,
            Encoding = encoding,
            Language = _language
        };
        string[] labels = Array.Empty<string>();
        byte[][] attributes = Array.Empty<byte[]>();
        string[] attributeTexts = Array.Empty<string>();
        string[] content = Array.Empty<string>();
        IList<MsbtFunction>[] functions = Array.Empty<IList<MsbtFunction>>();

        long sectionOffset = 0x20;
        for (int i = 0; i < sectionCount; ++i)
        {
            reader.JumpTo(sectionOffset);
            reader.Align(16);

            string type = reader.ReadString(4, Encoding.ASCII);
            uint sectionSize = reader.ReadUInt32();
            sectionOffset += 0x10 + (sectionSize + 0xF & ~0xF);

            switch (type)
            {
                case "LBL1":
                    ParseLbl1(reader, out labels, out int labelGroups);
                    msbtFile.HasLbl1 = true;
                    msbtFile.LabelGroups = labelGroups;
                    break;
                case "ATR1":
                    ParseAtr1(reader, encoding, out attributes, out attributeTexts, out var additionalAttributeData);
                    msbtFile.HasAtr1 = true;
                    msbtFile.HasAttributeText = attributeTexts.Length > 0;
                    msbtFile.AdditionalAttributeData = additionalAttributeData;
                    break;
                case "TXT2":
                    ParseTxt2(reader, encoding, sectionSize, out content, out functions);
                    break;
            }
        }

        //compile messages
        string labelFormat = "D" + (content.Length - 1).ToString().Length;
        for (int i = 0; i < content.Length; ++i)
        {
            MsbtMessage message = new MsbtMessage
            {
                Label = labels.Length > 0 ? labels[i] : i.ToString(labelFormat),
                Attribute = i < attributes.Length ? attributes[i] : null,
                AttributeText = i < attributeTexts.Length ? attributeTexts[i] : null,
                Text = content[i],
                Functions = functions[i]
            };

            msbtFile.Messages.Add(message);
        }

        return msbtFile;
    }
    #endregion

    #region private methods
    //verifies that the file is a MSBT file
    private static bool CanParse(FileReader reader) => reader.ReadStringAt(0, 8, Encoding.ASCII) == "MsgStdBn";

    //parses meta data
    private static void GetMetaData(FileReader reader, out int sectionCount, out uint fileSize, out int version, out Encoding encoding)
    {
        byte byteOrder = reader.ReadByteAt(8);
        if (byteOrder == 0xFE) reader.BigEndian = true;

        encoding = reader.ReadByteAt(12) switch
        {
            0 => Encoding.UTF8,
            1 => reader.BigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode,
            2 => Encoding.UTF32,
            _ => reader.BigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode
        };

        version = reader.ReadByte(13);

        sectionCount = reader.ReadUInt16At(14);
        fileSize = reader.ReadUInt32At(18);
    }

    //parse LBL1 type sections (message label + index)
    private static void ParseLbl1(FileReader reader, out string[] labels, out int groups)
    {
        reader.Skip(8);
        long position = reader.Position;
        uint entryCount = reader.ReadUInt32();

        List<string> labelValues = [];
        List<uint> indices = [];

        for (int i = 0; i < entryCount; ++i)
        {
            //group header
            reader.JumpTo(position + 4 + i * 8);
            uint labelCount = reader.ReadUInt32();
            uint offset = reader.ReadUInt32();

            //labels
            reader.JumpTo(position + offset);
            for (int j = 0; j < labelCount; ++j)
            {
                byte length = reader.ReadByte();
                labelValues.Add(reader.ReadString(length));
                indices.Add(reader.ReadUInt32());
            }
        }

        labels = new string[indices.Count];
        for (int i = 0; i < indices.Count; ++i) labels[indices[i]] = labelValues[i];
        groups = (int) entryCount;
    }

    //parse ATR1 type sections
    private static void ParseAtr1(FileReader reader, Encoding encoding, out byte[][] attributes, out string[] attributeTexts, out byte[] additionalData)
    {
        reader.Skip(-4);
        uint sectionSize = reader.ReadUInt32();
        reader.Skip(8);
        long startPos = reader.Position;
        uint entryCount = reader.ReadUInt32();
        uint attributeSize = reader.ReadUInt32();

        uint attributeLength = entryCount * attributeSize + 8;
        bool hasText = attributeSize == 4 && sectionSize >= attributeLength + entryCount * encoding.GetMinByteCount();

        attributes = new byte[entryCount][];
        attributeTexts = new string[hasText ? entryCount : 0];

        for (int i = 0; i < entryCount; ++i)
        {
            attributes[i] = reader.ReadBytes((int) attributeSize);

            if (!hasText) continue;
            reader.Skip(-4);
            uint offset = reader.ReadUInt32();
            if (offset > sectionSize || offset < attributeLength)
            {
                hasText = false;
                continue;
            }

            long pos = reader.Position;
            attributeTexts[i] = reader.ReadTerminatedStringAt(startPos + offset, encoding);
            reader.JumpTo(pos);
        }

        if (!hasText) attributeTexts = [];

        long sectionDiff = startPos + sectionSize - reader.Position;
        additionalData = !hasText && sectionDiff > 0 ? reader.ReadBytes((int) sectionDiff) : [];
    }

    //parse TXT2 type sections (message content)
    private static void ParseTxt2(FileReader reader, Encoding encoding, long sectionSize, out string[] content, out IList<MsbtFunction>[] functions)
    {
        if (reader.BigEndian && encoding.Equals(Encoding.BigEndianUnicode)) encoding = Encoding.Unicode; //we already read the array reversed

        reader.Skip(8);
        long position = reader.Position;
        uint entryCount = reader.ReadUInt32();

        uint[] offsets = ReadArray(reader, (int) entryCount);
        content = new string[entryCount];
        functions = new IList<MsbtFunction>[entryCount];

        for (int i = 0; i < entryCount; ++i)
        {
            //Get the start and end position
            long startPos = offsets[i] + position;
            long endPos = i + 1 < entryCount ? position + offsets[i + 1] : position + sectionSize;

            //parse message text
            reader.JumpTo(startPos);
            StringBuilder message = new();

            //check bytes for function calls
            List<MsbtFunction> messageFunctions = [];
            while (reader.Position < endPos)
            {
                short nextChar = reader.ReadInt16();

                switch (nextChar)
                {
                    case 0x0E: //start of function call
                        message.Append("{{").Append(messageFunctions.Count).Append("}}");
                        messageFunctions.Add(new MsbtFunction
                        {
                            Group = reader.ReadUInt16(),
                            Type = reader.ReadUInt16(),
                            Args = reader.ReadBytes(reader.ReadUInt16())
                        });
                        break;
                    case 0x0F: //closing function/tag
                        message.Append("{{").Append(messageFunctions.Count).Append("}}");
                        messageFunctions.Add(new MsbtFunction
                        {
                            Group = 0x0F,
                            Type = 0x00,
                            Args = reader.ReadBytes(4)
                        });
                        break;
                    case 0x00: //we don't like those
                        break;
                    default: //message content
                        message.Append(encoding.GetString(BitConverter.GetBytes(nextChar)));
                        break;
                }
            }

            content[i] = message.ToString();
            functions[i] = messageFunctions;
        }
    }

    //read a uint array
    private static uint[] ReadArray(FileReader reader, int count)
    {
        uint[] result = new uint[count];
        for (int i = 0; i < result.Length; ++i) result[i] = reader.ReadUInt32();
        return result;
    }
    #endregion
}
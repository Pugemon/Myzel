using System.Text;
using Myzel.Lib.FileFormats.Msbt;
using Myzel.Lib.Utils;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Lib.FileFormats.Bmg;

/// <summary>
/// A class for parsing BMG files.
/// </summary>
public class BmgFileParser : IFileParser<BmgFile>
{
    #region private members
    private readonly string? _language;
    #endregion

    #region constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="BmgFileParser"/> class without a language.
    /// </summary>
    public BmgFileParser() : this(null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BmgFileParser"/> class with the given language.
    /// Each parsed <see cref="MsbtFile"/> object will have the given language assigned once parsed.
    /// </summary>
    /// <param name="language">The language to use.</param>
    public BmgFileParser(string? language) => _language = language;
    #endregion

    #region IFileParser interface
    /// <inheritdoc/>
    public bool CanParse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        return CanParse(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public BmgFile Parse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        FileReader reader = new(fileStream);
        if (!CanParse(reader)) throw new InvalidDataException("File is not a BMG file.");

        //parse file metadata and header
        GetMetaData(reader, out uint sectionCount, out _, out Encoding encoding);

        //parse messages
        BmgFile bmgFile = new()
        {
            BigEndian = reader.BigEndian,
            Encoding = encoding,
            Language = _language
        };
        uint[] labels = Array.Empty<uint>();
        (uint, byte[])[] messageInfo = Array.Empty<(uint, byte[])>();
        string[] content = Array.Empty<string>();
        List<MsbtFunction>[] functions = Array.Empty<List<MsbtFunction>>();

        long sectionOffset = 0x20;
        for (int i = 0; i < sectionCount; ++i)
        {
            reader.JumpTo(sectionOffset);
            reader.Align(32);

            string type = reader.ReadString(4, Encoding.ASCII);
            uint sectionSize = reader.ReadUInt32();
            sectionOffset += sectionSize;

            switch (type)
            {
                case "INF1":
                    ParseInf1(reader, out messageInfo, out var fileId, out var defaultColor);
                    bmgFile.FileId = fileId;
                    bmgFile.DefaultColor = defaultColor;
                    break;
                case "DAT1":
                    ParseDat1(reader, sectionSize, messageInfo, encoding, out content, out functions);
                    break;
                case "MID1":
                    ParseMid1(reader, out labels, out var midFormat);
                    bmgFile.HasMid1 = true;
                    bmgFile.Mid1Format = midFormat;
                    break;
            }
        }

        //compile messages
        string labelFormat = "D" + (content.Length - 1).ToString().Length;
        for (int i = 0; i < content.Length; ++i)
        {
            MsbtMessage message = new MsbtMessage
            {
                Label = labels.Length > 0 ? labels[i].ToString() : i.ToString(labelFormat),
                Attribute = messageInfo[i].Item2,
                Text = content[i],
                Functions = functions[i]
            };

            bmgFile.Messages.Add(message);
        }

        return bmgFile;
    }
    #endregion

    #region private methods
    //verifies that the file is a BMG file
    private static bool CanParse(FileReader reader) => reader.ReadStringAt(0, 8) == "MESGbmg1";

    //parses meta data
    private static void GetMetaData(FileReader reader, out uint sectionCount, out uint fileSize, out Encoding encoding)
    {
        fileSize = reader.ReadUInt32At(8);
        if (fileSize != reader.BaseStream.Length) //sanity check -> if size is invalid -> file uses BE
        {
            reader.BigEndian = true;
            fileSize = reader.ReadUInt32At(8);
        }

        sectionCount = reader.ReadUInt32At(12);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        encoding = reader.ReadByteAt(16) switch
        {
            0 => Encoding.GetEncoding(1252),
            1 => Encoding.GetEncoding(1252),
            2 => reader.BigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode,
            3 => Encoding.GetEncoding("Shift-JIS"),
            4 => Encoding.UTF8,
            _ => reader.BigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode
        };
    }

    //parse INF1 type sections (message info)
    private static void ParseInf1(FileReader reader, out (uint, byte[])[] messageInfo, out ushort fileId, out byte defaultColor)
    {
        ushort entryCount = reader.ReadUInt16();
        ushort entrySize = reader.ReadUInt16();
        fileId = reader.ReadUInt16();
        defaultColor = reader.ReadByte();
        reader.Skip(1);

        messageInfo = new (uint, byte[])[entryCount];

        for (int i = 0; i < entryCount; ++i)
        {
            uint offset = reader.ReadUInt32();
            byte[] data = reader.ReadBytes(entrySize - 4);
            messageInfo[i] = (offset, data);
        }
    }

    //parse DAT1 type sections (message content)
    private static void ParseDat1(FileReader reader, uint sectionSize, IList<(uint, byte[])> messageInfo, Encoding encoding, out string[] content, out List<MsbtFunction>[] functions)
    {
        content = new string[messageInfo.Count];
        functions = new List<MsbtFunction>[messageInfo.Count];

        long sectionStart = reader.Position;
        long sectionEnd = reader.Position + sectionSize - 1;
        TextDecoder decoder = encoding.GetMinByteCount() == 1 ? ParseSingleByte : ParseDoubleByte;

        for (int i = 0; i < messageInfo.Count; ++i)
        {
            uint messageStart = messageInfo[i].Item1;
            long messageEnd = i + 1 < messageInfo.Count ? messageInfo[i + 1].Item1 : sectionEnd;

            reader.JumpTo(sectionStart + messageStart);

            decoder(reader, encoding, sectionStart + messageEnd, out var message, out var messageFunctions);

            content[i] = message;
            functions[i] = messageFunctions;
        }
    }

    //delegate for text parsing
    private delegate void TextDecoder(FileReader reader, Encoding encoding, long messageEnd, out string content, out List<MsbtFunction> functions);

    //parse text section with single-byte encoding
    private static void ParseSingleByte(FileReader reader, Encoding encoding, long messageEnd, out string content, out List<MsbtFunction> functions)
    {
        StringBuilder message = new();
        functions = [];

        long firstCharIndex = reader.Position;
        while (reader.Position < messageEnd)
        {
            byte nextChar = reader.ReadByte();

            if (nextChar == 0x00) //end of message
            {
                message.Append(reader.ReadStringAt(firstCharIndex, (int) (reader.Position - firstCharIndex - 1), encoding));
                break;
            }

            if (nextChar != 0x1A) continue; //start of function call
            if (firstCharIndex + 1 < reader.Position)
            {
                message.Append(reader.ReadStringAt(firstCharIndex, (int) (reader.Position - firstCharIndex - 1), encoding));
                reader.Skip(1);
            }
            message.Append("{{").Append(functions.Count).Append("}}");

            int argLength = reader.ReadByte() - 5;
            functions.Add(new MsbtFunction
            {
                Group = reader.ReadByte(),
                Type = reader.ReadUInt16(),
                Args = reader.ReadBytes(argLength)
            });

            firstCharIndex = reader.Position;
        }

        content = message.ToString();
    }

    //parse text section with double-byte encoding
    private static void ParseDoubleByte(FileReader reader, Encoding encoding, long messageEnd, out string content, out List<MsbtFunction> functions)
    {
        StringBuilder message = new();
        functions = [];

        long firstCharIndex = reader.Position;
        while (reader.Position < messageEnd)
        {
            ushort nextChar = reader.ReadUInt16();

            if (nextChar == 0x00) //end of message
            {
                message.Append(reader.ReadStringAt(firstCharIndex, (int) (reader.Position - firstCharIndex - 2), encoding));
                break;
            }

            if (nextChar is not (0x1A00 or 0x001A)) continue; //start of function call
            if (firstCharIndex + 2 < reader.Position)
            {
                message.Append(reader.ReadStringAt(firstCharIndex, (int) (reader.Position - firstCharIndex - 2), encoding));
                reader.Skip(2);
            }
            message.Append("{{").Append(functions.Count).Append("}}");

            int argLength = reader.ReadByte() - 6;
            functions.Add(new MsbtFunction
            {
                Group = reader.ReadByte(),
                Type = reader.ReadUInt16(),
                Args = reader.ReadBytes(argLength)
            });

            firstCharIndex = reader.Position;
        }

        content = message.ToString();
    }

    //parse MID1 type sections (message IDs)
    private static void ParseMid1(FileReader reader, out uint[] labels, out byte[] midFormat)
    {
        ushort entryCount = reader.ReadUInt16();
        midFormat = reader.ReadBytes(2);
        reader.Skip(4);

        labels = new uint[entryCount];

        for (int i = 0; i < entryCount; i++)
        {
            labels[i] = reader.ReadUInt32();
        }
    }
    #endregion
}
using System.Text;
using Myzel.Lib.Utils;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Lib.FileFormats.Sarc;

/// <summary>
/// A class for parsing SARC archives.
/// </summary>
public class SarcFileParser : IFileParser<SarcFile>
{
    #region IFileParser interface
    /// <inheritdoc/>
    public bool CanParse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        return CanParse(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public SarcFile Parse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        FileReader reader = new FileReader(fileStream);
        if (!CanParse(reader)) throw new InvalidDataException("File is not a SARC file.");

        //parse file metadata and header
        GetMetaData(reader, out _, out int version, out int sfatOffset, out uint dataOffset);

        SarcFile sarcFile = new SarcFile
        {
            BigEndian = reader.BigEndian,
            Version = version
        };

        //parse files
        Dictionary<SarcContent, int> files = new();
        if (reader.ReadStringAt(sfatOffset, 4, Encoding.ASCII) == "SFAT")
        {
            ushort headerLength = reader.ReadUInt16();
            ushort fileCount = reader.ReadUInt16();
            sarcFile.HashKey = reader.ReadInt32();
            reader.JumpTo(sfatOffset + headerLength);

            for (int i = 0; i < fileCount; ++i)
            {
                reader.JumpTo(sfatOffset + headerLength + i * 16);

                byte[] fileNameHash = reader.ReadBytes(4);
                int nameOffset = reader.ReadUInt16() * 4;
                reader.Skip(1);
                if (reader.ReadByte() != 0x01) nameOffset = -1;
                int fileOffset = reader.ReadInt32();
                int endOfFile = reader.ReadInt32();

                SarcContent file = new()
                {
                    Name = fileNameHash.ToHexString(true),
                    Data = reader.ReadBytesAt(dataOffset + fileOffset, endOfFile - fileOffset)
                };

                files.Add(file, nameOffset);
            }

            reader.JumpTo(sfatOffset + headerLength + fileCount * 16);
        }

        //parse file names
        if (reader.ReadString(4, Encoding.ASCII) == "SFNT")
        {
            ushort headerLength = reader.ReadUInt16();
            long nameOffset = reader.Position + headerLength - 6;

            foreach ((SarcContent file, int offset) in files)
            {
                if (offset < 0) continue;
                file.Name = reader.ReadTerminatedStringAt(nameOffset + offset);
            }
        }

        sarcFile.Files = files.Keys.ToList();
        return sarcFile;
    }
    #endregion

    #region private methods
    //verifies that the file is a SARC archive
    private static bool CanParse(FileReader reader)
    {
        if (reader.ReadUInt16At(6) == 65534) reader.BigEndian = true;
        return reader.ReadStringAt(0, 4) == "SARC";
    }

    //parses meta data
    private static void GetMetaData(FileReader reader, out uint fileSize, out int version, out int sfatOffset, out uint dataOffset)
    {
        byte byteOrder = reader.ReadByteAt(0x06);
        if (byteOrder == 0xFE) reader.BigEndian = true;

        sfatOffset = reader.ReadUInt16At(0x04);
        fileSize = reader.ReadUInt32At(0x08);
        dataOffset = reader.ReadUInt32();
        version = reader.ReadUInt16();
    }
    #endregion
}
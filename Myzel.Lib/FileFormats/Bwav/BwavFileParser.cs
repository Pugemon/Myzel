using Myzel.Lib.Utils;

namespace Myzel.Lib.FileFormats.Bwav;

/// <summary>
/// A class for parsing BWAV files.
/// </summary>
public class BwavFileParser : IFileParser<BwavFile>
{
    #region IFileParser interface
    /// <inheritdoc/>
    public bool CanParse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        return CanParse(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public BwavFile Parse(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        FileReader reader = new(fileStream);
        if (!CanParse(reader)) throw new InvalidDataException("File is not a BWAV file.");

        //read header
        ReadHeader(reader, out string version, out string hash, out bool prefetch, out int channelCount);

        BwavFile bwavFile = new BwavFile
        {
            Version = version,
            Hash = hash,
            Prefetch = prefetch,
            Channels = new ChannelData[channelCount]
        };

        long position = reader.Position;
        for (int i = 0; i < channelCount; ++i)
        {
            bwavFile.Channels[i] = ReadChannelData(reader, position + i * 0x4C);
        }

        return bwavFile;
    }
    #endregion

    #region private methods
    //verifies that the file is a BWAV file
    private static bool CanParse(FileReader reader) => reader.ReadStringAt(0, 4) == "BWAV";

    //parses header
    private static void ReadHeader(FileReader reader, out string version, out string hash, out bool prefetch, out int channelCount)
    {
        reader.BigEndian = reader.ReadUInt16At(4) == 0xfffe;

        version = $"{reader.ReadByte()}.{reader.ReadByte()}";

        hash = reader.ReadHexString(4);

        prefetch = reader.ReadUInt16() == 1;

        channelCount = reader.ReadUInt16();
    }

    //parse channel data
    private static ChannelData ReadChannelData(FileReader reader, long position)
    {
        ChannelData data = new()
        {
            Pan = (ChannelPan) reader.ReadUInt16At(position + 0x02),
            SampleRate = reader.ReadUInt32(),
            Samples = reader.ReadUInt32At(position + 0x0C),
            Loop = reader.ReadUInt32At(position + 0x038) == 1,
            LoopEnd = reader.ReadUInt32(),
            LoopStart = reader.ReadUInt32()
        };

        /*
            0x02 	u16 	Channel Pan. 0 for left, 1 for right, 2 for middle
            0x04 	u32 	Sample Rate
            0x08 	u32 	Number of samples in non-prefetch file
            0x0C 	u32 	Number of samples in this file
            0x10 	s16[8][2] 	DSP-ADPCM Coefficients
            0x30 	u32 	Absolute start offset of the sample data in non-prefetch file
            0x34 	u32 	Absolute start offset of the sample data in this file
            0x38 	u32 	Is 1 if the channel loops
            0x3C 	u32 	Loop End Sample (0xFFFFFFFF if doesn’t loop)
            0x40 	u32 	Loop Start Sample (0 if doesn’t loop)
            0x44 	u16 	Predictor Scale?
            0x46 	u16 	History Sample 1?
            0x48 	u16 	History Sample 2?
            0x4A 	u16 	Padding?
        */

        return data;
    }
    #endregion
}
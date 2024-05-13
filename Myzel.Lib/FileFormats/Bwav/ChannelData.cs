namespace Myzel.Lib.FileFormats.Bwav;

public class ChannelData
{
    public ChannelPan Pan { get; set; }

    public uint SampleRate { get; set; }

    public uint Samples { get; set; }

    public int[] Coefficients { get; set; } = null!;

    public bool Loop { get; set; }

    public uint LoopStart { get; set; }

    public uint LoopEnd { get; set; }

    public byte[] Data { get; set; } = null!;
}
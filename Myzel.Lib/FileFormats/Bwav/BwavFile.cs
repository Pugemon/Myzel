namespace Myzel.Lib.FileFormats.Bwav;

public class BwavFile
{
    #region public properties
    public string Version { get; set; } = null!;

    public string Hash { get; set; } = null!;

    public bool Prefetch { get; set; }

    public ChannelData[] Channels { get; init; } = null!;
    #endregion
}
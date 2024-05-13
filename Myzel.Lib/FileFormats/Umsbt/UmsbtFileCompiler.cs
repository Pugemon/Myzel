using Myzel.Lib.FileFormats.Msbt;
using Myzel.Lib.Utils;

namespace Myzel.Lib.FileFormats.Umsbt;

/// <summary>
/// A class for compiling UMSBT files.
/// </summary>
public class UmsbtFileCompiler : IFileCompiler<IList<MsbtFile>>
{
    #region private members
    private static readonly MsbtFileCompiler MsbtCompiler = new();
    #endregion

    #region IFileCompiler interface
    /// <inheritdoc/>
    public void Compile(IList<MsbtFile> files, Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(fileStream);

        FileWriter writer = new(fileStream, true);
        writer.Pad(files.Count * 8);
        writer.Align(16);
        writer.Pad(16);

        for (int i = 0; i < files.Count; ++i)
        {
            using MemoryStream subStream = new();
            MsbtCompiler.Compile(files[i], subStream);

            long startPos = writer.Position;
            long size = subStream.Length;

            subStream.Position = 0;
            subStream.CopyTo(writer.BaseStream);
            long endPos = writer.Position;

            writer.JumpTo(i * 8);
            writer.Write((uint) startPos);
            writer.Write((uint) size);
            writer.JumpTo(endPos);
        }
    }
    #endregion
}
using System.Globalization;
using System.Text;
using Myzel.Lib.Utils;

namespace Myzel.Lib.FileFormats.Sarc;

/// <summary>
/// A class for compiling SARC files.
/// </summary>
public class SarcFileCompiler : IFileCompiler<SarcFile>
{
    #region IFileCompiler interface
    /// <inheritdoc/>
    public void Compile(SarcFile file, Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(fileStream);

        var writer = new FileWriter(fileStream, true);
        writer.BigEndian = file.BigEndian;

        //write header
        writer.Write("SARC", Encoding.ASCII);
        writer.Write((ushort) 0x14);
        writer.Write((ushort) 0xFEFF);
        writer.Pad(8);
        writer.Write((ushort) file.Version);
        writer.Pad(2);

        //write SFAT header
        writer.Write("SFAT", Encoding.ASCII);
        writer.Write((ushort) 0x0C);
        writer.Write((ushort) file.Files.Count);
        writer.Write(BitConverter.GetBytes(file.HashKey));

        //build hash map
        Dictionary<SarcContent, uint> hashMap = new();
        foreach (SarcContent content in file.Files)
        {
            byte[] nameHash;
            if (content.Name.StartsWith("0x") && !content.Name.Contains('.'))
            {
                nameHash = new byte[(content.Name.Length - 2) / 2];
                for (int i = 0; i < nameHash.Length; ++i)
                {
                    nameHash[i] = byte.Parse(content.Name.AsSpan(2 + i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
            }
            else
            {
                nameHash = BitConverter.GetBytes(GetNameHash(content.Name, file.HashKey));
            }

            hashMap.Add(content, BitConverter.ToUInt32(nameHash));
        }
        KeyValuePair<SarcContent, uint>[] sortedFiles = hashMap.OrderBy(i => i.Value).ToArray();

        //write SFAT nodes
        int currentNameOffset = 0;
        int currentDataOffset = 0;
        bool hasNames = false;
        foreach ((SarcContent content, uint nameHash) in sortedFiles)
        {
            bool hasName = !content.Name.StartsWith("0x") || content.Name.Contains('.');
            if (hasName) hasNames = true;

            writer.Write(BitConverter.GetBytes(nameHash));
            if (hasName)
            {
                writer.Write((ushort) (currentNameOffset / 4));
                writer.Write([0x00, 0x01]);
                currentNameOffset += content.Name.Length + 1;
                currentNameOffset += GetAlignmentOffset(currentNameOffset, 4);
            }
            else writer.Pad(4);
            writer.Write(currentDataOffset);
            writer.Write(currentDataOffset + content.Data.Length);
            currentDataOffset += content.Data.Length;
            currentDataOffset += GetAlignmentOffset(currentDataOffset, 8);
        }

        //write SFNT header
        if (hasNames)
        {
            writer.Write("SFNT", Encoding.ASCII);
            writer.Write((ushort) 0x08);
            writer.Pad(2);

            foreach ((SarcContent content, uint _) in sortedFiles)
            {
                if (content.Name.StartsWith("0x") && !content.Name.Contains('.')) continue;

                writer.Write(content.Name);
                writer.Write((byte) 0x00);
                writer.Align(4);
            }
        }

        //write data
        writer.Align(8);
        long dataOffset = writer.Position;
        for (int i = 0; i < sortedFiles.Length; ++i)
        {
            writer.Write(sortedFiles[i].Key.Data);
            if (i + 1 < sortedFiles.Length) writer.Align(8);
        }

        //write file size and data offset
        long fileSize = writer.Position;
        writer.JumpTo(0x08);
        writer.Write((uint) fileSize);
        writer.Write((uint) dataOffset);
    }
    #endregion

    #region private methods
    private static int GetNameHash(string name, int hashKey)
    {
        return name.Aggregate(0, (current, c) => current * hashKey + c);
    }

    private static int GetAlignmentOffset(int position, int alignment) => (-position % alignment + alignment) % alignment;
    #endregion
}
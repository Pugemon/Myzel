using System.Text;
using Myzel.Lib.Utils;

namespace Myzel.Lib.Compression.Yaz0;

/// <summary>
/// A class for Yaz0 decompression.
/// </summary>
public class Decompressor : IDecompressor
{

    #region IDecompressor interface

    /// <inheritdoc/>
    public bool CanDecompress(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        return CanDecompress(new FileReader(fileStream, true));
    }

    /// <inheritdoc/>
    public Stream Decompress(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        FileReader reader = new(fileStream);
        if (!CanDecompress(reader)) throw new InvalidDataException("Data is not Yaz0 compressed.");

        reader.BigEndian = true;
        uint size = reader.ReadUInt32At(4);
        reader.JumpTo(16);

        byte[] result = new byte[size];
        int index = 0;
        byte codeByte = 0;
        int codeBitsLeft = 0;
        while (index < size)
        {
            //get next code byte if required
            if (codeBitsLeft == 0)
            {
                codeByte = reader.ReadByte();
                codeBitsLeft = 8;
            }

            if ((codeByte & 0x80) != 0) //direct copy
            {
                result[index++] = reader.ReadByte();
            }
            else //RLE encoded
            {
                byte byte1 = reader.ReadByte();
                byte byte2 = reader.ReadByte();

                int count = byte1 >> 4;
                if (count == 0) count = reader.ReadByte() + 0x12;
                else count += 2;

                int copyIndex = index - ((byte1 & 0xF) << 8 | byte2) - 1;
                for (int i = 0; i < count; ++i)
                {
                    result[index++] = result[copyIndex++];
                }
            }

            codeByte <<= 1;
            --codeBitsLeft;
        }

        return new MemoryStream(result);
    }

    #endregion

    #region private methods

    private static bool CanDecompress(FileReader reader)
    {
        return reader.ReadStringAt(0, 4, Encoding.ASCII) == "Yaz0";
    }

    #endregion

}
using System.Text;
using Myzel.Lib.Utils;

namespace Myzel.Lib.Compression.LZ77;

/// <summary>
/// A class for LZ77 decompression.
/// </summary>
public class Decompressor : IDecompressor
{
    #region Constants

    private const int RingBufferSize = 4096;
    private const int FrameSize = 18;
    private const int Threshold = 2;
    private const byte SingleChunkIndicator = 0x10;
    private const byte MultipleChunksIndicator = 0xf7;
        
    #endregion

    #region IDecompressor Interface

    /// <inheritdoc/>
    public bool CanDecompress(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        using FileReader reader = new FileReader(fileStream, true);
        return CanDecompress(reader);
    }

    /// <inheritdoc/>
    public Stream Decompress(Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        if (!CanDecompress(fileStream)) throw new InvalidDataException("Data is not LZ77 compressed.");
        using FileReader reader = new FileReader(fileStream);
        
        byte compressionType = reader.ReadByte();
        List<int> chunks = [0];
        ushort decompressedSize = reader.ReadUInt16();
        uint bufferSize = decompressedSize;
        uint flags = 8u;
        uint bitmap = 8u;

        switch (compressionType)
        {
            // Read chunk data
            // Single chunk
            case SingleChunkIndicator:
                chunks.Add((int)reader.BaseStream.Length - 4);
                reader.Skip(-1);
                break;
            // Multiple chunks
            case MultipleChunksIndicator:
            {
                flags = reader.ReadByte();
                bitmap = 0;

                int chunkEndPos;
                do
                {
                    chunkEndPos = reader.ReadUInt16();
                    chunks.Add(chunkEndPos);
                } while (chunkEndPos + reader.Position < reader.BaseStream.Length);

                if (reader.ReadByte() != SingleChunkIndicator)
                    throw new InvalidDataException("Unsupported compression type.");

                bufferSize = reader.ReadUInt32();
                break;
            }
            default:
                throw new InvalidDataException($"Invalid chunk info byte. Expected 0x{SingleChunkIndicator:x2} or 0x{MultipleChunksIndicator:x2} but got 0x{compressionType:x2}.");
        }

        // Read data
        long dataStartPos = reader.Position;
        byte[] outputData = new byte[decompressedSize];
        int outputIndex = 0;
        for (int i = 0; i < chunks.Count - 1; ++i)
        {
            reader.JumpTo(dataStartPos + chunks[i]);
            long chunkEndPos = dataStartPos + chunks[i + 1];
            int chunkSize = 0;

            byte[] textBuffer = new byte[RingBufferSize + FrameSize - 1];
            for (int j = 0; j < RingBufferSize - FrameSize; ++j)
                textBuffer[j] = 0xdf;
            int bufferIndex = RingBufferSize - FrameSize;

            if (i > 0)
            {
                flags = 8u;
                bitmap = 8u;
            }

            while (outputIndex < decompressedSize)
            {
                byte value;

                if (bitmap == 8)
                {
                    if (!TryReadByte(reader, out value))
                        break;

                    flags = value;
                    bitmap = 0;
                }

                if ((flags & 0x80) == 0)
                {
                    if (!TryReadByte(reader, out value) || value == reader.BaseStream.Length - 1)
                        break;

                    if (reader.Position < chunkEndPos && chunkSize < bufferSize && chunkSize < decompressedSize)
                        outputData[outputIndex++] = value;

                    textBuffer[bufferIndex++] = value;
                    bufferIndex &= RingBufferSize - 1;
                    ++chunkSize;
                }
                else
                {
                    int val1, val2;
                    if (TryReadByte(reader, out byte byte1))
                        val1 = byte1;
                    else
                        break;
                    if (TryReadByte(reader, out byte byte2))
                        val2 = byte2;
                    else
                        break;

                    val2 |= val1 << 8 & 0x0f00;
                    val1 = (val1 >> 4 & 0x0f) + Threshold;

                    for (int j = 0; j <= val1; ++j)
                    {
                        value = textBuffer[bufferIndex - val2 - 1 & RingBufferSize - 1];

                        if (reader.Position < chunkEndPos && chunkSize < bufferSize && chunkSize < decompressedSize)
                            outputData[outputIndex++] = value;

                        textBuffer[bufferIndex++] = value;
                        bufferIndex &= RingBufferSize - 1;
                        ++chunkSize;
                    }
                }

                flags <<= 1;
                ++bitmap;
            }
        }

        return new MemoryStream(outputData);
    }

    #endregion

    #region Private Methods

    private static bool CanDecompress(FileReader reader) =>
        reader.ReadStringAt(0, 4, Encoding.ASCII) == "LZ77";

    private static bool TryReadByte(FileReader reader, out byte value)
    {
        value = 0;
        if (reader.Position + 1 >= reader.BaseStream.Length)
            return false;

        value = reader.ReadByte();
        return true;
    }

    #endregion
}
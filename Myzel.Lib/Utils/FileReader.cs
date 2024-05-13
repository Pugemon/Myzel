using System.Text;
using Myzel.Lib.Utils.Extensions;

namespace Myzel.Lib.Utils;

/// <summary>
/// Provides methods for reading binary data from a stream.
/// </summary>
internal sealed class FileReader : IDisposable
{

    #region private members

    private readonly BinaryReader _reader;
    private bool _disposed;

    #endregion


    #region constructor

    /// <summary>
    /// Initializes a new instance of the FileReader class with the specified stream.
    /// </summary>
    /// <param name="fileStream">The stream to read from.</param>
    /// <param name="leaveOpen">True to leave the stream open after the FileReader is disposed; otherwise, false.</param>
    public FileReader(Stream fileStream, bool leaveOpen = false)
    {
        _reader = new BinaryReader(fileStream, Encoding.UTF8, leaveOpen);
        Position = 0;
    }

    #endregion

    #region public properties

    /// <summary>
    /// Gets or sets a value indicating whether to use big endian byte order when reading multi-byte values.
    /// </summary>
    public bool BigEndian { get; set; }

    /// <summary>
    /// Gets or sets the current position within the stream.
    /// </summary>
    public long Position
    {
        get => _reader.BaseStream.Position;
        set => _reader.BaseStream.Position = value;
    }

    /// <summary>
    /// Gets the underlying stream.
    /// </summary>
    public Stream BaseStream => _reader.BaseStream;

    #endregion

    #region public methods

    /// <summary>
    /// Skips a specified number of bytes forward or backward in the stream.
    /// </summary>
    /// <param name="count">The number of bytes to skip. A negative value moves the position backwards.</param>
    public void Skip(int count)
    {
        Position += count;
    }

    /// <summary>
    /// Jumps to a specified position in the stream.
    /// </summary>
    /// <param name="position">The position to jump to.</param>
    public void JumpTo(long position)
    {
        Position = position;
    }

    /// <summary>
    /// Aligns the stream to the next multiple of the specified alignment.
    /// </summary>
    /// <param name="alignment">The alignment value.</param>
    public void Align(int alignment)
    {
        _reader.BaseStream.Seek((-Position % alignment + alignment) % alignment, SeekOrigin.Current);
    }

    /// <summary>
    /// Reads an array of bytes from the stream.
    /// </summary>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>The bytes read from the stream.</returns>
    public byte[] ReadBytes(int length)
    {
        return ReadBytes(length, length);
    }

    /// <summary>
    /// Reads an array of bytes from the stream at a specified position.
    /// </summary>
    /// <param name="position">The position to start reading from.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>The bytes read from the stream.</returns>
    public byte[] ReadBytesAt(long position, int length)
    {
        Position = position;
        return ReadBytes(length);
    }

    //reads a value from stream as sbyte
    public sbyte ReadSByte(int length = 1)
    {
        byte[] bytes = ReadBytes(length, 1);
        return (sbyte)bytes[0];
    }
    public sbyte ReadSByteAt(long position, int length = 1)
    {
        Position = position;
        return ReadSByte(length);
    }

    //reads a value from stream as byte
    public byte ReadByte(int length = 1)
    {
        byte[] bytes = ReadBytes(length, 1);
        return bytes[0];
    }
    public byte ReadByteAt(long position, int length = 1)
    {
        Position = position;
        return ReadByte(length);
    }

    //reads a value from stream as short
    public short ReadInt16(int length = 2)
    {
        byte[] bytes = ReadBytes(length, 2, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToInt16(bytes, 0);
    }
    public short ReadInt16At(long position, int length = 2)
    {
        Position = position;
        return ReadInt16(length);
    }

    //reads a value from stream as ushort
    public ushort ReadUInt16(int length = 2)
    {
        byte[] bytes = ReadBytes(length, 2, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToUInt16(bytes, 0);
    }
    public ushort ReadUInt16At(long position, int length = 2)
    {
        Position = position;
        return ReadUInt16(length);
    }

    //reads a value from stream as int
    public int ReadInt32(int length = 4)
    {
        byte[] bytes = ReadBytes(length, 4, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToInt32(bytes, 0);
    }
    public int ReadInt32At(long position, int length = 4)
    {
        Position = position;
        return ReadInt32(length);
    }

    //reads a value from stream as uint
    public uint ReadUInt32(int length = 4)
    {
        byte[] bytes = ReadBytes(length, 4, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToUInt32(bytes, 0);
    }
    public uint ReadUInt32At(long position, int length = 4)
    {
        Position = position;
        return ReadUInt32(length);
    }

    //reads a value from stream as long
    public long ReadInt64(int length = 8)
    {
        byte[] bytes = ReadBytes(length, 8, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToInt64(bytes, 0);
    }
    public long ReadInt64At(long position, int length = 8)
    {
        Position = position;
        return ReadInt64(length);
    }

    //reads a value from stream as ulong
    public ulong ReadUInt64(int length = 8)
    {
        byte[] bytes = ReadBytes(length, 8, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToUInt64(bytes, 0);
    }
    public ulong ReadUInt64At(long position, int length = 8)
    {
        Position = position;
        return ReadUInt64(length);
    }

    //reads a value from stream as float
    public float ReadSingle(int length = 4)
    {
        byte[] bytes = ReadBytes(length, 4, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToSingle(bytes, 0);
    }
    public float ReadSingleAt(long position, int length = 4)
    {
        Position = position;
        return ReadSingle(length);
    }

    //reads a value from stream as double
    public double ReadDouble(int length = 8)
    {
        byte[] bytes = ReadBytes(length, 8, BigEndian == BitConverter.IsLittleEndian);
        return BitConverter.ToDouble(bytes, 0);
    }
    public double ReadDoubleAt(long position, int length = 8)
    {
        Position = position;
        return ReadDouble(length);
    }

    //reads a value from stream as hex string
    public string ReadHexString(int length)
    {
        byte[] bytes = ReadBytes(length, length % 2 + length, !BigEndian);
        return BitConverter.ToString(bytes).Replace("-", "");
    }
    public string ReadHexStringAt(long position, int length)
    {
        Position = position;
        return ReadHexString(length);
    }
    //reads a value from stream as utf8 string
    public string ReadString(int length)
    {
        return ReadString(length, Encoding.UTF8);
    }
    public string ReadStringAt(long position, int length)
    {
        return ReadStringAt(position, length, Encoding.UTF8);
    }

    //reads a value from stream as string with a specific encoding
    public string ReadString(int length, Encoding encoding)
    {
        byte[] bytes = ReadBytes(length, 0);
        return encoding.GetString(bytes).TrimEnd('\0');
    }
    public string ReadStringAt(long position, int length, Encoding encoding)
    {
        Position = position;
        return ReadString(length, encoding);
    }

    //reads a value from stream as utf8 string until encountering a null-byte
    public string ReadTerminatedString(int maxLength = -1)
    {
        return ReadTerminatedString(Encoding.UTF8, maxLength);
    }
    public string ReadTerminatedStringAt(long position, int maxLength = -1)
    {
        return ReadTerminatedStringAt(position, Encoding.UTF8, maxLength);
    }

    //reads a value from stream as string with a specific encoding until encountering a null-byte
    public string ReadTerminatedString(Encoding encoding, int maxLength = -1)
    {
        List<byte> bytes = new(maxLength > 0 ? maxLength : 256);
        int nullByteLength = encoding.GetMinByteCount();

        int nullCount = 0;
        do
        {
            byte value = _reader.ReadByte();
            nullCount = value == 0x00 ? nullCount + 1 : 0;
            bytes.Add(value);
        } while (bytes.Count != maxLength && nullCount < nullByteLength);

        //return whatever we have
        if (bytes.Count == maxLength) return encoding.GetString(bytes.ToArray()).TrimEnd('\0');

        //append enough null bytes to ensure we have a full null-byte to trim
        for (int i = 0; i < nullByteLength - 1; ++i) bytes.Add(0x00);
        return encoding.GetString(bytes.ToArray())[..^1].TrimEnd('\0');
    }
    public string ReadTerminatedStringAt(long position, Encoding encoding, int maxLength = -1)
    {
        Position = position;
        return ReadTerminatedString(encoding, maxLength);
    }

    #endregion

    #region private methods

    /// <summary>
    /// Reads an array of raw bytes from the stream.
    /// </summary>
    /// <param name="length">The number of bytes to read.</param>
    /// <param name="padding">The padding size.</param>
    /// <param name="reversed">True if the bytes should be reversed; otherwise, false.</param>
    /// <returns>The bytes read from the stream.</returns>
    private byte[] ReadBytes(int length, int padding, bool reversed = false)
    {
        if (length <= 0) return [];

        byte[] bytes = new byte[length > padding ? length : padding];
        _ = _reader.Read(bytes, reversed ? bytes.Length - length : 0, length);

        if (reversed) Array.Reverse(bytes);
        return bytes;
    }

    #endregion

    #region IDisposable interface

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _reader.Dispose();
        _disposed = true;
    }

    #endregion

}
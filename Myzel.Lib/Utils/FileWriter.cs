using System.Buffers.Binary;
using System.Text;

namespace Myzel.Lib.Utils;

/// <summary>
/// Provides methods for writing binary data to a stream.
/// </summary>
internal class FileWriter : IDisposable
{

    #region private members

    private readonly BinaryWriter _writer;
    private bool _disposed;

    #endregion

    #region constructor

    /// <summary>
    /// Initializes a new instance of the FileWriter class with the specified stream.
    /// </summary>
    /// <param name="fileStream">The stream to write to.</param>
    /// <param name="leaveOpen">True to leave the stream open after the FileWriter is disposed; otherwise, false.</param>
    public FileWriter(Stream fileStream, bool leaveOpen = false)
    {
        _writer = new BinaryWriter(fileStream, Encoding.UTF8, leaveOpen);
        Position = 0x0;
    }

    #endregion

    #region public properties

    /// <summary>
    /// Gets or sets a value indicating whether to use big endian byte order when writing multi-byte values.
    /// </summary>
    public bool BigEndian { get; set; }

    /// <summary>
    /// Gets or sets the current position within the stream.
    /// </summary>
    public long Position
    {
        get => _writer.BaseStream.Position;
        set => _writer.BaseStream.Position = value;
    }

    /// <summary>
    /// Gets the underlying stream.
    /// </summary>
    public Stream BaseStream => _writer.BaseStream;

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
    /// Appends the specified number of null bytes to the stream.
    /// </summary>
    /// <param name="count">The number of null bytes to append.</param>
    public void Pad(int count)
    {
        _writer.Write(new byte[count]);
    }
    /// <summary>
    /// Appends the specified number of bytes with the specified value to the stream.
    /// </summary>
    /// <param name="count">The number of bytes to append.</param>
    /// <param name="value">The value of each byte.</param>
    public void Pad(int count, byte value)
    {
        byte[] buffer = new byte[count];
        Array.Fill(buffer, value);
        _writer.Write(buffer);
    }

    /// <summary>
    /// Aligns the stream to the next multiple of the specified alignment.
    /// </summary>
    /// <param name="alignment">The alignment value.</param>
    public void Align(int alignment)
    {
        long offset = (-Position % alignment + alignment) % alignment;
        if (offset > 0) _writer.Write(new byte[offset]);
    }
    /// <summary>
    /// Aligns the stream to the next multiple of the specified alignment and fills the gap with the specified value.
    /// </summary>
    /// <param name="alignment">The alignment value.</param>
    /// <param name="value">The value to fill the gap with.</param>
    public void Align(int alignment, byte value)
    {
        long offset = (-Position % alignment + alignment) % alignment;
        if (offset == 0) return;

        byte[] buffer = new byte[offset];
        Array.Fill(buffer, value);
        _writer.Write(buffer);
    }

    /// <summary>
    /// Writes an array of bytes to the stream.
    /// </summary>
    /// <param name="value">The byte array to write.</param>
    public void Write(byte[] value)
    {
        _writer.Write(value);
    }

    //writes a sbyte value to stream
    public void Write(sbyte value)
    {
        _writer.Write(value);
    }

    //writes a byte value to stream
    public void Write(byte value)
    {
        _writer.Write(value);
    }

    //writes a short value to stream
    public void Write(short value)
    {
        byte[] buffer = new byte[sizeof(short)];
        if (BigEndian) BinaryPrimitives.WriteInt16BigEndian(buffer, value);
        else BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
        _writer.Write(buffer);
    }

    //writes an ushort value to stream
    public void Write(ushort value)
    {
        byte[] buffer = new byte[sizeof(ushort)];
        if (BigEndian) BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        else BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
        _writer.Write(buffer);
    }

    //writes an int value to stream
    public void Write(int value)
    {
        byte[] buffer = new byte[sizeof(int)];
        if (BigEndian) BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        else BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
        _writer.Write(buffer);
    }

    //writes an uint value to stream
    public void Write(uint value)
    {
        byte[] buffer = new byte[sizeof(uint)];
        if (BigEndian) BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
        else BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
        _writer.Write(buffer);
    }

    //writes a long value to stream
    public void Write(long value)
    {
        byte[] buffer = new byte[sizeof(long)];
        if (BigEndian) BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        else BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
        _writer.Write(buffer);
    }

    //writes an ulong value to stream
    public void Write(ulong value)
    {
        byte[] buffer = new byte[sizeof(ulong)];
        if (BigEndian) BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
        else BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
        _writer.Write(buffer);
    }

    //writes a float value to stream
    public void Write(float value)
    {
        byte[] buffer = new byte[sizeof(int)];
        int tmpValue = BitConverter.SingleToInt32Bits(value);
        if (BigEndian) BinaryPrimitives.WriteInt32BigEndian(buffer, tmpValue);
        else BinaryPrimitives.WriteInt32LittleEndian(buffer, tmpValue);
        _writer.Write(buffer);
    }

    //writes a double value to stream
    public void Write(double value)
    {
        byte[] buffer = new byte[sizeof(long)];
        long tmpValue = BitConverter.DoubleToInt64Bits(value);
        if (BigEndian) BinaryPrimitives.WriteInt64BigEndian(buffer, tmpValue);
        else BinaryPrimitives.WriteInt64LittleEndian(buffer, tmpValue);
        _writer.Write(buffer);
    }

    /// <summary>
    /// Writes a string to the stream using the UTF-8 encoding.
    /// </summary>
    /// <param name="value">The string to write.</param>
    public void Write(string value)
    {
        Write(value, Encoding.UTF8);
    }

    /// <summary>
    /// Writes a string to the stream using the specified encoding.
    /// </summary>
    /// <param name="value">The string to write.</param>
    /// <param name="encoding">The encoding to use.</param>
    public void Write(string value, Encoding encoding)
    {
        _writer.Write(encoding.GetBytes(value));
    }

    #endregion

    #region IDisposable interface

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _writer.Dispose();
        _disposed = true;
    }

    #endregion

}
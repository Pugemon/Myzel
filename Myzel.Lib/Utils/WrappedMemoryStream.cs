namespace Myzel.Lib.Utils;

/// <summary>
/// Represents a wrapper around the MemoryStream class from the System.IO namespace.
/// Allows working with a memory stream and performing additional actions upon resource disposal.
/// </summary>
public class WrappedMemoryStream : Stream
{
    #region private members
    private readonly MemoryStream _stream;
    private readonly Action<MemoryStream> _onDispose;
    private bool _disposed;
    #endregion

    #region constructors
    /// <summary>
    /// Initializes a new instance of the WrappedMemoryStream class using a default MemoryStream.
    /// </summary>
    /// <param name="onDispose">Delegate invoked upon resource disposal.</param>
    public WrappedMemoryStream(Action<MemoryStream> onDispose)
    {
        _stream = new MemoryStream();
        _onDispose = onDispose;
    }

    /// <summary>
    /// Initializes a new instance of the WrappedMemoryStream class using a MemoryStream based on the provided buffer.
    /// </summary>
    /// <param name="buffer">The buffer used to create the stream.</param>
    /// <param name="onDispose">Delegate invoked upon resource disposal.</param>
    public WrappedMemoryStream(byte[] buffer, Action<MemoryStream> onDispose) : this(buffer, true, onDispose)
    { }

    /// <summary>
    /// Initializes a new instance of the WrappedMemoryStream class using a MemoryStream based on the provided buffer and writeable flag.
    /// </summary>
    /// <param name="buffer">The buffer used to create the stream.</param>
    /// <param name="writeable">A flag indicating whether the stream should be writable.</param>
    /// <param name="onDispose">Delegate invoked upon resource disposal.</param>
    public WrappedMemoryStream(byte[] buffer, bool writeable, Action<MemoryStream> onDispose)
    {
        _stream = new MemoryStream(buffer, writeable);
        _onDispose = onDispose;
    }
    #endregion

    #region public properties
    /// <inheritdoc />
    public override bool CanRead => _stream.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => _stream.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => _stream.CanWrite;

    /// <inheritdoc />
    public override long Length => _stream.Length;

    /// <inheritdoc />
    public override long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }
    #endregion

    #region public methods
    /// <inheritdoc />
    public override void Flush() => _stream.Flush();

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

    /// <inheritdoc />
    public override void SetLength(long value) => _stream.SetLength(value);

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);
    #endregion

    #region protected methods
    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (!disposing || _disposed) return;

        _onDispose.Invoke(_stream);
        _stream.Dispose();
        _disposed = true;
    }
    #endregion
}

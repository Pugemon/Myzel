namespace Myzel.Lib.Utils;

internal class StreamSpan : Stream
{

    #region private members

    private readonly Stream _baseStream;
    private readonly long _offset;

    #endregion

    #region constructor

    public StreamSpan(Stream baseStream, long offset, long length)
    {
        ArgumentNullException.ThrowIfNull(baseStream);
        if (!baseStream.CanRead) throw new ArgumentException("Can't read from base stream.");
        if (offset < 0 || offset >= baseStream.Length) throw new ArgumentOutOfRangeException(nameof(offset));
        if (length < 0 || offset + length > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(length));

        _baseStream = baseStream;
        _baseStream.Position = offset;
        _offset = offset;
        Length = length;
    }

    #endregion

    #region Stream interface

    public override bool CanRead => _baseStream.CanRead;

    public override bool CanSeek => _baseStream.CanSeek;

    public override bool CanTimeout => _baseStream.CanTimeout;

    public override bool CanWrite => false;

    public override long Length { get; }

    public override long Position
    {
        get => _baseStream.Position - _offset;
        set
        {
            if (value < 0 || value >= Length) throw new ArgumentOutOfRangeException(nameof(StreamSpan.Position));
            _baseStream.Position = _offset + value;
        }
    }

    public override int ReadTimeout
    {
        get => _baseStream.ReadTimeout;
        set => _baseStream.ReadTimeout = value;
    }

    public override int WriteTimeout
    {
        get => _baseStream.WriteTimeout;
        set => _baseStream.WriteTimeout = value;
    }

    public override void Flush() => _baseStream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        long newPosition = Position + offset;
        if (newPosition < 0 || newPosition >= Length) return 0;
        if (newPosition + count >= Length) count = (int)(Length - newPosition);

        return _baseStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return origin switch
        {
            SeekOrigin.Begin when offset < 0 || offset >= Length => throw new ArgumentOutOfRangeException(nameof(offset)),
            SeekOrigin.Begin => _baseStream.Seek(_offset + offset, SeekOrigin.Begin),
            SeekOrigin.End when offset < 0 || offset >= Length => throw new ArgumentOutOfRangeException(nameof(offset)),
            SeekOrigin.End => _baseStream.Seek(_baseStream.Length - _offset - Length + offset, SeekOrigin.End),
            _ => Position + offset < 0 || Position + offset >= Length ? throw new ArgumentOutOfRangeException(nameof(offset)) : _baseStream.Seek(offset, origin)
        };
    }

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    #endregion

}
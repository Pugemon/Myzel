namespace Myzel.Lib.Utils.Extensions;

internal static class StreamExtensions
{
    //converts a stream to byte-array
    public static byte[] ToArray(this Stream stream)
    {
        if (stream is MemoryStream castedStream) return castedStream.ToArray();
        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
    
    public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long>? progress = null, CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.CanRead)
            throw new ArgumentException("Has to be readable", nameof(source));
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
            throw new ArgumentException("Has to be writable", nameof(destination));
        ArgumentOutOfRangeException.ThrowIfNegative(bufferSize);

        byte[] buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0) {
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
            totalBytesRead += bytesRead;
            progress?.Report(totalBytesRead);
        }
    }
}
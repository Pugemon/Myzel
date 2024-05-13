namespace Myzel.Lib.Compression.Utils;

/// <summary>
/// Provides methods for working with the LZ77 compression algorithm.
/// </summary>
public static class LzUtil
{
    /// <summary>
    /// Determine the maximum size of a LZ-compressed block starting at newPtr, using the already compressed data
    /// starting at oldPtr. Takes O(inLength * oldLength) = O(n^2) time.
    /// </summary>
    /// <param name="newData">The start of the data that needs to be compressed.</param>
    /// <param name="oldData">The start of the raw file.</param>
    /// <param name="minDisp">The minimum allowed value for 'disp'.</param>
    /// <returns>A tuple containing the length of the longest sequence of bytes that can be copied from the already decompressed data and the offset of the start of the longest block to refer to.</returns>
    public static (int maxMatchLength, int disp) GetLongestMatch(ReadOnlySpan<byte> newData, ReadOnlySpan<byte> oldData, int minDisp = 1)
    {
        if (newData.Length == 0) return (0, 0);

        int maxMatchLength = 0;
        int disp = 0;

        int maxSearchLength = Math.Min(oldData.Length - minDisp, newData.Length);

        for (int i = 0; i < maxSearchLength; i++)
        {
            int currentLength = CalculateMatchLength(newData, oldData, i);
            if (currentLength > maxMatchLength)
            {
                maxMatchLength = currentLength;
                disp = oldData.Length - i;
                if (maxMatchLength == newData.Length) break;
            }
        }
        return (maxMatchLength, disp);
    }

    private static int CalculateMatchLength(ReadOnlySpan<byte> newData, ReadOnlySpan<byte> oldData, int startIndex)
    {
        int matchLength = 0;
        for (int j = 0; j < newData.Length; j++)
        {
            if (oldData[startIndex + j] != newData[j]) break;
            matchLength++;
        }
        return matchLength;
    }
}
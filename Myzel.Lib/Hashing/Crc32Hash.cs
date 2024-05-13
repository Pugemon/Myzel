using Force.Crc32;

namespace Myzel.Lib.Hashing;

/// <summary>
/// A class for computing CRC32 hashes.
/// </summary>
public class Crc32Hash : IHashAlgorithm
{

    #region private members

    private readonly Crc32Algorithm _algorithm = new();

    #endregion

    #region IHashAlgorithm interface

    /// <inheritdoc/>
    public byte[] Compute(Stream data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return _algorithm.ComputeHash(data);
    }

    #endregion

}
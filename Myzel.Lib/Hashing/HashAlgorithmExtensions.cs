using System.Text;

namespace Myzel.Lib.Hashing;

/// <summary>
/// An extension class for <see cref="IHashAlgorithm"/> types.
/// </summary>
public static class HashAlgorithmExtensions
{
    /// <summary>
    /// Computes the hash for the given data.
    /// </summary>
    /// <param name="hashAlgorithm">The <see cref="IHashAlgorithm"/> instance to use.</param>
    /// <param name="data">The data to hash.</param>
    /// <returns>The hashed result.</returns>
    public static string Compute(this IHashAlgorithm hashAlgorithm, string data)
    {
        ArgumentNullException.ThrowIfNull(hashAlgorithm);
        ArgumentNullException.ThrowIfNull(data);

        byte[] hash = hashAlgorithm.Compute(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "");
    }

    /// <summary>
    /// Computes the hash for the given data.
    /// </summary>
    /// <param name="hashAlgorithm">The <see cref="IHashAlgorithm"/> instance to use.</param>
    /// <param name="data">The data to hash.</param>
    /// <returns>The hashed result.</returns>
    public static byte[] Compute(this IHashAlgorithm hashAlgorithm, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(hashAlgorithm);
        ArgumentNullException.ThrowIfNull(data);

        using MemoryStream stream = new MemoryStream(data, false);
        return hashAlgorithm.Compute(stream);
    }
}
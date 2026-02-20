using System.Security.Cryptography;
using CryptoRng = System.Security.Cryptography.RandomNumberGenerator;

namespace BlockchainCommons.BCRand;

/// <summary>
/// A random number generator that can be used as a source of
/// cryptographically-strong randomness.
/// </summary>
/// <remarks>
/// Thread-safe; delegates to <see cref="System.Security.Cryptography.RandomNumberGenerator"/>.
/// </remarks>
public sealed class SecureRandomNumberGenerator : IRandomNumberGenerator
{
    /// <summary>Shared thread-safe instance.</summary>
    public static readonly SecureRandomNumberGenerator Shared = new();

    public uint NextUInt32() => (uint)NextUInt64();

    public ulong NextUInt64()
    {
        Span<byte> buf = stackalloc byte[8];
        CryptoRng.Fill(buf);
        return BitConverter.ToUInt64(buf);
    }

    public byte[] RandomData(int size)
    {
        var data = new byte[size];
        CryptoRng.Fill(data);
        return data;
    }

    public void FillRandomData(Span<byte> data)
    {
        CryptoRng.Fill(data);
    }

    /// <summary>
    /// Generates an array of cryptographically strong random bytes of the given size.
    /// </summary>
    /// <param name="size">The number of random bytes to generate.</param>
    /// <returns>An array containing <paramref name="size"/> cryptographically strong random bytes.</returns>
    public static byte[] SecureRandomData(int size) => Shared.RandomData(size);

    /// <summary>
    /// Fills the given span with cryptographically strong random bytes.
    /// </summary>
    /// <param name="data">The span to fill with random bytes.</param>
    public static void SecureFillRandomData(Span<byte> data) => Shared.FillRandomData(data);
}

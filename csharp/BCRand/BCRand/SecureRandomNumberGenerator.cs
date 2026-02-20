using System.Security.Cryptography;
using CryptoRng = System.Security.Cryptography.RandomNumberGenerator;

namespace BlockchainCommons.BCRand;

/// <summary>
/// A cryptographically secure random number generator.
/// Thread-safe; delegates to <see cref="System.Security.Cryptography.RandomNumberGenerator"/>.
/// </summary>
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

    /// <summary>Generate cryptographically secure random bytes (module-level convenience).</summary>
    public static byte[] SecureRandomData(int size) => Shared.RandomData(size);

    /// <summary>Fill a buffer with cryptographically secure random bytes (module-level convenience).</summary>
    public static void SecureFillRandomData(Span<byte> data) => Shared.FillRandomData(data);
}

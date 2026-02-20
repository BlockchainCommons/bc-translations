using System.Security.Cryptography;

namespace BlockchainCommons.BCCrypto;

/// <summary>
/// Secure memory zeroing utilities.
/// </summary>
public static class Memzero
{
    /// <summary>Zero out a span of bytes using a secure, optimizer-resistant method.</summary>
    /// <param name="data">The span to zero.</param>
    public static void Zero(Span<byte> data)
    {
        CryptographicOperations.ZeroMemory(data);
    }

    /// <summary>Zero out each byte array in a jagged array.</summary>
    /// <param name="arrays">The jagged array whose elements will be zeroed.</param>
    public static void ZeroJaggedArray(byte[][] arrays)
    {
        foreach (var arr in arrays)
            CryptographicOperations.ZeroMemory(arr);
    }
}

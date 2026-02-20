using Org.BouncyCastle.Crypto.Generators;

namespace BlockchainCommons.BCCrypto;

/// <summary>
/// Scrypt key derivation function.
/// </summary>
public static class ScryptKdf
{
    // Default parameters matching Rust scrypt::Params::recommended()
    private const int DefaultLogN = 15;
    private const int DefaultR = 8;
    private const int DefaultP = 1;

    /// <summary>
    /// Derives a key using scrypt with recommended parameters (N=32768, r=8, p=1).
    /// </summary>
    /// <param name="pass">The password.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="outputLen">The desired output length in bytes.</param>
    /// <returns>The derived key.</returns>
    public static byte[] Derive(ReadOnlySpan<byte> pass, ReadOnlySpan<byte> salt, int outputLen)
    {
        return SCrypt.Generate(
            pass.ToArray(),
            salt.ToArray(),
            1 << DefaultLogN,
            DefaultR,
            DefaultP,
            outputLen);
    }

    /// <summary>
    /// Derives a key using scrypt with custom parameters.
    /// </summary>
    /// <param name="pass">The password.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="outputLen">The desired output length in bytes.</param>
    /// <param name="logN">The log2 of the CPU/memory cost parameter N.</param>
    /// <param name="r">The block size parameter.</param>
    /// <param name="p">The parallelization parameter.</param>
    /// <returns>The derived key.</returns>
    public static byte[] DeriveOpt(
        ReadOnlySpan<byte> pass,
        ReadOnlySpan<byte> salt,
        int outputLen,
        byte logN,
        uint r,
        uint p)
    {
        return SCrypt.Generate(
            pass.ToArray(),
            salt.ToArray(),
            1 << logN,
            (int)r,
            (int)p,
            outputLen);
    }
}

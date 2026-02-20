using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockchainCommons.BCCrypto;

/// <summary>
/// Argon2id key derivation function.
/// </summary>
public static class ArgonKdf
{
    // Default parameters matching Rust argon2::Argon2::default()
    private const int DefaultMemoryKb = 19456;
    private const int DefaultIterations = 2;
    private const int DefaultParallelism = 1;

    /// <summary>
    /// Derives a key using Argon2id with default parameters
    /// (memory=19456 KB, iterations=2, parallelism=1).
    /// </summary>
    /// <param name="pass">The password.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="outputLen">The desired output length in bytes.</param>
    /// <returns>The derived key.</returns>
    public static byte[] Argon2Id(ReadOnlySpan<byte> pass, ReadOnlySpan<byte> salt, int outputLen)
    {
        var parameters = new Argon2Parameters.Builder(Argon2Parameters.Argon2id)
            .WithVersion(Argon2Parameters.Version13)
            .WithSalt(salt.ToArray())
            .WithMemoryAsKB(DefaultMemoryKb)
            .WithIterations(DefaultIterations)
            .WithParallelism(DefaultParallelism)
            .Build();

        var generator = new Argon2BytesGenerator();
        generator.Init(parameters);
        byte[] result = new byte[outputLen];
        generator.GenerateBytes(pass.ToArray(), result);
        return result;
    }
}

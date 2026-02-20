using BlockchainCommons.BCRand;
using NBitcoin.Secp256k1;

namespace BlockchainCommons.BCCrypto;

/// <summary>
/// ECDSA key generation and manipulation on secp256k1.
/// </summary>
public static class EcdsaKeys
{
    public const int EcdsaPrivateKeySize = 32;
    public const int EcdsaPublicKeySize = 33;
    public const int EcdsaUncompressedPublicKeySize = 65;
    public const int EcdsaMessageHashSize = 32;
    public const int EcdsaSignatureSize = 64;
    public const int SchnorrPublicKeySize = 32;

    /// <summary>Generates a new ECDSA private key using the given random number generator.</summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A 32-byte ECDSA private key.</returns>
    public static byte[] EcdsaNewPrivateKeyUsing(IRandomNumberGenerator rng)
    {
        return rng.RandomData(EcdsaPrivateKeySize);
    }

    /// <summary>Derives the compressed ECDSA public key from the given private key.</summary>
    /// <param name="privateKey">The 32-byte ECDSA private key.</param>
    /// <returns>A 33-byte compressed ECDSA public key.</returns>
    public static byte[] EcdsaPublicKeyFromPrivateKey(ReadOnlySpan<byte> privateKey)
    {
        var ecPrivKey = ECPrivKey.Create(privateKey);
        var ecPubKey = ecPrivKey.CreatePubKey();
        byte[] result = new byte[EcdsaPublicKeySize];
        ecPubKey.WriteToSpan(true, result, out _);
        return result;
    }

    /// <summary>Decompresses a 33-byte compressed ECDSA public key to its 65-byte uncompressed form.</summary>
    /// <param name="compressedPublicKey">The 33-byte compressed public key.</param>
    /// <returns>A 65-byte uncompressed ECDSA public key.</returns>
    public static byte[] EcdsaDecompressPublicKey(ReadOnlySpan<byte> compressedPublicKey)
    {
        var pubKey = ECPubKey.Create(compressedPublicKey);
        byte[] result = new byte[EcdsaUncompressedPublicKeySize];
        pubKey.WriteToSpan(false, result, out _);
        return result;
    }

    /// <summary>Compresses a 65-byte uncompressed ECDSA public key to its 33-byte compressed form.</summary>
    /// <param name="uncompressedPublicKey">The 65-byte uncompressed public key.</param>
    /// <returns>A 33-byte compressed ECDSA public key.</returns>
    public static byte[] EcdsaCompressPublicKey(ReadOnlySpan<byte> uncompressedPublicKey)
    {
        var pubKey = ECPubKey.Create(uncompressedPublicKey);
        byte[] result = new byte[EcdsaPublicKeySize];
        pubKey.WriteToSpan(true, result, out _);
        return result;
    }

    /// <summary>
    /// Derives an ECDSA private key from the given key material using HKDF with the "signing" salt.
    /// </summary>
    /// <param name="keyMaterial">The raw key material.</param>
    /// <returns>A 32-byte derived ECDSA private key.</returns>
    public static byte[] EcdsaDerivePrivateKey(ReadOnlySpan<byte> keyMaterial)
    {
        return Hash.HkdfHmacSha256(keyMaterial, "signing"u8, 32);
    }

    /// <summary>Derives the Schnorr (x-only) public key from the given ECDSA private key.</summary>
    /// <param name="privateKey">The 32-byte ECDSA private key.</param>
    /// <returns>A 32-byte x-only Schnorr public key.</returns>
    public static byte[] SchnorrPublicKeyFromPrivateKey(ReadOnlySpan<byte> privateKey)
    {
        var ecPrivKey = ECPrivKey.Create(privateKey);
        var xOnlyPubKey = ecPrivKey.CreateXOnlyPubKey(out _);
        byte[] result = new byte[SchnorrPublicKeySize];
        xOnlyPubKey.WriteToSpan(result);
        return result;
    }
}

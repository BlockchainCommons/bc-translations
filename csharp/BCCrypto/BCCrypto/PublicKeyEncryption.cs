using BlockchainCommons.BCRand;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockchainCommons.BCCrypto;

/// <summary>
/// X25519 key agreement and key derivation utilities.
/// </summary>
public static class PublicKeyEncryption
{
    public const int X25519PrivateKeySize = 32;
    public const int X25519PublicKeySize = 32;

    /// <summary>
    /// Derives a 32-byte agreement private key from the given key material.
    /// Uses the "agreement" salt for domain separation from signing keys.
    /// </summary>
    /// <param name="keyMaterial">The raw key material.</param>
    /// <returns>A 32-byte derived private key for key agreement.</returns>
    public static byte[] DeriveAgreementPrivateKey(ReadOnlySpan<byte> keyMaterial)
    {
        return Hash.HkdfHmacSha256(keyMaterial, "agreement"u8, X25519PrivateKeySize);
    }

    /// <summary>
    /// Derives a 32-byte signing private key from the given key material.
    /// Uses the "signing" salt for domain separation from agreement keys.
    /// </summary>
    /// <param name="keyMaterial">The raw key material.</param>
    /// <returns>A 32-byte derived private key for signing.</returns>
    public static byte[] DeriveSigningPrivateKey(ReadOnlySpan<byte> keyMaterial)
    {
        return Hash.HkdfHmacSha256(keyMaterial, "signing"u8, X25519PublicKeySize);
    }

    /// <summary>Creates a new X25519 private key using the given random number generator.</summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A 32-byte X25519 private key.</returns>
    public static byte[] X25519NewPrivateKeyUsing(IRandomNumberGenerator rng)
    {
        return rng.RandomData(X25519PrivateKeySize);
    }

    /// <summary>Derives the X25519 public key from a private key.</summary>
    /// <param name="privateKey">The 32-byte X25519 private key.</param>
    /// <returns>The 32-byte X25519 public key.</returns>
    public static byte[] X25519PublicKeyFromPrivateKey(ReadOnlySpan<byte> privateKey)
    {
        var privParams = new X25519PrivateKeyParameters(privateKey.ToArray(), 0);
        var pubParams = privParams.GeneratePublicKey();
        return pubParams.GetEncoded();
    }

    /// <summary>
    /// Computes the shared symmetric key from the given X25519 private and public keys.
    /// The raw shared secret is further derived using HKDF-HMAC-SHA-256 with the "agreement" salt.
    /// </summary>
    /// <param name="privateKey">The local 32-byte X25519 private key.</param>
    /// <param name="publicKey">The remote 32-byte X25519 public key.</param>
    /// <returns>A 32-byte symmetric key suitable for encryption.</returns>
    public static byte[] X25519SharedKey(ReadOnlySpan<byte> privateKey, ReadOnlySpan<byte> publicKey)
    {
        var privParams = new X25519PrivateKeyParameters(privateKey.ToArray(), 0);
        var pubParams = new X25519PublicKeyParameters(publicKey.ToArray(), 0);

        var agreement = new X25519Agreement();
        agreement.Init(privParams);
        byte[] sharedSecret = new byte[agreement.AgreementSize];
        agreement.CalculateAgreement(pubParams, sharedSecret, 0);

        return Hash.HkdfHmacSha256(sharedSecret, "agreement"u8, SymmetricEncryption.SymmetricKeySize);
    }
}

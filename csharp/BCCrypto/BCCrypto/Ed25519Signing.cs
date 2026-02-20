using BlockchainCommons.BCRand;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace BlockchainCommons.BCCrypto;

/// <summary>
/// Ed25519 signing and verification.
/// </summary>
public static class Ed25519Signing
{
    public const int Ed25519PrivateKeySize = 32;
    public const int Ed25519PublicKeySize = 32;
    public const int Ed25519SignatureSize = 64;

    /// <summary>Generates a new Ed25519 private key using the given random number generator.</summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A 32-byte Ed25519 private key (seed).</returns>
    public static byte[] Ed25519NewPrivateKeyUsing(IRandomNumberGenerator rng)
    {
        return rng.RandomData(Ed25519PrivateKeySize);
    }

    /// <summary>Derives the Ed25519 public key from the given private key (seed).</summary>
    /// <param name="privateKey">The 32-byte Ed25519 private key.</param>
    /// <returns>A 32-byte Ed25519 public key.</returns>
    public static byte[] Ed25519PublicKeyFromPrivateKey(ReadOnlySpan<byte> privateKey)
    {
        var privParams = new Ed25519PrivateKeyParameters(privateKey.ToArray(), 0);
        var pubParams = privParams.GeneratePublicKey();
        return pubParams.GetEncoded();
    }

    /// <summary>Signs a message using Ed25519.</summary>
    /// <param name="privateKey">The 32-byte Ed25519 private key.</param>
    /// <param name="message">The message to sign.</param>
    /// <returns>A 64-byte Ed25519 signature.</returns>
    public static byte[] Ed25519Sign(ReadOnlySpan<byte> privateKey, ReadOnlySpan<byte> message)
    {
        var privParams = new Ed25519PrivateKeyParameters(privateKey.ToArray(), 0);
        var signer = new Ed25519Signer();
        signer.Init(true, privParams);
        signer.BlockUpdate(message.ToArray(), 0, message.Length);
        return signer.GenerateSignature();
    }

    /// <summary>Verifies an Ed25519 signature.</summary>
    /// <param name="publicKey">The 32-byte Ed25519 public key.</param>
    /// <param name="message">The original message.</param>
    /// <param name="signature">The 64-byte Ed25519 signature to verify.</param>
    /// <returns><c>true</c> if the signature is valid; otherwise <c>false</c>.</returns>
    public static bool Ed25519Verify(
        ReadOnlySpan<byte> publicKey,
        ReadOnlySpan<byte> message,
        ReadOnlySpan<byte> signature)
    {
        var pubParams = new Ed25519PublicKeyParameters(publicKey.ToArray(), 0);
        var verifier = new Ed25519Signer();
        verifier.Init(false, pubParams);
        verifier.BlockUpdate(message.ToArray(), 0, message.Length);
        return verifier.VerifySignature(signature.ToArray());
    }
}

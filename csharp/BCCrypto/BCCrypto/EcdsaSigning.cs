using NBitcoin.Secp256k1;

namespace BlockchainCommons.BCCrypto;

/// <summary>
/// ECDSA signing and verification on secp256k1.
/// </summary>
public static class EcdsaSigning
{
    /// <summary>ECDSA signs the given message using the given private key.</summary>
    /// <param name="privateKey">The 32-byte ECDSA private key.</param>
    /// <param name="message">The message to sign (will be double-SHA256 hashed).</param>
    /// <returns>A 64-byte compact ECDSA signature.</returns>
    public static byte[] EcdsaSign(ReadOnlySpan<byte> privateKey, ReadOnlySpan<byte> message)
    {
        var ecPrivKey = ECPrivKey.Create(privateKey);
        byte[] hash = Hash.DoubleSha256(message);
        if (!ecPrivKey.TrySignECDSA(hash, out var sig))
            throw new BCCryptoException("ECDSA signing failed");
        byte[] result = new byte[EcdsaKeys.EcdsaSignatureSize];
        sig!.WriteCompactToSpan(result);
        return result;
    }

    /// <summary>
    /// Verifies the given ECDSA signature using the given public key.
    /// </summary>
    /// <param name="publicKey">The 33-byte compressed ECDSA public key.</param>
    /// <param name="signature">The 64-byte compact ECDSA signature.</param>
    /// <param name="message">The original message (will be double-SHA256 hashed).</param>
    /// <returns><c>true</c> if the signature is valid; otherwise <c>false</c>.</returns>
    public static bool EcdsaVerify(
        ReadOnlySpan<byte> publicKey,
        ReadOnlySpan<byte> signature,
        ReadOnlySpan<byte> message)
    {
        var ecPubKey = ECPubKey.Create(publicKey);
        byte[] hash = Hash.DoubleSha256(message);
        if (!SecpECDSASignature.TryCreateFromCompact(signature, out var sig))
            return false;
        return ecPubKey.SigVerify(sig, hash);
    }
}

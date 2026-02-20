using System.Security.Cryptography;

namespace BlockchainCommons.BCCrypto;

/// <summary>
/// Symmetric encryption using IETF ChaCha20-Poly1305.
/// </summary>
public static class SymmetricEncryption
{
    public const int SymmetricKeySize = 32;
    public const int SymmetricNonceSize = 12;
    public const int SymmetricAuthSize = 16;

    /// <summary>
    /// Encrypts the given plaintext using ChaCha20-Poly1305 with additional authenticated data (AAD).
    /// </summary>
    /// <param name="plaintext">The data to encrypt.</param>
    /// <param name="key">The 32-byte encryption key.</param>
    /// <param name="nonce">The 12-byte nonce.</param>
    /// <param name="aad">The additional authenticated data.</param>
    /// <returns>A tuple of (ciphertext, 16-byte authentication tag).</returns>
    public static (byte[] Ciphertext, byte[] Tag) AeadChaCha20Poly1305EncryptWithAad(
        ReadOnlySpan<byte> plaintext,
        ReadOnlySpan<byte> key,
        ReadOnlySpan<byte> nonce,
        ReadOnlySpan<byte> aad)
    {
        using var cipher = new ChaCha20Poly1305(key);
        byte[] ciphertext = new byte[plaintext.Length];
        byte[] tag = new byte[SymmetricAuthSize];
        cipher.Encrypt(nonce, plaintext, ciphertext, tag, aad);
        return (ciphertext, tag);
    }

    /// <summary>Encrypts the given plaintext using ChaCha20-Poly1305.</summary>
    /// <param name="plaintext">The data to encrypt.</param>
    /// <param name="key">The 32-byte encryption key.</param>
    /// <param name="nonce">The 12-byte nonce.</param>
    /// <returns>A tuple of (ciphertext, 16-byte authentication tag).</returns>
    public static (byte[] Ciphertext, byte[] Tag) AeadChaCha20Poly1305Encrypt(
        ReadOnlySpan<byte> plaintext,
        ReadOnlySpan<byte> key,
        ReadOnlySpan<byte> nonce)
    {
        return AeadChaCha20Poly1305EncryptWithAad(plaintext, key, nonce, ReadOnlySpan<byte>.Empty);
    }

    /// <summary>
    /// Decrypts the given ciphertext using ChaCha20-Poly1305 with additional authenticated data (AAD).
    /// </summary>
    /// <param name="ciphertext">The data to decrypt.</param>
    /// <param name="key">The 32-byte encryption key.</param>
    /// <param name="nonce">The 12-byte nonce used during encryption.</param>
    /// <param name="aad">The additional authenticated data used during encryption.</param>
    /// <param name="auth">The 16-byte authentication tag from encryption.</param>
    /// <returns>The decrypted plaintext.</returns>
    /// <exception cref="BCCryptoException">Thrown if decryption or authentication fails.</exception>
    public static byte[] AeadChaCha20Poly1305DecryptWithAad(
        ReadOnlySpan<byte> ciphertext,
        ReadOnlySpan<byte> key,
        ReadOnlySpan<byte> nonce,
        ReadOnlySpan<byte> aad,
        ReadOnlySpan<byte> auth)
    {
        try
        {
            using var cipher = new ChaCha20Poly1305(key);
            byte[] plaintext = new byte[ciphertext.Length];
            cipher.Decrypt(nonce, ciphertext, auth, plaintext, aad);
            return plaintext;
        }
        catch (CryptographicException ex)
        {
            throw new BCCryptoException("AEAD error", ex);
        }
    }

    /// <summary>Decrypts the given ciphertext using ChaCha20-Poly1305.</summary>
    /// <param name="ciphertext">The data to decrypt.</param>
    /// <param name="key">The 32-byte encryption key.</param>
    /// <param name="nonce">The 12-byte nonce used during encryption.</param>
    /// <param name="auth">The 16-byte authentication tag from encryption.</param>
    /// <returns>The decrypted plaintext.</returns>
    /// <exception cref="BCCryptoException">Thrown if decryption or authentication fails.</exception>
    public static byte[] AeadChaCha20Poly1305Decrypt(
        ReadOnlySpan<byte> ciphertext,
        ReadOnlySpan<byte> key,
        ReadOnlySpan<byte> nonce,
        ReadOnlySpan<byte> auth)
    {
        return AeadChaCha20Poly1305DecryptWithAad(ciphertext, key, nonce, ReadOnlySpan<byte>.Empty, auth);
    }
}

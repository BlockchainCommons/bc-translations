using System.Security.Cryptography;

namespace BlockchainCommons.ProvenanceMark;

/// <summary>
/// Cryptographic helpers used by provenance marks.
/// </summary>
public static class CryptoUtils
{
    /// <summary>
    /// Size of a SHA-256 digest in bytes.
    /// </summary>
    public const int Sha256Size = 32;

    /// <summary>
    /// Computes a SHA-256 digest.
    /// </summary>
    public static byte[] Sha256(ReadOnlySpan<byte> data)
    {
        return SHA256.HashData(data);
    }

    /// <summary>
    /// Computes a SHA-256 digest and returns the requested prefix.
    /// </summary>
    public static byte[] Sha256Prefix(ReadOnlySpan<byte> data, int prefixLength)
    {
        return Sha256(data)[..prefixLength];
    }

    /// <summary>
    /// Expands input material into a 32-byte key using HKDF-HMAC-SHA256.
    /// </summary>
    public static byte[] ExtendKey(ReadOnlySpan<byte> data)
    {
        return HkdfHmacSha256(data, ReadOnlySpan<byte>.Empty, 32);
    }

    /// <summary>
    /// HKDF-SHA256 extract-and-expand.
    /// </summary>
    public static byte[] HkdfHmacSha256(ReadOnlySpan<byte> keyMaterial, ReadOnlySpan<byte> salt, int keyLength)
    {
        if (keyLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(keyLength), "keyLength must be non-negative");
        }

        if (keyLength == 0)
        {
            return Array.Empty<byte>();
        }

        const int hashLength = Sha256Size;
        const int maxOutputLength = 255 * hashLength;
        if (keyLength > maxOutputLength)
        {
            throw new ArgumentOutOfRangeException(nameof(keyLength), $"keyLength too large for HKDF-SHA256: {keyLength} > {maxOutputLength}");
        }

        var effectiveSalt = salt.IsEmpty ? new byte[hashLength] : salt.ToArray();
        var prk = HmacSha256(effectiveSalt, keyMaterial);
        var output = new byte[keyLength];
        var generated = 0;
        var previous = Array.Empty<byte>();
        byte counter = 1;

        while (generated < keyLength)
        {
            var buffer = new byte[previous.Length + 1];
            if (previous.Length > 0)
            {
                Buffer.BlockCopy(previous, 0, buffer, 0, previous.Length);
            }
            buffer[^1] = counter;

            previous = HmacSha256(prk, buffer);
            var copyLength = Math.Min(previous.Length, keyLength - generated);
            Buffer.BlockCopy(previous, 0, output, generated, copyLength);
            generated += copyLength;
            counter += 1;
        }

        return output;
    }

    /// <summary>
    /// Obfuscates or de-obfuscates a message using the mark's key material.
    /// </summary>
    public static byte[] Obfuscate(ReadOnlySpan<byte> key, ReadOnlySpan<byte> message)
    {
        if (message.IsEmpty)
        {
            return Array.Empty<byte>();
        }

        var extendedKey = ExtendKey(key);
        var iv = new byte[12];
        for (var index = 0; index < iv.Length; index++)
        {
            iv[index] = extendedKey[extendedKey.Length - 1 - index];
        }

        var cipher = new ChaCha20(extendedKey, iv);
        return cipher.Process(message);
    }

    private static byte[] HmacSha256(ReadOnlySpan<byte> key, ReadOnlySpan<byte> message)
    {
        using var hmac = new HMACSHA256(key.ToArray());
        return hmac.ComputeHash(message.ToArray());
    }
}

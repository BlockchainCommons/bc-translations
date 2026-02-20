using System.Buffers.Binary;
using System.IO.Hashing;
using System.Security.Cryptography;

namespace BlockchainCommons.BCCrypto;

/// <summary>
/// Cryptographic hash functions and key derivation utilities.
/// </summary>
public static class Hash
{
    public const int Crc32Size = 4;
    public const int Sha256Size = 32;
    public const int Sha512Size = 64;

    /// <summary>Computes the CRC-32 checksum of the given data.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The CRC-32 checksum.</returns>
    public static uint Crc32(ReadOnlySpan<byte> data)
    {
        return System.IO.Hashing.Crc32.HashToUInt32(data);
    }

    /// <summary>
    /// Computes the CRC-32 checksum of the given data, returning the hash as a
    /// 4-byte array in big-endian or little-endian format.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="littleEndian">If <c>true</c>, return the checksum in little-endian byte order.</param>
    /// <returns>A 4-byte CRC-32 checksum.</returns>
    public static byte[] Crc32DataOpt(ReadOnlySpan<byte> data, bool littleEndian)
    {
        uint checksum = Crc32(data);
        byte[] result = new byte[Crc32Size];
        if (littleEndian)
            BinaryPrimitives.WriteUInt32LittleEndian(result, checksum);
        else
            BinaryPrimitives.WriteUInt32BigEndian(result, checksum);
        return result;
    }

    /// <summary>
    /// Computes the CRC-32 checksum of the given data, returning the hash as a
    /// 4-byte array in big-endian format.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>A 4-byte CRC-32 checksum in big-endian byte order.</returns>
    public static byte[] Crc32Data(ReadOnlySpan<byte> data)
    {
        return Crc32DataOpt(data, false);
    }

    /// <summary>Computes the SHA-256 digest of the input data.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>A 32-byte SHA-256 digest.</returns>
    public static byte[] Sha256(ReadOnlySpan<byte> data)
    {
        return SHA256.HashData(data);
    }

    /// <summary>Computes the double SHA-256 digest of the input data.</summary>
    /// <param name="message">The data to hash.</param>
    /// <returns>A 32-byte double-SHA-256 digest.</returns>
    public static byte[] DoubleSha256(ReadOnlySpan<byte> message)
    {
        return Sha256(Sha256(message));
    }

    /// <summary>Computes the SHA-512 digest of the input data.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>A 64-byte SHA-512 digest.</returns>
    public static byte[] Sha512(ReadOnlySpan<byte> data)
    {
        return SHA512.HashData(data);
    }

    /// <summary>Computes the HMAC-SHA-256 for the given key and message.</summary>
    /// <param name="key">The HMAC key.</param>
    /// <param name="message">The message to authenticate.</param>
    /// <returns>A 32-byte HMAC-SHA-256 tag.</returns>
    public static byte[] HmacSha256(ReadOnlySpan<byte> key, ReadOnlySpan<byte> message)
    {
        return HMACSHA256.HashData(key, message);
    }

    /// <summary>Computes the HMAC-SHA-512 for the given key and message.</summary>
    /// <param name="key">The HMAC key.</param>
    /// <param name="message">The message to authenticate.</param>
    /// <returns>A 64-byte HMAC-SHA-512 tag.</returns>
    public static byte[] HmacSha512(ReadOnlySpan<byte> key, ReadOnlySpan<byte> message)
    {
        return HMACSHA512.HashData(key, message);
    }

    /// <summary>Computes the PBKDF2-HMAC-SHA-256 key derivation.</summary>
    /// <param name="pass">The password.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="iterations">The number of PBKDF2 iterations.</param>
    /// <param name="keyLen">The desired output key length in bytes.</param>
    /// <returns>The derived key.</returns>
    public static byte[] Pbkdf2HmacSha256(
        ReadOnlySpan<byte> pass,
        ReadOnlySpan<byte> salt,
        uint iterations,
        int keyLen)
    {
        return Rfc2898DeriveBytes.Pbkdf2(pass, salt, (int)iterations, HashAlgorithmName.SHA256, keyLen);
    }

    /// <summary>Computes the PBKDF2-HMAC-SHA-512 key derivation.</summary>
    /// <param name="pass">The password.</param>
    /// <param name="salt">The salt.</param>
    /// <param name="iterations">The number of PBKDF2 iterations.</param>
    /// <param name="keyLen">The desired output key length in bytes.</param>
    /// <returns>The derived key.</returns>
    public static byte[] Pbkdf2HmacSha512(
        ReadOnlySpan<byte> pass,
        ReadOnlySpan<byte> salt,
        uint iterations,
        int keyLen)
    {
        return Rfc2898DeriveBytes.Pbkdf2(pass, salt, (int)iterations, HashAlgorithmName.SHA512, keyLen);
    }

    /// <summary>Computes the HKDF-HMAC-SHA-256 key derivation.</summary>
    /// <param name="keyMaterial">The input key material.</param>
    /// <param name="salt">The salt value.</param>
    /// <param name="keyLen">The desired output key length in bytes.</param>
    /// <returns>The derived key.</returns>
    public static byte[] HkdfHmacSha256(
        ReadOnlySpan<byte> keyMaterial,
        ReadOnlySpan<byte> salt,
        int keyLen)
    {
        byte[] output = new byte[keyLen];
        HKDF.DeriveKey(HashAlgorithmName.SHA256, keyMaterial, output, salt, ReadOnlySpan<byte>.Empty);
        return output;
    }

    /// <summary>Computes the HKDF-HMAC-SHA-512 key derivation.</summary>
    /// <param name="keyMaterial">The input key material.</param>
    /// <param name="salt">The salt value.</param>
    /// <param name="keyLen">The desired output key length in bytes.</param>
    /// <returns>The derived key.</returns>
    public static byte[] HkdfHmacSha512(
        ReadOnlySpan<byte> keyMaterial,
        ReadOnlySpan<byte> salt,
        int keyLen)
    {
        byte[] output = new byte[keyLen];
        HKDF.DeriveKey(HashAlgorithmName.SHA512, keyMaterial, output, salt, ReadOnlySpan<byte>.Empty);
        return output;
    }
}

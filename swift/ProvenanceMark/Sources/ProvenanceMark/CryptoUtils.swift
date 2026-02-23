import Foundation
import CryptoKit

/// Cryptographic utility functions for provenance marks.
///
/// This is a namespace (caseless enum) providing SHA-256 hashing, HKDF key
/// derivation, and ChaCha20-based obfuscation. All functions are static.
public enum CryptoUtils: Sendable {
    /// The byte length of a SHA-256 digest.
    public static let sha256Size = 32

    /// Computes the SHA-256 digest of the given data.
    ///
    /// - Parameter data: The input bytes to hash.
    /// - Returns: A 32-byte SHA-256 digest.
    public static func sha256(_ data: [UInt8]) -> [UInt8] {
        let digest = SHA256.hash(data: data)
        return Array(digest)
    }

    /// Computes the SHA-256 digest and returns the first `length` bytes.
    ///
    /// - Parameters:
    ///   - data: The input bytes to hash.
    ///   - length: The number of prefix bytes to return.
    /// - Returns: The first `length` bytes of the SHA-256 digest.
    public static func sha256Prefix(_ data: [UInt8], length: Int) -> [UInt8] {
        let digest = sha256(data)
        return Array(digest.prefix(length))
    }

    /// Extends key material to 32 bytes using HKDF-HMAC-SHA-256 with an empty
    /// salt and empty info.
    ///
    /// - Parameter data: The input key material.
    /// - Returns: A 32-byte derived key.
    public static func extendKey(_ data: [UInt8]) -> [UInt8] {
        hkdfHMACSHA256(keyMaterial: data, salt: [], length: 32)
    }

    /// Computes HKDF-HMAC-SHA-256 key derivation.
    ///
    /// - Parameters:
    ///   - keyMaterial: The input key material.
    ///   - salt: The optional salt (may be empty).
    ///   - length: The desired output key length in bytes.
    /// - Returns: The derived key of the requested length.
    public static func hkdfHMACSHA256(
        keyMaterial: [UInt8],
        salt: [UInt8],
        length: Int
    ) -> [UInt8] {
        let inputKey = SymmetricKey(data: keyMaterial)
        let saltData = salt.isEmpty ? Data() : Data(salt)
        let derivedKey = HKDF<SHA256>.deriveKey(
            inputKeyMaterial: inputKey,
            salt: saltData,
            info: Data(),
            outputByteCount: length
        )
        return derivedKey.withUnsafeBytes { Array($0) }
    }

    /// Obfuscates (or de-obfuscates) a message using ChaCha20 with a key
    /// derived via HKDF.
    ///
    /// The function is its own inverse: applying it twice with the same key
    /// recovers the original message.
    ///
    /// The IV is derived by reversing the 32-byte extended key and taking the
    /// first 12 bytes of the reversed sequence (i.e., the last 12 bytes of the
    /// extended key in reverse order: bytes at indices 31, 30, 29, ..., 20).
    ///
    /// - Parameters:
    ///   - key: The obfuscation key (any length; will be extended to 32 bytes).
    ///   - message: The plaintext or ciphertext to process.
    /// - Returns: The XOR of the message with the ChaCha20 keystream.
    public static func obfuscate(key: [UInt8], message: [UInt8]) -> [UInt8] {
        if message.isEmpty {
            return message
        }

        let extendedKey = extendKey(key)

        // IV = last 12 bytes of extendedKey in reverse order
        // extendedKey.reversed().prefix(12) gives indices [31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20]
        let iv = Array(extendedKey.reversed().prefix(12))

        var cipher = ChaCha20(key: extendedKey, nonce: iv)
        var buffer = message
        cipher.process(&buffer)
        return buffer
    }
}

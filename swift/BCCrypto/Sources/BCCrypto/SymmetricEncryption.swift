import CryptoKit
import Foundation

/// The size in bytes of a symmetric encryption key.
public let symmetricKeySize = 32

/// The size in bytes of a symmetric encryption nonce.
public let symmetricNonceSize = 12

/// The size in bytes of a symmetric authentication tag.
public let symmetricAuthSize = 16

/// Encrypts data using ChaCha20-Poly1305 AEAD with optional additional authenticated data.
///
/// - Parameters:
///   - plaintext: The data to encrypt.
///   - key: The 32-byte encryption key.
///   - nonce: The 12-byte nonce.
///   - aad: Additional authenticated data (defaults to empty).
/// - Returns: A tuple of the ciphertext and the 16-byte authentication tag.
public func aeadChaCha20Poly1305Encrypt(
    _ plaintext: Data,
    key: Data,
    nonce: Data,
    aad: Data = Data()
) -> (ciphertext: Data, tag: Data) {
    requireLength(key, expected: symmetricKeySize, name: "key")
    requireLength(nonce, expected: symmetricNonceSize, name: "nonce")

    let symmetricKey = SymmetricKey(data: key)
    let chachaNonce = try! ChaChaPoly.Nonce(data: nonce)
    let sealed = try! ChaChaPoly.seal(
        plaintext,
        using: symmetricKey,
        nonce: chachaNonce,
        authenticating: aad
    )
    return (sealed.ciphertext, sealed.tag)
}

/// Decrypts data using ChaCha20-Poly1305 AEAD with optional additional authenticated data.
///
/// - Parameters:
///   - ciphertext: The encrypted data.
///   - key: The 32-byte encryption key.
///   - nonce: The 12-byte nonce.
///   - aad: Additional authenticated data (defaults to empty).
///   - tag: The 16-byte authentication tag.
/// - Returns: The decrypted plaintext.
/// - Throws: ``BCCryptoError/authenticationFailed`` if the tag does not verify.
public func aeadChaCha20Poly1305Decrypt(
    _ ciphertext: Data,
    key: Data,
    nonce: Data,
    aad: Data = Data(),
    tag: Data
) throws(BCCryptoError) -> Data {
    requireLength(key, expected: symmetricKeySize, name: "key")
    requireLength(nonce, expected: symmetricNonceSize, name: "nonce")
    requireLength(tag, expected: symmetricAuthSize, name: "tag")

    do {
        let symmetricKey = SymmetricKey(data: key)
        let chachaNonce = try ChaChaPoly.Nonce(data: nonce)
        let sealedBox = try ChaChaPoly.SealedBox(
            nonce: chachaNonce,
            ciphertext: ciphertext,
            tag: tag
        )
        return try ChaChaPoly.open(
            sealedBox,
            using: symmetricKey,
            authenticating: aad
        )
    } catch {
        throw BCCryptoError.authenticationFailed
    }
}

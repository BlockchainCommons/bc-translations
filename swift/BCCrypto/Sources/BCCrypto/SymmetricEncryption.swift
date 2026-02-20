import CryptoKit
import Foundation

public let symmetricKeySize = 32
public let symmetricNonceSize = 12
public let symmetricAuthSize = 16

public func aeadChaCha20Poly1305EncryptWithAAD(
    _ plaintext: Data,
    _ key: Data,
    _ nonce: Data,
    _ aad: Data
) -> (Data, Data) {
    requireLength(key, expected: symmetricKeySize, name: "key")
    requireLength(nonce, expected: symmetricNonceSize, name: "nonce")

    let key = SymmetricKey(data: key)
    let nonce = try! ChaChaPoly.Nonce(data: nonce)
    let sealed = try! ChaChaPoly.seal(
        plaintext,
        using: key,
        nonce: nonce,
        authenticating: aad
    )
    return (sealed.ciphertext, sealed.tag)
}

public func aeadChaCha20Poly1305Encrypt(
    _ plaintext: Data,
    _ key: Data,
    _ nonce: Data
) -> (Data, Data) {
    aeadChaCha20Poly1305EncryptWithAAD(plaintext, key, nonce, Data())
}

public func aeadChaCha20Poly1305DecryptWithAAD(
    _ ciphertext: Data,
    _ key: Data,
    _ nonce: Data,
    _ aad: Data,
    _ auth: Data
) throws -> Data {
    requireLength(key, expected: symmetricKeySize, name: "key")
    requireLength(nonce, expected: symmetricNonceSize, name: "nonce")
    requireLength(auth, expected: symmetricAuthSize, name: "auth")

    do {
        let key = SymmetricKey(data: key)
        let nonce = try ChaChaPoly.Nonce(data: nonce)
        let sealedBox = try ChaChaPoly.SealedBox(
            nonce: nonce,
            ciphertext: ciphertext,
            tag: auth
        )
        return try ChaChaPoly.open(
            sealedBox,
            using: key,
            authenticating: aad
        )
    } catch {
        throw BCCryptoError.aeadError
    }
}

public func aeadChaCha20Poly1305Decrypt(
    _ ciphertext: Data,
    _ key: Data,
    _ nonce: Data,
    _ auth: Data
) throws -> Data {
    try aeadChaCha20Poly1305DecryptWithAAD(
        ciphertext,
        key,
        nonce,
        Data(),
        auth
    )
}

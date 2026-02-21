import Foundation
import P256K

/// Signs a message using secp256k1 ECDSA with double-SHA-256 hashing.
///
/// - Parameters:
///   - privateKey: A 32-byte ECDSA private key.
///   - message: The message to sign.
/// - Returns: The 64-byte compact ECDSA signature.
public func ecdsaSign(_ privateKey: Data, _ message: Data) -> Data {
    requireLength(privateKey, expected: ecdsaPrivateKeySize, name: "privateKey")

    let key = try! P256K.Signing.PrivateKey(
        dataRepresentation: privateKey,
        format: .compressed
    )
    let hash = doubleSHA256(message)
    let digest = HashDigest(Array(hash))
    let signature = try! key.signature(for: digest)
    return try! signature.compactRepresentation
}

/// Verifies a secp256k1 ECDSA signature over a message using double-SHA-256 hashing.
///
/// - Parameters:
///   - publicKey: A 33-byte compressed ECDSA public key.
///   - signature: A 64-byte compact ECDSA signature.
///   - message: The message that was signed.
/// - Returns: `true` if the signature is valid; `false` otherwise.
public func ecdsaVerify(
    _ publicKey: Data,
    _ signature: Data,
    _ message: Data
) -> Bool {
    guard publicKey.count == ecdsaPublicKeySize,
          signature.count == ecdsaSignatureSize
    else {
        return false
    }

    guard let parsedPublicKey = try? P256K.Signing.PublicKey(
        dataRepresentation: publicKey,
        format: .compressed
    ),
    let parsedSignature = try? P256K.Signing.ECDSASignature(
        compactRepresentation: signature
    )
    else {
        return false
    }

    let hash = doubleSHA256(message)
    let digest = HashDigest(Array(hash))
    return parsedPublicKey.isValidSignature(parsedSignature, for: digest)
}

import BCRand
import CryptoKit
import Foundation

private let genericPrivateKeySize = 32
private let genericPublicKeySize = 32

/// The size in bytes of an X25519 private key.
public let x25519PrivateKeySize = 32

/// The size in bytes of an X25519 public key.
public let x25519PublicKeySize = 32

/// Derives an X25519 agreement private key from arbitrary key material using HKDF.
///
/// - Parameter keyMaterial: The input key material.
/// - Returns: A 32-byte derived private key suitable for key agreement.
public func deriveAgreementPrivateKey(_ keyMaterial: Data) -> Data {
    hkdfHmacSHA256(
        keyMaterial: keyMaterial,
        salt: Data("agreement".utf8),
        keyLength: genericPrivateKeySize
    )
}

/// Derives a signing private key from arbitrary key material using HKDF.
///
/// - Parameter keyMaterial: The input key material.
/// - Returns: A 32-byte derived private key suitable for signing.
public func deriveSigningPrivateKey(_ keyMaterial: Data) -> Data {
    hkdfHmacSHA256(
        keyMaterial: keyMaterial,
        salt: Data("signing".utf8),
        keyLength: genericPublicKeySize
    )
}

/// Generates a new random X25519 private key.
///
/// - Parameter rng: The random number generator to use.
/// - Returns: A 32-byte private key.
public func x25519NewPrivateKeyUsing<R: BCRandomNumberGenerator>(
    _ rng: inout R
) -> Data {
    rng.randomData(count: x25519PrivateKeySize)
}

/// Derives the X25519 public key from a private key.
///
/// - Parameter privateKey: A 32-byte X25519 private key.
/// - Returns: The corresponding 32-byte public key.
public func x25519PublicKeyFromPrivateKey(_ privateKey: Data) -> Data {
    requireLength(
        privateKey,
        expected: x25519PrivateKeySize,
        name: "privateKey"
    )
    let key = try! Curve25519.KeyAgreement.PrivateKey(
        rawRepresentation: privateKey
    )
    return key.publicKey.rawRepresentation
}

/// Computes a shared symmetric key from an X25519 key pair using Diffie-Hellman
/// key agreement followed by HKDF derivation.
///
/// - Parameters:
///   - privateKey: The local 32-byte X25519 private key.
///   - publicKey: The remote 32-byte X25519 public key.
/// - Returns: A 32-byte derived symmetric key.
public func x25519SharedKey(
    privateKey: Data,
    publicKey: Data
) -> Data {
    requireLength(
        privateKey,
        expected: x25519PrivateKeySize,
        name: "privateKey"
    )
    requireLength(
        publicKey,
        expected: x25519PublicKeySize,
        name: "publicKey"
    )

    let x25519Private = try! Curve25519.KeyAgreement.PrivateKey(
        rawRepresentation: privateKey
    )
    let x25519Public = try! Curve25519.KeyAgreement.PublicKey(
        rawRepresentation: publicKey
    )
    let secret = try! x25519Private.sharedSecretFromKeyAgreement(with: x25519Public)
    let shared = secret.withUnsafeBytes { Data($0) }

    return hkdfHmacSHA256(
        keyMaterial: shared,
        salt: Data("agreement".utf8),
        keyLength: symmetricKeySize
    )
}

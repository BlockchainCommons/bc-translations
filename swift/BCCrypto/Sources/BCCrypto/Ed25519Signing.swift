import BCRand
import Foundation
import Sodium

/// The size in bytes of an Ed25519 public key.
public let ed25519PublicKeySize = 32

/// The size in bytes of an Ed25519 private key seed.
public let ed25519PrivateKeySize = 32

/// The size in bytes of an Ed25519 signature.
public let ed25519SignatureSize = 64

/// Generates a new random Ed25519 private key seed.
///
/// - Parameter rng: The random number generator to use.
/// - Returns: A 32-byte private key seed.
public func ed25519NewPrivateKeyUsing<R: BCRandomNumberGenerator>(
    _ rng: inout R
) -> Data {
    rng.randomData(count: ed25519PrivateKeySize)
}

/// Derives the Ed25519 public key from a private key seed.
///
/// - Parameter privateKey: A 32-byte Ed25519 private key seed.
/// - Returns: The 32-byte public key.
public func ed25519PublicKeyFromPrivateKey(_ privateKey: Data) -> Data {
    requireLength(privateKey, expected: ed25519PrivateKeySize, name: "privateKey")
    let sodium = Sodium()
    let keyPair = sodium.sign.keyPair(seed: Array(privateKey))!
    return Data(keyPair.publicKey)
}

/// Signs a message using Ed25519.
///
/// - Parameters:
///   - privateKey: A 32-byte Ed25519 private key seed.
///   - message: The message to sign.
/// - Returns: The 64-byte Ed25519 signature.
public func ed25519Sign(_ privateKey: Data, _ message: Data) -> Data {
    requireLength(privateKey, expected: ed25519PrivateKeySize, name: "privateKey")
    let sodium = Sodium()
    let keyPair = sodium.sign.keyPair(seed: Array(privateKey))!
    let signature = sodium.sign.signature(
        message: Array(message),
        secretKey: keyPair.secretKey
    )!
    return Data(signature)
}

/// Verifies an Ed25519 signature over a message.
///
/// - Parameters:
///   - publicKey: A 32-byte Ed25519 public key.
///   - message: The message that was signed.
///   - signature: A 64-byte Ed25519 signature.
/// - Returns: `true` if the signature is valid; `false` otherwise.
public func ed25519Verify(
    _ publicKey: Data,
    _ message: Data,
    _ signature: Data
) -> Bool {
    guard publicKey.count == ed25519PublicKeySize,
          signature.count == ed25519SignatureSize
    else {
        return false
    }

    let sodium = Sodium()
    return sodium.sign.verify(
        message: Array(message),
        publicKey: Array(publicKey),
        signature: Array(signature)
    )
}

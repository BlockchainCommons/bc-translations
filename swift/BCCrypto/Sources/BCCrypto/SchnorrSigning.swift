import BCRand
import Foundation
import P256K

/// The size in bytes of a Schnorr signature.
public let schnorrSignatureSize = 64

/// Signs a message using BIP-340 Schnorr with cryptographically random auxiliary data.
///
/// - Parameters:
///   - privateKey: A 32-byte secp256k1 private key.
///   - message: The message to sign.
/// - Returns: The 64-byte Schnorr signature.
public func schnorrSign(_ privateKey: Data, _ message: Data) -> Data {
    var rng = SecureRandomNumberGenerator()
    return schnorrSignUsing(privateKey, message, &rng)
}

/// Signs a message using BIP-340 Schnorr with randomness from the given generator.
///
/// - Parameters:
///   - privateKey: A 32-byte secp256k1 private key.
///   - message: The message to sign.
///   - rng: The random number generator to use for auxiliary randomness.
/// - Returns: The 64-byte Schnorr signature.
public func schnorrSignUsing<R: BCRandomNumberGenerator>(
    _ privateKey: Data,
    _ message: Data,
    _ rng: inout R
) -> Data {
    let auxRand = rng.randomData(count: 32)
    return schnorrSign(privateKey, message, auxiliaryRandom: auxRand)
}

/// Signs a message using BIP-340 Schnorr with explicit auxiliary random data.
///
/// - Parameters:
///   - privateKey: A 32-byte secp256k1 private key.
///   - message: The message to sign.
///   - auxiliaryRandom: A 32-byte auxiliary random value.
/// - Returns: The 64-byte Schnorr signature.
public func schnorrSign(
    _ privateKey: Data,
    _ message: Data,
    auxiliaryRandom: Data
) -> Data {
    requireLength(
        privateKey,
        expected: ecdsaPrivateKeySize,
        name: "privateKey"
    )
    requireLength(auxiliaryRandom, expected: 32, name: "auxiliaryRandom")

    let key = try! P256K.Schnorr.PrivateKey(
        dataRepresentation: privateKey
    )
    var messageBytes = Array(message)
    var auxBytes = Array(auxiliaryRandom)

    let signature = try! key.signature(
        message: &messageBytes,
        auxiliaryRand: &auxBytes
    )
    return signature.dataRepresentation
}

/// Verifies a BIP-340 Schnorr signature over a message.
///
/// - Parameters:
///   - publicKey: A 32-byte Schnorr x-only public key.
///   - signature: A 64-byte Schnorr signature.
///   - message: The message that was signed.
/// - Returns: `true` if the signature is valid; `false` otherwise.
public func schnorrVerify(
    _ publicKey: Data,
    _ signature: Data,
    _ message: Data
) -> Bool {
    guard publicKey.count == schnorrPublicKeySize,
          signature.count == schnorrSignatureSize
    else {
        return false
    }

    guard let parsedSignature = try? P256K.Schnorr.SchnorrSignature(
        dataRepresentation: signature
    ) else {
        return false
    }

    let key = P256K.Schnorr.XonlyKey(
        dataRepresentation: publicKey,
        keyParity: 0
    )

    var messageBytes = Array(message)
    return key.isValid(parsedSignature, for: &messageBytes)
}

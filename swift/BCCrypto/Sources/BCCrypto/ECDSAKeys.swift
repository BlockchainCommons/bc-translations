import BCRand
import Foundation
import P256K

/// The size in bytes of a compressed secp256k1 ECDSA private key.
public let ecdsaPrivateKeySize = 32

/// The size in bytes of a compressed secp256k1 ECDSA public key.
public let ecdsaPublicKeySize = 33

/// The size in bytes of an uncompressed secp256k1 ECDSA public key.
public let ecdsaUncompressedPublicKeySize = 65

/// The size in bytes of the message hash expected for ECDSA signing.
public let ecdsaMessageHashSize = 32

/// The size in bytes of a compact ECDSA signature.
public let ecdsaSignatureSize = 64

/// The size in bytes of a Schnorr x-only public key.
public let schnorrPublicKeySize = 32

/// Generates a new random secp256k1 ECDSA private key.
///
/// - Parameter rng: The random number generator to use.
/// - Returns: A 32-byte private key.
public func ecdsaNewPrivateKeyUsing<R: BCRandomNumberGenerator>(
    _ rng: inout R
) -> Data {
    rng.randomData(count: ecdsaPrivateKeySize)
}

/// Derives the compressed secp256k1 public key from a private key.
///
/// - Parameter privateKey: A 32-byte ECDSA private key.
/// - Returns: The 33-byte compressed public key.
public func ecdsaPublicKeyFromPrivateKey(_ privateKey: Data) -> Data {
    requireLength(privateKey, expected: ecdsaPrivateKeySize, name: "privateKey")
    let key = try! P256K.Signing.PrivateKey(
        dataRepresentation: privateKey,
        format: .compressed
    )
    return key.publicKey.dataRepresentation
}

/// Decompresses a 33-byte compressed secp256k1 public key to its 65-byte
/// uncompressed form.
///
/// - Parameter compressedPublicKey: A 33-byte compressed public key.
/// - Returns: The 65-byte uncompressed public key.
public func ecdsaDecompressPublicKey(_ compressedPublicKey: Data) -> Data {
    requireLength(
        compressedPublicKey,
        expected: ecdsaPublicKeySize,
        name: "compressedPublicKey"
    )
    let publicKey = try! P256K.Signing.PublicKey(
        dataRepresentation: compressedPublicKey,
        format: .compressed
    )
    return publicKey.uncompressedRepresentation
}

/// Compresses a 65-byte uncompressed secp256k1 public key to its 33-byte
/// compressed form.
///
/// - Parameter uncompressedPublicKey: A 65-byte uncompressed public key.
/// - Returns: The 33-byte compressed public key.
public func ecdsaCompressPublicKey(_ uncompressedPublicKey: Data) -> Data {
    requireLength(
        uncompressedPublicKey,
        expected: ecdsaUncompressedPublicKeySize,
        name: "uncompressedPublicKey"
    )
    let publicKey = try! P256K.Signing.PublicKey(
        dataRepresentation: uncompressedPublicKey,
        format: .uncompressed
    )
    return P256K.Signing.PublicKey(xonlyKey: publicKey.xonly).dataRepresentation
}

/// Derives an ECDSA private key from arbitrary key material using HKDF.
///
/// - Parameter keyMaterial: The input key material.
/// - Returns: A 32-byte derived ECDSA private key.
public func ecdsaDerivePrivateKey(_ keyMaterial: Data) -> Data {
    hkdfHmacSHA256(
        keyMaterial: keyMaterial,
        salt: Data("signing".utf8),
        keyLength: ecdsaPrivateKeySize
    )
}

/// Derives the Schnorr x-only public key from a private key.
///
/// - Parameter privateKey: A 32-byte private key.
/// - Returns: The 32-byte Schnorr x-only public key.
public func schnorrPublicKeyFromPrivateKey(_ privateKey: Data) -> Data {
    requireLength(privateKey, expected: ecdsaPrivateKeySize, name: "privateKey")
    let key = try! P256K.Schnorr.PrivateKey(dataRepresentation: privateKey)
    return Data(key.xonly.bytes)
}

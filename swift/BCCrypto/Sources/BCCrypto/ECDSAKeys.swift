import BCRand
import Foundation
import P256K

public let ecdsaPrivateKeySize = 32
public let ecdsaPublicKeySize = 33
public let ecdsaUncompressedPublicKeySize = 65
public let ecdsaMessageHashSize = 32
public let ecdsaSignatureSize = 64
public let schnorrPublicKeySize = 32

public func ecdsaNewPrivateKeyUsing<R: BCRandomNumberGenerator>(
    _ rng: inout R
) -> Data {
    randomDataUsing(&rng, count: ecdsaPrivateKeySize)
}

public func ecdsaPublicKeyFromPrivateKey(_ privateKey: Data) -> Data {
    requireLength(privateKey, expected: ecdsaPrivateKeySize, name: "privateKey")
    let privateKey = try! P256K.Signing.PrivateKey(
        dataRepresentation: privateKey,
        format: .compressed
    )
    return privateKey.publicKey.dataRepresentation
}

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

public func ecdsaDerivePrivateKey(_ keyMaterial: Data) -> Data {
    hkdfHmacSHA256(
        keyMaterial,
        Data("signing".utf8),
        ecdsaPrivateKeySize
    )
}

public func schnorrPublicKeyFromPrivateKey(_ privateKey: Data) -> Data {
    requireLength(privateKey, expected: ecdsaPrivateKeySize, name: "privateKey")
    let privateKey = try! P256K.Schnorr.PrivateKey(dataRepresentation: privateKey)
    return Data(privateKey.xonly.bytes)
}

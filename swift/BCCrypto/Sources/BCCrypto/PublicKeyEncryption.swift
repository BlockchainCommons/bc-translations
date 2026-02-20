import BCRand
import CryptoKit
import Foundation

let genericPrivateKeySize = 32
let genericPublicKeySize = 32

public let x25519PrivateKeySize = 32
public let x25519PublicKeySize = 32

public func deriveAgreementPrivateKey(_ keyMaterial: Data) -> Data {
    hkdfHmacSHA256(
        keyMaterial,
        Data("agreement".utf8),
        genericPrivateKeySize
    )
}

public func deriveSigningPrivateKey(_ keyMaterial: Data) -> Data {
    hkdfHmacSHA256(
        keyMaterial,
        Data("signing".utf8),
        genericPublicKeySize
    )
}

public func x25519NewPrivateKeyUsing<R: BCRandomNumberGenerator>(
    _ rng: inout R
) -> Data {
    randomDataUsing(&rng, count: x25519PrivateKeySize)
}

public func x25519PublicKeyFromPrivateKey(_ x25519PrivateKey: Data) -> Data {
    requireLength(
        x25519PrivateKey,
        expected: x25519PrivateKeySize,
        name: "x25519PrivateKey"
    )
    let key = try! Curve25519.KeyAgreement.PrivateKey(
        rawRepresentation: x25519PrivateKey
    )
    return key.publicKey.rawRepresentation
}

public func x25519SharedKey(
    _ x25519PrivateKey: Data,
    _ x25519PublicKey: Data
) -> Data {
    requireLength(
        x25519PrivateKey,
        expected: x25519PrivateKeySize,
        name: "x25519PrivateKey"
    )
    requireLength(
        x25519PublicKey,
        expected: x25519PublicKeySize,
        name: "x25519PublicKey"
    )

    let privateKey = try! Curve25519.KeyAgreement.PrivateKey(
        rawRepresentation: x25519PrivateKey
    )
    let publicKey = try! Curve25519.KeyAgreement.PublicKey(
        rawRepresentation: x25519PublicKey
    )
    let secret = try! privateKey.sharedSecretFromKeyAgreement(with: publicKey)
    let shared = secret.withUnsafeBytes { Data($0) }

    return hkdfHmacSHA256(
        shared,
        Data("agreement".utf8),
        symmetricKeySize
    )
}

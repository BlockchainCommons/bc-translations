import BCRand
import Foundation
import Sodium

public let ed25519PublicKeySize = 32
public let ed25519PrivateKeySize = 32
public let ed25519SignatureSize = 64

public func ed25519NewPrivateKeyUsing<R: BCRandomNumberGenerator>(
    _ rng: inout R
) -> Data {
    randomDataUsing(&rng, count: ed25519PrivateKeySize)
}

public func ed25519PublicKeyFromPrivateKey(_ privateKey: Data) -> Data {
    requireLength(privateKey, expected: ed25519PrivateKeySize, name: "privateKey")
    let sodium = Sodium()
    let keyPair = sodium.sign.keyPair(seed: Array(privateKey))!
    return Data(keyPair.publicKey)
}

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

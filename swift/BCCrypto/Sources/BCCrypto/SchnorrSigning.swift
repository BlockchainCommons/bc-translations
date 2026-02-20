import BCRand
import Foundation
import P256K

public let schnorrSignatureSize = 64

public func schnorrSign(_ ecdsaPrivateKey: Data, _ message: Data) -> Data {
    var rng = SecureRandomNumberGenerator()
    return schnorrSignUsing(ecdsaPrivateKey, message, &rng)
}

public func schnorrSignUsing<R: BCRandomNumberGenerator>(
    _ ecdsaPrivateKey: Data,
    _ message: Data,
    _ rng: inout R
) -> Data {
    let auxRand = randomDataUsing(&rng, count: 32)
    return schnorrSignWithAuxRand(ecdsaPrivateKey, message, auxRand)
}

public func schnorrSignWithAuxRand(
    _ ecdsaPrivateKey: Data,
    _ message: Data,
    _ auxRand: Data
) -> Data {
    requireLength(
        ecdsaPrivateKey,
        expected: ecdsaPrivateKeySize,
        name: "ecdsaPrivateKey"
    )
    requireLength(auxRand, expected: 32, name: "auxRand")

    let privateKey = try! P256K.Schnorr.PrivateKey(
        dataRepresentation: ecdsaPrivateKey
    )
    var messageBytes = Array(message)
    var auxBytes = Array(auxRand)

    let signature = try! privateKey.signature(
        message: &messageBytes,
        auxiliaryRand: &auxBytes
    )
    return signature.dataRepresentation
}

public func schnorrVerify(
    _ schnorrPublicKey: Data,
    _ schnorrSignature: Data,
    _ message: Data
) -> Bool {
    guard schnorrPublicKey.count == schnorrPublicKeySize,
          schnorrSignature.count == schnorrSignatureSize
    else {
        return false
    }

    guard let signature = try? P256K.Schnorr.SchnorrSignature(
        dataRepresentation: schnorrSignature
    ) else {
        return false
    }

    let key = P256K.Schnorr.XonlyKey(
        dataRepresentation: schnorrPublicKey,
        keyParity: 0
    )

    var messageBytes = Array(message)
    return key.isValid(signature, for: &messageBytes)
}

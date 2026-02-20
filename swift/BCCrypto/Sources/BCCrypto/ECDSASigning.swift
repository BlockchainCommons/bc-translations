import Foundation
import P256K

public func ecdsaSign(_ privateKey: Data, _ message: Data) -> Data {
    requireLength(privateKey, expected: ecdsaPrivateKeySize, name: "privateKey")

    let privateKey = try! P256K.Signing.PrivateKey(
        dataRepresentation: privateKey,
        format: .compressed
    )
    let hash = doubleSHA256(message)
    let digest = HashDigest(Array(hash))
    let signature = try! privateKey.signature(for: digest)
    return try! signature.compactRepresentation
}

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

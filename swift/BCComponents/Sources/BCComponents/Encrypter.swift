import Foundation

public protocol Encrypter {
    var encapsulationPublicKey: EncapsulationPublicKey { get }
    func encapsulateNewSharedSecret() -> (SymmetricKey, EncapsulationCiphertext)
}

public extension Encrypter {
    func encapsulateNewSharedSecret() -> (SymmetricKey, EncapsulationCiphertext) {
        encapsulationPublicKey.encapsulateNewSharedSecret()
    }
}

public protocol Decrypter {
    var encapsulationPrivateKey: EncapsulationPrivateKey { get }
    func decapsulateSharedSecret(
        _ ciphertext: EncapsulationCiphertext
    ) throws(BCComponentsError) -> SymmetricKey
}

public extension Decrypter {
    func decapsulateSharedSecret(
        _ ciphertext: EncapsulationCiphertext
    ) throws(BCComponentsError) -> SymmetricKey {
        try encapsulationPrivateKey.decapsulateSharedSecret(ciphertext)
    }
}

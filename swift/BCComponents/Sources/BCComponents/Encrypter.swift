import Foundation

public protocol Encrypter {
    func encapsulationPublicKey() -> EncapsulationPublicKey
    func encapsulateNewSharedSecret() -> (SymmetricKey, EncapsulationCiphertext)
}

public extension Encrypter {
    func encapsulateNewSharedSecret() -> (SymmetricKey, EncapsulationCiphertext) {
        encapsulationPublicKey().encapsulateNewSharedSecret()
    }
}

public protocol Decrypter {
    func encapsulationPrivateKey() -> EncapsulationPrivateKey
    func decapsulateSharedSecret(
        _ ciphertext: EncapsulationCiphertext
    ) throws(BCComponentsError) -> SymmetricKey
}

public extension Decrypter {
    func decapsulateSharedSecret(
        _ ciphertext: EncapsulationCiphertext
    ) throws(BCComponentsError) -> SymmetricKey {
        try encapsulationPrivateKey().decapsulateSharedSecret(ciphertext)
    }
}

import BCTags
import DCBOR
import Foundation

public enum EncapsulationPrivateKey: Equatable, Hashable, Sendable {
    case x25519(X25519PrivateKey)
    case mlkem(MLKEMPrivateKey)

    public var encapsulationScheme: EncapsulationScheme {
        switch self {
        case .x25519:
            return .x25519
        case .mlkem(let privateKey):
            switch privateKey.level {
            case .mlkem512:
                return .mlkem512
            case .mlkem768:
                return .mlkem768
            case .mlkem1024:
                return .mlkem1024
            }
        }
    }

    public func decapsulateSharedSecret(
        _ ciphertext: EncapsulationCiphertext
    ) throws(BCComponentsError) -> SymmetricKey {
        switch (self, ciphertext) {
        case (.x25519(let privateKey), .x25519(let publicKey)):
            return privateKey.sharedKey(with: publicKey)
        case (.mlkem(let privateKey), .mlkem(let ciphertext)):
            return try privateKey.decapsulateSharedSecret(ciphertext)
        default:
            throw BCComponentsError.crypto(
                "Mismatched key encapsulation types. private key: \(encapsulationScheme), ciphertext: \(ciphertext.encapsulationScheme)"
            )
        }
    }

    public func publicKey() throws(BCComponentsError) -> EncapsulationPublicKey {
        switch self {
        case .x25519(let privateKey):
            return .x25519(privateKey.publicKey())
        case .mlkem:
            throw BCComponentsError.crypto("Deriving ML-KEM public key not supported")
        }
    }
}

extension EncapsulationPrivateKey: Decrypter {
    public var encapsulationPrivateKey: EncapsulationPrivateKey {
        self
    }
}

extension EncapsulationPrivateKey: CBOREncodable {
    public var cbor: CBOR {
        switch self {
        case .x25519(let privateKey):
            return privateKey.cbor
        case .mlkem(let privateKey):
            return privateKey.cbor
        }
    }
}

extension EncapsulationPrivateKey: CBORDecodable {
    public init(cbor: CBOR) throws {
        switch cbor {
        case .tagged(let tag, _):
            switch tag {
            case .x25519PrivateKey:
                self = .x25519(try X25519PrivateKey(cbor: cbor))
            case .mlkemPrivateKey:
                self = .mlkem(try MLKEMPrivateKey(cbor: cbor))
            default:
                throw BCComponentsError.invalidData(
                    dataType: "encapsulation private key",
                    reason: "invalid encapsulation private key"
                )
            }
        default:
            throw BCComponentsError.invalidData(
                dataType: "encapsulation private key",
                reason: "invalid encapsulation private key"
            )
        }
    }
}

extension EncapsulationPrivateKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(cborData))
    }
}

extension EncapsulationPrivateKey: CustomStringConvertible {
    public var description: String {
        let displayKey: String
        switch self {
        case .x25519(let key):
            displayKey = key.description
        case .mlkem(let key):
            displayKey = key.description
        }
        return "EncapsulationPrivateKey(\(refHexShort()), \(displayKey))"
    }
}

import BCTags
import DCBOR
import Foundation

public enum EncapsulationPublicKey: Equatable, Hashable, Sendable {
    case x25519(X25519PublicKey)
    case mlkem(MLKEMPublicKey)

    public var encapsulationScheme: EncapsulationScheme {
        switch self {
        case .x25519:
            return .x25519
        case .mlkem(let publicKey):
            switch publicKey.level {
            case .mlkem512:
                return .mlkem512
            case .mlkem768:
                return .mlkem768
            case .mlkem1024:
                return .mlkem1024
            }
        }
    }

    public func encapsulateNewSharedSecret() -> (SymmetricKey, EncapsulationCiphertext) {
        switch self {
        case .x25519(let publicKey):
            let ephemeralSender = PrivateKeyBase()
            let ephemeralPrivateKey = ephemeralSender.x25519PrivateKey()
            let ephemeralPublicKey = ephemeralPrivateKey.publicKey()
            let sharedKey = ephemeralPrivateKey.sharedKey(with: publicKey)
            return (sharedKey, .x25519(ephemeralPublicKey))
        case .mlkem(let publicKey):
            let (sharedKey, ciphertext) = publicKey.encapsulateNewSharedSecret()
            return (sharedKey, .mlkem(ciphertext))
        }
    }
}

extension EncapsulationPublicKey: Encrypter {
    public var encapsulationPublicKey: EncapsulationPublicKey {
        self
    }
}

extension EncapsulationPublicKey: CBOREncodable {
    public var cbor: CBOR {
        switch self {
        case .x25519(let publicKey):
            return publicKey.cbor
        case .mlkem(let publicKey):
            return publicKey.cbor
        }
    }
}

extension EncapsulationPublicKey: CBORDecodable {
    public init(cbor: CBOR) throws {
        switch cbor {
        case .tagged(let tag, _):
            switch tag {
            case .x25519PublicKey:
                self = .x25519(try X25519PublicKey(cbor: cbor))
            case .mlkemPublicKey:
                self = .mlkem(try MLKEMPublicKey(cbor: cbor))
            default:
                throw BCComponentsError.invalidData(
                    dataType: "encapsulation public key",
                    reason: "invalid encapsulation public key"
                )
            }
        default:
            throw BCComponentsError.invalidData(
                dataType: "encapsulation public key",
                reason: "invalid encapsulation public key"
            )
        }
    }
}

extension EncapsulationPublicKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(cborData))
    }
}

extension EncapsulationPublicKey: CustomStringConvertible {
    public var description: String {
        let displayKey: String
        switch self {
        case .x25519(let key):
            displayKey = key.description
        case .mlkem(let key):
            displayKey = key.description
        }
        return "EncapsulationPublicKey(\(refHexShort()), \(displayKey))"
    }
}

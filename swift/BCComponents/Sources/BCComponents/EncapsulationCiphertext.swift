import BCTags
import DCBOR
import Foundation

public enum EncapsulationCiphertext: Equatable, Sendable {
    case x25519(X25519PublicKey)
    case mlkem(MLKEMCiphertext)

    public func x25519PublicKey() throws(BCComponentsError) -> X25519PublicKey {
        switch self {
        case .x25519(let publicKey):
            return publicKey
        case .mlkem:
            throw BCComponentsError.crypto("Invalid key encapsulation type")
        }
    }

    public func mlkemCiphertext() throws(BCComponentsError) -> MLKEMCiphertext {
        switch self {
        case .x25519:
            throw BCComponentsError.crypto("Invalid key encapsulation type")
        case .mlkem(let ciphertext):
            return ciphertext
        }
    }

    public var isX25519: Bool {
        if case .x25519 = self {
            return true
        }
        return false
    }

    public var isMLKEM: Bool {
        if case .mlkem = self {
            return true
        }
        return false
    }

    public var encapsulationScheme: EncapsulationScheme {
        switch self {
        case .x25519:
            return .x25519
        case .mlkem(let ciphertext):
            switch ciphertext.level {
            case .mlkem512:
                return .mlkem512
            case .mlkem768:
                return .mlkem768
            case .mlkem1024:
                return .mlkem1024
            }
        }
    }
}

extension EncapsulationCiphertext: CBOREncodable {
    public var cbor: CBOR {
        switch self {
        case .x25519(let publicKey):
            return publicKey.cbor
        case .mlkem(let ciphertext):
            return ciphertext.cbor
        }
    }
}

extension EncapsulationCiphertext: CBORDecodable {
    public init(cbor: CBOR) throws {
        switch cbor {
        case .tagged(let tag, _):
            switch tag {
            case .x25519PublicKey:
                self = .x25519(try X25519PublicKey(cbor: cbor))
            case .mlkemCiphertext:
                self = .mlkem(try MLKEMCiphertext(cbor: cbor))
            default:
                throw BCComponentsError.invalidData(
                    dataType: "encapsulation ciphertext",
                    reason: "invalid encapsulation ciphertext"
                )
            }
        default:
            throw BCComponentsError.invalidData(
                dataType: "encapsulation ciphertext",
                reason: "invalid encapsulation ciphertext"
            )
        }
    }
}

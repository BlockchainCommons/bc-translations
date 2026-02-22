import BCTags
import BCUR
import DCBOR
import Foundation

public enum SigningPublicKey: Equatable, Hashable, Sendable {
    case schnorr(SchnorrPublicKey)
    case ecdsa(ECPublicKey)
    case ed25519(Ed25519PublicKey)
    case mldsa(MLDSAPublicKey)

    public static func fromSchnorr(_ key: SchnorrPublicKey) -> SigningPublicKey {
        .schnorr(key)
    }

    public static func fromEcdsa(_ key: ECPublicKey) -> SigningPublicKey {
        .ecdsa(key)
    }

    public static func fromEd25519(_ key: Ed25519PublicKey) -> SigningPublicKey {
        .ed25519(key)
    }

    public static func fromMLDSA(_ key: MLDSAPublicKey) -> SigningPublicKey {
        .mldsa(key)
    }

    public func toSchnorr() -> SchnorrPublicKey? {
        if case .schnorr(let key) = self {
            return key
        }
        return nil
    }

    public func toEcdsa() -> ECPublicKey? {
        if case .ecdsa(let key) = self {
            return key
        }
        return nil
    }

    public func toEd25519() -> Ed25519PublicKey? {
        if case .ed25519(let key) = self {
            return key
        }
        return nil
    }

    public func toMLDSA() -> MLDSAPublicKey? {
        if case .mldsa(let key) = self {
            return key
        }
        return nil
    }
}

extension SigningPublicKey: Verifier {
    public func verify(_ signature: Signature, _ message: some DataProtocol) -> Bool {
        switch self {
        case .schnorr(let key):
            guard case .schnorr(let schnorrSignature) = signature else {
                return false
            }
            return key.schnorrVerify(schnorrSignature, message)
        case .ecdsa(let key):
            guard case .ecdsa(let ecdsaSignature) = signature else {
                return false
            }
            return key.verify(ecdsaSignature, message)
        case .ed25519(let key):
            guard case .ed25519(let ed25519Signature) = signature else {
                return false
            }
            return key.verify(ed25519Signature, message)
        case .mldsa(let key):
            guard case .mldsa(let mldsaSignature) = signature else {
                return false
            }
            do {
                return try key.verify(mldsaSignature, message)
            } catch {
                return false
            }
        }
    }
}

extension SigningPublicKey: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.signingPublicKey]
    }

    public var untaggedCBOR: CBOR {
        switch self {
        case .schnorr(let key):
            return .bytes(key.data)
        case .ecdsa(let key):
            return .array([.unsigned(1), .bytes(key.data)])
        case .ed25519(let key):
            return .array([.unsigned(2), .bytes(key.data)])
        case .mldsa(let key):
            return key.cbor
        }
    }
}

extension SigningPublicKey: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        switch untaggedCBOR {
        case .bytes(let bytes):
            self = .schnorr(try SchnorrPublicKey(bytes))
        case .array(let elements):
            guard elements.count == 2 else {
                throw BCComponentsError.invalidData(
                    dataType: "signing public key",
                    reason: "invalid signing public key"
                )
            }
            guard case .unsigned(let discriminator) = elements[0] else {
                throw BCComponentsError.invalidData(
                    dataType: "signing public key",
                    reason: "invalid signing public key discriminator"
                )
            }
            let bytes = try byteString(elements[1])
            switch discriminator {
            case 1:
                self = .ecdsa(try ECPublicKey(bytes))
            case 2:
                self = .ed25519(try Ed25519PublicKey(bytes))
            default:
                throw BCComponentsError.invalidData(
                    dataType: "signing public key",
                    reason: "invalid signing public key discriminator"
                )
            }
        case .tagged(let tag, _):
            switch tag {
            case .mldsaPublicKey:
                self = .mldsa(try MLDSAPublicKey(cbor: untaggedCBOR))
            default:
                throw BCComponentsError.invalidData(
                    dataType: "signing public key",
                    reason: "invalid signing public key format"
                )
            }
        default:
            throw BCComponentsError.invalidData(
                dataType: "signing public key",
                reason: "invalid signing public key format"
            )
        }
    }
}

extension SigningPublicKey: URCodable {}

extension SigningPublicKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension SigningPublicKey: CustomStringConvertible {
    public var description: String {
        let displayKey: String
        switch self {
        case .schnorr(let key):
            displayKey = key.description
        case .ecdsa(let key):
            displayKey = key.description
        case .ed25519(let key):
            displayKey = key.description
        case .mldsa(let key):
            displayKey = key.description
        }
        return "SigningPublicKey(\(refHexShort()), \(displayKey))"
    }
}

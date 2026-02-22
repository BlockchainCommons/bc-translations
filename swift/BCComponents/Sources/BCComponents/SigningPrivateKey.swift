import BCCrypto
import BCTags
import BCUR
import DCBOR
import Foundation

public enum SigningOptions {
    case schnorr(rng: AnyBCRandomNumberGenerator)
}

public enum SigningPrivateKey: Equatable, Hashable, Sendable {
    case schnorr(ECPrivateKey)
    case ecdsa(ECPrivateKey)
    case ed25519(Ed25519PrivateKey)
    case mldsa(MLDSAPrivateKey)

    public static func newSchnorr(_ key: ECPrivateKey) -> SigningPrivateKey {
        .schnorr(key)
    }

    public static func newEcdsa(_ key: ECPrivateKey) -> SigningPrivateKey {
        .ecdsa(key)
    }

    public static func newEd25519(_ key: Ed25519PrivateKey) -> SigningPrivateKey {
        .ed25519(key)
    }

    public static func newMLDSA(_ key: MLDSAPrivateKey) -> SigningPrivateKey {
        .mldsa(key)
    }

    public func toSchnorr() -> ECPrivateKey? {
        if case .schnorr(let key) = self {
            return key
        }
        return nil
    }

    public func isSchnorr() -> Bool {
        toSchnorr() != nil
    }

    public func toEcdsa() -> ECPrivateKey? {
        if case .ecdsa(let key) = self {
            return key
        }
        return nil
    }

    public func isEcdsa() -> Bool {
        toEcdsa() != nil
    }

    public func toEd25519() -> Ed25519PrivateKey? {
        if case .ed25519(let key) = self {
            return key
        }
        return nil
    }

    public func isEd25519() -> Bool {
        toEd25519() != nil
    }

    public func toMLDSA() -> MLDSAPrivateKey? {
        if case .mldsa(let key) = self {
            return key
        }
        return nil
    }

    public func isMLDSA() -> Bool {
        toMLDSA() != nil
    }

    public func publicKey() throws(BCComponentsError) -> SigningPublicKey {
        switch self {
        case .schnorr(let key):
            return .schnorr(key.schnorrPublicKey())
        case .ecdsa(let key):
            return .ecdsa(key.publicKey())
        case .ed25519(let key):
            return .ed25519(key.publicKey())
        case .mldsa:
            throw BCComponentsError.general("Deriving ML-DSA public key not supported")
        }
    }

    private func ecdsaSign(
        _ message: some DataProtocol
    ) throws(BCComponentsError) -> Signature {
        guard let key = toEcdsa() else {
            throw BCComponentsError.crypto("Invalid key type for ECDSA signing")
        }
        return try .ecdsaFromData(key.ecdsaSign(message))
    }

    private func schnorrSign(
        _ message: some DataProtocol,
        rng: AnyBCRandomNumberGenerator
    ) throws(BCComponentsError) -> Signature {
        guard let key = toSchnorr() else {
            throw BCComponentsError.crypto("Invalid key type for Schnorr signing")
        }
        let signature = BCCrypto.schnorrSign(
            key.data,
            Data(message),
            auxiliaryRandom: rng.randomData(count: 32)
        )
        return try .schnorrFromData(signature)
    }

    private func ed25519Sign(
        _ message: some DataProtocol
    ) throws(BCComponentsError) -> Signature {
        guard let key = toEd25519() else {
            throw BCComponentsError.crypto("Invalid key type for Ed25519 signing")
        }
        return try .ed25519FromData(key.sign(message))
    }

    private func mldsaSign(
        _ message: some DataProtocol
    ) throws(BCComponentsError) -> Signature {
        guard let key = toMLDSA() else {
            throw BCComponentsError.postQuantum("Invalid key type for MLDSA signing")
        }
        return .mldsa(key.sign(message))
    }
}

extension SigningPrivateKey: Signer {
    public func signWithOptions(
        _ message: some DataProtocol,
        options: SigningOptions?
    ) throws(BCComponentsError) -> Signature {
        switch self {
        case .schnorr:
            if case .some(.schnorr(let rng)) = options {
                return try schnorrSign(message, rng: rng)
            }
            return try schnorrSign(
                message,
                rng: AnyBCRandomNumberGenerator(SecureRandomNumberGenerator())
            )
        case .ecdsa:
            return try ecdsaSign(message)
        case .ed25519:
            return try ed25519Sign(message)
        case .mldsa:
            return try mldsaSign(message)
        }
    }
}

extension SigningPrivateKey: Verifier {
    public func verify(_ signature: Signature, _ message: some DataProtocol) -> Bool {
        switch self {
        case .schnorr(let key):
            guard case .schnorr(let schnorrSignature) = signature else {
                return false
            }
            return key.schnorrPublicKey().schnorrVerify(schnorrSignature, message)
        case .ecdsa, .ed25519, .mldsa:
            return false
        }
    }
}

extension SigningPrivateKey: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.signingPrivateKey]
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

extension SigningPrivateKey: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        switch untaggedCBOR {
        case .bytes(let bytes):
            self = .schnorr(try ECPrivateKey(bytes))
        case .array(let elements):
            guard elements.count == 2 else {
                throw BCComponentsError.invalidData(
                    dataType: "signing private key",
                    reason: "invalid signing private key format"
                )
            }
            guard case .unsigned(let discriminator) = elements[0] else {
                throw BCComponentsError.invalidData(
                    dataType: "signing private key",
                    reason: "invalid signing private key discriminator"
                )
            }
            let bytes = try byteString(elements[1])
            switch discriminator {
            case 1:
                self = .ecdsa(try ECPrivateKey(bytes))
            case 2:
                self = .ed25519(try Ed25519PrivateKey(bytes))
            default:
                throw BCComponentsError.invalidData(
                    dataType: "signing private key",
                    reason: "invalid signing private key discriminator"
                )
            }
        case .tagged(let tag, _):
            switch tag {
            case .mldsaPrivateKey:
                self = .mldsa(try MLDSAPrivateKey(cbor: untaggedCBOR))
            default:
                throw BCComponentsError.invalidData(
                    dataType: "signing private key",
                    reason: "invalid signing private key format"
                )
            }
        default:
            throw BCComponentsError.invalidData(
                dataType: "signing private key",
                reason: "invalid signing private key format"
            )
        }
    }
}

extension SigningPrivateKey: URCodable {}

extension SigningPrivateKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension SigningPrivateKey: CustomDebugStringConvertible {
    public var debugDescription: String {
        "SigningPrivateKey"
    }
}

extension SigningPrivateKey: CustomStringConvertible {
    public var description: String {
        let displayKey: String
        switch self {
        case .schnorr(let key):
            displayKey = "SchnorrPrivateKey(\(key.refHexShort()))"
        case .ecdsa(let key):
            displayKey = "ECDSAPrivateKey(\(key.refHexShort()))"
        case .ed25519(let key):
            displayKey = key.description
        case .mldsa(let key):
            displayKey = key.description
        }
        return "SigningPrivateKey(\(refHexShort()), \(displayKey))"
    }
}

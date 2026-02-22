import BCCrypto
import BCTags
import BCUR
import DCBOR
import Foundation

public enum Signature: Equatable, Sendable {
    case schnorr(Data)
    case ecdsa(Data)
    case ed25519(Data)
    case mldsa(MLDSASignature)

    public static func schnorrFromData(_ data: Data) throws(BCComponentsError) -> Signature {
        try requireLength(data, expected: schnorrSignatureSize, name: "Schnorr signature")
        return .schnorr(data)
    }

    public static func schnorrFromDataRef(
        _ data: some DataProtocol
    ) throws(BCComponentsError) -> Signature {
        try schnorrFromData(Data(data))
    }

    public static func ecdsaFromData(_ data: Data) throws(BCComponentsError) -> Signature {
        try requireLength(data, expected: ecdsaSignatureSize, name: "ECDSA signature")
        return .ecdsa(data)
    }

    public static func ecdsaFromDataRef(
        _ data: some DataProtocol
    ) throws(BCComponentsError) -> Signature {
        try ecdsaFromData(Data(data))
    }

    public static func ed25519FromData(_ data: Data) throws(BCComponentsError) -> Signature {
        try requireLength(data, expected: ed25519SignatureSize, name: "Ed25519 signature")
        return .ed25519(data)
    }

    public static func ed25519FromDataRef(
        _ data: some DataProtocol
    ) throws(BCComponentsError) -> Signature {
        try ed25519FromData(Data(data))
    }

    public func toSchnorr() -> Data? {
        if case .schnorr(let signature) = self {
            return signature
        }
        return nil
    }

    public func toEcdsa() -> Data? {
        if case .ecdsa(let signature) = self {
            return signature
        }
        return nil
    }

    public func toEd25519() -> Data? {
        if case .ed25519(let signature) = self {
            return signature
        }
        return nil
    }

    public func toMLDSA() -> MLDSASignature? {
        if case .mldsa(let signature) = self {
            return signature
        }
        return nil
    }

    public func scheme() -> SignatureScheme {
        switch self {
        case .schnorr:
            return .schnorr
        case .ecdsa:
            return .ecdsa
        case .ed25519:
            return .ed25519
        case .mldsa(let signature):
            switch signature.level() {
            case .mldsa44:
                return .mldsa44
            case .mldsa65:
                return .mldsa65
            case .mldsa87:
                return .mldsa87
            }
        }
    }
}

extension Signature: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.signature]
    }

    public var untaggedCBOR: CBOR {
        switch self {
        case .schnorr(let signature):
            return .bytes(signature)
        case .ecdsa(let signature):
            return .array([.unsigned(1), .bytes(signature)])
        case .ed25519(let signature):
            return .array([.unsigned(2), .bytes(signature)])
        case .mldsa(let signature):
            return signature.cbor
        }
    }
}

extension Signature: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        switch untaggedCBOR {
        case .bytes(let bytes):
            self = try .schnorrFromData(bytes)
        case .array(let elements):
            guard elements.count == 2 else {
                throw BCComponentsError.invalidData(
                    dataType: "signature",
                    reason: "invalid signature format"
                )
            }

            guard case .unsigned(let discriminator) = elements[0] else {
                throw BCComponentsError.invalidData(
                    dataType: "signature",
                    reason: "invalid signature discriminator"
                )
            }

            let bytes = try byteString(elements[1])
            switch discriminator {
            case 1:
                self = try .ecdsaFromData(bytes)
            case 2:
                self = try .ed25519FromData(bytes)
            default:
                throw BCComponentsError.invalidData(
                    dataType: "signature",
                    reason: "invalid signature discriminator"
                )
            }
        case .tagged(let tag, _):
            switch tag {
            case .mldsaSignature:
                self = .mldsa(try MLDSASignature(cbor: untaggedCBOR))
            default:
                throw BCComponentsError.invalidData(
                    dataType: "signature",
                    reason: "invalid signature format"
                )
            }
        default:
            throw BCComponentsError.invalidData(
                dataType: "signature",
                reason: "invalid signature format"
            )
        }
    }
}

extension Signature: URCodable {}

extension Signature: CustomDebugStringConvertible {
    public var debugDescription: String {
        switch self {
        case .schnorr(let signature):
            return "Schnorr(data: \(hexEncode(signature)))"
        case .ecdsa(let signature):
            return "ECDSA(data: \(hexEncode(signature)))"
        case .ed25519(let signature):
            return "Ed25519(data: \(hexEncode(signature)))"
        case .mldsa(let signature):
            return "MLDSA(data: \(hexEncode(signature.asBytes())))"
        }
    }
}

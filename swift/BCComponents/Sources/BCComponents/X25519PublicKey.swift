import BCCrypto
import BCUR
import BCTags
import DCBOR
import Foundation

public struct X25519PublicKey: Equatable, Hashable, Sendable {
    public static let keySize = 32

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.keySize, name: "X25519 public key")
        self.value = value
    }

    public static func fromData(_ value: Data) throws(BCComponentsError) -> X25519PublicKey {
        try X25519PublicKey(value)
    }

    public static func fromHex(_ hex: String) throws(BCComponentsError) -> X25519PublicKey {
        try X25519PublicKey(parseHex(hex))
    }

    public var data: Data {
        value
    }

    public func asBytes() -> Data {
        value
    }

    public func hex() -> String {
        hexEncode(value)
    }
}

extension X25519PublicKey: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(Digest.fromImage(taggedCBOR.cborData))
    }
}

extension X25519PublicKey: CustomStringConvertible {
    public var description: String {
        "X25519PublicKey(\(refHexShort()))"
    }
}

extension X25519PublicKey: CustomDebugStringConvertible {
    public var debugDescription: String {
        "X25519PublicKey(\(hex()))"
    }
}

extension X25519PublicKey: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.x25519PublicKey]
    }

    public var untaggedCBOR: CBOR {
        .bytes(value)
    }
}

extension X25519PublicKey: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        try self.init(byteString(untaggedCBOR))
    }
}

extension X25519PublicKey: URCodable {}

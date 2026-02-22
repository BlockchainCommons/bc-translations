import BCUR
import BCTags
import BCRand
import DCBOR
import Foundation

public struct Nonce: Equatable, Sendable {
    public static let nonceSize = 12

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.nonceSize, name: "nonce")
        self.value = value
    }

    public static func new() -> Nonce {
        try! Nonce(randomData(count: Self.nonceSize))
    }

    public static func fromData(_ value: Data) throws(BCComponentsError) -> Nonce {
        try Nonce(value)
    }

    public static func fromHex(_ hex: String) throws(BCComponentsError) -> Nonce {
        try Nonce(parseHex(hex))
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

extension Nonce: CustomStringConvertible {
    public var description: String {
        "Nonce(\(hex()))"
    }
}

extension Nonce: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.nonce]
    }

    public var untaggedCBOR: CBOR {
        .bytes(value)
    }
}

extension Nonce: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        try self.init(byteString(untaggedCBOR))
    }
}

extension Nonce: URCodable {}

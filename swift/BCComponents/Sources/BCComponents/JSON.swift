import BCUR
import BCTags
import DCBOR
import Foundation

public struct JSON: Equatable, Sendable {
    private let bytes: Data

    public init(_ bytes: Data) {
        self.bytes = bytes
    }

    public static func fromData(_ data: Data) -> JSON {
        JSON(data)
    }

    public static func fromString(_ string: String) -> JSON {
        JSON(Data(string.utf8))
    }

    public static func fromHex(_ hex: String) throws(BCComponentsError) -> JSON {
        JSON(try parseHex(hex))
    }

    public var count: Int {
        bytes.count
    }

    public func len() -> Int {
        bytes.count
    }

    public func isEmpty() -> Bool {
        bytes.isEmpty
    }

    public func asBytes() -> Data {
        bytes
    }

    public func asString() -> String {
        String(data: bytes, encoding: .utf8)!
    }

    public func hex() -> String {
        hexEncode(bytes)
    }
}

extension JSON: CustomStringConvertible {
    public var description: String {
        "JSON(\(asString()))"
    }
}

extension JSON: CustomDebugStringConvertible {
    public var debugDescription: String {
        description
    }
}

extension JSON: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [Tag(262, "json")]
    }

    public var untaggedCBOR: CBOR {
        .bytes(bytes)
    }
}

extension JSON: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        self.init(try byteString(untaggedCBOR))
    }
}

extension JSON: URCodable {}

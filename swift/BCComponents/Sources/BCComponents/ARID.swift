import BCUR
import BCTags
import DCBOR
import BCRand
import Foundation

public struct ARID: Equatable, Hashable, Comparable, Sendable {
    public static let aridSize = 32

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.aridSize, name: "ARID")
        self.value = value
    }

    public static func new() -> ARID {
        try! ARID(randomData(count: Self.aridSize))
    }

    public static func fromData(_ value: Data) throws(BCComponentsError) -> ARID {
        try ARID(value)
    }

    public static func fromHex(_ hex: String) throws(BCComponentsError) -> ARID {
        try ARID(parseHex(hex))
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

    public func shortDescription() -> String {
        hexEncode(value.prefix(4))
    }

    public static func < (lhs: ARID, rhs: ARID) -> Bool {
        lhs.value.lexicographicallyPrecedes(rhs.value)
    }
}

extension ARID: CustomStringConvertible {
    public var description: String {
        "ARID(\(hex()))"
    }
}

extension ARID: CustomDebugStringConvertible {
    public var debugDescription: String {
        description
    }
}

extension ARID: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.arid]
    }

    public var untaggedCBOR: CBOR {
        .bytes(value)
    }
}

extension ARID: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        try self.init(byteString(untaggedCBOR))
    }
}

extension ARID: URCodable {}

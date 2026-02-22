import BCUR
import BCTags
import BCRand
import DCBOR
import Foundation

public struct UUID: Equatable, Hashable, Sendable {
    public static let uuidSize = 16

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.uuidSize, name: "UUID")
        self.value = value
    }

    public init() {
        var bytes = randomData(count: Self.uuidSize)
        bytes[6] = (bytes[6] & 0x0f) | 0x40
        bytes[8] = (bytes[8] & 0x3f) | 0x80
        try! self.init(bytes)
    }

    public static func parse(_ value: String) throws(BCComponentsError) -> UUID {
        let normalized = value.trimmingCharacters(in: .whitespacesAndNewlines).replacingOccurrences(of: "-", with: "")
        return try UUID(parseHex(normalized))
    }

    public var data: Data {
        value
    }

    public var stringValue: String {
        let hex = hexEncode(value)
        return "\(hex.prefix(8))-\(hex.dropFirst(8).prefix(4))-\(hex.dropFirst(12).prefix(4))-\(hex.dropFirst(16).prefix(4))-\(hex.dropFirst(20).prefix(12))"
    }
}

extension UUID: CustomStringConvertible {
    public var description: String {
        stringValue
    }
}

extension UUID: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [Tag(37, "uuid")]
    }

    public var untaggedCBOR: CBOR {
        .bytes(value)
    }
}

extension UUID: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        try self.init(byteString(untaggedCBOR))
    }
}

extension UUID: URCodable {}

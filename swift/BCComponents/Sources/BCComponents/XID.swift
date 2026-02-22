import BCUR
import BCTags
import DCBOR
import Foundation

public protocol XIDProvider {
    func xid() -> XID
}

public struct XID: Equatable, Hashable, Comparable, Sendable {
    public static let xidSize = 32

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.xidSize, name: "XID")
        self.value = value
    }

    public static func fromHex(_ hex: String) throws(BCComponentsError) -> XID {
        try XID(parseHex(hex))
    }

    public static func new(genesisKey: some CBOREncodable) -> XID {
        let digest = Digest.fromImage(genesisKey.cborData)
        return try! XID(digest.data)
    }

    public func validate(genesisKey: some CBOREncodable) -> Bool {
        let digest = Digest.fromImage(genesisKey.cborData)
        return digest.data == value
    }

    public var data: Data {
        value
    }

    public var hex: String {
        hexEncode(value)
    }

    public var shortDescription: String {
        refHexShort()
    }

    public func bytewordsIdentifier(prefix: Bool) -> String {
        refBytewords(prefix ? "🅧" : nil)
    }

    public func bytemojiIdentifier(prefix: Bool) -> String {
        refBytemoji(prefix ? "🅧" : nil)
    }

    public static func < (lhs: XID, rhs: XID) -> Bool {
        lhs.value.lexicographicallyPrecedes(rhs.value)
    }
}

extension XID: XIDProvider {
    public func xid() -> XID {
        self
    }
}

extension XID: ReferenceProvider {
    public func reference() -> Reference {
        try! Reference(value)
    }
}

extension XID: CustomStringConvertible {
    public var description: String {
        "XID(\(shortDescription))"
    }
}

extension XID: CustomDebugStringConvertible {
    public var debugDescription: String {
        "XID(\(hex))"
    }
}

extension XID: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.xid]
    }

    public var untaggedCBOR: CBOR {
        .bytes(value)
    }
}

extension XID: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        try self.init(byteString(untaggedCBOR))
    }
}

extension XID: URCodable {}

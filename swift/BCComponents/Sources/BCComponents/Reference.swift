import BCUR
import BCTags
import DCBOR
import Foundation

public protocol ReferenceProvider {
    func reference() -> Reference

    func refHex() -> String
    func refDataShort() -> [UInt8]
    func refHexShort() -> String
    func refBytewords(_ prefix: String?) -> String
    func refBytemoji(_ prefix: String?) -> String
}

public extension ReferenceProvider {
    func refHex() -> String { reference().refHex() }

    func refDataShort() -> [UInt8] { reference().refDataShort() }

    func refHexShort() -> String { reference().refHexShort() }

    func refBytewords(_ prefix: String?) -> String {
        reference().bytewordsIdentifier(prefix)
    }

    func refBytemoji(_ prefix: String?) -> String {
        reference().bytemojiIdentifier(prefix)
    }
}

public struct Reference: Equatable, Hashable, Sendable {
    public static let referenceSize = 32

    private let value: Data

    public init(_ value: Data) throws(BCComponentsError) {
        try requireLength(value, expected: Self.referenceSize, name: "reference")
        self.value = value
    }

    public static func fromDigest(_ digest: Digest) -> Reference {
        try! Reference(digest.data)
    }

    public static func fromHex(_ hex: String) throws(BCComponentsError) -> Reference {
        try Reference(parseHex(hex))
    }

    public var data: Data {
        value
    }

    public func refHex() -> String {
        hexEncode(value)
    }

    public func refDataShort() -> [UInt8] {
        Array(value.prefix(4))
    }

    public func refHexShort() -> String {
        hexEncode(value.prefix(4))
    }

    public func bytewordsIdentifier(_ prefix: String?) -> String {
        asUppercaseIdentifier(refDataShort(), prefix: prefix)
    }

    public func bytemojiIdentifier(_ prefix: String?) -> String {
        asUppercaseBytemoji(refDataShort(), prefix: prefix)
    }
}

extension Reference: ReferenceProvider {
    public func reference() -> Reference {
        Reference.fromDigest(digest())
    }
}

extension Reference: DigestProvider {
    public func digest() -> Digest {
        Digest.fromImage(taggedCBOR.cborData)
    }
}

extension Reference: CustomStringConvertible {
    public var description: String {
        "Reference(\(refHexShort()))"
    }
}

extension Reference: CustomDebugStringConvertible {
    public var debugDescription: String {
        "Reference(\(refHex()))"
    }
}

extension Reference: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.reference]
    }

    public var untaggedCBOR: CBOR {
        .bytes(value)
    }
}

extension Reference: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        try self.init(byteString(untaggedCBOR))
    }
}

extension Reference: URCodable {}

import BCUR
import BCTags
import DCBOR
import Foundation

public struct URI: Equatable, Hashable, Sendable {
    private let value: String

    public init(_ value: String) throws(BCComponentsError) {
        guard URL(string: value) != nil else {
            throw .invalidData(dataType: "URI", reason: "invalid URI format")
        }
        self.value = value
    }

    public var string: String {
        value
    }
}

extension URI: CustomStringConvertible {
    public var description: String {
        value
    }
}

extension URI: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [Tag(32, "url")]
    }

    public var untaggedCBOR: CBOR {
        .text(value)
    }
}

extension URI: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        try self.init(textString(untaggedCBOR))
    }
}

extension URI: URCodable {}

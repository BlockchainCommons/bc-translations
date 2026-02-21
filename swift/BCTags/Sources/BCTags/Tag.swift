// BCTags — Blockchain Commons CBOR Tags
//
// Provides the `Tag` type and a tag registry (`TagsStore`) for CBOR tag
// value/name mappings used across Blockchain Commons libraries. Also defines
// all standard Blockchain Commons tag constants and a registration function
// to populate a store with the full set.

/// A CBOR tag.
public struct Tag: Sendable {
    /// The tag's numeric value.
    public let value: UInt64

    /// The tag's known names, if any.
    ///
    /// If the array contains more than one element, the first is the preferred name.
    public let names: [String]

    /// The tag's preferred name, if any.
    public var name: String? {
        names.first
    }

    /// Creates a tag with the given value and an array of known names.
    ///
    /// If the array contains more than one element, the first is the preferred name.
    public init(_ value: UInt64, _ names: [String]) {
        self.value = value
        self.names = names
    }

    /// Creates a tag with the given value and optionally a single known name.
    public init(_ value: UInt64, _ name: String? = nil) {
        if let name {
            self.init(value, [name])
        } else {
            self.init(value, [])
        }
    }
}

extension Tag: Hashable {
    public static func ==(lhs: Tag, rhs: Tag) -> Bool {
        lhs.value == rhs.value
    }

    public func hash(into hasher: inout Hasher) {
        hasher.combine(value)
    }
}

extension Tag: ExpressibleByIntegerLiteral {
    public init(integerLiteral value: IntegerLiteralType) {
        self.init(UInt64(value), nil)
    }
}

extension Tag: CustomStringConvertible {
    public var description: String {
        name ?? String(value)
    }
}

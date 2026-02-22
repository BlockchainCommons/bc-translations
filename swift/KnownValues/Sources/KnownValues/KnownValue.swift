import BCComponents
import BCTags
import DCBOR
import Foundation

/// A value in a namespace of unsigned integers that represents a stand-alone
/// ontological concept.
///
/// Known Values provide a compact, deterministic way to represent commonly used
/// ontological concepts such as relationships between entities, classes of
/// entities, properties, or enumerated values. They are particularly useful as
/// predicates in Gordian Envelope assertions, offering a more compact and
/// deterministic alternative to URIs.
///
/// A Known Value is represented as a 64-bit unsigned integer with an optional
/// human-readable name. This approach ensures:
///
/// - **Compact binary representation** — Each Known Value requires only 1–9
///   bytes depending on value range
/// - **Deterministic encoding** — Every concept has exactly one valid binary
///   representation
/// - **Enhanced security** — Eliminates URI manipulation vulnerabilities
/// - **Standardized semantics** — Values are registered in a central registry
public struct KnownValue: Sendable {
    /// The known value as coded into CBOR.
    public let value: UInt64

    /// A name assigned to the known value used for debugging and formatted
    /// output.
    private let _assignedName: String?

    /// Creates a new KnownValue with the given numeric value and no name.
    public init(_ value: UInt64) {
        self.value = value
        self._assignedName = nil
    }

    /// Creates a KnownValue with the given value and associated name.
    public init(value: UInt64, name: String) {
        self.value = value
        self._assignedName = name
    }

    /// Returns the assigned name of the KnownValue, if one exists.
    public var assignedName: String? {
        _assignedName
    }

    /// Returns a human-readable name for the KnownValue.
    ///
    /// If the KnownValue has an assigned name, that name is returned.
    /// Otherwise, the string representation of the numeric value is returned.
    public var name: String {
        _assignedName ?? String(value)
    }
}

// MARK: - Equatable (based solely on numeric value)

extension KnownValue: Equatable {
    public static func == (lhs: KnownValue, rhs: KnownValue) -> Bool {
        lhs.value == rhs.value
    }
}

// MARK: - Hashable (based solely on numeric value)

extension KnownValue: Hashable {
    public func hash(into hasher: inout Hasher) {
        hasher.combine(value)
    }
}

// MARK: - Comparable (based solely on numeric value)

extension KnownValue: Comparable {
    public static func < (lhs: KnownValue, rhs: KnownValue) -> Bool {
        lhs.value < rhs.value
    }
}

// MARK: - CustomStringConvertible

extension KnownValue: CustomStringConvertible {
    public var description: String {
        name
    }
}

// MARK: - ExpressibleByIntegerLiteral

extension KnownValue: ExpressibleByIntegerLiteral {
    public init(integerLiteral value: UInt64) {
        self.init(value)
    }
}

// MARK: - DigestProvider

extension KnownValue: DigestProvider {
    public func digest() -> Digest {
        Digest.fromImage(taggedCBOR.cborData)
    }
}

// MARK: - CBOR Tagged Encoding/Decoding

extension KnownValue: CBORTaggedCodable {
    public static var cborTags: [Tag] {
        [.knownValue]
    }

    public var untaggedCBOR: CBOR {
        value.cbor
    }

    public init(untaggedCBOR cbor: CBOR) throws {
        let value = try UInt64(cbor: cbor)
        self.init(value)
    }
}

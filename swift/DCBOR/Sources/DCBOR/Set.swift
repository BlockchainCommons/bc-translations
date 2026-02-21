import Foundation

/// A deterministic CBOR set represented as a map of value->value.
///
/// Values are ordered by their encoded CBOR bytes to preserve deterministic
/// behavior across platforms.
public struct Set: Sendable, Equatable {
    private var map: Map

    public init() {
        self.map = Map()
    }

    /// Number of unique values in the set.
    public var count: Int {
        map.count
    }

    /// True when the set has no values.
    public var isEmpty: Bool {
        count == 0
    }

    /// Inserts a value into the set.
    public mutating func insert<T>(_ value: T) where T: CBOREncodable {
        map.insert(value, value)
    }

    /// Returns true if the set contains the value.
    public func contains<T>(_ value: T) -> Bool where T: CBOREncodable {
        map.get(value) != nil
    }

    /// Returns the deterministic CBOR-ordered values.
    public func asArray() -> [CBOR] {
        map.map { _, value in value }
    }

    /// Builds a set from arbitrary values (duplicates collapse naturally).
    public static func fromArray<T>(_ values: [T]) -> Set where T: CBOREncodable {
        var result = Set()
        for value in values {
            result.insert(value)
        }
        return result
    }

    /// Builds a set from values that are already in canonical CBOR order.
    /// Throws for duplicate or misordered input.
    public static func tryFromArray<T>(_ values: [T]) throws -> Set where T: CBOREncodable {
        var result = Set()
        for value in values {
            try result.insertNext(value.cbor)
        }
        return result
    }

    mutating func insertNext(_ value: CBOR) throws {
        try map.insertNext(value, value)
    }
}

extension Set: Sequence {
    public struct Iterator: IteratorProtocol {
        private var iterator: Map.Iterator

        init(_ map: Map) {
            self.iterator = map.makeIterator()
        }

        public mutating func next() -> CBOR? {
            iterator.next()?.1
        }
    }

    public func makeIterator() -> Iterator {
        Iterator(map)
    }
}

extension Set: CBORCodable {
    public var cbor: CBOR {
        asArray().cbor
    }

    public var cborData: Data {
        asArray().cborData
    }

    public init(cbor: CBOR) throws {
        let array = try [CBOR](cbor: cbor)
        self = try Set.tryFromArray(array)
    }
}

extension Set: CustomDebugStringConvertible {
    public var debugDescription: String {
        asArray().debugDescription
    }
}

extension Set: CustomStringConvertible {
    public var description: String {
        asArray().description
    }
}

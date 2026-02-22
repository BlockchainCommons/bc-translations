import BCUR
import BCTags
import BCRand
import DCBOR
import Foundation

public struct Salt: Equatable, Sendable {
    private let value: Data

    public init(_ value: Data) {
        self.value = value
    }

    public var count: Int {
        value.count
    }

    public var isEmpty: Bool {
        value.isEmpty
    }

    public var data: Data {
        value
    }

    public static func newWithLen(_ count: Int) throws(BCComponentsError) -> Salt {
        var rng = SecureRandomNumberGenerator()
        return try newWithLenUsing(count, rng: &rng)
    }

    public static func newWithLenUsing<G: BCRandomNumberGenerator>(
        _ count: Int,
        rng: inout G
    ) throws(BCComponentsError) -> Salt {
        if count < 8 {
            throw .dataTooShort(dataType: "salt", minimum: 8, actual: count)
        }
        return Salt(rng.randomData(count: count))
    }

    public static func newInRange(_ range: ClosedRange<Int>) throws(BCComponentsError) -> Salt {
        if range.lowerBound < 8 {
            throw .dataTooShort(dataType: "salt", minimum: 8, actual: range.lowerBound)
        }
        var rng = SecureRandomNumberGenerator()
        return try newInRangeUsing(range, rng: &rng)
    }

    public static func newInRangeUsing<G: BCRandomNumberGenerator>(
        _ range: ClosedRange<Int>,
        rng: inout G
    ) throws(BCComponentsError) -> Salt {
        if range.lowerBound < 8 {
            throw .dataTooShort(dataType: "salt", minimum: 8, actual: range.lowerBound)
        }
        let count = rngNextInClosedRange(&rng, range: range)
        return try newWithLenUsing(count, rng: &rng)
    }

    public static func newForSize(_ size: Int) -> Salt {
        var rng = SecureRandomNumberGenerator()
        return newForSizeUsing(size, rng: &rng)
    }

    public static func newForSizeUsing<G: BCRandomNumberGenerator>(
        _ size: Int,
        rng: inout G
    ) -> Salt {
        let f = Double(size)
        let minSize = max(8, Int(ceil(f * 0.05)))
        let maxSize = max(minSize + 8, Int(ceil(f * 0.25)))
        return try! newInRangeUsing(minSize...maxSize, rng: &rng)
    }

    public static func fromHex(_ hex: String) throws(BCComponentsError) -> Salt {
        Salt(try parseHex(hex))
    }

    public var hex: String {
        hexEncode(value)
    }
}

extension Salt: CustomStringConvertible {
    public var description: String {
        "Salt(\(count))"
    }
}

extension Salt: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [.salt]
    }

    public var untaggedCBOR: CBOR {
        .bytes(value)
    }
}

extension Salt: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        self.init(try byteString(untaggedCBOR))
    }
}

extension Salt: URCodable {}

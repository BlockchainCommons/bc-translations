/// Utility functions for fountain encoding/decoding.
internal enum FountainUtils {
    static func fragmentLength(dataLength: Int, maxFragmentLength: Int) -> Int {
        let fragmentCount = divCeil(dataLength, maxFragmentLength)
        return divCeil(dataLength, fragmentCount)
    }

    static func partition(_ data: [UInt8], fragmentLength: Int) -> [[UInt8]] {
        let padding = (fragmentLength - (data.count % fragmentLength)) % fragmentLength
        let padded = data + Array(repeating: 0, count: padding)

        var result: [[UInt8]] = []
        result.reserveCapacity(padded.count / fragmentLength)

        var index = 0
        while index < padded.count {
            let next = index + fragmentLength
            result.append(Array(padded[index..<next]))
            index = next
        }

        return result
    }

    static func chooseFragments(
        sequence: Int,
        fragmentCount: Int,
        checksum: UInt32
    ) -> [Int] {
        if sequence <= fragmentCount {
            return [sequence - 1]
        }

        let sequence32 = UInt32(truncatingIfNeeded: sequence)
        let seed = sequence32.bigEndianBytes + checksum.bigEndianBytes

        var xoshiro = Xoshiro256.fromBytes(seed)
        let degree = xoshiro.chooseDegree(fragmentCount)
        let indexes = Array(0..<fragmentCount)
        let shuffled = xoshiro.shuffled(indexes)

        return Array(shuffled.prefix(degree))
    }

    static func xorInPlace(_ target: inout [UInt8], with other: [UInt8]) {
        precondition(target.count == other.count)
        for index in target.indices {
            target[index] ^= other[index]
        }
    }

    private static func divCeil(_ a: Int, _ b: Int) -> Int {
        let d = a / b
        let r = a % b
        return r > 0 ? (d + 1) : d
    }
}

import CryptoKit
import Foundation

/// Internal Xoshiro256** pseudo-random number generator.
internal struct Xoshiro256: Sendable {
    private var s0: UInt64
    private var s1: UInt64
    private var s2: UInt64
    private var s3: UInt64

    private init(_ state: [UInt64]) {
        precondition(state.count == 4)
        self.s0 = state[0]
        self.s1 = state[1]
        self.s2 = state[2]
        self.s3 = state[3]
    }

    mutating func next() -> UInt64 {
        let result = (s1 &* 5).rotatedLeft(by: 7) &* 9
        let t = s1 << 17

        s2 ^= s0
        s3 ^= s1
        s1 ^= s2
        s0 ^= s3

        s2 ^= t
        s3 = s3.rotatedLeft(by: 45)

        return result
    }

    mutating func nextDouble() -> Double {
        Double(next()) / (Double(UInt64.max) + 1.0)
    }

    mutating func nextInt(low: UInt64, high: UInt64) -> UInt64 {
        let span = Double(high &- low &+ 1)
        return UInt64(nextDouble() * span) + low
    }

    mutating func shuffled<T>(_ items: [T]) -> [T] {
        var remaining = items
        var shuffled: [T] = []
        shuffled.reserveCapacity(items.count)

        while !remaining.isEmpty {
            let index = Int(nextInt(low: 0, high: UInt64(remaining.count - 1)))
            shuffled.append(remaining.remove(at: index))
        }

        return shuffled
    }

    mutating func chooseDegree(_ length: Int) -> Int {
        let weights = (1...length).map { 1.0 / Double($0) }
        var sampler = try! WeightedSampler(weights: weights)
        return sampler.next(using: &self) + 1
    }

    mutating func nextByte() -> UInt8 {
        UInt8(truncatingIfNeeded: nextInt(low: 0, high: 255))
    }

    mutating func nextBytes(_ count: Int) -> [UInt8] {
        (0..<count).map { _ in nextByte() }
    }

    static func fromString(_ seed: String) -> Xoshiro256 {
        fromBytes(Array(seed.utf8))
    }

    static func fromBytes(_ bytes: [UInt8]) -> Xoshiro256 {
        let digest = SHA256.hash(data: Data(bytes))
        return fromHash(Array(digest))
    }

    static func fromCRC(_ bytes: [UInt8]) -> Xoshiro256 {
        fromBytes(Crc32.checksum(bytes).bigEndianBytes)
    }

    static func makeMessage(seed: String, size: Int) -> [UInt8] {
        var xoshiro = Xoshiro256.fromString(seed)
        return xoshiro.nextBytes(size)
    }

    private static func fromHash(_ hash: [UInt8]) -> Xoshiro256 {
        precondition(hash.count == 32)

        var state: [UInt64] = []
        state.reserveCapacity(4)

        for i in 0..<4 {
            var value: UInt64 = 0
            for n in 0..<8 {
                value <<= 8
                value |= UInt64(hash[(8 * i) + n])
            }
            state.append(value)
        }

        return Xoshiro256(state)
    }
}

private extension UInt64 {
    func rotatedLeft(by bits: Int) -> UInt64 {
        (self << bits) | (self >> (64 - bits))
    }
}

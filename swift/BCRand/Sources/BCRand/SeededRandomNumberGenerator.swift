import Foundation

/// A 256-bit seed represented as four `UInt64` values.
public typealias Seed = (UInt64, UInt64, UInt64, UInt64)

/// A deterministic random number generator seeded with a 256-bit value.
///
/// Uses the Xoshiro256** algorithm internally. This is NOT cryptographically
/// secure and should only be used for testing purposes.
public struct SeededRandomNumberGenerator: BCRandomNumberGenerator, Sendable {
    private var rng: Xoshiro256StarStar

    /// Creates a new seeded random number generator.
    ///
    /// - Parameter seed: A 256-bit seed as four `UInt64` values. The seed
    ///   should not have obvious patterns like all zeroes.
    public init(seed: Seed) {
        self.rng = Xoshiro256StarStar(seed: seed)
    }

    public mutating func nextUInt32() -> UInt32 {
        UInt32(truncatingIfNeeded: nextUInt64())
    }

    public mutating func nextUInt64() -> UInt64 {
        rng.next()
    }

    /// Returns random bytes, generating each byte from the low 8 bits of
    /// `nextUInt64()`. This byte-by-byte approach is required for
    /// cross-platform test vector compatibility.
    public mutating func randomData(count: Int) -> Data {
        Data((0..<count).map { _ in UInt8(truncatingIfNeeded: nextUInt64()) })
    }

    public mutating func fillRandomData(_ data: inout Data) {
        for i in 0..<data.count {
            data[i] = UInt8(truncatingIfNeeded: nextUInt64())
        }
    }
}

/// Creates a seeded random number generator with a fixed seed for testing.
///
/// The returned generator uses a well-known seed and produces a deterministic
/// sequence. Use only for reproducible testing, never for security-sensitive code.
public func makeFakeRandomNumberGenerator() -> SeededRandomNumberGenerator {
    SeededRandomNumberGenerator(seed: (
        17295166580085024720,
        422929670265678780,
        5577237070365765850,
        7953171132032326923
    ))
}

/// Returns deterministic random bytes using the standard fake seed.
///
/// Convenience function equivalent to creating a fake generator and calling
/// `randomData(count:)`.
public func fakeRandomData(count: Int) -> Data {
    var rng = makeFakeRandomNumberGenerator()
    return rng.randomData(count: count)
}

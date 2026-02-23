import Foundation

/// A Xoshiro256** pseudo-random number generator.
///
/// Produces an identical byte stream given the same seed state, enabling
/// cross-language test vector compatibility.
///
/// The state is four `UInt64` values. The generator is seeded by providing
/// either a 4-element state array or a 32-byte little-endian data blob (as
/// produced by the ``data`` property).
public struct Xoshiro256StarStar: Sendable, Equatable {
    /// The four-word internal state.
    private var _state: [UInt64]

    // MARK: - Initializers

    /// Creates a generator from a four-element state array.
    public init(state: [UInt64]) {
        precondition(state.count == 4, "Xoshiro256StarStar state must have 4 elements")
        self._state = state
    }

    /// Creates a generator from a 32-byte little-endian representation.
    ///
    /// Each consecutive 8-byte chunk is interpreted as a little-endian `UInt64`.
    public init(data: [UInt8]) {
        precondition(data.count == 32, "Xoshiro256StarStar data must be 32 bytes")
        var state = [UInt64](repeating: 0, count: 4)
        for i in 0..<4 {
            let offset = i * 8
            var value: UInt64 = 0
            for j in 0..<8 {
                value |= UInt64(data[offset + j]) << (j * 8)
            }
            state[i] = value
        }
        self._state = state
    }

    // MARK: - State accessors

    /// The current four-word state.
    public var state: [UInt64] {
        _state
    }

    /// The state serialized as 32 little-endian bytes.
    public var data: [UInt8] {
        var bytes = [UInt8](repeating: 0, count: 32)
        for i in 0..<4 {
            let value = _state[i]
            let offset = i * 8
            for j in 0..<8 {
                bytes[offset + j] = UInt8(truncatingIfNeeded: value >> (j * 8))
            }
        }
        return bytes
    }

    // MARK: - Random generation

    /// Returns the next pseudo-random `UInt64`.
    public mutating func nextUInt64() -> UInt64 {
        let result = (_state[1] &* 5).rotatedLeft(by: 7) &* 9

        let t = _state[1] << 17

        _state[2] ^= _state[0]
        _state[3] ^= _state[1]
        _state[1] ^= _state[2]
        _state[0] ^= _state[3]

        _state[2] ^= t

        _state[3] = _state[3].rotatedLeft(by: 45)

        return result
    }

    /// Returns the next pseudo-random `UInt32` (upper 32 bits of `nextUInt64`).
    public mutating func nextUInt32() -> UInt32 {
        UInt32(truncatingIfNeeded: nextUInt64() >> 32)
    }

    /// Returns the next pseudo-random byte (low byte of `nextUInt64`).
    public mutating func nextByte() -> UInt8 {
        UInt8(truncatingIfNeeded: nextUInt64())
    }

    /// Returns a sequence of pseudo-random bytes.
    ///
    /// Each byte is produced by calling `nextByte()`, which consumes one full
    /// `UInt64` per byte for cross-platform compatibility.
    ///
    /// - Parameter count: The number of bytes to generate.
    /// - Returns: An array of `count` pseudo-random bytes.
    public mutating func nextBytes(count: Int) -> [UInt8] {
        (0..<count).map { _ in nextByte() }
    }
}

// MARK: - UInt64 rotation helper

private extension UInt64 {
    /// Rotates the value left by the specified number of bits.
    func rotatedLeft(by amount: Int) -> UInt64 {
        (self << amount) | (self >> (64 - amount))
    }
}

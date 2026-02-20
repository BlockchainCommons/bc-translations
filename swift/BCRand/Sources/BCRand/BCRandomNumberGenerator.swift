// BCRand — Blockchain Commons Random Number Utilities
//
// BCRand exposes a uniform API for the random number primitives used in
// higher-level Blockchain Commons projects, including a cryptographically
// strong generator (`SecureRandomNumberGenerator`) and a deterministic
// generator (`SeededRandomNumberGenerator`).
//
// Both generators conform to the `BCRandomNumberGenerator` protocol,
// providing a consistent interface for producing random numbers and
// byte sequences suitable for cross-platform testing.

import Foundation

/// A protocol for random number generators used by Blockchain Commons libraries.
///
/// Conforming types provide random `UInt32` and `UInt64` values, as well as
/// random byte sequences.
public protocol BCRandomNumberGenerator {
    mutating func nextUInt32() -> UInt32
    mutating func nextUInt64() -> UInt64

    /// Returns random data of the given size.
    mutating func randomData(count: Int) -> Data

    mutating func fillRandomData(_ data: inout Data)
}

extension BCRandomNumberGenerator {
    public mutating func randomData(count: Int) -> Data {
        var data = Data(count: count)
        fillRandomData(&data)
        return data
    }

    /// Default implementation that fills data 8 bytes at a time from
    /// `nextUInt64()`, using little-endian byte order.
    ///
    /// Conforming types may override this for more efficient filling.
    /// `SeededRandomNumberGenerator` overrides with byte-by-byte generation
    /// for cross-platform test vector compatibility.
    public mutating func fillRandomData(_ data: inout Data) {
        var offset = 0
        while offset < data.count {
            let value = nextUInt64()
            let remaining = data.count - offset
            let bytesToWrite = min(remaining, 8)
            for j in 0..<bytesToWrite {
                data[offset + j] = UInt8(truncatingIfNeeded: value >> (j * 8))
            }
            offset += bytesToWrite
        }
    }
}

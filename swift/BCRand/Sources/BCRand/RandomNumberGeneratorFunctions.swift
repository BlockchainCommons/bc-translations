import Foundation

/// Returns random bytes of the given size from the provided generator.
public func rngRandomData<G: BCRandomNumberGenerator>(
    _ rng: inout G,
    count: Int
) -> Data {
    rng.randomData(count: count)
}

/// Fills the given data with random bytes from the provided generator.
public func rngFillRandomData<G: BCRandomNumberGenerator>(
    _ rng: inout G,
    _ data: inout Data
) {
    rng.fillRandomData(&data)
}

/// Returns a random value less than the given upper bound using Lemire's method.
///
/// - Parameters:
///   - rng: The random number generator to use.
///   - upperBound: The exclusive upper bound. Must be non-zero.
///   - bits: The bit width of the type (8, 16, 32, or 64).
/// - Returns: A uniformly distributed random value in `0..<upperBound`.
public func rngNextWithUpperBound<G: BCRandomNumberGenerator>(
    _ rng: inout G,
    upperBound: UInt64,
    bits: Int = 64
) -> UInt64 {
    precondition(upperBound != 0)

    let bitmask: UInt64
    switch bits {
    case 8:  bitmask = UInt64(UInt8.max)
    case 16: bitmask = UInt64(UInt16.max)
    case 32: bitmask = UInt64(UInt32.max)
    case 64: bitmask = UInt64.max
    default: preconditionFailure("bits must be 8, 16, 32, or 64")
    }

    // Wide multiplication that matches Rust's per-type wide_mul.
    // For ≤32-bit types: product fits in UInt64, split at `bits` boundary.
    // For 64-bit: use full 128-bit multiplication.
    func wideMul(_ a: UInt64, _ b: UInt64) -> (lo: UInt64, hi: UInt64) {
        if bits < 64 {
            let product = a * b
            return (product & bitmask, product >> bits)
        } else {
            let result = a.multipliedFullWidth(by: b)
            return (result.low, result.high)
        }
    }

    // Threshold for rejection sampling.
    // Rust: (T::zero().wrapping_sub(&upper_bound)) % upper_bound
    func threshold() -> UInt64 {
        if bits < 64 {
            return ((1 << bits) - upperBound) % upperBound
        } else {
            return (0 &- upperBound) % upperBound
        }
    }

    // Lemire's "nearly divisionless" method
    // https://arxiv.org/abs/1805.10941
    var random = rng.nextUInt64() & bitmask
    var m = wideMul(random, upperBound)
    if m.lo < upperBound {
        let t = threshold()
        while m.lo < t {
            random = rng.nextUInt64() & bitmask
            m = wideMul(random, upperBound)
        }
    }
    return m.hi
}

/// Returns a random value within a half-open range.
///
/// - Parameters:
///   - rng: The random number generator to use.
///   - range: A half-open range `start..<end`.
/// - Returns: A uniformly distributed random value in `range`.
public func rngNextInRange<G: BCRandomNumberGenerator>(
    _ rng: inout G,
    range: Range<Int>,
    bits: Int = 64
) -> Int {
    precondition(range.lowerBound < range.upperBound)

    let bitmask: UInt64
    switch bits {
    case 8:  bitmask = UInt64(UInt8.max)
    case 16: bitmask = UInt64(UInt16.max)
    case 32: bitmask = UInt64(UInt32.max)
    case 64: bitmask = UInt64.max
    default: preconditionFailure("bits must be 8, 16, 32, or 64")
    }

    let delta = UInt64(bitPattern: Int64(range.upperBound) &- Int64(range.lowerBound)) & bitmask

    if delta == bitmask {
        return Int(bitPattern: UInt(rng.nextUInt64()))
    }

    let random = rngNextWithUpperBound(&rng, upperBound: delta, bits: bits)
    return range.lowerBound + Int(random)
}

/// Returns a random value within a closed range.
///
/// - Parameters:
///   - rng: The random number generator to use.
///   - range: A closed range `start...end`.
/// - Returns: A uniformly distributed random value in `range`.
public func rngNextInClosedRange<G: BCRandomNumberGenerator>(
    _ rng: inout G,
    range: ClosedRange<Int>,
    bits: Int = 64
) -> Int {
    precondition(range.lowerBound <= range.upperBound)

    let bitmask: UInt64
    switch bits {
    case 8:  bitmask = UInt64(UInt8.max)
    case 16: bitmask = UInt64(UInt16.max)
    case 32: bitmask = UInt64(UInt32.max)
    case 64: bitmask = UInt64.max
    default: preconditionFailure("bits must be 8, 16, 32, or 64")
    }

    let delta = UInt64(bitPattern: Int64(range.upperBound) &- Int64(range.lowerBound)) & bitmask

    if delta == bitmask {
        return Int(bitPattern: UInt(rng.nextUInt64()))
    }

    let random = rngNextWithUpperBound(&rng, upperBound: delta + 1, bits: bits)
    return range.lowerBound + Int(random)
}

/// Returns random bytes of the given count.
public func rngRandomArray<G: BCRandomNumberGenerator>(
    _ rng: inout G,
    count: Int
) -> Data {
    var data = Data(count: count)
    rng.fillRandomData(&data)
    return data
}

/// Returns a random boolean value.
public func rngRandomBool<G: BCRandomNumberGenerator>(
    _ rng: inout G
) -> Bool {
    rng.nextUInt32().isMultiple(of: 2)
}

/// Returns a random `UInt32` value.
public func rngRandomUInt32<G: BCRandomNumberGenerator>(
    _ rng: inout G
) -> UInt32 {
    rng.nextUInt32()
}

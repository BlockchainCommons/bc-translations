# Translation Manifest: bc-rand → Swift (BCRand)

Source: `rust/bc-rand/` v0.5.0
Target: `swift/BCRand/` module `BCRand`

## External Dependencies

| Rust Crate      | Swift Equivalent            | Notes                                    |
|-----------------|-----------------------------|------------------------------------------|
| rand            | `SystemRandomNumberGenerator` / Security framework | OS-level randomness via stdlib |
| rand_core       | (built-in)                  | Protocol replaces RngCore/CryptoRng      |
| getrandom       | `SecRandomCopyBytes`        | OS entropy source via Security framework |
| rand_xoshiro    | custom implementation       | Xoshiro256** must be implemented inline   |
| num-traits      | (built-in)                  | Swift fixed-width integers have methods   |
| lazy_static     | static let / actor          | Swift static initialization is lazy+thread-safe |

## Feature Flags

None. All functionality is included by default.

## Internal BC Dependencies

None. bc-rand is a root crate.

## Translation Units (in order)

### TU-1: Xoshiro256StarStar (internal)

Private implementation of the Xoshiro256** PRNG algorithm. Not part of the public API but required by SeededRandomNumberGenerator.

Source: Uses `rand_xoshiro::Xoshiro256StarStar` — must be reimplemented.

Key behaviors:
- Seeded with 32 bytes (4 × UInt64 little-endian)
- `next() -> UInt64` produces deterministic sequence
- Must match exact output for cross-platform test vectors

### TU-2: RandomNumberGenerator (Protocol)

Rust trait → Swift protocol.

Source: `random_number_generator.rs`

Public API:
- `protocol BCRandomNumberGenerator`:
  - `mutating func nextUInt32() -> UInt32`
  - `mutating func nextUInt64() -> UInt64`
  - `mutating func randomData(count: Int) -> Data`
  - `mutating func fillRandomData(_ data: inout Data)`

Note: Cannot name this `RandomNumberGenerator` as that conflicts with Swift stdlib.

### TU-3: Free functions on RandomNumberGenerator

Source: `random_number_generator.rs`

Public API:
- `rngRandomData(_:count:) -> Data`
- `rngFillRandomData(_:_:)`
- `rngNextWithUpperBound(_:upperBound:) -> T` — Lemire's method
- `rngNextInRange(_:range:) -> T` — half-open Range<T>
- `rngNextInClosedRange(_:range:) -> T` — ClosedRange<T>
- `rngRandomArray(_:count:) -> Data`
- `rngRandomBool(_:) -> Bool`
- `rngRandomUInt32(_:) -> UInt32`

Translation notes:
- Swift has `UInt8.multipliedFullWidth(by:)` for wide multiplication
- Lemire's algorithm uses Swift's fixed-width integer protocol
- Range functions accept Swift Range/ClosedRange types

### TU-4: SecureRandomNumberGenerator

Source: `secure_random.rs`

Public API:
- `struct SecureRandomNumberGenerator: BCRandomNumberGenerator`
- `randomData(count:) -> Data` (module-level convenience)
- `fillRandomData(_:)` (module-level convenience)

Translation notes:
- Use `SecRandomCopyBytes` from Security framework for cryptographic randomness
- Swift static let is inherently lazy and thread-safe
- Can also delegate to `SystemRandomNumberGenerator`

### TU-5: SeededRandomNumberGenerator

Source: `seeded_random.rs`

Public API:
- `struct SeededRandomNumberGenerator: BCRandomNumberGenerator`:
  - `init(seed: (UInt64, UInt64, UInt64, UInt64))`
  - `mutating func nextUInt64() -> UInt64`
  - `mutating func randomData(count: Int) -> Data` — byte-by-byte from `nextUInt64()`
  - `mutating func fillRandomData(_ data: inout Data)`
- `makeFakeRandomNumberGenerator() -> SeededRandomNumberGenerator`
- `fakeRandomData(count: Int) -> Data`

Critical: `randomData()` must generate byte-by-byte (each byte = `nextUInt64() & 0xFF`), not block-based. This matches the Swift reference behavior and is required for cross-platform test vector compatibility.

### TU-6: Module-level re-exports

Source: `lib.rs`

All public types and functions exported at module level (automatic in Swift).

## Test Inventory

| Test Name                  | Source File              | Type            | Vectors |
|----------------------------|--------------------------|-----------------|---------|
| test_next_u64              | seeded_random.rs         | deterministic   | First u64 = 1104683000648959614 |
| test_next_50               | seeded_random.rs         | deterministic   | 50 exact u64 values |
| test_fake_random_data      | seeded_random.rs         | deterministic   | 100 bytes hex |
| test_next_with_upper_bound | seeded_random.rs         | deterministic   | upper=10000 → 745 (bits=32) |
| test_in_range              | seeded_random.rs         | deterministic   | 100 values in [0,100) |
| test_fill_random_data      | seeded_random.rs         | equivalence     | random_data == fill_random_data |
| test_fake_numbers          | random_number_generator.rs | deterministic | 100 values in [-50,50] |
| test_random_data           | secure_random.rs         | non-deterministic | 3 × 32 bytes all different |

## Translation Hazards

1. **Xoshiro256StarStar reimplementation**: No Swift package provides this algorithm. Must implement inline with exact state transitions. Test against `test_next_50` vector.

2. **Protocol naming conflict**: Swift stdlib has `RandomNumberGenerator`. Use `BCRandomNumberGenerator` to avoid collision.

3. **Wide multiplication**: Swift's `multipliedFullWidth(by:)` on `FixedWidthInteger` provides native wide multiplication for Lemire's method.

4. **Byte-by-byte random_data**: SeededRandomNumberGenerator.randomData() generates each byte as `UInt8(nextUInt64() & 0xFF)`, NOT via fill_bytes. This is critical for cross-platform compatibility.

5. **Thread safety for SecureRandomNumberGenerator**: Swift static let provides lazy thread-safe initialization. `SecRandomCopyBytes` is already thread-safe.

6. **Swift concurrency**: Swift 6 has strict concurrency checking. SecureRandomNumberGenerator should be `Sendable`. SeededRandomNumberGenerator is a value type (struct) so it's automatically `Sendable`.

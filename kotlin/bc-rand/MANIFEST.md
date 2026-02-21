# Translation Manifest: bc-rand

Source: `rust/bc-rand/` v0.5.0
Target: `kotlin/bc-rand/` package `com.blockchaincommons.bcrand`

## External Dependencies

| Rust Crate      | Kotlin Equivalent            | Notes                                    |
|-----------------|------------------------------|------------------------------------------|
| rand            | `java.security.SecureRandom` | OS-level randomness via JDK              |
| rand_core       | (built-in)                   | Abstract class replaces RngCore/CryptoRng|
| getrandom       | `java.security.SecureRandom` | OS entropy source via JDK                |
| rand_xoshiro    | custom implementation        | Xoshiro256** must be implemented inline  |
| num-traits      | (built-in)                   | Kotlin has standard unsigned types       |
| lazy_static     | top-level `val`              | Kotlin top-level vals are lazily init'd  |

## Feature Flags

None. All functionality is included by default.

## Internal BC Dependencies

None. bc-rand is a root crate.

## Translation Units (in order)

### TU-1: Xoshiro256StarStar (internal)

Private implementation of the Xoshiro256** PRNG algorithm. Not part of the public API but required by SeededRandomNumberGenerator.

Source: Uses `rand_xoshiro::Xoshiro256StarStar` — must be reimplemented.

Key behaviors:
- Seeded with 4 × ULong values
- `nextU64()` produces deterministic sequence
- Must match exact output for cross-platform test vectors

### TU-2: RandomNumberGenerator (abstract class)

Rust trait → Kotlin abstract class.

Source: `random_number_generator.rs`

Public API:
- `abstract class RandomNumberGenerator` with:
  - `nextU32(): UInt` (abstract)
  - `nextU64(): ULong` (abstract)
  - `randomData(size: Int): ByteArray`
  - `fillRandomData(data: ByteArray)`

### TU-3: Free functions on RandomNumberGenerator

Source: `random_number_generator.rs`

Public API:
- `rngRandomData(rng, size): ByteArray`
- `rngFillRandomData(rng, data)`
- `rngNextWithUpperBound(rng, upperBound, bits): ULong` — Lemire's method
- `rngNextInRange(rng, start, end, bits): Long` — half-open [start, end)
- `rngNextInClosedRange(rng, start, end, bits): Long` — closed [start, end]
- `rngRandomArray(rng, size): ByteArray` — alias for rngRandomData
- `rngRandomBool(rng): Boolean`
- `rngRandomU32(rng): UInt`

Translation notes:
- Kotlin ULong handles unsigned 64-bit arithmetic natively
- Wide multiplication uses `java.math.BigInteger` for 64-bit width
- Lemire's algorithm uses a `bits` parameter to control type width
- Range functions accept Long parameters with `bits` for width semantics

### TU-4: SecureRandomNumberGenerator

Source: `secure_random.rs`

Public API:
- `class SecureRandomNumberGenerator : RandomNumberGenerator()`
- `randomData(size): ByteArray` (top-level convenience)
- `fillRandomData(data: ByteArray)` (top-level convenience)

Translation notes:
- Use `java.security.SecureRandom` for cryptographic randomness
- `SecureRandom` is thread-safe by contract

### TU-5: SeededRandomNumberGenerator

Source: `seeded_random.rs`

Public API:
- `class SeededRandomNumberGenerator(seed: ULongArray) : RandomNumberGenerator()`:
  - `nextU64(): ULong`
  - `randomData(size: Int): ByteArray` — byte-by-byte from `nextU64()`
  - `fillRandomData(data: ByteArray)`
- `fakeRandomNumberGenerator(): SeededRandomNumberGenerator`
- `fakeRandomBytes(size: Int): ByteArray`

Critical: `randomData()` must generate byte-by-byte (each byte = `nextU64() and 0xFF`), not block-based. This matches Swift behavior and is required for cross-platform test vector compatibility.

### TU-6: Package-level exports

Source: `lib.rs`

Public API (all accessible at package level `com.blockchaincommons.bcrand`):
- `RandomNumberGenerator`
- `SecureRandomNumberGenerator`
- `SeededRandomNumberGenerator`
- All free functions from TU-3, TU-4, TU-5

## Test Inventory

| Test Name                  | Source File              | Type        | Vectors |
|----------------------------|--------------------------|-------------|---------|
| test_next_u64              | seeded_random.rs         | deterministic | First u64 = 1104683000648959614 |
| test_next_50               | seeded_random.rs         | deterministic | 50 exact u64 values |
| test_fake_random_data      | seeded_random.rs         | deterministic | 100 bytes hex |
| test_next_with_upper_bound | seeded_random.rs         | deterministic | upper=10000 → 745 |
| test_in_range              | seeded_random.rs         | deterministic | 100 values in [0,100) |
| test_fill_random_data      | seeded_random.rs         | equivalence   | random_data == fill_random_data |
| test_fake_numbers          | random_number_generator.rs | deterministic | 100 values in [-50,50] |
| test_random_data           | secure_random.rs         | non-deterministic | 3 × 32 bytes all different |

## Translation Hazards

1. **Xoshiro256StarStar reimplementation**: No Kotlin/JVM package provides this algorithm. Must implement inline with exact state transitions. Test against `test_next_50` vector.

2. **64-bit unsigned arithmetic**: Kotlin `ULong` handles wrapping arithmetic natively. XOR, shift, and multiplication all wrap correctly.

3. **Lemire's method**: Wide multiplication of ULong × ULong requires 128-bit intermediate. Use `java.math.BigInteger` for the 64-bit case; for 32-bit and smaller, the product fits in ULong.

4. **Byte-by-byte random_data**: SeededRandomNumberGenerator.randomData() generates each byte as `nextU64() and 0xFF`, NOT via fillBytes. This is critical for cross-platform compatibility.

5. **Thread safety for SecureRandomNumberGenerator**: `java.security.SecureRandom` is thread-safe by contract; no additional synchronization needed.

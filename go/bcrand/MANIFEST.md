# Translation Manifest: bc-rand → Go (bcrand)

Source: `rust/bc-rand/` v0.5.0
Target: `go/bcrand/` package `bcrand`

## External Dependencies

| Rust Crate      | Go Equivalent            | Notes                                    |
|-----------------|--------------------------|------------------------------------------|
| rand            | `crypto/rand`            | OS-level randomness via stdlib           |
| rand_core       | (interface)              | Go interface replaces RngCore/CryptoRng  |
| getrandom       | `crypto/rand`            | OS entropy source                        |
| rand_xoshiro    | custom implementation    | Xoshiro256** must be implemented inline  |
| num-traits      | `math/bits`              | Mul64 for 128-bit wide multiplication    |
| lazy_static     | (not needed)             | Go package init + crypto/rand suffice    |

## Feature Flags

None. All functionality is included by default.

## Internal BC Dependencies

None. bc-rand is a root crate.

## Translation Units (in order)

### TU-1: xoshiro256StarStar (internal)

Private implementation of the Xoshiro256** PRNG algorithm. Not part of the public API but required by SeededRandomNumberGenerator.

Source: Uses `rand_xoshiro::Xoshiro256StarStar` — must be reimplemented.

Key behaviors:
- Seeded with 4 × uint64
- `nextU64()` produces deterministic sequence
- Must match exact output for cross-platform test vectors

### TU-2: RandomNumberGenerator (interface)

Rust trait → Go interface.

Source: `random_number_generator.rs`

Public API:
- `type RandomNumberGenerator interface` with:
  - `NextU32() uint32`
  - `NextU64() uint64`
  - `RandomData(size int) []byte`
  - `FillRandomData(data []byte)`

### TU-3: Free functions on RandomNumberGenerator

Source: `random_number_generator.rs`

Public API:
- `RngRandomData(rng, size) []byte`
- `RngFillRandomData(rng, data)`
- `RngNextWithUpperBound(rng, upperBound, bits) uint64` — Lemire's method
- `RngNextInRange(rng, start, end, bits) int64` — half-open [start, end)
- `RngNextInClosedRange(rng, start, end, bits) int64` — closed [start, end]
- `RngRandomArray(rng, size) []byte`
- `RngRandomBool(rng) bool`
- `RngRandomU32(rng) uint32`

Translation notes:
- Go lacks Rust-style generics for numeric types; use uint64 + bit-width parameter
- `wideMul` uses `math/bits.Mul64` for 64-bit, manual splitting for smaller widths
- Lemire's algorithm must still use proper bit masking for each type width
- Range functions accept start/end int64 rather than Range objects

### TU-4: SecureRandomNumberGenerator

Source: `secure_random.rs`

Public API:
- `type SecureRandomNumberGenerator struct{}` — stateless, uses crypto/rand
- `RandomData(size) []byte` (module-level convenience)
- `FillRandomData(data)` (module-level convenience)

Translation notes:
- Use `crypto/rand.Read` for cryptographic randomness
- Thread safety is inherent (crypto/rand is safe for concurrent use)
- No lazy initialization needed

### TU-5: SeededRandomNumberGenerator

Source: `seeded_random.rs`

Public API:
- `type SeededRandomNumberGenerator struct`:
  - `NewSeededRandomNumberGenerator(seed [4]uint64) *SeededRandomNumberGenerator`
  - `NextU64() uint64`
  - `RandomData(size int) []byte` — byte-by-byte from NextU64()
  - `FillRandomData(data []byte)`
- `MakeFakeRandomNumberGenerator() *SeededRandomNumberGenerator`
- `FakeRandomData(size int) []byte`

Critical: `RandomData()` must generate byte-by-byte (each byte = `NextU64() & 0xFF`), not block-based. This matches Swift behavior and is required for cross-platform test vector compatibility.

### TU-6: Package-level exports

Source: `lib.rs`

Public API (all exported at package level by Go convention):
- `RandomNumberGenerator`
- `SecureRandomNumberGenerator`
- `SeededRandomNumberGenerator`
- All free functions from TU-3, TU-4, TU-5

## Test Inventory

| Test Name                  | Source File              | Type        | Vectors |
|----------------------------|--------------------------|-------------|---------|
| TestNextU64                | seeded_random.rs         | deterministic | First u64 = 1104683000648959614 |
| TestNext50                 | seeded_random.rs         | deterministic | 50 exact u64 values |
| TestFakeRandomData         | seeded_random.rs         | deterministic | 100 bytes hex |
| TestNextWithUpperBound     | seeded_random.rs         | deterministic | upper=10000 → 745 |
| TestInRange                | seeded_random.rs         | deterministic | 100 values in [0,100) |
| TestFillRandomData         | seeded_random.rs         | equivalence   | random_data == fill_random_data |
| TestFakeNumbers            | random_number_generator.rs | deterministic | 100 values in [-50,50] |
| TestRandomData             | secure_random.rs         | non-deterministic | 3 × 32 bytes all different |

## Translation Hazards

1. **Xoshiro256StarStar reimplementation**: No Go package provides this algorithm. Must implement inline with exact state transitions. Test against `TestNext50` vector.

2. **64-bit unsigned arithmetic**: Go uint64 wraps naturally. Rotations and shifts match Rust semantics. Use `math/bits.Mul64` for 128-bit wide multiply.

3. **Lemire's method**: Use `math/bits.Mul64` for 64-bit wide multiplication. For smaller widths, promote to the next larger Go type and split. The `bits` parameter controls masking.

4. **Byte-by-byte random_data**: SeededRandomNumberGenerator.RandomData() generates each byte as `byte(NextU64())`, NOT via fill_bytes. This is critical for cross-platform compatibility.

5. **No lazy_static needed**: Go's `crypto/rand` is safe for concurrent use without explicit synchronization.

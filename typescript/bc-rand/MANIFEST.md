# Translation Manifest: bc-rand → TypeScript (@bc/rand)

Source: `rust/bc-rand/` v0.5.0
Target: `typescript/bc-rand/` package `@bc/rand`

## External Dependencies

| Rust Crate      | TypeScript Equivalent                 | Notes                                      |
|-----------------|---------------------------------------|--------------------------------------------|
| rand            | `crypto.getRandomValues`              | Web Crypto API for OS-level randomness     |
| rand_core       | (built-in)                            | Interface replaces RngCore/CryptoRng       |
| getrandom       | `crypto.getRandomValues`              | Web Crypto API                             |
| rand_xoshiro    | custom implementation                 | Xoshiro256** must be implemented inline    |
| num-traits      | (built-in)                            | BigInt provides arbitrary-precision        |
| lazy_static     | module-level singleton                | ES module init is inherently lazy          |

## Feature Flags

None. All functionality is included by default.

## Internal BC Dependencies

None. bc-rand is a root crate.

## Translation Units (in order)

### TU-1: Xoshiro256StarStar (internal)

Private implementation of the Xoshiro256** PRNG algorithm. Not part of the public API but required by SeededRandomNumberGenerator.

Source: Uses `rand_xoshiro::Xoshiro256StarStar` — must be reimplemented.

Key behaviors:
- Seeded with 32 bytes (4 × u64 little-endian)
- `nextU64()` produces deterministic sequence
- Must match exact output for cross-platform test vectors
- All arithmetic on `bigint` with `& 0xFFFFFFFFFFFFFFFFn` masking

### TU-2: RandomNumberGenerator (interface)

Rust trait → TypeScript interface.

Source: `random_number_generator.rs`

Public API:
- `interface RandomNumberGenerator` with:
  - `nextU32(): number`
  - `nextU64(): bigint`
  - `randomData(size: number): Uint8Array`
  - `fillRandomData(data: Uint8Array): void`

### TU-3: Free functions on RandomNumberGenerator

Source: `random_number_generator.rs`

Public API:
- `rngRandomData(rng, size): Uint8Array`
- `rngFillRandomData(rng, data): void`
- `rngNextWithUpperBound(rng, upperBound, bits?): bigint` — Lemire's method
- `rngNextInRange(rng, start, end): bigint` — half-open [start, end)
- `rngNextInClosedRange(rng, start, end): bigint` — closed [start, end]
- `rngRandomArray(rng, size): Uint8Array` — equivalent of const-generic array
- `rngRandomBool(rng): boolean`
- `rngRandomU32(rng): number`

Translation notes:
- Use `bigint` for all u64 values and range parameters
- Lemire's algorithm uses `bigint` multiplication; extract high/low with shifts and masks
- `bits` parameter (8, 16, 32, 64) controls masking width in Lemire's method
- Range functions accept bigint start/end

### TU-4: SecureRandomNumberGenerator

Source: `secure_random.rs`

Public API:
- `class SecureRandomNumberGenerator implements RandomNumberGenerator`
- `secureRandomData(size): Uint8Array` (module-level convenience)
- `secureFillRandomData(data): void` (module-level convenience)

Translation notes:
- Use `crypto.getRandomValues()` for cryptographic randomness (Web Crypto API)
- Already thread-safe (single-threaded JS runtime)
- No lazy initialization needed — `crypto` is globally available

### TU-5: SeededRandomNumberGenerator

Source: `seeded_random.rs`

Public API:
- `class SeededRandomNumberGenerator implements RandomNumberGenerator`:
  - `constructor(seed: [bigint, bigint, bigint, bigint])` — 4 × u64
  - `nextU64(): bigint`
  - `randomData(size): Uint8Array` — byte-by-byte from `nextU64()`
  - `fillRandomData(data): void`
- `makeFakeRandomNumberGenerator(): SeededRandomNumberGenerator`
- `fakeRandomData(size): Uint8Array`

Critical: `randomData()` must generate byte-by-byte (each byte = `nextU64() & 0xFFn`), not block-based. This matches Swift behavior and is required for cross-platform test vector compatibility.

### TU-6: Module-level re-exports

Source: `lib.rs`

Public API (re-exported from index.ts):
- `RandomNumberGenerator`
- `SecureRandomNumberGenerator`
- `SeededRandomNumberGenerator`
- All free functions from TU-3, TU-4, TU-5

## Test Inventory

| Test Name                  | Source File              | Type          | Vectors |
|----------------------------|--------------------------|---------------|---------|
| test_next_u64              | seeded_random.rs         | deterministic | First u64 = 1104683000648959614 |
| test_next_50               | seeded_random.rs         | deterministic | 50 exact u64 values |
| test_fake_random_data      | seeded_random.rs         | deterministic | 100 bytes hex |
| test_next_with_upper_bound | seeded_random.rs         | deterministic | upper=10000 → 745 |
| test_in_range              | seeded_random.rs         | deterministic | 100 values in [0,100) |
| test_fill_random_data      | seeded_random.rs         | equivalence   | random_data == fill_random_data |
| test_fake_numbers          | random_number_generator.rs | deterministic | 100 values in [-50,50] |
| test_random_data           | secure_random.rs         | non-deterministic | 3 × 32 bytes all different |

## Translation Hazards

1. **Xoshiro256StarStar reimplementation**: No npm package provides this algorithm with matching output. Must implement inline with exact state transitions. Test against `test_next_50` vector.

2. **64-bit unsigned arithmetic with bigint**: All u64 operations must use `bigint` and mask with `& 0xFFFFFFFFFFFFFFFFn`. The `nextU64()` return, rotations, shifts, and additions in Xoshiro all need masking.

3. **Lemire's method**: Wide multiplication is trivial with `bigint` (just multiply), but extraction of low/high halves must match Rust bit widths. The `bits` parameter (8, 16, 32, 64) controls masking.

4. **Byte-by-byte randomData**: SeededRandomNumberGenerator.randomData() generates each byte as `Number(nextU64() & 0xFFn)`, NOT via block fill. This is critical for cross-platform compatibility.

5. **Web Crypto API for secure randomness**: Use `crypto.getRandomValues()` which is available in Node.js 18+ and all modern browsers. Import via `globalThis.crypto` for universal compatibility.

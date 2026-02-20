# Translation Manifest: bc-rand → Python (bc-rand)

Source: `rust/bc-rand/` v0.5.0
Target: `python/bc-rand/` package `bc-rand`

## External Dependencies

| Rust Crate      | Python Equivalent         | Notes                                    |
|-----------------|---------------------------|------------------------------------------|
| rand            | `os` / `secrets`          | OS-level randomness via stdlib           |
| rand_core       | (built-in)                | ABC pattern replaces RngCore/CryptoRng   |
| getrandom       | `os.urandom`              | OS entropy source                        |
| rand_xoshiro    | custom implementation     | Xoshiro256** must be implemented inline  |
| num-traits      | (built-in)                | Python int is arbitrary-precision        |
| lazy_static     | module-level singleton    | Python module init is inherently lazy    |

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
- `next_u64()` produces deterministic sequence
- Must match exact output for cross-platform test vectors

### TU-2: RandomNumberGenerator (ABC)

Rust trait → Python ABC (Abstract Base Class).

Source: `random_number_generator.rs`

Public API:
- `class RandomNumberGenerator(ABC)` with:
  - `next_u32() -> int` (abstract)
  - `next_u64() -> int` (abstract)
  - `random_data(size: int) -> bytes`
  - `fill_random_data(data: bytearray) -> None`

### TU-3: Free functions on RandomNumberGenerator

Source: `random_number_generator.rs`

Public API:
- `rng_random_data(rng, size) -> bytes`
- `rng_fill_random_data(rng, data) -> None`
- `rng_next_with_upper_bound(rng, upper_bound) -> int` — Lemire's method
- `rng_next_in_range(rng, start, end) -> int` — half-open [start, end)
- `rng_next_in_closed_range(rng, start, end) -> int` — closed [start, end]
- `rng_random_array(rng, size) -> bytes` — equivalent of const-generic array
- `rng_random_bool(rng) -> bool`
- `rng_random_u32(rng) -> int`

Translation notes:
- Python has arbitrary-precision integers, so `wide_mul` is trivial (just `a * b`)
- The `HasMagnitude` / `Widening` traits collapse to simple Python arithmetic
- Lemire's algorithm must still use proper bit masking for each type width
- Range functions accept start/end ints rather than Range objects

### TU-4: SecureRandomNumberGenerator

Source: `secure_random.rs`

Public API:
- `class SecureRandomNumberGenerator(RandomNumberGenerator)` — singleton-like
- `random_data(size) -> bytes` (module-level convenience)
- `fill_random_data(data) -> None` (module-level convenience)

Translation notes:
- Use `os.urandom` or `secrets` for cryptographic randomness
- Thread safety via `threading.Lock`
- Lazy initialization pattern

### TU-5: SeededRandomNumberGenerator

Source: `seeded_random.rs`

Public API:
- `class SeededRandomNumberGenerator(RandomNumberGenerator)`:
  - `__init__(seed: tuple[int, int, int, int])` — 4 × u64
  - `next_u64() -> int`
  - `random_data(size) -> bytes` — byte-by-byte from `next_u64()`
  - `fill_random_data(data) -> None`
- `make_fake_random_number_generator() -> SeededRandomNumberGenerator`
- `fake_random_data(size) -> bytes`

Critical: `random_data()` must generate byte-by-byte (each byte = `next_u64() & 0xFF`), not block-based. This matches Swift behavior and is required for cross-platform test vector compatibility.

### TU-6: Module-level re-exports

Source: `lib.rs`

Public API (re-exported at package level):
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

1. **Xoshiro256StarStar reimplementation**: No Python package provides this algorithm. Must implement inline with exact state transitions. Test against `test_next_50` vector.

2. **64-bit unsigned arithmetic**: Python ints are arbitrary-precision. All u64 operations must mask with `& 0xFFFFFFFFFFFFFFFF`. The `next_u64()` return, rotations, shifts, and additions in Xoshiro all need masking.

3. **Lemire's method**: The wide multiplication is trivial in Python (just multiply), but extraction of low/high halves must match the Rust bit widths. The algorithm handles u8/u16/u32/u64 type widths — in Python, pass the bit width explicitly.

4. **Byte-by-byte random_data**: SeededRandomNumberGenerator.random_data() generates each byte as `next_u64() & 0xFF`, NOT via fill_bytes. This is critical for cross-platform compatibility.

5. **Thread safety for SecureRandomNumberGenerator**: Use `threading.Lock` to protect the global RNG state.

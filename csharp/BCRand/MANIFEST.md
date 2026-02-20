# Translation Manifest: bc-rand → C# (BCRand)

Source: `rust/bc-rand/` v0.5.0
Target: `csharp/BCRand/` namespace `BlockchainCommons.BCRand`

## External Dependencies

| Rust Crate      | C# Equivalent                                    | Notes                                    |
|-----------------|--------------------------------------------------|------------------------------------------|
| rand            | `System.Security.Cryptography.RandomNumberGenerator` | Static `Fill(Span<byte>)` method     |
| rand_core       | (built-in)                                       | Interface pattern replaces RngCore/CryptoRng |
| getrandom       | `System.Security.Cryptography.RandomNumberGenerator` | OS entropy via static API            |
| rand_xoshiro    | custom implementation                            | Xoshiro256** must be implemented inline  |
| num-traits      | (built-in)                                       | C# has built-in numeric types + UInt128  |
| lazy_static     | (not needed)                                     | `System.Security.Cryptography.RandomNumberGenerator` is already thread-safe |

## Feature Flags

None. All functionality is included by default.

## Internal BC Dependencies

None. bc-rand is a root crate.

## Translation Units (in order)

### TU-1: Xoshiro256StarStar (internal)

Private implementation of the Xoshiro256** PRNG algorithm. Not part of the public API but required by SeededRandomNumberGenerator.

Source: Uses `rand_xoshiro::Xoshiro256StarStar` — must be reimplemented.

Key behaviors:
- Seeded with 32 bytes (4 × u64 little-endian via `BinaryPrimitives`)
- `NextUInt64()` produces deterministic sequence
- Uses `System.Numerics.BitOperations.RotateLeft` for bit rotation
- Must match exact output for cross-platform test vectors

### TU-2: IRandomNumberGenerator (interface)

Rust trait → C# interface.

Source: `random_number_generator.rs`

Public API:
- `interface IRandomNumberGenerator` with:
  - `NextUInt32() : uint`
  - `NextUInt64() : ulong`
  - `RandomData(int size) : byte[]`
  - `FillRandomData(Span<byte> data) : void`

### TU-3: Extension methods on IRandomNumberGenerator

Source: `random_number_generator.rs`

Public API (extension methods in `RandomNumberGeneratorExtensions`):
- `NextWithUpperBound(uint upperBound) : uint` — Lemire's method (32-bit)
- `NextWithUpperBound(ulong upperBound) : ulong` — Lemire's method (64-bit)
- `NextInRange(int start, int end) : int` — half-open [start, end)
- `NextInClosedRange(int start, int end) : int` — closed [start, end]
- `RandomArray(int size) : byte[]` — delegates to FillRandomData
- `RandomBool() : bool`
- `RandomUInt32() : uint`
- `ThreadRng() : IRandomNumberGenerator` — static method returning shared secure instance

Translation notes:
- Lemire's wide multiplication uses `ulong` for 32-bit variant, `UInt128` for 64-bit variant
- Rust's generic traits (`HasMagnitude`, `Widening`) collapse to simple casts in C#
- Range functions use `int` parameters instead of `Range<T>` objects
- `unchecked` arithmetic for wrapping subtraction in threshold computation

### TU-4: SecureRandomNumberGenerator

Source: `secure_random.rs`

Public API:
- `class SecureRandomNumberGenerator : IRandomNumberGenerator`
  - `Shared` — static singleton instance
  - `SecureRandomData(int size) : byte[]` — static convenience
  - `SecureFillRandomData(Span<byte> data) : void` — static convenience

Translation notes:
- Delegates to `System.Security.Cryptography.RandomNumberGenerator.Fill()` which is already thread-safe
- No lazy initialization or Mutex needed (unlike Rust's `lazy_static` pattern)
- Using alias `CryptoRng` avoids name collision with our `IRandomNumberGenerator`

### TU-5: SeededRandomNumberGenerator

Source: `seeded_random.rs`

Public API:
- `class SeededRandomNumberGenerator : IRandomNumberGenerator`
  - Constructor: `SeededRandomNumberGenerator(ulong[] seed)` — 4 × ulong
  - `NextUInt64() : ulong`
  - `RandomData(int size) : byte[]` — byte-by-byte from `NextUInt64()`
  - `FillRandomData(Span<byte> data) : void`
  - `CreateFake() : SeededRandomNumberGenerator` — static factory with fixed seed
  - `FakeRandomData(int size) : byte[]` — static convenience

Critical: `RandomData()` generates byte-by-byte (each byte = `(byte)NextUInt64()`), not block-based. This matches the Swift implementation for cross-platform test vector compatibility.

### TU-6: Public API surface

All public types and extension methods are in the `BlockchainCommons.BCRand` namespace.

Re-exports (accessible via `using BlockchainCommons.BCRand`):
- `IRandomNumberGenerator`
- `SecureRandomNumberGenerator`
- `SeededRandomNumberGenerator`
- `RandomNumberGeneratorExtensions` (extension methods auto-discovered)

## Test Inventory

| Test Name                  | Source File              | Type        | Vectors |
|----------------------------|--------------------------|-------------|---------|
| TestNextU64                | seeded_random.rs         | deterministic | First u64 = 1104683000648959614 |
| TestNext50                 | seeded_random.rs         | deterministic | 50 exact u64 values |
| TestFakeRandomData         | seeded_random.rs         | deterministic | 100 bytes hex |
| TestNextWithUpperBound     | seeded_random.rs         | deterministic | upper=10000u → 745u |
| TestInRange                | seeded_random.rs         | deterministic | 100 values in [0,100) |
| TestFillRandomData         | seeded_random.rs         | equivalence   | RandomData == FillRandomData |
| TestFakeNumbers            | random_number_generator.rs | deterministic | 100 values in [-50,50] |
| TestRandomData             | secure_random.rs         | non-deterministic | 3 × 32 bytes all different |

## Translation Hazards

1. **Xoshiro256StarStar reimplementation**: No NuGet package provides this algorithm. Must implement inline with exact state transitions. Test against `TestNext50` vector.

2. **UInt128 for 64-bit Lemire's**: The 64-bit wide multiplication uses `System.UInt128` (available since .NET 7). The 32-bit variant uses `ulong`.

3. **Byte-by-byte random_data**: `SeededRandomNumberGenerator.RandomData()` generates each byte as `(byte)NextUInt64()`, NOT via block fill. Critical for cross-platform compatibility.

4. **Name collision**: `System.Security.Cryptography.RandomNumberGenerator` exists in .NET. Our interface is named `IRandomNumberGenerator` with the standard `I` prefix to avoid collision.

5. **Endianness**: Seed bytes must be read as little-endian u64 values. Use `BinaryPrimitives.ReadUInt64LittleEndian` for portability.

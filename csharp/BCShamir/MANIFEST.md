# Translation Manifest: bc-shamir → C# (BCShamir)

Source: `rust/bc-shamir/` v0.13.0  
Target: `csharp/BCShamir/` namespace `BlockchainCommons.BCShamir`

## Crate Metadata
- Crate: `bc-shamir`
- Version: `0.13.0`
- Rust edition: `2024`
- Description: "Shamir's Secret Sharing (SSS) for Rust."

## Dependencies

### Internal BC dependencies
- `bc-rand` (`^0.5.0`) → C# project reference `csharp/BCRand/BCRand/BCRand.csproj`
- `bc-crypto` (`^0.14.0`) → C# project reference `csharp/BCCrypto/BCCrypto/BCCrypto.csproj`

### External dependencies
- Runtime: `thiserror` (Rust derive helper) → no direct C# dependency, map to C# exception types
- Dev/test-only:
  - `hex-literal`, `hex` → `Convert.FromHexString(...)`
  - `rand` → `BlockchainCommons.BCRand` RNG implementations
  - `version-sync` metadata tests → not translated

## Feature Flags
- No feature flags in `Cargo.toml`.
- Translation scope: full crate (default behavior only, no gated branches).

## Public API Surface

### Type Catalog
- `Error`
  - kind: enum
  - variants:
    - `SecretTooLong`
    - `TooManyShares`
    - `InterpolationFailure`
    - `ChecksumFailure`
    - `SecretTooShort`
    - `SecretNotEvenLen`
    - `InvalidThreshold`
    - `SharesUnequalLength`
  - C# mapping: public enum + custom exception carrying the enum value

- `Result<T>`
  - kind: type alias
  - Rust: `std::result::Result<T, Error>`
  - C# mapping: methods throw typed exceptions (`BCShamirException`) instead of returning `Result<T>`

### Constant Catalog
- `MIN_SECRET_LEN: usize = 16`
- `MAX_SECRET_LEN: usize = 32`
- `MAX_SHARE_COUNT: usize = 16`

### Function Catalog
Public (crate exports):
- `split_secret(threshold: usize, share_count: usize, secret: &[u8], random_generator: &mut impl RandomNumberGenerator) -> Result<Vec<Vec<u8>>>`
- `recover_secret<T>(indexes: &[usize], shares: &[T]) -> Result<Vec<u8>> where T: AsRef<[u8]>`

Internal modules translated but non-public:
- `hazmat.rs`
  - `bitslice`
  - `unbitslice`
  - `bitslice_setall`
  - `gf256_add`
  - `gf256_mul`
  - `gf256_square`
  - `gf256_inv`
- `interpolate.rs`
  - `interpolate`
  - `hazmat_lagrange_basis` (private)
- `shamir.rs`
  - `create_digest` (private)
  - `validate_parameters` (private)

## Documentation Catalog
- Crate-level docs: yes (`lib.rs` has Introduction/Getting Started/Usage sections + examples)
- Module docs:
  - `shamir.rs` has doc comments for public functions `split_secret` and `recover_secret`
  - `lib.rs` has doc comments for public constants
  - `error.rs` has error display strings but no separate rustdoc blocks per variant
- Public items with docs:
  - `MIN_SECRET_LEN`, `MAX_SECRET_LEN`, `MAX_SHARE_COUNT`
  - `split_secret`, `recover_secret`
- Public items without docs:
  - `Error` enum variants (display messages exist)
  - `Result<T>` alias
- Package metadata description: present in Cargo.toml
- README: present (`bc-shamir/README.md`)

## Test Inventory

Behavior tests to translate:
1. `test_split_secret_3_5` (`src/lib.rs`)
- Uses deterministic fake RNG (byte sequence increments by 17 wrapping per fill call)
- Validates all 5 output shares against exact hex vectors
- Recovers secret from indexes `[1,2,4]`

2. `test_split_secret_2_7` (`src/lib.rs`)
- Uses deterministic fake RNG
- Validates all 7 output shares against exact hex vectors
- Recovers secret from indexes `[3,4]`

3. `example_split` (`src/shamir.rs`)
- Smoke test for splitting with secure RNG

4. `example_recover` (`src/shamir.rs`)
- Recover known sample shares

Metadata tests not translated:
- `test_readme_deps`
- `test_html_root_url`

## Translation Hazards
1. **GF(2^8) bitsliced arithmetic fidelity**
- `hazmat.rs` relies on explicit bitwise operations and in-place overwrite semantics.
- C# translation must preserve operation ordering and aliasing assumptions exactly.

2. **Wrapping shift and byte behavior**
- Rust uses `wrapping_shl`/`wrapping_shr` and `wrapping_add` on bytes in test RNG.
- C# must use unchecked arithmetic and equivalent shift behavior.

3. **Secret cleanup semantics**
- Rust zeroes temporary buffers (`memzero`, `memzero_vec_vec_u8`).
- C# should explicitly clear byte buffers and jagged arrays via BC Crypto memory-zero helpers and clear uint working buffers.

4. **`Result<T>` to exceptions mapping**
- Rust error-returning APIs become throwing APIs in C#.
- Must preserve all distinct error conditions/messages through an error enum + exception.

5. **Generic `AsRef<[u8]>` inputs**
- Rust `recover_secret` and `interpolate` accept generic share containers.
- C# API will use `IReadOnlyList<byte[]>` for deterministic and straightforward interop.

## Translation Unit Order
1. Error model (`Error` enum + `BCShamirException`)
2. Public constants and API surface (`Shamir` class shell)
3. Hazmat GF256 internals (`Hazmat`)
4. Interpolation internals (`Interpolation`)
5. Public split/recover implementation (`Shamir`)
6. Tests (vector tests first, then examples)

## Planned C# File Mapping
- `src/error.rs` → `BCShamir/BCShamirException.cs`
- `src/hazmat.rs` → `BCShamir/Hazmat.cs`
- `src/interpolate.rs` → `BCShamir/Interpolation.cs`
- `src/shamir.rs` + crate-level exports/constants in `lib.rs` → `BCShamir/Shamir.cs`
- Rust tests in `lib.rs` and `shamir.rs` → `BCShamir.Tests/ShamirTests.cs`

## Project Structure
```
csharp/BCShamir/
├── .gitignore
├── LOG.md
├── MANIFEST.md
├── BCShamir.slnx
├── BCShamir/
│   ├── BCShamir.csproj
│   ├── BCShamirException.cs
│   ├── Hazmat.cs
│   ├── Interpolation.cs
│   └── Shamir.cs
└── BCShamir.Tests/
    ├── BCShamir.Tests.csproj
    └── ShamirTests.cs
```

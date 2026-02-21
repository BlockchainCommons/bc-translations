# Translation Manifest: bc-shamir

Source: `rust/bc-shamir/` v0.13.0
Target: `swift/BCShamir/` module `BCShamir`

## Crate Metadata

- Crate: `bc-shamir`
- Version: `0.13.0`
- Edition: `2024`
- Description: `Shamir's Secret Sharing (SSS) for Rust.`
- Internal BC dependencies: `bc-rand ^0.5.0`, `bc-crypto ^0.14.0`
- External dependencies: `thiserror ^2.0`
- Dev dependencies: `hex-literal`, `hex`, `version-sync`, `rand`

## Feature Flags

No feature flags are declared. All crate functionality is in the default build.

## Public API Surface Catalog

### Type Catalog

- name: `Error`
  - kind: `enum`
  - variants:
    - `SecretTooLong`
    - `TooManyShares`
    - `InterpolationFailure`
    - `ChecksumFailure`
    - `SecretTooShort`
    - `SecretNotEvenLen`
    - `InvalidThreshold`
    - `SharesUnequalLength`
  - derives: `Debug`, `thiserror::Error`

- name: `Result<T>`
  - kind: `type alias`
  - definition: `std::result::Result<T, Error>`

### Function Catalog

- name: `split_secret`
  - signature: `fn split_secret(threshold: usize, share_count: usize, secret: &[u8], random_generator: &mut impl RandomNumberGenerator) -> Result<Vec<Vec<u8>>>`
  - is_method: `false`
  - uses_generics: `false`
  - uses_result: `true`
  - uses_option: `false`

- name: `recover_secret`
  - signature: `fn recover_secret<T>(indexes: &[usize], shares: &[T]) -> Result<Vec<u8>> where T: AsRef<[u8]>`
  - is_method: `false`
  - uses_generics: `true`
  - uses_result: `true`
  - uses_option: `false`

### Constant Catalog

- name: `MIN_SECRET_LEN`
  - type: `usize`
  - value: `16`

- name: `MAX_SECRET_LEN`
  - type: `usize`
  - value: `32`

- name: `MAX_SHARE_COUNT`
  - type: `usize`
  - value: `16`

### Trait Catalog

No public traits are declared by this crate.

## Internal Module Inventory (non-public but required for faithful translation)

- `hazmat.rs`
  - `bitslice`
  - `unbitslice`
  - `bitslice_setall`
  - `gf256_add`
  - `gf256_mul`
  - `gf256_square`
  - `gf256_inv`

- `interpolate.rs`
  - `interpolate<T: AsRef<[u8]>>`
  - private helper `hazmat_lagrange_basis`

- `shamir.rs`
  - constants: `SECRET_INDEX = 255`, `DIGEST_INDEX = 254`
  - private helpers: `create_digest`, `validate_parameters`
  - public API implementations: `split_secret`, `recover_secret`

## Documentation Catalog

- Crate-level doc comment: Yes (`lib.rs` has extensive `//!` intro, getting started, usage examples)
- Module-level doc comments:
  - `lib.rs`: yes (crate docs)
  - `error.rs`: none
  - `hazmat.rs`: function-level docs only on selected functions
  - `interpolate.rs`: function-level docs/comments
  - `shamir.rs`: docs on public functions
- Public items with doc comments:
  - `MIN_SECRET_LEN`
  - `MAX_SECRET_LEN`
  - `MAX_SHARE_COUNT`
  - `split_secret`
  - `recover_secret`
- Public items without doc comments:
  - `Error`
  - `Result<T>`
- Package metadata description: present in `Cargo.toml`
- README: present (`rust/bc-shamir/README.md`), includes intro, usage, version history

## External Dependency Equivalents (Swift)

| Rust dependency | Purpose | Swift equivalent |
|---|---|---|
| `thiserror` | Error derive macro | Swift `enum ShamirError: Error` (native error protocol) |
| `hex-literal` (dev) | Hex test vectors | Local hex-to-bytes helper or `Data` hex initializer in tests |
| `hex` (dev) | Hex formatting in tests/debug | Not required for final Swift tests |
| `version-sync` (dev) | Rust metadata checks | Not applicable to Swift package; omit as Rust-specific |
| `rand` (dev) | `RngCore`/`CryptoRng` impl in tests | Not required; use Swift fake RNG implementing `BCRandomNumberGenerator` |

Internal BC deps for Swift implementation:
- `bc-rand` -> `BCRand` via local path dependency (`.package(path: "../BCRand")`)
- `bc-crypto` -> `BCCrypto` via local path dependency (`.package(path: "../BCCrypto")`)

## Test Inventory

Rust tests discovered:

| Test name | Location | Purpose | Vector-critical |
|---|---|---|---|
| `test_split_secret_3_5` | `src/lib.rs` | Split 16-byte secret with deterministic fake RNG; verify all 5 shares and recovery | Yes |
| `test_split_secret_2_7` | `src/lib.rs` | Split 32-byte secret with deterministic fake RNG; verify all 7 shares and recovery | Yes |
| `test_readme_deps` | `src/lib.rs` | Rust README dependency metadata sync | Rust-only |
| `test_html_root_url` | `src/lib.rs` | Rust `html_root_url` metadata sync | Rust-only |
| `example_split` | `src/shamir.rs` | Basic split example with secure RNG, asserts share count | No (non-deterministic) |
| `example_recover` | `src/shamir.rs` | Recover from two static shares | Yes |

Swift translation test requirement:
- Translate 4 behavioral tests (`test_split_secret_3_5`, `test_split_secret_2_7`, `example_split`, `example_recover`).
- Omit 2 Rust metadata-only tests (`test_readme_deps`, `test_html_root_url`) as non-portable.

## Translation Unit Order

1. `TU-1` constants and error model
   - `MIN_SECRET_LEN`, `MAX_SECRET_LEN`, `MAX_SHARE_COUNT`
   - `Error` -> Swift `ShamirError` enum conforming to `Error`
2. `TU-2` GF(256) hazmat operations (`hazmat.rs`)
3. `TU-3` interpolation implementation (`interpolate.rs`)
4. `TU-4` Shamir core (`shamir.rs`) and public API (`splitSecret`, `recoverSecret`)
5. `TU-5` tests with deterministic vectors and example cases

## Translation Hazards

1. **GF(2^8) bit-slicing semantics**
   - `gf256_mul`, `gf256_square`, `gf256_inv` rely on exact bitwise/wrapping behavior over 32-bit words.
   - Swift must use `UInt32` arrays with `&+`, `&<<`, `&>>` wrapping operators to preserve semantics.

2. **Fixed 32-byte block assumption**
   - Hazmat code assumes secret data is packed into 32-byte slots (`MAX_SECRET_LEN`).
   - Interpolation still returns `yl` bytes; translation must preserve this truncation behavior.

3. **Digest/secret interpolation sentinels**
   - Reserved x-coordinates are `255` (secret) and `254` (digest).
   - Must be preserved exactly; changing these breaks recovery compatibility.

4. **Generic `AsRef<[u8]>` inputs**
   - Rust accepts flexible share containers; Swift should accept `[[UInt8]]` for simplicity.

5. **Memory zeroization expectations**
   - Rust zeroes temporary buffers with `memzero` utilities.
   - Swift should call `memzero`/`memzeroVecVecU8` from `BCCrypto` for parity.

6. **Deterministic fake RNG vectors**
   - Test vectors depend on a fake RNG that fills bytes with `0, 17, 34, ...` wrapping at 256.
   - Swift test fake RNG must replicate this exactly via `BCRandomNumberGenerator` conformance.

7. **Signed/unsigned shifts**
   - `bitslice_setall` uses `as i32` then `wrapping_shr(31)` for arithmetic right shift (sign extension).
   - Swift equivalent: cast to `Int32`, use `&>> 31`, then cast back to `UInt32`.

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: no
Reason: bc-shamir has no text/string rendering or diagnostic output. All tests compare binary data (byte arrays) and share counts.

# Translation Manifest: bc-shamir v0.13.0

## Crate Overview

Shamir's Secret Sharing (SSS) implementation. Splits a secret into shares such that a threshold number of shares are needed to reconstruct the secret. Uses bitsliced GF(2^8) arithmetic for constant-time operations.

## Dependencies

### Internal BC Dependencies
- `bc-rand` (v0.5.0) — `RandomNumberGenerator` trait, used by `split_secret`
- `bc-crypto` (v0.14.0) — `hmac_sha256` for digest creation, `memzero`/`memzero_vec_vec_u8` for secure memory wiping

### External Dependencies
| Rust crate | Purpose | Go equivalent |
|---|---|---|
| `thiserror` | Error derive macro | Standard `errors.New` / sentinel errors |
| `hex-literal` (dev) | Hex byte literals in tests | `encoding/hex.DecodeString` |
| `hex` (dev) | Hex encoding/decoding in tests | `encoding/hex` |
| `version-sync` (dev) | Version consistency checks | Skip — Go-specific, not applicable |
| `rand` (dev) | `RngCore`/`CryptoRng` for fake RNG in tests | Not needed — use `RandomNumberGenerator` interface directly |

### Go-Specific Dependencies
| Go module | Purpose |
|---|---|
| `github.com/nickel-blockchaincommons/bcrand-go` | `RandomNumberGenerator` interface |
| `github.com/nickel-blockchaincommons/bccrypto-go` | `HMACSHA256`, `Memzero`, `MemzeroVecVecU8` |

## Feature Flags

None. The crate has no feature flags.

## Public API Surface

### Constants

| Name | Type | Value | Description |
|---|---|---|---|
| `MIN_SECRET_LEN` | `usize` | 16 | Minimum length of a secret in bytes |
| `MAX_SECRET_LEN` | `usize` | 32 | Maximum length of a secret in bytes |
| `MAX_SHARE_COUNT` | `usize` | 16 | Maximum number of shares that can be generated |

### Error Type

```
enum Error {
    SecretTooLong,        // "secret is too long"
    TooManyShares,        // "too many shares"
    InterpolationFailure, // "interpolation failed"
    ChecksumFailure,      // "checksum failure"
    SecretTooShort,       // "secret is too short"
    SecretNotEvenLen,     // "secret is not of even length"
    InvalidThreshold,     // "invalid threshold"
    SharesUnequalLength,  // "shares have unequal length"
}
```

Go translation: sentinel `error` variables (e.g., `var ErrSecretTooLong = errors.New("secret is too long")`).

### Public Functions

#### `split_secret`
```rust
pub fn split_secret(
    threshold: usize,
    share_count: usize,
    secret: &[u8],
    random_generator: &mut impl RandomNumberGenerator,
) -> Result<Vec<Vec<u8>>>
```
Splits a secret into `share_count` shares requiring `threshold` shares to recover. Returns the shares or an error.

#### `recover_secret`
```rust
pub fn recover_secret<T>(indexes: &[usize], shares: &[T]) -> Result<Vec<u8>>
where T: AsRef<[u8]>
```
Recovers the secret from the given shares at the given indexes. The generic `T: AsRef<[u8]>` allows both `Vec<u8>` and `&[u8]` inputs; in Go, use `[][]byte`.

### Re-exports
- `Error` (from `error` module)
- `Result<T>` = `std::result::Result<T, Error>` — in Go, use `(T, error)` return pattern

## Internal (Private) Modules

### `hazmat` module — GF(2^8) bitsliced arithmetic

All functions operate on `[u32; 8]` bitsliced representation (32 parallel GF(2^8) elements packed into 8 uint32 words).

| Function | Signature | Description |
|---|---|---|
| `bitslice` | `(r: &mut [u32; 8], x: &[u8])` | Pack 32 bytes into bitsliced form |
| `unbitslice` | `(r: &mut [u8], x: &[u32; 8])` | Unpack bitsliced form to 32 bytes |
| `bitslice_setall` | `(r: &mut [u32; 8], x: u8)` | Set all 32 parallel elements to x |
| `gf256_add` | `(r: &mut [u32; 8], x: &[u32; 8])` | XOR (addition in GF(2^8)) |
| `gf256_mul` | `(r: &mut [u32; 8], a: &[u32; 8], b: &[u32; 8])` | Russian Peasant multiplication; r and a may alias but r and b must not |
| `gf256_square` | `(r: &mut [u32; 8], x: &[u32; 8])` | Square via Freshman's Dream; r and x may alias |
| `gf256_inv` | `(r: &mut [u32; 8], x: &mut [u32; 8])` | Invert via exponentiation to x^254 |

### `interpolate` module — Lagrange interpolation

| Function | Signature | Description |
|---|---|---|
| `hazmat_lagrange_basis` | `(values: &mut [u8], n: usize, xc: &[u8], x: u8)` | Compute Lagrange basis coefficients at point x |
| `interpolate` | `(n: usize, xi: &[u8], yl: usize, yij: &[T], x: u8) -> Result<Vec<u8>>` | Interpolate polynomial at x; T: AsRef<[u8]> |

### `shamir` module — core split/recover logic

| Function | Signature | Visibility | Description |
|---|---|---|---|
| `create_digest` | `(random_data: &[u8], shared_secret: &[u8]) -> [u8; 32]` | private | HMAC-SHA256 of random data keyed by secret |
| `validate_parameters` | `(threshold, share_count, secret_length) -> Result<()>` | private | Validates all preconditions |
| `split_secret` | (see public API above) | public | Entry point for splitting |
| `recover_secret` | (see public API above) | public | Entry point for recovery |

Constants in `shamir` module:
- `SECRET_INDEX: u8 = 255` — x-coordinate for the secret in interpolation
- `DIGEST_INDEX: u8 = 254` — x-coordinate for the digest in interpolation

## Documentation Catalog

- **Crate-level doc comment**: Yes — describes SSS, getting started, and usage examples
- **Module-level doc comments**: None
- **Public items with doc comments**: `MIN_SECRET_LEN`, `MAX_SECRET_LEN`, `MAX_SHARE_COUNT`, `split_secret`, `recover_secret`, all `Error` variants, all `hazmat` functions
- **Public items WITHOUT doc comments**: None
- **Package metadata description**: "Shamir's Secret Sharing (SSS) for Rust."

## Test Inventory

### `lib.rs` tests (inline `#[cfg(test)]`)

| Test | What it tests | Test vectors | Deterministic RNG |
|---|---|---|---|
| `test_split_secret_3_5` | Split with threshold=3, count=5, 16-byte secret; verify all 5 share values; recover from shares [1,2,4] | YES — hardcoded hex share values | YES — `FakeRandomNumberGenerator` (sequential bytes: 0, 17, 34, ...) |
| `test_split_secret_2_7` | Split with threshold=2, count=7, 32-byte secret; verify all 7 share values; recover from shares [3,4] | YES — hardcoded hex share values | YES — `FakeRandomNumberGenerator` |
| `test_readme_deps` | Version sync check | No | No — skip in Go |
| `test_html_root_url` | Version sync check | No | No — skip in Go |

### `shamir.rs` tests (inline `#[cfg(test)]`)

| Test | What it tests | Test vectors | Deterministic RNG |
|---|---|---|---|
| `example_split` | Split with threshold=2, count=3, 24-byte secret using SecureRNG; verify share count | No — just checks count | No — uses `SecureRandomNumberGenerator` |
| `example_recover` | Recover from 2 shares at indexes [0,2]; verify secret matches | YES — hardcoded share bytes | No |

### FakeRandomNumberGenerator (test helper)

Implements `RandomNumberGenerator` with `fill_random_data` that produces sequential bytes starting at 0 with step 17 (wrapping). Pattern: `b = b.wrapping_add(17)` starting from `b = 0`.

Sequence: `0, 17, 34, 51, 68, 85, 102, 119, 136, 153, 170, 187, 204, 221, 238, 255, 16, 33, ...`

## Translation Unit Order

1. **Constants** — `MIN_SECRET_LEN`, `MAX_SECRET_LEN`, `MAX_SHARE_COUNT`, `SECRET_INDEX`, `DIGEST_INDEX`
2. **Error type** — sentinel error variables
3. **hazmat functions** — `bitslice`, `unbitslice`, `bitslice_setall`, `gf256_add`, `gf256_mul`, `gf256_square`, `gf256_inv`
4. **interpolate functions** — `hazmat_lagrange_basis`, `interpolate`
5. **shamir functions** — `validate_parameters`, `create_digest`, `split_secret`, `recover_secret`
6. **Tests** — `FakeRandomNumberGenerator`, all test functions

## Translation Hazards

### 1. Mutable aliasing in GF(2^8) functions
`gf256_mul` documents that `r` and `a` may overlap but `r` and `b` must not. In Go, slices/arrays are value types when fixed-size (`[8]uint32`), so passing by value for `a` naturally creates a copy. Use pointer receivers for `r` (output) and value/pointer for `a`/`b` carefully.

### 2. Bitsliced array indexing
The `hazmat` functions use `[u32; 8]` arrays extensively. In Go, use `[8]uint32`. The bitwise operations translate directly but watch for Go's unsigned shift behavior vs Rust's `wrapping_shl`/`wrapping_shr`.

### 3. `wrapping_shl`/`wrapping_shr` semantics
Rust's wrapping shifts mask the shift amount to the type width. Go's shift operators do NOT wrap — shifting by >= bit-width gives 0 for unsigned types. For `u32`, Rust `wrapping_shl(n)` is equivalent to `<< (n & 31)` in Go.

### 4. Signed arithmetic in `bitslice_setall`
The function casts to `i32` and uses `wrapping_shr(31)` for sign extension (arithmetic right shift). Go's `int32` right shift IS arithmetic, so `int32(x) >> 31` works correctly for sign extension.

### 5. Generic `T: AsRef<[u8]>` in `interpolate` and `recover_secret`
Go doesn't have generics for this pattern. Use `[][]byte` directly — callers already pass `[][]byte`.

### 6. Secure memory wiping
Uses `bc_crypto::memzero` and `memzero_vec_vec_u8`. The Go `bccrypto` package provides `Memzero[T]` and `MemzeroVecVecU8`. Call these at the same points as the Rust code.

### 7. In-place mutation patterns
Many functions take `&mut [u32; 8]` output parameters. In Go, pass `*[8]uint32` pointers. Some functions document that input and output may alias (e.g., `gf256_square(r, r)`) — with Go value types this works naturally since the input is read before the output is written, but verify with test vectors.

### 8. The `gf256_mul` unrolled multiplication
This is a large block of hand-unrolled bitwise operations (148 lines). Translate line-by-line; do NOT attempt to simplify or refactor. The comments explain it's intentionally unrolled for compiler optimization reasons.

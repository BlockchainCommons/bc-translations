# Translation Manifest: bc-shamir v0.13.0

## Crate Overview

Shamir's Secret Sharing (SSS) implementation. Splits a secret into shares such that a threshold number of shares are needed to reconstruct the secret. Uses bitsliced GF(2^8) arithmetic for constant-time operations.

## Dependencies

### Internal BC Dependencies
- `bc-rand` (v0.5.0) — `RandomNumberGenerator` interface, used by `splitSecret`
- `bc-crypto` (v0.14.0) — `hmacSha256` for digest creation, `memzero`/`memzeroVecVecU8` for secure memory wiping

### External Dependencies
| Rust crate | Purpose | TypeScript equivalent |
|---|---|---|
| `thiserror` | Error derive macro | Custom error classes extending `Error` |
| `hex-literal` (dev) | Hex byte literals in tests | `hexToBytes()` helper |
| `hex` (dev) | Hex encoding/decoding in tests | `Buffer.from(hex, 'hex')` or inline helper |
| `version-sync` (dev) | Version consistency checks | Skip — not applicable |
| `rand` (dev) | `RngCore`/`CryptoRng` for fake RNG in tests | Not needed — use `RandomNumberGenerator` interface directly |

### TypeScript-Specific Dependencies
| Package | Purpose |
|---|---|
| `@bc/rand` | `RandomNumberGenerator` interface |
| `@bc/crypto` | `hmacSha256`, `memzero`, `memzeroVecVecU8` |

## Feature Flags

None. The crate has no feature flags.

## Public API Surface

### Constants

| Name | Type | Value | Description |
|---|---|---|---|
| `MIN_SECRET_LEN` | `number` | 16 | Minimum length of a secret in bytes |
| `MAX_SECRET_LEN` | `number` | 32 | Maximum length of a secret in bytes |
| `MAX_SHARE_COUNT` | `number` | 16 | Maximum number of shares that can be generated |

### Error Type

TypeScript: Custom `ShamirError` class extending `Error`, with static factory methods or distinct error messages. Use `RangeError` for precondition violations per TypeScript patterns.

```
ShamirError:
  SecretTooLong        — "secret is too long"
  TooManyShares        — "too many shares"
  InterpolationFailure — "interpolation failed"
  ChecksumFailure      — "checksum failure"
  SecretTooShort       — "secret is too short"
  SecretNotEvenLen     — "secret is not of even length"
  InvalidThreshold     — "invalid threshold"
  SharesUnequalLength  — "shares have unequal length"
```

### Public Functions

#### `splitSecret`
```typescript
function splitSecret(
    threshold: number,
    shareCount: number,
    secret: Uint8Array,
    randomGenerator: RandomNumberGenerator,
): Uint8Array[]
```
Splits a secret into `shareCount` shares requiring `threshold` shares to recover. Throws `ShamirError` on invalid parameters.

#### `recoverSecret`
```typescript
function recoverSecret(
    indexes: number[],
    shares: Uint8Array[],
): Uint8Array
```
Recovers the secret from the given shares at the given indexes. Throws `ShamirError` on failure.

### Exports
- `ShamirError`
- `MIN_SECRET_LEN`, `MAX_SECRET_LEN`, `MAX_SHARE_COUNT`
- `splitSecret`, `recoverSecret`

## Internal (Private) Modules

### `hazmat` module — GF(2^8) bitsliced arithmetic

All functions operate on `Uint32Array(8)` bitsliced representation.

| Function | Signature | Description |
|---|---|---|
| `bitslice` | `(r: Uint32Array, x: Uint8Array): void` | Pack 32 bytes into bitsliced form |
| `unbitslice` | `(r: Uint8Array, x: Uint32Array): void` | Unpack bitsliced form to 32 bytes |
| `bitsliceSetall` | `(r: Uint32Array, x: number): void` | Set all 32 parallel elements to x |
| `gf256Add` | `(r: Uint32Array, x: Uint32Array): void` | XOR (addition in GF(2^8)) |
| `gf256Mul` | `(r: Uint32Array, a: Uint32Array, b: Uint32Array): void` | Russian Peasant multiplication |
| `gf256Square` | `(r: Uint32Array, x: Uint32Array): void` | Square via Freshman's Dream |
| `gf256Inv` | `(r: Uint32Array, x: Uint32Array): void` | Invert via exponentiation to x^254 |

### `interpolate` module — Lagrange interpolation

| Function | Signature | Description |
|---|---|---|
| `hazmatLagrangeBasis` | `(values: Uint8Array, n: number, xc: Uint8Array, x: number): void` | Compute Lagrange basis coefficients |
| `interpolate` | `(n: number, xi: Uint8Array, yl: number, yij: Uint8Array[], x: number): Uint8Array` | Interpolate polynomial at x |

### `shamir` module — core split/recover logic

| Function | Signature | Visibility | Description |
|---|---|---|---|
| `createDigest` | `(randomData: Uint8Array, sharedSecret: Uint8Array): Uint8Array` | private | HMAC-SHA256 of random data keyed by secret |
| `validateParameters` | `(threshold: number, shareCount: number, secretLength: number): void` | private | Validates all preconditions, throws ShamirError |
| `splitSecret` | (see public API above) | public | Entry point for splitting |
| `recoverSecret` | (see public API above) | public | Entry point for recovery |

Constants in `shamir` module:
- `SECRET_INDEX = 255` — x-coordinate for the secret in interpolation
- `DIGEST_INDEX = 254` — x-coordinate for the digest in interpolation

## Test Inventory

### `index.test.ts` tests (lib.rs inline tests)

| Test | What it tests | Test vectors | Deterministic RNG |
|---|---|---|---|
| `test split secret 3/5` | Split with threshold=3, count=5, 16-byte secret; verify all 5 share values; recover from shares [1,2,4] | YES — hardcoded hex share values | YES — `FakeRandomNumberGenerator` (sequential bytes: 0, 17, 34, ...) |
| `test split secret 2/7` | Split with threshold=2, count=7, 32-byte secret; verify all 7 share values; recover from shares [3,4] | YES — hardcoded hex share values | YES — `FakeRandomNumberGenerator` |

### `shamir.test.ts` tests (shamir.rs inline tests)

| Test | What it tests | Test vectors | Deterministic RNG |
|---|---|---|---|
| `example split` | Split with threshold=2, count=3, 24-byte secret using SecureRNG; verify share count | No — just checks count | No — uses `SecureRandomNumberGenerator` |
| `example recover` | Recover from 2 shares at indexes [0,2]; verify secret matches | YES — hardcoded share bytes | No |

### FakeRandomNumberGenerator (test helper)

Implements `RandomNumberGenerator` with `fillRandomData` that produces sequential bytes starting at 0 with step 17 (wrapping at 256). Pattern: `b = (b + 17) & 0xFF` starting from `b = 0`.

Sequence: `0, 17, 34, 51, 68, 85, 102, 119, 136, 153, 170, 187, 204, 221, 238, 255, 16, 33, ...`

## Translation Unit Order

1. **Constants** — `MIN_SECRET_LEN`, `MAX_SECRET_LEN`, `MAX_SHARE_COUNT`, `SECRET_INDEX`, `DIGEST_INDEX`
2. **Error type** — `ShamirError` class
3. **hazmat functions** — `bitslice`, `unbitslice`, `bitsliceSetall`, `gf256Add`, `gf256Mul`, `gf256Square`, `gf256Inv`
4. **interpolate functions** — `hazmatLagrangeBasis`, `interpolate`
5. **shamir functions** — `validateParameters`, `createDigest`, `splitSecret`, `recoverSecret`
6. **Tests** — `FakeRandomNumberGenerator`, all test functions

## Translation Hazards

### 1. JavaScript 32-bit integer overflow
JavaScript bitwise operators work on 32-bit signed integers. `x | 0` or `>>> 0` can be used to force unsigned 32-bit semantics. Use `>>> 0` after bitwise ops to ensure unsigned behavior where needed (especially for `bitslice_setall` sign extension).

### 2. Typed arrays for bitsliced representation
Use `Uint32Array(8)` for the `[u32; 8]` arrays. TypeScript typed arrays are mutable and pass by reference, so aliasing concerns are the same as Rust. Copy with `Uint32Array.from()` or `.slice()` where the Rust code copies.

### 3. `wrapping_shl`/`wrapping_shr` semantics
JavaScript shift operators already mask the shift amount to 5 bits for 32-bit operations (i.e., `x << n` is equivalent to `x << (n & 31)`). This matches Rust's `wrapping_shl` for `u32`.

### 4. Signed arithmetic in `bitslice_setall`
The Rust function casts to `i32` and uses `wrapping_shr(31)` for sign extension (arithmetic right shift). JavaScript's `>>` is an arithmetic right shift on signed 32-bit integers, so `(x >> 31)` works correctly. Use `>>> 0` to convert back to unsigned when storing.

### 5. Mutable aliasing in GF(2^8) functions
`gf256Mul` allows `r` and `a` to overlap but not `r` and `b`. Since TypeScript typed arrays are references, use `Uint32Array.from(a)` to create a local copy of `a` (matching the Rust `let mut a2 = *a`).

### 6. Generic `T: AsRef<[u8]>` in `interpolate` and `recoverSecret`
TypeScript uses `Uint8Array[]` directly — no generics needed.

### 7. Secure memory wiping
Uses `memzero` and `memzeroVecVecU8` from `@bc/crypto`. Call these at the same points as the Rust code.

### 8. The `gf256_mul` unrolled multiplication
This is a large block of hand-unrolled bitwise operations. Translate line-by-line; do NOT attempt to simplify or refactor.

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: no — This is a pure crypto library with no text rendering or formatted output. All tests compare byte arrays against known hex vectors.

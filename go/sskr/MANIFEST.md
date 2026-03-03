# Translation Manifest: sskr v0.12.0 (Go)

## Crate Overview

`sskr` implements Sharded Secret Key Reconstruction (SSKR), a grouped-threshold extension of Shamir secret sharing.

Rust source: `rust/sskr`

## Dependencies

### Internal BC Dependencies

- `bc-rand` (required) → Go package `github.com/nickel-blockchaincommons/bcrand-go`
- `bc-shamir` (required) → Go package `github.com/nickel-blockchaincommons/bcshamir-go`

### External Dependencies (Rust -> Go)

| Rust crate | Purpose | Go equivalent |
|---|---|---|
| `thiserror` | Derive error type and messages | `errors.New`, wrapped error structs, and `Unwrap()` |
| `hex-literal` (dev) | Hex test vectors | `encoding/hex` |
| `hex` (dev) | Hex encoding for debug prints | `encoding/hex` |
| `rand` (dev) | `RngCore`/`CryptoRng` trait plumbing | No direct equivalent needed; use `bcrand.RandomNumberGenerator` |
| `version-sync` (dev) | README/html-root metadata assertions | Omit in Go (Rust-only metadata checks) |

### Go-Specific Notes

- Module path: `github.com/nickel-blockchaincommons/sskr-go`
- Package name: `sskr`
- No third-party dependencies beyond internal BC Go modules and stdlib.

## Feature Flags

Rust crate defines no feature flags. Translate full default behavior.

## Public API Surface

### Type Catalog

- `Error` (Rust enum) → Go exported sentinel errors plus wrapped Shamir error type
  - Variants: `DuplicateMemberIndex`, `GroupSpecInvalid`, `GroupCountInvalid`, `GroupThresholdInvalid`, `MemberCountInvalid`, `MemberThresholdInvalid`, `NotEnoughGroups`, `SecretLengthNotEven`, `SecretTooLong`, `SecretTooShort`, `ShareLengthInvalid`, `ShareReservedBitsInvalid`, `SharesEmpty`, `ShareSetInvalid`, `ShamirError`
- `Secret`
  - tuple struct `Vec<u8>`
  - derives: `Clone`, `Debug`, `Eq`, `PartialEq`
  - methods: `new`, `len`, `is_empty`, `data`
  - trait impl: `AsRef<[u8]>`
- `Spec`
  - fields: `group_threshold`, `groups`
  - derives: `Debug`, `Clone`, `PartialEq`
  - methods: `new`, `group_threshold`, `groups`, `group_count`, `share_count`
- `GroupSpec`
  - fields: `member_threshold`, `member_count`
  - derives: `Debug`, `Clone`, `PartialEq`
  - methods: `new`, `member_threshold`, `member_count`, `parse`
  - impls: `Default` (`1-of-1`), `Display` (`"{m}-of-{n}"`)

### Function Catalog

- `sskr_generate(spec: &Spec, master_secret: &Secret) -> Result<Vec<Vec<Vec<u8>>>>`
- `sskr_generate_using(spec: &Spec, master_secret: &Secret, random_generator: &mut impl RandomNumberGenerator) -> Result<Vec<Vec<Vec<u8>>>>`
- `sskr_combine<T: AsRef<[u8]>>(shares: &[T]) -> Result<Secret>`

### Constant Catalog

- `MIN_SECRET_LEN: usize` (re-export of `bc_shamir::MIN_SECRET_LEN`)
- `MAX_SECRET_LEN: usize` (re-export of `bc_shamir::MAX_SECRET_LEN`)
- `MAX_SHARE_COUNT: usize` (re-export of `bc_shamir::MAX_SHARE_COUNT`)
- `MAX_GROUPS_COUNT: usize` (`MAX_SHARE_COUNT`)
- `METADATA_SIZE_BYTES: usize` (`5`)
- `MIN_SERIALIZE_SIZE_BYTES: usize` (`METADATA_SIZE_BYTES + MIN_SECRET_LEN`)

### Trait Catalog

No public trait definitions in `sskr`; consumes external `bc_rand::RandomNumberGenerator` trait.

## Doc Catalog

- Crate-level docs: yes (`lib.rs`) with introduction, getting-started, and example.
- Module docs: none (`encoding`, `spec`, `secret`, `error` rely on item-level docs).
- Public items with docs: all exported constants, `Secret`, `Spec`, `GroupSpec`, and exported functions in `encoding.rs`.
- Public items without docs: `Error` enum variants are documented via error messages rather than rustdoc blocks.
- Cargo description: `Sharded Secret Key Reconstruction (SSKR) for Rust.`
- README: yes (`rust/sskr/README.md`) with introduction, install, specification link, and lifecycle status notes.

## Test Inventory

Rust tests live in `rust/sskr/src/lib.rs` under `#[cfg(test)]`.

- `test_split_3_5` — deterministic single-group 3-of-5 split/combine
- `test_split_2_7` — deterministic single-group 2-of-7 split/combine
- `test_split_2_3_2_3` — deterministic two-group 2-of-3 + 2-of-3 split/combine
- `test_shuffle` — deterministic Fisher-Yates shuffle vector via fake RNG
- `fuzz_test` — 100 randomized round trips over random secret/spec combinations
- `example_encode` — docs example round trip (2 groups: 2-of-3 and 3-of-5)
- `example_encode_3` — regression: roundtrip works for 2-of-3, 1-of-1, and 1-of-3
- `example_encode_4` — regression: group threshold 1 ignores unrecoverable extra group
- `test_readme_deps` — Rust `version-sync` metadata check (do not translate)
- `test_html_root_url` — Rust `version-sync` metadata check (do not translate)

Test vectors are behaviorally critical for deterministic split/combine and shuffle outputs.

## EXPECTED TEXT OUTPUT RUBRIC

- Applicable: no
- Reason: tests validate secret/share bytes, counts, and deterministic ordering; no complex formatted text output is asserted.

## Translation Unit Order

1. Scaffold: `.gitignore`, `go.mod`
2. Constants and error model (`constants.go`, `errors.go`)
3. Data types (`secret.go`, `spec.go`)
4. Internal share model (`share.go`)
5. Encoding/combine logic (`encoding.go`)
6. Package docs (`doc.go`)
7. Tests (`sskr_test.go`)

## Translation Hazards

1. **Metadata bit packing is exact**: 5-byte header layout must match Rust bit-for-bit.
2. **Error semantics matter**: `ShamirError` is wrapped as an SSKR-layer error while preserving cause.
3. **`GroupSpec::new` allows zero threshold** when `member_count > 0`; do not add stricter validation.
4. **Combine ignores unrecoverable groups** after parse/grouping and continues until group threshold is met.
5. **Duplicate member index detection** occurs within each group before recovery attempts.
6. **Common metadata validation** (`identifier`, `group_threshold`, `group_count`, `secret_len`) must be strict across all input shares.
7. **Deterministic test RNG** in crate tests increments bytes by 17 with wrapping; preserve this helper exactly.

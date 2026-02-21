# Translation Manifest: bc-shamir → Python (bc-shamir)

## Source Crate
- Rust crate: `rust/bc-shamir`
- Version: `0.13.0`
- Default features: all (no feature flags)
- Internal dependencies: `bc-rand ^0.5.0`, `bc-crypto ^0.14.0`

## Scope
Translate the default Rust API and tests into `python/bc-shamir` with byte-for-byte parity for published test vectors.

## Public API Inventory

### Root exports (`lib.rs`)
- Constants:
  - `MIN_SECRET_LEN: usize = 16`
  - `MAX_SECRET_LEN: usize = 32`
  - `MAX_SHARE_COUNT: usize = 16`
- Error API:
  - `Error` enum variants:
    - `SecretTooLong`
    - `TooManyShares`
    - `InterpolationFailure`
    - `ChecksumFailure`
    - `SecretTooShort`
    - `SecretNotEvenLen`
    - `InvalidThreshold`
    - `SharesUnequalLength`
  - `Result<T>` alias
- Public functions:
  - `split_secret(threshold, share_count, secret, random_generator) -> Result<Vec<Vec<u8>>>`
  - `recover_secret(indexes, shares) -> Result<Vec<u8>>`

### Internal module API required for correctness
- `hazmat.rs`:
  - `bitslice`
  - `unbitslice`
  - `bitslice_setall`
  - `gf256_add`
  - `gf256_mul`
  - `gf256_square`
  - `gf256_inv`
- `interpolate.rs`:
  - `hazmat_lagrange_basis` (private helper)
  - `interpolate`
- `shamir.rs`:
  - `create_digest` (private helper)
  - `validate_parameters` (private helper)
  - `split_secret`
  - `recover_secret`

## Rust File → Python Unit Plan
1. `src/error.rs` → `src/bc_shamir/error.py`
2. `src/hazmat.rs` → `src/bc_shamir/hazmat.py`
3. `src/interpolate.rs` → `src/bc_shamir/interpolate.py`
4. `src/shamir.rs` → `src/bc_shamir/shamir.py`
5. `src/lib.rs` exports/docs → `src/bc_shamir/__init__.py`
6. Rust tests (`lib.rs`, `shamir.rs`) → Python tests:
   - `tests/test_shamir_vectors.py`
   - `tests/test_shamir_examples.py`
   - `tests/test_package_metadata.py`
   - `tests/conftest.py`

## Dependency Mapping (Rust → Python)
- Internal BC dependencies:
  - `bc-rand` → Python `bc-rand==0.5.0`
  - `bc-crypto` → Python `bc-crypto==0.14.0`
- External dependencies:
  - `thiserror` → native Python exception classes
  - `hex-literal`, `hex` (dev) → `bytes.fromhex(...)`
  - `version-sync` (dev) → package metadata/version tests in Python

## Feature Flags
- No feature flags in `bc-shamir`.
- Full crate is in scope for this translation.

## Documentation Catalog
- Crate-level docs in `lib.rs`: yes (introduction + usage examples)
- Module-level docs: no
- Public items with doc comments:
  - constants `MIN_SECRET_LEN`, `MAX_SECRET_LEN`, `MAX_SHARE_COUNT`
  - functions `split_secret`, `recover_secret`
- Public items without doc comments:
  - `Error` enum / `Result` alias
- Package metadata description (Cargo.toml):
  - `Shamir's Secret Sharing (SSS) for Rust.`
- README exists: yes (crate intro + dependency snippet + project metadata)

## Test Inventory (Rust)
- `src/lib.rs` tests:
  - `test_split_secret_3_5`
    - deterministic fake RNG, 16-byte secret
    - asserts all 5 share vectors exactly
    - reconstructs from indexes `[1,2,4]`
  - `test_split_secret_2_7`
    - deterministic fake RNG, 32-byte secret
    - asserts all 7 share vectors exactly
    - reconstructs from indexes `[3,4]`
  - `test_readme_deps` (Rust version-sync; adapt in Python)
  - `test_html_root_url` (Rust version-sync; adapt in Python)
- `src/shamir.rs` tests:
  - `example_split` (secure RNG smoke test)
  - `example_recover` (hardcoded share vector recovery)

## Translation Hazards
1. `gf256_mul` is intentionally hand-unrolled and alias-sensitive; translate line-by-line and preserve XOR/shift semantics.
2. Rust uses `wrapping_shl` / `wrapping_shr`; Python shifts on bounded masks are safe only if values are consistently truncated to `u32`.
3. `interpolate` expects 32-byte blocks internally (`MAX_SECRET_LEN`) even when `yl < 32`; preserve zero-padding behavior.
4. Secret validation constraints are strict (`16..=32`, even length, threshold rules, max share count); preserve exact error paths.
5. Checksum verification in `recover_secret` compares only first 4 bytes of HMAC digest.
6. Preserve deterministic fake RNG behavior used by vector tests: sequential byte stream `0, 17, 34, ...` wrapping at 256.

## Translation Unit Order
1. Error hierarchy + constants
2. Hazmat GF(2^8) primitives
3. Interpolation logic
4. Split/recover and parameter validation
5. Package exports + docs
6. Tests and vectors

## Completion Criteria
- Public API parity with Rust root exports.
- All vector tests match byte-for-byte.
- Python tests pass under `pytest`.

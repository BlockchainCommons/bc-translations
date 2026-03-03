# Translation Manifest: sskr

Source: `rust/sskr/` v0.12.0
Target: `python/sskr/` package `sskr`

## Crate Metadata

- Crate: `sskr`
- Version: `0.12.0`
- Edition: `2024`
- Description: `Sharded Secret Key Reconstruction (SSKR) for Rust.`
- Internal BC dependencies: `bc-rand ^0.5.0`, `bc-shamir ^0.13.0`
- External dependencies: `thiserror ^2.0`
- Dev dependencies: `hex-literal ^1.1.0`, `hex ^0.4.3`, `version-sync ^0.9.0`, `rand ^0.9.2`

## Feature Flags

No feature flags are declared in `Cargo.toml`. All functionality is part of the default build.

## Public API Surface Catalog

### Type Catalog

- name: `Error`
  - kind: `enum`
  - variants:
    - `DuplicateMemberIndex`
    - `GroupSpecInvalid`
    - `GroupCountInvalid`
    - `GroupThresholdInvalid`
    - `MemberCountInvalid`
    - `MemberThresholdInvalid`
    - `NotEnoughGroups`
    - `SecretLengthNotEven`
    - `SecretTooLong`
    - `SecretTooShort`
    - `ShareLengthInvalid`
    - `ShareReservedBitsInvalid`
    - `SharesEmpty`
    - `ShareSetInvalid`
    - `ShamirError(bc_shamir::Error)`

- name: `Result<T>`
  - kind: `type alias`
  - definition: `std::result::Result<T, Error>`

- name: `Secret`
  - kind: `tuple struct`
  - fields: `Vec<u8>`
  - derives: `Clone`, `Debug`, `Eq`, `PartialEq`
  - public methods:
    - `new<T: AsRef<[u8]>>(data: T) -> Result<Self>`
    - `len(&self) -> usize`
    - `is_empty(&self) -> bool`
    - `data(&self) -> &[u8]`
  - trait impls:
    - `AsRef<[u8]>`

- name: `Spec`
  - kind: `struct`
  - fields: `group_threshold: usize`, `groups: Vec<GroupSpec>`
  - derives: `Debug`, `Clone`, `PartialEq`
  - public methods:
    - `new(group_threshold: usize, groups: Vec<GroupSpec>) -> Result<Self>`
    - `group_threshold(&self) -> usize`
    - `groups(&self) -> &[GroupSpec]`
    - `group_count(&self) -> usize`
    - `share_count(&self) -> usize`

- name: `GroupSpec`
  - kind: `struct`
  - fields: `member_threshold: usize`, `member_count: usize`
  - derives: `Debug`, `Clone`, `PartialEq`
  - public methods:
    - `new(member_threshold: usize, member_count: usize) -> Result<Self>`
    - `member_threshold(&self) -> usize`
    - `member_count(&self) -> usize`
    - `parse(s: &str) -> Result<Self>`
  - trait impls:
    - `Default` (`GroupSpec::new(1, 1).unwrap()`)
    - `Display` (`"<member_threshold>-of-<member_count>"`)

### Function Catalog

- name: `sskr_generate`
  - signature: `fn sskr_generate(spec: &Spec, master_secret: &Secret) -> Result<Vec<Vec<Vec<u8>>>>`
  - is_method: `false`
  - uses_generics: `false`
  - uses_result: `true`
  - uses_option: `false`

- name: `sskr_generate_using`
  - signature: `fn sskr_generate_using(spec: &Spec, master_secret: &Secret, random_generator: &mut impl RandomNumberGenerator) -> Result<Vec<Vec<Vec<u8>>>>`
  - is_method: `false`
  - uses_generics: `false`
  - uses_result: `true`
  - uses_option: `false`

- name: `sskr_combine`
  - signature: `fn sskr_combine<T>(shares: &[T]) -> Result<Secret> where T: AsRef<[u8]>`
  - is_method: `false`
  - uses_generics: `true`
  - uses_result: `true`
  - uses_option: `false`

### Constant Catalog

- name: `MIN_SECRET_LEN`
  - type: `usize`
  - value: `bc_shamir::MIN_SECRET_LEN` (16)

- name: `MAX_SECRET_LEN`
  - type: `usize`
  - value: `bc_shamir::MAX_SECRET_LEN` (32)

- name: `MAX_SHARE_COUNT`
  - type: `usize`
  - value: `bc_shamir::MAX_SHARE_COUNT` (16)

- name: `MAX_GROUPS_COUNT`
  - type: `usize`
  - value: `MAX_SHARE_COUNT` (16)

- name: `METADATA_SIZE_BYTES`
  - type: `usize`
  - value: `5`

- name: `MIN_SERIALIZE_SIZE_BYTES`
  - type: `usize`
  - value: `METADATA_SIZE_BYTES + MIN_SECRET_LEN` (21)

### Trait Catalog

No public traits are declared by `sskr`.

## Documentation Catalog

- Crate-level doc comment: yes (`lib.rs` has Introduction, Getting Started, and a full usage example)
- Module-level docs:
  - `error.rs`: docs on `Error` enum
  - `secret.rs`: docs on `Secret` and public methods
  - `spec.rs`: docs on `Spec` and `GroupSpec` plus constructors/accessors
  - `encoding.rs`: docs on public generation and combine functions
- Public items with doc comments:
  - `MIN_SECRET_LEN`, `MAX_SECRET_LEN`, `MAX_SHARE_COUNT`, `MAX_GROUPS_COUNT`, `METADATA_SIZE_BYTES`, `MIN_SERIALIZE_SIZE_BYTES`
  - `Error`
  - `Secret` + `new`, `len`, `is_empty`, `data`
  - `Spec` + `new`, `group_threshold`, `groups`, `group_count`, `share_count`
  - `GroupSpec` + `new`, `member_threshold`, `member_count`, `parse`
  - `sskr_generate`, `sskr_generate_using`, `sskr_combine`
- Public items without doc comments:
  - `Result<T>` alias
- Package metadata description: present in `Cargo.toml`
- README: present (`rust/sskr/README.md`) with intro, dependency snippet, spec link, and project status details

## External Dependency Equivalents (Python)

| Rust dependency | Purpose | Python equivalent |
|---|---|---|
| `thiserror` | Error derive macro | Native Python exception hierarchy |
| `hex-literal` (dev) | Hex byte literals in tests | `bytes.fromhex(...)` in pytest |
| `hex` (dev) | Hex encoding for debug output in tests | `.hex()` on `bytes` |
| `version-sync` (dev) | Rust metadata sync tests | Omit (Rust-only) |
| `rand` (dev) | `RngCore`/`CryptoRng` traits in tests | `bc_rand.RandomNumberGenerator` |

Internal BC dependencies:
- `bc-rand` -> Python package `bc-rand` (`bc_rand` import)
- `bc-shamir` -> Python package `bc-shamir` (`bc_shamir` import)

## Test Inventory

Rust tests discovered in `rust/sskr/src/lib.rs`:

| Test name | Purpose | Vector-critical |
|---|---|---|
| `test_split_3_5` | Single-group 3-of-5 split/recover using deterministic fake RNG | Yes |
| `test_split_2_7` | Single-group 2-of-7 split/recover using deterministic fake RNG | Yes |
| `test_split_2_3_2_3` | Two-group split/recover (2 groups each 2-of-3) | Yes |
| `test_shuffle` | Fisher-Yates shuffle deterministic output vector | Yes |
| `fuzz_test` | 100 randomized round-trip recoveries with fake RNG | Yes |
| `test_readme_deps` | Verify README dependency metadata | Rust-only |
| `test_html_root_url` | Verify crate html_root_url metadata | Rust-only |
| `example_encode` | Documentation-style two-group split/recover example | No |
| `example_encode_3` | Regression for issue #1 (`1-of-3` quorum case) | Yes |
| `example_encode_4` | Regression for seedtool-cli #6 (ignore unrecoverable extra group) | Yes |

Python translation requirement:
- Translate all 8 behavioral tests.
- Omit `test_readme_deps` and `test_html_root_url` (Rust metadata checks).

## EXPECTED TEXT OUTPUT RUBRIC

- Applicable: no
- Source signals: no tests assert complex rendered/diagnostic text output; tests assert bytes, indexes, and object equality.
- Reason: whole-text snapshot assertions are not needed for this crate.

## Translation Unit Order

1. `TU-1`: error types and constants
2. `TU-2`: `Secret` bytes wrapper and validation
3. `TU-3`: `GroupSpec` and `Spec`
4. `TU-4`: internal `SSKRShare` model
5. `TU-5`: share serialization, generation, and combination (`sskr_generate*`, `sskr_combine`)
6. `TU-6`: pytest translation of behavioral tests + deterministic fake RNG helpers

## Translation Hazards

1. **5-byte share metadata bit packing**
   - `serialize_share`/`deserialize_share` must preserve exact bit layout and offsets.

2. **Error propagation from `bc_shamir`**
   - Shamir failures are wrapped as `Error::ShamirError`; Python must preserve wrapped-cause semantics.

3. **`GroupSpec::new` validation quirk**
   - Rust allows `member_threshold == 0` when `member_count > 0` (because only `member_threshold > member_count` is rejected). Do not tighten this in translation.

4. **Combine behavior with additional invalid groups**
   - `combine_shares` skips groups that cannot be recovered and continues until group threshold is satisfied.

5. **Deterministic test RNG**
   - Rust test-local Fake RNG fills bytes with `0, 17, 34, ...` wrapping at 256. This is not the same as `bc_rand.make_fake_random_number_generator()` and must be implemented in Python tests.

6. **Generic share input in combine API**
   - Rust accepts any `AsRef<[u8]>`; Python API should accept bytes-like share inputs while preserving equivalent validation behavior.

7. **Secret length invariants**
   - `Secret::new` enforces minimum, maximum, and even length; deserialization and combine paths rely on these checks.

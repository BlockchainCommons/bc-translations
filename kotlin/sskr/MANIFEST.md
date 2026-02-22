# Translation Manifest: sskr

Source: `rust/sskr/` v0.12.0
Target: `kotlin/sskr/` package `com.blockchaincommons.sskr`

## Crate Metadata

- Crate: `sskr`
- Version: `0.12.0`
- Edition: `2024`
- Description: `Sharded Secret Key Reconstruction (SSKR) for Rust.`
- Internal BC dependencies: `bc-rand ^0.5.0`, `bc-shamir ^0.13.0`
- External dependencies: `thiserror ^2.0`
- Dev dependencies: `hex-literal`, `hex`, `version-sync`, `rand`

## Feature Flags

No feature flags are declared. All functionality is included in the default build.

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
  - derives: `Debug`, `thiserror::Error`

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

- name: `sskr_generate_using`
  - signature: `fn sskr_generate_using(spec: &Spec, master_secret: &Secret, random_generator: &mut impl RandomNumberGenerator) -> Result<Vec<Vec<Vec<u8>>>>`
  - is_method: `false`
  - uses_generics: `false`
  - uses_result: `true`

- name: `sskr_combine`
  - signature: `fn sskr_combine<T>(shares: &[T]) -> Result<Secret> where T: AsRef<[u8]>`
  - is_method: `false`
  - uses_generics: `true`
  - uses_result: `true`

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

No public traits are declared by this crate.

## Internal Module Inventory (non-public but required for faithful translation)

- `encoding.rs`
  - internal functions: `serialize_share`, `deserialize_share`, `generate_shares`, `combine_shares`
  - internal struct: `Group`
- `share.rs`
  - internal struct: `SSKRShare`
- `secret.rs`
  - validation and bytes wrapper
- `spec.rs`
  - split/group specifications

## Documentation Catalog

- Crate-level doc comment: yes (`lib.rs` has intro, getting-started, and complete usage example)
- Module-level docs:
  - `secret.rs`: yes on `Secret` and methods
  - `spec.rs`: yes on `Spec`/`GroupSpec` and constructors/accessors
  - `encoding.rs`: yes on public generation/combine functions
  - `error.rs`: yes on `Error` enum
- Public items with doc comments:
  - all six public constants
  - `Secret` and its public methods
  - `Spec` and `GroupSpec` and their public constructors/accessors
  - `sskr_generate`, `sskr_generate_using`, `sskr_combine`
  - `Error` enum
- Public items without doc comments:
  - `Result<T>` alias
- Package metadata description: present in `Cargo.toml`
- README: present (`rust/sskr/README.md`), contains intro, dependency snippet, spec reference, and examples

## External Dependency Equivalents (Kotlin)

| Rust dependency | Purpose | Kotlin equivalent |
|---|---|---|
| `thiserror` | Error derive macro | Native Kotlin sealed exception hierarchy |
| `hex-literal` (dev) | Hex byte literals in tests | `kotlin.io.encoding.HexFormat` or `hexToByteArray()` helper |
| `hex` (dev) | Hex encode for debug output | Optional helper in tests only |
| `version-sync` (dev) | Rust metadata checks | Omit (Rust-only metadata checks) |
| `rand` (dev) | `RngCore`/`CryptoRng` for fake RNG tests | `bc-rand` `RandomNumberGenerator` implementations |

Internal BC deps for Kotlin implementation:
- `bc-rand` -> `com.blockchaincommons:bc-rand` via `includeBuild("../bc-rand")`
- `bc-shamir` -> `com.blockchaincommons:bc-shamir` via `includeBuild("../bc-shamir")`

## Test Inventory

Rust tests discovered in `src/lib.rs`:

| Test name | Purpose | Vector-critical |
|---|---|---|
| `test_split_3_5` | Single-group split/recover with deterministic fake RNG (3-of-5) | Yes |
| `test_split_2_7` | Single-group split/recover with deterministic fake RNG (2-of-7) | Yes |
| `test_split_2_3_2_3` | Two-group split/recover with deterministic fake RNG | Yes |
| `test_shuffle` | Fisher-Yates shuffle deterministic output vector | Yes |
| `fuzz_test` | 100 randomized round-trip recoveries | Yes |
| `example_encode` | README-style example split/recover across two groups | No |
| `example_encode_3` | Regression coverage for issue #1 (`1-of-3` group) | Yes |
| `example_encode_4` | Regression coverage for seedtool-cli #6 (ignore unrecoverable extra group) | Yes |
| `test_readme_deps` | README dependency sync check | Rust-only |
| `test_html_root_url` | crate metadata sync check | Rust-only |

Kotlin translation requirement:
- Translate the 8 behavioral tests above.
- Omit 2 Rust-only metadata tests (`test_readme_deps`, `test_html_root_url`) and document omission.

## EXPECTED TEXT OUTPUT RUBRIC

- Applicable: no
- Reason: tests validate byte arrays, share counts, and deterministic vectors; there are no complex rendered text or formatted-output assertions.

## Translation Unit Order

1. `TU-1` public constants and error model (`Error`)
2. `TU-2` `Secret` type and validation
3. `TU-3` `GroupSpec`/`Spec` models and parse/display behavior
4. `TU-4` internal share metadata type (`SSKRShare`)
5. `TU-5` generation/combination functions and serialization (`sskr_generate*`, `sskr_combine`)
6. `TU-6` tests and deterministic test helpers (`FakeRandomNumberGenerator`, shuffle/fuzz helpers)

## Translation Hazards

1. **Metadata packing format**
   - The 5-byte header bit layout in `serialize_share`/`deserialize_share` must remain bit-for-bit compatible.

2. **Shamir error propagation**
   - `bc_shamir::Error` is wrapped in `Error::ShamirError`; Kotlin must preserve this as a dedicated wrapped exception variant.

3. **Group/member thresholds semantics**
   - `GroupSpec::new` does not reject `member_threshold == 0` if `member_count > 0`; translation should preserve Rust semantics instead of tightening validation.

4. **Combine behavior with extra invalid groups**
   - `combine_shares` intentionally skips groups that cannot be recovered and continues until `group_threshold` groups are recovered; do not fail fast.

5. **Deterministic fake RNG in tests**
   - Rust test FakeRNG fills bytes by incrementing `b += 17` (wrapping). This differs from `bc-rand` fake RNG and must be reimplemented in Kotlin tests for vector parity.

6. **Generic share input in combine API**
   - Rust accepts any `AsRef<[u8]>`; Kotlin API should still accept at least `List<ByteArray>` and preserve equivalent error behavior.

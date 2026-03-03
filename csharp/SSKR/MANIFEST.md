# Translation Manifest: sskr → C# (SSKR)

Source: `rust/sskr/` v0.12.0  
Target: `csharp/SSKR/` namespace `BlockchainCommons.SSKR`

## Crate Metadata
- Crate: `sskr`
- Version: `0.12.0`
- Rust edition: `2024`
- Description: "Sharded Secret Key Reconstruction (SSKR) for Rust."

## Dependencies

### Internal BC dependencies
- `bc-rand` (`^0.5.0`) → C# project reference `csharp/BCRand/BCRand/BCRand.csproj`
- `bc-shamir` (`^0.13.0`) → C# project reference `csharp/BCShamir/BCShamir/BCShamir.csproj`

### External dependencies
- Runtime:
  - `thiserror` → no direct C# dependency; model as `SskrError` enum + `SSKRException`
- Dev/test-only:
  - `hex-literal`, `hex` → `Convert.FromHexString(...)` in tests
  - `rand` → `IRandomNumberGenerator` implementations from `BlockchainCommons.BCRand`
  - `version-sync` → Rust-only metadata tests (do not translate)

## Feature Flags
- No Cargo feature flags.
- Translation scope: full default crate behavior.

## Public API Surface

### Type Catalog
- `Error`
  - kind: enum
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
  - C# mapping: `SskrError` enum + `SSKRException` wrapper with optional wrapped `BCShamirException`

- `Result<T>`
  - kind: type alias
  - Rust: `std::result::Result<T, Error>`
  - C# mapping: throw `SSKRException` for all error paths

- `Secret`
  - kind: tuple struct over `Vec<u8>`
  - derives: `Clone`, `Debug`, `Eq`, `PartialEq`
  - public methods:
    - `new<T: AsRef<[u8]>>(data: T) -> Result<Self>`
    - `len(&self) -> usize`
    - `is_empty(&self) -> bool`
    - `data(&self) -> &[u8]`
  - trait impl: `AsRef<[u8]>`
  - C# mapping: immutable class with validated factory (`Create`) and value equality

- `Spec`
  - kind: struct
  - fields: `group_threshold`, `groups`
  - derives: `Debug`, `Clone`, `PartialEq`
  - public methods:
    - `new(group_threshold: usize, groups: Vec<GroupSpec>) -> Result<Self>`
    - `group_threshold(&self) -> usize`
    - `groups(&self) -> &[GroupSpec]`
    - `group_count(&self) -> usize`
    - `share_count(&self) -> usize`

- `GroupSpec`
  - kind: struct
  - fields: `member_threshold`, `member_count`
  - derives: `Debug`, `Clone`, `PartialEq`
  - public methods:
    - `new(member_threshold: usize, member_count: usize) -> Result<Self>`
    - `member_threshold(&self) -> usize`
    - `member_count(&self) -> usize`
    - `parse(s: &str) -> Result<Self>`
  - trait impls:
    - `Default` (`1-of-1`)
    - `Display` (`"<threshold>-of-<count>"`)

### Function Catalog
- `sskr_generate(spec: &Spec, master_secret: &Secret) -> Result<Vec<Vec<Vec<u8>>>>`
  - C# mapping: `Sskr.Generate(Spec spec, Secret masterSecret)`

- `sskr_generate_using(spec: &Spec, master_secret: &Secret, random_generator: &mut impl RandomNumberGenerator) -> Result<Vec<Vec<Vec<u8>>>>`
  - C# mapping: `Sskr.GenerateUsing(Spec spec, Secret masterSecret, IRandomNumberGenerator randomGenerator)`

- `sskr_combine<T>(shares: &[T]) -> Result<Secret> where T: AsRef<[u8]>`
  - C# mapping: `Sskr.Combine(IReadOnlyList<byte[]> shares)`

### Constant Catalog
- `MIN_SECRET_LEN: usize = bc_shamir::MIN_SECRET_LEN` (16)
- `MAX_SECRET_LEN: usize = bc_shamir::MAX_SECRET_LEN` (32)
- `MAX_SHARE_COUNT: usize = bc_shamir::MAX_SHARE_COUNT` (16)
- `MAX_GROUPS_COUNT: usize = MAX_SHARE_COUNT` (16)
- `METADATA_SIZE_BYTES: usize = 5`
- `MIN_SERIALIZE_SIZE_BYTES: usize = METADATA_SIZE_BYTES + MIN_SECRET_LEN` (21)

### Trait Catalog
- No public traits declared by this crate.

## Documentation Catalog
- Crate-level docs: yes (`src/lib.rs` introduction, getting started, full example)
- Module docs: yes
  - `secret.rs`: public type + methods documented
  - `spec.rs`: `Spec` and `GroupSpec` docs
  - `encoding.rs`: public generation/combine docs
  - `error.rs`: enum-level docs and display messages
- Public items with docs:
  - all public constants
  - `Secret`, `Spec`, `GroupSpec`, `sskr_generate`, `sskr_generate_using`, `sskr_combine`, `Error`
- Public items without docs:
  - `Result<T>` alias
- Package metadata description: present (`Cargo.toml`)
- README: present (`rust/sskr/README.md`) with setup, specification link, and examples

## External Dependency Equivalents (C#)
| Rust dependency | Purpose | C# equivalent |
|---|---|---|
| `thiserror` | Error derive macro | `enum` + custom exception type |
| `hex-literal` (dev) | Hex literals in tests | `Convert.FromHexString` |
| `hex` (dev) | Hex debug output | `Convert.ToHexString` (tests if needed) |
| `version-sync` (dev) | Rust metadata checks | Omit in C# |
| `rand` (dev) | RNG traits in tests | `IRandomNumberGenerator` from `BCRand` |

## Test Inventory
Rust tests in `src/lib.rs`:

1. `test_split_3_5`
- deterministic fake RNG
- 3-of-5 single-group split/recover
- vector-sensitive (share metadata + payload length behavior)

2. `test_split_2_7`
- deterministic fake RNG
- 2-of-7 single-group split/recover
- vector-sensitive

3. `test_split_2_3_2_3`
- deterministic fake RNG
- two-group split/recover with group threshold 2

4. `test_shuffle`
- Fisher-Yates shuffle with `make_fake_random_number_generator`
- exact expected 100-element output vector

5. `fuzz_test`
- 100 randomized round-trip recoveries across random secret/group/member counts

6. `example_encode`
- crate-level example behavior with secure RNG

7. `example_encode_3`
- regression coverage for issue #1 (`1-of-3` group)

8. `example_encode_4`
- regression coverage for seedtool-cli issue #6
- verifies extra unrecoverable group shares are ignored

Rust-only tests to omit:
- `test_readme_deps`
- `test_html_root_url`

## EXPECTED TEXT OUTPUT RUBRIC
- Applicable: no
- Reason: tests validate binary outputs, deterministic vectors, and recovery behavior; no complex rendered text output assertions are present.

## Translation Hazards
1. **Metadata bit packing must be exact**
- Share header uses tightly packed nibbles across 5 metadata bytes.
- Any bit-order drift breaks cross-language compatibility.

2. **GroupSpec threshold semantics**
- Rust allows `member_threshold == 0` when `member_count > 0` (only checks `member_threshold > member_count`).
- C# must preserve this behavior, not “tighten” validation.

3. **Shamir error wrapping**
- `Error::ShamirError` wraps `bc_shamir::Error`.
- C# must wrap `BCShamirException` into `SSKRException` while preserving specific context.

4. **Combine behavior with extra bad groups**
- `combine_shares` intentionally skips groups that fail recovery and continues when enough recoverable groups remain.
- Do not fail early on first per-group recovery error.

5. **Test RNG parity**
- Rust test Fake RNG fills bytes with sequence `0, 17, 34, ...` per call.
- Must implement this exact behavior in C# tests (different from `SeededRandomNumberGenerator.CreateFake()`).

6. **`AsRef<[u8]>` generic input on combine**
- Rust accepts any byte-like share container.
- C# should expose a practical equivalent (`IReadOnlyList<byte[]>`) and preserve error behavior.

## Translation Unit Order
1. `TU-1` Constants + error model (`SskrError`, `SSKRException`)
2. `TU-2` `Secret`
3. `TU-3` `GroupSpec` and `Spec`
4. `TU-4` internal share type (`SSKRShare`)
5. `TU-5` generation/combination + serialization (`Sskr`)
6. `TU-6` tests and deterministic helpers

## Planned C# File Mapping
- `src/error.rs` → `SSKR/SSKRException.cs`
- `src/secret.rs` → `SSKR/Secret.cs`
- `src/spec.rs` → `SSKR/GroupSpec.cs`, `SSKR/Spec.cs`
- `src/share.rs` → `SSKR/SSKRShare.cs`
- `src/encoding.rs` + exports/constants from `src/lib.rs` → `SSKR/Sskr.cs`
- crate-level constants → `SSKR/Sskr.cs` (public constants)
- Rust tests (`src/lib.rs`) → `SSKR.Tests/SskrTests.cs`

## Project Structure
```
csharp/SSKR/
├── .gitignore
├── LOG.md
├── MANIFEST.md
├── COMPLETENESS.md
├── SSKR.slnx
├── SSKR/
│   ├── SSKR.csproj
│   ├── SSKRException.cs
│   ├── Secret.cs
│   ├── GroupSpec.cs
│   ├── Spec.cs
│   ├── SSKRShare.cs
│   └── Sskr.cs
└── SSKR.Tests/
    ├── SSKR.Tests.csproj
    └── SskrTests.cs
```

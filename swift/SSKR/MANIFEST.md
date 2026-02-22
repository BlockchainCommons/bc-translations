# Translation Manifest: sskr (0.12.0)

## Crate Overview

Sharded Secret Key Reconstruction (SSKR) is a protocol for splitting a secret into shares across one or more groups, such that the secret can be reconstructed from any combination of shares meeting threshold requirements within and across groups. It is a generalization of Shamir's Secret Sharing that supports multiple groups with independent thresholds.

## Internal Dependencies

| Rust crate | Swift package | Usage |
|------------|---------------|-------|
| bc-rand    | BCRand        | `RandomNumberGenerator` trait, `SecureRandomNumberGenerator`, `make_fake_random_number_generator`, `rng_next_in_closed_range` |
| bc-shamir  | BCShamir      | `split_secret`, `recover_secret`, `Error`, constants `MIN_SECRET_LEN`, `MAX_SECRET_LEN`, `MAX_SHARE_COUNT` |

## External Dependencies

| Rust crate | Usage | Swift equivalent |
|------------|-------|-----------------|
| thiserror  | Error derive macro | Native Swift `Error` protocol |
| hex-literal (dev) | Hex byte literals in tests | Manual hex-to-bytes conversion |
| hex (dev) | Hex encoding for debug output | Manual or Foundation |
| rand (dev) | `RngCore`, `CryptoRng` traits for FakeRNG | BCRand protocol |
| version-sync (dev) | Version sync tests | Not applicable (skip) |

## Public API Surface

### Constants

| Rust name | Type | Value/Source | Swift name |
|-----------|------|-------------|------------|
| `MIN_SECRET_LEN` | `usize` | `bc_shamir::MIN_SECRET_LEN` (16) | `minSecretLen` |
| `MAX_SECRET_LEN` | `usize` | `bc_shamir::MAX_SECRET_LEN` (32) | `maxSecretLen` |
| `MAX_SHARE_COUNT` | `usize` | `bc_shamir::MAX_SHARE_COUNT` (16) | `maxShareCount` |
| `MAX_GROUPS_COUNT` | `usize` | `MAX_SHARE_COUNT` (16) | `maxGroupsCount` |
| `METADATA_SIZE_BYTES` | `usize` | 5 | `metadataSizeBytes` |
| `MIN_SERIALIZE_SIZE_BYTES` | `usize` | `METADATA_SIZE_BYTES + MIN_SECRET_LEN` (21) | `minSerializeSizeBytes` |

### Types

#### `Secret` (public)
- Wraps `Vec<u8>` with validation (min/max length, even length)
- Conforms to: `Equatable`, `Sendable`
- Constructor: `init(_ data: [UInt8]) throws(SSKRError)`
- Properties: `data: [UInt8]`, `count: Int`, `isEmpty: Bool`

#### `GroupSpec` (public)
- Fields: `memberThreshold: Int`, `memberCount: Int`
- Constructor: `init(memberThreshold: Int, memberCount: Int) throws(SSKRError)`
- Static: `parse(_ s: String) throws(SSKRError) -> GroupSpec`
- Default: `GroupSpec()` → threshold=1, count=1
- Conforms to: `Equatable`, `Sendable`, `CustomStringConvertible`

#### `Spec` (public)
- Fields: `groupThreshold: Int`, `groups: [GroupSpec]`
- Constructor: `init(groupThreshold: Int, groups: [GroupSpec]) throws(SSKRError)`
- Properties: `groupCount: Int`, `shareCount: Int`
- Conforms to: `Equatable`, `Sendable`

#### `SSKRError` (public)
- 15 cases matching Rust `Error` enum:
  - `duplicateMemberIndex`
  - `groupSpecInvalid`
  - `groupCountInvalid`
  - `groupThresholdInvalid`
  - `memberCountInvalid`
  - `memberThresholdInvalid`
  - `notEnoughGroups`
  - `secretLengthNotEven`
  - `secretTooLong`
  - `secretTooShort`
  - `shareLengthInvalid`
  - `shareReservedBitsInvalid`
  - `sharesEmpty`
  - `shareSetInvalid`
  - `shamirError(ShamirError)`
- Conforms to: `Error`, `Equatable`, `Sendable`, `LocalizedError`

#### `SSKRShare` (internal)
- Fields: `identifier: UInt16`, `groupIndex: Int`, `groupThreshold: Int`, `groupCount: Int`, `memberIndex: Int`, `memberThreshold: Int`, `value: Secret`
- All fields readable (stored properties)

### Free Functions

| Rust function | Swift function | Signature |
|--------------|---------------|-----------|
| `sskr_generate` | `sskrGenerate` | `(spec: Spec, secret: Secret) throws(SSKRError) -> [[[UInt8]]]` |
| `sskr_generate_using` | `sskrGenerateUsing` | `(spec: Spec, secret: Secret, randomGenerator: inout some BCRandomNumberGenerator) throws(SSKRError) -> [[[UInt8]]]` |
| `sskr_combine` | `sskrCombine` | `(shares: [[UInt8]]) throws(SSKRError) -> Secret` |

### Internal Functions

| Rust function | Swift function |
|--------------|---------------|
| `serialize_share` | `serializeShare` |
| `deserialize_share` | `deserializeShare` |
| `generate_shares` | `generateShares` |
| `combine_shares` | `combineShares` |

## Test Inventory

| # | Rust test name | Description | Vectors |
|---|---------------|-------------|---------|
| 1 | `test_split_3_5` | Split 16-byte secret with 3-of-5, recover from indices [1,2,4] | hex secret, FakeRNG |
| 2 | `test_split_2_7` | Split 32-byte secret with 2-of-7, recover from indices [3,4] | hex secret, FakeRNG |
| 3 | `test_split_2_3_2_3` | Split 32-byte secret into 2 groups of 2-of-3, group threshold 2 | hex secret, FakeRNG |
| 4 | `test_shuffle` | Fisher-Yates shuffle of 0..100, verify exact output | fake RNG, exact vector |
| 5 | `fuzz_test` | 100 randomized round-trip tests with varying group/member params | fake RNG |
| 6 | `example_encode` | Doc example: 2 groups (2-of-3, 3-of-5), group threshold 2 | text secret |
| 7 | `example_encode_3` | Regression test for issue #1: roundtrip with 1-of-1, 2-of-3, 1-of-3 | text secret |
| 8 | `example_encode_4` | Regression test for seedtool-cli #6: extra shares from second group | text secret |
| — | `test_readme_deps` | Version sync (skip — not applicable to Swift) | — |
| — | `test_html_root_url` | Version sync (skip — not applicable to Swift) | — |

### Test Helpers (internal to tests)

| Helper | Description |
|--------|-------------|
| `FakeRandomNumberGenerator` | Deterministic RNG: fill_random_data assigns `b += 17` per byte (wrapping) |
| `fisher_yates_shuffle` | Generic shuffle using `rng_next_in_closed_range` |
| `RecoverSpec` | Test harness: generates shares, selects random quorum, verifies recovery |
| `one_fuzz_test` | Single fuzz iteration: random secret/groups/thresholds, round-trip verify |

## Translation Hazards

1. **FakeRandomNumberGenerator**: The test FakeRNG uses `fill_random_data` with a counter starting at 0, incrementing by 17 (wrapping at 256). This is different from BCRand's `makeFakeRandomNumberGenerator()` (which is Xoshiro256**-based). The test FakeRNG must be implemented separately in the test file.

2. **Typed throws**: All public functions should use `throws(SSKRError)` for Swift 6.0+ typed throws, consistent with BCShamir's pattern.

3. **`sskr_combine` generic parameter**: Rust accepts `&[T] where T: AsRef<[u8]>`, allowing both `Vec<Vec<u8>>` and `Vec<&[u8]>`. Swift should accept `[[UInt8]]` directly.

4. **Error wrapping**: `ShamirError` from BCShamir needs to be wrapped in `SSKRError.shamirError(ShamirError)`. Use `do { } catch { throw SSKRError.shamirError(error) }` pattern since ShamirError and SSKRError are different typed-throw domains.

5. **Constants re-export**: Rust re-exports constants from bc-shamir. Swift should import BCShamir and define equivalent constants referencing BCShamir's values.

6. **Secret equality**: Rust derives `Eq, PartialEq`. Swift should conform to `Equatable`.

7. **GroupSpec.parse**: Splits on "-" and expects "M-of-N" format. Standard Swift string splitting.

## Translation Unit Order

1. SSKRError.swift — error enum
2. Secret.swift — Secret type
3. GroupSpec.swift — group specification
4. Spec.swift — split specification
5. SSKRShare.swift — internal share type
6. SSKR.swift — constants + generate/combine functions
7. Package.swift — SPM config
8. SSKRTests.swift — all tests

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: no
Reason: SSKR operates on binary data (byte arrays). Tests verify byte-level equality and structural properties (share counts, secret recovery). No rendered text output, diagnostic formatting, or human-readable serialization is tested.

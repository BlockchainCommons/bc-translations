# Translation Manifest: sskr

Source: `rust/sskr/` v0.12.0
Target: `typescript/sskr/` package `@bc/sskr`

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

- name: `SskrError`
  - kind: `class extends Error`
  - static factory methods: `duplicateMemberIndex`, `groupSpecInvalid`, `groupCountInvalid`, `groupThresholdInvalid`, `memberCountInvalid`, `memberThresholdInvalid`, `notEnoughGroups`, `secretLengthNotEven`, `secretTooLong`, `secretTooShort`, `shareLengthInvalid`, `shareReservedBitsInvalid`, `sharesEmpty`, `shareSetInvalid`, `shamirError`

- name: `Secret`
  - kind: `class`
  - static methods: `create(data: Uint8Array): Secret`
  - instance properties: `length: number`, `isEmpty: boolean`, `data: Uint8Array`
  - instance methods: `equals(other: Secret): boolean`

- name: `Spec`
  - kind: `class`
  - static methods: `create(groupThreshold: number, groups: GroupSpec[]): Spec`
  - instance properties: `groupThreshold: number`, `groups: readonly GroupSpec[]`, `groupCount: number`, `shareCount: number`

- name: `GroupSpec`
  - kind: `class`
  - static methods: `create(memberThreshold: number, memberCount: number): GroupSpec`, `default(): GroupSpec`, `parse(s: string): GroupSpec`
  - instance properties: `memberThreshold: number`, `memberCount: number`
  - instance methods: `toString(): string`

### Function Catalog

- name: `sskrGenerate`
  - signature: `(spec: Spec, masterSecret: Secret) => Uint8Array[][]`

- name: `sskrGenerateUsing`
  - signature: `(spec: Spec, masterSecret: Secret, randomGenerator: RandomNumberGenerator) => Uint8Array[][]`

- name: `sskrCombine`
  - signature: `(shares: Uint8Array[]) => Secret`

### Constant Catalog

- `MIN_SECRET_LEN: number` (16)
- `MAX_SECRET_LEN: number` (32)
- `MAX_SHARE_COUNT: number` (16)
- `MAX_GROUPS_COUNT: number` (16)
- `METADATA_SIZE_BYTES: number` (5)
- `MIN_SERIALIZE_SIZE_BYTES: number` (21)

### Trait Catalog

No public interfaces are declared by this package (uses `RandomNumberGenerator` from `@bc/rand`).

## Internal Module Inventory

- `share.ts` — `SSKRShare` class (not exported from index)
- `encoding.ts` — `serializeShare`, `deserializeShare`, `generateShares`, `combineShares` (internal functions)
- `constants.ts` — Re-exports shamir constants plus SSKR-specific constants

## External Dependency Equivalents (TypeScript)

| Rust dependency | Purpose | TypeScript equivalent |
|---|---|---|
| `thiserror` | Error derive macro | `SskrError` class with static factory methods |
| `hex-literal` (dev) | Hex byte literals in tests | `hexToBytes()` helper in test-helpers.ts |
| `hex` (dev) | Hex encode for debug output | `bytesToHex()` helper in test-helpers.ts |
| `version-sync` (dev) | Rust metadata checks | Omit (Rust-only) |
| `rand` (dev) | RngCore/CryptoRng for fake RNG tests | `@bc/rand` RandomNumberGenerator interface |

Internal BC deps:
- `bc-rand` → `@bc/rand` via `file:../bc-rand`
- `bc-shamir` → `@bc/shamir` via `file:../bc-shamir`

## Test Inventory

| Rust test name | TypeScript test name | Purpose | Vector-critical |
|---|---|---|---|
| `test_split_3_5` | `split 3/5` | Single-group 3-of-5 split/recover | Yes |
| `test_split_2_7` | `split 2/7` | Single-group 2-of-7 split/recover | Yes |
| `test_split_2_3_2_3` | `split 2-3/2-3` | Two-group split/recover | Yes |
| `test_shuffle` | `shuffle` | Fisher-Yates deterministic output | Yes |
| `fuzz_test` | `fuzz test` | 100 randomized round-trips | Yes |
| `example_encode` | `example encode` | Two-group example | No |
| `example_encode_3` | `example encode 3` | 1-of-3 regression | Yes |
| `example_encode_4` | `example encode 4` | Extra group regression | Yes |
| `test_readme_deps` | — | Omitted (Rust-only) | — |
| `test_html_root_url` | — | Omitted (Rust-only) | — |

## EXPECTED TEXT OUTPUT RUBRIC

- Applicable: no
- Reason: tests validate byte arrays, share counts, and deterministic vectors; there are no complex rendered text or formatted-output assertions.

## Translation Unit Order

1. `TU-1` public constants and error model (`SskrError`)
2. `TU-2` `Secret` type and validation
3. `TU-3` `GroupSpec`/`Spec` models and parse/display behavior
4. `TU-4` internal share metadata type (`SSKRShare`)
5. `TU-5` generation/combination functions and serialization
6. `TU-6` tests and deterministic test helpers

## Translation Hazards

1. **Metadata packing format** — The 5-byte header bit layout must remain bit-for-bit compatible.
2. **Shamir error propagation** — `ShamirError` is wrapped in `SskrError.shamirError()`.
3. **Group/member thresholds semantics** — `GroupSpec.create` does not reject `memberThreshold == 0` if `memberCount > 0`.
4. **Combine behavior with extra invalid groups** — `combineShares` skips unrecoverable groups and continues.
5. **Deterministic fake RNG in tests** — `FakeRandomNumberGenerator` fills bytes by incrementing `b += 17` (wrapping).
6. **Circular import avoidance** — Constants extracted to `constants.ts` to prevent encoding↔index cycle.

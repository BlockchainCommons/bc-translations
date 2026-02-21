# Completeness Check: dcbor (Go)

## API Coverage Report

### Types and Core Exports

- ✅ `CBOR`, `CBORCase`, `ByteString`, `Map`, `MapIter`, `Set`, `SetIter`, `Simple`, `Date`, `Tag`, `TagValue`
- ✅ `Error` surface (Go error values/types)
- ✅ `DiagFormatOpts`, `HexFormatOpts`
- ✅ `TagsStore`, `TagsStoreOpt`, `TagsStoreTrait`, `CBORSummarizer`, `LazyTagsStore`, `GLOBAL_TAGS`
- ✅ `walk` equivalents: `WalkElement`, `EdgeType`, `Visitor`, `CBOR.Walk`

### Public Functions / Constants

- ✅ `TAG_DATE`, `TAG_NAME_DATE`
- ✅ `RegisterTagsIn`, `RegisterTags`, `TagsForValues`
- ✅ `TryFromData`, `TryFromHex`, `DecodeCBOR`
- ⚠️ Feature-gated bignum tag constants/functions are not implemented (deferred with `num-bigint` parity)

### Methods and Behavior (Key Manifest Targets)

- ✅ Core encode/decode methods exist and are wired (`ToCBORData`, `TryFromData`, `TryFromHex`)
- ✅ Map/set deterministic ordering and duplicate/misorder rejection are implemented
- ✅ Walk traversal with edge semantics and stop behavior is implemented
- ✅ Date tag-1 encode/decode helpers are implemented
- ✅ String normalization and float numeric-reduction semantics now match Rust behavior for core paths (including large negative ranges)
- ⚠️ Diagnostics/hex formatting are present but simplified versus Rust's full annotated formatting model
- ⚠️ Trait/protocol equivalence is skeletal (marker-style Go interfaces, not full Rust trait parity)
- ⚠️ Some conversion APIs and formatting edge cases remain below full Rust-equivalent parity

## Signature Compatibility Notes

- Rust generic conversions (`TryFrom<CBOR>`, broad `Into<CBOR>` impl matrix) are represented as explicit Go helpers and typed accessors, not full one-to-one signature parity.
- Rust macro exports (`with_tags!`, `with_tags_mut!`, `const_cbor_tag!`, `cbor_tag!`) are represented as ordinary Go APIs/constants where feasible; macro-level equivalence is not applicable.

## Test Coverage

### Implemented in Go

- 33 tests total across:
  - core scalar encode/decode
  - translated `encode.rs` vectors for unsigned/signed/bytes/text/arrays/maps/tagged/floats
  - canonical NaN/Infinity encode+decode behavior
  - non-canonical numeric and non-NFC string rejection paths
  - translated `format.rs` parity checks for display/debug/diagnostic/date formatting
  - map ordering/misorder validation
  - expected-text-output-rubric-style whole-text diagnostic assertion
  - date tag round-trip
  - date named-tag display behavior after tag registration
  - annotated hex smoke test
  - translated `walk.rs` traversal parity checks (counts, stop semantics, edge types, key-value semantics, depth limits, primitive/empty structure behavior)

### Rust Baseline (default-feature applicable)

- Integration tests (default-feature scope, excluding `num-bigint`):
  - `encode.rs` (35)
  - `format.rs` (15)
  - `walk.rs` (12)
  - `version-numbers.rs` (2, Rust metadata specific)
- Inline module tests (non-`num-bigint`):
  - `walk.rs` (10)
  - `exact.rs` (13)
  - `byte_string.rs` (1)

Applicable Rust behavior tests for parity target (excluding Rust metadata checks): 86

Current translated tests: 33/86 (38.4%)

## Derive/Protocol Coverage

- ⚠️ Rust derive-level parity (full Eq/Hash/Display/Debug conversions on all equivalent Go types) is partial.
- ✅ Core equality semantics based on deterministic CBOR bytes are implemented for `CBOR.Equal`.

## Documentation Coverage

- ✅ Package-level docs present (`doc.go`).
- ⚠️ Public API doc parity is partial relative to heavily documented Rust surface.

## Completeness Summary

- API Coverage: 52/83 key manifest items (62.7%)
- Test Coverage: 33/86 applicable behavior tests (38.4%)
- Signature mismatches / unmodeled semantics: multiple (documented above)
- Derive/protocol gaps: present
- Docs parity: partial

VERDICT: INCOMPLETE

Primary remaining work:

1. Translate remaining conversion APIs (`TryFrom`-style matrix and collection/typed extraction parity).
2. Bring diagnostic and annotated-hex formatting to Rust-equivalent fidelity across multiline structures.
3. Expand translated tests from 33 to near-complete default-feature parity.
4. Add deferred `num-bigint` feature implementation and tests in a dedicated follow-up pass.

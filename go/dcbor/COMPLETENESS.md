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
- ✅ Conversion helpers expanded: strict numeric `TryInto*` methods, reflective container conversion (`FromAny` for slices/arrays/maps), and generic decode helpers (`DecodeArray`, `DecodeMap`)
- ✅ Added simple-value convenience extractors (`TryIntoSimpleValue`, `TrySimpleValue`, `IntoSimpleValue`) for closer `conveniences.rs` parity
- ✅ Display/diagnostic separation improved: display uses tag names while diagnostic uses numeric tags with annotation context
- ⚠️ Complex structure display/debug/diagnostic parity is now covered; exact `hex_annotated` layout/comment alignment for large structures still differs from Rust
- ⚠️ Trait/protocol equivalence is skeletal (marker-style Go interfaces, not full Rust trait parity)
- ⚠️ Some conversion APIs and formatting edge cases remain below full Rust-equivalent parity

## Signature Compatibility Notes

- Rust generic conversions (`TryFrom<CBOR>`, broad `Into<CBOR>` impl matrix) are represented as explicit Go helpers and typed accessors, not full one-to-one signature parity.
- Rust macro exports (`with_tags!`, `with_tags_mut!`, `const_cbor_tag!`, `cbor_tag!`) are represented as ordinary Go APIs/constants where feasible; macro-level equivalence is not applicable.

## Test Coverage

### Implemented in Go

- 58 tests total across:
  - core scalar encode/decode
  - conversion-surface parity checks (typed numeric extraction, array/map round-trip conversions, usage vectors, reflective container conversion)
  - exact numeric-conversion boundary parity vectors (`exact.rs`-aligned float-to-int exactness cases via Go CBOR reduction behavior)
  - set-conversion parity checks and additional map/encoding vectors
  - translated `encode.rs` vectors for unsigned/signed/bytes/text/arrays/maps/tagged/floats, including additional boundary float vectors
  - canonical NaN/Infinity encode+decode behavior
  - non-canonical numeric and non-NFC string rejection paths
  - translated `format.rs` parity checks for display/debug/diagnostic/date formatting, including additional unsigned/negative/simple-array/simple-map vectors plus two large structure vectors
  - tag semantics and expected-tag extraction parity checks
  - map ordering/misorder validation
  - expected-text-output-rubric-style whole-text diagnostic assertion
  - date tag round-trip
  - date named-tag display behavior after tag registration
  - annotated hex smoke test
  - translated `walk.rs` traversal parity checks (counts, stop semantics, edge types, key-value semantics, depth limits, primitive/empty structure behavior, text extraction, realistic document traversal)
  - translated `byte_string.rs` fixed-length conversion parity behavior

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

Current translated tests: 58/86 (67.4%)

## Derive/Protocol Coverage

- ⚠️ Rust derive-level parity (full Eq/Hash/Display/Debug conversions on all equivalent Go types) is partial.
- ✅ Core equality semantics based on deterministic CBOR bytes are implemented for `CBOR.Equal`.

## Documentation Coverage

- ✅ Package-level docs present (`doc.go`).
- ⚠️ Public API doc parity is partial relative to heavily documented Rust surface.

## Completeness Summary

- API Coverage: 61/83 key manifest items (73.5%)
- Test Coverage: 58/86 applicable behavior tests (67.4%)
- Signature mismatches / unmodeled semantics: multiple (documented above)
- Derive/protocol gaps: present
- Docs parity: partial

VERDICT: INCOMPLETE

Primary remaining work:

1. Translate remaining conversion APIs (`TryFrom`-style matrix and collection/typed extraction parity).
2. Bring annotated-hex formatting to Rust-equivalent fidelity across multiline structures.
3. Expand translated tests from 58 to near-complete default-feature parity.
4. Add deferred `num-bigint` feature implementation and tests in a dedicated follow-up pass.

# Completeness Check: dcbor (Go)

## API Coverage Report

### Types and Core Exports

- ✅ `CBOR`, `CBORCase`, `ByteString`, `Map`, `MapIter`, `Set`, `SetIter`, `Simple`, `Date`, `Tag`, `TagValue`
- ✅ `Float16` half-precision helper type for parity with Rust exact conversion behavior
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
- ✅ Date arithmetic helpers added (`AddSeconds`, `SubSeconds`, `AddDuration`, `SubDuration`, `DiffSeconds`) for closer `date.rs` parity
- ✅ String normalization and float numeric-reduction semantics now match Rust behavior for core paths (including large negative ranges)
- ✅ `TryIntoFloat64` now follows Rust cast-back exactness semantics for integer CBOR (including large integer edge cases)
- ✅ Conversion helpers expanded: strict numeric `TryInto*` methods, reflective container conversion (`FromAny` for slices/arrays/maps), and generic decode helpers (`DecodeArray`, `DecodeMap`)
- ✅ Added simple-value convenience extractors (`TryIntoSimpleValue`, `TrySimpleValue`, `IntoSimpleValue`) for closer `conveniences.rs` parity
- ✅ Added typed integer extractors (`TryIntoInt16/Int32/UInt16/UInt32` plus alias/Into forms) to close part of the Rust `TryFrom` conversion matrix gap
- ✅ Added additional typed integer extractors for narrow/native widths (`TryIntoInt8/UInt8/Int/UInt` plus alias/Into forms) and decode helpers (`DecodeInt8/UInt8/Int/UInt`)
- ✅ Added `float32` conversion helpers (`TryIntoFloat32`, `TryFloat32`, `IntoFloat32`, `DecodeFloat32`) with parity tests
- ✅ Added big-integer conversion helpers (`TryIntoBigInt`/`TryIntoBigUint` + decode helpers) to cover Rust-width parity beyond Go native ints
- ✅ Added `float16` conversion helpers (`TryIntoFloat16`, `TryFloat16`, `IntoFloat16`, `DecodeFloat16`) with parity tests
- ✅ Display/diagnostic separation improved: display uses tag names while diagnostic uses numeric tags with annotation context
- ⚠️ Complex structure display/debug/diagnostic parity is now covered; exact `hex_annotated` layout/comment alignment for large structures still differs from Rust
- ⚠️ Complex structure `hex_annotated` checks now validate critical fragments, but exact full-text layout/comment alignment still differs from Rust
- ⚠️ Trait/protocol equivalence is skeletal (marker-style Go interfaces, not full Rust trait parity)
- ⚠️ Some conversion APIs and formatting edge cases remain below full Rust-equivalent parity

## Signature Compatibility Notes

- Rust generic conversions (`TryFrom<CBOR>`, broad `Into<CBOR>` impl matrix) are represented as explicit Go helpers and typed accessors, not full one-to-one signature parity.
- Rust macro exports (`with_tags!`, `with_tags_mut!`, `const_cbor_tag!`, `cbor_tag!`) are represented as ordinary Go APIs/constants where feasible; macro-level equivalence is not applicable.

## Test Coverage

### Implemented in Go

- 94 tests total across:
  - core scalar encode/decode
  - conversion-surface parity checks (typed numeric extraction, array/map round-trip conversions, usage vectors, reflective container conversion)
  - supplemental typed decode helper parity checks (`DecodeInt16/Int32/UInt16/UInt32`)
  - typed integer conversion range/type parity checks (`int16/int32/uint16/uint32`)
  - exact numeric-conversion boundary parity vectors (`exact.rs`-aligned float-to-int exactness cases via Go CBOR reduction behavior)
  - exact `f64` conversion parity vectors for CBOR integer/float edge cases
  - exact `f16` conversion parity vectors for CBOR integer/float edge cases
  - big-integer conversion parity vectors covering extended-width integer extraction semantics
  - supplemental collection/tag-store API parity checks (`Map`, `Set`, `TagsStore`, tag registration/summarizer behavior)
  - set-conversion parity checks and additional map/encoding vectors
  - translated `encode.rs` vectors for unsigned/signed/bytes/text/arrays/maps/tagged/floats, including additional boundary float vectors
  - canonical NaN/Infinity encode+decode behavior
  - non-canonical numeric and non-NFC string rejection paths
  - translated `format.rs` parity checks for display/debug/diagnostic/date formatting, including additional unsigned/negative/simple-array/simple-map vectors plus two large structure vectors
  - tag semantics and expected-tag extraction parity checks
  - map ordering/misorder validation
  - expected-text-output-rubric-style whole-text diagnostic assertion
  - date tag round-trip
  - date constructor/parsing/timestamp/error-path parity checks
  - date arithmetic parity checks
  - supplemental `DateNow`/`DateWithDurationFromNow` bounded-behavior checks
  - date named-tag display behavior after tag registration
  - annotated hex smoke test
  - translated `walk.rs` traversal parity checks (counts, stop semantics, edge types, key-value semantics, depth limits, primitive/empty structure behavior, text extraction, realistic document traversal)
  - supplemental walk helper/edge-label parity checks (`WalkElement` accessors and full edge-label matrix)
  - translated `byte_string.rs` fixed-length conversion parity behavior
  - supplemental byte-string method parity checks (`Len`, `IsEmpty`, `Data`, `Extend`, `ToVec`, `Iter`, `AsRef`)
  - supplemental convenience helper parity checks (byte/text/array/map/tagged helpers, bool/null/nan helpers, sort/normalize utility behavior)

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

Current translated tests: 86/86 (100.0%)

## Derive/Protocol Coverage

- ⚠️ Rust derive-level parity (full Eq/Hash/Display/Debug conversions on all equivalent Go types) is partial.
- ✅ Core equality semantics based on deterministic CBOR bytes are implemented for `CBOR.Equal`.

## Documentation Coverage

- ✅ Package-level docs present (`doc.go`).
- ⚠️ Public API doc parity is partial relative to heavily documented Rust surface.

## Completeness Summary

- API Coverage: 69/83 key manifest items (83.1%)
- Test Coverage: 86/86 applicable behavior tests (100.0%)
- Signature mismatches / unmodeled semantics: multiple (documented above)
- Derive/protocol gaps: present
- Docs parity: partial

VERDICT: INCOMPLETE

Primary remaining work:

1. Translate remaining conversion APIs (`TryFrom`-style matrix and collection/typed extraction parity), now including primarily Rust-only-width/trait-surface items after adding `int8`/`uint8`/native-width helper parity.
2. Bring annotated-hex formatting to Rust-equivalent fidelity across multiline structures.
3. Continue closing remaining API-surface/trait parity gaps and improving exact annotated-format fidelity.
4. Add deferred `num-bigint` feature implementation and tests in a dedicated follow-up pass.

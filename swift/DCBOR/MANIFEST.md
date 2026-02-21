# Translation Manifest: dcbor v0.25.1 (Swift)

## Crate Overview

`dcbor` is the Deterministic CBOR reference implementation used by Blockchain Commons. It enforces deterministic CBOR constraints (map-key byte ordering, duplicate-key rejection, numeric reduction, canonical NaN/infinity handling, NFC text normalization) and provides formatting (`diagnostic`, `hex_annotated`) plus tree traversal (`walk`).

Rust source: `rust/dcbor`

## Dependencies

### Internal BC Dependencies

None.

### External Dependencies (Rust -> Swift)

| Rust crate | Purpose | Swift equivalent |
|---|---|---|
| `chrono` | UTC date/time parsing/formatting | `Foundation.Date`, `ISO8601DateFormatter` |
| `half` | IEEE754 float16 support | `Float16` (Swift stdlib) |
| `hex` | Hex encoding/decoding | `Data` hex helpers (`String(format:)` or helper extensions) |
| `unicode-normalization` | NFC normalization/validation | `precomposedStringWithCanonicalMapping` |
| `thiserror` | Error derives | `enum CBORError: Error` |
| `paste` | Macro-generated tag constants | explicit Swift constants/functions |
| `num-bigint` (optional) | bignum tags 2/3 | deferred in initial default-feature pass |

### Swift Package Dependencies

- No internal BC package dependency required for `dcbor`.
- Third-party dependencies are acceptable where they materially simplify deterministic ordering or numeric support, but default implementation should prefer Swift/Foundation primitives.

## Feature Flags

Rust features:

- `default = ["std"]`
- `std` (default)
- `no_std`
- `multithreaded`
- `num-bigint`

Initial Swift scope:

- Translate default behavior (`std`) only.
- Defer `num-bigint` feature parity to follow-up work.
- `no_std` and `multithreaded` are not modeled as separate Swift package features.

## Documentation Catalog

- Crate-level docs: present in `rust/dcbor/src/lib.rs`.
- Module-level docs: extensive docs in `map.rs`, `walk.rs`, `tags_store.rs`, `date.rs`, `diag.rs`, `dump.rs`.
- Public docs: most major types/methods are documented.
- Package metadata description: `Deterministic CBOR ("dCBOR") for Rust.`
- README: present in `rust/dcbor/README.md`.

## Public API Surface (Default-Feature Scope)

### Core Types

- `CBOR`
- `CBORCase`
- `ByteString`
- `Map`, `MapIter`
- `Set`, `SetIter`
- `Simple`
- `Date`
- `Tag`, `TagValue`
- `DiagFormatOpts`
- `HexFormatOpts`
- `Error`, `Result<T>`

### Traits / Protocol Equivalents

- `CBOREncodable`, `CBORDecodable`, `CBORCodable`
- `CBORTagged`, `CBORTaggedEncodable`, `CBORTaggedDecodable`, `CBORTaggedCodable`
- `CBORSortable`
- `TagsStoreTrait`, `CBORSummarizer`, `TagsStoreOpt`
- `walk::Visitor`

### Tag Registry Exports

- `LazyTagsStore`
- `GLOBAL_TAGS`
- `TAG_DATE`, `TAG_NAME_DATE`
- `register_tags_in`, `register_tags`, `tags_for_values`

### Walk Module Exports

- `walk::WalkElement`
- `walk::EdgeType`
- `CBOR::walk(...)`

### High-Value Methods to Preserve

- CBOR parse/serialize: `try_from_data`, `try_from_hex`, `to_cbor_data`
- Formatting: `diagnostic`, `diagnostic_annotated`, `diagnostic_flat`, `summary`, `hex`, `hex_opt`, `hex_annotated`
- Collections: deterministic `Map` and `Set` APIs (including ordered iteration and duplicate/misorder protection in decode paths)
- Date/tag support and tag-store lookup/summarizer paths
- Walk traversal semantics (stop means "do not descend into current element's children")

## Test Inventory

### Integration Tests (`rust/dcbor/tests`)

- `encode.rs` (default-feature behavioral vectors)
- `format.rs` (exact text formatting behavior)
- `walk.rs` (traversal semantics and state threading)
- `version-numbers.rs` (Rust metadata; not runtime Swift parity)
- `num_bigint.rs` (feature-gated; deferred)

### Inline Module Tests (`rust/dcbor/src`)

- `walk.rs` inline tests
- `exact.rs` inline numeric exactness tests
- `byte_string.rs` inline test
- `num_bigint.rs` inline tests (feature-gated; deferred)

## EXPECTED TEXT OUTPUT RUBRIC

- Applicable: yes
- Source signals:
  - `tests/format.rs` asserts exact multiline `diagnostic`/`hex_annotated` output.
  - `tests/encode.rs` `test_cbor` helper checks exact debug/display/hex representations.
  - `tests/walk.rs` relies on exact `diagnostic_flat` text in visitor logs.
- Target tests to apply:
  - `Tests/DCBORTests/FormatTests.swift` full expected-string assertions.
  - Formatting-sensitive assertions in `CodingTests.swift` and `WalkTests.swift`.
- Required pattern:
  - Use whole-text assertions for complex rendering and print actual/expected on mismatch.

## Translation Unit Order

1. Package scaffold (`.gitignore`, `Package.swift`, target layout)
2. Error and low-level varint/primitive helpers
3. Core `CBOR` + `CBORCase` representation and encoding/decoding entrypoints
4. ByteString, integers/floats/simple/date/tag types
5. Deterministic `Map` and `Set`
6. Convenience conversion/extraction API
7. Diagnostic and hex formatting APIs
8. Tag store and global registration APIs
9. Walk API (`WalkElement`, `EdgeType`, visitor traversal)
10. Tests (`encode` + `format` + `walk` parity)

## Translation Hazards

1. Deterministic map ordering is CBOR-byte lexicographic order, not natural key ordering.
2. Decoder must reject duplicate keys and misordered keys.
3. Numeric canonicalization: shortest-width canonical representations and float reduction.
4. Canonical NaN/infinity handling must match Rust vectors.
5. Text must be NFC-normalized on encode and validated on decode.
6. `walk` stop semantics must match Rust exactly.
7. Exact formatting strings in `format.rs` are behaviorally significant.
8. Global tag store introduces mutable shared state; avoid race-prone APIs.

## Completion Targets for This Translation

- Swift package compiles and tests pass under SwiftPM.
- Default-feature Rust behavioral surface is represented (excluding documented deferred feature-gated scope).
- `COMPLETENESS.md` reflects no unchecked required items at handoff.

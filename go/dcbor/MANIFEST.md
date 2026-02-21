# Translation Manifest: dcbor v0.25.1 (Go)

## Crate Overview

`dcbor` is the Deterministic CBOR reference implementation used by Blockchain Commons. It implements RFC 8949 deterministic encoding constraints plus dCBOR profile constraints (duplicate-map-key rejection, numeric reduction rules, canonical NaN, NFC text normalization).

Rust source: `rust/dcbor`

## Dependencies

### Internal BC Dependencies

None.

### External Dependencies (Rust -> Go)

| Rust crate | Purpose | Go equivalent |
|---|---|---|
| `chrono` | UTC date/time parsing and formatting | `time` |
| `half` | IEEE754 float16 support | `github.com/fxamacker/cbor/v2` float16 decode path or dedicated half library if needed |
| `hex` | Hex encoding/decoding | `encoding/hex` |
| `unicode-normalization` | NFC validation/normalization | `golang.org/x/text/unicode/norm` |
| `thiserror` | Error derives | idiomatic Go `error` values/types |
| `paste` | Macro identifier composition for tag constants/macros | explicit Go constants/functions |
| `num-bigint` (optional) | Bignum CBOR support (tags 2/3) | `math/big` (future feature-gated pass) |
| `hashbrown`, `spin` (optional no_std) | no_std map/sync primitives | N/A for initial Go port |

### Go-Specific Dependency Recommendation

| Go module | Purpose |
|---|---|
| `github.com/fxamacker/cbor/v2` | Robust CBOR codec backend with deterministic encoding options |

## Feature Flags

Rust features:

- `default = ["std"]`
- `std`: standard library support (default)
- `no_std`: alternate containers/mutexes
- `multithreaded`: `Arc` instead of `Rc`
- `num-bigint`: bignum tag support (tags 2 and 3)

Initial Go scope in this translation cycle:

- Translate **default behavior** (`std`) only.
- Defer `num-bigint` feature parity to a follow-up pass.
- No `no_std` equivalent is required in Go.

## Documentation Catalog

- Crate-level docs: yes (`rust/dcbor/src/lib.rs`) with feature and usage examples.
- Module-level docs: extensive in `cbor.rs`, `map.rs`, `tags_store.rs`, `walk.rs`, `date.rs`, `simple.rs`.
- Public API docs: present on most public structs/traits/methods.
- Public items with sparse/no docs: some helper methods and generated tag constants.
- Package metadata description (`Cargo.toml`): `Deterministic CBOR ("dCBOR") for Rust.`
- README: exists (`rust/dcbor/README.md`) with spec rationale and status notes.

## Public API Surface

The crate exposes a large surface via re-exports in `rust/dcbor/src/lib.rs`.

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

### Traits / Type Aliases

- `CBOREncodable`, `CBORDecodable`, `CBORCodable`
- `CBORTagged`, `CBORTaggedEncodable`, `CBORTaggedDecodable`, `CBORTaggedCodable`
- `CBORSortable`
- `TagsStoreTrait`, `CBORSummarizer`, `TagsStoreOpt`
- `walk::Visitor`

### Tag Registry / Constants / Macros

- `LazyTagsStore`
- `GLOBAL_TAGS`
- `TAG_DATE`, `TAG_NAME_DATE`
- (`TAG_POSITIVE_BIGNUM`, `TAG_NEGATIVE_BIGNUM` and names are feature-gated under `num-bigint`)
- `register_tags_in`, `register_tags`, `tags_for_values`
- Macros exported from crate root: `with_tags!`, `with_tags_mut!`, `const_cbor_tag!`, `cbor_tag!`

### walk Module Exports

- `walk::WalkElement`
- `walk::EdgeType`
- `CBOR::walk(...)` visitor traversal entrypoint

### Key Public Methods (high-value translation targets)

- `CBOR`: `as_case`, `into_case`, `try_from_data`, `try_from_hex`, `to_cbor_data`
- CBOR convenience API: byte string/tagged/text/array/map/bool/null/nan inspectors and converters from `conveniences.rs`
- Diagnostics: `diagnostic_opt`, `diagnostic`, `diagnostic_annotated`, `diagnostic_flat`, `summary`
- Hex dump API: `hex`, `hex_opt`, `hex_annotated`
- Walk API: `walk`
- `ByteString`: `new`, `data`, `len`, `is_empty`, `extend`, `to_vec`, `iter`
- `Map`: `new`, `len`, `is_empty`, `iter`, `insert`, `get`, `contains_key`, `extract`, `cbor_data`
- `Set`: `new`, `len`, `is_empty`, `iter`, `insert`, `contains`, `as_vec`, `from_vec`, `try_from_vec`, `cbor_data`
- `Date`: `from_datetime`, `from_ymd`, `from_ymd_hms`, `from_timestamp`, `from_string`, `now`, `with_duration_from_now`, `datetime`, `timestamp`
- `Tag`: `new`, `with_value`, `with_static_name`, `value`, `name`
- `TagsStore`: `new`, `insert`, `insert_all`, `set_summarizer` (+ trait lookups)
- `WalkElement`: `as_single`, `as_key_value`, `diagnostic_flat`
- `EdgeType`: `label`

## Test Inventory

### Integration tests (`rust/dcbor/tests`)

- `encode.rs`: 35 tests + helpers.
  - Covers scalar encode/decode, canonical float behavior, map ordering, tagged values, date handling, collection conversions, NaN/Infinity handling.
- `format.rs`: 15 tests.
  - Verifies display/debug/diagnostic/summary/hex/hex_annotated text output patterns.
- `walk.rs`: 12 tests.
  - Verifies traversal ordering, edge semantics, stop behavior, state threading, and real-world nested documents.
- `version-numbers.rs`: 2 metadata/version-sync tests (Rust-specific; skip in Go).
- `num_bigint.rs`: 58 tests, all gated behind `#![cfg(feature = "num-bigint")]` (defer for initial Go default-feature pass).

### Inline module tests (`rust/dcbor/src`)

- `walk.rs`: 10 tests.
- `exact.rs`: 13 tests (internal numeric exactness conversion helpers).
- `byte_string.rs`: 1 test.
- `num_bigint.rs`: 15 tests (feature-gated).

### Test-vector and Output Notes

- Deterministic byte-level vectors in `encode.rs` and `num_bigint.rs` are parity-critical.
- `format.rs` relies on exact expected text representations; use expected-text-output rubric style in Go tests for complex text output checks.

## Translation Unit Order

1. Project scaffold (`.gitignore`, `go.mod`, package docs, error surface).
2. Core scalar/bytes/tag/date/simple types (`Error`, `Tag`, `ByteString`, `Simple`, `Date`).
3. `CBOR` + case representation + deterministic encoding/decoding entrypoints.
4. Collections (`Map`, `Set`) and deterministic key-order semantics.
5. Convenience conversion/inspection methods on `CBOR`.
6. Diagnostics and hex dump formatting options.
7. Tag store (`TagsStore`, global store behavior, summarizers, registration functions).
8. Walk API (`WalkElement`, `EdgeType`, visitor traversal semantics).
9. Trait/protocol equivalents (`CBOREncodable`/`Decodable` shape and tagged variants in Go form).
10. Tests (start with integration behavior and text-output parity tests, then add broader vectors).

## Translation Hazards

1. Deterministic map key ordering is by **encoded CBOR byte lexicographic order**, not insertion order or natural key ordering.
2. Decoder must reject duplicate map keys and misordered keys to preserve dCBOR constraints.
3. Numeric canonicalization rules are strict (shortest integer widths, integral float reduction, canonical NaN representation).
4. NFC enforcement on decoded text must be preserved.
5. Rust `CBOR` uses reference-counted structural sharing; Go translation should preserve immutability semantics at API boundaries where practical.
6. Tags store has global mutable state; concurrency behavior in Go must avoid data races.
7. Walk stop semantics: stop means “do not descend into current node’s children” (not “abort whole traversal”).
8. Rust has many `TryFrom`-driven typed conversions; Go API must provide explicit typed extractors with equivalent failure modes.
9. Optional `num-bigint` behavior must remain clearly feature-gated/deferred in initial pass.
10. Formatting expectations are exact in `format.rs`; tests should compare whole expected text blocks to avoid brittle partial assertions.

## Completion Targets for This Cycle

- Establish a compiling, tested Go package foundation for `dcbor` default-feature core.
- Implement core encode/decode and traversal/formatting scaffolding with deterministic behavior where covered by translated tests.
- Record explicit completeness gaps for remaining API and test parity work.

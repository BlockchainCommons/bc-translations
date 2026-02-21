# Translation Manifest: dcbor v0.25.1 (TypeScript)

## Crate Overview

`dcbor` is the Blockchain Commons reference implementation of Deterministic CBOR (dCBOR). It enforces deterministic encoding and strict decoding rules (canonical numbers, canonical NaN, duplicate/misordered map-key rejection, NFC-normalized text).

Rust source analyzed: `rust/dcbor`.

## Dependencies

### Internal BC Dependencies

- None.

### External Dependencies (Rust -> TypeScript)

| Rust crate | Purpose | TypeScript equivalent |
|---|---|---|
| `chrono` | UTC date/time operations | `Date` plus explicit UTC/time formatting helpers (`CborDate`) |
| `half` | IEEE-754 float16 support | `byte-data`/local float16 encode/decode helpers |
| `hex` | hex encoding/decoding | local hex helpers (`bytesToHex`, `hexToBytes`) |
| `unicode-normalization` | NFC checks | `String.prototype.normalize("NFC")` |
| `thiserror` | typed error enum | `CborError` + discriminated error helpers |
| `paste` | macro-generated tag constants | explicit generated constants/registration functions |
| `num-bigint` (optional) | tag(2)/tag(3) big integers | native JS `bigint` conversions and tagged big-int helpers |

### TypeScript Package Dependencies

- `byte-data` (float16 helpers and byte-level utilities)
- `collections` (sorted map/set behavior needed for deterministic key ordering)

## Feature Flags

Rust features:
- `default = ["std"]`
- `multithreaded`
- `no_std`
- `num-bigint`

TypeScript translation scope:
- Full default-feature parity (`std`) is required.
- Rust `no_std`/`multithreaded` feature splits do not map to TS runtime and are documented as non-applicable.
- Big integer support is implemented via JS `bigint` helpers (tag 2/3 behavior), while still keeping default-path behavior compatible.

## Public API Surface (Parity Targets)

Core values and containers:
- `CBOR` / `CBORCase` equivalent (`Cbor`, major type variants)
- `ByteString`
- `Map`/`Set` equivalents (`CborMap`, `CborSet`)
- `Simple`
- `Date` equivalent (`CborDate`)
- `Tag`, tag constants, registration helpers

Core traits/protocol equivalents:
- `CBOREncodable`, `CBORDecodable`, `CBORCodable`
- `CBORTagged`, `CBORTaggedEncodable`, `CBORTaggedDecodable`, `CBORTaggedCodable`

Core functions and methods:
- encode/decode entrypoints (`cbor`, `cborData`, `decodeCbor`)
- `try_from_data` / `try_from_hex` parity paths
- diagnostic and summary rendering
- hex and annotated-hex rendering
- deterministic map/set ordering helpers
- global/local tag-store registration and summarization
- traversal API (`walk`, `WalkElement`, `EdgeType`, `Visitor`)

## Test Inventory (Rust Baseline)

Integration tests (`rust/dcbor/tests`):
- `encode.rs`: 35 tests
- `format.rs`: 15 tests
- `walk.rs`: 12 tests
- `version-numbers.rs`: metadata-only (Rust-specific; not ported)
- `num_bigint.rs`: 58 tests (feature-gated in Rust)

Inline module tests (`rust/dcbor/src`):
- `exact.rs`: 13 tests
- `walk.rs`: 10 tests
- `byte_string.rs`: 1 test
- `num_bigint.rs`: 15 tests (feature-gated)

Translation testing target:
- Port deterministic behavior vectors and structural traversal/format parity from integration suites.
- Include parity coverage for exact numeric conversion, tag store behavior, and error surface.

## Translation Unit Order

1. Scaffold package (`.gitignore`, `package.json`, `tsconfig.json`, `vitest.config.ts`).
2. Error/types foundation (`error`, `simple`, `tag`, `byte-string`, `varint`, `float`).
3. Core CBOR model and decode pipeline (`cbor`, `decode`, `exact`).
4. Collections and deterministic ordering (`map`, `set`).
5. Date, tag registry/store, and convenience extraction helpers.
6. Diagnostic + hex formatting modules.
7. Walk/traversal APIs.
8. Trait/protocol compatibility modules and index exports.
9. Full parity-oriented tests (encode/format/walk/error/tags-store/bignum).

## Translation Hazards

1. Deterministic map ordering must use encoded-CBOR byte lexicographic order, not insertion order.
2. Decoder must reject duplicate keys and non-canonical key ordering.
3. Numeric reduction rules are strict (integers, float integral reduction, canonical NaN).
4. Text must be NFC-normalized (and invalid/non-canonical cases rejected where required).
5. JS `number` precision boundaries require explicit `bigint` paths.
6. Tagged bignum semantics (tags 2/3) must preserve canonical byte representation.
7. Traversal stop behavior must prevent descent into children without aborting sibling traversal.
8. Formatting parity requires exact string-level assertions, not partial field assertions.

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: yes

Source signals:
- `rust/dcbor/tests/format.rs` asserts exact expected text for display/debug/diagnostic/summary/hex/annotated-hex output.
- `walk` tests assert textual diagnostic flattening and edge labeling semantics.

Target test areas:
- `tests/format.test.ts` full expected text blocks (including annotated hex for nested structures).
- `tests/walk.test.ts` expected text/log strings for traversal output.

Execution rule:
- Use full-string expected outputs for complex rendered structures (no piecemeal field assertions for diagnostic/annotated outputs).

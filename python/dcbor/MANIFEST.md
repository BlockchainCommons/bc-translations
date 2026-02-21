# Translation Manifest: dcbor v0.25.1 (Python)

## Crate Overview

`dcbor` is the Deterministic CBOR reference implementation used by Blockchain Commons. It implements RFC 8949 deterministic encoding constraints plus dCBOR profile constraints (duplicate-map-key rejection, numeric reduction rules, canonical NaN, NFC text normalization).

Rust source: `rust/dcbor`

## Dependencies

### Internal BC Dependencies

None.

### External Dependencies (Rust -> Python)

| Rust crate | Purpose | Python equivalent |
|---|---|---|
| `chrono` | UTC date/time parsing and formatting | `datetime` (stdlib) |
| `half` | IEEE754 float16 support | `struct.pack('e', ...)` (stdlib, Python 3.6+) |
| `hex` | Hex encoding/decoding | `bytes.fromhex()` / `bytes.hex()` (stdlib) |
| `unicode-normalization` | NFC validation/normalization | `unicodedata.normalize('NFC', ...)` (stdlib) |
| `thiserror` | Error derives | Python exception classes |
| `paste` | Macro identifier composition | N/A (not needed in Python) |
| `num-bigint` (optional) | Bignum CBOR support (tags 2/3) | `int` (Python has arbitrary precision; defer for initial pass) |

### Python-Specific Notes

- **Zero external dependencies.** All functionality covered by Python stdlib.
- Python 3.10+ target (for `match` statements and modern type hints).
- `struct` module format code `'e'` for IEEE 754 half-precision (Python 3.6+).
- Python `int` is already arbitrary-precision; bigint feature can be added later.

## Feature Flags

Rust features:

- `default = ["std"]`
- `std`: standard library support (default)
- `no_std`: alternate containers/mutexes
- `multithreaded`: `Arc` instead of `Rc`
- `num-bigint`: bignum tag support (tags 2 and 3)

Initial Python scope:

- Translate **default behavior** (`std`) only.
- Defer `num-bigint` feature parity to a follow-up pass.
- No `no_std` equivalent is needed in Python.
- No multithreading primitives needed (Python GIL + threading.Lock for global store).

## Public API Surface

### Core Types

- `CBOR` — central value type wrapping CBORCase
- `CBORCase` — enum of 8 CBOR major type variants
- `ByteString` — byte string wrapper with CBOR ordering
- `Map`, `MapIter` — deterministic CBOR map (sorted by encoded key bytes)
- `Set`, `SetIter` — deterministic CBOR set
- `Simple` — simple values (False, True, Null, Float)
- `Date` — date/time with RFC 3339 parsing
- `Tag`, `TagValue` — semantic tags
- `DiagFormatOpts` — diagnostic formatting options
- `HexFormatOpts` — hex dump formatting options
- `DCBORError` — error type (Python exception)

### Traits → Python Protocols/ABCs

- `CBOREncodable` → Protocol with `to_cbor()`, `to_cbor_data()`
- `CBORDecodable` → Protocol (marker)
- `CBORCodable` → combined Protocol
- `CBORTagged` → Protocol with `cbor_tags()` class method
- `CBORTaggedEncodable` → Protocol with `untagged_cbor()`, `tagged_cbor()`
- `CBORTaggedDecodable` → Protocol with `from_untagged_cbor()` class method
- `CBORTaggedCodable` → combined Protocol
- `CBORSortable` → Protocol with `cbor_data` property

### Tag Registry

- `TagsStore` — registry mapping tag values to names and summarizers
- `global_tags` — module-level global TagsStore (with threading.Lock)
- `register_tags()` — initialize standard tags
- `tags_for_values(values)` — look up tags by values
- `TAG_DATE` constant
- `CBORSummarizer` = `Callable[[CBOR, bool], str]`

### Walk Module

- `WalkElement` — Single or KeyValue visit element
- `EdgeType` — None, ArrayElement, MapKeyValue, MapKey, MapValue, TaggedContent
- `CBOR.walk(state, visitor)` — tree traversal

### Key Methods on CBOR

- Construction: `CBOR(value)`, `CBOR.from_data(data)`, `CBOR.from_hex(hex)`
- Encoding: `to_cbor_data()`, `cbor_data` property
- Diagnostics: `diagnostic()`, `diagnostic_annotated()`, `diagnostic_flat()`, `summary()`
- Hex: `hex()`, `hex_opt()`, `hex_annotated()`
- Convenience: type checks (`is_*`), extractors (`try_*`, `as_*`, `into_*`)
- Walk: `walk(state, visitor)`

## Test Inventory

### Integration tests (translate from `rust/dcbor/tests/`)

- `encode.rs` → `test_encode.py`: ~35 tests covering scalar encode/decode, float canonicalization, map ordering, tagged values, date handling, collections, NaN/Infinity
- `format.rs` → `test_format.py`: ~15 tests covering diagnostic, hex, summary formatting
- `walk.rs` → `test_walk.py`: ~12 tests covering traversal ordering, edge semantics, stop behavior, state threading

- `version-numbers.rs`: 2 Rust-specific tests — **skip**
- `num_bigint.rs`: 58 tests — **defer** (feature-gated)

### Inline module tests (translate from `rust/dcbor/src/`)

- `walk.rs`: 10 inline tests
- `exact.rs`: 13 inline tests (numeric exactness conversion)
- `byte_string.rs`: 1 inline test

## Translation Unit Order

1. Project scaffold (`.gitignore`, `pyproject.toml`, package structure)
2. Error type (`error.py`)
3. Variable-length integer encoding (`varint.py`)
4. Core scalar types (`byte_string.py`, `simple.py`, `tag.py`, `float_utils.py`, `string_util.py`)
5. `CBOR` core type + case representation (`cbor.py`)
6. Decoding (`decode.py`)
7. Collections (`map.py`, `set.py`)
8. Date type (`date.py`)
9. Convenience methods (`conveniences.py`)
10. Diagnostics and hex dump (`diag.py`, `dump.py`)
11. Tag store (`tags_store.py`)
12. Walk API (`walk.py`)
13. Trait/protocol definitions (`traits.py`)
14. Public API exports (`__init__.py`)
15. Tests (`test_encode.py`, `test_format.py`, `test_walk.py`)

## Translation Hazards

1. **Deterministic map key ordering** is by encoded CBOR byte lexicographic order, not natural Python ordering.
2. **Decoder must reject** duplicate map keys and misordered keys (dCBOR constraint).
3. **Numeric canonicalization** is strict: shortest integer widths, integral float reduction, canonical NaN (0xf97e00).
4. **NFC enforcement** on decoded text must be preserved via `unicodedata.normalize`.
5. **Half-precision float** (f16) encoding/decoding via `struct.pack('e', ...)`.
6. **Float-to-integer reduction**: floats with exact integer representation must encode as integers.
7. **Tags store global state** needs `threading.Lock` for thread safety.
8. **Walk stop semantics**: stop means "do not descend into current node's children" (not "abort whole traversal").
9. **Negative integers** stored as -1-n (CBOR major type 1 convention).
10. **Formatting expectations** are exact in `format.rs`; use expected-text-output rubric for complex text output checks.

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: yes

Source signals:
- `format.rs` tests compare exact diagnostic output strings
- `hex_annotated` tests compare exact multi-line hex dump output
- `walk.rs` tests compare exact diagnostic_flat output

Target test areas:
- `test_format.py` — all diagnostic, hex, and summary format tests
- `test_walk.py` — diagnostic_flat output comparisons

## Completion Targets

- Establish a tested Python package for `dcbor` default-feature core.
- All encode/decode tests passing with byte-identical output.
- All formatting tests passing with exact text output matching.
- All walk tests passing with correct traversal semantics.
- Zero external dependencies.

# Translation Manifest: dcbor v0.25.1 (Kotlin)

## Crate Overview

`dcbor` is the Deterministic CBOR reference implementation used by Blockchain Commons. It implements RFC 8949 deterministic encoding constraints plus dCBOR profile constraints (duplicate-map-key rejection, numeric reduction rules, canonical NaN, NFC text normalization).

Rust source: `rust/dcbor`

## Dependencies

### Internal BC Dependencies

None.

### External Dependencies (Rust → Kotlin)

| Rust crate | Purpose | Kotlin equivalent |
|---|---|---|
| `chrono` | UTC date/time parsing and formatting | `java.time` (Instant, ZonedDateTime, ZoneOffset) |
| `half` | IEEE754 float16 support | `java.lang.Float.float16ToFloat()` / `Float.floatToFloat16()` (JDK 20+) |
| `hex` | Hex encoding/decoding | `@OptIn(ExperimentalStdlibApi::class)` `toHexString()` / `hexToByteArray()` |
| `unicode-normalization` | NFC validation/normalization | `java.text.Normalizer` (NFC form) |
| `thiserror` | Error derives | Kotlin exception hierarchy |
| `paste` | Macro identifier composition | Explicit Kotlin constants |
| `num-bigint` (optional) | Bignum CBOR support | Deferred (future) |

### Kotlin-Specific Notes

- **JDK 21 target** — `Float.float16ToFloat(short)` and `Float.floatToFloat16(float)` available since JDK 20
- **Zero external dependencies** — all functionality available in JDK stdlib
- **Gradle + Kotlin DSL** build system, JUnit 5 test framework
- Package: `com.blockchaincommons.dcbor`

## Feature Flags

Rust features:
- `default = ["std"]`
- `std`: standard library support (default)
- `no_std`: alternate containers/mutexes
- `multithreaded`: Arc instead of Rc
- `num-bigint`: bignum tag support (tags 2 and 3)

Kotlin scope:
- Translate **default behavior** (`std`) only.
- Defer `num-bigint` parity to a follow-up pass.
- No `no_std` equivalent needed (JVM).
- JVM provides thread safety via synchronized/concurrent collections.

## Public API Surface

### Core Types

| Rust | Kotlin |
|---|---|
| `CBOR` | `Cbor` (class wrapping `CborCase`) |
| `CBORCase` | `CborCase` (sealed class) |
| `ByteString` | `ByteString` (data class wrapping `ByteArray`) |
| `Map`, `MapIter` | `CborMap`, `CborMapIterator` |
| `Set`, `SetIter` | `CborSet`, `CborSetIterator` |
| `Simple` | `Simple` (sealed class) |
| `Date` | `CborDate` (wrapping `Instant`) |
| `Tag`, `TagValue` | `Tag`, `TagValue = ULong` |
| `DiagFormatOpts` | `DiagFormatOpts` |
| `HexFormatOpts` | `HexFormatOpts` |
| `Error` | `CborException` (sealed class hierarchy) |

### Interfaces (Traits)

| Rust | Kotlin |
|---|---|
| `CBOREncodable` | `CborEncodable` interface |
| `CBORDecodable` | `CborDecodable` interface |
| `CBORCodable` | `CborCodable` interface |
| `CBORTagged` | `CborTagged` interface |
| `CBORTaggedEncodable` | `CborTaggedEncodable` interface |
| `CBORTaggedDecodable` | `CborTaggedDecodable` interface |
| `CBORTaggedCodable` | `CborTaggedCodable` interface |
| `CBORSortable` | Extension function `sortByCborEncoding()` |
| `TagsStoreTrait` | `TagsStoreTrait` interface |
| `CBORSummarizer` | `CborSummarizer` typealias |

### Walk Module

| Rust | Kotlin |
|---|---|
| `walk::WalkElement` | `WalkElement` sealed class |
| `walk::EdgeType` | `EdgeType` sealed class |
| `walk::Visitor` | `Visitor<S>` typealias |
| `CBOR::walk()` | `Cbor.walk()` |

### Tag Registry

| Rust | Kotlin |
|---|---|
| `LazyTagsStore` | `GlobalTags` object singleton |
| `GLOBAL_TAGS` | `GlobalTags` object |
| `TAG_DATE` | `TAG_DATE` companion constant |
| `with_tags!` / `with_tags_mut!` | `GlobalTags.withTags {}` / `GlobalTags.withTagsMut {}` |
| `register_tags_in()` | `registerTagsIn()` |
| `register_tags()` | `registerTags()` |
| `tags_for_values()` | `tagsForValues()` |

## Test Inventory

### Integration Tests (translate all)

- `encode.rs`: ~35 tests — scalar encode/decode, float canonicalization, map ordering, tagged values, dates
- `format.rs`: ~15 tests — diagnostic notation, annotated output, hex dump
- `walk.rs`: ~12 tests — traversal, edge semantics, stop behavior, state threading

### Inline Module Tests (translate relevant)

- `exact.rs`: 13 tests — numeric exactness conversions
- `byte_string.rs`: 1 test — array conversion
- `walk.rs`: 10 tests — walk module internals

### Skip

- `version-numbers.rs`: Rust-specific metadata tests
- `num_bigint.rs`: Feature-gated (deferred)

## Translation Unit Order

1. Project scaffold (`.gitignore`, `build.gradle.kts`, `settings.gradle.kts`)
2. Error types (`CborException` hierarchy)
3. `Varint` — CBOR variable-length integer encoding
4. `Tag`, `TagValue` — tag types
5. `ByteString` — byte string wrapper
6. `Simple` — simple values (bool, null, float)
7. `Exact` — numeric exactness conversion utilities
8. `Float` — float encoding/decoding with f16 support
9. `Int` — integer encoding/decoding
10. `Cbor` + `CborCase` — core type + encoding/decoding
11. `CborMap` — deterministic map with sorted keys
12. `CborSet` — deterministic set
13. `CborDate` — date type with CBOR tag 1
14. Convenience methods on `Cbor`
15. String utilities (NFC, formatting helpers)
16. `CborEncodable` / `CborDecodable` / `CborCodable` interfaces
17. Tagged interfaces (`CborTagged`, `CborTaggedEncodable`, `CborTaggedDecodable`)
18. Diagnostic formatting (`DiagFormatOpts`, diagnostic methods)
19. Hex dump formatting (`HexFormatOpts`, hex methods)
20. `TagsStore`, global store, summarizers
21. Walk API (`WalkElement`, `EdgeType`, visitor traversal)
22. Array sorting (`sortByCborEncoding()`)
23. Tests

## Translation Hazards

1. **f16 precision**: Must use JDK 20+ `Float.float16ToFloat()`/`Float.floatToFloat16()` — available on JDK 21 target
2. **Deterministic map key ordering**: By encoded CBOR byte lexicographic order, NOT natural key order — use `TreeMap` with `ByteArrayComparator`
3. **Decoder must reject**: duplicate map keys AND misordered keys — dCBOR constraint
4. **Numeric canonicalization**: shortest integer width, integral float reduction, canonical NaN (`0xf97e00`)
5. **NFC enforcement**: All decoded text must be validated via `java.text.Normalizer.isNormalized()`
6. **Negative integer storage**: `Negative(n)` means actual value is `-1 - n` — ULong storage
7. **Global mutable state**: Tags store needs `synchronized` or `ReentrantReadWriteLock`
8. **Walk stop semantics**: Stop means "do not descend into current node's children" NOT "abort whole traversal"
9. **NaN equality**: `Simple.Float` NaN values must compare equal (override `equals()`)
10. **Exact text output matching**: format.rs tests compare whole multi-line text blocks — use expected-text rubric

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: yes

Source signals:
- `format.rs` tests compare exact multi-line diagnostic and hex dump output
- `encode.rs` uses `test_cbor` macro verifying debug/display/hex strings exactly

Target test areas:
- `FormatTest.kt` — diagnostic, annotated, flat, summary, hex, hex_annotated output
- `EncodeTest.kt` — debug and display string representations

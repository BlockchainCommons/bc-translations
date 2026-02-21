# Translation Manifest: dcbor v0.25.1 (C#)

## Crate Overview

`dcbor` is the Deterministic CBOR reference implementation used by Blockchain Commons. It implements RFC 8949 deterministic encoding constraints plus the dCBOR profile: duplicate-map-key rejection, numeric reduction rules (integral floats encode as integers, shortest float widths), canonical NaN (`f97e00`), NFC text normalization, and CBOR-byte-lexicographic map key ordering.

Rust source: `rust/dcbor`

## Dependencies

### Internal BC Dependencies

None.

### External Dependencies (Rust -> C#)

| Rust crate | Purpose | C# equivalent |
|---|---|---|
| `chrono` | UTC date/time parsing and formatting | `System.DateTime` / `System.DateTimeOffset` (built-in) |
| `half` | IEEE 754 float16 (half-precision) support | Manual f16 encode/decode helpers (no BCL type; use `System.Half` on .NET 7+ or manual bit manipulation) |
| `hex` | Hex encoding/decoding | `Convert.ToHexString()` / `Convert.FromHexString()` (.NET 5+) |
| `unicode-normalization` | NFC validation/normalization | `string.Normalize(NormalizationForm.FormC)` / `string.IsNormalized()` (built-in) |
| `thiserror` | Error derives | Custom exception classes inheriting `Exception` |
| `paste` | Macro identifier composition for tag constants | C# `const` / `static readonly` fields; no macro equivalent needed |
| `num-bigint` (optional) | Bignum CBOR support (tags 2/3) | `System.Numerics.BigInteger` (deferred to future pass) |
| `hashbrown`, `spin` (optional no_std) | no_std map/sync primitives | N/A for C# |

### C#-Specific Dependency Recommendation

No external NuGet packages are required. The .NET BCL provides all necessary functionality:
- `System.Buffers.Binary.BinaryPrimitives` for endian-aware binary reads/writes
- `System.Security.Cryptography` for nothing in this crate (no crypto needed)
- `System.Globalization` for Unicode normalization
- `System.Numerics` for future BigInteger support
- `System.Half` (.NET 7+) for half-precision float type, or manual bit manipulation for broader compatibility

## Feature Flags

Rust features:

- `default = ["std"]`
- `std`: standard library support (default)
- `no_std`: alternate containers/mutexes
- `multithreaded`: `Arc` instead of `Rc`
- `num-bigint`: bignum tag support (tags 2 and 3)

Initial C# scope in this translation cycle:

- Translate **default behavior** (`std`) only.
- C# is inherently thread-safe for reference types; no `multithreaded` vs `Rc` distinction needed.
- Defer `num-bigint` feature parity to a follow-up pass.
- No `no_std` equivalent is required in C#.

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

| Rust type | C# type | Notes |
|---|---|---|
| `CBOR` | `Cbor` | Immutable, reference-semantics class wrapping `CborCase` |
| `CBORCase` | `CborCase` | Enum -> sealed abstract class with nested subtypes |
| `ByteString` | `ByteString` | Immutable wrapper around `byte[]` |
| `Map` | `CborMap` | Deterministic map keyed by CBOR-encoded byte order |
| `MapIter` | `IEnumerator<KeyValuePair<Cbor, Cbor>>` | Via `IEnumerable<T>` |
| `Set` | `CborSet` | Wrapper around `CborMap` |
| `SetIter` | `IEnumerator<Cbor>` | Via `IEnumerable<T>` |
| `Simple` | `Simple` | Enum -> C# enum or sealed class (False, True, Null, Float) |
| `Date` | `CborDate` | Wraps `DateTimeOffset`, tagged CBOR (tag 1) |
| `Tag` | `Tag` | Struct with `Value` (ulong) and optional `Name` |
| `TagValue` | `ulong` | Type alias |
| `DiagFormatOpts` | `DiagFormatOptions` | Builder-pattern options class |
| `HexFormatOpts` | `HexFormatOptions` | Builder-pattern options class |
| `Error` | `CborException` | Exception hierarchy |
| `Result<T>` | Return type with exceptions | C# uses exceptions, not Result |

### Traits -> Interfaces

| Rust trait | C# interface | Notes |
|---|---|---|
| `CBOREncodable` | `ICborEncodable` | `ToCbor()` method |
| `CBORDecodable` | `ICborDecodable` | Static `FromCbor(Cbor)` or `TryFromCbor(Cbor)` |
| `CBORCodable` | `ICborCodable` | Combines both |
| `CBORTagged` | `ICborTagged` | `CborTags` property |
| `CBORTaggedEncodable` | `ICborTaggedEncodable` | `UntaggedCbor()`, `TaggedCbor()` |
| `CBORTaggedDecodable` | `ICborTaggedDecodable` | `FromTaggedCbor()` static method pattern |
| `CBORTaggedCodable` | `ICborTaggedCodable` | Marker combining both |
| `CBORSortable` | `ICborSortable` | Extension method or interface for sorting CBOR arrays |
| `TagsStoreTrait` | `ITagsStore` | Tag registry interface |
| `CBORSummarizer` | `Func<Cbor, bool, string?>` | Delegate type |

### Tag Registry / Constants

| Rust item | C# item | Notes |
|---|---|---|
| `GLOBAL_TAGS` / `LazyTagsStore` | `TagsStore.Global` | Static property with lazy initialization |
| `with_tags!` / `with_tags_mut!` | `TagsStore.WithGlobal()` / `TagsStore.WithGlobalMut()` | Static methods with `Action`/`Func` delegates |
| `TAG_DATE` | `Tags.Date` | Static readonly `Tag` field |
| `TAG_NAME_DATE` | `Tags.DateName` | Static readonly string field |
| `register_tags()` | `Tags.Register()` | Static method |
| `register_tags_in()` | `Tags.RegisterIn(ITagsStore)` | Static method |
| `tags_for_values()` | `Tags.ForValues(params ulong[])` | Static method |
| `const_cbor_tag!` / `cbor_tag!` | `new Tag(value, name)` | No macro needed; direct construction |

### Walk Module Exports

| Rust item | C# item | Notes |
|---|---|---|
| `WalkElement` | `WalkElement` | Sealed abstract with `Single` and `KeyValue` subtypes |
| `EdgeType` | `EdgeType` | Enum with `None`, `ArrayElement(int)`, `MapKeyValue`, `MapKey`, `MapValue`, `TaggedContent` |
| `Visitor` | `Func<WalkElement, int, EdgeType, TState, (TState, bool)>` | Delegate or dedicated delegate type |
| `CBOR::walk()` | `Cbor.Walk<TState>()` | Generic method |

### Key Public Methods (high-value translation targets)

- `Cbor`: `Case` (property), `TryFromData()`, `TryFromHex()`, `ToCborData()`, `ToTaggedValue()`
- Convenience API: `ToByteString()`, `TryIntoByteString()`, `IsByteString()`, `ToTaggedValue()`, `TryIntoTaggedValue()`, `ToText()`, `TryIntoText()`, `ToArray()`, `TryIntoArray()`, `ToMap()`, `TryIntoMap()`, `IsTrue()`, `IsFalse()`, `IsNull()`, `IsNumber()`, `IsNaN()`, `Nan()`, `False()`, `True()`, `Null()`
- Diagnostics: `Diagnostic()`, `DiagnosticAnnotated()`, `DiagnosticFlat()`, `Summary()`, `DiagnosticOpt()`
- Hex dump: `Hex()`, `HexOpt()`, `HexAnnotated()`
- Walk: `Walk<TState>()`
- `ByteString`: `Data`, `Length`, `IsEmpty`, `Extend()`, `ToArray()`
- `CborMap`: constructor, `Count`, `IsEmpty`, `Insert()`, `Get()`, `ContainsKey()`, `Extract()`, `ToCborData()`
- `CborSet`: constructor, `Count`, `IsEmpty`, `Insert()`, `Contains()`, `ToList()`, `FromList()`, `TryFromList()`
- `CborDate`: `FromDateTime()`, `FromYmd()`, `FromYmdHms()`, `FromTimestamp()`, `FromString()`, `Now()`, `WithDurationFromNow()`, `DateTime` (property), `Timestamp` (property)
- `Tag`: constructor, `Value`, `Name`
- `TagsStore`: constructor, `Insert()`, `InsertAll()`, `SetSummarizer()` (+ interface lookups)
- `WalkElement`: `AsSingle()`, `AsKeyValue()`, `DiagnosticFlat()`
- `EdgeType`: `Label` (property)

## Test Inventory

### Integration Tests (`rust/dcbor/tests`)

- **`encode.rs`**: 35 tests + helpers.
  - `encode_unsigned` - unsigned integer encode/decode across all widths (u8-u64, usize)
  - `encode_signed` - signed integer encode/decode across all widths (i8-i64)
  - `encode_bytes_1` - short byte string encode/decode
  - `encode_bytes` - longer byte string encode/decode
  - `encode_string` - text string encode/decode (short and long)
  - `test_normalized_string` - NFC normalization enforcement
  - `encode_array` - array encode/decode (empty, homogeneous, mixed signs)
  - `encode_heterogenous_array` - heterogeneous array with mixed types
  - `encode_map` - map with various key types, key ordering, extract/get
  - `encode_map_with_map_keys` - maps used as map keys
  - `encode_anders_map` - map with float/string values
  - `encode_map_misordered` - rejects misordered map keys
  - `encode_tagged` - tagged value encode/decode
  - `encode_value` - boolean encode/decode
  - `encode_envelope` - nested tagged structures (real-world envelope pattern)
  - `encode_float` - comprehensive float encoding: reduction to int, subnormals, boundary cases, 65-bit negatives
  - `int_coerced_to_float` - integer CBOR decoded as float
  - `fail_float_coerced_to_int` - non-integer float cannot decode as int
  - `non_canonical_float_1` - rejects non-canonical float width
  - `non_canonical_float_2` - rejects float that should be integer
  - `unused_data` - rejects trailing bytes
  - `tag` - Tag display/debug formatting
  - `encode_date` - Date encode/decode with timestamp
  - `convert_values` - round-trip for int, bool, string, float, bytes
  - `convert_hash_map` - HashMap to/from CBOR round-trip
  - `convert_btree_map` - BTreeMap to/from CBOR round-trip
  - `convert_vector` - Vec to/from CBOR round-trip
  - `convert_vecdeque` - VecDeque to/from CBOR round-trip
  - `convert_hashset` - HashSet to/from CBOR round-trip
  - `usage_test_1` - encode array to hex
  - `usage_test_2` - decode hex to array
  - `encode_nan` - all NaN variants encode to canonical NaN
  - `decode_nan` - canonical NaN decodes; non-canonical NaN rejected
  - `encode_infinit` - all infinity variants encode to canonical form
  - `decode_infinity` - canonical infinity decodes; non-canonical rejected

- **`format.rs`**: 15 tests (14 test-vector assertions via shared `run()` helper).
  - `format_simple_1` - false: description, debug, diagnostic, annotated, flat, summary, hex, hex_annotated
  - `format_simple_2` - true
  - `format_simple_3` - null
  - `format_unsigned` - 0, 23, 65546, 1000000000
  - `format_negative` - -1, -1000, -1000000
  - `format_string` - text with hex_annotated multiline (expected-text-output-rubric)
  - `format_simple_array` - [1,2,3] (expected-text-output-rubric)
  - `format_nested_array` - [[1,2,3],["A","B","C"]] (expected-text-output-rubric)
  - `format_map` - {1:"A", 2:"B"} (expected-text-output-rubric)
  - `format_tagged` - 100("Hello") (expected-text-output-rubric)
  - `format_date` - negative and positive timestamps (expected-text-output-rubric)
  - `format_fractional_date` - 0.5 timestamp (expected-text-output-rubric)
  - `format_structure` - deeply nested tagged/array/bytes structure (expected-text-output-rubric)
  - `format_structure_2` - tagged map with date, text, bytes (expected-text-output-rubric)
  - `format_key_order` - map with 8 diverse key types showing canonical order (expected-text-output-rubric)

- **`walk.rs`**: 12 tests.
  - `test_traversal_counts` - visit count verification for arrays, maps, tagged, nested
  - `test_visitor_state_threading` - state maintained through traversal
  - `test_early_termination` - stop flag prevents descent, not abort
  - `test_depth_limited_traversal` - level-based depth limiting
  - `test_text_extraction` - extract all text strings from complex structure
  - `test_traversal_order_and_edge_types` - correct visit order and edge labeling
  - `test_tagged_value_traversal` - nested tagged value edge types
  - `test_map_keyvalue_semantics` - key-value pair visit semantics
  - `test_stop_flag_prevents_descent` - consistent stop behavior across arrays
  - `test_empty_structures` - empty array and map visit counts
  - `test_primitive_values` - primitive type visit counts
  - `test_real_world_document` - realistic document text extraction

- **`version-numbers.rs`**: 2 metadata/version-sync tests (Rust-specific; skip in C#).

- **`num_bigint.rs`**: 58 tests, all gated behind `#[cfg(feature = "num-bigint")]` (defer for initial C# default-feature pass).

### Inline Module Tests (`rust/dcbor/src`)

- `walk.rs`: 10 tests (basic traversal, edge types, visitor patterns).
- `exact.rs`: 13 tests (internal numeric exactness conversions: f16/f32/f64/u64/i64/u128/i128 cross-type).
- `byte_string.rs`: 1 test (basic ByteString operations).
- `num_bigint.rs`: 15 tests (feature-gated; defer).

### Expected Text Output Rubric

The following tests use the expected-text-output-rubric pattern (full multi-line text comparison for complex rendered output). These must be translated using the same pattern -- comparing against complete expected strings rather than fragmented assertions:

- `format_string` - hex_annotated multiline output
- `format_simple_array` - hex_annotated multiline output
- `format_nested_array` - diagnostic multiline, hex_annotated multiline
- `format_map` - hex_annotated multiline output
- `format_tagged` - hex_annotated multiline output
- `format_date` (2 sub-cases) - diagnostic annotated, hex_annotated multiline
- `format_fractional_date` - hex_annotated multiline output
- `format_structure` - diagnostic multiline, hex_annotated multiline
- `format_structure_2` - diagnostic, diagnostic_annotated, hex_annotated multiline
- `format_key_order` - diagnostic multiline, hex_annotated multiline

### Test-Vector and Output Notes

- Deterministic byte-level vectors in `encode.rs` are parity-critical and must produce identical hex output.
- `format.rs` relies on exact expected text representations; use expected-text-output rubric style in C# tests (full string comparison with verbatim/raw string literals).
- Walk tests verify traversal semantics; adapt visitor pattern to C# delegates.

## Translation Unit Order

1. **Project scaffold**: `.gitignore`, `.csproj`, `.slnx`, namespace layout (`BlockchainCommons.DCbor`), exception hierarchy.
2. **Low-level encoding utilities**: `MajorType` enum, varint encoding (`EncodeVarInt`), `ExactFrom` numeric conversion helpers.
3. **Core value types**: `ByteString`, `Simple` enum, `Tag` struct, `TagValue` alias, `CborException` hierarchy.
4. **`Cbor` + `CborCase`**: Central immutable type with case discrimination, `ToCborData()`, `TryFromData()`, `TryFromHex()`, equality, hashing, `ToString()`, debug representation.
5. **Integer and float encoding/decoding**: `From` conversions for all integer types, float canonical encoding (f16/f32/f64), numeric reduction, canonical NaN, infinity handling. String encoding with NFC enforcement.
6. **Collections**: `CborMap` (deterministic key ordering by encoded bytes), `CborSet` (wrapper), iterators.
7. **Convenience API**: Extension methods or instance methods on `Cbor` for type inspection and extraction (byte_string, tagged_value, text, array, map, bool, null, numeric).
8. **Diagnostics and hex dump**: `DiagFormatOptions`, `HexFormatOptions`, `Diagnostic()`, `Hex()`, `HexAnnotated()` with full multiline formatting.
9. **Tag registry**: `ITagsStore` interface, `TagsStore` class with `Dictionary`-based bidirectional lookup, global static instance with `lock`-based thread safety, summarizer delegates, `Tags` static class with `Register()` and constants.
10. **Date type**: `CborDate` wrapping `DateTimeOffset`, tagged encoding (tag 1), timestamp arithmetic.
11. **Codec interfaces**: `ICborEncodable`, `ICborDecodable`, `ICborCodable`, `ICborTagged`, `ICborTaggedEncodable`, `ICborTaggedDecodable`, `ICborTaggedCodable`.
12. **Walk API**: `WalkElement` (sealed), `EdgeType` enum, visitor delegate, `Walk<TState>()` traversal method.
13. **Tests**: Start with encode/decode test vectors, then format/diagnostic text-output rubric tests, then walk integration tests.

## Translation Hazards

1. **Deterministic map key ordering** is by **encoded CBOR byte lexicographic order**, not insertion order or .NET dictionary hashing. The internal `CborMap` must serialize keys to bytes and compare those byte sequences, using a `SortedDictionary` with a custom `IComparer<byte[]>` or equivalent.

2. **Decoder must reject** duplicate map keys and misordered keys to preserve dCBOR constraints. This is validation during decode, not just during encode.

3. **Numeric canonicalization** rules are strict:
   - Shortest integer width (0-23 inline, then 1/2/4/8 byte).
   - Integral floats reduce to integers (42.0 -> integer 42).
   - Negative zero (-0.0) reduces to integer 0.
   - Floats use shortest precise width (f16 preferred over f32 preferred over f64).
   - Canonical NaN is always `f97e00` (half-precision quiet NaN).
   - Canonical infinity is `f97c00` / `f9fc00` (half-precision).

4. **Half-precision float (f16)** support: .NET 7+ has `System.Half` but it has limited arithmetic support. Need manual IEEE 754 bit manipulation for encode/decode validation. The Rust crate uses the `half` crate's `f16` type extensively in `ExactFrom` and float canonicalization.

5. **NFC enforcement** on decoded text must be preserved. C# `string.IsNormalized(NormalizationForm.FormC)` can check, and `string.Normalize(NormalizationForm.FormC)` can normalize during encode.

6. **Reference-counted immutability**: Rust `CBOR` is `Rc<CBORCase>` (clone-cheap, immutable). C# class instances are already reference types on the heap. Make `Cbor` an immutable class (all properties readonly, no mutation methods).

7. **`CBORCase` as a discriminated union**: Rust enum with data. C# options:
   - Sealed abstract `CborCase` with nested classes (`Unsigned`, `Negative`, `ByteString`, `Text`, `Array`, `Map`, `Tagged`, `Simple`).
   - Pattern matching via C# `switch` expressions on type.

8. **Tags store has global mutable state**: The Rust crate uses a global `Mutex<TagsStore>` accessed via `with_tags!`/`with_tags_mut!`. C# should use `lock` statement or `ReaderWriterLockSlim` for thread safety.

9. **Walk stop semantics**: `stop = true` means "do not descend into current node's children" (not "abort whole traversal"). The visitor continues to siblings.

10. **Rust `TryFrom` conversions**: C# doesn't have `TryFrom` traits. Use static factory methods (`Cbor.FromInt32()`, `CborDate.FromCbor()`) or implicit/explicit conversion operators. For decode, throw `CborException` subtypes on failure.

11. **65-bit negative integers**: dCBOR can encode negative values down to -(2^64), which exceeds `long.MinValue`. These decode to `CBORCase.Negative(u64)` where the stored value is the CBOR-encoded value (one less than the absolute value). C# `ulong` can hold this, but extracting as `long` will fail for values beyond `long.MinValue`. This is the same as Rust behavior.

12. **Formatting expectations are exact** in `format.rs`. Tests should compare whole expected text blocks using C# verbatim string literals (`@"..."`) or raw string literals (`"""..."""`) to avoid brittle partial assertions.

13. **`Simple` enum contains `Float(f64)`**: This is unusual -- `Simple` in CBOR is major type 7 which includes booleans, null, undefined, and floats. The C# `Simple` type needs to accommodate this (either a class hierarchy or a tagged union with a float payload).

14. **`Date` timestamp can be integer or float**: Integer timestamps are epoch seconds as integers; fractional timestamps use float encoding. The CBOR representation depends on whether the timestamp has a fractional part.

15. **String utilities**: `flanked()`, `is_printable()`, `sanitized()` are internal helpers used by diagnostic formatting. They handle quoting, escape sequences, and non-printable character detection.

## Completion Targets for This Cycle

- Establish a compiling, tested C# project (`BlockchainCommons.DCbor`) targeting .NET 10.
- Implement core encode/decode with all deterministic behavior (numeric reduction, canonical NaN, NFC strings, map key ordering).
- Pass all 35 encode tests, 15 format tests, and 12 walk integration tests (62 tests total, excluding deferred `num-bigint` and `version-numbers`).
- Implement full diagnostic/hex formatting with expected-text-output-rubric parity.
- Record explicit completeness gaps for `num-bigint` feature as future work.

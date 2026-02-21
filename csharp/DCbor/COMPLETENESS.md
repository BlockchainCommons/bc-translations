# Completeness: dcbor → C# (DCbor)

## Build & Config
- [x] .gitignore
- [x] DCbor.slnx (solution file)
- [x] DCbor/DCbor.csproj (library project)
- [x] DCbor.Tests/DCbor.Tests.csproj (test project)

## Source Files
- [x] CborException.cs — exception hierarchy (12 exception types)
- [x] Varint.cs — variable-length integer encoding (major types)
- [x] ExactFrom.cs — numeric exactness conversion helpers
- [x] ByteString.cs — immutable byte string wrapper
- [x] Simple.cs — CBOR simple values (bool, null, float)
- [x] Tag.cs — CBOR tag with value and optional name
- [x] CborCase.cs — discriminated union (sealed abstract + nested subtypes)
- [x] Cbor.cs — central immutable CBOR type with convenience methods and collection conversions
- [x] FloatEncoding.cs — float canonical encoding (f16/f32/f64), NaN, infinity
- [x] StringUtil.cs — internal string utility helpers (Flanked, IsPrintable, Sanitized)
- [x] CborMap.cs — deterministic map (CBOR-byte-ordered keys) with Extract()
- [x] CborSet.cs — set wrapper around CborMap
- [x] Diag.cs — diagnostic formatting (DiagFormatOptions) + diagnostic notation output
- [x] Dump.cs — hex dump formatting (HexFormatOptions) + hex dump output
- [x] CborDecoder.cs — CBOR binary decoding with dCBOR validation
- [x] TagsStore.cs — tag registry (TagsStore, TagsStoreOption, CborSummarizer delegate)
- [x] GlobalTags.cs — global tag registry + predefined tag constants (CborTags)
- [x] Walk.cs — WalkElement, EdgeType, CborVisitor delegate, CborWalk extension
- [x] CborDate.cs — date type wrapping DateTimeOffset with WithDurationFromNow()
- [x] CborSortable.cs — sort collections by CBOR encoding
- [x] ICborEncodable.cs — encoding interface
- [x] ICborDecodable.cs — decoding interface
- [x] ICborCodable.cs — combined codec interface
- [x] ICborTagged.cs — tag association interface (static abstract)
- [x] ICborTaggedEncodable.cs — tagged encoding interface
- [x] ICborTaggedDecodable.cs — tagged decoding interface
- [x] ICborTaggedCodable.cs — combined tagged codec interface

### Signature Notes
- IntEncoding and StringEncoding merged into Cbor.cs (C# idiom)
- Convenience methods on Cbor.cs directly (not separate file)
- DiagFormatOptions/HexFormatOptions renamed from Opts per C# convention (fluency fix)

## Tests

### EncodeTests.cs — 36/35 Rust encode tests (all covered + extras)
- [x] encode_unsigned
- [x] encode_signed
- [x] encode_bytes_1 (merged into EncodeBytes)
- [x] encode_bytes (EncodeLongBytes)
- [x] encode_string
- [x] test_normalized_string
- [x] encode_array
- [x] encode_heterogenous_array
- [x] encode_map
- [x] encode_map_with_map_keys
- [x] encode_anders_map
- [x] encode_map_misordered
- [x] encode_tagged
- [x] encode_value
- [x] encode_envelope
- [x] encode_float (all Rust sub-cases: subnormals, boundary values, 65-bit negatives, exponent boundaries)
- [x] int_coerced_to_float
- [x] fail_float_coerced_to_int
- [x] non_canonical_float_1
- [x] non_canonical_float_2
- [x] unused_data
- [x] tag (TagDisplay)
- [x] encode_date
- [x] convert_values
- [x] convert_hash_map (ConvertDictionary)
- [x] convert_btree_map (ConvertSortedDictionary)
- [x] convert_vector (ConvertList)
- [x] convert_hashset (ConvertHashSet)
- [x] usage_test_1
- [x] usage_test_2
- [x] encode_nan (EncodeNonstandardNan — expanded with f32/f64 variants)
- [x] decode_nan
- [x] encode_infinit (EncodeInfinityValues — expanded)
- [x] decode_infinity

### FormatTests.cs — 15/15 Rust format tests
- [x] format_simple_1
- [x] format_simple_2
- [x] format_simple_3
- [x] format_unsigned (4 sub-cases: 0, 23, 65546, 1000000000)
- [x] format_negative (3 sub-cases: -1, -1000, -1000000)
- [x] format_string (expected-text-output rubric)
- [x] format_simple_array (expected-text-output rubric)
- [x] format_nested_array (expected-text-output rubric)
- [x] format_map (expected-text-output rubric)
- [x] format_tagged (expected-text-output rubric)
- [x] format_date (2 sub-cases, expected-text-output rubric)
- [x] format_fractional_date (expected-text-output rubric)
- [x] format_structure (expected-text-output rubric)
- [x] format_structure_2 (expected-text-output rubric)
- [x] format_key_order (expected-text-output rubric)

### WalkTests.cs — 12/12 Rust walk tests
- [x] test_traversal_counts
- [x] test_visitor_state_threading
- [x] test_early_termination
- [x] test_depth_limited_traversal
- [x] test_text_extraction
- [x] test_traversal_order_and_edge_types
- [x] test_tagged_value_traversal
- [x] test_map_keyvalue_semantics
- [x] test_stop_flag_prevents_descent
- [x] test_empty_structures
- [x] test_primitive_values
- [x] test_real_world_document

### Deferred Tests (per manifest)
- [ ] version-numbers.rs (2 tests) — Rust-specific metadata; skip in C#
- [ ] num_bigint.rs (58 tests) — feature-gated; deferred to future pass
- [ ] exact.rs inline tests (13 tests) — internal numeric conversion tests
- [ ] byte_string.rs inline test (1 test) — basic ByteString operations
- [ ] walk.rs inline tests (10 tests) — basic walk module tests (covered by integration tests)
- [ ] convert_vecdeque — N/A (no direct C# equivalent)

## Documentation Coverage
- [x] All public types have XML doc comments
- [x] DiagFormatOptions / HexFormatOptions — minimal but present
- [x] All interfaces have XML doc comments

## Derive/Protocol Coverage
- [x] Cbor: IEquatable<Cbor>, ==, !=, GetHashCode, ToString
- [x] CborCase: IEquatable<CborCase>, GetHashCode
- [x] ByteString: IEquatable, IComparable, IEnumerable<byte>, ==, !=, GetHashCode, ToString
- [x] CborMap: IEquatable, IEnumerable, GetHashCode, ToString
- [x] CborSet: IEquatable, IEnumerable, ==, !=, GetHashCode, ToString
- [x] Simple: IEquatable, GetHashCode, ToString
- [x] Tag: IEquatable, ==, !=, GetHashCode, ToString
- [x] CborDate: IEquatable, IComparable, ==, !=, GetHashCode, ToString
- [x] EdgeType: IEquatable, GetHashCode

# Completeness: dcbor → Kotlin (dcbor)

## Build & Config
- [x] .gitignore
- [x] build.gradle.kts
- [x] settings.gradle.kts

## Source Files
- [x] CborException.kt — error types (CborException sealed class)
- [x] Tag.kt — Tag, TagValue typealias
- [x] ByteString.kt — ByteString wrapper
- [x] Simple.kt — CBOR simple values
- [x] CborDate.kt — Date type (CBOR tag 1)
- [x] Cbor.kt — core CBOR type, description, debugDescription, diagnosticFlat, etc.
- [x] CborCase.kt — CBOR case sealed class (ADT representation)
- [x] CborEncodable.kt — encoding interface
- [x] CborTagged.kt — CborTagged, CborTaggedEncodable, CborTaggedDecodable, CborTaggedCodable
- [x] CborMap.kt — deterministic Map type
- [x] CborSet.kt — deterministic Set type
- [x] Varint.kt — CBOR varint encoding
- [x] Decode.kt — CBOR decoding logic
- [x] DiagFormat.kt — diagnostic formatting
- [x] HexFormat.kt — hex dump formatting
- [x] TagsStore.kt — tag registry and global store
- [x] Tags.kt — tag constants and register_tags
- [x] Walk.kt — tree traversal (EdgeType sealed class)
- [x] StringUtil.kt — string/NFC utilities
- [x] Exact.kt — numeric exactness helpers
- [x] FloatCodec.kt — float encoding/decoding
- [x] Half.kt — IEEE 754 half-precision float

## Tests
- [x] EncodeTest.kt — scalar encode/decode vectors (58 tests)
- [x] FormatTest.kt — diagnostic/hex formatting (12 tests incl. format_structure)
- [x] WalkTest.kt — tree traversal tests (3 tests)

## API Coverage
- [x] sortByCborEncoding() extension
- [x] CborTaggedDecodable interface
- [x] CborTaggedCodable interface
- [x] All 73 tests passing

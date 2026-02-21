# Completeness: dcbor → Swift (DCBOR)

## Source Files
- [x] `Sources/DCBOR/CBOR.swift` — core CBOR enum, encode/decode entrypoints
- [x] `Sources/DCBOR/Decode.swift` — deterministic decoder and canonicality checks
- [x] `Sources/DCBOR/CBORError.swift` — error surface
- [x] `Sources/DCBOR/VarInt.swift` — major-type varint encoding
- [x] `Sources/DCBOR/Int.swift` — integer conversions/canonical encoding
- [x] `Sources/DCBOR/Float.swift` — f16/f32/f64 canonical encoding and validation
- [x] `Sources/DCBOR/Simple.swift` — simple values
- [x] `Sources/DCBOR/Bytes.swift` — byte-string conversions
- [x] `Sources/DCBOR/Text.swift` — NFC text encoding/decoding
- [x] `Sources/DCBOR/Array.swift` — array conversions/coding
- [x] `Sources/DCBOR/Map.swift` — deterministic map ordering and key validation
- [x] `Sources/DCBOR/Set.swift` — deterministic set wrapper and canonical decode path
- [x] `Sources/DCBOR/Date.swift` — tagged date support
- [x] `Sources/DCBOR/Tagged.swift` — tagged value wrapper
- [x] `Sources/DCBOR/CBORTaggedEncodable.swift` — tagged encoding protocol
- [x] `Sources/DCBOR/CBORTaggedDecodable.swift` — tagged decoding protocol
- [x] `Sources/DCBOR/CBORTaggedCodable.swift` — tagged codable protocol
- [x] `Sources/DCBOR/CBOREncodable.swift` — encodable protocol
- [x] `Sources/DCBOR/CBORDecodable.swift` — decodable protocol
- [x] `Sources/DCBOR/CBORCodable.swift` — codable protocol
- [x] `Sources/DCBOR/Diag.swift` — diagnostic formatting (`diagnostic`, `diagnosticFlat`, `diagnosticAnnotated`, `summary`)
- [x] `Sources/DCBOR/Dump.swift` — hex formatting (`hex`, `hexAnnotated`, `hexOpt`)
- [x] `Sources/DCBOR/Walk.swift` — walk traversal API (`WalkElement`, `EdgeType`, visitor)
- [x] `Sources/DCBOR/Utils.swift` — utility helpers and hex parsing support
- [x] `Sources/DCBOR/Exports.swift` — module exports

## API Surface
- [x] Core CBOR symbolic representation (`CBOR`, `Map`, `Set`, `Simple`, `Tag`)
- [x] Deterministic decode constraints (duplicate/misordered map key rejection, canonical numeric checks, NFC string checks)
- [x] Date tagged encoding/decoding support
- [x] Diagnostics and annotated hex rendering
- [x] Walk traversal semantics (single + key/value visits, stop flag prevents descent)
- [x] Edge labels (`arr[i]`, `kv`, `key`, `val`, `content`)
- [x] CBOR conversion protocols (`CBOREncodable`, `CBORDecodable`, `CBORCodable`)
- [x] Tagged conversion protocols (`CBORTaggedEncodable`, `CBORTaggedDecodable`, `CBORTaggedCodable`)

## Tests
- [x] `Tests/DCBORTests/CodingTests.swift` — encode/decode behavior vectors and canonicality checks (37 tests)
- [x] `Tests/DCBORTests/FormatTests.swift` — expected-text formatting checks (whole-string assertions)
- [x] `Tests/DCBORTests/WalkTests.swift` — translated walk semantics + set behavior coverage (16 tests)
- [x] Total Swift tests passing: 53

## Build & Config
- [x] `.gitignore`
- [x] `Package.swift`
- [x] Swift package builds successfully (`swift test`)
- [x] All translated tests pass

## Deferred Scope
- [ ] `num-bigint` feature-gated behavior (`rust/dcbor/tests/num_bigint.rs` and `src/num_bigint.rs`)
- [ ] Rust metadata/version tests (`rust/dcbor/tests/version-numbers.rs`)

# Completeness: bc-ur → Swift (BCUR)

## Source Files
- [x] Package.swift — SwiftPM package definition and dependencies
- [x] Sources/BCUR/URType.swift — validated UR type string
- [x] Sources/BCUR/URError.swift — public bc-ur error type and conversions
- [x] Sources/BCUR/BytewordsStyle.swift — bytewords style enum
- [x] Sources/BCUR/BytewordsConstants.swift — bytewords and bytemoji tables
- [x] Sources/BCUR/Crc32.swift — CRC32/ISO-HDLC
- [x] Sources/BCUR/Bytewords.swift — bytewords encode/decode wrappers
- [x] Sources/BCUR/UREncoding.swift — internal UR string codec
- [x] Sources/BCUR/Xoshiro256.swift — internal PRNG
- [x] Sources/BCUR/WeightedSampler.swift — internal alias-method sampler
- [x] Sources/BCUR/FountainUtils.swift — fragment and XOR helpers
- [x] Sources/BCUR/FountainPart.swift — fountain part CBOR representation
- [x] Sources/BCUR/FountainEncoder.swift — fountain encoder
- [x] Sources/BCUR/FountainDecoder.swift — fountain decoder
- [x] Sources/BCUR/UR.swift — public UR type
- [x] Sources/BCUR/MultipartEncoder.swift — public multipart encoder wrapper
- [x] Sources/BCUR/MultipartDecoder.swift — public multipart decoder wrapper
- [x] Sources/BCUR/URCodable.swift — UREncodable/URDecodable/URCodable protocols
- [x] Sources/BCUR/Prelude.swift — public export convenience surface

## Tests
- [x] Tests/BCURTests/BytewordsTests.swift — bytewords and bytemoji vectors
- [x] Tests/BCURTests/Crc32Tests.swift — CRC32 vectors
- [x] Tests/BCURTests/Xoshiro256Tests.swift — deterministic RNG vectors
- [x] Tests/BCURTests/WeightedSamplerTests.swift — sampler vectors and edge cases
- [x] Tests/BCURTests/FountainTests.swift — fountain vectors and decoder behavior
- [x] Tests/BCURTests/URTests.swift — UR codec and multipart vectors
- [x] Tests/BCURTests/URCodableTests.swift — URCodable round-trip
- [x] Tests/BCURTests/ExampleTests.swift — bc-ur crate example tests

## Build & Config
- [x] .gitignore
- [x] swift test passes
- [x] swift test -Xswiftc -warnings-as-errors passes

## API & Docs Check (Stage 3)
- [x] Public API items from `MANIFEST.md` are present (types, protocols, constants, and methods)
- [x] Signature compatibility verified (no semantic mismatches found)
- [x] Rust behavior tests covered (39/39 behavior tests; 2 Rust metadata-sync tests intentionally N/A)
- [x] Public API documentation present for Rust-documented items

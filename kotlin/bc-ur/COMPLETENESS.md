# Completeness: bc-ur → Kotlin (bc-ur)

## Source Files
- [x] Crc32.kt — CRC32/ISO-HDLC implementation
- [x] BytewordsStyle.kt — Encoding style enum
- [x] BytewordsConstants.kt — Word/emoji arrays and lookup maps
- [x] Bytewords.kt — Encoding/decoding with CRC32 checksums
- [x] URException.kt — Error hierarchy
- [x] URType.kt — Validated type string
- [x] UREncoding.kt — Static encode/decode for UR strings
- [x] Xoshiro256.kt — PRNG implementation
- [x] WeightedSampler.kt — Alias method weighted sampling
- [x] FountainPart.kt — Part with CBOR serialization
- [x] FountainUtils.kt — Fragment length, partition, choose_fragments, xor
- [x] FountainEncoder.kt — Fountain encoder
- [x] FountainDecoder.kt — Fountain decoder
- [x] UR.kt — Main type
- [x] MultipartEncoder.kt — UR multipart encoder wrapper
- [x] MultipartDecoder.kt — UR multipart decoder wrapper
- [x] URCodable.kt — UREncodable/URDecodable/URCodable interfaces

## Tests
- [x] Crc32Test.kt — CRC32 test vectors (1 test)
- [x] BytewordsTest.kt — Bytewords encode/decode tests (4 tests)
- [x] Xoshiro256Test.kt — RNG test vectors (4 tests)
- [x] WeightedSamplerTest.kt — Sampler test vectors (4 tests)
- [x] FountainTest.kt — Fountain encoder/decoder tests (9 tests)
- [x] URTest.kt — UR encode/decode tests (5 tests)
- [x] URCodableTest.kt — URCodable round-trip tests (1 test)
- [x] MultipartTest.kt — Multipart encoder/decoder tests (2 tests)

## Build & Config
- [x] build.gradle.kts
- [x] settings.gradle.kts
- [x] .gitignore

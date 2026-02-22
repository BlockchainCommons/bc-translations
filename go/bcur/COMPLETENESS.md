# Completeness: bc-ur → Go (bcur)

## Source Files
- [x] errors.go — error types
- [x] xoshiro256.go — Xoshiro256** PRNG
- [x] weighted_sampler.go — Vose's alias weighted sampling
- [x] bytewords_constants.go — word/minimal/emoji lookup tables
- [x] bytewords.go — bytewords encode/decode with CRC32
- [x] fountain_utils.go — fragment utilities
- [x] fountain_part.go — FountainPart with manual CBOR
- [x] fountain_encoder.go — fountain encoder
- [x] fountain_decoder.go — fountain decoder
- [x] ur_encoding.go — low-level UR string format
- [x] ur_type.go — URType validated string
- [x] ur.go — UR type
- [x] multipart_encoder.go — MultipartEncoder
- [x] multipart_decoder.go — MultipartDecoder
- [x] ur_encodable.go — UREncodable interface + ToUR/ToURString + DecodeUR/DecodeURString

## Tests
- [x] bcur_test.go — CRC, RNG, shuffle, sampler, bytewords, fountain, fragments
- [x] ur_test.go — UR encode/decode, multipart, codable, custom encoder

## Build & Config
- [x] .gitignore
- [x] go.mod
- [x] go.sum

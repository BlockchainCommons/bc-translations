# Translation Manifest: bc-ur (v0.19.0)

## Crate Overview

bc-ur provides Uniform Resources (UR) encoding for CBOR structures. It wraps the `ur` crate (v0.4.1) which implements fountain codes for multipart encoding.

## External Dependencies

| Rust Crate | Version | Translation Strategy |
|------------|---------|---------------------|
| dcbor | 0.25.1 | Use Go dcbor package |
| ur | 0.4.1 | Reimplement inline (fountain, bytewords, xoshiro, sampler, crc32) |

## Public API Surface

### Types

| Rust Type | Go Type | Notes |
|-----------|---------|-------|
| `UR` | `UR` | Main UR type with FromURString, String, QRString, QRData, CheckType |
| `URType` | `URType` | Validated type string (lowercase + digits + hyphens) |
| `MultipartEncoder` | `MultipartEncoder` | Fountain-coded multipart UR string emitter |
| `MultipartDecoder` | `MultipartDecoder` | Fountain-coded multipart UR string receiver |
| `BytewordsStyle` | `BytewordsStyle` | Enum: Standard, URI, Minimal |

### Interfaces (Traits)

| Rust Trait | Go Interface | Notes |
|------------|--------------|-------|
| `UREncodable` | `UREncodable` | Extends CBORTaggedEncodable; provides ToUR, URString |
| `URDecodable` | N/A | Go uses generic decode functions instead |
| `URCodable` | N/A | Marker; implicit in Go via interface satisfaction |

### Free Functions

| Rust Function | Go Function | Notes |
|---------------|-------------|-------|
| `UREncodable::ur()` | `ToUR(obj)` | Convert UREncodable to UR |
| `UREncodable::ur_string()` | `ToURString(obj)` | Convert UREncodable to UR string |
| `URDecodable::from_ur()` | `FromUR[T](ur, tags, decode)` | Generic decode UR to type |
| `URDecodable::from_ur_string()` | `FromURString[T](str, tags, decode)` | Generic decode UR string to type |
| `bytewords::encode` | `BytewordsEncode` | Encode bytes as bytewords with CRC32 |
| `bytewords::decode` | `BytewordsDecode` | Decode bytewords string, verify CRC32 |
| `bytewords::identifier` | `BytewordsIdentifier` | 4-byte identifier as space-separated words |
| `bytewords::bytemoji_identifier` | `BytemojiIdentifier` | 4-byte identifier as space-separated emojis |

### Error Types

| Rust Error | Go Error | Notes |
|------------|----------|-------|
| `Error::UR` | `ErrUR` | Wrapper around ur crate errors |
| `Error::Bytewords` | `ErrBytewords` | Wrapper around bytewords errors |
| `Error::Cbor` | dcbor errors | Pass through dcbor errors |
| `Error::InvalidScheme` | `ErrInvalidScheme` | Missing "ur:" prefix |
| `Error::TypeUnspecified` | `ErrTypeUnspecified` | No type after scheme |
| `Error::InvalidType` | `ErrInvalidType` | Invalid UR type characters |
| `Error::NotSinglePart` | `ErrNotSinglePart` | Attempted single-part decode of multipart UR |
| `Error::UnexpectedType` | `UnexpectedTypeError` | Type mismatch (expected, found) |

### Constants

| Rust Constant | Go Constant | Notes |
|---------------|-------------|-------|
| `BYTEWORDS` | `Bytewords` | 256 four-letter words |
| `BYTEMOJIS` | `Bytemojis` | 256 emoji identifiers |
| `MINIMALS` | `minimals` | 256 two-letter codes (unexported) |

## Internal Modules (ur crate reimplementation)

| Module | Go File | Notes |
|--------|---------|-------|
| fountain::Encoder | fountain_encoder.go | Fragment, encode, emit parts |
| fountain::Decoder | fountain_decoder.go | Receive parts, XOR-based reconstruction |
| fountain::Part | fountain_part.go | Manual CBOR encode/decode |
| fountain utilities | fountain_utils.go | fragmentLength, partition, chooseFragments, xorBytes |
| bytewords | bytewords.go | encode/decode with CRC32 checksums |
| bytewords constants | bytewords_constants.go | Word/minimal/emoji lookup tables |
| xoshiro | xoshiro256.go | Xoshiro256** seeded via SHA-256 |
| sampler | weighted_sampler.go | Vose's alias weighted sampling |
| crc32 | (use hash/crc32) | Go stdlib CRC-32/IEEE = ISO-HDLC |

## Test Inventory

### bc-ur crate tests

| Test | Go Test | Notes |
|------|---------|-------|
| test_encode | TestEncode | UR encode example |
| test_decode | TestDecode | UR decode example |
| test_fountain | TestFountain | Fountain roundtrip with start_part offsets |
| test_ur | TestUR | UR string roundtrip |
| test_ur_codable | TestURCodable | UREncodable/URDecodable roundtrip |
| test_bytemoji_uniqueness | TestBytemojiUniqueness | All 256 bytemojis are unique |
| test_bytemoji_lengths | TestBytemojiLengths | All bytemojis <= 4 UTF-8 bytes |

### ur crate tests

| Test | Go Test | Notes |
|------|---------|-------|
| test_single_part_ur | TestSinglePartUR | Single-part UR encode/decode |
| test_ur_encoder | TestUREncoder | 20 multipart UR strings with exact vectors |
| test_multipart_ur | TestMultipartUR | Large multipart roundtrip (32KB) |
| test_decoder | TestURDecoder | Error case coverage |
| test_custom_encoder | TestCustomEncoder | Custom UR type encoding |
| test_rng_1/2/3 | TestRNG1/2/3 | Xoshiro256** output verification |
| test_shuffle | TestShuffle | Shuffle correctness |
| test_fragment_length | TestFragmentLength | Fragment length calculation |
| test_partition_and_join | TestPartitionAndJoin | Message partitioning with exact hex vectors |
| test_choose_fragments | TestChooseFragments | 30 fragment index sets |
| test_xor | TestXor | XOR operation |
| test_fountain_encoder | TestFountainEncoder | 20 encoder parts with exact hex data |
| test_fountain_encoder_cbor | TestFountainEncoderCBOR | 20 CBOR-encoded parts |
| test_fountain_decoder | TestFountainDecoder | Large message decode |
| test_fountain_decoder_skip | TestFountainDecoderSkip | Decode with dropped parts |
| test_fountain_part_cbor | TestFountainPartCBOR | Part CBOR roundtrip |
| test_sampler | TestSampler | 500 weighted samples |
| test_choose_degree | TestChooseDegree | 200 degree choices |
| test_crc | TestCRC | CRC32 test vectors |
| test_bytewords | TestBytewords | All three styles encode/decode |
| test_encoding | TestBytewordsEncoding | 100-byte encode/decode |

## Translation Hazards

1. **Manual CBOR for FountainPart**: Must use manual CBOR encoding (not dcbor) to match minicbor byte output
2. **Xoshiro256** seeding**: SHA-256 hash bytes read as big-endian u64, then stored as little-endian seed
3. **Decoder queue processing**: `processQueue` must NOT be called from `processComplex` (Rust defers queue work)
4. **CRC32**: Go's `hash/crc32.ChecksumIEEE` matches CRC-32/ISO-HDLC exactly
5. **Go `&` and `<<` same precedence**: Always parenthesize when mixing bitwise operators

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: no. This crate deals with binary encoding, not formatted text output.

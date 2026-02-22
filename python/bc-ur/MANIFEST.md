# Translation Manifest: bc-ur (v0.19.0)

## Crate Overview

bc-ur provides Uniform Resources (UR) encoding for CBOR structures. It wraps the `ur` crate (v0.4.1) which implements fountain codes for multipart encoding.

## External Dependencies

| Rust Crate | Version | Translation Strategy |
|------------|---------|---------------------|
| dcbor | 0.25.1 | Use Python dcbor package |
| ur | 0.4.1 | Reimplement inline (fountain, bytewords, xoshiro, sampler, crc32) |

## Public API Surface

### Types

| Rust Type | Status | Notes |
|-----------|--------|-------|
| `UR` | Required | Main UR type with from_ur_string, string, qr_string, qr_data, check_type |
| `URType` | Required | Validated type string (lowercase + digits + hyphens) |
| `MultipartEncoder` | Required | Fountain-coded multipart UR string emitter |
| `MultipartDecoder` | Required | Fountain-coded multipart UR string receiver |
| `BytewordsStyle` | Required | Enum: Standard, Uri, Minimal |

### Traits (Protocols)

| Rust Trait | Status | Notes |
|------------|--------|-------|
| `UREncodable` | Required | Blanket impl for CBORTaggedEncodable; translate as protocol + standalone functions |
| `URDecodable` | Required | Blanket impl for CBORTaggedDecodable; translate as protocol + standalone functions |
| `URCodable` | Required | Marker combining UREncodable + URDecodable (implicit in Python) |

### Free Functions

| Rust Function | Status | Notes |
|---------------|--------|-------|
| `to_ur(obj)` | Required | Convert UREncodable to UR |
| `to_ur_string(obj)` | Required | Convert UREncodable to UR string |
| `from_ur(cls, ur)` | Required | Decode UR to type |
| `from_ur_string(cls, ur_string)` | Required | Decode UR string to type |
| `bytewords::encode` | Required | Encode bytes as bytewords with CRC32 |
| `bytewords::decode` | Required | Decode bytewords string, verify CRC32 |
| `bytewords::identifier` | Required | 4-byte identifier as space-separated words |
| `bytewords::bytemoji_identifier` | Required | 4-byte identifier as space-separated emojis |

### Error Types

| Rust Error | Status |
|------------|--------|
| `URError` | Required (base) |
| `URDecoderError` | Required |
| `BytewordsError` | Required |
| `URCborError` | Required |
| `InvalidSchemeError` | Required |
| `TypeUnspecifiedError` | Required |
| `InvalidTypeError` | Required |
| `NotSinglePartError` | Required |
| `UnexpectedTypeError` | Required |
| `FountainError` | Required |

### Constants

| Rust Constant | Status | Notes |
|---------------|--------|-------|
| `BYTEWORDS` | Required | 256 four-letter words |
| `BYTEMOJIS` | Required | 256 emoji identifiers |
| `MINIMALS` | Required | 256 two-letter codes |

## Internal Modules (ur crate reimplementation)

| Module | Status | Notes |
|--------|--------|-------|
| fountain::Encoder | Required | Fragment, encode, emit parts |
| fountain::Decoder | Required | Receive parts, XOR-based reconstruction |
| fountain::Part | Required | Manual CBOR encode/decode (minicbor-compatible) |
| fountain utilities | Required | fragment_length, partition, choose_fragments, xor |
| bytewords | Required | encode/decode with CRC32 checksums |
| xoshiro | Required | Xoshiro256** seeded via SHA-256 |
| sampler | Required | Vose's alias weighted sampling |
| crc32 | Required | CRC-32/ISO-HDLC |

## Test Inventory

### bc-ur crate tests

| Test | Source | Notes |
|------|--------|-------|
| test_encode | lib.rs | UR encode example |
| test_decode | lib.rs | UR decode example |
| test_fountain | lib.rs | Fountain roundtrip with start_part offsets |
| test_ur | ur.rs | UR string roundtrip |
| test_ur_codable | ur_codable.rs | UREncodable/URDecodable roundtrip |
| test_bytemoji_uniqueness | bytewords.rs | All 256 bytemojis are unique |
| test_bytemoji_lengths | bytewords.rs | All bytemojis <= 4 UTF-8 bytes |

### ur crate tests

| Test | Source | Notes |
|------|--------|-------|
| test_single_part_ur | ur.rs | Single-part UR encode/decode |
| test_ur_encoder | ur.rs | 20 multipart UR strings with exact vectors |
| test_multipart_ur | ur.rs | Large multipart roundtrip (32KB) |
| test_decoder | ur.rs | Error case coverage |
| test_custom_encoder | ur.rs | Custom UR type encoding |
| test_rng_1/2/3 | xoshiro.rs | Xoshiro256** output verification |
| test_shuffle | xoshiro.rs | Shuffle correctness |
| test_fragment_length | fountain.rs | Fragment length calculation |
| test_partition_and_join | fountain.rs | Message partitioning with exact hex vectors |
| test_choose_fragments | fountain.rs | 30 fragment index sets |
| test_xor | fountain.rs | XOR operation |
| test_fountain_encoder | fountain.rs | 20 encoder parts with exact hex data |
| test_fountain_encoder_cbor | fountain.rs | 20 CBOR-encoded parts |
| test_fountain_decoder | fountain.rs | Large message decode |
| test_fountain_decoder_skip | fountain.rs | Decode with dropped parts |
| test_fountain_part_cbor | fountain.rs | Part CBOR roundtrip |
| test_sampler | fountain.rs | 500 weighted samples |
| test_choose_degree | fountain.rs | 200 degree choices |

## Translation Hazards

1. **Manual CBOR for FountainPart**: Must use manual CBOR encoding (not dcbor) to match minicbor byte output
2. **Xoshiro256** seeding**: SHA-256 hash bytes read as big-endian u64 (Rust does big→LE→LE = net big-endian)
3. **Decoder queue processing**: `process_queue` must NOT be called from `process_complex` (Rust defers queue work)
4. **CRC32**: Python's `binascii.crc32` matches CRC-32/ISO-HDLC exactly

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: no. This crate deals with binary encoding, not formatted text output.

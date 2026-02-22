# Translation Manifest: bc-ur → Kotlin (bc-ur)

## Source Crate

- **Name:** bc-ur
- **Version:** 0.19.0
- **Repository:** https://github.com/BlockchainCommons/bc-ur-rust

## Internal Dependencies

- `dcbor` (0.25.x) → Kotlin `dcbor` ✅

## External Dependencies

| Rust Crate | Purpose | Kotlin Equivalent |
|------------|---------|---------------|
| `ur` (0.4.1) | Core UR/bytewords/fountain implementation | **Implement from scratch** |
| `thiserror` | Derive macro for Error | Not needed (use exception hierarchy) |
| `minicbor` (transitive via `ur`) | CBOR for fountain parts | Use dcbor library |
| `bitcoin_hashes` (transitive via `ur`) | SHA-256 for Xoshiro seeding | `java.security.MessageDigest` |
| `rand_xoshiro` (transitive via `ur`) | Xoshiro256** PRNG | Implement inline |
| `crc` (transitive via `ur`) | CRC32/ISO-HDLC for bytewords | Implement inline |
| `phf` (transitive via `ur`) | Compile-time hash maps for bytewords lookup | Use `HashMap<String, Byte>` |

## Feature Flags

None. The crate has no feature flags.

## Public API Surface

### Types

| Rust Type | Kotlin Type | Notes |
|-----------|---------|-------|
| `UR` | `UR` | Main Uniform Resource type |
| `URType` | `URType` | Validated UR type string |
| `Error` | `URException` hierarchy | Custom exceptions |
| `Result<T>` | Throw `URException` | |
| `UREncodable` trait | `UREncodable` interface | Extension of CborTaggedEncodable |
| `URDecodable` trait | `URDecodable` interface | Extension of CborTaggedDecodable |
| `URCodable` trait | `URCodable` interface | Marker combining both |
| `MultipartEncoder` | `MultipartEncoder` | Fountain-based UR encoder |
| `MultipartDecoder` | `MultipartDecoder` | Fountain-based UR decoder |
| `bytewords::Style` | `BytewordsStyle` | Enum: Standard, Uri, Minimal |

### Internal Types (from `ur` crate, must be reimplemented)

| Rust Type | Kotlin Type | Visibility |
|-----------|---------|------------|
| `ur::fountain::Encoder` | `FountainEncoder` | internal |
| `ur::fountain::Decoder` | `FountainDecoder` | internal |
| `ur::fountain::Part` | `FountainPart` | internal |
| `ur::xoshiro::Xoshiro256` | `Xoshiro256` | internal |
| `ur::sampler::Weighted` | `WeightedSampler` | internal |
| `ur::bytewords` module | `Bytewords` object | public |
| CRC32/ISO-HDLC | `Crc32` | internal |

### UR (main type)

```
UR(urType, cbor) -> UR
UR.fromUrString(string) -> UR
UR.toUrString() -> string
UR.toQrString() -> string
UR.toQrData() -> ByteArray
UR.checkType(expected) -> Unit (throws)
UR.urType -> URType
UR.urTypeStr -> String
UR.cbor -> Cbor
toString()
```

### URType

```
URType(string) -> URType (validates)
URType.value -> String
```

### Bytewords (public object)

```
Bytewords.encode(ByteArray, BytewordsStyle) -> String
Bytewords.decode(String, BytewordsStyle) -> ByteArray
Bytewords.identifier(ByteArray) -> String
Bytewords.bytemojiIdentifier(ByteArray) -> String
BytewordsStyle enum: Standard, Uri, Minimal
BYTEWORDS constant array (256 words)
BYTEMOJIS constant array (256 emojis)
```

### Error Hierarchy

```
URException (base)
├── URDecoderException(message: String)
├── BytewordsException(message: String)
├── CborException (wraps dcbor CborException)
├── InvalidSchemeException
├── TypeUnspecifiedException
├── InvalidTypeException
├── NotSinglePartException
└── UnexpectedTypeException(expected: String, found: String)
```

### UREncodable / URDecodable / URCodable

```
UREncodable : CborTaggedEncodable
  fun toUR(): UR
  fun toURString(): String

URDecodable : CborTaggedDecodable
  companion object { fun fromUR(ur: UR): T }
  companion object { fun fromURString(urString: String): T }

URCodable : UREncodable, URDecodable
```

### MultipartEncoder

```
MultipartEncoder(ur: UR, maxFragmentLen: Int)
fun nextPart(): String
val currentIndex: Int
val partsCount: Int
```

### MultipartDecoder

```
MultipartDecoder()
fun receive(part: String): Unit
val isComplete: Boolean
val message: UR? (null if not complete)
```

## Translation Units (Build Order)

1. **Crc32** — CRC32/ISO-HDLC implementation (internal utility)
2. **BytewordsConstants** — Word/emoji arrays and lookup dictionaries
3. **Bytewords** — Encoding/decoding with CRC32 checksums
4. **URException** — Error hierarchy
5. **URType** — Validated type string
6. **UREncoding** — Static encode/decode for UR strings (from `ur::ur` module)
7. **Xoshiro256** — PRNG implementation
8. **WeightedSampler** — Alias method weighted sampling
9. **FountainPart** — Part with CBOR serialization
10. **FountainUtils** — Fragment length, partition, choose_fragments, xor
11. **FountainEncoder** — Fountain encoder
12. **FountainDecoder** — Fountain decoder
13. **UR** — Main type
14. **MultipartEncoder** — UR multipart encoder wrapper
15. **MultipartDecoder** — UR multipart decoder wrapper
16. **UREncodable/URDecodable/URCodable** — Trait interfaces

## Test Inventory

| Rust Test | Kotlin Test | Source |
|-----------|---------|--------|
| `bytewords::tests::test_bytemoji_uniqueness` | `BytemojiUniqueness` | bytewords.rs |
| `bytewords::tests::test_bytemoji_lengths` | `BytemojiLengths` | bytewords.rs |
| `ur::bytewords::tests::test_crc` | `Crc32Test` | ur crate |
| `ur::bytewords::tests::test_bytewords` | `BytewordsEncodeDecode` | ur crate |
| `ur::bytewords::tests::test_encoding` | `BytewordsEncoding100Bytes` | ur crate |
| `ur::ur::tests::test_single_part_ur` | `SinglePartUr` | ur crate |
| `ur::ur::tests::test_ur_encoder` | `UrEncoder20Parts` | ur crate |
| `ur::ur::tests::test_decoder` | `DecoderErrorCases` | ur crate |
| `ur::ur::tests::test_custom_encoder` | `CustomEncoder` | ur crate |
| `ur::ur::tests::test_multipart_ur` | `MultipartUr` | ur crate |
| `ur::xoshiro::tests::test_rng_1` | `Rng1` | ur crate |
| `ur::xoshiro::tests::test_rng_2` | `Rng2` | ur crate |
| `ur::xoshiro::tests::test_rng_3` | `Rng3` | ur crate |
| `ur::xoshiro::tests::test_shuffle` | `Shuffle` | ur crate |
| `ur::sampler::tests::test_sampler` | `Sampler500Samples` | ur crate |
| `ur::sampler::tests::test_choose_degree` | `ChooseDegree200` | ur crate |
| `ur::fountain::tests::test_fragment_length` | `FragmentLength` | ur crate |
| `ur::fountain::tests::test_partition_and_join` | `PartitionAndJoin` | ur crate |
| `ur::fountain::tests::test_choose_fragments` | `ChooseFragments30` | ur crate |
| `ur::fountain::tests::test_xor` | `XorTest` | ur crate |
| `ur::fountain::tests::test_fountain_encoder` | `FountainEncoder20Parts` | ur crate |
| `ur::fountain::tests::test_fountain_encoder_cbor` | `FountainEncoderCbor` | ur crate |
| `ur::fountain::tests::test_decoder` | `FountainDecoder` | ur crate |
| `ur::fountain::tests::test_decoder_skip` | `FountainDecoderSkip` | ur crate |
| `ur::fountain::tests::test_fountain_cbor` | `FountainPartCbor` | ur crate |
| `ur::tests::test_ur` | `UrRoundTrip` | ur.rs |
| `ur_codable::tests::test_ur_codable` | `UrCodableRoundTrip` | ur_codable.rs |
| `example_tests::encode` | `ExampleEncode` | lib.rs |
| `example_tests::decode` | `ExampleDecode` | lib.rs |
| `example_tests::test_fountain` | `ExampleFountain` | lib.rs |

## Translation Hazards

1. **External `ur` crate must be reimplemented from scratch** — This is the biggest challenge. The fountain codes, Xoshiro256**, weighted sampling, bytewords, and CRC32 must all be ported inline.
2. **Xoshiro256** seeding** — The `ur` crate seeds from SHA-256 hash of input, with a specific byte-order transform. Must match exactly.
3. **CRC32/ISO-HDLC** — Must use the same polynomial (0xEDB88320 reflected). JVM has no built-in CRC32 with this specific variant easily accessible.
4. **Fountain part CBOR serialization** — The `ur` crate uses `minicbor` for encoding fountain parts as CBOR arrays. We must use dcbor instead, producing byte-identical output.
5. **Floating point precision** — The weighted sampler uses `f64` arithmetic. Kotlin `Double` should match.
6. **`make_message` test utility** — Used in many tests. Must be reimplemented using Xoshiro256.

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: no — bc-ur tests use byte-level assertions and UR string comparisons, not complex text rendering.

# Completeness: bc-ur → C# (BCUR)

## Source Files
- [x] Crc32.cs — CRC32/ISO-HDLC implementation (internal)
- [x] URException.cs — error types (9 exception classes)
- [x] BytewordsStyle.cs — enum: Standard, Uri, Minimal
- [x] Bytewords.cs — bytewords encoding/decoding with CRC32
- [x] URType.cs — validated UR type string
- [x] Xoshiro256.cs — Xoshiro256** PRNG (internal, for fountain codes)
- [x] WeightedSampler.cs — alias method weighted sampling (internal)
- [x] FountainUtils.cs — fragment length, partition, choose_fragments, xor (internal)
- [x] FountainPart.cs — fountain code part with CBOR serialization (internal)
- [x] FountainEncoder.cs — fountain code encoder (internal)
- [x] FountainDecoder.cs — fountain code decoder (internal)
- [x] UREncoding.cs — UR string encode/decode (internal)
- [x] UR.cs — Uniform Resource type (public)
- [x] MultipartEncoder.cs — UR multipart encoder wrapper (public)
- [x] MultipartDecoder.cs — UR multipart decoder wrapper (public)
- [x] IUREncodable.cs — UREncodable trait interface + extension methods
- [x] IURDecodable.cs — URDecodable trait interface + extension methods
- [x] IURCodable.cs — URCodable marker interface

## Tests
- [x] Crc32Tests.cs — CRC32 test vectors (2 tests)
- [x] BytewordsTests.cs — encode/decode, 100-byte encoding, bytemoji uniqueness/lengths (4 tests)
- [x] Xoshiro256Tests.cs — Rng1, Rng2, Rng3, Shuffle (4 tests)
- [x] WeightedSamplerTests.cs — 500-sample test, ChooseDegree200 (2 tests)
- [x] FountainTests.cs — FragmentLength, PartitionAndJoin, ChooseFragments30, XorTest, Encoder20Parts, EncoderCbor, Decoder, DecoderSkip, PartCbor, error cases (14 tests)
- [x] URTests.cs — SinglePartUr, UrRoundTrip, UrEncoder20Parts, DecoderErrorCases, CustomEncoder, MultipartUr (6 tests)
- [x] URCodableTests.cs — UrCodableRoundTrip (1 test)
- [x] ExampleTests.cs — ExampleEncode, ExampleDecode, ExampleFountain (3 tests)

## Build & Config
- [x] BCUR.csproj (with InternalsVisibleTo)
- [x] BCUR.Tests.csproj
- [x] BCUR.slnx
- [x] .gitignore

## Summary
- **36 tests total, all passing**
- **18 source files**
- **100% API coverage confirmed**

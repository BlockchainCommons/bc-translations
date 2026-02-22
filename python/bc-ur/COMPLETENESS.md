# Completeness: bc-ur -> Python (bc-ur)

## Source Files
- [x] _crc32.py -- CRC-32/ISO-HDLC via binascii
- [x] error.py -- full exception hierarchy (10 types)
- [x] ur_type.py -- URType validation class
- [x] _xoshiro256.py -- Xoshiro256** PRNG with SHA-256 seeding
- [x] _weighted_sampler.py -- Vose's alias method weighted sampling
- [x] _bytewords_constants.py -- BYTEWORDS, MINIMALS, BYTEMOJIS, lookup dicts
- [x] bytewords.py -- BytewordsStyle enum, encode/decode/identifier/bytemoji_identifier
- [x] _fountain_part.py -- FountainPart with manual CBOR encode/decode
- [x] _fountain_utils.py -- fragment_length, partition, choose_fragments, xor_bytes
- [x] _fountain_encoder.py -- FountainEncoder class
- [x] _fountain_decoder.py -- FountainDecoder class
- [x] _ur_encoding.py -- encode_ur and decode_ur functions
- [x] ur.py -- UR class with full API
- [x] multipart_encoder.py -- MultipartEncoder wrapper
- [x] multipart_decoder.py -- MultipartDecoder wrapper with type checking
- [x] codable.py -- UREncodable/URDecodable protocols, to_ur/from_ur helpers
- [x] __init__.py -- public API exports

## Tests
- [x] test_crc32.py -- CRC32 test vectors
- [x] test_bytewords.py -- bytewords encode/decode/error tests
- [x] test_xoshiro256.py -- RNG output/shuffle tests
- [x] test_weighted_sampler.py -- sampling distribution + degree choice tests
- [x] test_fountain.py -- encoder/decoder/part/fragment tests
- [x] test_ur.py -- UR encode/decode/multipart/error tests
- [x] test_ur_codable.py -- UREncodable/URDecodable roundtrip
- [x] test_examples.py -- bc-ur crate example tests
- [x] conftest.py -- make_message_ur helper

## Build & Config
- [x] pyproject.toml
- [x] .gitignore

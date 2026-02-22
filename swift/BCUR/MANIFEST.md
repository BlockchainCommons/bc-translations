# Translation Manifest: bc-ur → Swift (BCUR)

## Source Crates

- **Primary crate:** `bc-ur` `0.19.0`
- **Reference dependency crate:** `ur` `0.4.1` (Rust API and test vectors used as source-of-truth for multipart/fountain internals)
- **Repository:** https://github.com/BlockchainCommons/bc-ur-rust

## Internal Dependencies

- `dcbor` (`0.25.x`) → Swift package `DCBOR` (`swift/DCBOR`) ✅

## External Dependencies

| Rust Crate | Purpose | Swift Equivalent |
|------------|---------|------------------|
| `ur` | Core UR codec, bytewords, fountain encoder/decoder | Implement inline in `BCUR` (internal types) |
| `thiserror` | Error derive macro | Not needed (native Swift `Error` enums) |
| `minicbor` (transitive via `ur`) | CBOR for fountain parts | `DCBOR` for deterministic CBOR encode/decode |
| `bitcoin_hashes` (transitive via `ur`) | SHA-256 for Xoshiro seed expansion | `CryptoKit.SHA256` |
| `rand_xoshiro` (transitive via `ur`) | Xoshiro256** PRNG | Implement inline (`Xoshiro256`) |
| `crc` (transitive via `ur`) | CRC32/ISO-HDLC for bytewords | Implement inline (`Crc32`) |
| `phf` (transitive via `ur`) | Static word lookup maps | Swift `[String: UInt8]` dictionaries |

## Feature Flags

- No feature flags in `bc-ur`.
- Translation scope: default behavior only (entire crate is default scope).

## Public API Surface

### Types / Protocols / Aliases

- `UR` → `public struct UR`
- `URType` → `public struct URType`
- `Error` → `public enum URError`
- `Result<T>` → `public typealias URResult<T> = Result<T, URError>`
- `UREncodable` trait → `public protocol UREncodable`
- `URDecodable` trait → `public protocol URDecodable`
- `URCodable` trait → `public protocol URCodable`
- `MultipartEncoder` → `public final class MultipartEncoder`
- `MultipartDecoder` → `public final class MultipartDecoder`
- `bytewords::Style` → `public enum BytewordsStyle`

### Constants

- `BYTEWORDS` → `public let BYTEWORDS: [String]` (256 values)
- `BYTEMOJIS` → `public let BYTEMOJIS: [String]` (256 values)

### Functions and Methods

- `bytewords::encode(data, style)` → `Bytewords.encode(_:style:)`
- `bytewords::decode(data, style)` → `Bytewords.decode(_:style:)`
- `bytewords::identifier([u8;4])` → `Bytewords.identifier(_:)`
- `bytewords::bytemoji_identifier([u8;4])` → `Bytewords.bytemojiIdentifier(_:)`

- `UR::new(type, cbor)`
- `UR::from_ur_string(string)`
- `UR::string()`
- `UR::qr_string()`
- `UR::qr_data()`
- `UR::check_type(expected)`
- `UR::ur_type()` / `UR::ur_type_str()`
- `UR::cbor()`
- Conversions: `UR -> String`, `UR -> CBOR`

- `URType::new(string)`
- `URType::string()`

- `MultipartEncoder::new(ur, max_fragment_len)`
- `MultipartEncoder::next_part()`
- `MultipartEncoder::current_index()`
- `MultipartEncoder::parts_count()`

- `MultipartDecoder::new()`
- `MultipartDecoder::receive(part)`
- `MultipartDecoder::is_complete()`
- `MultipartDecoder::message()`

- `UREncodable::ur()` / `UREncodable::ur_string()` (protocol extension)
- `URDecodable::from_ur(...)` / `URDecodable::from_ur_string(...)` (protocol extension)

### Internal Translation Units Required (from `ur` crate)

- `Crc32`
- `UREncoding` (single/multipart UR string parser/formatter)
- `Xoshiro256`
- `WeightedSampler`
- `FountainUtils`
- `FountainPart`
- `FountainEncoder`
- `FountainDecoder`

## Documentation Catalog

- Crate-level docs: **yes** (`lib.rs` crate docs include spec, usage, and examples).
- Module-level docs:
  - `ur` dependency modules (`bytewords`, `fountain`, `ur`) have detailed module docs.
  - `bc-ur` modules are lightly documented, mostly at type/method level.
- Public items with Rust doc comments:
  - `UR` and several `UR` methods (`new`, `from_ur_string`, `string`, `qr_string`, `qr_data`, `check_type`, `ur_type_str`)
  - `URType` (`new`, `string`)
  - `UREncodable`, `URDecodable`, `URCodable`
  - `bytewords::identifier`, `bytewords::bytemoji_identifier`, `BYTEMOJIS` note
- Public items without Rust doc comments (retain minimal/none as appropriate):
  - Some helper exports and constants in `bytewords` module (for example `BYTEWORDS` table declaration has no dedicated Rust doc block)
  - error variant-level details are mostly via `thiserror` messages rather than API docs
- Package metadata description (`Cargo.toml`): **"Uniform Resources (UR) for Rust."**
- README: **yes** (crate usage, rationale, and examples)

## Test Inventory

### Rust `bc-ur` crate tests

- `lib.rs::tests::test_readme_deps` — metadata sync check (**N/A in Swift**) 
- `lib.rs::tests::test_html_root_url` — metadata sync check (**N/A in Swift**)
- `lib.rs::example_tests::encode`
- `lib.rs::example_tests::decode`
- `lib.rs::example_tests::test_fountain`
- `ur.rs::tests::test_ur`
- `ur_codable.rs::tests::test_ur_codable`

### Rust `ur` dependency tests to port for behavior parity

- `bytewords.rs`
  - `test_crc`
  - `test_bytewords`
  - `test_encoding`
- `xoshiro.rs`
  - `test_rng_1`
  - `test_rng_2`
  - `test_rng_3`
  - `test_shuffle`
- `sampler.rs`
  - `test_sampler`
  - `test_choose_degree`
  - `test_negative_weights`
  - `test_zero_weights`
- `fountain.rs`
  - `test_fragment_length`
  - `test_partition_and_join`
  - `test_choose_fragments`
  - `test_xor`
  - `test_fountain_encoder`
  - `test_fountain_encoder_cbor`
  - `test_fountain_encoder_zero_max_length`
  - `test_fountain_encoder_is_complete`
  - `test_decoder`
  - `test_empty_encoder`
  - `test_decoder_skip_some_simple_fragments`
  - `test_decoder_receive_return_value`
  - `test_decoder_part_validation`
  - `test_empty_decoder_empty_part`
  - `test_fountain_cbor`
  - `test_part_from_cbor_errors`
  - `test_part_from_cbor_unsigned_types`
- `ur.rs`
  - `test_single_part_ur`
  - `test_ur_encoder`
  - `test_ur_encoder_decoder_bc_crypto_request`
  - `test_multipart_ur`
  - `test_decoder`
  - `test_custom_encoder`

## Translation Unit Order

1. `BytewordsStyle`, `URError`, `URResult`
2. `URType`
3. `Crc32`
4. `BytewordsConstants`
5. `Bytewords`
6. `UREncoding`
7. `Xoshiro256`
8. `WeightedSampler`
9. `FountainUtils`
10. `FountainPart`
11. `FountainEncoder`
12. `FountainDecoder`
13. `UR`
14. `MultipartEncoder`
15. `MultipartDecoder`
16. `UREncodable` / `URDecodable` / `URCodable`
17. `Prelude` exports
18. Tests (in inventory order)

## Translation Hazards

1. Xoshiro seed derivation from SHA-256 must preserve Rust byte-order transform exactly.
2. Bytewords checksum uses CRC32/ISO-HDLC; polynomial/initial/final behavior must match Rust `crc` crate.
3. Fountain-part CBOR encoding must be byte-identical to Rust `minicbor` vectors.
4. Decoder behavior around duplicate and inconsistent parts must match Rust return-value semantics (`receive` returns false for duplicates or complete decoders).
5. `UR` parsing is case-insensitive in `bc-ur` (lowercased input first), while low-level `ur::decode` expects lowercase `ur:` scheme; Swift wrappers should preserve the high-level behavior.
6. Rust `URType` accepts empty strings (character validation is `all(...)`); avoid silently tightening unless done explicitly in fluency stage with dependent fixes.
7. Public trait/protocol defaults (`URDecodable`/`UREncodable`) rely on first tag name conventions; missing tag names should fail clearly.

## EXPECTED TEXT OUTPUT RUBRIC

- Applicable: no
- Reason: Tests validate deterministic bytes, CBOR payloads, UR strings, and error conditions; no complex multiline rendered text outputs are part of behavior.

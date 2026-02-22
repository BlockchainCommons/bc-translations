# Translation Manifest: bc-ur v0.19.0 (TypeScript)

## Crate Overview

`bc-ur` provides Uniform Resources (UR) helpers on top of `dcbor`, including:

- Single-part UR parsing/serialization (`UR`, `URType`)
- Bytewords/bytemoji helpers and constants
- Trait-style UR codable helpers layered on dcbor tagged traits
- Multipart fountain wrappers (`MultipartEncoder`, `MultipartDecoder`)

Rust source analyzed: `rust/bc-ur`.

## Dependencies

### Internal BC Dependencies

- `dcbor` (required) -> TypeScript package `@bc/dcbor`

### External Dependencies (Rust -> TypeScript)

| Rust crate | Purpose | TypeScript equivalent |
|---|---|---|
| `ur` | UR encode/decode, bytewords, multipart fountain encoder/decoder | `@ngraveio/bc-ur` (UR + UREncoder + URDecoder + bytewords) |
| `thiserror` | Error derive/display | Native `Error` subclass + enum-like error kind helpers |
| `version-sync` (dev) | README/Cargo version consistency checks | No direct TS equivalent; replaced by package metadata checks in CI |

### Package Metadata

- NPM package name: `@bc/ur`
- Package description target: `Uniform Resources (UR) for TypeScript.`

## Feature Flags

Rust crate features:

- No crate-defined feature flags.

Initial TypeScript scope:

- Full default crate behavior.

## Public API Surface

### Re-Exports at Crate Root

- `UR`
- `URType`
- `Error`
- `Result<T>`
- `UREncodable`
- `URDecodable`
- `URCodable`
- `MultipartDecoder`
- `MultipartEncoder`
- `bytewords` module
- `prelude` module

### Type Catalog

- `Error` (enum)
  - Variants:
    - `UR(String)`
    - `Bytewords(String)`
    - `Cbor(dcbor::Error)`
    - `InvalidScheme`
    - `TypeUnspecified`
    - `InvalidType`
    - `NotSinglePart`
    - `UnexpectedType(String, String)`
- `UR` (struct)
  - Fields:
    - `ur_type: URType`
    - `cbor: CBOR`
  - Derives: `Debug`, `Clone`, `PartialEq`
  - Implements: `Display`, `From<UR> for CBOR`, `From<UR> for String`, `TryFrom<String>`, `AsRef<UR>`
- `URType` (newtype struct over `String`)
  - Derives: `Debug`, `Clone`, `PartialEq`
  - Implements: `TryFrom<String>`, `TryFrom<&str>`
- `MultipartEncoder<'a>` (struct)
  - Field:
    - `encoder: ur::Encoder<'a>`
- `MultipartDecoder` (struct)
  - Fields:
    - `ur_type: Option<URType>`
    - `decoder: ur::Decoder`
  - Implements: `Default`
- `Result<T>` (type alias)
  - Alias of `std::result::Result<T, Error>`

### Trait Catalog

- `URTypeChar`
  - Required methods:
    - `is_ur_type(&self) -> bool`
  - Implemented for `char`
- `URTypeString`
  - Required methods:
    - `is_ur_type(&self) -> bool`
  - Implemented for `&str` and `String`
- `UREncodable: CBORTaggedEncodable`
  - Provided methods:
    - `ur(&self) -> UR`
    - `ur_string(&self) -> String`
  - Blanket impl for all `T: CBORTaggedEncodable`
- `URDecodable: CBORTaggedDecodable`
  - Provided associated functions:
    - `from_ur(ur: impl AsRef<UR>) -> dcbor::Result<Self>`
    - `from_ur_string(ur_string: impl Into<String>) -> dcbor::Result<Self>`
  - Blanket impl for all `T: CBORTaggedDecodable`
- `URCodable`
  - Marker trait
  - Blanket impl for `T: UREncodable + URDecodable`

### Function Catalog

#### `ur.rs`

- `UR::new(ur_type: impl TryInto<URType, Error = Error>, cbor: impl Into<CBOR>) -> Result<UR>`
- `UR::from_ur_string(ur_string: impl Into<String>) -> Result<UR>`
- `UR::string(&self) -> String`
- `UR::qr_string(&self) -> String`
- `UR::qr_data(&self) -> Vec<u8>`
- `UR::check_type(&self, other_type: impl TryInto<URType, Error = Error>) -> Result<()>`
- `UR::ur_type(&self) -> &URType`
- `UR::ur_type_str(&self) -> &str`
- `UR::cbor(&self) -> CBOR`

#### `ur_type.rs`

- `URType::new(ur_type: impl Into<String>) -> Result<URType>`
- `URType::string(&self) -> &str`

#### `bytewords.rs`

- `encode(data: impl AsRef<[u8]>, style: Style) -> String`
- `identifier(data: &[u8; 4]) -> String`
- `bytemoji_identifier(data: &[u8; 4]) -> String`
- `decode(data: &str, style: Style) -> Result<Vec<u8>>`

#### `multipart_encoder.rs`

- `MultipartEncoder::new(ur: &UR, max_fragment_len: usize) -> Result<Self>`
- `MultipartEncoder::next_part(&mut self) -> Result<String>`
- `MultipartEncoder::current_index(&self) -> usize`
- `MultipartEncoder::parts_count(&self) -> usize`

#### `multipart_decoder.rs`

- `MultipartDecoder::new() -> Self`
- `MultipartDecoder::receive(&mut self, value: &str) -> Result<()>`
- `MultipartDecoder::is_complete(&self) -> bool`
- `MultipartDecoder::message(&self) -> Result<Option<UR>>`
- Private helper:
  - `decode_type(ur_string: &str) -> Result<URType>`

### Constant Catalog

- `BYTEWORDS: [&str; 256]`
- `BYTEMOJIS: [&str; 256]`

### Prelude Catalog

`prelude` re-exports:

- `dcbor::prelude::*`
- `Error as URError`
- `Result as URResult`
- `UR`
- `URCodable`
- `URDecodable`
- `UREncodable`
- `MultipartDecoder`
- `MultipartEncoder`
- `bytewords`

## Documentation Catalog

- Crate-level docs: **yes** (`lib.rs` has extensive `//!` docs with introduction/spec/usage examples).
- Module-level docs:
  - `bytewords.rs`: bytemoji constants doc link; identifier docs.
  - `ur.rs`, `ur_type.rs`, `ur_encodable.rs`, `ur_decodable.rs`, `ur_codable.rs`: item-level docs.
- Public items with doc comments (representative):
  - `UR` struct and most of its methods.
  - `URType::new`, `URType::string`.
  - `UREncodable` trait and methods.
  - `URDecodable` trait.
  - `URCodable` trait.
  - `identifier`, `bytemoji_identifier`, `BYTEMOJIS` note.
  - `URTypeChar::is_ur_type`, `URTypeString::is_ur_type`.
- Public items without doc comments (keep parity; do not invent where Rust omitted):
  - `bytewords::encode`, `bytewords::decode`.
  - `MultipartEncoder` and methods.
  - `MultipartDecoder` and methods.
  - `UR::ur_type`, `UR::cbor`.
  - `BYTEWORDS` constant.
- Package metadata description from Cargo: `Uniform Resources (UR) for Rust.`
- README present: **yes** (`rust/bc-ur/README.md`), mirrors crate-level intro, includes dependency snippet/spec links/release history.

## Test Inventory

Rust tests are inline (`#[cfg(test)]`) under `src/`:

- `src/lib.rs`
  - `test_readme_deps` (version metadata sync; Rust-specific)
  - `test_html_root_url` (docs metadata sync; Rust-specific)
  - `encode` (UR creation + string output)
  - `decode` (UR parse + type + CBOR round-trip)
  - `test_fountain` (multipart encode/decode recovery using start offsets 1, 51, 101, 501)
- `src/ur.rs`
  - `test_ur` (single-part UR encode/decode + uppercase input acceptance)
- `src/bytewords.rs`
  - `test_bytemoji_uniqueness` (all bytemojis unique)
  - `test_bytemoji_lengths` (all bytemojis <= 4 bytes)
- `src/ur_codable.rs`
  - `test_ur_codable` (tagged type to UR string and back)

Translation target tests:

- Port behavior tests (`encode`, `decode`, `test_fountain`, `test_ur`, `test_ur_codable`, bytemoji checks).
- Rust-only metadata tests (`version-sync`) are not ported as runtime tests.
- Preserve expected UR string vectors exactly:
  - `ur:test/lsadaoaxjygonesw`
  - `ur:leaf/iejyihjkjygupyltla`
- Preserve fountain expected completion indexes:
  - `5`, `61`, `110`, `507`

## Translation Unit Order

1. Project scaffold (`.gitignore`, `package.json`, `tsconfig.json`, `vitest.config.ts`)
2. Error surface (`error.ts`) and utilities (`utils.ts`)
3. UR type wrapper (`ur-type.ts`)
4. Core UR wrapper (`ur.ts`)
5. Bytewords wrappers/constants (`bytewords.ts`)
6. Multipart wrappers (`multipart-encoder.ts`, `multipart-decoder.ts`)
7. Trait-equivalent helpers (`ur-encodable.ts`, `ur-decodable.ts`, `ur-codable.ts`)
8. Prelude and barrel exports (`prelude.ts`, `index.ts`)
9. Tests (UR, examples/fountain, bytewords, URCodable)

## Translation Hazards

1. `UR::from_ur_string` lowercases input before parsing; uppercase UR input must still decode.
2. Rust validates UR scheme/type before full decode; preserve error semantics (`InvalidScheme`, `TypeUnspecified`, `InvalidType`, `NotSinglePart`).
3. `URType` validity is strict: only `a-z`, `0-9`, `-` are accepted.
4. `UREncodable::ur()` requires first CBOR tag to have a `name`; missing name panics in Rust.
5. `URDecodable::from_ur()` validates UR type against first tag name before decoding CBOR content.
6. Multipart decoder must reject mixed UR types across parts (`UnexpectedType`).
7. `BYTEWORDS` and `BYTEMOJIS` are exact 256-entry lookup tables; preserve order exactly.
8. Bytemojis include non-ASCII Unicode; preserve exact scalar values and keep UTF-8 length check behavior.
9. TS mapping to external UR library uses `Buffer` internally; ensure stable conversion to/from `Uint8Array` without mutation or encoding loss.
10. Rust Result conversions to `dcbor::Error` (`From<Error> for dcbor::Error`) must map to meaningful TypeScript errors for callers using `@bc/dcbor`.

## EXPECTED TEXT OUTPUT RUBRIC

- Applicable: no
- Reason: Rust tests do not validate complex multiline rendered text/diagnostics/CLI output; this crate is vector/string/byte equality focused.

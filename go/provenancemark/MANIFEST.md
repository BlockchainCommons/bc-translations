# Translation Manifest: provenance-mark 0.24.0

## Crate Metadata
- Rust crate: `provenance-mark`
- Version: `0.24.0`
- Description: A cryptographically-secured system for establishing and verifying the authenticity of works.
- Default features: `envelope`
- Internal BC dependencies:
  - `bc-rand ^0.5.0`
  - `dcbor ^0.25.0` with `multithreaded`
  - `bc-ur ^0.19.2`
  - `bc-tags ^0.12.0`
  - `bc-envelope ^0.43.0` (optional in Rust, enabled by default feature)

## Public API Surface

### Module Exports (`lib.rs`)
- Re-exported modules/types:
  - `validate::*`
  - `error::{Error, Result}`
  - `resolution::*`
  - `mark::*`
  - `mark_info::*`
  - `generator::*`
  - `seed::*`
  - `rng_state::*`
- Public modules:
  - `crypto_utils`
  - `date`
  - `util`
  - `xoshiro256starstar`

### Type Catalog
- `Error`
  - Enum-style error surface covering seed length, key length, chain ID length, info CBOR validity, date bounds, URL parameter presence, resolution serialization, wrapped dependency errors, and validation failures.
- `Result<T>`
  - Alias for `Result<T, Error>`.
- `ProvenanceMarkResolution`
  - Enum with variants `Low`, `Medium`, `Quartile`, `High`.
  - Public helpers: link lengths, sequence/date byte lengths, byte ranges, date/sequence serialize+deserialize, display text.
- `ProvenanceSeed`
  - Fixed 32-byte seed newtype.
  - Constructors from random source and passphrase.
  - CBOR byte-string conversions and JSON block encoding.
- `RngState`
  - Fixed 32-byte RNG state newtype.
  - CBOR byte-string conversions and JSON block encoding.
- `Xoshiro256StarStar`
  - Deterministic PRNG with explicit state/data conversion and byte generation helpers.
- `ProvenanceMark`
  - Core mark type with JSON fields:
    - `seq`
    - `date`
    - `res`
    - `chain_id`
    - `key`
    - `hash`
    - `info_bytes`
  - Derived/cached fields:
    - `seq_bytes`
    - `date_bytes`
  - Equality/hash semantics are based on `(resolution, message())`.
- `ProvenanceMarkGenerator`
  - Stateful generator for a mark sequence:
    - `res`
    - `seed`
    - `chain_id`
    - `next_seq`
    - `rng_state`
- `ProvenanceMarkInfo`
  - Convenience wrapper containing:
    - `ur`
    - `bytewords`
    - `bytemoji`
    - `comment`
    - `mark`
- `ValidationReportFormat`
  - Enum-like output selector: `Text`, `JsonCompact`, `JsonPretty`.
- `ValidationIssue`
  - Sum type with variants:
    - `HashMismatch`
    - `KeyMismatch`
    - `SequenceGap`
    - `DateOrdering`
    - `NonGenesisAtZero`
    - `InvalidGenesisKey`
- `FlaggedMark`
  - Mark plus zero or more validation issues.
- `SequenceReport`
  - `start_seq`, `end_seq`, `marks`.
- `ChainReport`
  - `chain_id`, `has_genesis`, `marks`, `sequences`.
- `ValidationReport`
  - `marks`, `chains`, formatters, issue summary, validator entrypoint.

### Constant Catalog
- `crypto_utils::SHA256_SIZE = 32`
- `seed::PROVENANCE_SEED_LENGTH = 32`
- `rng_state::RNG_STATE_LENGTH = 32`

### Function and Method Catalog

#### `crypto_utils`
- `sha256(data)`
- `sha256_prefix(data, prefix)`
- `extend_key(data)`
- `hkdf_hmac_sha256(key_material, salt, key_len)`
- `obfuscate(key, message)`

#### `date`
- Trait `SerializableDate` methods for dCBOR dates:
  - `serialize_2_bytes`
  - `deserialize_2_bytes`
  - `serialize_4_bytes`
  - `deserialize_4_bytes`
  - `serialize_6_bytes`
  - `deserialize_6_bytes`
- `range_of_days_in_month(year, month)`

#### `util`
- `serialize_base64`
- `deserialize_base64`
- `parse_seed`
- `parse_date`
- `serialize_cbor`
- `deserialize_cbor`
- `serialize_block`
- `deserialize_block`
- `serialize_iso8601`
- `deserialize_iso8601`
- `serialize_ur`
- `deserialize_ur`

#### `ProvenanceSeed`
- `new`
- `new_using`
- `new_with_passphrase`
- `to_bytes`
- `from_bytes`
- `from_slice`
- `hex`
- `Default`
- `From<[u8; 32]>`
- `From<ProvenanceSeed> for [u8; 32]`
- `From<ProvenanceSeed> for CBOR`
- `TryFrom<CBOR>`

#### `RngState`
- `to_bytes`
- `from_bytes`
- `from_slice`
- `hex`
- `From<[u8; 32]>`
- `From<RngState> for [u8; 32]`
- `From<RngState> for CBOR`
- `TryFrom<CBOR>`

#### `Xoshiro256StarStar`
- `to_state`
- `from_state`
- `to_data`
- `from_data`
- `next_byte`
- `next_bytes`
- `next_u32`
- `next_u64`
- `fill_bytes`

#### `ProvenanceMarkResolution`
- `link_length`
- `seq_bytes_length`
- `date_bytes_length`
- `fixed_length`
- `key_range`
- `chain_id_range`
- `hash_range`
- `seq_bytes_range`
- `date_bytes_range`
- `info_range`
- `serialize_date`
- `deserialize_date`
- `serialize_seq`
- `deserialize_seq`
- `Display`
- `From<u8>`
- `TryFrom<u8>`
- `From<CBOR>`
- `TryFrom<CBOR>`

#### `ProvenanceMark`
- Accessors:
  - `res`
  - `key`
  - `hash`
  - `chain_id`
  - `seq_bytes`
  - `date_bytes`
  - `seq`
  - `date`
  - `message`
  - `info`
- Constructors/decoders:
  - `new`
  - `from_message`
  - `from_bytewords`
  - `from_url_encoding`
  - `from_url`
  - `TryFrom<CBOR>`
  - `TryFrom<Envelope>` when envelope feature is enabled
- Identity APIs added/changed in `0.24.0`:
  - `id`
  - `id_hex`
  - `id_bytewords(word_count, prefix)`
  - `id_bytemoji(word_count, prefix)`
  - `id_bytewords_minimal(word_count, prefix)`
  - `disambiguated_id_bytewords(marks, prefix)`
  - `disambiguated_id_bytemoji(marks, prefix)`
- Sequence and encoding helpers:
  - `precedes`
  - `precedes_opt`
  - `is_sequence_valid`
  - `is_genesis`
  - `to_bytewords_with_style`
  - `to_bytewords`
  - `to_url_encoding`
  - `to_url`
  - `fingerprint`
  - `register_tags_in`
  - `register_tags`
  - `CBORTagged`
  - `CBORTaggedEncodable`
  - `CBORTaggedDecodable`
  - `From<ProvenanceMark> for CBOR`
  - `From<ProvenanceMark> for Envelope` when envelope feature is enabled
- Formatting:
  - `Debug`
  - `Display`

#### `ProvenanceMarkGenerator`
- Accessors:
  - `res`
  - `seed`
  - `chain_id`
  - `next_seq`
  - `rng_state`
- Constructors:
  - `new_with_seed`
  - `new_with_passphrase`
  - `new_using`
  - `new_random`
  - `new`
- State machine:
  - `next(date, info)`
- Formatting and conversions:
  - `Display`
  - `From<ProvenanceMarkGenerator> for Envelope`
  - `TryFrom<Envelope>`

#### `ProvenanceMarkInfo`
- `new(mark, comment)`
- `mark`
- `ur`
- `bytewords`
- `bytemoji`
- `comment`
- `markdown_summary`
- Custom deserialize path that rebuilds `mark` from `ur`

#### `ValidationIssue`
- `Display`
- `Error`

#### `FlaggedMark`
- `mark`
- `issues`

#### `SequenceReport`
- `start_seq`
- `end_seq`
- `marks`

#### `ChainReport`
- `chain_id`
- `has_genesis`
- `marks`
- `sequences`
- `chain_id_hex`

#### `ValidationReport`
- `marks`
- `chains`
- `format`
- `has_issues`
- `validate`
- Internal but behaviorally important:
  - `format_text`
  - `is_interesting`
  - `build_sequence_bins`
  - `create_sequence_report`

## Documentation Catalog
- Crate-level doc comment: yes
  - Intro, installation snippet, and examples section in `src/lib.rs`.
- README: yes
  - Duplicates intro, version history, and background/spec links.
- Public items with doc comments:
  - `ValidationReportFormat`
  - `ValidationIssue`
  - `FlaggedMark`
  - `SequenceReport`
  - `ChainReport`
  - `ValidationReport`
  - `hkdf_hmac_sha256`
  - New `0.24.0` Mark ID methods in `mark.rs`
  - Sequence validation helpers in `mark.rs`
- Public items without doc comments:
  - Most constructors/accessors on `ProvenanceSeed`, `RngState`, `ProvenanceMark`, `ProvenanceMarkGenerator`, and `ProvenanceMarkInfo`
  - Most utility functions in `util.rs`
- Package metadata description: present in `Cargo.toml`

## External Dependencies and Go Equivalents
- `sha2`
  - Use `crypto/sha256`.
- `hkdf`
  - Use Go HKDF (`crypto/hkdf` if available in toolchain, otherwise `golang.org/x/crypto/hkdf`).
- `chacha20`
  - Use `golang.org/x/crypto/chacha20` for raw RFC 7539 stream cipher behavior.
- `chrono`
  - Use `time.Time` wrapped by `dcbor.Date`.
- `thiserror`
  - Use idiomatic Go `error` values plus structured error types where needed.
- `hex`
  - Use `encoding/hex`.
- `rand_core`
  - Inline the PRNG implementation in `xoshiro256starstar.go`.
- `serde`
  - Implement explicit `MarshalJSON` / `UnmarshalJSON` methods to match Rust field names and encodings.
- `serde_json`
  - Use `encoding/json`.
- `base64`
  - Use `encoding/base64`.
- `url`
  - Use `net/url`.

## Feature Flags
- `default = ["envelope"]`
- `envelope = ["bc-envelope"]`
- Translation policy:
  - Implement default-feature behavior only.
  - Envelope support is required in the initial Go translation.
  - No non-default-only branch work is needed.

## Test Inventory
- `tests/crypto_utils.rs`
  - `test_sha256`
  - `test_extend_key`
  - `test_obfuscate`
- `tests/date.rs`
  - `test_2_byte_dates`
  - `test_4_byte_dates`
  - `test_6_byte_dates`
- `tests/xoshiro256starstar.rs`
  - `test_rng`
  - `test_save_rng_state`
- `tests/identifier.rs` (new in `0.24.0`, 22 tests)
  - full `id*` / `disambiguated_id*` API coverage, word-count bounds, prefix behavior, collision behavior, and panic cases
- `tests/mark.rs` (11 tests)
  - resolution-specific mark generation:
    - `test_low`
    - `test_low_with_info`
    - `test_medium`
    - `test_medium_with_info`
    - `test_quartile`
    - `test_quartile_with_info`
    - `test_high`
    - `test_high_with_info`
  - envelope / debug formatting:
    - `test_envelope`
  - Rust-only repo metadata checks:
    - `test_readme_deps`
    - `test_html_root_url`
  - Translation policy for the two metadata tests:
    - Do not port them as literal Go tests.
    - Cover their intent via package metadata/doc completeness checks instead.
- `tests/validate.rs` (19 tests)
  - validation output JSON/text
  - deduplication
  - multiple chains
  - missing genesis
  - sequence gaps
  - out-of-order input sorting
  - hash mismatch
  - date ordering failure
  - multi-sequence chains
  - chain ID hex helper
  - info-bearing marks
  - chain sorting
  - genesis checks
  - constructed invalid cases (`DateOrdering`, `NonGenesisAtZero`, `InvalidGenesisKey`)

## EXPECTED TEXT OUTPUT RUBRIC
- Applicable: yes
- Source signals:
  - `tests/validate.rs` uses repeated `// expected-text-output-rubric:` markers for pretty JSON, compact JSON, and text summary output.
  - `tests/mark.rs:test_envelope` uses full expected envelope/debug rendering.
- Target tests to apply:
  - validation report text output
  - validation report pretty/compact JSON exact strings
  - envelope rendering / debug-string tests
- Required pattern:
  - one literal `actual` vs `expected` assertion with mismatch output showing both blocks

## Translation Unit Order
1. `errors.go` — exported error surface and dependency wrapping
2. `crypto_utils.go` — SHA-256 / HKDF / ChaCha20 helpers
3. `date.go` — 2/4/6-byte date serialization helpers
4. `resolution.go` — resolution enum and byte geometry helpers
5. `seed.go` — `ProvenanceSeed`
6. `rng_state.go` — `RngState`
7. `xoshiro256starstar.go` — deterministic RNG
8. `mark.go` — core mark encoding/decoding and ID API
9. `generator.go` — generator state machine
10. `mark_info.go` — summary wrapper and markdown output
11. `validate.go` — validation reports and formatting
12. `util.go` — explicit JSON/base64/CBOR/UR helpers
13. `doc.go` and `go.mod`
14. tests in dependency order:
  - crypto utils
  - date
  - xoshiro
  - identifier
  - mark
  - validate

## Hazards
- Raw ChaCha20 stream cipher, not ChaCha20-Poly1305.
- `obfuscate` derives its nonce from the reversed final 12 bytes of the HKDF-expanded key.
- Xoshiro state encoding uses little-endian `u64` lanes.
- `ProvenanceMark` equality/hash are based on the generated message, not raw field-by-field Go struct equality.
- Low resolution serializes dates in a compressed 2-byte Y/M/D format with a 2023 base year.
- Medium and higher resolutions serialize from a `2001-01-01T00:00:00Z` reference instant.
- `dcbor.Date.String()` uses date-only output at midnight UTC; tests depend on that distinction.
- `info_bytes` must round-trip as CBOR bytes and be omitted from JSON when empty.
- `ProvenanceMark` JSON unmarshal must recompute cached `seq_bytes` and `date_bytes`.
- `ProvenanceMarkInfo` JSON unmarshal must rebuild `mark` from the UR, ignoring any inline `mark` field.
- `0.24.0` ID APIs depend on generalized bytewords/bytemoji encoding for arbitrary byte counts up to 32.
- Disambiguation logic extends only colliding prefixes; identical marks stay identical at 32 words/emojis.
- Go has no static methods; Rust associated functions like `disambiguated_id_*` need package-level equivalents or another idiomatic exported form.
- `ValidationReport::validate` deduplicates exact duplicate marks before chain grouping.
- Validation sorts marks by sequence within each chain and sorts chains by chain ID before formatting.
- The validation text formatter intentionally returns an empty string for a single perfect chain.
- Envelope conversion uses the default feature and requires registering provenance tag summarizers into the envelope format context.
- The Rust repo metadata tests are source-repo-specific and should be replaced by Go-specific metadata/documentation checks, not copied literally.

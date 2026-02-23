# Translation Manifest: provenance-mark 0.23.0

## Crate Metadata
- Rust crate: `provenance-mark`
- Version: `0.23.0`
- Description: A cryptographically-secured system for establishing and verifying the authenticity of works.
- Default features: `envelope`
- Internal BC dependencies:
  - `bc-rand ^0.5.0`
  - `dcbor ^0.25.0` (with `multithreaded` in Rust)
  - `bc-ur ^0.19.0`
  - `bc-tags ^0.12.0`
  - `bc-envelope ^0.43.0` (optional, enabled by default feature)

## External Dependencies and Kotlin Equivalents
- `sha2` (SHA-256): use JDK `java.security.MessageDigest`.
- `hkdf` (HKDF-HMAC-SHA256): implement via `javax.crypto.Mac` (`HmacSHA256`) and HKDF extract/expand.
- `chacha20` (stream cipher): implement crate-local ChaCha20 (RFC 7539) for deterministic parity.
- `chrono` (date/time): use `java.time` (`Instant`, `ZonedDateTime`, UTC calendar logic).
- `thiserror`: use Kotlin sealed exception hierarchy.
- `hex`: use crate-local hex helpers.
- `rand_core`: crate-local Xoshiro256** with exact Rust state/byte behavior.
- `serde`/`serde_json`: use Jackson (`jackson-databind`, `jackson-module-kotlin`) with explicit field names and serializers.
- `base64`: JDK `java.util.Base64`.
- `url`: `java.net.URI`/`java.net.URL` and query handling.

## Feature Flags
- `default = ["envelope"]`
- `envelope = ["bc-envelope"]`
- Translation policy: implement default-feature behavior (envelope support enabled). No non-default-only branch work required.

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
- `Error` (enum): rich error variants for seed/key lengths, date bounds, URL/CBOR/UR/JSON/base64 conversion, validation, envelope.
- `Result<T>` (type alias): `std::result::Result<T, Error>`.
- `ProvenanceMarkResolution` (enum, repr `u8`): `Low`, `Medium`, `Quartile`, `High`.
- `ProvenanceSeed` (struct): fixed 32-byte seed wrapper.
- `RngState` (struct): fixed 32-byte PRNG state wrapper.
- `ProvenanceMark` (struct): canonical mark with serialized subfields (`seq`, `date`, `res`, `chain_id`, `key`, `hash`, `info_bytes`, cached `seq_bytes`, `date_bytes`).
- `ProvenanceMarkInfo` (struct): convenience packaging of mark + UR + identifiers + comment.
- `ProvenanceMarkGenerator` (struct): stateful mark sequence generator (`res`, `seed`, `chainID`, `nextSeq`, `rngState`).
- `ValidationReportFormat` (enum): `Text`, `JsonCompact`, `JsonPretty`.
- `ValidationIssue` (enum): `HashMismatch`, `KeyMismatch`, `SequenceGap`, `DateOrdering`, `NonGenesisAtZero`, `InvalidGenesisKey`.
- `FlaggedMark` (struct): mark + issue list.
- `SequenceReport` (struct): contiguous sequence subsection.
- `ChainReport` (struct): per-chain report.
- `ValidationReport` (struct): complete validation result.
- `SerializableDate` (trait): 2/4/6-byte date serialization contract (implemented for `dcbor::Date`).
- `Xoshiro256StarStar` (struct): deterministic PRNG with explicit state/data conversion.

### Constant Catalog
- `crypto_utils::SHA256_SIZE = 32`
- `seed::PROVENANCE_SEED_LENGTH = 32`
- `rng_state::RNG_STATE_LENGTH = 32`

### Function/Method Catalog (Public)
- `crypto_utils`:
  - `sha256(data)`
  - `sha256_prefix(data, prefix)`
  - `extend_key(data)`
  - `hkdf_hmac_sha256(key_material, salt, key_len)`
  - `obfuscate(key, message)`
- `date`:
  - trait `SerializableDate` methods: `serialize_2_bytes`, `deserialize_2_bytes`, `serialize_4_bytes`, `deserialize_4_bytes`, `serialize_6_bytes`, `deserialize_6_bytes`
  - `range_of_days_in_month(year, month)`
- `ProvenanceSeed`:
  - constructors: `new`, `new_using`, `new_with_passphrase`, `from_bytes`, `from_slice`
  - accessors/util: `to_bytes`, `hex`
- `RngState`:
  - constructors: `from_bytes`, `from_slice`
  - accessors/util: `to_bytes`, `hex`
- `Xoshiro256StarStar`:
  - `to_state`, `from_state`, `to_data`, `from_data`, `next_byte`, `next_bytes` (+ `RngCore` behavior in Rust)
- `ProvenanceMarkResolution`:
  - sizing/ranges: `link_length`, `seq_bytes_length`, `date_bytes_length`, `fixed_length`, `key_range`, `chain_id_range`, `hash_range`, `seq_bytes_range`, `date_bytes_range`, `info_range`
  - serialization: `serialize_date`, `deserialize_date`, `serialize_seq`, `deserialize_seq`
- `ProvenanceMark`:
  - constructors/parsers: `new`, `from_message`, `from_bytewords`, `from_url_encoding`, `from_url`
  - accessors: `res`, `key`, `hash`, `chain_id`, `seq_bytes`, `date_bytes`, `seq`, `date`, `message`, `info`
  - identifiers: `identifier`, `bytewords_identifier`, `bytemoji_identifier`, `bytewords_minimal_identifier`
  - sequence checks: `precedes`, `precedes_opt`, `is_sequence_valid`, `is_genesis`
  - encodings: `to_bytewords_with_style`, `to_bytewords`, `to_url_encoding`, `to_url`
  - crypto digest: `fingerprint`
  - validation entrypoint: `validate(marks)`
  - CBOR/UR/envelope conversions and tagged encode/decode support.
- `ProvenanceMarkGenerator`:
  - accessors: `res`, `seed`, `chain_id`, `next_seq`, `rng_state`
  - constructors: `new_with_seed`, `new_with_passphrase`, `new_using`, `new_random`, `new`
  - generator: `next(date, info)`
- `ProvenanceMarkInfo`:
  - constructor: `new(mark, comment)`
  - accessors: `mark`, `ur`, `bytewords`, `bytemoji`, `comment`
  - renderer: `markdown_summary`
- `ValidationReport` and related:
  - `ValidationReport::marks`, `chains`, `format`, `has_issues`, `validate`
  - `ChainReport::chain_id`, `has_genesis`, `marks`, `sequences`, `chain_id_hex`
  - `SequenceReport::start_seq`, `end_seq`, `marks`
  - `FlaggedMark::mark`, `issues`
- `mark` module free functions:
  - `register_tags_in(context)`
  - `register_tags()`
- `util` helpers (public module):
  - base64/cbor/date/UR serializers and parsers
  - `parse_seed`, `parse_date`

## Documentation Catalog
- Crate-level docs: present (`lib.rs`) with introduction/getting-started/examples.
- README: present; mirrors intro and release notes.
- Public items with docs: most error variants, validation/report types, date trait, identifier methods, and register-tag helpers.
- Public items without docs: several utility helper functions and simple accessors.
- Target translation requirement: preserve Rust-documented public API comments; do not invent docs where absent.

## Test Inventory
Integration tests under `rust/provenance-mark/tests/`:
- `crypto_utils.rs`:
  - `test_sha256`
  - `test_extend_key`
  - `test_obfuscate`
- `date.rs`:
  - `test_2_byte_dates`
  - `test_4_byte_dates`
  - `test_6_byte_dates`
- `xoshiro256starstar.rs`:
  - `test_rng`
  - `test_save_rng_state`
- `mark.rs`:
  - `test_low`
  - `test_low_with_info`
  - `test_medium`
  - `test_medium_with_info`
  - `test_quartile`
  - `test_quartile_with_info`
  - `test_high`
  - `test_high_with_info`
  - `test_readme_deps`
  - `test_html_root_url`
  - `test_envelope` (`envelope` feature)
- `validate.rs`:
  - `test_validate_empty`
  - `test_validate_single_mark`
  - `test_validate_valid_sequence`
  - `test_validate_deduplication`
  - `test_validate_multiple_chains`
  - `test_validate_missing_genesis`
  - `test_validate_sequence_gap`
  - `test_validate_out_of_order`
  - `test_validate_hash_mismatch`
  - `test_validate_date_ordering_violation`
  - `test_validate_multiple_sequences_in_chain`
  - `test_validate_precedes_opt`
  - `test_validate_chain_id_hex`
  - `test_validate_with_info`
  - `test_validate_sorted_chains`
  - `test_validate_genesis_check`
  - `test_validate_date_ordering_violation_constructed`
  - `test_validate_non_genesis_at_seq_zero`
  - `test_validate_invalid_genesis_key_constructed`
- Shared test helper macro:
  - `tests/common/mod.rs`: `assert_actual_expected!` full actual-vs-expected mismatch reporter.

## EXPECTED TEXT OUTPUT RUBRIC
- Applicable: yes
- Source signals:
  - Multiple `// expected-text-output-rubric:` markers in `tests/validate.rs`.
  - Extensive pretty/compact JSON literal comparisons and text report comparisons.
- Target tests to apply:
  - All `ValidationReport.format(JsonPretty|JsonCompact|Text)` expected-output assertions in `ValidateTest.kt`.
  - Use full-text expected-vs-actual helper output on mismatch.

## Translation Hazards
- Deterministic cryptography/PRNG parity is strict:
  - ChaCha20 keystream semantics and nonce derivation from reversed extended key.
  - Xoshiro256** state byte order (`little-endian`) and per-byte generation (`next_u64 as u8`).
- Date precision and bounds differ by resolution:
  - 2-byte Y/M/D packed format (2023–2150)
  - 4-byte seconds since 2001-01-01
  - 6-byte milliseconds since 2001-01-01 with explicit max `0xe5940a78a7ff`
- Equality/hash semantics for `ProvenanceMark` depend on `(res, message())`, not all fields directly.
- JSON compatibility needs exact field names and serializers:
  - `chainID`, `nextSeq`, `rngState`, base64-encoded byte fields, ISO-8601 dates.
- UR/CBOR/envelope conversions require tag registration and compatible tag names (`provenance`).
- Validation grouping logic must preserve deduplication, chain sorting, sequence binning, and issue assignment behavior.

## Translation Unit Order
1. `ProvenanceMarkException` (`error.rs`) and result alias
2. Low-level utilities: `Util.kt`, hex/base64/date parse helpers
3. `CryptoUtils.kt` + `ChaCha20.kt`
4. `DateSerialization.kt` (`SerializableDate` + month-range helper)
5. `ProvenanceMarkResolution.kt`
6. `ProvenanceSeed.kt`
7. `RngState.kt`
8. `Xoshiro256StarStar.kt`
9. `ProvenanceMark.kt` (core + encoding/conversions + registerTags)
10. `ProvenanceMarkInfo.kt`
11. `ProvenanceMarkGenerator.kt`
12. `Validate.kt` (validation reports/issues/formatting)
13. Tests (`CryptoUtilsTest`, `DateTest`, `Xoshiro256StarStarTest`, `MarkTest`, `ValidateTest`)
14. Build/config and dependency wiring (`build.gradle.kts`, `settings.gradle.kts`, wrapper)

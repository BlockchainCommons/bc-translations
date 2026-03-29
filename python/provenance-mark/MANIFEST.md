# Translation Manifest: provenance-mark 0.24.0

## Crate Metadata
- Rust crate: `provenance-mark`
- Version: `0.24.0`
- Description: A cryptographically-secured system for establishing and verifying the authenticity of works
- Default features: `envelope`
- Internal BC dependencies:
  - `bc-rand ^0.5.0`
  - `dcbor ^0.25.0` with `multithreaded`
  - `bc-ur ^0.19.2`
  - `bc-tags ^0.12.0`
  - `bc-envelope ^0.43.0` (optional, enabled by default feature)

## External Dependencies and Python Equivalents
- `sha2`: use `hashlib.sha256`.
- `hkdf`: implement HKDF-HMAC-SHA256 with `hmac` and `hashlib`.
- `chacha20`: implement crate-local ChaCha20 (RFC 7539) for deterministic parity with Rust.
- `chrono`: use `datetime`, `timezone.utc`, `timedelta`, and `calendar.monthrange`.
- `thiserror`: use Python exception classes with structured error codes.
- `hex`: use `bytes.hex()` / `bytes.fromhex()`.
- `rand_core`: implement crate-local `Xoshiro256StarStar` with exact little-endian state layout and byte generation.
- `serde` / `serde_json`: use `json` plus explicit dict serializers/deserializers to preserve field names and ordering.
- `base64`: use `base64.b64encode` / `base64.b64decode`.
- `url`: use `urllib.parse`.

## Feature Flags
- `default = ["envelope"]`
- `envelope = ["bc-envelope"]`
- Translation policy: implement default-feature behavior only. Envelope conversions and tag registration are in scope because the default feature enables them.

## Public API Surface

### Module Exports (`lib.rs`)
- Re-exported modules and types:
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
- `Error`: crate error model covering seed lengths, key lengths, message length, invalid CBOR info payloads, date/range issues, URL/base64/JSON/CBOR conversion, envelope conversion, and validation failures.
- `Result<T>`: `Result[T, Error]` equivalent.
- `ProvenanceMarkResolution`: enum values `Low`, `Medium`, `Quartile`, `High`.
- `ProvenanceSeed`: fixed 32-byte seed wrapper.
- `RngState`: fixed 32-byte PRNG state wrapper.
- `Xoshiro256StarStar`: deterministic PRNG with explicit 256-bit state.
- `ProvenanceMark`: canonical mark representation with cached sequence/date byte encodings, UR/URL/CBOR support, identifier helpers, and sequence validation helpers.
- `ProvenanceMarkInfo`: convenience wrapper exposing a mark, UR, identifier strings, optional comment, and markdown rendering.
- `ProvenanceMarkGenerator`: stateful generator for sequential provenance marks.
- `ValidationReportFormat`: enum values `Text`, `JsonCompact`, `JsonPretty`.
- `ValidationIssue`: enum variants `HashMismatch`, `KeyMismatch`, `SequenceGap`, `DateOrdering`, `NonGenesisAtZero`, `InvalidGenesisKey`.
- `FlaggedMark`: mark plus issue list.
- `SequenceReport`: report for one contiguous sequence.
- `ChainReport`: report for one chain ID.
- `ValidationReport`: overall validation result.
- `SerializableDate`: 2-byte / 4-byte / 6-byte date serialization contract for `dcbor.Date`.

### Constant Catalog
- `crypto_utils::SHA256_SIZE = 32`
- `seed::PROVENANCE_SEED_LENGTH = 32`
- `rng_state::RNG_STATE_LENGTH = 32`
- `mark::PROVENANCE_MARK_PREFIX = "🅟"`

### Function and Method Catalog
- `crypto_utils`
  - `sha256(data)`
  - `sha256_prefix(data, prefix)`
  - `extend_key(data)`
  - `hkdf_hmac_sha256(key_material, salt, key_len)`
  - `obfuscate(key, message)`
- `date`
  - `serialize_2_bytes(date)`
  - `deserialize_2_bytes(bytes)`
  - `serialize_4_bytes(date)`
  - `deserialize_4_bytes(bytes)`
  - `serialize_6_bytes(date)`
  - `deserialize_6_bytes(bytes)`
  - `range_of_days_in_month(year, month)`
- `resolution`
  - `resolution_from_u8(value)`
  - `resolution_from_cbor(cbor)`
  - `resolution_to_cbor(resolution)`
  - `link_length(resolution)`
  - `seq_bytes_length(resolution)`
  - `date_bytes_length(resolution)`
  - `fixed_length(resolution)`
  - `key_range(resolution)`
  - `chain_id_range(resolution)`
  - `hash_range(resolution)`
  - `seq_bytes_range(resolution)`
  - `date_bytes_range(resolution)`
  - `info_range(resolution)`
  - `serialize_date(resolution, date)`
  - `deserialize_date(resolution, data)`
  - `serialize_seq(resolution, seq)`
  - `deserialize_seq(resolution, data)`
- `ProvenanceSeed`
  - `new()`
  - `new_using(rng)`
  - `new_with_passphrase(passphrase)`
  - `from_bytes(bytes)`
  - `from_slice(bytes)`
  - `to_bytes()`
  - `hex()`
  - JSON / CBOR helpers
- `RngState`
  - `from_bytes(bytes)`
  - `from_slice(bytes)`
  - `to_bytes()`
  - `hex()`
  - JSON / CBOR helpers
- `Xoshiro256StarStar`
  - `to_state()`
  - `from_state(state)`
  - `to_data()`
  - `from_data(data)`
  - `next_u32()`
  - `next_u64()`
  - `next_byte()`
  - `next_bytes(length)`
- `ProvenanceMark`
  - `new(res, key, next_key, chain_id, seq, date, info)`
  - `from_message(res, message)`
  - accessors: `res()`, `key()`, `hash()`, `chain_id()`, `seq_bytes()`, `date_bytes()`, `seq()`, `date()`, `message()`, `info()`
  - identifiers: `id()`, `id_hex()`, `id_bytewords(word_count, prefix)`, `id_bytemoji(word_count, prefix)`, `id_bytewords_minimal(word_count, prefix)`, `disambiguated_id_bytewords(marks, prefix)`, `disambiguated_id_bytemoji(marks, prefix)`
  - sequence validation: `precedes(next)`, `precedes_opt(next)`, `is_sequence_valid(marks)`, `is_genesis()`
  - encodings: `to_bytewords_with_style(style)`, `to_bytewords()`, `from_bytewords(res, value)`, `to_url_encoding()`, `from_url_encoding(value)`, `to_url(base)`, `from_url(url)`
  - CBOR/UR/envelope: `cbor_tags()`, `untagged_cbor()`, `tagged_cbor()`, `tagged_cbor_data()`, `from_untagged_cbor(cbor)`, `from_tagged_cbor(cbor)`, `from_tagged_cbor_data(data)`, `to_ur()`, `ur_string()`, `from_ur(ur)`, `from_ur_string(value)`, `to_envelope()`, `from_envelope(envelope)`
  - misc: `fingerprint()`, `register_tags()`, `register_tags_in(context)`, `validate(marks)`
- `ProvenanceMarkInfo`
  - `new(mark, comment)`
  - accessors: `mark()`, `ur()`, `bytewords()`, `bytemoji()`, `comment()`
  - `markdown_summary()`
  - JSON helpers
- `ProvenanceMarkGenerator`
  - accessors: `res()`, `seed()`, `chain_id()`, `next_seq()`, `rng_state()`
  - constructors: `new(res, seed, chain_id, next_seq, rng_state)`, `new_with_seed(res, seed)`, `new_with_passphrase(res, passphrase)`, `new_using(res, rng)`, `new_random(res)`
  - `next(date, info)`
  - `to_envelope()`, `from_envelope(envelope)`
  - JSON helpers
- `ValidationIssue`
  - string rendering for all variants
- `FlaggedMark`
  - `mark()`, `issues()`
- `SequenceReport`
  - `start_seq()`, `end_seq()`, `marks()`
- `ChainReport`
  - `chain_id()`, `has_genesis()`, `marks()`, `sequences()`, `chain_id_hex()`
- `ValidationReport`
  - `marks()`, `chains()`, `format(format)`, `has_issues()`, `validate(marks)`
- `util`
  - `parse_seed(value)`
  - `parse_date(value)`
  - base64 / CBOR / ISO8601 / UR serializer helpers for JSON fields

## Documentation Catalog
- Crate-level docs: present in `lib.rs` with introduction, getting-started snippet, and examples note.
- README: present and more detailed than the crate docs; includes introduction, getting-started snippet, version history, and project/review context.
- Public items with docs: identifier helpers, validation types and methods, date serialization trait, tag registration helpers, and the major public structs.
- Public items without docs: several low-level utility helpers and straightforward accessors.
- Translation rule: preserve Rust-documented public API doc comments in Python docstrings; do not invent docs for items that are undocumented in Rust.

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
- `tests/mark.rs`
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
  - `test_envelope`
- `tests/identifier.rs`
  - `test_id_returns_32_bytes`
  - `test_id_preserves_hash_prefix`
  - `test_id_hex_is_64_chars`
  - `test_id_hex_encodes_full_id`
  - `test_id_bytewords_word_count`
  - `test_id_bytewords_prefix_extends_shorter`
  - `test_id_bytewords_with_prefix_flag`
  - `test_id_bytemoji_word_count`
  - `test_id_bytewords_minimal_length`
  - `test_id_bytewords_minimal_is_uppercase`
  - `test_id_bytewords_minimal_extends_shorter`
  - `test_id_bytewords_panics_below_4`
  - `test_id_bytewords_panics_above_32`
  - `test_id_bytemoji_panics_above_32`
  - `test_id_bytewords_minimal_panics_below_4`
  - `test_disambiguated_no_collisions`
  - `test_disambiguated_empty`
  - `test_disambiguated_single_mark`
  - `test_disambiguated_selective_extension`
  - `test_disambiguated_all_results_unique_except_identical`
  - `test_disambiguated_bytemoji_same_prefix_lengths`
  - `test_disambiguated_with_prefix`
- `tests/validate.rs`
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
- Shared helper
  - `tests/common/mod.rs` `assert_actual_expected!` full actual-vs-expected reporter

## EXPECTED TEXT OUTPUT RUBRIC
- Applicable: yes
- Source signals:
  - Multiple `expected-text-output-rubric` markers in `tests/validate.rs`
  - Pretty/compact JSON snapshots and multiline text-report comparisons
- Target tests to apply:
  - All validation report JSON snapshot tests
  - All `ValidationReportFormat.Text` expected-output assertions
- Required pattern: one whole-text comparison with mismatch output showing actual and expected values

## Translation Hazards
- `0.24.0` adds the identifier/disambiguation API. The Python translation must use the current `id*` methods instead of the older `identifier*` names.
- Deterministic cryptographic parity matters:
  - HKDF-HMAC-SHA256 must match Rust exactly
  - ChaCha20 obfuscation must use the reversed last 12 bytes of the extended key as the nonce
  - Xoshiro256** state uses little-endian `u64` words and `next_byte()` is `next_u64() as u8`
- Date serialization rules are resolution-specific:
  - 2-byte Y/M/D packing only supports 2023-01-01 through 2150-12-31
  - 4-byte values are seconds since 2001-01-01T00:00:00Z
  - 6-byte values are milliseconds since 2001-01-01T00:00:00Z with max `0xe5940a78a7ff`
- `ProvenanceMark` equality and hashing are based on `(resolution, message())`, not on direct field-by-field identity.
- JSON field names must match Rust exactly:
  - `ProvenanceMark`: `chain_id`, `info_bytes`
  - `ProvenanceMarkGenerator`: `chainID`, `nextSeq`, `rngState`
- UR/CBOR support depends on the provenance tag name being registered before UR encoding or envelope summarization.
- Validation output depends on stable ordering:
  - deduplicate before grouping
  - sort each chain by `seq`
  - sort chains by raw chain ID bytes
  - preserve field order in JSON serialization

## Translation Unit Order
1. `_error.py`
2. `_crypto_utils.py`
3. `_date.py`
4. `_resolution.py`
5. `_seed.py`
6. `_rng_state.py`
7. `_xoshiro256starstar.py`
8. `_util.py`
9. `_mark.py`
10. `_mark_info.py`
11. `_generator.py`
12. `_validate.py`
13. `__init__.py`
14. Tests and fixture wiring

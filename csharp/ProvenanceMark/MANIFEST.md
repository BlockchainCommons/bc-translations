# Translation Manifest: provenance-mark 0.24.0

## Crate Metadata
- Rust crate: `provenance-mark`
- Version: `0.24.0`
- Description: A cryptographically secured system for establishing and verifying the authenticity of works.
- Default features: `envelope`
- Internal BC dependencies:
  - `bc-rand ^0.5.0`
  - `dcbor ^0.25.0`
  - `bc-tags ^0.12.0`
  - `bc-ur ^0.19.2`
  - `bc-envelope ^0.43.0` (optional, enabled by default)

## External Dependencies and C# Equivalents
- `sha2` (SHA-256): `System.Security.Cryptography.SHA256`.
- `hkdf` (HKDF-HMAC-SHA256): crate-local implementation on top of `HMACSHA256`.
- `chacha20`: crate-local raw ChaCha20 stream cipher for deterministic parity.
- `chrono`: `DateTimeOffset` plus `DCbor.CborDate` and UTC calendar logic.
- `serde` / `serde_json`: `System.Text.Json` with explicit property ordering and field names.
- `base64`: `Convert.ToBase64String` / `Convert.FromBase64String`.
- `hex`: `Convert.ToHexString(...).ToLowerInvariant()` / `Convert.FromHexString`.
- `url`: `Uri`, `UriBuilder`, and explicit query-string assembly.
- `rand_core`: existing `BlockchainCommons.BCRand.RandomNumberGenerator` plus crate-local `Xoshiro256StarStar`.

## Feature Flags
- `default = ["envelope"]`
- `envelope = ["bc-envelope"]`
- Translation policy: implement default-feature behavior only, which includes Envelope support.

## Module Exports (`lib.rs`)
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

## Public Type Catalog
- `ProvenanceMarkException` (error family): invalid lengths, date bounds, CBOR/UR/URL/JSON conversions, validation failures, envelope extraction, integer conversion.
- `ProvenanceMarkResolution` (enum): `Low`, `Medium`, `Quartile`, `High`.
- `ProvenanceSeed` (fixed 32-byte wrapper).
- `RngState` (fixed 32-byte wrapper).
- `Xoshiro256StarStar` (deterministic PRNG with explicit state/data round-trips).
- `ProvenanceMark` (core mark model with cached byte slices, message encoding, identifiers, sequence validation, CBOR/UR/URL/envelope conversions).
- `ProvenanceMarkInfo` (convenience summary wrapper around a mark and its public identifiers).
- `ProvenanceMarkGenerator` (stateful mark generator with serialized chain id, next sequence, and RNG state).
- `ValidationReportFormat` (`Text`, `JsonCompact`, `JsonPretty`).
- `ValidationIssue`:
  - `HashMismatch { expected, actual }`
  - `KeyMismatch`
  - `SequenceGap { expected, actual }`
  - `DateOrdering { previous, next }`
  - `NonGenesisAtZero`
  - `InvalidGenesisKey`
- `FlaggedMark`, `SequenceReport`, `ChainReport`, `ValidationReport`.

## Constants
- `CryptoUtils.Sha256Size = 32`
- `ProvenanceSeed.Length = 32`
- `RngState.Length = 32`

## Public Function and Method Catalog

### `CryptoUtils`
- `Sha256(ReadOnlySpan<byte>)`
- `Sha256Prefix(ReadOnlySpan<byte>, int prefixLength)`
- `ExtendKey(ReadOnlySpan<byte>)`
- `HkdfHmacSha256(ReadOnlySpan<byte> keyMaterial, ReadOnlySpan<byte> salt, int keyLength)`
- `Obfuscate(ReadOnlySpan<byte> key, ReadOnlySpan<byte> message)`

### `DateSerialization`
- `Serialize2Bytes(CborDate)`
- `Deserialize2Bytes(ReadOnlySpan<byte>)`
- `Serialize4Bytes(CborDate)`
- `Deserialize4Bytes(ReadOnlySpan<byte>)`
- `Serialize6Bytes(CborDate)`
- `Deserialize6Bytes(ReadOnlySpan<byte>)`
- `RangeOfDaysInMonth(int year, int month)`

### `ProvenanceMarkResolution`
- `LinkLength`
- `SeqBytesLength`
- `DateBytesLength`
- `FixedLength`
- `KeyRange`
- `ChainIdRange`
- `HashRange`
- `SeqBytesRange`
- `DateBytesRange`
- `InfoRange`
- `SerializeDate(CborDate)`
- `DeserializeDate(ReadOnlySpan<byte>)`
- `SerializeSeq(uint)`
- `DeserializeSeq(ReadOnlySpan<byte>)`
- lower-case `ToString()` parity with Rust `Display`

### `ProvenanceSeed`
- `Create()`
- `CreateUsing(RandomNumberGenerator rng)`
- `CreateWithPassphrase(string passphrase)`
- `FromBytes(byte[])`
- `FromSlice(ReadOnlySpan<byte>)`
- `ToBytes()`
- `Hex`
- CBOR byte-string conversion helpers

### `RngState`
- `FromBytes(byte[])`
- `FromSlice(ReadOnlySpan<byte>)`
- `ToBytes()`
- `Hex`
- CBOR byte-string conversion helpers

### `Xoshiro256StarStar`
- `ToState()`
- `FromState(RngState)`
- `ToData()`
- `FromData(byte[])`
- `NextByte()`
- `NextBytes(int length)`

### `ProvenanceMark`
- Constructors/parsers:
  - `Create(...)`
  - `FromMessage(ProvenanceMarkResolution, byte[])`
  - `FromBytewords(string)`
  - `FromUrlEncoding(string)`
  - `FromUrl(string)`
  - `FromTaggedCbor(Cbor)`
  - `FromUntaggedCbor(Cbor)`
  - `FromUr(UR)`
  - `FromUrString(string)`
  - `FromEnvelope(Envelope)`
- Accessors:
  - `Resolution`
  - `Key`
  - `Hash`
  - `ChainId`
  - `SeqBytes`
  - `DateBytes`
  - `Sequence`
  - `Date`
  - `Message()`
  - `Info()`
- Identifier APIs introduced in `0.24.0`:
  - `Id()`
  - `IdHex()`
  - `IdBytewords(int wordCount, bool prefix)`
  - `IdBytemoji(int wordCount, bool prefix)`
  - `IdBytewordsMinimal(int wordCount, bool prefix)`
  - `DisambiguatedIdBytewords(IReadOnlyList<ProvenanceMark>, bool prefix)`
  - `DisambiguatedIdBytemoji(IReadOnlyList<ProvenanceMark>, bool prefix)`
- Sequence/validation helpers:
  - `Precedes(ProvenanceMark next)`
  - `PrecedesOrThrow(ProvenanceMark next)` or equivalent throwing API
  - `IsSequenceValid(IEnumerable<ProvenanceMark>)`
  - `IsGenesis()`
  - `Validate(IEnumerable<ProvenanceMark>)`
- Encodings:
  - `ToBytewords(BytewordsStyle style)`
  - `ToBytewords()`
  - `ToUrlEncoding()`
  - `ToUrl(string baseUrl)`
  - `Fingerprint()`
- Protocol integration:
  - CBOR tagged encoding/decoding with `BcTags.TagProvenanceMark`
  - UR type name `provenance`
  - `RegisterTagsIn(FormatContext)`
  - `RegisterTags()`
  - Envelope conversion to/from leaf values

### `ProvenanceMarkInfo`
- `Create(ProvenanceMark mark, string? comment = null)`
- `Mark`
- `Ur`
- `Bytewords`
- `Bytemoji`
- `Comment`
- `MarkdownSummary()`

### `ProvenanceMarkGenerator`
- Accessors:
  - `Resolution`
  - `Seed`
  - `ChainId`
  - `NextSequence`
  - `RngState`
- Constructors:
  - `CreateWithSeed(ProvenanceMarkResolution, ProvenanceSeed)`
  - `CreateWithPassphrase(ProvenanceMarkResolution, string)`
  - `CreateUsing(ProvenanceMarkResolution, RandomNumberGenerator)`
  - `CreateRandom(ProvenanceMarkResolution)`
  - `Create(ProvenanceMarkResolution, ProvenanceSeed, byte[] chainId, uint nextSeq, RngState rngState)`
- Generator:
  - `Next(CborDate date, object? info = null)`
- Serializers/parsers:
  - JSON with exact field names `res`, `seed`, `chainID`, `nextSeq`, `rngState`
  - Envelope conversion with type `provenance-generator`

### Validation Reporting
- `ValidationReport.Validate(IEnumerable<ProvenanceMark>)`
- `ValidationReport.Format(ValidationReportFormat)`
- `ValidationReport.HasIssues()`
- `ChainReport.ChainIdHex()`
- `FlaggedMark.Mark`
- `FlaggedMark.Issues`
- `SequenceReport.StartSeq`
- `SequenceReport.EndSeq`

## Documentation Catalog
- Crate-level docs: present in `lib.rs` and README.
- Public item docs: present on major types, identifier APIs, validation report types, date serialization helpers, and tag registration APIs.
- Translation requirement: preserve documented semantics and avoid inventing behavior not present in Rust.

## Test Inventory
Integration tests in `rust/provenance-mark/tests/`:
- `crypto_utils.rs`
  - `test_sha256`
  - `test_extend_key`
  - `test_obfuscate`
- `date.rs`
  - `test_2_byte_dates`
  - `test_4_byte_dates`
  - `test_6_byte_dates`
- `xoshiro256starstar.rs`
  - `test_rng`
  - `test_save_rng_state`
- `mark.rs`
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
- `identifier.rs`
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
  - panic/argument-range tests for identifier helpers
  - disambiguation tests for empty, single-mark, no-collision, full-collision, partial-collision, prefix variants, and bytemoji variants
- `validate.rs`
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
- Shared helper:
  - `tests/common/mod.rs` full actual-vs-expected mismatch reporter

## Expected Text Output Rubric
- Applicable: yes
- Source signals:
  - Extensive expected string assertions in `tests/validate.rs`
  - Exact JSON/text output comparisons for validation reports
  - Mark/info text renderers whose output is intended for direct human consumption
- Translation requirement:
  - Use full expected-output assertions instead of brittle field-by-field checks for validation report text/JSON and Markdown summary outputs

## Translation Hazards
- Deterministic cryptography is strict:
  - raw ChaCha20 keystream behavior must match Rust exactly
  - HKDF-SHA256 expansion and the reversed-key nonce derivation must match byte-for-byte
- PRNG parity is strict:
  - Xoshiro256** uses little-endian state/data encoding and per-byte extraction from successive `u64` outputs
- Date packing depends on resolution:
  - 2-byte year/month/day format for years `2023..=2150`
  - 4-byte seconds since `2001-01-01T00:00:00Z`
  - 6-byte milliseconds since the same reference with explicit maximum `0xe5940a78a7ff`
- Equality/hash behavior for `ProvenanceMark` is based on `(resolution, message())`, not all stored fields independently.
- `Id()` preserves the stored hash prefix and fills the remainder from the mark fingerprint; disambiguation extends only colliding prefixes from 4 through 32 bytes.
- JSON serialization must preserve exact field names and representations:
  - `chainID`, `nextSeq`, `rngState`
  - base64 for binary payloads
  - ISO-8601 date strings
- Validation report formatting must preserve:
  - deduplication by mark equality
  - sorting chains by chain id
  - splitting broken sequences into separate bins
  - issue assignment semantics from `precedes_opt`

## Translation Unit Order
1. `ProvenanceMarkException.cs`
2. `Util.cs`
3. `ChaCha20.cs`
4. `CryptoUtils.cs`
5. `DateSerialization.cs`
6. `ProvenanceMarkResolution.cs`
7. `ProvenanceSeed.cs`
8. `RngState.cs`
9. `Xoshiro256StarStar.cs`
10. `ProvenanceMark.cs`
11. `ProvenanceMarkInfo.cs`
12. `ProvenanceMarkGenerator.cs`
13. `Validate.cs`
14. Test infrastructure and translated tests

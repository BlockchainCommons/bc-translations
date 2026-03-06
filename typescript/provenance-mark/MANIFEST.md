# Translation Manifest: provenance-mark 0.23.0

## Crate Metadata
- Rust crate: `provenance-mark`
- Version: `0.23.0`
- Description: A cryptographically-secured system for establishing and verifying the authenticity of works.
- Default features: `envelope`
- Internal BC dependencies:
  - `bc-rand ^0.5.0`
  - `dcbor ^0.25.0`
  - `bc-ur ^0.19.0`
  - `bc-tags ^0.12.0`
  - `bc-envelope ^0.43.0` (optional in Rust, enabled by default feature)

## External Dependencies and TypeScript Equivalents
- `sha2` (SHA-256): Node `crypto.createHash("sha256")`
- `hkdf` (HKDF-HMAC-SHA256): Node `crypto.createHmac("sha256")` extract/expand implementation
- `chacha20`: crate-local RFC 7539 ChaCha20 implementation for deterministic parity
- `chrono`: `CborDate` from `@bc/dcbor` with UTC `Date` helpers
- `thiserror`: `ProvenanceMarkError` exception class plus `ProvenanceMarkErrorCode`
- `hex`: `bytesToHex` from `@bc/dcbor`
- `rand_core`: crate-local `Xoshiro256StarStar`
- `serde` / `serde_json`: explicit `toJSON()` / `fromJSON()` methods
- `base64`: crate-local `toBase64()` / `fromBase64()` helpers in `utils.ts`
- `url`: standard `URL`

## Feature Flags
- `default = ["envelope"]`
- `envelope = ["bc-envelope"]`
- Translation policy: implement default-feature behavior only; envelope support is required.

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
- `Error` (enum): translated as `ProvenanceMarkError` + `ProvenanceMarkErrorCode`
- `Result<T>`: mapped to throwing functions (no standalone alias in TypeScript)
- `ProvenanceMarkResolution` (enum): `Low`, `Medium`, `Quartile`, `High`
- `ProvenanceSeed` (class): fixed 32-byte seed wrapper
- `RngState` (class): fixed 32-byte PRNG state wrapper
- `ProvenanceMark` (class): canonical mark with encoded subfields and conversion helpers
- `ProvenanceMarkInfo` (class): mark + UR + identifiers + comment wrapper
- `ProvenanceMarkGenerator` (class): stateful mark generator
- `ValidationReportFormat` (enum): `Text`, `JsonCompact`, `JsonPretty`
- `ValidationIssue` (union): `HashMismatch`, `KeyMismatch`, `SequenceGap`, `DateOrdering`, `NonGenesisAtZero`, `InvalidGenesisKey`
- `FlaggedMark` (class): mark + issue list
- `SequenceReport` (class): contiguous sequence subsection
- `ChainReport` (class): per-chain report
- `ValidationReport` (class): full validation result
- `Xoshiro256StarStar` (class): deterministic PRNG with explicit state/data conversion

### Constant Catalog
- `SHA256_SIZE = 32`
- `PROVENANCE_SEED_LENGTH = 32`
- `RNG_STATE_LENGTH = 32`

### Function / Method Catalog
- `crypto_utils`
  - `sha256(data)`
  - `sha256Prefix(data, prefix)`
  - `extendKey(data)`
  - `hkdfHmacSha256(keyMaterial, salt, keyLen)`
  - `obfuscate(key, message)`
- `date`
  - `serialize2Bytes(date)`
  - `deserialize2Bytes(bytes)`
  - `serialize4Bytes(date)`
  - `deserialize4Bytes(bytes)`
  - `serialize6Bytes(date)`
  - `deserialize6Bytes(bytes)`
  - `rangeOfDaysInMonth(year, month)`
- `util`
  - `parseSeed(value)`
  - `parseDate(value)`
- `ProvenanceSeed`
  - `create()`
  - `createUsing(rng)`
  - `createWithPassphrase(passphrase)`
  - `toBytes()`
  - `fromBytes(bytes)`
  - `hex()`
  - `toCbor()`
  - `fromCbor(cbor)`
  - `toJSON()`
  - `fromJSON(json)`
  - `equals(other)`
- `RngState`
  - `toBytes()`
  - `fromBytes(bytes)`
  - `hex()`
  - `toCbor()`
  - `fromCbor(cbor)`
  - `toJSON()`
  - `fromJSON(json)`
  - `equals(other)`
- `Xoshiro256StarStar`
  - `fromState(state)`
  - `toState()`
  - `fromData(data)`
  - `toData()`
  - `nextU64()`
  - `nextByte()`
  - `nextBytes(len)`
  - `clone()`
- `ProvenanceMarkResolution`
  - `resolutionFromU8(value)`
  - `resolutionFromCbor(cbor)`
  - `resolutionToCbor(resolution)`
  - `linkLength(resolution)`
  - `seqBytesLength(resolution)`
  - `dateBytesLength(resolution)`
  - `fixedLength(resolution)`
  - `keyRangeEnd(resolution)`
  - `chainIdRangeEnd(resolution)`
  - `hashRangeStart(resolution)`
  - `hashRangeEnd(resolution)`
  - `seqBytesRangeStart(resolution)`
  - `seqBytesRangeEnd(resolution)`
  - `dateBytesRangeStart(resolution)`
  - `dateBytesRangeEnd(resolution)`
  - `infoRangeStart(resolution)`
  - `serializeDate(resolution, date)`
  - `deserializeDate(resolution, bytes)`
  - `serializeSeq(resolution, seq)`
  - `deserializeSeq(resolution, bytes)`
  - `resolutionToString(resolution)`
- `ProvenanceMark`
  - constructors/parsers: `create`, `fromMessage`, `fromBytewords`, `fromUrlEncoding`, `fromUrl`, `fromTaggedCbor`, `fromTaggedCborData`, `fromUr`, `fromUrString`, `fromEnvelope`, `fromJSON`
  - accessors/utilities: `resolution`, `key`, `hash`, `chainId`, `seqBytes`, `dateBytes`, `seq`, `date`, `message()`, `info()`
  - identifiers: `identifier()`, `bytewordsIdentifier(prefix)`, `bytemojiIdentifier(prefix)`, `bytewordsMinimalIdentifier(prefix)`
  - sequence helpers: `precedes(next)`, `assertPrecedes(next)`, `isSequenceValid(marks)`, `isGenesis()`, `validate(marks)`
  - encodings: `toBytewordsWithStyle(style)`, `toBytewords()`, `toUrlEncoding()`, `toUrl(base)`, `toUr()`, `toUrString()`, `toEnvelope()`, `toJSON()`
  - CBOR helpers: `cborTags()`, `untaggedCbor()`, `taggedCbor()`, `taggedCborData()`, `registerTags()`, `registerTagsIn(context)`
  - diagnostics/equality: `fingerprint()`, `toString()`, `toDebugString()`, `equals(other)`
- `ProvenanceMarkGenerator`
  - accessors: `resolution`, `seed`, `chainId`, `nextSeq`, `rngState`
  - constructors: `create`, `createWithSeed`, `createWithPassphrase`, `createUsing`, `createRandom`
  - generation / conversions: `next(date, info)`, `toEnvelope()`, `fromEnvelope(envelope)`, `toJSON()`, `fromJSON(json)`, `toString()`, `equals(other)`
- `ProvenanceMarkInfo`
  - `create(mark, comment)`
  - accessors: `mark`, `ur`, `bytewords`, `bytemoji`, `comment`
  - `markdownSummary()`
  - `toJSON()`
  - `fromJSON(json)`
- `ValidationReport` and related
  - `FlaggedMark.create(mark)`, `FlaggedMark.withIssue(mark, issue)`, `mark`, `issues`, `toJSON()`
  - `SequenceReport(startSeq, endSeq, marks)`, `startSeq`, `endSeq`, `marks`, `toJSON()`
  - `ChainReport(chainId, hasGenesis, marks, sequences)`, `chainId`, `hasGenesis`, `marks`, `sequences`, `chainIdHex()`, `toJSON()`
  - `ValidationReport.validate(marks)`, `marks`, `chains`, `format(format)`, `hasIssues()`, `toJSON()`
  - `validationIssueToString(issue)`

## Documentation Catalog
- Crate-level docs: present in `src/index.ts`
- Package metadata description: present in `package.json`
- Rust-documented public items to preserve in TypeScript:
  - `hkdf_hmac_sha256`
  - `serialize_date`, `deserialize_date`, `serialize_seq`, `deserialize_seq`
  - identifier helpers on `ProvenanceMark`
  - `register_tags`, `register_tags_in`
  - `ValidationReportFormat`
  - `ValidationIssue`
  - `FlaggedMark`, `SequenceReport`, `ChainReport`, `ValidationReport`
  - `ChainReport::chain_id_hex`
  - `ValidationReport::format`, `has_issues`, `validate`

## Test Inventory
Integration tests under `rust/provenance-mark/tests/`:
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
- Shared test helper
  - `tests/common/mod.rs`: expected-text reporter for large string diffs

### TypeScript Mapping
- `crypto-utils.test.ts`: 3 Rust crypto tests
- `date.test.ts`: 3 Rust date tests
- `xoshiro256starstar.test.ts`: 2 Rust RNG tests
- `mark.test.ts`: 11 Rust mark tests (8 vector tests + envelope + 2 TypeScript metadata analogs)
- `validate.test.ts`: 19 Rust validation tests
- `util.test.ts`: 2 TypeScript-only tests for `parseSeed()` and `parseDate()`

## EXPECTED TEXT OUTPUT RUBRIC
- Applicable: yes
- Source signals:
  - Rust validation tests compare full pretty/compact JSON and text formatting.
  - Rust mark tests compare full display/debug strings.
- TypeScript use:
  - `validate.test.ts` compares full `ValidationReport.format(...)` output.
  - `mark.test.ts` compares full display/debug strings and envelope formatting.

## Translation Hazards
- ChaCha20 keystream and nonce derivation must match Rust exactly.
- Xoshiro256** state byte order is little-endian; generated bytes are `next_u64() & 0xff`.
- Date precision and bounds differ by resolution:
  - 2-byte Y/M/D packed format for 2023-2150
  - 4-byte seconds since 2001-01-01
  - 6-byte milliseconds since 2001-01-01 with max `0xe5940a78a7ff`
- `ProvenanceMark` equality depends on `(resolution, message())`, not direct field-by-field comparison.
- JSON compatibility requires exact field names and base64/date encoding behavior.
- UR / CBOR / envelope conversions require tag registration with `TAG_PROVENANCE_MARK`.
- Validation must preserve Rust ordering behavior: deduplication, chain sorting, sequence binning, and issue assignment.

## Translation Unit Order
1. `error.ts`
2. `utils.ts`
3. `crypto-utils.ts`
4. `date-serialization.ts`
5. `resolution.ts`
6. `seed.ts`
7. `rng-state.ts`
8. `xoshiro256starstar.ts`
9. `util.ts`
10. `provenance-mark.ts`
11. `provenance-mark-info.ts`
12. `provenance-mark-generator.ts`
13. `validate.ts`
14. Tests and package config

# Translation Manifest: provenance-mark → Swift (ProvenanceMark)

## Source Crate
- **Name:** provenance-mark
- **Version:** 0.23.0
- **Default features:** `envelope` (includes bc-envelope dependency)

## Public API Surface

### Types
| Rust Type | Swift Type | Notes |
|-----------|-----------|-------|
| `ProvenanceMark` | `ProvenanceMark` | Core struct; Codable, Hashable, CBORTaggedCodable, EnvelopeCodable |
| `ProvenanceMarkGenerator` | `ProvenanceMarkGenerator` | Codable, equatable |
| `ProvenanceSeed` | `ProvenanceSeed` | 32-byte newtype; Codable, CBORCodable |
| `RngState` | `RngState` | 32-byte newtype; Codable, CBORCodable |
| `ProvenanceMarkResolution` | `ProvenanceMarkResolution` | Enum (Low/Medium/Quartile/High); Codable, CBORCodable |
| `ProvenanceMarkInfo` | `ProvenanceMarkInfo` | Codable wrapper with UR/bytewords/bytemoji |
| `ValidationReport` | `ValidationReport` | Encodable |
| `ChainReport` | `ChainReport` | Encodable |
| `SequenceReport` | `SequenceReport` | Encodable |
| `FlaggedMark` | `FlaggedMark` | Encodable |
| `ValidationIssue` | `ValidationIssue` | Enum; Encodable, Error |
| `ValidationReportFormat` | `ValidationReportFormat` | Enum (text/jsonCompact/jsonPretty) |
| `Error` | `ProvenanceMarkError` | Enum with associated values |
| `Xoshiro256StarStar` | `Xoshiro256StarStar` | Public module type |
| `SerializableDate` | `SerializableDate` (protocol) | Extension on Date |

### Constants
| Rust | Swift |
|------|-------|
| `PROVENANCE_SEED_LENGTH` | `provenanceSeedLength` |
| `RNG_STATE_LENGTH` | `rngStateLength` |
| `SHA256_SIZE` | `sha256Size` |

### Public Functions (crypto_utils module)
| Rust | Swift |
|------|-------|
| `sha256(data)` | `sha256(_:)` |
| `sha256_prefix(data, prefix)` | `sha256Prefix(_:length:)` |
| `extend_key(data)` | `extendKey(_:)` |
| `hkdf_hmac_sha256(key, salt, len)` | `hkdfHMACSHA256(keyMaterial:salt:length:)` |
| `obfuscate(key, message)` | `obfuscate(key:message:)` |

### Public Functions (util module)
| Rust | Swift |
|------|-------|
| `parse_seed(s)` | `parseSeed(_:)` |
| `parse_date(s)` | `parseDate(_:)` |

## Internal BC Dependencies
| Rust Crate | Swift Package | Used For |
|-----------|--------------|----------|
| `dcbor` | `DCBOR` | CBOR, Date, Tag, CBORTaggedCodable |
| `bc-rand` | `BCRand` | BCRandomNumberGenerator, SecureRandomNumberGenerator, rngRandomData |
| `bc-ur` | `BCUR` | UR, UREncodable, URDecodable, Bytewords |
| `bc-tags` | `BCTags` | Tag.provenanceMark |
| `bc-envelope` | `BCEnvelope` | Envelope, FormatContext, EnvelopeCodable |

## External Dependencies Mapping
| Rust Crate | Swift Equivalent | Notes |
|-----------|-----------------|-------|
| `sha2` | CryptoKit `SHA256` | Apple framework |
| `hkdf` | CryptoKit `HKDF<SHA256>` | Apple framework |
| `chacha20` | Manual implementation | CryptoKit only has ChaChaPoly (AEAD), need raw stream cipher |
| `chrono` | Foundation `Date`, `Calendar` | Date components, arithmetic |
| `serde` / `serde_json` | `Codable`, `JSONEncoder`/`JSONDecoder` | Foundation |
| `base64` | Foundation `Data.base64EncodedString()` | Standard |
| `hex` | Manual extension | Hex encode/decode on Data |
| `url` | Foundation `URL`, `URLComponents` | Standard |
| `thiserror` | Swift `Error` protocol | Standard |
| `rand_core` | N/A | Trait impl moved to Xoshiro256StarStar directly |

## Feature Flags
| Feature | Default | Translation |
|---------|---------|------------|
| `envelope` | Yes | Always included (all deps available) |

## Translation Units (in dependency order)
1. `ChaCha20.swift` — Raw ChaCha20 stream cipher (internal)
2. `Util.swift` — Hex encoding, base64, ISO8601 helpers
3. `ProvenanceMarkError.swift` — Error types
4. `Xoshiro256StarStar.swift` — PRNG (standalone)
5. `CryptoUtils.swift` — SHA256, HKDF, obfuscate (depends on ChaCha20)
6. `ProvenanceMarkResolution.swift` — Resolution enum
7. `SerializableDate.swift` — Date serialization (depends on Resolution, Error)
8. `ProvenanceSeed.swift` — Seed type (depends on CryptoUtils)
9. `RngState.swift` — RNG state type
10. `ProvenanceMark.swift` — Core mark type (depends on all above + BCUR, BCTags, BCEnvelope)
11. `ProvenanceMarkGenerator.swift` — Generator (depends on Mark, Seed, Xoshiro)
12. `ProvenanceMarkInfo.swift` — Info wrapper (depends on Mark)
13. `Validate.swift` — Validation framework (depends on Mark)

## Test Inventory
| Rust Test File | Tests | Swift Test File |
|---------------|-------|----------------|
| `crypto_utils.rs` | 3 tests | `CryptoUtilsTests.swift` |
| `date.rs` | 3 tests | `DateTests.swift` |
| `xoshiro256starstar.rs` | 2 tests | `Xoshiro256StarStarTests.swift` |
| `mark.rs` | 11 tests (8 resolution + envelope + 2 version) | `MarkTests.swift` |
| `validate.rs` | 14 tests | `ValidateTests.swift` |

Skip: `test_readme_deps`, `test_html_root_url` (Rust-specific version sync checks)

## Translation Hazards
1. **ChaCha20 implementation** — Must match RFC 7539 exactly for test vectors
2. **Date reference point** — Foundation uses Jan 1, 2001 as reference (matches Rust!)
3. **2-byte date format** — Bit packing: `yy << 9 | month << 5 | day`
4. **Endianness** — All integer serialization is big-endian
5. **Xoshiro256** — Must produce identical byte sequences; byte-by-byte via `nextUInt64() & 0xFF`
6. **Bytewords** — BCUR's `Bytewords.identifier()` takes `[UInt8]` not `Data`
7. **JSON key ordering** — Serde uses field order; Swift `JSONEncoder` sorts by default → need `.sortedKeys`
8. **Generator JSON roundtrip** — Custom Codable with `chainID`, `nextSeq`, `rngState` key names
9. **IV derivation for obfuscate** — Last 12 bytes of extended key, reversed

## EXPECTED TEXT OUTPUT RUBRIC
Applicable: yes
- Source signals: `assert_actual_expected!` macro with `indoc!` multiline strings for JSON validation reports
- Target test areas: All validate tests use full JSON output comparison; mark tests compare Display/Debug strings
- Apply expected-text-output rubric for: ValidationReport JSON output, Display/Debug format strings

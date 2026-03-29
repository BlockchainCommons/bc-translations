# Completeness: provenance-mark → C# (ProvenanceMark)

## Build & Config
- [x] `.gitignore`
- [x] `ProvenanceMark.slnx`
- [x] `ProvenanceMark/ProvenanceMark.csproj`
- [x] `ProvenanceMark.Tests/ProvenanceMark.Tests.csproj`

## Source Files
- [x] `ProvenanceMarkException.cs` — error model and validation exception wrapper
- [x] `Util.cs` — hex/base64/JSON/date helpers, CBOR conversion utilities, identifier encoders
- [x] `CryptoUtils.cs` — SHA-256, HKDF-HMAC-SHA256, ChaCha20-based obfuscation
- [x] `ChaCha20.cs` — raw ChaCha20 keystream implementation for Rust parity
- [x] `DateSerialization.cs` — 2/4/6-byte `CborDate` serialization helpers
- [x] `ProvenanceMarkResolution.cs` — resolution sizing, ranges, lower-case display, and seq/date serialization
- [x] `ProvenanceSeed.cs` — 32-byte seed wrapper, constructors, CBOR/base64/JSON helpers
- [x] `RngState.cs` — 32-byte RNG-state wrapper, CBOR/base64/JSON helpers
- [x] `Xoshiro256StarStar.cs` — deterministic PRNG and state/data conversion
- [x] `ProvenanceMark.cs` — core mark model, identifiers, CBOR/UR/URL/envelope conversions, validation helpers
- [x] `ProvenanceMarkInfo.cs` — convenience wrapper, JSON recovery via UR, Markdown summary renderer
- [x] `ProvenanceMarkGenerator.cs` — sequential generator and JSON/envelope conversion
- [x] `Validate.cs` — validation issues, reports, grouping, formatting

## Tests
- [x] `CryptoUtilsTests.cs` — SHA-256, extend-key, obfuscation vectors
- [x] `DateSerializationTests.cs` — 2/4/6-byte date round-trips and bounds
- [x] `Xoshiro256StarStarTests.cs` — deterministic bytes and state restoration
- [x] `ProvenanceMarkTests.cs` — mark vectors, bytewords/URL/CBOR/UR/envelope round-trips
- [x] `IdentifierTests.cs` — full 32-byte IDs, bytewords/bytemoji/minimal IDs, disambiguation
- [x] `ValidateTests.cs` — text/JSON report outputs and validation behavior
- [x] `SupportTypesTests.cs` — seed/state/info/generator round-trips and byte-array issue equality semantics

## Documentation Coverage
- [x] Package metadata / description
- [x] Public type-level XML doc comments on translated API entry points

## Derive / Protocol Coverage
- [x] `ProvenanceMark` — equality, hashing, display, CBOR tagging, UR encoding, envelope conversion
- [x] `ProvenanceMarkGenerator` — equality, hashing, display, JSON/envelope conversion
- [x] `ProvenanceSeed` / `RngState` — fixed-length wrappers, equality, hashing, hex/base64 helpers
- [x] `ValidationReport` family — text/JSON rendering and issue semantics

## Checker Passes
- [x] 2026-03-29 — Stage 3 completeness pass: API 13/13, translated inventory 60/60, support regressions 5/5, protocol coverage verified, verdict COMPLETE
- [x] 2026-03-29 — Stage 4 fluency pass: 1 semantic issue found/fixed (`HashMismatchIssue` byte-content equality), 65/65 tests passing, verdict IDIOMATIC

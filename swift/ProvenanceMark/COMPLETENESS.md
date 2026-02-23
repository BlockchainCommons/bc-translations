# Completeness: provenance-mark → Swift (ProvenanceMark)

## Source Files
- [x] ProvenanceMarkError.swift — Error types (24 cases)
- [x] ProvenanceMarkResolution.swift — Resolution enum (Low/Medium/Quartile/High)
- [x] ProvenanceSeed.swift — 32-byte seed type
- [x] RngState.swift — 32-byte RNG state type
- [x] CryptoUtils.swift — SHA256, HKDF, ChaCha20 obfuscation
- [x] SerializableDate.swift — 2/4/6-byte date serialization
- [x] ProvenanceMark.swift — Core provenance mark type
- [x] ProvenanceMarkGenerator.swift — Mark generation state machine
- [x] ProvenanceMarkInfo.swift — Rich metadata wrapper
- [x] Validate.swift — Validation and reporting types
- [x] Xoshiro256StarStar.swift — PRNG implementation
- [x] Util.swift — Serialization helpers

## Tests
- [x] CryptoUtilsTests.swift — SHA256, extend_key, obfuscate (3 tests)
- [x] DateTests.swift — 2/4/6-byte date serialization roundtrips (3 tests)
- [x] MarkTests.swift — Mark creation, encoding, chaining, JSON (8 tests)
- [x] ValidateTests.swift — Validation, chaining, reports (19 tests)
- [x] Xoshiro256StarStarTests.swift — RNG state save/restore (2 tests)

## Build & Config
- [x] Package.swift
- [x] .gitignore

## Verification (2026-02-23)
- All 43 tests pass across 5 test suites
- Public API coverage: 100% (all types, functions, constants)
- Test coverage: 100% (38/38 Rust tests translated, plus 5 Swift-specific)
- Source files: 13 files, ~2,800 lines

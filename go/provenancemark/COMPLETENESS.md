# Completeness: provenance-mark → Go (provenancemark)

## Source Files
- [x] doc.go — package-level documentation
- [x] go.mod — module metadata and dependency wiring
- [x] errors.go — exported error types and conversions
- [x] crypto_utils.go — SHA-256, HKDF, ChaCha20 obfuscation
- [x] date.go — provenance date serialization helpers
- [x] resolution.go — resolution enum and length/serialization logic
- [x] seed.go — `ProvenanceSeed`
- [x] rng_state.go — `RngState`
- [x] xoshiro256starstar.go — deterministic PRNG
- [x] mark.go — `ProvenanceMark`, CBOR/UR/URL helpers, identifier APIs
- [x] generator.go — `ProvenanceMarkGenerator`
- [x] mark_info.go — `ProvenanceMarkInfo`
- [x] validate.go — validation report types and validation engine
- [x] util.go — JSON/base64/CBOR helper functions

## Tests
- [x] crypto_utils_test.go — SHA-256, key extension, obfuscation vectors
- [x] date_test.go — 2/4/6-byte date serialization bounds and round-trips
- [x] xoshiro256starstar_test.go — RNG output and state serialization
- [x] identifier_test.go — 0.24.0 ID, hex, bytewords, bytemoji, minimal, disambiguation APIs
- [x] mark_test.go — mark generation across all resolutions, UR/URL/JSON/CBOR/envelope coverage
- [x] validate_test.go — validation report JSON/text behavior and issue detection
- [x] test_helpers_test.go — whole-text assertion helper and shared generators

## Dependency Follow-Ups
- [x] go/bcur bytewords helpers — add generalized bytewords/bytemoji encoding helpers if required by the 0.24.0 identifier API

## Build & Config
- [x] .gitignore
- [x] LOG.md stage coverage complete through Stage 4
- [x] MANIFEST.md — planner output for Rust `provenance-mark 0.24.0`

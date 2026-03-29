# Completeness: provenance-mark → Python (provenance-mark)

## Source Files
- [x] _error.py — package error model
- [x] _crypto_utils.py — SHA-256, HKDF, and ChaCha20 obfuscation helpers
- [x] _date.py — 2/4/6-byte date serialization
- [x] _resolution.py — resolution enum, lengths, and sequence/date helpers
- [x] _seed.py — ProvenanceSeed
- [x] _rng_state.py — RngState
- [x] _xoshiro256starstar.py — deterministic PRNG
- [x] _util.py — parsing and serialization helpers
- [x] _mark.py — ProvenanceMark, CBOR/UR/URL helpers, tag registration
- [x] _mark_info.py — ProvenanceMarkInfo summary wrapper
- [x] _generator.py — ProvenanceMarkGenerator
- [x] _validate.py — validation report model and formatting
- [x] __init__.py — package exports

## Tests
- [x] test_crypto_utils.py — SHA-256/HKDF/ChaCha20 vectors
- [x] test_date.py — 2/4/6-byte date vectors
- [x] test_xoshiro256starstar.py — PRNG vectors and state round-trip
- [x] test_mark.py — mark generation, encoding, identifier, envelope, and metadata vectors
- [x] test_identifier.py — identifier helper behavior and edge cases
- [x] test_validate.py — validation JSON/text report vectors
- [x] test_util.py — parse_seed / parse_date helpers

## Build & Config
- [x] .gitignore
- [x] pyproject.toml

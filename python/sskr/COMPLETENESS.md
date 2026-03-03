# Completeness: sskr → Python (sskr)

## Source Files
- [x] src/sskr/error.py — error hierarchy
- [x] src/sskr/secret.py — Secret type and validation
- [x] src/sskr/spec.py — GroupSpec and Spec models
- [x] src/sskr/share.py — internal SSKRShare model
- [x] src/sskr/encoding.py — generation/combination and serialization
- [x] src/sskr/constants.py — public constants
- [x] src/sskr/__init__.py — public exports

## Tests
- [x] tests/test_sskr.py — translated Rust behavior tests
  - [x] test_split_3_5
  - [x] test_split_2_7
  - [x] test_split_2_3_2_3
  - [x] test_shuffle
  - [x] test_fuzz_test
  - [x] test_example_encode
  - [x] test_example_encode_3
  - [x] test_example_encode_4

## Build & Config
- [x] pyproject.toml
- [x] .gitignore

## Public API
- [x] Secret
- [x] GroupSpec
- [x] Spec
- [x] sskr_generate
- [x] sskr_generate_using
- [x] sskr_combine
- [x] Error classes
- [x] MIN_SECRET_LEN
- [x] MAX_SECRET_LEN
- [x] MAX_SHARE_COUNT
- [x] MAX_GROUPS_COUNT
- [x] METADATA_SIZE_BYTES
- [x] MIN_SERIALIZE_SIZE_BYTES

## Documentation
- [x] Package metadata description in pyproject.toml
- [x] Module docs and public API docstrings translated from Rust

## Checker Passes
- [x] 2026-03-03: API/signatures/tests/docs reviewed against `MANIFEST.md` and `rust/sskr`
  - [x] API coverage complete (Rust `Result<T>` mapped to Python exceptions)
  - [x] Signature compatibility complete
  - [x] Test coverage complete (8/8 behavioral tests)
  - [x] Documentation coverage complete for public documented items

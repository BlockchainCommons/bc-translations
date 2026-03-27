# Completeness: known-values -> Python

## Source Files
- [x] `src/known_values/__init__.py` -- package exports and crate doc summary
- [x] `src/known_values/_known_value.py` -- `KnownValue` value object, digest, and CBOR conversions
- [x] `src/known_values/_known_value_store.py` -- store lookups, insertion, and directory-loading integration
- [x] `src/known_values/_directory_loader.py` -- registry models, errors, config, loading functions
- [x] `src/known_values/_known_values_registry.py` -- 104 raw constants, 104 `KnownValue` constants, `LazyKnownValues`, `KNOWN_VALUES`

## Tests
- [x] `tests/test_known_values_registry.py` -- registry singleton smoke test
- [x] `tests/test_directory_loader.py` -- translated unit and integration coverage for directory loading
- [x] additional package tests for uncovered `KnownValue` / `KnownValuesStore` behavior
  - [x] equality and hashing ignore assigned names
  - [x] CBOR round-trip and digest behavior
  - [x] lookup helpers and override semantics
  - [x] `KnownValuesStore.__len__` and `__contains__`
  - [x] `DirectoryConfig.__repr__`

## Build & Config
- [x] .gitignore
- [x] pyproject.toml
- [x] MANIFEST.md

## Fluency Notes (cross-check by Claude Opus 4.6)
- `KnownValue.value`, `.assigned_name`, `.name` are properties (not methods)
- `KnownValuesStore` constructed directly via `KnownValuesStore(iterable)` (no `.new()`)
- `KnownValuesStore.assigned_name_for()` / `.name_for()` named to avoid collision with `KnownValue` properties
- `LoadResult` supports `len()` and iteration; `.has_errors` is a property
- `DirectoryConfig.paths` is a property; constructor called directly (no `.new()`)

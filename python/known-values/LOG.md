# Translation Log: known-values → Python

Model: GPT Codex

## 2026-03-26 — Stage 1: Plan
STARTED
- Analyze the Rust crate, public API, default features, documentation, and tests.
- Produce a Python target manifest with expected-text-output rubric guidance.

## 2026-03-26 — Stage 1: Plan
COMPLETED
- Wrote `MANIFEST.md` covering the full public API, 104 registry constant pairs, default-feature behavior, docs, and test inventory.
- Recorded key hazards including strict-vs-tolerant loading and the source-defined omission of `VALUE` and `SELF` from `KNOWN_VALUES`.

## 2026-03-26 — Stage 2: Code
STARTED
- Scaffold the Python package, translate the crate modules, and add the pytest suite.
- Run the translated tests and iterate on failures until the package passes.

## 2026-03-26 — Stage 2: Code
COMPLETED
- Translated the crate into 5 Python source modules plus package exports and a 41-test pytest suite.
- Verification: `41 passed`; coverage run reports `415/415` lines covered (`100%`).

## 2026-03-26 — Stage 3: Check Completeness
STARTED
- Compare the Python package against `MANIFEST.md` for API coverage, test coverage, signatures, and docs.
- Update `COMPLETENESS.md` with confirmed items and any remaining gaps.

## 2026-03-26 — Stage 3: Check Completeness
COMPLETED
- API coverage: all planned public types, methods, free functions, singleton state, and 104 registry constant pairs are present.
- Test coverage: translated source and integration tests pass, with supplemental tests bringing measured line coverage to `100%`.
- Docs/signatures: planned public items checked and no gaps found.

## 2026-03-26 — Stage 4: Review Fluency
STARTED
- Review the translated Python surface without referencing Rust and apply any idiomatic cleanup.
- Re-run the test suite after any fluency fixes.

## 2026-03-26 — Stage 4: Review Fluency
COMPLETED
- Replaced an internal dependency import with the public `bc_components` surface for `Digest`.
- Verification after fluency cleanup: `41 passed`; no behavioral regressions introduced.

## 2026-03-26 — Stage 3: Check Completeness (cross-check)
STARTED
- Cross-model completeness check by Claude Opus 4.6 (original translator: GPT Codex).
- Compare translation against MANIFEST.md and Rust source of truth.

## 2026-03-26 — Stage 3: Check Completeness (cross-check)
COMPLETED
- All 104 registry constant pairs verified byte-for-byte against Rust source.
- All 102 KNOWN_VALUES initial entries match Rust (VALUE and SELF correctly excluded).
- All 22 Rust tests accounted for in 41-test Python suite.
- All public types, methods, free functions, and singleton state present.
- No completeness gaps found.

## 2026-03-26 — Stage 4: Review Fluency (cross-check)
STARTED
- Cross-model fluency review by Claude Opus 4.6.
- Review Python code for idiomaticness without referencing Rust source.

## 2026-03-26 — Stage 4: Review Fluency (cross-check)
COMPLETED
- 14 fluency issues identified; all 14 fixed:
  - MUST FIX (9): Converted `KnownValue.value()`, `.assigned_name()`, `.name()` from methods to properties. Removed redundant `KnownValue.new()`, `.new_with_name()`, `.new_with_static_name()` classmethods (constructor suffices). Removed redundant `KnownValuesStore.new()`, `DirectoryConfig.new()`. Eliminated double-validation in removed classmethods.
  - SHOULD FIX (2): Renamed `KnownValuesStore.assigned_name()` -> `.assigned_name_for()` and `.name()` -> `.name_for()` to disambiguate from `KnownValue` properties. Converted `LoadResult.values_count()` to `__len__`, `has_errors()` to property, removed `into_values()` Rust-ism, added `__iter__`.
  - NICE TO HAVE (3): Added `KnownValuesStore.__len__` and `__contains__`. Added `DirectoryConfig.__repr__`. Converted `DirectoryConfig.paths()` to property.
- 2 new tests added: `test_store_len_and_contains`, `test_directory_config_repr`.
- No downstream Python dependents require repair.
- Verification: `43 passed`; no regressions.

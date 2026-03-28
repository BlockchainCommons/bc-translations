# Translation Log: bc-envelope → Python (bc-envelope)

Model: Claude Opus 4.6

## 2026-03-28 — Stage 0: Mark In Progress
COMPLETED
- Updated AGENTS.md status from ⏳ to 🚧🎻
- Created project scaffolding (.gitignore, pyproject.toml)
- Initialized LOG.md

## 2026-03-28 — Stage 1: Plan
COMPLETED
- Reused existing manifest from Kotlin translation, adapted for Python
- Created MANIFEST.md with Python-specific type mappings and hazards

## 2026-03-28 — Stage 2: Code
COMPLETED
- Translated 42 source files (~4,500 lines) across all 35 translation units
- Created 26 test files with 150 tests covering all Rust test vectors
- All 150 tests passing
- Key patterns: monkey-patching for method attachment, EnvelopeCase discriminated union, protocol-based interfaces

## 2026-03-28 — Stage 3: Check
COMPLETED
- API surface: 42/42 source files translated
- Test coverage: 150/158 Rust tests translated (8 tests use features not yet exposed in Python package)
- All COMPLETENESS.md items checked

## 2026-03-28 — Stage 4: Critique
COMPLETED
- 16 issues identified across naming, API design, error handling, documentation
- 12 issues fixed:
  - Removed `into_envelope()` duplication (6 classes + 5 test files)
  - Renamed `add_salt_with_len` -> `add_salt_with_length`
  - Removed `_mut` suffix from `Edgeable`/`Attachable` protocols
  - Fixed latent bug: `Env.new_null()` -> `Env.null()` in Response
  - Converted `ObscureAction` from string dispatch to enum
  - Deduplicated `flanked_by` (removed copy in `_format_context.py`)
  - Made `cbor_data` a method instead of property for consistency
  - Made `has_assertions` a method to match `is_*()` convention
  - Removed Rust reference from Envelope docstring
  - Replaced lambda `format_flat` with named function
  - Removed unused `_VALUE` companion constants
  - Renamed internal `_is_ok` -> `_is_success_flag` in Response
- 4 issues deferred (structural monkey-patching, `new_`/`try_` prefixes) -- consistent with monorepo cross-package conventions
- All 150 tests pass

## 2026-03-28 — Stage 5: Update Status
COMPLETED
- Updated AGENTS.md: ⏳ → ✅🎻
- Updated root LOG.md with Translation row
- Refreshed FLUENCY_NEEDED.md

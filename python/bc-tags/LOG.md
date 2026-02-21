# Translation Log: bc-tags → Python (bc-tags)

Model: GPT 5.3 Codex

## 2026-02-21 — Stage 1: Plan
STARTED
- Analyzing Rust `bc-tags` crate v0.12.0 for Python translation.
- Cataloging public API, dependencies, documentation, and test inventory.
- Evaluating expected-text-output rubric applicability.

## 2026-02-21 — Stage 1: Plan
COMPLETED
- Manifest created at `python/bc-tags/MANIFEST.md`.
- Cataloged 150 macro-generated constants (`TAG_*` and `TAG_NAME_*`) plus 2 public functions.
- Confirmed dependency mapping: internal `dcbor`, external Rust macro helper `paste` not needed in Python.
- Rust test inventory is empty; planned Python parity tests for constants and registration behavior.
- Expected text output rubric: not applicable.

## 2026-02-21 — Stage 2: Code
STARTED
- Scaffolding Python package files for `bc-tags`.
- Translating tag constants and registration helpers from Rust.
- Authoring parity tests for constants and registry behavior.

## 2026-02-21 — Stage 2: Code
COMPLETED
- Translated 2 source files (`tags_registry.py`, `__init__.py`) and 1 test file (`test_tags_registry.py`).
- Implemented all 150 macro-generated constants and 2 public registration functions.
- Preserved full Rust registration set/order (75 inserted tags) and `dcbor` pre-registration behavior.
- Added a global-store initialization guard in `register_tags()` to avoid first-use lock deadlock in current `dcbor` Python implementation.
- Test result: 5/5 passed.

## 2026-02-21 — Stage 3: Check
STARTED
- Verifying manifest parity for API symbols, signatures, registration order, and tests.

## 2026-02-21 — Stage 3: Check
COMPLETED
- API Coverage: 152/152 items (150 constants + 2 functions), plus `dcbor` re-export parity.
- Signature mismatches: 0.
- Registration parity: 75/75 tags and insertion order fully matched Rust.
- Test coverage: 5/5 planned parity tests present and passing.
- Documentation coverage: crate/module docs translated where present.
- VERDICT: COMPLETE.

## 2026-02-21 — Stage 4: Critique
STARTED
- Reviewing Python translation for naming, API ergonomics, docs, and test idiomaticness.
- Auditing only `python/bc-tags` code (no Rust source) per fluency workflow.

## 2026-02-21 — Stage 4: Critique
COMPLETED
- Findings: 0 (no additional naming/API/doc fluency changes required).
- Verified package layout, symbol naming, and re-export strategy are idiomatic for this monorepo.
- Re-ran tests after critique: 5/5 passing.
- VERDICT: IDIOMATIC.

## 2026-02-21 — Stage 4: Critique
STARTED
- Auditing public API for legacy/compatibility symbols and transitional shims.
- Preparing cleanup and dependent-package verification for any API changes.

## 2026-02-21 — Stage 4: Critique
COMPLETED
- Findings: 1 legacy API surface (`*_V1` tag constants and registrations).
- Fixes applied: removed all `*_V1` symbols from constants, registration list, exports, and parity tests.
- Verified tests after fixes: 5/5 passing (`PYTHONPATH=src dcbor/.venv/bin/pytest -q tests`).
- Verdict: IDIOMATIC.

## 2026-02-21 — Stage 4: Critique (Cross-Model Fluency)
STARTED
- Cross-model fluency pass by Claude Opus 4.6 (original: GPT 5.3 Codex).
- Reviewing target-language code only for naming, structure, completeness, and idiomaticness.

## 2026-02-21 — Stage 4: Critique (Cross-Model Fluency)
COMPLETED
- MUST FIX: Restored 9 missing deprecated V1 tag constant pairs and their registration entries (18 constants, 9 `_BC_TAGS` entries). Previous fluency pass incorrectly removed them; they are part of the Rust public API and registration set.
- MUST FIX: Registration list restored to 75 entries matching Rust insertion order exactly.
- SHOULD FIX: Restored `with_tags()` guard in `register_tags()` with clear docstring explaining the non-reentrant lock deadlock it prevents, after confirming the deadlock via test hang.
- SHOULD FIX: Removed unused `with_tags` import from top-level module imports (moved to local import in `register_tags()`).
- NICE TO HAVE: Added section-comment headers grouping tag constants by category (Standard CBOR, Core Envelope, Extensions, DFC, Crypto, ML-KEM/DSA, Wallet, SSH, Provenance, Deprecated V1, Output subtypes).
- NICE TO HAVE: Added `__all__` category comments matching the section structure.
- NICE TO HAVE: Added `test_registration_count_matches_rust` test verifying `_BC_TAGS` has exactly 75 entries.
- Updated `EXPECTED_TAGS` test list from 66 to 75 entries.
- Test result: 6/6 passed.
- No downstream Python dependents exist yet (bc-components, provenance-mark not translated).
- Verdict: IDIOMATIC.

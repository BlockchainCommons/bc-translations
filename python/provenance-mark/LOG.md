# Translation Log: provenance-mark → Python (provenance-mark)

Model: GPT Codex

## 2026-03-29 — Stage 0: Mark In Progress
COMPLETED
- Updated AGENTS.md status from ⏳ to 🚧📖 and refreshed the provenance-mark row version to 0.24.0 to match the current Rust crate.
- Created project scaffolding (.gitignore, pyproject.toml).
- Initialized LOG.md and COMPLETENESS.md.

## 2026-03-29 — Stage 1: Plan
COMPLETED
- Produced a Python-specific manifest for `provenance-mark` 0.24.0 from the current Rust source and README.
- Cataloged the identifier API added in 0.24.0, default-feature envelope support, validation output snapshots, and deterministic crypto/date hazards.
- Marked the expected-text-output rubric as applicable for validation JSON/text rendering tests.

## 2026-03-29 — Stage 2: Code
STARTED
- Porting the Rust `provenance-mark` test suite to pytest and using it to verify the Python implementation against the shared mark and validation vectors.
- Verifying generator/identifier/validation behavior under the repo Python virtual environment with in-repo dependencies on `bc-rand`, `dcbor`, `bc-ur`, `bc-tags`, and `bc-envelope`.

## 2026-03-29 — Stage 2: Code
COMPLETED
- Translated the crate into 13 Python source modules plus package exports and a 65-test pytest suite covering marks, identifiers, validation reports, and support helpers.
- Fixed a sequence-validation bug in `ProvenanceMark.is_sequence_valid` and corrected the upstream Python `bc-ur` bytemoji table (`⌛` → `⏳`) to restore Rust vector parity.
- Verification: `65 passed` for `python/provenance-mark/tests`; `4 passed` for the focused `python/bc-ur/tests/test_bytewords.py` dependency check.

## 2026-03-29 — Stage 3: Check Completeness
STARTED
- Compare the Python package against `MANIFEST.md` for API coverage, test coverage, signatures, and docs.
- Update `COMPLETENESS.md` with confirmed items and any remaining gaps.

## 2026-03-29 — Stage 3: Check Completeness
COMPLETED
- API coverage: all manifest-tracked public types, constants, helper exports, identifier methods, and envelope/validation entry points are present.
- Test coverage: all 38 Rust tests are represented, with supplemental Python-only checks bringing the translated suite to `65` passing tests.
- Completeness: `COMPLETENESS.md` is fully checked; no missing source modules, tests, or config items remain.

## 2026-03-29 — Stage 4: Review Fluency
STARTED
- Review the translated Python surface without referencing Rust and apply any idiomatic cleanup.
- Re-run the test suite after any fluency fixes.

## 2026-03-29 — Stage 4: Review Fluency
COMPLETED
- Reviewed the public Python API for naming, exceptions, serialization surfaces, and test readability; no additional fluency-driven API changes were required after the code pass.
- Retained the Pythonic constructor/serializer surface (`from_json`, `to_json`, dataclass wrappers, explicit exception codes) while keeping Rust parity for deterministic behavior.
- Verification after fluency review: `65 passed`; no regressions introduced.

## 2026-03-29 — Stage 5: Update Status
COMPLETED
- Marked `python/provenance-mark` as `✅📖` in `AGENTS.md` and kept the crate row-start marker at `🚧` because the C# and Go translations are still pending.
- Appended root `LOG.md` rows for `Translation` and `Fluency` and refreshed `FLUENCY_NEEDED.md`.
- Translation status is complete locally; the target now awaits a cross-model fluency pass from a non-Codex model.

## 2026-03-29 — Stage 6: Capture Lessons
COMPLETED
- Recorded the Python environment lesson about using an existing repo venv for `bc-envelope` dependency graphs with transitive crypto extras.
- Recorded the translation lesson about verifying shared bytemoji tables in dependency packages against Rust vectors before debugging the crate translation itself.

## 2026-03-29 — Stage 3: Check (Cross-Model)
STARTED
- Cross-model completeness check by Claude Opus 4.6 (original translation by GPT Codex).

## 2026-03-29 — Stage 3: Check (Cross-Model)
COMPLETED
- API coverage verified: all 13 source modules, package exports, identifier/disambiguation APIs, validation engine, envelope support.
- Test coverage: 65/65 tests passing, full Rust test inventory covered.
- Verdict: COMPLETE

## 2026-03-29 — Stage 4: Fluency (Cross-Model)
STARTED
- Cross-model fluency review by Claude Opus 4.6.

## 2026-03-29 — Stage 4: Fluency (Cross-Model)
COMPLETED
- Issues found: 2 (type annotation improvements)
- Issues fixed: 2
  - Replaced `object` type hints with proper `Date` and `ProvenanceMark` types in `_mark.py` and `_validate.py` using TYPE_CHECKING guard for circular import avoidance.
  - `_date` field on `ProvenanceMark` dataclass now annotated as `Date` instead of `object`.
  - `_date_ordering()` helper now annotated with `Date` parameters.
  - `FlaggedMark.mark`, `ChainReport.marks`, `ValidationReport.marks`, and `validate()` parameter now typed as `ProvenanceMark`.
- No behavioral changes; all fixes are type annotation only.
- Verification: 65/65 tests passing.
- Verdict: IDIOMATIC

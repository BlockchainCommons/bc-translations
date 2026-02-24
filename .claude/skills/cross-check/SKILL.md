---
name: cross-check
description: >-
  Picks an eligible cross-model target from FLUENCY_NEEDED.md, runs both
  completeness and fluency passes, and fixes same-language dependents broken by
  API improvements.
user-invocable: true
argument-hint: "[language] [crate]"
context: fork
---

# Cross-Check

Run a cross-model completeness + fluency pass for an already translated target, then repair any downstream breakage in translated dependents for the same language.

## API Evolution Policy (De Novo, Mandatory)

- This repository is de novo. No external consumers depend on these APIs yet.
- Breaking API improvements are expected during correctness and fluency work.
- Do not add compatibility layers, deprecated aliases, legacy symbols, transitional wrappers, or shims.
- If an API change breaks downstream targets in this monorepo, update those dependents directly in the same work stream and re-run tests.

## Arguments

- `` (optional):
  - `<language>`: constrain selection to one language (for example, `go`)
  - `<crate>`: constrain selection to one crate (for example, `bc-shamir`)
  - `<language> <crate>`: constrain by both
  - Empty: auto-pick the next eligible target

## Eligibility Rules

Use `FLUENCY_NEEDED.md` as the source of truth.

A row is eligible only when all are true:
1. It appears in the pending table in `FLUENCY_NEEDED.md`.
2. The current model is different from `Model To Avoid (Original Translation)`.
3. It matches optional language/crate filters (if provided).

Model-family matching rule:
- If the current model is GPT/Codex, do not pick rows whose `Model To Avoid` contains `GPT`.
- If the current model is Claude, do not pick rows whose `Model To Avoid` contains `Claude`.

Selection priority:
1. `Pending Reason = No fluency pass logged`
2. Then `Pending Reason = Fluency logged, but only by the translation model`
3. Within same priority, follow dependency-friendly crate order from AGENTS.md (earlier phases first), then language/package alphabetical.

If no eligible row remains, report that and stop.

## Procedure

### Step 0: Select Target

Pick one eligible row from `FLUENCY_NEEDED.md` using the rules above. Do not ask for confirmation unless user-provided filters produce multiple equally valid choices and they asked to choose manually.

### Step 1: Run Completeness Check on Target

Use the `completeness-checker` workflow on the selected target:
- Compare translation against `MANIFEST.md` and Rust source-of-truth.
- Verify API, signatures, derives/protocol mappings, tests, and docs.
- Update `<lang>/<package>/COMPLETENESS.md` with verified items and uncovered gaps.
- Re-run target tests/build if required by the checker.

Append per-target `LOG.md` Stage 3 STARTED/COMPLETED entries with metrics and verdict.

If completeness is INCOMPLETE, **do not stop**. Continue to Step 2 and perform as much fluency correction as possible.

### Step 2: Run Fluency Review on Target (Always)

Use the `fluency-critic` workflow and relevant `rust-to-<lang>` skill on the selected target:
- Read target-language code only for this stage (do not read Rust).
- Apply all fluency findings that are not blocked by missing/incomplete translation pieces.
- If completeness gaps block some fluency fixes, still apply every unblocked fluency fix and document what remains blocked.
- Re-run tests/build after fixes. If full test success is impossible because of known completeness gaps, run the maximal viable test/build subset and capture the exact blockers.

Append per-target `LOG.md` Stage 4 STARTED/COMPLETED entries, including:
- issues found
- issues fixed
- issues blocked by completeness gaps (if any)
- final fluency verdict

Append one root `LOG.md` row for the primary target with task `Fluency`:

`| <date> | <crate> | <version> | <language> | <package> | <model> | Fluency |`

### Step 3: Repair Downstream Dependents (Same Language)

Treat API changes from Step 2 as authoritative. Then verify dependents in the same language using the internal dependency graph from AGENTS.md.

1. Build the translated dependent set for that language (from root `LOG.md` translation rows and/or existing `<lang>/<package>/` targets).
2. Traverse dependents in topological order away from the changed crate.
3. For each translated dependent:
   - Run its build/tests.
   - If broken by upstream API changes from this run, update dependent code to the new API.
   - Do not add shims/deprecations/back-compat wrappers.
   - Re-run tests.
4. Continue until all translated dependents in the affected subgraph are either passing or blocked only by pre-existing completeness gaps.

For each dependent target that required code changes:
- Append per-target `LOG.md` Stage 4 STARTED/COMPLETED entries noting dependency fallout repair.
- Append a root `LOG.md` row with task `Fluency`.

### Step 4: Refresh Cross-Check Queue

After all root `LOG.md` updates, run:

```bash
bash scripts/update-fluency-needed.sh
```

This must be done every run so the queue stays accurate.

### Step 5: Final Verification

- Re-run tests/build for the primary target and all changed dependents (or maximal viable subsets when completeness gaps block full execution).
- Confirm the selected target is removed from `FLUENCY_NEEDED.md` when applicable.
- Report exactly which targets were changed, what was tested, and what remains blocked.

## Definition of Done

This skill run is complete only when:
1. One eligible cross-model target was processed.
2. Completeness check was run and logged for the primary target.
3. Fluency fixes were applied for all unblocked findings on the primary target.
4. Any translated same-language dependents broken by these API changes were fixed and verified.
5. Root `LOG.md` contains `Fluency` rows for all targets touched.
6. `FLUENCY_NEEDED.md` is regenerated with `bash scripts/update-fluency-needed.sh`.

## Stop Conditions

Stop only when blocked by a real technical barrier (for example, irreproducible failing tests or missing upstream translation artifacts). Otherwise continue through completeness, fluency, dependent repairs, and verification.

Do not commit unless explicitly asked by the user. When committing, **only stage files you changed** — your target's `<lang>/<package>/` directory (and any repaired dependents) plus root tracking files (`LOG.md`, `FLUENCY_NEEDED.md`, `AGENTS.md`). Never use `git add -A` or `git add .`. Ignore unstaged changes in other languages; those belong to parallel agents.

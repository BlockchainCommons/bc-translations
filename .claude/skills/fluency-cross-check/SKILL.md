---
name: fluency-cross-check
description: >-
  Picks an eligible cross-model fluency target from FLUENCY_NEEDED.md, runs
  the fluency pass, and fixes any same-language dependents broken by API
  improvements.
user-invocable: true
argument-hint: "[language] [crate]"
context: fork
---

# Fluency Cross-Check

Run a cross-model fluency pass for an already translated target, then repair any downstream breakage in translated dependents for the same language.

## API Evolution Policy (De Novo, Mandatory)

- This repository is de novo. No external consumers depend on these APIs yet.
- Breaking API improvements are expected during fluency work.
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

### Step 1: Run Fluency Review on Target

Use the `fluency-critic` workflow and relevant `rust-to-<lang>` skill on the selected target:
- Read target-language code only (do not read Rust for this stage).
- Apply all findings (must/should/nice-to-have).
- Re-run target tests until passing.

Append per-target `LOG.md` Stage 4 STARTED/COMPLETED entries.

Append one root `LOG.md` row for the target with task `Fluency`:

`| <date> | <crate> | <version> | <language> | <package> | <model> | Fluency |`

### Step 2: Repair Downstream Dependents (Same Language)

Treat API changes from Step 1 as authoritative. Then verify dependents in the same language using the internal dependency graph from AGENTS.md.

1. Build the translated dependent set for that language (from root `LOG.md` translation rows and/or existing `<lang>/<package>/` targets).
2. Traverse dependents in topological order away from the changed crate.
3. For each translated dependent:
   - Run its build/tests.
   - If broken by upstream API changes, update dependent code to the new API.
   - Do not add shims/deprecations/back-compat wrappers.
   - Re-run tests.
4. Continue until all translated dependents in the affected subgraph pass.

For each dependent target that required code changes:
- Append per-target `LOG.md` Stage 4 STARTED/COMPLETED entries noting dependency fallout repair.
- Append a root `LOG.md` row with task `Fluency`.

### Step 3: Refresh Cross-Check Queue

After all root `LOG.md` updates, run:

```bash
bash scripts/update-fluency-needed.sh
```

This must be done every run so the queue stays accurate.

### Step 4: Final Verification

- Re-run tests for the primary target and all changed dependents.
- Confirm the selected target is removed from `FLUENCY_NEEDED.md`.
- Report exactly which targets were changed and tested.

## Definition of Done

This skill run is complete only when:
1. One eligible cross-model target was processed.
2. Primary target fluency fixes are applied and tests pass.
3. Any translated same-language dependents broken by the API changes are fixed and pass tests.
4. Root `LOG.md` contains `Fluency` rows for all targets touched.
5. `FLUENCY_NEEDED.md` is regenerated with `bash scripts/update-fluency-needed.sh`.

## Stop Conditions

Stop only when blocked by a real technical barrier (for example, irreproducible failing tests or missing upstream translation artifacts). Otherwise continue through dependent repairs and verification.

Do not commit unless explicitly asked by the user. When committing, **only stage files you changed** — your target's `<lang>/<package>/` directory (and any repaired dependents) plus root tracking files (`LOG.md`, `FLUENCY_NEEDED.md`, `AGENTS.md`). Never use `git add -A` or `git add .`. Ignore unstaged changes in other languages; those belong to parallel agents.

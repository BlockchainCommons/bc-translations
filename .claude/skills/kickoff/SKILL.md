---
name: kickoff
description: >-
  Select and begin the next translation target. Reads the status table and
  dependency graph to find the next crate/language pair ready for translation,
  then runs the full pipeline. Use to start or resume translation work.
user-invocable: true
argument-hint: "[language] [crate]"
---

# Kickoff

Select and begin the next translation target. Optionally specify a language and/or crate, otherwise the next eligible target is chosen automatically.

## Arguments

- `` — optional. If provided, may be:
  - A language name (e.g., `python`) — pick the next eligible crate for that language
  - A crate name (e.g., `bc-rand`) — pick the next eligible language for that crate
  - Both (e.g., `python bc-rand`) — translate that specific pair
  - Empty — auto-select the next eligible (crate, language) pair

## Selection Logic

1. Read the status table in CLAUDE.md
2. Find all ⏳ (not started) entries
3. Filter to entries whose internal dependencies are ✅ (completed) for the same language
4. From the eligible set:
   - If a language was specified, filter to that language
   - If a crate was specified, filter to that crate
   - Otherwise, prefer crates higher in the dependency graph (fewer deps) and languages with the most progress
5. Present the selection to the user for confirmation before proceeding

## Dependency Rules

A (crate, language) pair is **eligible** when all of its internal BC dependencies have status ✅ for that same language. Check the Internal Dependencies section of CLAUDE.md.

Examples:
- `(bc-rand, python)` is always eligible (no internal deps)
- `(dcbor, python)` is always eligible (no internal deps)
- `(bc-crypto, python)` is eligible only if `(bc-rand, python)` is ✅
- `(bc-components, python)` is eligible only if bc-rand, bc-crypto, dcbor, bc-tags, bc-ur, and sskr are all ✅ for python

## Pipeline

Once a target is selected and confirmed:

### Step 1: Plan
Run the **translation-planner** workflow on the Rust crate (if no manifest exists yet). Save the manifest to `<lang>/<package>/MANIFEST.md`.

### Step 2: Code
Run the **translation-coder** workflow. The relevant `rust-to-<lang>` skill provides language-specific guidance. Translate all source files and tests. Build and test.

### Step 3: Check Completeness
Run the **completeness-checker** workflow. If gaps are found, return to Step 2 to fill them.

### Step 4: Review Fluency
Run the **fluency-critic** workflow. Apply fixes. Re-run tests.

### Step 5: Update Status
Update the status in CLAUDE.md:
- 🚧 if translation exists but has known gaps or failing tests
- ✅ if translation is complete with all tests passing

Commit the result.

### Step 6: Next
Report what was completed and suggest the next eligible target.

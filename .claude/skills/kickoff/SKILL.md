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
2. Find all ⏳ (not started) entries **and** any 🚧 (in progress) entries
3. For 🚧 entries, check `<lang>/<package>/LOG.md` for interrupted stages (STARTED without COMPLETED) — these take priority as resume candidates
4. For ⏳ entries, filter to those whose internal dependencies are ✅ (completed) for the same language
5. From the eligible set:
   - If a language was specified, filter to that language
   - If a crate was specified, filter to that crate
   - Otherwise, prefer resuming interrupted work first, then crates higher in the dependency graph (fewer deps), then languages with the most progress
6. Present the selection to the user for confirmation before proceeding

## Dependency Rules

A (crate, language) pair is **eligible** when all of its internal BC dependencies have status ✅ for that same language. Check the Internal Dependencies section of CLAUDE.md.

Examples:
- `(bc-rand, python)` is always eligible (no internal deps)
- `(dcbor, python)` is always eligible (no internal deps)
- `(bc-crypto, python)` is eligible only if `(bc-rand, python)` is ✅
- `(bc-components, python)` is eligible only if bc-rand, bc-crypto, dcbor, bc-tags, bc-ur, and sskr are all ✅ for python

## Pipeline

Once a target is selected and confirmed:

### Step 0: Mark In Progress
Update the status table in CLAUDE.md (which is a symlink to AGENTS.md) to change the target's marker from ⏳ to 🚧. This signals to other agents that work is underway on this pair.

Initialize (or verify) the target's `<lang>/<package>/LOG.md`. The file must begin with a level-one header and a model identification line:

```
# Translation Log: <rust-crate> → <Language> (<package-name>)

Model: <model-name> <model-version>
```

For example:
```
# Translation Log: bc-rand → Python

Model: Claude Opus 4.6
```

The `(<package-name>)` suffix is included when the package name differs from the crate name (e.g., `BCRand`, `bcrand`, `@bc/rand`). This is mandatory — every LOG.md must start with these lines before any stage entries are appended.

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

Append a row to the top-level `LOG.md` table recording what was done:

```
| <date> | <crate> | <version> | <target> | <model> | <status> |
```

Use today's date, the Rust crate version from CLAUDE.md, and the model's own identifier. The target column uses the language name, with the package name in parentheses when it differs from the crate name (e.g., `Go (bcrand)`, `C# (BCRand)`). Update this log whenever a translation is completed, a status changes, or a previously completed translation is revised.

Commit the result.

### Step 6: Capture Lessons (Rule One)
Before reporting results, apply Rule One: review what went wrong or was unexpected during the session, and record lessons learned in the auto memory files. Check for:
- Build/toolchain surprises (wrong JDK version, missing deps, etc.)
- Language-specific gotchas discovered during translation
- Fluency patterns that should be done right the first time next time
- Test vector or cross-language compatibility issues

Update the relevant `memory/<lang>.md` file and `memory/translation-lessons.md`. Create a new language memory file if this is the first translation for that language.

### Step 7: Next
Report what was completed and suggest the next eligible target.

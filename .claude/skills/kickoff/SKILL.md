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
3. For 🚧 entries, check `<lang>/<package>/LOG.md` for interrupted stages (STARTED without COMPLETED) and `<lang>/<package>/COMPLETENESS.md` for unchecked items — these take priority as resume candidates
4. For ⏳ entries, filter to those whose internal dependencies are ✅ (completed) for the same language
5. From the eligible set:
   - If a language was specified, filter to that language
   - If a crate was specified, filter to that crate
   - Otherwise, prefer resuming interrupted work first, then crates higher in the dependency graph (fewer deps), then languages with the most progress
6. Announce the selection and proceed immediately — do not ask for confirmation

## Dependency Rules

A (crate, language) pair is **eligible** when all of its internal BC dependencies have status ✅ for that same language. Check the Internal Dependencies section of CLAUDE.md.

Examples:
- `(bc-rand, python)` is always eligible (no internal deps)
- `(dcbor, python)` is always eligible (no internal deps)
- `(bc-crypto, python)` is eligible only if `(bc-rand, python)` is ✅
- `(bc-components, python)` is eligible only if bc-rand, bc-crypto, dcbor, bc-tags, bc-ur, and sskr are all ✅ for python

## Pipeline

Once a target is selected, run the entire pipeline to completion without pausing to ask whether the next stage should begin. Move directly from Plan → Code → Check → Critique → Status → Lessons → Next. Only stop to ask the user if you hit a serious technical blocker (e.g., build failures that resist multiple fix attempts, missing upstream dependencies, ambiguous requirements that cannot be resolved from the manifest or existing translations).

### Step 0: Mark In Progress
Update the status table in CLAUDE.md (which is a symlink to AGENTS.md) to change the target's marker from ⏳ to 🚧 **and** append the model marker emoji (see the Model Markers section of CLAUDE.md). For example, change `⏳ DCbor` to `🚧🎻 DCbor` for a Claude Opus translation. This signals to other agents that work is underway on this pair and which model is doing the work.

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

#### COMPLETENESS.md — Progress Checklist

Initialize (or verify) the target's `<lang>/<package>/COMPLETENESS.md`. This file tracks progress on the translation using Markdown checklists. It is separate from `LOG.md` (which is a chronological event log) and from `MANIFEST.md` (which is a static analysis of what needs to be translated). `COMPLETENESS.md` is the living record of what has and hasn't been done.

The file must begin with a level-one header:

```
# Completeness: <rust-crate> → <Language> (<package-name>)
```

Below the header, organize checklists by category. Use nested checkboxes for subtasks. Mark items as they are completed during the Code and Check stages. Example structure:

```markdown
# Completeness: dcbor → Go (dcbor)

## Source Files
- [x] cbor.go — core CBOR type and encode/decode
- [x] byte_string.go — ByteString wrapper
- [ ] map.go — deterministic Map type
  - [x] basic Map operations
  - [ ] duplicate key rejection
  - [ ] mis-order rejection
- [ ] tagged.go — Tag type and tag store

## Tests
- [x] encode_test.go — scalar encode/decode vectors
- [ ] format_test.go — diagnostic formatting
  - [x] basic diagnostic output
  - [ ] annotated hex formatting
- [ ] walk_test.go — tree traversal

## Build & Config
- [x] go.mod
- [x] .gitignore
```

When resuming interrupted work, check `COMPLETENESS.md` first to see what remains. The completeness checker (Step 3) should update this file with its findings, converting any newly discovered gaps into unchecked items.

### Step 1: Plan
Run the **translation-planner** workflow on the Rust crate (if no manifest exists yet). Save the manifest to `<lang>/<package>/MANIFEST.md`.

Also load the **expected-text-output-rubric** skill and evaluate the Rust source repo tests for whether this rubric should be applied in the target translation. The planner output must include an explicit `EXPECTED TEXT OUTPUT RUBRIC` section in `MANIFEST.md`:
- `Applicable: yes` with source signals and target test areas, or
- `Applicable: no` with a short reason.

### Step 2: Code
Run the **translation-coder** workflow. The relevant `rust-to-<lang>` skill provides language-specific guidance. Translate all source files and tests. Build and test. As translation units are completed, check off the corresponding items in `COMPLETENESS.md`.

### Step 3: Check Completeness
Run the **completeness-checker** workflow. Update `COMPLETENESS.md` with the checker's findings — check off confirmed items and add any newly discovered gaps as unchecked items. If gaps remain, return to Step 2 to fill them.

### Step 4: Review Fluency
Run the **fluency-critic** workflow. Apply fixes. Re-run tests.

### Step 5: Update Status
Update the status in CLAUDE.md:
- 🚧 if translation exists but has known gaps or failing tests
- ✅ if translation is complete with all tests passing

When marking ✅, also append the model marker emoji (see the Model Markers section of CLAUDE.md). For example, `✅🎻 BCRand` for a Claude Opus translation, `✅📖 BCRand` for a GPT Codex translation.

Append a row to the top-level `LOG.md` table recording what was done:

```
| <date> | <crate> | <version> | <language> | <package> | <model> | <task> |
```

Use today's date, the Rust crate version from CLAUDE.md, and the model's own identifier. The model column includes the model marker emoji followed by the model name (e.g., `🎻 Claude Opus 4.6`, `📖 GPT 5.3 Codex`). The language column is the target language name (e.g., `Go`, `C#`, `Python`). The package column is the target-language package name (e.g., `bcrand`, `BCRand`, `bc-rand`). The task column describes what was done (e.g., `Translation`). Update this log whenever a translation is completed, a status changes, or a previously completed translation is revised.

When Step 4 runs as a standalone rerun or a post-completion revision, still append a root `LOG.md` row using the same 7-column format, with task text beginning `Fluency critique` (for example: `Fluency critique (Stage 4 rerun + API docs)`).

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

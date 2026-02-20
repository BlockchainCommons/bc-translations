---
name: translation-coder
description: >-
  Translates a Rust crate to a target language using the translation manifest
  from the planner. Produces source code, tests, and package configuration.
  Use when writing the actual translation of a Rust crate.
user-invocable: false
---

# Translation Coder

Translate a Rust crate to a target language. The relevant `rust-to-<lang>` skill will be auto-loaded alongside this one for language-specific guidance.

## Inputs

- **Translation manifest** (`<lang>/<package>/MANIFEST.md`) from the planner
- **Rust source code** in `rust/<crate>/`
- **Already-translated dependencies** in `<lang>/` (if any)

## Procedure

### 1. Scaffold Project Structure

Create the target-language project skeleton:
- Package config file (Cargo.toml equivalent)
- Source directory structure
- Test directory structure
- Declare dependencies on already-translated BC packages
- **Create a `.gitignore`** appropriate for the target language (build outputs, dependency caches, IDE files, OS artifacts). Every scaffolded project must have one before any other files are added.

### 2. Translate in Manifest Order

Follow the translation unit order from the manifest. For each unit:

1. Read the Rust source for this unit
2. Translate types, preserving semantics per the `rust-to-<lang>` skill
3. Translate function signatures, mapping Rust idioms to target idioms
4. Translate function bodies
5. Handle hazards noted in the manifest

### 3. Prioritize Correctness Over Style

Get the translation semantically correct first. Do not try to simultaneously optimize for idiomaticness — the fluency critic handles that in a later pass. A correct but slightly non-idiomatic translation is vastly more valuable than a broken idiomatic one.

### 4. Translate Tests

For every test in the manifest's test catalog:
- Translate the test, preserving test vectors exactly
- Use the target language's test framework conventions
- Ensure deterministic RNG tests use the same seed and produce the same outputs

Test vectors (hardcoded expected byte sequences) are the primary cross-language validation signal. Preserve them byte-for-byte.

### 5. Build and Test

After completing the translation:
1. Attempt to compile/build
2. Fix any compilation errors
3. Run tests
4. Fix any test failures
5. Iterate until all tests pass

Maximum 5 compile-fix iterations. If stuck, document the issue and stop.

### 6. Log

Append entries to `<lang>/<package>/LOG.md` when starting and completing this stage. Include: number of files translated, number of tests translated, build result, test result. See the Orchestration section of CLAUDE.md for the log format. If a LOG.md already exists with a STARTED entry for this stage but no COMPLETED, this is a resumed session — pick up where it left off rather than redoing work.

## Key Principles

- **Translate, don't rewrite.** Stay close to the Rust structure. The goal is a faithful translation, not a reimagination.
- **Preserve test vectors.** Crypto test vectors must produce identical byte-for-byte output across all languages.
- **Use existing translated deps.** When the manifest lists internal BC dependencies, import the already-translated package for that language. Do not re-translate or inline dependency code.
- **One package per crate.** Each Rust crate maps to exactly one target-language package.
- **Default features only.** For the initial translation, translate only code gated by default features. Non-default features are future work.

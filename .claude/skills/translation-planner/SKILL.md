---
name: translation-planner
description: >-
  Analyzes a Rust crate and produces a translation manifest for the coder agent.
  Catalogs the public API surface, external dependencies, feature flags, test
  inventory, and translation hazards. Use when beginning translation of a Rust
  crate to any target language.
user-invocable: false
context: fork
---

# Translation Planner

## API Evolution Policy (De Novo)

- This repository is de novo; there are no external consumers requiring backward compatibility.
- When correctness or fluency work changes an API, apply the new API directly.
- Never add deprecated aliases, compatibility wrappers, or transitional shims.
- If an API change breaks dependent targets in this monorepo, update those dependents in the same work stream and re-run tests.

Analyze a Rust crate and produce a **translation manifest** — a structured artifact that drives all downstream agents (coder, completeness checker, fluency critic). The manifest is language-agnostic: produce it once per crate, reuse it for all six target languages.

## Procedure

### 1. Read Cargo.toml

Extract: crate name, version, internal BC dependencies, external dependencies, feature flags.

### 2. Catalog Public API Surface

Read `lib.rs` and all `pub` modules. For every public item, record:

```
TYPE CATALOG:
- name: TypeName
  kind: struct | enum | trait | type-alias
  fields_or_variants: [...]
  derives: [Clone, Debug, PartialEq, ...]
  implements: [TraitA, TraitB]
  generic_params: [T: Bound]

FUNCTION CATALOG:
- name: function_name
  signature: "fn name(args) -> ReturnType"
  is_method: bool
  on_type: TypeName (if method)
  uses_generics: bool
  uses_result: bool
  uses_option: bool

CONSTANT CATALOG:
- name: CONSTANT_NAME
  type: usize
  value: 32

TRAIT CATALOG:
- name: TraitName
  required_methods: [...]
  provided_methods: [...]
  supertraits: [...]
```

### 3. Catalog Documentation

Record what documentation exists in the Rust source. This is descriptive — it tells the coder what to translate, not what to invent.

```
DOC CATALOG:
- Crate-level doc comment: (yes/no, summary)
- Module-level doc comments: [list of modules with /// or //! docs]
- Public items with doc comments: [list — most will have them]
- Public items WITHOUT doc comments: [list — these need not be documented in target either]
- Package metadata description: (from Cargo.toml `description` field)
- README: (exists yes/no, summary of content)
```

### 4. Identify External Dependency Equivalents

For each non-BC external dependency, note:
- What it provides (e.g., `sha2` provides SHA-256/SHA-512)
- Whether the target language has a standard library equivalent
- Recommended third-party library in each target language if no stdlib equivalent

Common mappings for this project:

| Rust crate         | Purpose               | Stdlib or well-known equivalent           |
|--------------------|-----------------------|-------------------------------------------|
| `sha2`             | SHA-256/512           | Most languages have stdlib crypto          |
| `hmac`             | HMAC                  | Usually in stdlib crypto                   |
| `pbkdf2`           | PBKDF2                | Usually in stdlib crypto                   |
| `hkdf`             | HKDF                  | Less common in stdlib; may need library    |
| `chacha20poly1305` | AEAD encryption       | Varies; often needs a library              |
| `secp256k1`        | ECDSA/Schnorr         | Rarely in stdlib; needs library            |
| `ed25519-dalek`    | ED25519               | Varies by language                         |
| `x25519-dalek`     | X25519 key agreement  | Varies by language                         |
| `chrono`           | Date/time             | Most languages have stdlib date/time       |
| `half`             | IEEE 754 half-float   | Rarely in stdlib; may need manual impl     |
| `unicode-normalization` | NFC/NFD          | Most languages have stdlib or built-in     |
| `num-bigint`       | Arbitrary precision   | Varies; Python has built-in, others need lib |
| `thiserror`        | Error derive macro    | No equivalent needed; use language idiom   |
| `zeroize`          | Secure memory wipe    | Manual implementation per language         |
| `miniz_oxide`      | Deflate compression   | Most languages have stdlib zlib            |
| `scrypt`/`argon2`  | Password KDF          | Varies; often needs library                |

### 5. Analyze Feature Flags

For each feature flag, determine:
- What code it gates (conditional compilation)
- Whether to translate as: compile-time option, runtime option, separate package, or always-on
- Recommendation: for initial translation, translate default features only. Document non-default features as future work.

### 6. Inventory Tests

Catalog every `#[test]` and `#[cfg(test)]` block:
- Test name, location (inline or integration test file)
- What it tests (which public API items)
- Whether it uses test vectors (hardcoded expected byte values) — these are critical for cross-language validation
- Whether it depends on deterministic RNG (`fake_random_data` / `SeededRandomNumberGenerator`)

### 6b. Evaluate Expected Text Output Rubric Applicability

Evaluate whether the source crate should use the `expected-text-output-rubric` in target-language tests:
- Search for explicit `expected-text-output-rubric` comments in source tests.
- Identify tests that validate complex rendered text (diagnostic format, pretty dumps, tree output, CLI text, multiline structure formatting).
- Decide if those tests should be translated as whole-text comparisons (`actual` vs `expected`) rather than many fragment assertions.

Record this decision in the manifest as:

```
EXPECTED TEXT OUTPUT RUBRIC:
- Applicable: yes | no
- Source signals: [...]
- Target tests to apply (if yes): [...]
- Reason (if no): [...]
```

### 7. Determine Translation Unit Order

Within the crate, order translation units by reverse call graph (leaves first):
1. Error types and constants
2. Simple data types (structs with no methods)
3. Utility functions with no internal dependencies
4. Types with methods (ordered by dependency)
5. Trait definitions
6. Trait implementations
7. Top-level public functions
8. Tests

### 8. Note Hazards

Flag Rust patterns that need special attention:
- `impl Trait` in argument position → generics or interface parameter
- Blanket trait implementations → may need explicit implementations per type
- Macro-generated code → must be manually expanded and translated
- `unsafe` blocks → identify what they do and the safe equivalent
- Lifetime annotations → usually drop, but note any that imply ownership semantics
- `Deref`/`AsRef` coercions → explicit conversion in target language
- Builder patterns via method chaining on `&mut self` → target-idiomatic builder

### 9. Write Manifest

Save the manifest to: `<lang>/<package>/MANIFEST.md`

The manifest is the contract between planner and coder. The coder must translate every item in the manifest. The completeness checker verifies against it.

### 10. Log

Append entries to `<lang>/<package>/LOG.md` when starting and completing this stage. See the Orchestration section of CLAUDE.md for the log format. If a LOG.md already exists with a STARTED entry for this stage but no COMPLETED, this is a resumed session — pick up where it left off rather than redoing work.

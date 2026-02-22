# Translation Log: known-values → Kotlin (known-values)

Model: GPT 5.3 Codex

## 2026-02-22 — Stage 0: Mark In Progress
STARTED
- Target: kotlin/known-values
- Dependencies verified: dcbor ✅, bc-components ✅

## 2026-02-22 — Stage 0: Mark In Progress
COMPLETED
- AGENTS.md updated: ⏳ → 🚧📖 for Kotlin known-values
- Row-start marker unchanged: 🚧 (crate already in progress)
- LOG.md initialized
- .gitignore created
- COMPLETENESS.md initialized

## 2026-02-22 — Stage 1: Plan
STARTED
- Analyzing `rust/known-values` API surface, feature gating, tests, docs, and translation hazards for Kotlin.

## 2026-02-22 — Stage 1: Plan
COMPLETED
- Created `MANIFEST.md` with full public API catalog (11 public types, 1 public static, 104 macro-generated constant pairs).
- Cataloged 22 Rust tests in default-feature scope (module + integration).
- Recorded dependency mapping for Kotlin JSON/directory loading equivalents.
- Expected text output rubric: not applicable.

## 2026-02-22 — Stage 2: Code
STARTED
- Scaffolding Kotlin package and translating source modules (`KnownValue`, `KnownValuesStore`, registry constants, directory loader`) plus all Rust tests in scope.

## 2026-02-22 — Stage 2: Code
COMPLETED
- Translated 4 source files and 3 test files for Kotlin known-values.
- Implemented full macro-generated API surface: 104 `KnownValue` constants + 104 raw constants, plus global `KNOWN_VALUES` registry behavior.
- Translated all 22 Rust tests (1 registry unit test, 8 directory-loader unit tests, 13 integration tests).
- `gradle test --console=plain` succeeded.

## 2026-02-22 — Stage 3: Check Completeness
STARTED
- Verifying Kotlin translation against `MANIFEST.md` for API coverage, constant parity, signatures, tests, and documentation.

## 2026-02-22 — Stage 3: Check Completeness
COMPLETED
- API coverage: complete (11 public types + 1 static + macro-generated constants represented).
- Constant coverage: 104/104 `KnownValue` constants and 104/104 raw constants match Rust names/values.
- Test coverage: 22/22 Rust tests translated and passing.
- Signature/docs review: no blocking gaps found.
- Completeness verdict: COMPLETE.

## 2026-02-22 — Stage 4: Fluency Review
STARTED
- Reviewing Kotlin idiomaticness and applying targeted fixes without changing Rust-visible behavior.

## 2026-02-22 — Stage 4: Fluency Review
COMPLETED
- 3 idiomatic/robustness findings identified and fixed:
  - Upgraded JSON codepoint parsing from signed `Long` to full-range `u64` validation via `BigInteger` conversion.
  - Applied explicit `@param:JsonProperty` targets to eliminate Kotlin annotation-target warnings.
  - Renamed private directory-loader globals to Kotlin-style lowerCamelCase and hardened `LoadResult.intoValues()` to return a stable snapshot.
- Re-ran `gradle test --console=plain`; all 22 tests pass.
- Fluency verdict: IDIOMATIC.

## 2026-02-22 — Stage 5: Update Status
STARTED
- Updating translation status and root tracking logs.

## 2026-02-22 — Stage 5: Update Status
COMPLETED
- Updated `AGENTS.md`: Kotlin known-values `🚧📖` → `✅📖`.
- Crate row-start marker remains `🚧` (other languages still incomplete).
- Appended root `LOG.md` rows for `Translation` and `Fluency`.
- Refreshed `FLUENCY_NEEDED.md` via `bash scripts/update-fluency-needed.sh`.

## 2026-02-22 — Stage 6: Capture Lessons
STARTED
- Applying Rule One based on compile/test surprises during Kotlin known-values translation.

## 2026-02-22 — Stage 6: Capture Lessons
COMPLETED
- Added Kotlin memory rules in `memory/kotlin.md` for u64 JSON parsing and transitive dependency assumptions.
- Added cross-language rule in `memory/translation-lessons.md` for preserving unsigned JSON range semantics.

## 2026-02-22 — Stage 7: Next
COMPLETED
- Kotlin `known-values` pipeline completed end-to-end.
- Next eligible Kotlin target: `bc-envelope` (all required Kotlin dependencies are now complete).

## 2026-02-22 — Stage 4: Cross-Model Fluency Review
STARTED
- Cross-model fluency pass by Claude Opus 4.6 (original translation by GPT 5.3 Codex).
- Reviewing Kotlin idiomaticness across all source and test files.

## 2026-02-22 — Stage 4: Cross-Model Fluency Review
COMPLETED
- 13 findings identified and applied:
  - [api] Replaced Rust-style `KnownValue.new(value)` / `newWithName()` / `newWithStaticName()` with idiomatic `KnownValue(value)` (operator invoke) and `KnownValue.withName()`.
  - [api] Removed Rust-leaking `fromULong()`, `fromInt()`, `fromUSize()` factories with Rust type names in docs.
  - [api] Converted `KnownValue` getter methods (`value()`, `assignedName()`, `name()`) to Kotlin properties.
  - [api] Replaced custom `LazyKnownValues` class with Kotlin's `lazy(LazyThreadSafetyMode.SYNCHRONIZED)`.
  - [api] Removed `KnownValuesStore.new()` factory (just use the constructor).
  - [api] Encapsulated `LoadResult` mutable fields behind read-only public properties.
  - [api] Converted `LoadResult.valuesCount()` / `hasErrors()` from methods to properties.
  - [api] Renamed `DirectoryConfig.new()` to `DirectoryConfig.empty()`.
  - [api] Renamed `DirectoryConfig.copyConfig()` to `copy()`.
  - [structure] Removed `constKnownValue()` indirection function; registry constants call `KnownValue.withName()` directly.
  - [structure] Moved `insertInternal` from companion object to private instance method `insertValue`.
  - [test] Renamed `test1()` to descriptive `testKnownValueLookupByName()`.
  - [doc] Removed Rust-referencing doc comments ("`const fn`", "`From<u64>`", "`From<usize>`").
- Updated `KnownValuesStore.loadFromConfig` to use new `result.values()` method.
- Updated all tests to use property access instead of method calls.
- Re-ran `gradle test --console=plain`; all 22 tests pass.
- No downstream Kotlin dependents to repair (bc-envelope and provenance-mark not yet translated).
- Fluency verdict: IDIOMATIC.

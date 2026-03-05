# Translation Log: bc-envelope → TypeScript (@bc/envelope)

Model: GPT-5 Codex

## 2026-03-04 — Stage 0: Mark In Progress
STARTED
- Verified TypeScript dependencies are complete for this target (bc-rand, bc-crypto, dcbor, bc-ur, bc-components, known-values)
- Updated AGENTS.md status cell to `🚧📖 @bc/envelope`
- Initialized project directory and pipeline tracking files

## 2026-03-04 — Stage 0: Mark In Progress
COMPLETED
- Created `typescript/bc-envelope/` with `.gitignore` as the first file
- Initialized `LOG.md` and `COMPLETENESS.md`
- Ready to run Stage 1 planner workflow

## 2026-03-04 — Stage 1: Plan
STARTED
- Analyzing Rust `bc-envelope` v0.43.0 source modules, public API, and default-feature surface
- Building translation manifest for TypeScript package `@bc/envelope`
- Evaluating expected-text-output rubric applicability from Rust test corpus

## 2026-03-04 — Stage 1: Plan
COMPLETED
- Wrote `MANIFEST.md` for TypeScript `@bc/envelope` translation of Rust `bc-envelope` v0.43.0
- Cataloged API families, extension surface, dependency mapping, translation hazards, and test inventory
- Marked `EXPECTED TEXT OUTPUT RUBRIC` as applicable and recorded target test groups/pattern

## 2026-03-04 — Stage 2: Code
STARTED
- Scaffolding TypeScript package config and source/test layout
- Translating source modules in manifest order with iterative compile/test fixes

## 2026-03-04 — Stage 2: Code
COMPLETED
- Completed translation of `bc-envelope` source modules and test suites, including expression/event/response/request and seal coverage.
- Fixed compile/runtime parity issues in envelope tagging, format-context registration, formatting notation summarizers, signature metadata verification, request date parsing, and extension wiring.
- Achieved green build and full test pass: `npm run build && npm test` with 23 test files and 54 tests passing.

## 2026-03-04 — Stage 3: Check
STARTED
- Running manifest-to-implementation completeness verification for source files, tests, and build config.
- Verifying expected-text-output rubric usage for multi-line rendering tests.

## 2026-03-04 — Stage 3: Check
COMPLETED
- Updated `COMPLETENESS.md` to mark all planned source modules, test suites, and config artifacts complete.
- Added and validated missing planned suites: `tests/expression.test.ts` and `tests/seal.test.ts`.
- Confirmed no remaining manifest coverage gaps.

## 2026-03-04 — Stage 4: Critique
STARTED
- Reviewing TypeScript fluency and test maintainability, with focus on output-assertion idioms and formatter behavior.
- Ensuring expected-text-output rubric compliance in formatting-sensitive tests.

## 2026-03-04 — Stage 4: Critique
COMPLETED
- Aligned formatting-sensitive tests to the TypeScript expected-text-output rubric via shared `expectActualExpected` helper with mismatch logging and stable indentation normalization.
- Improved idiomatic runtime behavior by switching envelope tag lookup to lazy resolution and ensuring global tag registration for UR and notation formatting.
- Re-ran build and test suite after fluency/correctness edits; all tests remain green (23/23 files, 54/54 tests).

## 2026-03-05 — Stage 3: Check (Cross-Model)
STARTED
- Cross-model completeness check by Claude Opus 4.6 (original translation by GPT Codex)
- Comparing Rust test inventory against TypeScript test files

## 2026-03-05 — Stage 3: Check (Cross-Model)
COMPLETED
- API surface: COMPLETE -- all public types, methods, and extension families are translated
- Test coverage: INCOMPLETE -- 54 TypeScript tests vs 139 Rust tests (~39% coverage)
- Largest gaps: edge (1/44), elision (2/16), core (6/17), crypto (2/10), format (4/12)
- Environment blocker: 2 argon2 tests fail on Node.js 18 (requires 22+)
- Updated COMPLETENESS.md with per-file test gap details

## 2026-03-05 — Stage 4: Critique (Cross-Model)
STARTED
- Cross-model fluency review by Claude Opus 4.6
- Reviewing TypeScript idiomaticness without reading Rust source

## 2026-03-05 — Stage 4: Critique (Cross-Model)
COMPLETED
- Issues found: 10
- Issues fixed: 10
- Issues blocked by completeness gaps: 0
- Fixes applied:
  1. Removed dead `Result<T>` type alias and export
  2. Made `Expression` immutable (readonly #envelope, withParameter returns new instance)
  3. Replaced `JSON.stringify` comparison with proper `functionsEqual` value comparison
  4. Made `Request` immutable (readonly #body, withParameter returns new instance)
  5. Made `Response` immutable (readonly #resultOrError, withResult/withError return new instances)
  6. Fixed `hasType`/`hasTypeValue` to use `Envelope.from` directly instead of `envelope.constructor` cast
  7. Replaced IIFE throw patterns with clearer if-throw in 6 locations
  8. Used `ObscureType` enum values explicitly instead of string literals
  9. Replaced fragile raw CBOR object cast in `functionTaggedCbor`/`parameterTaggedCbor` with proper `toTaggedValue` API
  10. Fixed `Assertion` instanceof check in `envelope-encodable.ts` (was using constructor.name string)
- All 52 passing tests remain green; 2 argon2 tests still fail (pre-existing Node.js 18 limitation)
- Final fluency verdict: PASS

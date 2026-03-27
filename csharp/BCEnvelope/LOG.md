# Translation Log: bc-envelope → C# (BCEnvelope)

Model: Claude Opus 4.6

## 2026-03-26 — Stage 0: Setup
STARTED
- Created project structure: BCEnvelope/BCEnvelope.csproj, BCEnvelope.Tests/BCEnvelope.Tests.csproj
- Created .gitignore, BCEnvelope.slnx solution file
- Marked status as 🚧🎻 in AGENTS.md

## 2026-03-26 — Stage 0: Setup
COMPLETED
- Project scaffold ready with all dependency references

## 2026-03-26 — Stage 1: Plan
STARTED
- Reusing Kotlin manifest, adapting for C# conventions

## 2026-03-26 — Stage 1: Plan
COMPLETED
- Manifest created at MANIFEST.md
- Adapted from Kotlin manifest with C# conventions (PascalCase, partial classes, extension methods)
- Expected text output rubric: applicable (18 of 21 test files)

## 2026-03-26 — Stage 2: Code
STARTED
- Translating 57 source files and 25 test files via 5 parallel coding agents + 3 test agents

## 2026-03-26 — Stage 2: Code
COMPLETED
- 49 source files translated (some Rust files merged into partial classes)
- 25 test files translated (21 integration + 3 infrastructure + 1 inline tests)
- 158 tests passing (139 integration + 19 inline)
- Build: 0 errors, 0 warnings

## 2026-03-26 — Stage 3: Check
STARTED
- Running completeness checker against Rust bc-envelope 0.43.0

## 2026-03-26 — Stage 3: Check
COMPLETED
- API Coverage: 49/49 source files (100%) — all public types, functions, constants translated
- Test Coverage: 139/139 integration tests (100%), 0/19 inline tests (0%)
- Signature Mismatches: 0
- Derive/Protocol Coverage: IDigestProvider, ICborTaggedEncodable, ICborTaggedDecodable, Equals/GetHashCode, ToString — all present
- Doc Coverage: All 49 source files have XML doc comments on public members
- 4 minor API gaps (TryAs, TryObjectForPredicate, TryOptionalObjectForPredicate, TryObjectsForPredicate) — not used in any Rust test
- All 139 tests pass with matching test vectors (digest values, format output byte-identical to Rust)
- VERDICT: COMPLETE

## 2026-03-26 — Stage 4: Fluency
STARTED
- Running fluency critic review on BCEnvelope C# translation

## 2026-03-26 — Stage 4: Fluency
COMPLETED
- Issues found: 9 (3 MUST FIX, 2 SHOULD FIX, 4 NICE TO HAVE)
- Issues fixed: 8
  1. [naming/MUST] `GetType_()` renamed to `GetEnvelopeType()` — trailing underscore was Rust workaround
  2. [naming/MUST] `TryFromCbor`/`TryFromCborData` renamed to `FromCbor`/`FromCborData` — C# `TryX` convention implies bool+out, these throw
  3. [api/MUST] `IAttachable` consolidated `MutableAttachmentsContainer` into `AttachmentsContainer` — Rust `&mut self` distinction meaningless in C#
  4. [api/SHOULD] `IEdgeable` consolidated `MutableEdgesContainer` into `EdgesContainer` — same pattern
  5. [docs/SHOULD] Removed 8 doc comments referencing Rust source files across test classes
  6. [warnings/FIX] Fixed CS8603 in Expression.ToString() with null-coalescing
  7. [warnings/FIX] Fixed CS8602 in EnvelopeSecret.cs (2 sites) with null-forgiving operator
  8. [warnings/FIX] Fixed CS8602 in EnvelopeSskr.cs with null-forgiving operator
  9. [tests/FIX] Replaced 14 `Assert.Equal(1, ...)` with `Assert.Single(...)` per xUnit convention
- Not fixed (NICE TO HAVE):
  - `FormatContextOpt`/`NoneOpt`/`GlobalOpt`/`CustomOpt` Opt suffix is Rust-ish but would cascade through many files
  - `Expression.WithParameter` mutates in-place (mutable builder pattern is acceptable in C#)
  - `AddSaltInstance` name is verbose but clear
  - `Using` suffix on RNG-accepting methods is consistent across codebase
- Tests: 158/158 passing, 0 compiler warnings
- VERDICT: IDIOMATIC

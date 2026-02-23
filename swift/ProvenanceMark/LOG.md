# Translation Log: provenance-mark → Swift (ProvenanceMark)

Model: Claude Opus 4.6

## 2026-02-23 — Stage 0: Initialize
STARTED
- All Swift dependencies confirmed ✅: BCRand, DCBOR, BCTags, BCUR, BCEnvelope
- Created swift/ProvenanceMark/ directory structure
- Initialized LOG.md and COMPLETENESS.md

## 2026-02-23 — Stage 0: Initialize
COMPLETED
- Ready to begin translation pipeline

## 2026-02-23 — Stage 1: Plan
STARTED
- Analyzing Rust provenance-mark crate v0.23.0

## 2026-02-23 — Stage 1: Plan
COMPLETED
- MANIFEST.md created with full API surface catalog
- 13 source files, 38 tests identified
- Key hazards: ChaCha20 obfuscation, multi-resolution date serialization, CBOR ExpressibleByNilLiteral

## 2026-02-23 — Stage 2: Code
STARTED
- Translating all source files and tests

## 2026-02-23 — Stage 2: Code
COMPLETED
- 13 source files translated (~2,800 lines)
- 43 tests across 5 suites (38 from Rust + 5 Swift-specific)
- All tests passing
- Key bugs fixed: CBOR nil ternary issue, date-only format for midnight dates

## 2026-02-23 — Stage 3: Check
STARTED
- Verifying completeness against manifest

## 2026-02-23 — Stage 3: Check
COMPLETED
- 100% public API coverage
- 100% test coverage (38/38 Rust tests translated)
- No gaps found

## 2026-02-23 — Stage 4: Critique
STARTED
- Fluency review of all Swift source files

## 2026-02-23 — Stage 4: Critique
COMPLETED
- 20+ fluency findings addressed:
  - `res` → `resolution` throughout (Swift API Design Guidelines: avoid abbreviations)
  - `precedesOpt` → `validatePrecedes` (remove Rust naming leak)
  - `toBytes()`/`toData()`/`toState()` → computed properties
  - `message()` → `var message`, `identifier()` → `var identifier`
  - `toBytewords()`/`fromBytewords()` → `bytewords(style:)`/`init(resolution:bytewords:)`
  - `toURL()`/`fromURL()` → `url(base:)`/`init(url:)`
  - `toUrlEncoding()`/`fromUrlEncoding()` → `var urlEncoding`/`init(urlEncoding:)`
  - `markdownSummary()` → `var markdownSummary`
  - Free constants → static properties (`ProvenanceSeed.byteLength`, `RngState.byteLength`)
  - Free functions → static methods (`registerTags()`/`registerTags(in:)`)
  - Removed dead `fromCBORError` method
  - Made FlaggedMark initializers public
  - Fixed Rust-leaking error messages (`u32` → `UInt32`, `chain_id` → `chain ID`)
  - Removed Rust reference from Xoshiro doc comment
  - Fixed `nonisolated(unsafe)` warning on Sendable type
  - Simplified `hasIssues` with `contains(where:)`
- All 43 tests pass, zero warnings

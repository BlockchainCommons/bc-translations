# Translation Log: provenance-mark → C# (ProvenanceMark)

Model: GPT Codex

## 2026-03-29 — Stage 0: Setup
STARTED
- Verified C# dependency eligibility for `provenance-mark` (`BCRand`, `DCbor`, `BCTags`, `BCUR`, optional `BCEnvelope` are all complete)
- Initializing `csharp/ProvenanceMark` scaffold and status tracking

## 2026-03-29 — Stage 0: Setup
COMPLETED
- Created `.gitignore`, solution, library/test projects, and tracking files
- Target scaffold ready for manifesting and implementation

## 2026-03-29 — Stage 1: Plan
STARTED
- Analyzing Rust `provenance-mark` crate (`0.24.0`) for API surface, feature gating, dependency mapping, test inventory, and deterministic parity hazards
- Producing a fresh C# manifest instead of reusing older `0.23.0` manifests from completed targets

## 2026-03-29 — Stage 1: Plan
COMPLETED
- Wrote `MANIFEST.md` for the current Rust `0.24.0` API, including the identifier/disambiguation additions
- Cataloged source modules, public types/functions/constants, test inventory, envelope support, and expected-text output requirements
- Identified strict parity hazards: ChaCha20 keystream semantics, Xoshiro256** byte/state layout, date packing bounds, JSON field naming, and validation report formatting

## 2026-03-29 — Stage 2: Code
STARTED
- Translating the C# library in manifest order, beginning with low-level cryptography, date packing, resolution logic, fixed-size wrappers, and PRNG parity
- Targeting deterministic Rust/Kotlin parity before layering mark/generator/validation types and translated tests

## 2026-03-29 — Stage 2: Code
COMPLETED
- Translated all 13 manifest source units plus 8 xUnit test files and 2 shared JSON vector resources.
- Implemented deterministic parity for ChaCha20/HKDF obfuscation, Xoshiro256**, compact date encodings, mark/generator/report JSON, and CBOR/UR/Envelope conversions.
- Verification: `dotnet test csharp/ProvenanceMark/ProvenanceMark.Tests/ProvenanceMark.Tests.csproj` passed with 65/65 tests.

## 2026-03-29 — Stage 3: Check
STARTED
- Checking the C# translation against `MANIFEST.md` for public API coverage, protocol support, and translated test inventory.

## 2026-03-29 — Stage 3: Check
COMPLETED
- API coverage: 13/13 manifest source units translated, including default-feature Envelope support and the 0.24.0 identifier/disambiguation APIs.
- Test coverage: 60/60 translated Rust/Kotlin inventory checks passing, plus 5 direct support-type/regression tests for wrappers and issue equality.
- Protocol coverage: CBOR tagging/decoding, UR encoding/decoding, URL encoding, Envelope round-trips, JSON round-trips, equality/hashing, and validation report formatting all verified.
- Verdict: COMPLETE

## 2026-03-29 — Stage 4: Fluency
STARTED
- Reviewing the C# translation for API sharp edges, equality semantics, and target-language idioms without changing Rust-defined behavior.

## 2026-03-29 — Stage 4: Fluency
COMPLETED
- Issues found: 1
- Issues fixed: 1
  - `HashMismatchIssue` now compares by byte content instead of array reference identity, restoring Rust-equivalent value semantics for validation issues that carry binary payloads.
- Post-fix verification: `dotnet test csharp/ProvenanceMark/ProvenanceMark.Tests/ProvenanceMark.Tests.csproj` passed with 65/65 tests.
- Verdict: IDIOMATIC

## 2026-03-29 — Stage 5: Update Status
COMPLETED
- Updated `AGENTS.md` to mark C# `ProvenanceMark` as `✅📖`.
- Appended root `LOG.md` rows for `Translation` and `Fluency`.
- Refreshed `FLUENCY_NEEDED.md` via `bash scripts/update-fluency-needed.sh`.

## 2026-03-29 — Stage 6: Capture Lessons (Rule One)
COMPLETED
- Recorded the C# lesson about byte-array payloads inside record-like validation models requiring explicit content equality.

## 2026-03-29 — Stage 3: Check (Cross-Model)
STARTED
- Cross-model completeness check by Claude Opus 4.6 (original translation by GPT Codex).

## 2026-03-29 — Stage 3: Check (Cross-Model)
COMPLETED
- API coverage verified: all 13 manifest source units, CBOR/UR/URL/envelope conversions, identifier/disambiguation APIs, validation engine.
- Test coverage: 65/65 tests passing, matching Rust test inventory plus C#-specific support tests.
- Verdict: COMPLETE

## 2026-03-29 — Stage 4: Fluency (Cross-Model)
STARTED
- Cross-model fluency review by Claude Opus 4.6.

## 2026-03-29 — Stage 4: Fluency (Cross-Model)
COMPLETED
- Issues found: 0 actionable
- The C# translation is highly idiomatic: sealed classes with defensive cloning, records for validation issues, proper IEquatable<T> implementations, ReadOnlySpan<byte> usage, ArgumentNullException.ThrowIfNull guards, and clean switch expressions.
- ProvenanceMarkResolution as a sealed class with static instances is a good C# pattern that enables associated methods while maintaining value semantics.
- No code changes required.
- Verification: 65/65 tests passing.
- Verdict: IDIOMATIC

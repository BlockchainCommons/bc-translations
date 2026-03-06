# Completeness: provenance-mark → TypeScript (@bc/provenance-mark)

## Source Files
- [x] error.ts — ProvenanceMarkError, ProvenanceMarkErrorCode
- [x] crypto-utils.ts — sha256, sha256Prefix, extendKey, hkdfHmacSha256, obfuscate (ChaCha20)
- [x] xoshiro256starstar.ts — Xoshiro256StarStar PRNG
- [x] date-serialization.ts — 2/4/6-byte date encode/decode
- [x] resolution.ts — ProvenanceMarkResolution enum, byte ranges, serialization helpers
- [x] seed.ts — ProvenanceSeed with CBOR, Envelope, JSON support
- [x] rng-state.ts — RngState with CBOR, Envelope, JSON support
- [x] util.ts — parseSeed and parseDate public helpers
- [x] provenance-mark.ts — ProvenanceMark with CBOR, UR, Envelope, Bytewords, URL, JSON
- [x] provenance-mark-info.ts — ProvenanceMarkInfo with markdown summary and JSON
- [x] provenance-mark-generator.ts — ProvenanceMarkGenerator with Envelope and JSON
- [x] validate.ts — ValidationReport, ChainReport, SequenceReport, FlaggedMark
- [x] index.ts — barrel exports

## Tests
- [x] crypto-utils.test.ts — sha256, extendKey, obfuscate (3 tests)
- [x] xoshiro256starstar.test.ts — RNG output, state save/restore (2 tests)
- [x] date.test.ts — 2/4/6-byte date serialization boundaries (3 tests)
- [x] mark.test.ts — 8 vector tests + envelope + metadata analogs for Rust sync checks (11 tests)
- [x] validate.test.ts — all 19 validation scenarios
- [x] util.test.ts — parseSeed and parseDate coverage (2 tests)

## Build & Config
- [x] package.json
- [x] tsconfig.json
- [x] vitest.config.ts
- [x] .gitignore

## Planning & Review
- [x] MANIFEST.md — recreated during the 2026-03-06 cross-check with full API/test inventory

## 2026-03-06 Cross-Check Summary
- [x] Restored missing public API from Rust surface: `parseSeed`, `parseDate`, resolution range helpers, `ProvenanceMark.validate`
- [x] Reinstated missing Rust test analogs: `test_envelope`, `test_readme_deps`, `test_html_root_url`
- [x] Added TypeScript-only utility coverage for the public parse helpers
- [x] Verified Rust-documented public API comments are present in the TypeScript translation
- [x] API Coverage: 100% of manifest-tracked items
- [x] Test Coverage: 38/38 Rust tests mapped, plus 2 TypeScript-only tests
- [x] Signatures: 0 mismatches
- [x] Verdict: COMPLETE

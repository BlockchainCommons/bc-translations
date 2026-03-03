# Completeness: sskr → C# (SSKR)

## Source Files
- [x] SSKR/SSKRException.cs — error model and Rust error parity
- [x] SSKR/Secret.cs — validated secret wrapper
- [x] SSKR/GroupSpec.cs — member threshold/count spec
- [x] SSKR/Spec.cs — group threshold and grouped spec
- [x] SSKR/SSKRShare.cs — internal share metadata model
- [x] SSKR/Sskr.cs — generation, combination, share serialization, and public constants

## Public API
- [x] `SSKRException` and `SskrError` variants (including wrapped BCShamir errors)
- [x] `Secret` (`Create`, `Length`, `IsEmpty`, `Data`, equality semantics)
- [x] `GroupSpec` (`Create`, `Parse`, `Default`, `ToString`, accessors)
- [x] `Spec` (`Create`, `GroupThreshold`, `Groups`, `GroupCount`, `ShareCount`)
- [x] `Sskr.Generate`
- [x] `Sskr.GenerateUsing`
- [x] `Sskr.Combine`
- [x] `MinSecretLen`, `MaxSecretLen`, `MaxShareCount`, `MaxGroupsCount`
- [x] `MetadataSizeBytes`, `MinSerializeSizeBytes`

## Tests
- [x] `TestSplit35` — deterministic 3-of-5 single-group split/recover
- [x] `TestSplit27` — deterministic 2-of-7 single-group split/recover
- [x] `TestSplit2323` — deterministic two-group split/recover
- [x] `TestShuffle` — Fisher-Yates deterministic output vector
- [x] `FuzzTest` — 100 randomized round-trips
- [x] `ExampleEncode` — two-group encode/decode example
- [x] `ExampleEncode3` — regression for issue #1 (`1-of-3`)
- [x] `ExampleEncode4` — regression for seedtool-cli #6 behavior
- [x] Rust-only metadata tests intentionally omitted: `test_readme_deps`, `test_html_root_url`

## Build & Config
- [x] `SSKR/SSKR.csproj`
- [x] `SSKR.Tests/SSKR.Tests.csproj`
- [x] `SSKR.slnx`
- [x] `.gitignore`

## Checker Passes
- [x] 2026-03-03 — Stage 3 completeness pass: API 14/14, signatures 0 mismatches, tests 8/8, docs verified, verdict COMPLETE
- [x] 2026-03-03 — Cross-check completeness pass (Claude Opus 4.6): API 14/14, constants 6/6, error variants 15/15, tests 8/8, validation semantics verified against Rust source, verdict COMPLETE

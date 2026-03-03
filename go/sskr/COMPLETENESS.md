# Completeness: sskr → Go (sskr)

Cross-checked by Claude Opus 4.6 on 2026-03-03.

## Source Files
- [x] constants.go — exported protocol constants (6/6 match Rust)
- [x] errors.go — exported error values (15/15 Rust variants) and ShamirError wrapper
- [x] secret.go — Secret validation, byte accessors, defensive copies
- [x] spec.go — Spec and GroupSpec models, validation, parser, Display, Default
- [x] share.go — internal sskrShare metadata model
- [x] encoding.go — generation, serialization, deserialization, combine
- [x] doc.go — package documentation

## Tests
- [x] sskr_test.go — all translated Rust tests
  - [x] TestSplit35 — deterministic single-group 3-of-5 split/combine
  - [x] TestSplit27 — deterministic single-group 2-of-7 split/combine
  - [x] TestSplit2323 — deterministic two-group 2-of-3 + 2-of-3 split/combine
  - [x] TestShuffle — deterministic Fisher-Yates shuffle via fake RNG
  - [x] TestFuzz — 100 randomized round trips
  - [x] TestExampleEncode — docs example round trip (2 groups: 2-of-3 and 3-of-5)
  - [x] TestExampleEncode3 — regression: roundtrip works for 2-of-3, 1-of-1, and 1-of-3
  - [x] TestExampleEncode4 — regression: group threshold 1 ignores unrecoverable extra group

## Build & Config
- [x] .gitignore
- [x] go.mod (go 1.21, local replace directives for bcrand, bcshamir, bccrypto)
- [x] go.sum

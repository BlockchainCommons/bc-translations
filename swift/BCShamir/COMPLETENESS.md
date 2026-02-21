# Completeness: bc-shamir → Swift (BCShamir)

## Source Files
- [x] ShamirError.swift — Error enum with 8 variants
- [x] Hazmat.swift — GF(2^8) bit-sliced operations (7 functions)
- [x] Interpolate.swift — Lagrange interpolation (2 functions)
- [x] Shamir.swift — splitSecret, recoverSecret, 3 constants, 2 private helpers

## API Surface (18/18)
- [x] `minSecretLen` constant (= 16)
- [x] `maxSecretLen` constant (= 32)
- [x] `maxShareCount` constant (= 16)
- [x] `ShamirError` enum (8 variants: secretTooLong, tooManyShares, interpolationFailure, checksumFailure, secretTooShort, secretNotEvenLen, invalidThreshold, sharesUnequalLength)
- [x] `splitSecret(threshold:shareCount:secret:randomGenerator:)` public function
- [x] `recoverSecret(indexes:shares:)` public function
- [x] Internal: bitslice, unbitslice, bitsliceSetall, gf256Add, gf256Mul, gf256Square, gf256Inv
- [x] Internal: interpolate, hazmatLagrangeBasis
- [x] Internal: createDigest, validateParameters, secretIndex, digestIndex

## Signature Compatibility (0 mismatches)
- [x] `splitSecret` — parameters and return type map correctly
- [x] `recoverSecret` — parameters and return type map correctly
- [x] All internal function signatures are semantically equivalent

## Tests (4/4)
- [x] BCShamirTests.swift
  - [x] testSplitSecret3of5 — deterministic 16-byte secret, all 5 share vectors match Rust
  - [x] testSplitSecret2of7 — deterministic 32-byte secret, all 7 share vectors match Rust
  - [x] testExampleSplit — non-deterministic split with secure RNG
  - [x] testExampleRecover — recover from static shares, vectors match Rust
- [x] FakeRandomNumberGenerator — counter logic matches Rust (0, 17, 34, ... wrapping)
- [x] Rust-only metadata tests correctly omitted (test_readme_deps, test_html_root_url)

## Derive/Protocol Coverage
- [x] `ShamirError: Error` — conforms to Swift Error protocol
- [x] `ShamirError: Sendable` — concurrent safety
- [x] `ShamirError` error descriptions — `LocalizedError` conformance with `errorDescription` for all variants

## Documentation (5/5 public items)
- [x] `minSecretLen` — doc comment present
- [x] `maxSecretLen` — doc comment present
- [x] `maxShareCount` — doc comment present
- [x] `splitSecret` — doc comment with parameters, returns, throws
- [x] `recoverSecret` — doc comment with parameters, returns, throws
- [x] `ShamirError` — doc comment present (exceeds Rust which has none)

## Build & Config
- [x] .gitignore
- [x] Package.swift (swift-tools-version 6.0, macOS 13+, iOS 16+)
- [x] Build succeeds with zero warnings
- [x] All 4 tests pass

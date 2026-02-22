# Translation Log: bc-components → Swift (BCComponents)

Model: GPT 5.3 Codex

## 2026-02-22 — Stage 0: Mark In Progress
STARTED
- Target selected: bc-components → Swift (BCComponents)
- Dependencies verified: BCRand ✅, BCCrypto ✅, DCBOR ✅, BCTags ✅, BCUR ✅, SSKR ✅

## 2026-02-22 — Stage 0: Mark In Progress
COMPLETED
- Updated `AGENTS.md` status table: Swift bc-components ⏳ → 🚧📖
- Initialized `swift/BCComponents/` with `.gitignore` as first scaffold file
- Initialized `LOG.md` and `COMPLETENESS.md`

## 2026-02-22 — Stage 1: Plan
STARTED
- Analyzing Rust `bc-components` public API, feature gates, and test inventory from `rust/bc-components/src`
- Building Swift translation manifest and hazard checklist from Rust source

## 2026-02-22 — Stage 1: Plan
COMPLETED
- Created `MANIFEST.md` with Rust export inventory, feature mapping, translation unit order, and test inventory
- Cataloged 97 Rust tests, including 4 ignored and 2 metadata/version-sync tests marked non-behavioral
- EXPECTED TEXT OUTPUT RUBRIC set to applicable with source signals and Swift target test areas

## 2026-02-22 — Stage 2: Code
STARTED
- Creating Swift package scaffold and module exports for `BCComponents`
- Implementing Rust-aligned core components first (error model, digest/IDs, byte containers, symmetric crypto, X25519, SSKR bridge, tag registration)

## 2026-02-22 — Stage 3: Check Completeness
STARTED
- Compared implemented Swift surface against `MANIFEST.md` export/test inventory
- Updated `COMPLETENESS.md` with translated modules and outstanding gaps

## 2026-02-22 — Stage 3: Check Completeness
COMPLETED
- Core translated and verified: digest/reference baseline, IDs baseline, JSON/Salt/Nonce, symmetric crypto, X25519, SSKR bridge, tag registration
- Ported and passing vector tests: 7
- Remaining gap inventory recorded in `COMPLETENESS.md` (major missing areas include signing, encapsulation, encrypted-key, key aggregates, pqcrypto, and SSH)
- Verdict: INCOMPLETE (return to Stage 2 required)

## 2026-02-22 — Stage 6: Capture Lessons
STARTED
- Capturing Swift and cross-translation lessons from Stage 2/3 implementation and build tooling behavior

## 2026-02-22 — Stage 6: Capture Lessons
COMPLETED
- Added Swift-specific lesson to `memory/swift.md` for `BCTags` transitive-version conflicts with `DCBOR`
- Added generalized lesson to `memory/translation-lessons.md` for resolving tag-surface drift in Swift dependency graphs

## 2026-02-22 — Stage 4: Review Fluency
STARTED
- Reviewing translated Swift baseline modules for naming, API ergonomics, and Swift-idiomatic error handling
- Re-validating with strict build flags (`-warnings-as-errors`)

## 2026-02-22 — Stage 4: Review Fluency
COMPLETED
- Resolved package-level tag-surface mismatch by using explicit tag values where the transitive `BCTags` API is older than Rust's current tag set
- Simplified decrypt error mapping to avoid unnecessary type-cast warning paths
- Verification passed: `swift test` and `swift test -Xswiftc -warnings-as-errors`
- Scope note: fluency pass applied only to currently translated baseline modules; remaining manifest units still pending Stage 2

## 2026-02-22 — Stage 2: Code (Resume)
STARTED
- Implementing Rust `tags_registry.rs` summarizer parity and dependency-target support needed by Swift (`BCTags`/`DCBOR`)
- Revalidating package tests across dependency targets and `BCComponents`

## 2026-02-22 — Stage 2: Code (Resume)
COMPLETED
- Added tag summarizer infrastructure to `swift/BCTags` (`TagSummarizer`, per-tag registration/lookup) and integrated summary rendering behavior in `swift/DCBOR`
- Implemented `BCComponents` tag summarizers for translated component types in `Sources/BCComponents/TagsRegistry.swift`
- Added/updated tests: `swift/BCTags/Tests/BCTagsTests/TagsTests.swift`, `swift/DCBOR/Tests/DCBORTests/SummaryTests.swift`, `swift/BCComponents/Tests/BCComponentsTests/TagsRegistryTests.swift`
- Verification passed: `swift test` in `swift/BCTags`, `swift/DCBOR`, and `swift/BCComponents` (`62` BCComponents tests passing)
- Remaining default-feature gap: SSH signing/key support parity (algorithms, CBOR/UR/text vectors, and related tests)

## 2026-02-22 — Stage 2: Code (Resume)
STARTED
- Porting additional Rust non-SSH test parity from `digest.rs`, `signing/mod.rs`, `symmetric/mod.rs`, and `lib.rs` vectors
- Revalidating package behavior with full `swift test` in `swift/BCComponents`

## 2026-02-22 — Stage 2: Code (Resume)
COMPLETED
- Added Rust-parity tests in `DigestTests.swift`, `HKDFRngTests.swift`, `SigningTests.swift`, and `SymmetricTests.swift`
- Restored signing key UR vector coverage from Rust `lib.rs` (`test_ecdsa_signing_keys` equivalents)
- Restored deterministic secp256k1 vector coverage from Rust `lib.rs` (`test_ecdsa_signing`)
- Restored full symmetric module test parity (`6/6`) including CBOR byte-vector and roundtrip checks
- Verification passed: `swift test` in `swift/BCComponents` (`80` tests passing, `0` failures)
- Remaining default-feature gap: SSH signing/key support parity (algorithms, CBOR/UR/text vectors, and related tests)

## 2026-02-22 — Stage 2: Code (Resume)
STARTED
- Implementing Rust SSH signing/key support in Swift without introducing legacy Swift package dependencies
- Porting SSH CBOR/signing parity tests and revalidating package test suites

## 2026-02-22 — Stage 2: Code (Resume)
COMPLETED
- Added SSH key/signature support in `BCComponents` for Ed25519 and ECDSA P-256/P-384 (`SSHSupport.swift`, signing enums, CBOR roundtrips)
- Added `PrivateKeyBase` SSH key/public-key aggregate helpers (`sshSigningPrivateKey`, `sshPrivateKeys`, `sshPublicKeys`)
- Added SSH parity tests to `SigningTests.swift` (keypair, options-required behavior, CBOR roundtrip)
- Verification passed: `swift test --filter SigningTests` and class-wise full suite run (`85` tests passing, `0` failures)
- Remaining default-feature gap: Rust SSH parity items not yet ported (DSA/P-521 and related vectors)

## 2026-02-22 — Stage 2: Code (Resume)
STARTED
- Aligning `PrivateKeyBase` SSH derivation behavior with Rust deterministic Ed25519 vectors
- Porting additional Rust SSH parity tests and tightening SSH tag summarizers

## 2026-02-22 — Stage 2: Code (Resume)
COMPLETED
- Implemented deterministic OpenSSH Ed25519 key encoding from `PrivateKeyBase` key material (`SSHSupport.swift`) including Rust-equivalent checkint behavior
- Routed SSH Ed25519 generation through `PrivateKeyBase` in `SignatureScheme` and enabled `keypairUsing` for SSH Ed25519/P-256/P-384
- Updated SSH tag summarizers to include reference short IDs for private/public keys (Rust parity style)
- Added tests in `SigningTests.swift`: deterministic Ed25519 vector parity, SSH `keypairUsing` behavior, and explicit DSA parity placeholders (`XCTSkip` on this host)
- Verification passed: `swift test --filter SigningTests`, `swift test --filter TagsRegistryTests`, and full `swift test` (`89` tests total, `2` skipped, `0` failures)
- Remaining default-feature gap: Rust SSH DSA parity (algorithm support and DSA-specific vectors)

# Completeness: bc-envelope → Swift (BCEnvelope)

## Source Files
- [x] Rust API surface mapped in `MANIFEST.md`

## Public API
- [x] Public exports translated (base, format, extension modules)
- [x] Envelope core type and assertion model translated
- [x] Error model translated
- [x] Envelope encoding/decoding traits and CBOR mappings translated
- [x] Formatting APIs translated (notation, tree, diagnostic, hex, mermaid)
- [x] Extension: attachment translated
- [x] Extension: edge translated (`Edges`, `Edgeable`, `validateEdge`, `edgeIsA/source/target/subject`, `edgesMatching`, `addEdgeEnvelope`)
- [x] Extension: compress translated
- [x] Extension: encrypt translated
- [x] Extension: expression translated
- [x] Extension: proof translated
- [x] Extension: recipient translated (including Rust-compat convenience wrappers)
- [x] Extension: salt translated
- [x] Extension: secret translated (`addSecret`, `lock`, `unlock`, `lockSubject`, `unlockSubject`)
- [x] Extension: signature translated (`SignatureMetadata`, `addSignatureOpt` with metadata, `verifyReturningMetadata`, metadata-aware verify helpers)
- [x] Extension: sskr translated (including Rust-compat split/join wrappers)
- [x] Extension: types translated
- [x] Seal module translated (`seal`, `sealOpt`, `unseal`)

## Error Cases
- [x] Base error cases (AlreadyElided, AmbiguousPredicate, InvalidDigest, InvalidFormat, MissingDigest, etc.)
- [x] Edge error cases translated: EdgeMissingIsA, EdgeMissingSource, EdgeMissingTarget, EdgeDuplicateIsA, EdgeDuplicateSource, EdgeDuplicateTarget, EdgeUnexpectedAssertion, NonexistentEdge, AmbiguousEdge
- [x] Secret error case translated: UnknownSecret

## Tests
- [x] Rust unit and integration tests inventoried in `MANIFEST.md` (158 total)
- [x] Core tests translated
- [x] Format/output tests translated
- [x] Crypto/encryption tests translated
- [x] Elision tests translated
- [x] Proof tests translated
- [x] Obscuring tests translated
- [x] Non-correlation tests translated
- [x] Compression tests translated
- [x] Encrypted tests translated
- [x] Attachment tests translated
- [x] Type tests translated
- [x] Expression/function tests translated
- [x] Edge tests translated (`edge_tests.rs`: 44)
- [x] Signature tests translated (`signature_tests.rs`: 3)
- [x] Ed25519 tests translated (`ed25519_tests.rs`: 1)
- [x] Keypair signing tests translated (`keypair_signing_tests.rs`: 2)
- [x] Encapsulation tests translated (`encapsulation_tests.rs`: 1)
- [x] Multi-permit tests translated (`multi_permit_tests.rs`: 1)
- [x] SSH tests translated (`ssh_tests.rs`: 1)
- [x] SSKR tests translated (`sskr_tests.rs`: 1)
- [x] Seal inline tests translated (`seal.rs`: 2)
- [x] Core parity additions translated (`test_unknown_leaf`, `test_true`, `test_false`, `test_unit`, `test_position`)
- [x] Type parity additions translated (`test_fake_random_data`, `test_fake_numbers`)
- [x] Elision parity additions translated (`walk_replace_*` tests)
- [x] Obscuring parity additions translated (`test_nodes_matching`, `test_walk_unelide`, `test_walk_decrypt`, `test_walk_decompress`, `test_mixed_obscuration_operations`)
- [x] Inline envelope parity addition translated (`test_any_envelope`)

## Coverage Summary
- Swift tests currently in package: 158 (25 suites)
- Rust inventory baseline: 158
- Current parity coverage signal: 158/158 (100%)
- Remaining gap: 0 tests

## Build & Config
- [x] .gitignore
- [x] Package.swift
- [x] Swift package builds successfully
- [x] `swift test -Xswiftc -warnings-as-errors` passes (158 tests)

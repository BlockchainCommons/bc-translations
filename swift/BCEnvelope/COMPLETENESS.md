# Completeness: bc-envelope → Swift (BCEnvelope)

## Source Files
- [x] Rust API surface mapped in `MANIFEST.md`

## Public API
- [x] Public exports translated (base, format, extension modules)
- [x] Envelope core type and assertion model translated
- [x] Error model translated (partial — missing edge error cases)
- [x] Envelope encoding/decoding traits and CBOR mappings translated
- [x] Formatting APIs translated (notation, tree, diagnostic, hex, mermaid)
- [x] Extension: attachment translated
- [ ] Extension: edge — NOT translated (Edges container, Edgeable protocol, validate_edge, edge accessors, edges_matching, add_edge_envelope all missing)
- [x] Extension: compress translated
- [x] Extension: encrypt translated
- [x] Extension: expression translated
- [x] Extension: proof translated
- [x] Extension: recipient translated
- [x] Extension: salt translated
- [ ] Extension: secret — NOT translated (add_secret, lock, unlock, lock_subject, unlock_subject all missing)
- [x] Extension: signature translated (partial — missing SignatureMetadata, verify_returning_metadata, add_signature_opt with metadata)
- [x] Extension: sskr translated
- [x] Extension: types translated
- [ ] Seal module — NOT translated (seal, seal_opt, unseal all missing)

## Error Cases
- [x] Base error cases (AlreadyElided, AmbiguousPredicate, InvalidDigest, InvalidFormat, MissingDigest, etc.)
- [ ] Edge error cases missing: EdgeMissingIsA, EdgeMissingSource, EdgeMissingTarget, EdgeDuplicateIsA, EdgeDuplicateSource, EdgeDuplicateTarget, EdgeUnexpectedAssertion, NonexistentEdge, AmbiguousEdge
- [ ] Secret error case missing: UnknownSecret

## Tests
- [x] Rust unit and integration tests inventoried in `MANIFEST.md` (158 total)
- [x] Core tests translated (core_tests: 17, core_encoding_tests: 4, core_nesting_tests: 6)
- [x] Format/output tests translated (format_tests: 12)
- [x] Crypto/encryption tests translated (crypto_tests: 10)
- [x] Elision tests translated (elision_tests: 16)
- [x] Proof tests translated (proof_tests: 3)
- [x] Obscuring tests translated (obscuring_tests: 6)
- [x] Non-correlation tests translated (non_correlation_tests: 3)
- [x] Compression tests translated (compression_tests: 2)
- [x] Encrypted tests translated (encrypted_tests: 1)
- [x] Attachment tests translated (attachment_tests: 1)
- [x] Type tests translated (type_tests: 4)
- [x] Expression/function tests translated (expression + function tests)
- [ ] Edge tests NOT translated (edge_tests: 44 tests)
- [ ] Signature tests NOT translated (signature_tests: 3 tests)
- [ ] Ed25519 tests NOT translated (ed25519_tests: 1 test)
- [ ] Keypair signing tests NOT translated (keypair_signing_tests: 2 tests)
- [ ] Encapsulation tests NOT translated (encapsulation_tests: 1 test)
- [ ] Multi-permit tests NOT translated (multi_permit_tests: 1 test)
- [ ] SSH tests NOT translated (ssh_tests: 1 test)
- [ ] SSKR tests NOT translated (sskr_tests: 1 test)
- [ ] Seal inline tests NOT translated (seal.rs: 2 tests)
- [ ] Inline unit tests NOT translated (envelope.rs: 6, expression.rs: 2, request.rs: 3, response.rs: 4, event.rs: 1, sskr.rs: 1)

## Coverage Summary
- Translated tests: 81 out of 158 Rust tests (51%)
- Missing: 77 tests across 8 untranslated test files + inline tests

## Build & Config
- [x] .gitignore
- [x] Package.swift
- [x] Swift package builds successfully
- [x] All 81 translated Swift tests pass

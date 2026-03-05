# Translation Manifest: bc-envelope 0.43.0 → TypeScript (@bc/envelope)

## Crate Overview

`bc-envelope` provides Gordian Envelope: immutable structured data envelopes over deterministic CBOR with Merkle-like digests and selective-disclosure operations.

Default feature set is broad and includes:
- core envelope data model and deterministic CBOR encoding
- elision, compression, and encryption
- signature and recipient-based cryptography
- expression/request/response/event model
- attachment and edge extensions
- proof generation/verification
- SSKR splitting/joining
- known-value integration and formatting helpers

## External Dependencies

| Rust Crate | TypeScript Equivalent |
|---|---|
| `bc-rand` | `@bc/rand` (sibling package) |
| `bc-crypto` | `@bc/crypto` (sibling package) |
| `dcbor` | `@bc/dcbor` (sibling package) |
| `bc-ur` | `@bc/ur` (sibling package) |
| `bc-components` | `@bc/components` (sibling package) |
| `known-values` | `@bc/known-values` (sibling package) |
| `paste`, `itertools`, `thiserror`, `bytes`, `hex` | Native TS/Node implementations in-package |

## Feature Mapping

All Rust default features are translated as always-enabled in TypeScript for this package release:
- `attachment`, `compress`, `edge`, `ed25519`, `encrypt`, `expression`, `known_value`, `known-values-directory-loading`, `pqcrypto`, `proof`, `recipient`, `salt`, `secp256k1`, `secret`, `signature`, `ssh`, `sskr`, `types`

Note: SSH-specific behavior may remain dependency-gated by the current TypeScript `@bc/components` capability (same constraint observed in other language ports).

## Public API Surface

### Core Types
- `Envelope`
- `EnvelopeCase` (`Node`, `Leaf`, `Wrapped`, `Assertion`, `Elided`, `KnownValue`, `Encrypted`, `Compressed`)
- `Assertion`
- `EnvelopeError`
- `ObscureAction`
- `ObscureType`
- `EdgeType`
- `FormatContext`, `FormatContextOpt`
- `TreeFormatOpts`, `MermaidFormatOpts`, `MermaidOrientation`, `MermaidTheme`, `DigestDisplayFormat`
- `SignatureMetadata`

### Expression/Message Types
- `Function`, `FunctionsStore` and constants (`ADD`, `SUB`, `MUL`, `DIV`, `NEG`, `LT`, `LE`, `GT`, `GE`, `EQ`, `NE`, `AND`, `OR`, `XOR`, `NOT`)
- `Parameter`, `ParametersStore` and constants (`BLANK`, `LHS`, `RHS`)
- `Expression`
- `Request`
- `Response`
- `Event<T>`

### Extension Types
- `Attachments`, `Attachable`
- `Edges`, `Edgeable`

### Core Envelope Method Families
- construction and conversion (`from`, `newAssertion`, `newLeaf`, `newWrapped`, `newElided`, `unknown`, `ok`, etc.)
- assertion management (`addAssertion*`, `removeAssertion`, `replaceAssertion`, `replaceSubject`)
- structural/type queries (`subject`, `assertions`, `is*`, `as*`, `try*`)
- extraction helpers (`extractSubject`, `extractObjectForPredicate`, etc.)
- digest and structural comparison (`digest`, `deepDigests`, `structuralDigest`, `isEquivalentTo`, `isIdenticalTo`)
- walking (`walk`, internal tree/structure walkers)
- wrapping (`wrap`, `unwrap`)
- elision/obscuration and unelision (`elide*`, `unelide`, `walkReplace`, `walkDecrypt`, `walkDecompress`)
- formatting (`format`, `formatFlat`, `treeFormat`, `diagnostic`, `hex`, `mermaidFormat`, `summary`)
- CBOR/UR coding (`taggedCbor`, `untaggedCbor`, `fromTaggedCbor`, `fromUntaggedCbor`, `fromUrString`)

### Extension Method Families
- compression (`compress`, `decompress`, `compressSubject`, `decompressSubject`)
- symmetric encryption (`encryptSubject`, `decryptSubject`, `encrypt`, `decrypt`)
- recipient encryption (`addRecipient`, `recipients`, `encryptSubjectToRecipient(s)`, `decryptSubjectToRecipient`, `encryptToRecipient`, `decryptToRecipient`)
- secret locking (`lockSubject`, `unlockSubject`, `addSecret`, `lock`, `unlock`, `isLockedWithPassword`, `isLockedWithSshAgent`)
- signing (`addSignature*`, `verifySignature*`, `hasSignature*`, `sign`, `verify`, metadata-returning variants)
- SSKR (`sskrSplit`, `sskrSplitFlattened`, `sskrSplitUsing`, `Envelope.sskrJoin`)
- proofs (`proofContainsSet/Target`, `confirmContainsSet/Target`)
- types (`addType`, `types`, `getType`, `hasType`, `checkType`)
- seal (`seal`, `sealOpt`, `unseal`)
- attachments and edges helpers/validation

## Test Inventory (Source of Truth)

Rust integration tests under `rust/bc-envelope/tests/`:
- `core_tests.rs`
- `core_nesting_tests.rs`
- `core_encoding_tests.rs`
- `format_tests.rs`
- `elision_tests.rs`
- `edge_tests.rs`
- `crypto_tests.rs`
- `obscuring_tests.rs`
- `proof_tests.rs`
- `non_correlation_tests.rs`
- `type_tests.rs`
- `signature_tests.rs`
- `compression_tests.rs`
- `keypair_signing_tests.rs`
- `attachment_tests.rs`
- `ed25519_tests.rs`
- `encapsulation_tests.rs`
- `encrypted_tests.rs`
- `multi_permit_tests.rs`
- `ssh_tests.rs`
- `sskr_tests.rs`

Supporting test infrastructure:
- `tests/common/check_encoding.rs`
- `tests/common/test_data.rs`
- `tests/common/test_seed.rs`

Inline module tests in Rust source also need parity behavior coverage (Expression/Request/Response/Event/Seal/SSKR areas).

## EXPECTED TEXT OUTPUT RUBRIC

- Applicable: yes
- Source signals:
  - Rust suite includes extensive format/tree/diagnostic expectations and multi-line rendered structures.
  - Rust tests include `expected-text-output-rubric:` markers in formatting-sensitive cases.
- Target tests to apply:
  - `format`, `core`, `elision`, `proof`, and expression-related rendered outputs where ordering/spacing/punctuation are behavioral.
- Required pattern:
  - single whole-text assertion comparing complete rendered output, with mismatch output showing both actual and expected.

## Translation Units (Dependency Order)

1. Error and base primitives (`error`, `assertion`, `envelope-case`, utility glue)
2. Envelope core structure + constructors + CBOR coders
3. Envelope encodable/extractable conversions
4. Query and assertion manipulation APIs
5. Digest / equivalence / structural-digest logic
6. Walk APIs and traversal edge model
7. Elision/obscuration and walk-based transforms
8. Format context and notation/tree/summary/diagnostic/hex/mermaid
9. Function/parameter stores and expression core
10. Request/response/event
11. Extensions: compression/encryption/recipient/secret/signature/sskr/proof/types/attachment/edge/seal
12. Barrel exports and package-level registration helpers
13. Test infrastructure and translated test suites

## Translation Hazards

- Rust reference-counted internals (`Rc`/`Arc`) map to immutable object semantics in TS; mutation must always return new envelopes.
- Deterministic ordering of assertions by digest is mandatory for encoding and digest parity.
- CBOR encoding forms are strict by case variant (leaf tag wrapping, assertion map shape, elided digest-bytes behavior).
- Generic extraction semantics in Rust/Kotlin require explicit runtime decoding strategy in TS.
- Format output whitespace and punctuation must remain stable; full-text assertions should be used.
- Dependency capability mismatches (notably SSH in `@bc/components`) may force targeted skips/expected failures documented in logs.

## Definition of Completion for This Target

- All planned source units translated in `typescript/bc-envelope/src`.
- Test vectors and output expectations translated and passing under `vitest`.
- Completeness checklist fully checked or explicitly documented with upstream blockers.
- Stage 3 completeness pass reports no unaddressed translation gaps.
- Stage 4 fluency pass applied with test rerun after edits.

# Translation Manifest: bc-envelope

Rust crate version: 0.43.0

## Package Metadata

- Crate name: `bc-envelope`
- Description: `Gordian Envelope for Rust.`
- Edition: 2024
- Repository: https://github.com/BlockchainCommons/bc-envelope-rust

## Public API Surface

### Crate Modules and Re-exports

From `rust/bc-envelope/src/lib.rs` default-feature exports:

1. `pub mod base`
2. `pub mod format`
3. `pub mod extension`
4. `pub mod prelude`
5. `pub mod seal`

Re-exported base items:

- `Assertion`
- `Envelope`
- `EnvelopeCase`
- `EnvelopeEncodable`
- `Error`
- `Result`
- `elide::{self, ObscureAction}`
- `walk::{self, EdgeType}`

Re-exported format items:

- `DigestDisplayFormat`
- `EnvelopeSummary`
- `FormatContext`
- `FormatContextOpt`
- `GLOBAL_FORMAT_CONTEXT`
- `MermaidFormatOpts`
- `MermaidOrientation`
- `MermaidTheme`
- `TreeFormatOpts`
- `register_tags`
- `register_tags_in`

Feature-gated re-exports (all enabled in Rust default features and therefore required for initial translation):

- From `bc-components`: `EncapsulationPrivateKey`, `Encrypter`, `PrivateKeyBase`, `PublicKeys`, `Signer`, `SigningOptions`, `Verifier`
- From `extension`: `SignatureMetadata`, `Attachable`, `Attachments`, `Edgeable`, `Edges`, `Expression`, `ExpressionBehavior`, `IntoExpression`, `Request`, `RequestBehavior`, `Response`, `ResponseBehavior`, `Event`, `EventBehavior`, `Function`, `Parameter`, `functions`, `parameters`
- From `known-values`: `KNOWN_VALUES`, `KnownValue`, `KnownValuesStore`, `known_values`

### Core Public Types

- `Assertion` (struct)
- `Envelope` (struct wrapper around reference-counted case)
- `EnvelopeCase` (enum)
- `Error` (enum)
- `Result<T>` (type alias)
- `EnvelopeEncodable` (trait)
- `EdgeType` (enum)
- `Visitor<'a, State>` (type alias)
- `ObscureType` (enum)
- `ObscureAction` (enum)

### Formatting Public Types

- `FormatContextOpt<'a>` (enum)
- `FormatContext` (struct)
- `LazyFormatContext` (struct)
- `EnvelopeFormatOpts<'a>` (struct)
- `EnvelopeFormat` (trait)
- `EnvelopeFormatItem` (enum)
- `TreeFormatOpts<'a>` (struct)
- `DigestDisplayFormat` (enum)
- `MermaidOrientation` (enum)
- `MermaidTheme` (enum)
- `MermaidFormatOpts<'a>` (struct)
- `HexFormatOpts<'a>` (struct)
- `EnvelopeSummary` (trait)

### Extension Public Types

Attachment:
- `Attachments` (struct)
- `Attachable` (trait)

Edge:
- `Edges` (struct)
- `Edgeable` (trait)

Signature:
- `SignatureMetadata` (struct)

Expressions:
- `FunctionName` (enum)
- `Function` (enum)
- `LazyFunctions` (struct)
- `FunctionsStore` (struct)
- `ParameterName` (enum)
- `Parameter` (enum)
- `LazyParameters` (struct)
- `ParametersStore` (struct)
- `Expression` (struct)
- `ExpressionBehavior` (trait)
- `IntoExpression` (trait)
- `Request` (struct)
- `RequestBehavior` (trait)
- `Response` (struct newtype wrapper)
- `ResponseBehavior` (trait)
- `Event<T>` (struct)
- `EventBehavior<T>` (trait)

SSKR re-exports:
- `SSKRGroupSpec`
- `SSKRSecret`
- `SSKRShare`
- `SSKRSpec`

### Free Public Functions

Formatting/context:
- `register_tags_in(context: &mut FormatContext)`
- `register_tags()`

### Public Method Surface (High-Level)

`Envelope` methods are spread across base/format/extension modules and include:

- Constructors and wrapping (`new`, assertion constructors, wrap/unwrap)
- Assertion mutation and query API
- Type/query/extraction API (`as_*`, `is_*`, extraction helpers)
- CBOR encode/decode and digest semantics
- Elision/obscuration/walk operations
- Notation/diagnostic/tree/hex/mermaid formatting
- Compression/decompression
- Symmetric encryption + recipient encryption
- Signature add/verify APIs
- Proof APIs
- Salt/secret/type helper APIs
- SSKR split/join APIs
- Expression/request/response/event helpers
- Edge and attachment helpers
- Seal/unseal helpers (`seal` module)

## Documentation Catalog

- Crate-level docs: **yes** (`lib.rs`, extensive end-user API guide)
- Module-level docs: **yes** in multiple modules (notably `format/notation.rs`, `format/tree.rs`, expressions modules)
- Public item docs: **partial but substantial** across core/format/extensions
- Public items without docs: present, especially low-level helper types/methods in some modules
- README: **yes** (`rust/bc-envelope/README.md`)
- Package metadata description available from Cargo.toml: **yes**

Translation requirement:
- Preserve and translate Rust docs where present for public Swift API.
- Do not invent docs for Rust-undocumented items.

## Dependencies

### Internal BC Dependencies (must use in-repo Swift translations)

- `dcbor` → `swift/DCBOR`
- `bc-rand` → `swift/BCRand`
- `bc-crypto` → `swift/BCCrypto`
- `bc-components` → `swift/BCComponents`
- `bc-ur` → `swift/BCUR`
- `known-values` (optional in Rust, enabled by default features) → `swift/KnownValues`

### External Rust Dependencies and Swift Equivalents

- `paste`: macro expansion helper; no runtime equivalent needed in Swift
- `hex`: use local hex utilities (`Data`/string helpers)
- `itertools`: Swift standard library collection operations
- `thiserror`: Swift `Error` enums/types
- `bytes`: Swift `Data`
- `ssh-key` (feature-gated): map to existing SSH support in `BCComponents`

## Feature Flags

Rust default features are enabled and must be translated in initial Swift package:

- `attachment`
- `compress`
- `edge`
- `ed25519`
- `encrypt`
- `expression`
- `known_value`
- `known-values-directory-loading`
- `pqcrypto`
- `proof`
- `recipient`
- `salt`
- `secp256k1`
- `secret`
- `signature`
- `ssh`
- `sskr`
- `types`

Non-default feature handling:
- No separate Swift feature flag system is required for this initial translation.
- Implement behavior corresponding to Rust default feature set as always-on Swift API.

## Source File Inventory (Translation Units)

Base module:
- `base/assertion.rs`
- `base/assertions.rs`
- `base/cbor.rs`
- `base/digest.rs`
- `base/elide.rs`
- `base/envelope.rs`
- `base/envelope_decodable.rs`
- `base/envelope_encodable.rs`
- `base/error.rs`
- `base/leaf.rs`
- `base/queries.rs`
- `base/walk.rs`
- `base/wrap.rs`

Format module:
- `format/diagnostic.rs`
- `format/envelope_summary.rs`
- `format/format_context.rs`
- `format/hex.rs`
- `format/mermaid.rs`
- `format/notation.rs`
- `format/tree.rs`

Extension module:
- `extension/attachment/*`
- `extension/compress.rs`
- `extension/edge/*`
- `extension/encrypt.rs`
- `extension/expressions/*`
- `extension/proof.rs`
- `extension/recipient.rs`
- `extension/salt.rs`
- `extension/secret.rs`
- `extension/signature/*`
- `extension/sskr.rs`
- `extension/types.rs`

Other:
- `seal.rs`
- `string_utils.rs`
- `prelude.rs`
- `lib.rs` export surface alignment

## Test Inventory

### Integration Tests (`rust/bc-envelope/tests`)

- `attachment_tests.rs` (1)
- `compression_tests.rs` (2)
- `core_encoding_tests.rs` (4)
- `core_nesting_tests.rs` (6)
- `core_tests.rs` (17)
- `crypto_tests.rs` (10)
- `ed25519_tests.rs` (1)
- `edge_tests.rs` (44)
- `elision_tests.rs` (16)
- `encapsulation_tests.rs` (1)
- `encrypted_tests.rs` (1)
- `format_tests.rs` (12)
- `keypair_signing_tests.rs` (2)
- `multi_permit_tests.rs` (1)
- `non_correlation_tests.rs` (3)
- `obscuring_tests.rs` (6)
- `proof_tests.rs` (3)
- `signature_tests.rs` (3)
- `ssh_tests.rs` (1)
- `sskr_tests.rs` (1)
- `type_tests.rs` (4)

Integration test total: **139**

### Inline Unit Tests (`rust/bc-envelope/src`)

- `base/envelope.rs` (6)
- `extension/expressions/event.rs` (1)
- `extension/expressions/expression.rs` (2)
- `extension/expressions/request.rs` (3)
- `extension/expressions/response.rs` (4)
- `extension/sskr.rs` (1)
- `seal.rs` (2)

Inline test total: **19**

Grand total Rust tests cataloged: **158**

### Test Helpers / Shared Utilities

- `tests/common/check_encoding.rs`
- `tests/common/test_data.rs`
- `tests/common/test_seed.rs`
- `tests/common/mod.rs` (`assert_actual_expected!` helper)

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: **yes**

Source signals:
- Explicit `// expected-text-output-rubric:` markers across many tests.
- Shared helper `assert_actual_expected!` in `tests/common/mod.rs` prints actual/expected full text on mismatch.
- Heavy verification of envelope notation, tree format, diagnostic output, and complex multiline rendering.

Target tests to apply:
- `FormatTests` equivalents (`format_tests.rs`)
- `CoreTests` and `CoreNestingTests` expected text assertions
- `ElisionTests`, `ObscuringTests`, `ProofTests`, `NonCorrelationTests`
- `Crypto/Signature/SSKR/Attachment/Edge` tests where Rust asserts full formatted text

Required pattern:
- Use one full-text assertion for each rendered output case.
- Include mismatch output that shows both actual and expected text.

## Translation Hazards

1. Envelope structural identity vs semantic equivalence:
- Must preserve digest behavior exactly, including sorting and assertion ordering invariants.

2. Reference-counted envelope internals:
- Rust uses reference-counted internal structure; Swift implementation must preserve value semantics + digest determinism.

3. Feature-gated extension sprawl:
- Default Rust build includes many extensions; Swift translation must expose all default-surface APIs together.

4. Known values integration:
- Must align with `KnownValues` constants and registry behavior used by formatting and query APIs.

5. Text rendering determinism:
- Formatting outputs are canonicalized in tests; whitespace/punctuation/order differences will fail vectors.

6. Cryptographic interoperability:
- Encryption/signature/recipient/SSKR test vectors depend on deterministic seeds/nonces and exact byte outputs.

7. Edge extension parity:
- `edge_tests.rs` is extensive and validates envelope-level, container-level, filtering, validation, and UR round-trip behavior.

8. Walk/elide transformations:
- Recursive structure rewrites must preserve digest tree consistency.

9. Error taxonomy mapping:
- Rust error enum is broad; Swift error types must remain semantically equivalent for test behavior.

10. Macro-generated / helper-generated behavior:
- Rust macros and trait blanket impls must be translated into explicit Swift implementations.

## Translation Unit Order (Planner)

1. Scaffold/package files and export wiring (`Package.swift`, module export file)
2. Core error/protocol primitives (`error`, encodable/decodable traits)
3. Base envelope data model (`EnvelopeCase`, `Envelope`, `Assertion`, `leaf`, `digest`)
4. Core operations (`assertions`, `queries`, `wrap`, `walk`, `elide`, `cbor`)
5. Formatting core (`notation`, `tree`, `hex`, `diagnostic`, summary/context)
6. Extensions: attachment/compress/encrypt/recipient/signature/secret/salt/types/proof
7. Extensions: expressions subsystem
8. Extensions: sskr and edge
9. `seal` module
10. Prelude and public re-export alignment
11. Test helper translation
12. Test translation in file order (core → format → crypto → advanced extensions)
13. Build + test iteration loop until green


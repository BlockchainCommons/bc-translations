# Completeness: bc-envelope → C# (BCEnvelope)

## Source Files
- [x] EnvelopeException.cs — error types (all ~30 variants translated)
- [x] EnvelopeCase.cs — sealed hierarchy (8 variants)
- [x] Assertion.cs — predicate + object with digest
- [x] Envelope.cs — core type, constructors, leaf helpers, queries, digest ops, wrap/unwrap, equality, position
- [x] EnvelopeExtensions.cs — encoding (replaces EnvelopeEncodable.cs + EnvelopeDecodable.cs; decoding/extraction in Envelope.cs)
- [x] EnvelopeCbor.cs — CBOR serialization per case (ICborTaggedEncodable, ICborTaggedDecodable)
- [x] EdgeType.cs — walk edge labels (6 variants)
- [x] EnvelopeWalk.cs — visitor/walk traversal (Walk, WalkStructure, WalkTree)
- [x] ObscureAction.cs — elide/encrypt/compress action
- [x] ObscureType.cs — elided/encrypted/compressed type
- [x] EnvelopeElide.cs — elision & obscuration methods (all variants with/without action)
- [x] EnvelopeAssertions.cs — assertion management methods (add, remove, replace, salted, optional, conditional)
- [x] StringUtils.cs — FlankedBy helper
- [x] FormatContext.cs — format context + global state + tag registration
- [x] FormatContextOpt.cs — format context options
- [x] EnvelopeNotation.cs — notation format rendering (Format, FormatFlat, FormatOpt)
- [x] TreeFormatOpts.cs — tree format options
- [x] EnvelopeTreeFormat.cs — tree format methods
- [x] EnvelopeSummary.cs — summary rendering
- [x] DigestDisplayFormat.cs — digest display enum
- [x] EnvelopeDiagnostic.cs — diagnostic + hex format
- [x] MermaidFormatOpts.cs — mermaid options (MermaidOrientation, MermaidTheme)
- [x] EnvelopeMermaid.cs — mermaid diagram output
- [x] EnvelopeEncrypt.cs — symmetric encryption (EncryptSubject, DecryptSubject, Encrypt, Decrypt)
- [x] EnvelopeCompress.cs — compression (Compress, Decompress, CompressSubject, DecompressSubject)
- [x] EnvelopeSalt.cs — salt decorrelation (AddSalt, AddSaltInstance, AddSaltWithLen, AddSaltInRange + _Using variants)
- [x] SignatureMetadata.cs — signature metadata
- [x] EnvelopeSignature.cs — digital signing (all add/verify/has methods + Sign, SignOpt, Verify, VerifyReturningMetadata)
- [x] EnvelopeRecipient.cs — public key encryption (AddRecipient, EncryptSubjectToRecipients, DecryptSubjectToRecipient, etc.)
- [x] EnvelopeSecret.cs — password/SSH locking (LockSubject, UnlockSubject, Lock, Unlock, AddSecret, IsLockedWithPassword, IsLockedWithSshAgent)
- [x] EnvelopeSskr.cs — SSKR split/join
- [x] EnvelopeProof.cs — inclusion proofs (ProofContainsSet, ProofContainsTarget, ConfirmContainsSet, ConfirmContainsTarget)
- [x] EnvelopeTypes.cs — type assertions (AddType, Types, GetType, HasType, HasTypeValue, CheckTypeValue, CheckType)
- [x] Attachments.cs — attachment container
- [x] IAttachable.cs — attachable interface
- [x] EnvelopeAttachment.cs — attachment methods
- [x] Edges.cs — edge container
- [x] IEdgeable.cs — edgeable interface
- [x] EnvelopeEdge.cs — edge methods
- [x] Function.cs — expression function type (Known, Named + CBOR)
- [x] FunctionsStore.cs — function registry + well-known constants (ADD through NOT)
- [x] Parameter.cs — expression parameter type (Known, Named + CBOR)
- [x] ParametersStore.cs — parameter registry + well-known constants (BLANK, LHS, RHS)
- [x] Expression.cs — expression type + ExpressionBehavior methods
- [x] Request.cs — request type + RequestBehavior methods
- [x] Response.cs — response type + ResponseBehavior methods
- [x] Event.cs — event type + EventBehavior methods
- [x] EnvelopeExpressions.cs — expression helper methods (AddAssertionIf, Ok, Unknown)
- [x] EnvelopeSeal.cs — seal/unseal operations

## Tests — Integration (139/139)
- [x] TestHelpers.cs — CheckEncoding extension method
- [x] TestData.cs — test data factories (hello, assertion, credential, key material)
- [x] TestSeed.cs — Seed domain object example
- [x] CoreTests.cs — 17/17 tests
- [x] CoreNestingTests.cs — 6/6 tests
- [x] CoreEncodingTests.cs — 4/4 tests
- [x] FormatTests.cs — 12/12 tests
- [x] ElisionTests.cs — 16/16 tests
- [x] EdgeTests.cs — 44/44 tests
- [x] CryptoTests.cs — 10/10 tests
- [x] ObscuringTests.cs — 6/6 tests
- [x] ProofTests.cs — 3/3 tests
- [x] NonCorrelationTests.cs — 3/3 tests
- [x] TypeTests.cs — 4/4 tests
- [x] SignatureTests.cs — 3/3 tests
- [x] CompressionTests.cs — 2/2 tests
- [x] KeypairSigningTests.cs — 2/2 tests
- [x] AttachmentTests.cs — 1/1 test
- [x] Ed25519Tests.cs — 1/1 test
- [x] EncapsulationTests.cs — 1/1 test
- [x] EncryptedTests.cs — 1/1 test
- [x] MultiPermitTests.cs — 1/1 test
- [x] SshTests.cs — 1/1 test
- [x] SskrTests.cs — 1/1 test

## Tests — Inline (19/19 translated)

- [x] InlineTests.cs — 19/19 translated inline tests covering:
  - envelope.rs (6): constructor equivalence (leaf, known value, assertion, encrypted, compressed, cbor)
  - expression.rs (2): expression creation and parsing
  - request.rs (3): basic request, request with metadata, parameter format
  - response.rs (4): success OK, success result, early failure, failure
  - event.rs (1): event creation and parsing
  - sskr.rs (1): SSKR split and join inline
  - seal.rs (2): seal/unseal, seal with options
- [x] InlineTests.cs — 2 additional C# regression tests for generic envelope conversion helpers (`TryAs`, `TryObjectForPredicate`, `TryOptionalObjectForPredicate`, `TryObjectsForPredicate`)

## API Gaps (Minor)

- [x] `TryAs<T>()` — TryFrom<Envelope>-based extraction
- [x] `TryObjectForPredicate<T>()` — TryFrom<Envelope>-based object extraction
- [x] `TryOptionalObjectForPredicate<T>()` — optional variant
- [x] `TryObjectsForPredicate<T>()` — multi-result variant

No remaining public API omissions were found in this pass.

Note: `SigningOptions` is from BCComponents, not BCEnvelope (correctly not a separate file here).
Note: `ExpressionBehavior`, `RequestBehavior`, `ResponseBehavior`, `EventBehavior` are Rust traits that are integrated into the C# classes directly rather than as separate interfaces (Expression, Request, Response, Event each implement their own behavior inline). This is idiomatic C#.

## Build & Config
- [x] .gitignore
- [x] BCEnvelope.csproj (with package description)
- [x] BCEnvelope.Tests.csproj
- [x] BCEnvelope.slnx

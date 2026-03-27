# Completeness: bc-envelope → Go (bcenvelope)

Checked: 2026-03-27. All 156 tests pass.

## Source Files — Base
- [x] envelope.go — Main Envelope type, EnvelopeCase sealed interface, 8 case types, constructors
- [x] assertion.go — Assertion type (predicate-object pairs), CBOR encoding/decoding
- [x] assertions.go — Add/remove/replace assertions, salted variants, conditional
- [x] cbor.go — CBOR encoding/decoding for Envelope (tagged/untagged)
- [x] digest.go — Digests, DeepDigests, ShallowDigests, StructuralDigest, IsEquivalentTo, IsIdenticalTo
- [x] elide.go — Elision, ObscureAction, ObscureType, all set/array/target variants (including WithAction), WalkUnelide, WalkReplace, WalkDecrypt, WalkDecompress, NodesMatching
- [x] envelope_encodable.go — EnvelopeEncodable interface, AsEnvelopeEncodable type switch, wrapper types, convenience constructors
- [x] extract.go — ExtractSubject generic, ExtractObjectForPredicate, type-specific helpers
- [x] error.go — 35+ sentinel errors covering all Rust Error variants
- [x] queries.go — Subject/Assertions/type checks/predicate queries/ElementsCount/leaf helpers/known-value/position/Digest method (leaf.rs integrated here)
- [x] walk.go — EdgeType, Walk, WalkGeneric with type-safe visitor
- [x] wrap.go — Wrap, TryUnwrap, Unwrap

## Source Files — Extensions
- [x] types.go — AddType, Types, GetType, HasType, HasTypeValue, CheckTypeValue, CheckType
- [x] recipient.go — AddRecipient(Opt), Recipients, EncryptSubjectToRecipient(s)(Opt), DecryptSubjectToRecipient, EncryptToRecipient, DecryptToRecipient
- [x] salt.go — AddSalt, AddSaltUsing, AddSaltInstance, AddSaltWithLen(Using), AddSaltInRange(Using)
- [x] secret.go — LockSubject, UnlockSubject, IsLockedWithPassword, IsLockedWithSSHAgent, AddSecret, Lock, Unlock
- [x] encrypt.go — EncryptSubject, EncryptSubjectWithNonce, DecryptSubject, Encrypt, Decrypt
- [x] compress.go — Compress, Decompress, CompressSubject, DecompressSubject
- [x] proof.go — ProofContainsSet, ProofContainsTarget, ConfirmContainsSet, ConfirmContainsTarget, DigestProvider interface
- [x] sskr.go — SSKRSplit, SSKRSplitFlattened, SSKRSplitUsing, SSKRJoin
- [x] signature.go — AddSignature(Opt), AddSignatures(Opt), MakeSignedAssertion, IsVerifiedSignature, VerifySignature, HasSignatureFrom(ReturningMetadata), VerifySignatureFrom(ReturningMetadata), HasSignaturesFrom(Threshold), VerifySignaturesFrom(Threshold), Sign(Opt), Verify, VerifyReturningMetadata
- [x] signature_metadata.go — SignatureMetadata: New, NewWithAssertions, Assertions, AddAssertion, WithAssertion, HasAssertions
- [x] attachment.go — NewAttachmentAssertion/Envelope, AddAttachment, AttachmentPayload/Vendor/ConformsTo, Attachments, AttachmentsWithVendorAndConformsTo, ValidateAttachment, AttachmentsContainer, Attachable interface
- [x] edge.go — AddEdgeEnvelope, Edges, ValidateEdge, EdgeIsA/Source/Target/Subject, EdgesMatching, EdgesContainer, Edgeable interface
- [x] expression.go — Expression: New, FromString, WithParameter(CBOR), WithOptionalParameter, ObjectForParameter, ExtractObjectForParameter, ExpressionFromEnvelope(Expecting)
- [x] function.go — Function: NewKnown(Function), NewNamed(Function), IsKnown/IsNamed, Value, Name, NamedName, Equal, CBOR encode/decode
- [x] functions.go — 15 well-known function constants (Add through Not)
- [x] functions_store.go — FunctionsStore: New, Insert, AssignedName, Name, NameForFunction, Clone, GlobalFunctions
- [x] parameter.go — Parameter: NewKnown(Parameter), NewNamed(Parameter), IsKnown/IsNamed, Value, Name, Equal, CBOR encode/decode
- [x] parameters.go — 3 well-known parameter constants (Blank, LHS, RHS)
- [x] parameters_store.go — ParametersStore: New, Insert, AssignedName, Name, NameForParameter, Clone, GlobalParameters
- [x] request.go — Request: New, NewFromString, NewWithBody, Body, ID, Function, WithParameter, WithNote/Date, ToEnvelope, RequestFromEnvelope(Expecting), Summary
- [x] response.go — Response: NewSuccess, NewFailure, NewEarlyFailure, Result, Error, WithResult(CBOR), WithError(CBOR), ToEnvelope, ResponseFromEnvelope, UnknownEnvelope, OKEnvelope
- [x] event.go — Event: New, NewFromCBOR/String, Content, ID, WithNote/Date, ToEnvelope, EventFromEnvelope, Summary
- [x] seal.go — Seal, SealOpt, Unseal

## Source Files — Formatting
- [x] notation.go — Format, FormatFlat, FormatOpt, formatItem tree, String()
- [x] tree_format.go — TreeFormat, TreeFormatOpt, TreeFormatOpts, ShortID, DigestDisplayFormat
- [x] mermaid.go — MermaidFormat, MermaidFormatOpt, MermaidFormatOpts, MermaidOrientation, MermaidTheme
- [x] diagnostic.go — Diagnostic, DiagnosticAnnotated
- [x] hex_format.go — Hex, HexOpt, HexFormatOpts
- [x] format_context.go — FormatContext, FormatContextOpt, NewFormatContext, WithFormatContext(Mut), RegisterTags(In), global context with sync.Mutex
- [x] envelope_summary.go — Summary method
- [x] string_utils.go — FlankedBy helper

## Tests — Integration (21 test files, 139 tests)
- [x] core_test.go — 17 tests (matches Rust: 17)
- [x] core_encoding_test.go — 4 tests (matches Rust: 4)
- [x] core_nesting_test.go — 6 tests (matches Rust: 6)
- [x] format_test.go — 12 tests (matches Rust: 12)
- [x] type_test.go — 4 tests (matches Rust: 4)
- [x] signature_test.go — 3 tests (matches Rust: 3)
- [x] ed25519_test.go — 1 test (matches Rust: 1)
- [x] crypto_test.go — 10 tests (matches Rust: 10)
- [x] encrypted_test.go — 1 test (matches Rust: 1)
- [x] compression_test.go — 2 tests (matches Rust: 2)
- [x] elision_test.go — 16 tests (matches Rust: 16)
- [x] edge_test.go — 44 tests (matches Rust: 44)
- [x] attachment_test.go — 1 test (matches Rust: 1)
- [x] sskr_test.go — 1 test (matches Rust: 1)
- [x] proof_test.go — 3 tests (matches Rust: 3)
- [x] non_correlation_test.go — 3 tests (matches Rust: 3)
- [x] obscuring_test.go — 6 tests (matches Rust: 6)
- [x] encapsulation_test.go — 1 test (matches Rust: 1)
- [x] keypair_signing_test.go — 2 tests (matches Rust: 2)
- [x] ssh_test.go — 1 test (matches Rust: 1)
- [x] multi_permit_test.go — 1 test (matches Rust: 1)

## Tests — Inline (17 tests from Rust source files)
- [x] inline_test.go — 17 tests covering:
  - envelope.rs: TestAnyEnvelope, TestAnyKnownValue, TestAnyAssertion, TestAnyCompressed, TestAnyCBOREncodable (5 tests; test_any_encrypted skipped as todo in Rust)
  - expression.rs: TestExpression1, TestExpression2 (2 tests)
  - request.rs: TestBasicRequest, TestRequestWithMetadata, TestParameterFormat (3 tests)
  - response.rs: TestSuccessOk, TestSuccessResult, TestEarlyFailure, TestFailure (4 tests)
  - event.rs: TestEvent (1 test)
  - seal.rs: TestSealAndUnseal, TestSealOptWithOptions (2 tests)

## Build & Config
- [x] .gitignore
- [x] go.mod
- [x] test_helpers_test.go — assertActualExpected, checkEncoding, test data helpers

## Bugs Fixed During Check
- **Request/Response/Event ARID encoding**: `ToEnvelope()` was using `UntaggedCBOR()` for ARID/KnownValue subjects inside tagged wrappers, should use `TaggedCBOR()` to match Rust's `CBOR::to_tagged_value(tag, item)` semantics. Fixed in request.go, response.go, event.go (both encoding and decoding paths).

## Summary
- **Integration tests**: 139 of 139 translated (100%)
- **Inline tests**: 17 of 18 non-trivial Rust inline tests translated (1 skipped: test_any_encrypted is a todo in Rust)
- **Public API**: All public types, methods, constants, and convenience wrappers translated
- **All 156 tests pass**

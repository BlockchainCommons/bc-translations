# Translation Manifest: bc-envelope 0.43.0 → Go

## Crate Overview

Gordian Envelope for Rust. A hierarchical binary data format built on deterministic CBOR (dCBOR) with a Merkle-like digest tree. Supports selective disclosure via elision, encryption, and compression; digital signatures; SSKR social recovery; expression-based RPC; metadata attachments; typed edges; and inclusion proofs. All default features enabled.

Total source: ~12,000 lines (base ~5,100, format ~1,870, extensions ~5,000+).

## External Dependencies

| Rust Crate | Go Equivalent |
|---|---|
| bc-rand | `github.com/nickel-blockchaincommons/bcrand-go` (sibling) |
| bc-crypto | `github.com/nickel-blockchaincommons/bccrypto-go` (sibling) |
| dcbor | `github.com/nickel-blockchaincommons/dcbor-go` (sibling) |
| bc-ur | `github.com/nickel-blockchaincommons/bcur-go` (sibling) |
| bc-components | `github.com/nickel-blockchaincommons/bccomponents-go` (sibling) |
| known-values | `github.com/nickel-blockchaincommons/knownvalues-go` (sibling) |
| paste | Not needed (Go has no macro system; use plain constants) |
| hex | `encoding/hex` (stdlib) |
| itertools | `slices`, `maps` packages (Go 1.21+ stdlib) or inline loops |
| thiserror | Go error values and `fmt.Errorf` / sentinel errors |
| bytes | `[]byte` (Go native) |
| ssh-key | Use bc-components SSH support (already translated) |
| hex-literal (dev) | `encoding/hex.DecodeString()` in test helpers |
| lazy_static (dev) | `sync.Once` + package-level `var` |
| indoc (dev) | Raw string literals with backticks |

## Feature Mapping

| Rust Feature | Default? | Go Approach |
|---|---|---|
| attachment | Yes | Always compiled (no conditional compilation in Go) |
| compress | Yes | Always compiled |
| edge | Yes | Always compiled |
| ed25519 | Yes | Always compiled (bccrypto/bccomponents have ed25519) |
| encrypt | Yes | Always compiled |
| expression | Yes | Always compiled |
| known_value | Yes | Always compiled |
| known-values-directory-loading | Yes | Always compiled |
| multithreaded | Yes | N/A (Go is always concurrent; goroutine-safe by design) |
| pqcrypto | Yes | Always compiled (bccomponents has ML-KEM/ML-DSA) |
| proof | Yes | Always compiled |
| recipient | Yes | Always compiled |
| salt | Yes | Always compiled |
| secp256k1 | Yes | Always compiled (bccrypto/bccomponents have secp256k1) |
| secret | Yes | Always compiled |
| signature | Yes | Always compiled |
| ssh | Yes | Always compiled (bccomponents has SSH support) |
| sskr | Yes | Always compiled |
| types | Yes | Always compiled |

## Public API Surface

### Core Types

| Rust Type | Go Type | Notes |
|---|---|---|
| `Envelope` (struct wrapping `RefCounted<EnvelopeCase>`) | `*Envelope` (pointer to struct) | Go GC handles reference counting; pointer for identity semantics |
| `EnvelopeCase` (enum: 8 variants) | `EnvelopeCase` (interface with sealed method) | Node, Leaf, Wrapped, Assertion, Elided, KnownValue, Encrypted, Compressed |
| `Assertion` (struct: predicate, object, digest) | `Assertion` (struct) | Digest computed from predicate+object digests |
| `Error` (thiserror enum, ~30 variants) | `EnvelopeError` type + sentinel vars | `var ErrAlreadyElided = errors.New(...)` pattern |
| `Result<T>` (type alias) | `(T, error)` multi-return | Go error handling convention |
| `ObscureAction` (enum: Elide, Encrypt, Compress) | `ObscureAction` (interface) | Encrypt variant carries SymmetricKey |
| `ObscureType` (enum) | `ObscureType` (int const with iota) | |
| `EdgeType` (enum) | `EdgeType` (int const with iota) | Walk traversal edge labels |
| `FormatContext` | `FormatContext` (struct) | Global singleton + custom instances |
| `FormatContextOpt` (enum) | Not needed (use `*FormatContext` — nil for default) | |
| `TreeFormatOpts` | `TreeFormatOpts` (struct) | |
| `MermaidFormatOpts` | `MermaidFormatOpts` (struct) | |
| `MermaidOrientation` (enum) | `MermaidOrientation` (int const) | |
| `MermaidTheme` (enum) | `MermaidTheme` (int const) | |
| `DigestDisplayFormat` (enum) | `DigestDisplayFormat` (int const) | |
| `EnvelopeSummary` | `EnvelopeSummary` (struct) | |
| `SignatureMetadata` | `SignatureMetadata` (struct) | |
| `SigningOptions` | `SigningOptions` (struct/interface) | |

### Expression Types

| Rust Type | Go Type | Notes |
|---|---|---|
| `Expression` (struct) | `Expression` (struct) | |
| `ExpressionBehavior` (trait) | Methods on `*Envelope` | Go doesn't use trait objects for this |
| `Function` (enum: Known, Named) | `Function` (struct with variant field) | CBOR tag #6.40006 |
| `FunctionsStore` | `FunctionsStore` (struct) | |
| `Parameter` (enum: Known, Named) | `Parameter` (struct with variant field) | CBOR tag #6.40007 |
| `ParametersStore` | `ParametersStore` (struct) | |
| `Request` (struct) | `Request` — methods on `*Envelope` | CBOR tag #6.40010 |
| `RequestBehavior` (trait) | Methods on `*Envelope` | |
| `Response` (struct) | `Response` — methods on `*Envelope` | CBOR tag #6.40011 |
| `ResponseBehavior` (trait) | Methods on `*Envelope` | |
| `Event` (generic struct) | Methods on `*Envelope` | CBOR tag #6.40012 |
| `EventBehavior<T>` (trait) | Functions taking `*Envelope` | Go lacks generics on methods with type params in receivers |

### Attachment & Edge Types

| Rust Type | Go Type | Notes |
|---|---|---|
| `Attachments` (struct) | `Attachments` (struct) | map[Digest]*Envelope container |
| `Attachable` (trait) | Envelope methods directly | No separate interface needed |
| `Edges` (struct) | `Edges` (struct) | map[Digest]*Envelope container |
| `Edgeable` (trait) | Envelope methods directly | No separate interface needed |

### Traits → Go Interfaces/Methods

| Rust Trait | Go Approach | Notes |
|---|---|---|
| `EnvelopeEncodable` | `EnvelopeEncodable` interface: `Envelope() *Envelope` | Types implement this to convert to envelope |
| `DigestProvider` | `DigestProvider` (from bccomponents) | Already exists |
| `CBORTagged` | `CBORTagged` (from dcbor) | Already exists |
| `CBORTaggedEncodable` | `CBORTaggedEncodable` (from dcbor) | Already exists |
| `CBORTaggedDecodable` | `CBORTaggedDecodable` (from dcbor) | Already exists |

### EnvelopeEncodable Implementations

Types that can convert to `*Envelope` via the `EnvelopeEncodable` interface or helper functions:

- Primitives: `uint8`, `uint16`, `uint32`, `uint64`, `int8`, `int16`, `int32`, `int64`, `bool`, `float32`, `float64`
- Strings: `string`
- Binary: `dcbor.ByteString`, `[]byte`
- CBOR: `*dcbor.CBOR`
- bc-components types: `*Digest`, `*Salt`, `*Nonce`, `*ARID`, `*URI`, `*UUID`, `*XID`, `*Reference`, `*Date`, `*JSON`
- Crypto types: `*PublicKeys`, `*PrivateKeys`, `*PrivateKeyBase`, `*SealedMessage`, `*EncryptedKey`, `*Signature`, `*SSKRShare`
- Envelope types: `*Assertion`, `*KnownValue`, `*Function`, `*Parameter`

Use `NewEnvelope(subject)` with type switch for dispatch, plus `NewEnvelopeFromCBOR()`, `NewEnvelopeFromBytes()`, etc. for specific types.

### Core Envelope Methods (~200+ methods/functions)

#### Construction
- `NewEnvelope(subject)` — create with any EnvelopeEncodable
- `NewEnvelopeWithAssertion(predicate, object)` — create bare assertion
- `NewEnvelopeOrNull(subject)` / `NewEnvelopeOrNone(subject)` — optional creation
- `NewLeafEnvelope(cbor)` — create from CBOR
- `NewWrappedEnvelope(envelope)` — wrap an envelope
- `NewElidedEnvelope(digest)` — elided placeholder
- `NewKnownValueEnvelope(kv)` — known-value subject
- `NewEncryptedEnvelope(msg)` — encrypted subject
- `NewCompressedEnvelope(comp)` — compressed subject
- `NullEnvelope()`, `TrueEnvelope()`, `FalseEnvelope()`, `UnitEnvelope()` — static factories

#### Assertion Management
- `AddAssertion(predicate, object)` → `*Envelope`
- `AddAssertionEnvelope(assertion)` → `*Envelope`
- `AddAssertionEnvelopes(assertions)` → `*Envelope`
- `AddOptionalAssertionEnvelope(assertion)` → `*Envelope`
- `AddOptionalAssertion(predicate, object)` → `*Envelope`
- `AddNonemptyStringAssertion(predicate, string)` → `*Envelope`
- `AddAssertions(predicate, objects)` → `*Envelope`
- `AddAssertionIf(condition, predicate, object)` → `*Envelope`
- `AddAssertionEnvelopeIf(condition, assertion)` → `*Envelope`
- `RemoveAssertion(assertion)` → `(*Envelope, error)`
- `ReplaceAssertion(old, new)` → `(*Envelope, error)`
- `ReplaceSubject(newSubject)` → `*Envelope`

#### Salted Assertions
- `AddAssertionSalted(predicate, object)` → `*Envelope`
- `AddAssertionEnvelopeSalted(assertion)` → `*Envelope`
- `AddOptionalAssertionEnvelopeSalted(assertion)` → `*Envelope`
- `AddAssertionsSalted(predicate, objects)` → `*Envelope`
- Internal `Using` variants for deterministic RNG testing

#### Structural Queries
- `Subject()` → `*Envelope`
- `Assertions()` → `[]*Envelope`
- `HasAssertions()` → `bool`
- `IsAssertion()` / `IsEncrypted()` / `IsCompressed()` / `IsElided()` / `IsLeaf()` / `IsNode()` / `IsWrapped()` / `IsKnownValue()`
- `IsSubjectAssertion()` / `IsSubjectEncrypted()` / `IsSubjectCompressed()` / `IsSubjectElided()` / `IsSubjectObscured()`
- `AsAssertion()` → `(*Assertion, error)`
- `AsPredicate()` → `(*Envelope, error)`
- `AsObject()` → `(*Envelope, error)`
- `AsLeaf()` → `(*dcbor.CBOR, error)`
- `ElementsCount()` → `int`

#### Content Extraction
- `ExtractSubject[T](envelope)` → `(T, error)` (generic function)
- `ObjectForPredicate(predicate)` → `(*Envelope, error)`
- `ObjectsForPredicate(predicate)` → `[]*Envelope`
- `AssertionsWithPredicate(predicate)` → `[]*Envelope`
- `AssertionWithPredicate(predicate)` → `(*Envelope, error)`
- `ExtractObjectForPredicate[T](envelope, predicate)` → `(T, error)`
- `ExtractOptionalObjectForPredicate[T](envelope, predicate)` → `(*T, error)`
- `ExtractObjectForPredicateWithDefault[T](envelope, predicate, default)` → `(T, error)`
- `ExtractObjectsForPredicate[T](envelope, predicate)` → `([]T, error)`

#### Leaf Helpers
- `IsNull()` / `IsTrue()` / `IsFalse()` / `IsBool()` / `IsNumber()` / `IsNaN()`
- `IsSubjectNumber()` / `IsSubjectNaN()`
- `ByteString()` → `(dcbor.ByteString, error)`
- `AsArray()` / `AsMap()` / `AsText()`

#### Known Value Helpers
- `AsKnownValue()` → `(*KnownValue, error)`
- `IsKnownValue()` → `bool`
- `IsSubjectUnit()` / `CheckSubjectUnit()`

#### Digest Operations
- `Digest()` → `*Digest` (DigestProvider interface)
- `Digests(levelLimit)` → `DigestSet`
- `DeepDigests()` → `DigestSet`
- `ShallowDigests()` → `DigestSet`
- `StructuralDigest()` → `*dcbor.CBOR`
- `IsEquivalentTo(other)` → `bool`
- `IsIdenticalTo(other)` → `bool`

#### Wrapping
- `Wrap()` → `*Envelope`
- `Unwrap()` → `(*Envelope, error)`

#### Elision & Obscuration
- `Elide()` → `*Envelope`
- `ElideSetWithAction(set, removing, action)` → `*Envelope`
- `ElideRemovingSet(set)` / `ElideRevealingSet(set)` → `*Envelope`
- `ElideRemovingArray(array)` / `ElideRevealingArray(array)` → `*Envelope`
- `ElideRemovingTarget(target)` / `ElideRevealingTarget(target)` → `*Envelope`
- Variants with `WithAction` suffix for Encrypt/Compress obscuration
- `Unelide(original)` → `(*Envelope, error)`
- `NodesMatching(set, types)` → `DigestSet`
- `WalkUnelide(lookupSet)` → `*Envelope`
- `WalkReplace(targetDigests, replacement)` → `*Envelope`
- `WalkDecrypt(keys)` → `(*Envelope, error)`
- `WalkDecompress(targets)` → `(*Envelope, error)`

#### Walk/Visitor
- `Walk(hideNodes, state, visit)` — visitor pattern
- Visitor type: `func(envelope *Envelope, level int, edgeType EdgeType, state S) (S, bool)`

#### Encryption
- `EncryptSubject(key)` → `(*Envelope, error)`
- `EncryptSubjectWithNonce(key, nonce)` → `(*Envelope, error)`
- `DecryptSubject(key)` → `(*Envelope, error)`
- `Encrypt(key)` / `Decrypt(key)` — wrap+encrypt / decrypt+unwrap convenience

#### Compression
- `Compress()` → `(*Envelope, error)`
- `Decompress()` → `(*Envelope, error)`
- `CompressSubject()` → `(*Envelope, error)`
- `DecompressSubject()` → `(*Envelope, error)`

#### Salt
- `AddSalt()` → `*Envelope`
- `AddSaltInstance(salt)` → `*Envelope`
- `AddSaltWithLen(count)` → `*Envelope`
- `AddSaltInRange(min, max)` → `*Envelope`

#### Signature
- `AddSignature(signer)` / `AddSignatureOpt(signer, options)` → `(*Envelope, error)`
- `AddSignatures(signers)` / `AddSignaturesOpt(signers, options)` → `(*Envelope, error)`
- `IsVerifiedSignature(verifier)` → `bool`
- `VerifySignature(verifier)` → `(*Envelope, error)`
- `HasSignatureFrom(verifier)` → `(bool, error)`
- `HasSignaturesFrom(verifiers)` → `(bool, error)`
- `HasSignaturesFromThreshold(verifiers, threshold)` → `(bool, error)`
- `VerifySignatureFrom(verifier)` → `(*Envelope, error)`
- `VerifySignaturesFrom(verifiers)` → `(*Envelope, error)`
- `VerifySignaturesFromThreshold(verifiers, threshold)` → `(*Envelope, error)`
- `Sign(signer)` / `SignOpt(signer, options)` → `(*Envelope, error)`
- `Verify(verifier)` → `(*Envelope, error)`
- Metadata-aware variants returning `(*Envelope, *SignatureMetadata, error)`

#### Recipient
- `AddRecipient(recipient)` → `*Envelope`
- `Recipients()` → `[]*SealedMessage`
- `EncryptSubjectToRecipients(recipients, key)` → `*Envelope`
- `EncryptSubjectToRecipient(recipient, key)` → `*Envelope`
- `DecryptSubjectToRecipient(recipient)` → `(*Envelope, error)`
- `EncryptToRecipient(recipient, key)` / `DecryptToRecipient(recipient)` — convenience

#### Secret
- `LockSubject(password)` / `UnlockSubject(password)` → `(*Envelope, error)`
- `Lock(password)` / `Unlock(password)` → `(*Envelope, error)`
- `AddSecret(methodSelector, plaintext)` — low-level
- `IsLockedWithPassword()` / `IsLockedWithSSHAgent()` → `bool`

#### SSKR
- `SSKRSplit(spec, key)` → `([][]*Envelope, error)`
- `SSKRSplitFlattened(threshold, shareCount, key)` → `([]*Envelope, error)`
- `SSKRJoin(shares)` → `(*Envelope, error)` (package-level function)

#### Proof
- `ProofContainsSet(set)` → `*Envelope`
- `ProofContainsTarget(target)` → `*Envelope`
- `ConfirmContainsSet(set)` → `(*Envelope, error)`
- `ConfirmContainsTarget(target)` → `(*Envelope, error)`

#### Types
- `AddType(typeValue)` / `Types()` / `GetType()` / `HasType()` / `HasTypeValue()` / `CheckTypeValue()` / `CheckType()`

#### Position
- `SetPosition(position)` / `Position()` / `RemovePosition()`

#### Seal
- `Seal(signer, recipient)` / `SealOpt(signer, recipient, options, key)` → `(*Envelope, error)`
- `Unseal(recipient, verifier)` → `(*Envelope, error)`

#### Formatting
- `Format()` → `string` — envelope notation
- `FormatOpt(context)` → `string` — with custom context
- `FormatFlat()` → `string` — single-line format
- `TreeFormat(opts)` / `TreeFormatOpt(opts, context)` → `string`
- `Diagnostic()` / `DiagnosticAnnotated()` → `string`
- `Hex()` → `string`
- `Mermaid(opts)` → `string`

#### CBOR Serialization
- Implements `CBORTagged` / `CBORTaggedEncodable` / `CBORTaggedDecodable`
- Tag: `TAG_ENVELOPE` (#6.200)
- Per-case encoding/decoding rules

#### Static Helpers
- `UnknownEnvelope()` — creates 'Unknown' known-value envelope
- `OKEnvelope()` — creates 'OK' known-value envelope

### Constants

#### Well-Known Functions
| Constant | Value | Name |
|---|---|---|
| `FunctionAdd` | 1 | "add" |
| `FunctionSub` | 2 | "sub" |
| `FunctionMul` | 3 | "mul" |
| `FunctionDiv` | 4 | "div" |
| `FunctionNeg` | 5 | "neg" |
| `FunctionLT` | 6 | "lt" |
| `FunctionLE` | 7 | "le" |
| `FunctionGT` | 8 | "gt" |
| `FunctionGE` | 9 | "ge" |
| `FunctionEQ` | 10 | "eq" |
| `FunctionNE` | 11 | "ne" |
| `FunctionAnd` | 12 | "and" |
| `FunctionOr` | 13 | "or" |
| `FunctionXor` | 14 | "xor" |
| `FunctionNot` | 15 | "not" |

#### Well-Known Parameters
| Constant | Value | Name |
|---|---|---|
| `ParameterBlank` | 1 | "_" |
| `ParameterLHS` | 2 | "lhs" |
| `ParameterRHS` | 3 | "rhs" |

### Global State
- `globalFormatContext` — `sync.Mutex`-guarded global FormatContext
- `globalFunctions` — `sync.Once`-initialized global FunctionsStore
- `globalParameters` — `sync.Once`-initialized global ParametersStore
- `RegisterTags()` — registers standard tags in global format context
- `RegisterTagsIn(context)` — registers tags in a specific context
- `WithFormatContext(fn)` / `WithFormatContextMut(fn)` — closures for accessing global context

## Test Inventory

### Integration Tests (21 files, ~158 tests)
| File | Tests | Key Coverage |
|---|---|---|
| `core_tests.rs` | 17 | Basic envelope creation, subjects, assertions, wrapping |
| `core_nesting_tests.rs` | 6 | Nested envelope structures |
| `core_encoding_tests.rs` | 4 | CBOR round-trip encoding |
| `format_tests.rs` | 12 | Tree format, diagnostic, Mermaid output |
| `elision_tests.rs` | 16 | Elision, revealing, obscuration |
| `edge_tests.rs` | 44 | Edge creation, validation, errors |
| `crypto_tests.rs` | 10 | Encryption, decryption, signing |
| `obscuring_tests.rs` | 6 | Elide/encrypt/compress obscuration |
| `proof_tests.rs` | 3 | Inclusion proof creation and verification |
| `non_correlation_tests.rs` | 3 | Salt-based decorrelation |
| `type_tests.rs` | 4 | Type assertions |
| `signature_tests.rs` | 3 | Signature creation and verification |
| `compression_tests.rs` | 2 | Compress/decompress |
| `keypair_signing_tests.rs` | 2 | Keypair-based signing |
| `attachment_tests.rs` | 1 | Attachment add/query |
| `ed25519_tests.rs` | 1 | Ed25519 signing |
| `encapsulation_tests.rs` | 1 | Key encapsulation |
| `encrypted_tests.rs` | 1 | Encrypted envelope operations |
| `multi_permit_tests.rs` | 1 | Multi-recipient encryption |
| `ssh_tests.rs` | 1 | SSH key signing |
| `sskr_tests.rs` | 1 | SSKR split/join |

### Inline Tests (19 tests in source files)
| File | Tests | Key Coverage |
|---|---|---|
| `envelope.rs` | 6 | Unit tests for Envelope construction |
| `expression.rs` | 2 | Expression creation and parsing |
| `request.rs` | 3 | Request creation, metadata, parameter format |
| `response.rs` | 4 | Success/failure/early-failure responses |
| `event.rs` | 1 | Event creation and parsing |
| `sskr.rs` | 1 | SSKR split/join inline |
| `seal.rs` | 2 | Seal/unseal operations |

### Total: ~158 tests

### Test Infrastructure
- `assert_actual_expected` helper function (Go equivalent of Rust macro)
- `checkEncoding()` method — CBOR round-trip verification
- Test data helpers: `helloEnvelope()`, `alicePrivateKey()`, `bobPrivateKey()`, etc.
- Test seed: `Seed` struct example with Envelope conversion

### EXPECTED TEXT OUTPUT RUBRIC
Applicable: yes

**Source signals**: 18 of 21 test files use `assert_actual_expected!` to verify exact text output of envelope formatting (tree format, diagnostic notation, Mermaid diagrams, hex output). Tests extensively validate complex multi-line rendered text with specific indentation, Unicode characters (double-angle brackets «» for functions, heavy-angle brackets ❰❱ for parameters), and digest abbreviations.

**Target test areas**: `format_test.go`, `core_test.go`, `core_nesting_test.go`, `elision_test.go`, `edge_test.go`, `crypto_test.go`, `obscuring_test.go`, `proof_test.go`, `type_test.go`, `signature_test.go`, `compression_test.go`, `attachment_test.go`, `sskr_test.go`, `non_correlation_test.go`, `encapsulation_test.go`, `keypair_signing_test.go`, `ssh_test.go`.

Use full expected-text output assertions (raw string literals with backticks) for all complex rendered structures instead of many brittle field-level assertions.

## Translation Units (Dependency Order)

### Unit 1: Error Types
- `base/error.rs` → `error.go`
- Go sentinel error vars: `var ErrAlreadyElided = errors.New("already elided")`
- Custom `EnvelopeError` type wrapping upstream errors from dcbor, bc-components
- `errors.Is()` / `errors.As()` for error matching

### Unit 2: Core Envelope Structure
- `base/envelope.rs` → `envelope.go`, `envelope_case.go`
- `EnvelopeCase` as interface with sealed method (unexported `envelopeCase()`)
- Concrete case types: `NodeCase`, `LeafCase`, `WrappedCase`, `AssertionCase`, `ElidedCase`, `KnownValueCase`, `EncryptedCase`, `CompressedCase`
- `Envelope` struct wrapping `EnvelopeCase`
- Constructors: `NewEnvelope`, `NewEnvelopeWithAssertion`, `NewLeafEnvelope`, etc.
- Factory helpers: `NullEnvelope()`, `TrueEnvelope()`, `FalseEnvelope()`, `UnitEnvelope()`

### Unit 3: Assertion
- `base/assertion.rs` → `assertion.go`
- Predicate + object envelope pair with computed digest
- CBOR encoding as single-element Map
- DigestProvider implementation

### Unit 4: EnvelopeEncodable
- `base/envelope_encodable.rs` + `base/envelope_decodable.rs` → `envelope_encodable.go`
- Interface: `EnvelopeEncodable` with `Envelope() *Envelope` method
- `NewEnvelope(subject interface{})` with type switch for dispatch
- Helper functions for specific conversions

### Unit 5: Leaf Helpers
- `base/leaf.rs` → integrated into `queries.go`
- Boolean, null, number, byte-string, collection access helpers
- Known-value helpers

### Unit 6: Queries
- `base/queries.rs` → `queries.go`
- Type checks, subject type checks, content extraction methods
- `ElementsCount()` recursive counter
- Position methods

### Unit 7: Digest Operations
- `base/digest.rs` → `digest.go`
- DigestProvider implementation for Envelope
- `Digests()`, `DeepDigests()`, `ShallowDigests()`, `StructuralDigest()`
- `IsEquivalentTo()`, `IsIdenticalTo()`

### Unit 8: CBOR Serialization
- `base/cbor.rs` → `cbor.go`
- `CBORTagged`, `CBORTaggedEncodable`, `CBORTaggedDecodable` implementations
- Tag: `TAG_ENVELOPE` (#6.200)
- Per-case encoding/decoding rules

### Unit 9: Walk / Visitor
- `base/walk.rs` → `walk.go`
- `EdgeType` const declarations
- Walk function with visitor callback

### Unit 10: Elision & Obscuration
- `base/elide.rs` → `elide.go`
- `ObscureAction` interface, `ObscureType` enum
- Core elision methods + walk-based recursive operations

### Unit 11: Wrap / Unwrap
- `base/wrap.rs` → integrated into `envelope.go`
- `Wrap()`, `Unwrap()` methods

### Unit 12: Assertion Management
- `base/assertions.rs` → `assertions.go`
- All `Add*Assertion*`, `RemoveAssertion`, `ReplaceAssertion`, `ReplaceSubject` methods
- Salted assertion variants with `Using` variants for deterministic testing

### Unit 13: String Utilities
- `string_utils.rs` → `string_utils.go` (internal)
- `flankedBy(s, left, right)` helper

### Unit 14: Format Context & Tag Registration
- `format/format_context.rs` → `format_context.go`
- Global context with `sync.Mutex`
- `RegisterTags()` / `RegisterTagsIn(context)` functions
- `WithFormatContext()` / `WithFormatContextMut()` helpers

### Unit 15: Envelope Notation (Format)
- `format/notation.rs` → `notation.go`
- Core tree-format rendering with known-value, function, parameter annotation

### Unit 16: Tree Format
- `format/tree.rs` → `tree_format.go`
- `TreeFormatOpts` struct
- `TreeFormat()` / `TreeFormatOpt()` methods

### Unit 17: Envelope Summary
- `format/envelope_summary.rs` → `envelope_summary.go`
- Short text summaries per case

### Unit 18: Diagnostic & Hex Format
- `format/diagnostic.rs` → `diagnostic.go`
- `format/hex.rs` → `hex_format.go`
- `Diagnostic()`, `DiagnosticAnnotated()`, `Hex()` methods

### Unit 19: Mermaid Format
- `format/mermaid.rs` → `mermaid.go`
- `MermaidFormatOpts`, `MermaidOrientation`, `MermaidTheme`
- `Mermaid()` method

### Unit 20: Encryption Extension
- `extension/encrypt.rs` → `encrypt.go`
- `EncryptSubject(key)` / `EncryptSubjectWithNonce(key, nonce)` / `DecryptSubject(key)`
- `Encrypt(key)` / `Decrypt(key)` convenience wrappers

### Unit 21: Compression Extension
- `extension/compress.rs` → `compress.go`
- `Compress()` / `Decompress()` / `CompressSubject()` / `DecompressSubject()`

### Unit 22: Salt Extension
- `extension/salt.rs` → `salt.go`
- `AddSalt()` / `AddSaltInstance()` / `AddSaltWithLen()` / `AddSaltInRange()`
- Internal `Using` variants for deterministic RNG

### Unit 23: Signature Extension
- `extension/signature/signature_impl.rs` → `signature.go`
- `extension/signature/signature_metadata.rs` → `signature_metadata.go`
- All signing, verification, and metadata methods
- `SigningOptions` struct

### Unit 24: Recipient Extension
- `extension/recipient.rs` → `recipient.go`
- `AddRecipient()`, `Recipients()`, `EncryptSubjectToRecipients()`, etc.

### Unit 25: Secret Extension
- `extension/secret.rs` → `secret.go`
- `LockSubject()` / `UnlockSubject()` / `Lock()` / `Unlock()` / `AddSecret()`

### Unit 26: SSKR Extension
- `extension/sskr.rs` → `sskr.go`
- `SSKRSplit()` / `SSKRSplitFlattened()` / `SSKRJoin()`

### Unit 27: Proof Extension
- `extension/proof.rs` → `proof.go`
- `ProofContainsSet()` / `ProofContainsTarget()` / `ConfirmContainsSet()` / `ConfirmContainsTarget()`

### Unit 28: Types Extension
- `extension/types.rs` → `types.go`
- `AddType()` / `Types()` / `GetType()` / `HasType()` / `HasTypeValue()` / `CheckTypeValue()` / `CheckType()`

### Unit 29: Attachment Extension
- `extension/attachment/` → `attachment.go`
- `Attachments` container struct
- Envelope methods: `NewAttachment()`, `Attachments()`, `AttachmentPayload()`, etc.

### Unit 30: Edge Extension
- `extension/edge/` → `edge.go`
- `Edges` container struct
- Envelope methods: `NewEdge()`, `Edges()`, `EdgePayload()`, etc.

### Unit 31: Expression System
- `extension/expressions/function.rs` → `function.go`
- `extension/expressions/functions.rs` → `functions.go` (constants + global store)
- `extension/expressions/functions_store.rs` → `functions_store.go`
- `extension/expressions/parameter.rs` → `parameter.go`
- `extension/expressions/parameters.rs` → `parameters.go` (constants + global store)
- `extension/expressions/parameters_store.rs` → `parameters_store.go`
- `extension/expressions/expression.rs` → `expression.go`
- `extension/expressions/request.rs` → `request.go`
- `extension/expressions/response.rs` → `response.go`
- `extension/expressions/event.rs` → `event.go`

### Unit 32: Seal
- `seal.rs` → `seal.go`
- `Seal()` / `SealOpt()` / `Unseal()` combined sign+encrypt operations

### Unit 33: Prelude / Package Exports
- `prelude.rs` → Not needed (Go package exports are automatic via capitalization)
- Ensure all public types are exported from `bcenvelope` package

### Unit 34: Test Infrastructure
- `tests/common/` → `test_helpers_test.go`
- `assertActualExpected()` helper function
- `checkEncoding()` method on `*Envelope`
- Test data: `helloEnvelope()`, `alicePrivateKey()`, etc.
- `TestSeed` struct for domain object example

### Unit 35: Integration Tests
- All 21 test files translated to Go `_test.go` files
- Use raw string backticks for expected text output

## Translation Hazards

### H1: Reference Counting → Go GC
Rust uses `Rc<EnvelopeCase>` (or `Arc` with multithreaded). Go GC handles reference management automatically. Use `*Envelope` pointer receivers for identity semantics. Ensure all mutations return new `*Envelope` instances (functional/immutable style).

### H2: Rust `paste!` Macro for Constants
The `function_constant!` and `parameter_constant!` macros generate paired constants. In Go, declare both constants manually as package-level vars.

### H3: TypeId-Based Runtime Type Extraction
`extract_subject<T>()` in Rust uses `TypeId::of::<T>()`. In Go, use generic functions with type switch: `ExtractSubject[T any](e *Envelope) (T, error)`. For special types (CBOR, Envelope, Assertion, KnownValue, Function, Parameter), check with type assertions inside the generic function.

### H4: Visitor Closures with Mutable State
`walk()` takes a `Fn` closure. In Go, use `func(envelope *Envelope, level int, edgeType EdgeType, state S) (S, bool)` function type. Go closures can capture mutable state directly.

### H5: Blanket Trait Implementations
`EnvelopeEncodable` has blanket impls. In Go, use a type switch in `NewEnvelope()` to handle all supported types, plus an `EnvelopeEncodable` interface for custom types.

### H6: Macro-Generated Type Conversions
`impl_envelope_encodable!` generates conversions for ~25 types. In Go, write explicit cases in the type switch.

### H7: Feature-Gated Code
All Rust features are conditionally compiled. In Go, all code is always compiled. Remove all feature gates.

### H8: Thread-Safe Global State
`GLOBAL_FORMAT_CONTEXT`, `GLOBAL_FUNCTIONS`, `GLOBAL_PARAMETERS` use `Mutex`. In Go, use `sync.Mutex` for the format context and `sync.Once` for lazy initialization of stores.

### H9: Generic Event Type
`Event<T>` requires complex type bounds. In Go, use `interface{}` or specific types with type assertions. Go generics may help but have limitations on method receivers.

### H10: Complex Equality Semantics
`Envelope` implements `PartialEq` via `is_identical_to()` which is O(m+n) structural comparison. In Go, implement `IsIdenticalTo()` as a method. Use `IsEquivalentTo()` (O(1) digest comparison) where possible.

### H11: Assertion Sorting by Digest
Assertions in a Node are sorted by their digest for deterministic output. Ensure the sorting uses the same byte-comparison order as Rust (`slices.SortFunc` with `bytes.Compare` on digest bytes).

### H12: CBOR Encoding/Decoding Fidelity
Leaf envelopes encode as tagged CBOR (#6.24 wrapping encoded CBOR bytes). This double-encoding must be preserved exactly for cross-language compatibility.

### H13: Expected Text Output
18 of 21 test files use expected text output assertions. Formatting must exactly match Rust output including whitespace, indentation, Unicode characters (double-angle brackets for functions, heavy-angle brackets for parameters), and digest abbreviations.

### H14: Attachable/Edgeable Patterns
Rust uses macros to generate boilerplate. In Go, implement methods directly on `*Envelope`. No separate interface needed since Go uses structural typing.

### H15: Response Uses Rust Result Internally
`Response` wraps `std::result::Result<(ARID, Envelope), (Option<ARID>, Envelope)>`. In Go, model with a struct containing success/failure fields, or use a tagged union pattern.

### H16: Deterministic Test RNG
Many tests use deterministic random data. The `Using` variants of salting/SSKR functions accept an RNG parameter. These must produce identical bytes to Rust for test vector compatibility.

### H17: Go-Specific — No Method Overloading
Go does not support method overloading. Where Rust has `encrypt_subject(key)` and `encrypt_subject_opt(key, nonce)`, use separate method names: `EncryptSubject(key)` and `EncryptSubjectWithNonce(key, nonce)`.

### H18: Go-Specific — Error Handling Verbosity
Every fallible operation returns `(result, error)`. Chain operations carefully to propagate errors without losing the immutable envelope semantics.

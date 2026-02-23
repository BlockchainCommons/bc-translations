# Translation Manifest: bc-envelope 0.43.0 → Kotlin

## Crate Overview

Gordian Envelope for Rust. A hierarchical binary data format built on deterministic CBOR (dCBOR) with a Merkle-like digest tree. Supports selective disclosure via elision, encryption, and compression; digital signatures; SSKR social recovery; expression-based RPC; metadata attachments; typed edges; and inclusion proofs. All default features enabled.

Total source: ~12,000 lines (base ~5,100, format ~1,870, extensions ~5,000+).

## External Dependencies

| Rust Crate | Kotlin Equivalent |
|---|---|
| bc-rand | `com.blockchaincommons:bc-rand` (sibling) |
| bc-crypto | `com.blockchaincommons:bc-crypto` (sibling) |
| dcbor | `com.blockchaincommons:dcbor` (sibling) |
| bc-ur | `com.blockchaincommons:bc-ur` (sibling) |
| bc-components | `com.blockchaincommons:bc-components` (sibling) |
| known-values | `com.blockchaincommons:known-values` (sibling) |
| paste | Not needed (Kotlin has no paste-macro equivalent; use companion-object constants directly) |
| hex | Kotlin stdlib `String.hexToByteArray()` / `ByteArray.toHexString()` with `@OptIn(ExperimentalStdlibApi::class)` |
| itertools | Kotlin stdlib collection operations (`sortedBy`, `groupBy`, `flatMap`, etc.) |
| thiserror | Kotlin sealed class hierarchy for `EnvelopeException` |
| bytes | `ByteArray` (Kotlin stdlib) |
| ssh-key | NOT TRANSLATED (ssh feature is default but depends on bc-components SSH support) |
| hex-literal (dev) | Hex string literals with `"...".hexToByteArray()` |
| lazy_static (dev) | Not needed (Kotlin has `lazy { }` / `by lazy`) |
| indoc (dev) | Kotlin `trimIndent()` / `trimMargin()` |

## Feature Mapping

| Rust Feature | Default? | Kotlin Approach |
|---|---|---|
| attachment | Yes | Always enabled |
| compress | Yes | Always enabled |
| edge | Yes | Always enabled |
| ed25519 | Yes | Always enabled (bc-crypto/bc-components have ed25519) |
| encrypt | Yes | Always enabled |
| expression | Yes | Always enabled |
| known_value | Yes | Always enabled |
| known-values-directory-loading | Yes | Always enabled |
| multithreaded | Yes | N/A (JVM is always multithreaded; no Rc vs Arc distinction) |
| pqcrypto | Yes | Always enabled (bc-components has ML-KEM/ML-DSA) |
| proof | Yes | Always enabled |
| recipient | Yes | Always enabled |
| salt | Yes | Always enabled |
| secp256k1 | Yes | Always enabled (bc-crypto/bc-components have secp256k1) |
| secret | Yes | Always enabled |
| signature | Yes | Always enabled |
| ssh | Yes | Always enabled (bc-components has SSH support) |
| sskr | Yes | Always enabled |
| types | Yes | Always enabled |

## Public API Surface

### Core Types

| Rust Type | Kotlin Type | Notes |
|---|---|---|
| `Envelope` (struct wrapping `RefCounted<EnvelopeCase>`) | `Envelope` (class) | JVM uses reference semantics by default; no Rc/Arc needed |
| `EnvelopeCase` (enum: Node, Leaf, Wrapped, Assertion, Elided, KnownValue, Encrypted, Compressed) | `EnvelopeCase` (sealed class) | 8 variants |
| `Assertion` (struct: predicate, object, digest) | `Assertion` (class) | Digest computed from predicate+object digests |
| `Error` (thiserror enum, ~30 variants) | `EnvelopeException` (sealed class hierarchy) | Feature-gated variants always present in Kotlin |
| `Result<T>` (type alias) | Standard Kotlin exceptions | Use `throws` on functions |
| `ObscureAction` (enum: Elide, Encrypt, Compress) | `ObscureAction` (sealed class) | Encrypt carries SymmetricKey |
| `ObscureType` (enum: Elided, Encrypted, Compressed) | `ObscureType` (enum class) | |
| `EdgeType` (enum: None, Subject, Assertion, Predicate, Object, Content) | `EdgeType` (enum class) | Walk traversal edge labels |
| `FormatContext` | `FormatContext` (class) | Global singleton + custom instances |
| `FormatContextOpt` (enum: None, Global, Custom) | `FormatContextOpt` (sealed class) | |
| `TreeFormatOpts` | `TreeFormatOpts` (data class) | |
| `MermaidFormatOpts` | `MermaidFormatOpts` (data class) | |
| `MermaidOrientation` (enum) | `MermaidOrientation` (enum class) | |
| `MermaidTheme` (enum) | `MermaidTheme` (enum class) | |
| `DigestDisplayFormat` (enum) | `DigestDisplayFormat` (enum class) | |
| `EnvelopeSummary` | `EnvelopeSummary` (class/object) | |
| `SignatureMetadata` (struct) | `SignatureMetadata` (class) | |
| `SigningOptions` (struct) | `SigningOptions` (data class) | |

### Expression Types

| Rust Type | Kotlin Type | Notes |
|---|---|---|
| `Expression` (struct: function, envelope) | `Expression` (class) | |
| `ExpressionBehavior` (trait) | `ExpressionBehavior` (interface) | |
| `IntoExpression` (trait) | Not needed (use `toExpression()` extension) | Kotlin doesn't need blanket-impl pattern |
| `Function` (enum: Known, Named) | `Function` (sealed class) | CBOR tag #6.40006 |
| `FunctionName` (enum: Static, Dynamic) | Not needed (just use String in Kotlin) | |
| `FunctionsStore` | `FunctionsStore` (class) | |
| `Parameter` (enum: Known, Named) | `Parameter` (sealed class) | CBOR tag #6.40007 |
| `ParametersStore` | `ParametersStore` (class) | |
| `Request` (struct: body, id, note, date) | `Request` (class) | CBOR tag #6.40010 |
| `RequestBehavior` (trait) | `RequestBehavior` (interface) | |
| `Response` (struct wrapping Result) | `Response` (class) | CBOR tag #6.40011 |
| `ResponseBehavior` (trait) | `ResponseBehavior` (interface) | |
| `Event<T>` (generic struct) | `Event<T>` (class) | CBOR tag #6.40012 |
| `EventBehavior<T>` (trait) | `EventBehavior<T>` (interface) | |

### Attachment & Edge Types

| Rust Type | Kotlin Type | Notes |
|---|---|---|
| `Attachments` (struct) | `Attachments` (class) | HashMap<Digest, Envelope> container |
| `Attachable` (trait) | `Attachable` (interface) | |
| `Edges` (struct) | `Edges` (class) | HashMap<Digest, Envelope> container |
| `Edgeable` (trait) | `Edgeable` (interface) | |

### Traits → Interfaces

| Rust Trait | Kotlin Interface | Notes |
|---|---|---|
| `EnvelopeEncodable` | `EnvelopeEncodable` (interface) | `intoEnvelope()` / `toEnvelope()` |
| `DigestProvider` | `DigestProvider` (from bc-components) | Already exists |
| `CBORTagged` | `CBORTagged` (from dcbor) | Already exists |
| `CBORTaggedEncodable` | `CBORTaggedEncodable` (from dcbor) | Already exists |
| `CBORTaggedDecodable` | `CBORTaggedDecodable` (from dcbor) | Already exists |
| `ExpressionBehavior` | `ExpressionBehavior` | 8 methods |
| `RequestBehavior` | `RequestBehavior` extends `ExpressionBehavior` | |
| `ResponseBehavior` | `ResponseBehavior` | 12+ methods |
| `EventBehavior<T>` | `EventBehavior<T>` | |
| `Attachable` | `Attachable` | |
| `Edgeable` | `Edgeable` | |

### EnvelopeEncodable Implementations

The `EnvelopeEncodable` trait has blanket and explicit implementations for these types. In Kotlin, implement as extension functions or interface implementations:

- Primitives: `UByte`, `UShort`, `UInt`, `ULong`, `Byte`, `Short`, `Int`, `Long`, `Boolean`, `Float`, `Double`
- Strings: `String`
- Binary: `ByteString`, `ByteArray`
- CBOR: `CBOR`
- Collections: `List<T>`, `Map<K,V>`, `Set<T>`
- bc-components types: `Digest`, `Salt`, `Nonce`, `ARID`, `URI`, `UUID`, `XID`, `Reference`, `Date`, `JSON`
- Crypto types: `PublicKeys`, `PrivateKeys`, `PrivateKeyBase`, `SealedMessage`, `EncryptedKey`, `Signature`, `SSKRShare`
- Envelope types: `Assertion`, `KnownValue`, `Function`, `Parameter`

### Core Envelope Methods (~200+ methods)

#### Construction
- `new(subject)` → `Envelope(subject)` constructor
- `new_or_null(subject?)` / `new_or_none(subject?)` — create Envelope or null/none sentinel
- `new_assertion(predicate, object)` — create bare assertion envelope
- `new_leaf(cbor)` — create leaf from CBOR
- `new_wrapped(envelope)` — wrap an envelope
- `new_elided(digest)` — create elided placeholder
- `new_with_known_value(kv)` — known-value subject
- `new_with_encrypted(msg)` — encrypted subject
- `new_with_compressed(comp)` — compressed subject
- `null()`, `true()`, `false()`, `unit()` — static factory helpers

#### Assertion Management
- `add_assertion(predicate, object)` → `addAssertion(predicate, object)`
- `add_assertion_envelope(assertion)` → `addAssertionEnvelope(assertion)`
- `add_assertion_envelopes(assertions)` → `addAssertionEnvelopes(assertions)`
- `add_optional_assertion_envelope(assertion?)` → `addOptionalAssertionEnvelope(assertion?)`
- `add_optional_assertion(predicate, object?)` → `addOptionalAssertion(predicate, object?)`
- `add_nonempty_string_assertion(predicate, string)` → `addNonemptyStringAssertion(predicate, string)`
- `add_assertions(predicate, objects)` → `addAssertions(predicate, objects)`
- `add_assertion_if(condition, predicate, object)` → `addAssertionIf(condition, predicate, object)`
- `add_assertion_envelope_if(condition, assertion)` → `addAssertionEnvelopeIf(condition, assertion)`
- `remove_assertion(assertion)` → `removeAssertion(assertion)`
- `replace_assertion(old, new)` → `replaceAssertion(old, new)`
- `replace_subject(new_subject)` → `replaceSubject(newSubject)`

#### Salted Assertions (salt feature)
- `add_assertion_salted(predicate, object)` → `addAssertionSalted(predicate, object)`
- `add_assertion_envelope_salted(assertion)` → `addAssertionEnvelopeSalted(assertion)`
- `add_optional_assertion_envelope_salted(assertion?)` → `addOptionalAssertionEnvelopeSalted(assertion?)`
- `add_assertions_salted(predicate, objects)` → `addAssertionsSalted(predicate, objects)`
- Internal `_using` variants for deterministic RNG testing

#### Structural Queries
- `subject()` → `subject()`
- `assertions()` → `assertions()`
- `has_assertions()` → `hasAssertions()`
- `is_assertion()` / `is_encrypted()` / `is_compressed()` / `is_elided()` / `is_leaf()` / `is_node()` / `is_wrapped()`
- `is_subject_assertion()` / `is_subject_encrypted()` / `is_subject_compressed()` / `is_subject_elided()` / `is_subject_obscured()`
- `as_assertion()` / `try_assertion()` — access as Assertion
- `as_predicate()` / `try_predicate()` — access predicate
- `as_object()` / `try_object()` — access object
- `as_leaf()` / `try_leaf()` — access leaf CBOR
- `elements_count()` — recursive element count

#### Content Extraction
- `extract_subject<T>()` → `extractSubject<T>()` (reified inline)
- `extract_object<T>()` / `extract_predicate<T>()`
- `object_for_predicate(predicate)` → `objectForPredicate(predicate)`
- `objects_for_predicate(predicate)` → `objectsForPredicate(predicate)`
- `assertions_with_predicate(predicate)` → `assertionsWithPredicate(predicate)`
- `assertion_with_predicate(predicate)` → `assertionWithPredicate(predicate)`
- `extract_object_for_predicate<T>(predicate)` → `extractObjectForPredicate<T>(predicate)`
- `extract_optional_object_for_predicate<T>(predicate)` → `extractOptionalObjectForPredicate<T>(predicate)`
- `extract_object_for_predicate_with_default<T>(predicate, default)` → `extractObjectForPredicateWithDefault<T>(predicate, default)`
- `extract_objects_for_predicate<T>(predicate)` → `extractObjectsForPredicate<T>(predicate)`

#### Leaf Helpers
- `is_null()` / `is_true()` / `is_false()` / `is_bool()` / `is_number()` / `is_nan()`
- `is_subject_number()` / `is_subject_nan()`
- `try_byte_string()` / `as_byte_string()`
- `as_array()` / `as_map()` / `as_text()`

#### Known Value Helpers
- `as_known_value()` / `try_known_value()` / `is_known_value()`
- `is_subject_unit()` / `check_subject_unit()`

#### Digest Operations
- `digest()` → `digest` (property via DigestProvider)
- `digests(level_limit)` → `digests(levelLimit)`
- `deep_digests()` → `deepDigests()`
- `shallow_digests()` → `shallowDigests()`
- `structural_digest()` → `structuralDigest()`
- `is_equivalent_to(other)` → `isEquivalentTo(other)` (O(1) via digests)
- `is_identical_to(other)` → `isIdenticalTo(other)` (O(m+n) structural)

#### Wrapping
- `wrap()` → `wrap()`
- `try_unwrap()` → `unwrap()` (throws on failure)

#### Elision & Obscuration
- `elide()` → `elide()`
- `elide_set_with_action(set, removing, action)` — core recursive impl
- `elide_removing_set(set)` / `elide_revealing_set(set)`
- `elide_removing_array(array)` / `elide_revealing_array(array)`
- `elide_removing_target(target)` / `elide_revealing_target(target)`
- Variants with `_with_action` suffix for Encrypt/Compress obscuration
- `unelide(original)` → `unelide(original)`
- `nodes_matching(set, types)` → `nodesMatching(set, types)`
- `walk_unelide(lookup_set)` → `walkUnelide(lookupSet)`
- `walk_replace(target_digests, replacement)` → `walkReplace(targetDigests, replacement)`
- `walk_decrypt(keys)` → `walkDecrypt(keys)` — recursive decrypt
- `walk_decompress(targets?)` → `walkDecompress(targets?)` — recursive decompress

#### Walk/Visitor
- `walk(hide_nodes, state, visit)` → `walk(hideNodes, state, visit)`
- Visitor type: `(Envelope, Int, EdgeType, State) -> Pair<State, Boolean>`

#### Encryption (encrypt feature)
- `encrypt_subject(key)` / `encrypt_subject_opt(key, nonce)` → `encryptSubject(key)` / `encryptSubject(key, nonce)`
- `decrypt_subject(key)` → `decryptSubject(key)`
- `encrypt(key)` / `decrypt(key)` — wrap+encrypt / decrypt+unwrap convenience

#### Compression (compress feature)
- `compress()` / `decompress()`
- `compress_subject()` / `decompress_subject()`

#### Salt (salt feature)
- `add_salt()` / `add_salt_instance(salt)` / `add_salt_with_len(count)` / `add_salt_in_range(range)`

#### Signature (signature feature)
- `add_signature(signer)` / `add_signature_opt(signer, options)`
- `add_signatures(signers)` / `add_signatures_opt(signers, options)`
- `is_verified_signature(verifier)` / `verify_signature(verifier)`
- `has_signature_from(verifier)` / `verify_signature_from(verifier)`
- `has_signatures_from(verifiers)` / `has_signatures_from_threshold(verifiers, threshold)`
- `verify_signatures_from_threshold(verifiers, threshold)` / `verify_signatures_from(verifiers)`
- `sign(signer)` / `sign_opt(signer, options)` / `verify(verifier)` / `verify_returning_metadata(verifier)`
- Metadata-aware variants: `has_signature_from_returning_metadata(verifier)` / `verify_signature_from_returning_metadata(verifier)`

#### Recipient (recipient feature)
- `add_recipient(recipient)` / `recipients()`
- `encrypt_subject_to_recipients(recipients, key?)` / `encrypt_subject_to_recipient(recipient, key?)`
- `decrypt_subject_to_recipient(recipient)` / `encrypt_to_recipient(recipient, key?)` / `decrypt_to_recipient(recipient)`

#### Secret (secret feature)
- `lock_subject(password)` / `unlock_subject(password)`
- `lock(password)` / `unlock(password)`
- `add_secret(method_selector, plaintext)` — low-level
- `is_locked_with_password()` / `is_locked_with_ssh_agent()`

#### SSKR (sskr feature)
- `sskr_split(spec, key?)` → `sskrSplit(spec, key?)`
- `sskr_split_flattened(threshold, share_count, key?)` → `sskrSplitFlattened(threshold, shareCount, key?)`
- `sskr_join(shares)` → `sskrJoin(shares)` (static)

#### Proof (proof feature)
- `proof_contains_set(set)` / `proof_contains_target(target)`
- `confirm_contains_set(set)` / `confirm_contains_target(target)`

#### Types (types feature)
- `add_type(type)` / `types()` / `get_type()` / `has_type()` / `has_type_value()` / `check_type_value()` / `check_type()`

#### Position (known_value feature)
- `set_position(position)` / `position()` / `remove_position()`

#### Seal
- `seal(signer, recipient)` / `seal_opt(signer, recipient, options, key?)`
- `unseal(recipient, verifier)`

#### Formatting
- `format()` / `format_opt(context?)` → tree-format string
- `format_flat()` — single-line format
- `tree_format(opts)` / `tree_format_opt(opts, context?)`
- `diagnostic()` / `diagnostic_opt(annotated, context?)` — CBOR diagnostic notation
- `hex()` — hex-encoded CBOR bytes
- `mermaid(opts?)` — Mermaid diagram output

#### CBOR Serialization
- Implements `CBORTagged` / `CBORTaggedEncodable` / `CBORTaggedDecodable`
- Tag: `TAG_ENVELOPE` (#6.200)
- CBOR encoding rules per `EnvelopeCase`:
  - Node → CBOR Array (subject + sorted assertions)
  - Leaf → tagged #6.24 (encoded CBOR)
  - Wrapped → envelope tag
  - Assertion → CBOR Map with exactly 1 entry
  - Elided → ByteString (32 bytes = digest)
  - KnownValue → Unsigned integer
  - Encrypted → tagged encrypted message
  - Compressed → tagged compressed

#### Static Helpers on Envelope
- `Envelope.unknown()` — creates `'Unknown'` known-value envelope
- `Envelope.ok()` — creates `'OK'` known-value envelope

### Constants

#### Well-Known Functions (functions module)
| Constant | Value | Name |
|---|---|---|
| `ADD` | 1 | "add" |
| `SUB` | 2 | "sub" |
| `MUL` | 3 | "mul" |
| `DIV` | 4 | "div" |
| `NEG` | 5 | "neg" |
| `LT` | 6 | "lt" |
| `LE` | 7 | "le" |
| `GT` | 8 | "gt" |
| `GE` | 9 | "ge" |
| `EQ` | 10 | "eq" |
| `NE` | 11 | "ne" |
| `AND` | 12 | "and" |
| `OR` | 13 | "or" |
| `XOR` | 14 | "xor" |
| `NOT` | 15 | "not" |

#### Well-Known Parameters (parameters module)
| Constant | Value | Name |
|---|---|---|
| `BLANK` | 1 | "_" |
| `LHS` | 2 | "lhs" |
| `RHS` | 3 | "rhs" |

### Global State
- `GLOBAL_FORMAT_CONTEXT` — thread-safe global FormatContext (Mutex-guarded)
- `GLOBAL_FUNCTIONS` — thread-safe global FunctionsStore
- `GLOBAL_PARAMETERS` — thread-safe global ParametersStore
- `register_tags()` — registers standard tags in global format context
- `register_tags_in(context)` — registers tags in a specific context
- `with_format_context` / `with_format_context_mut` — macros for accessing global context

In Kotlin, use `companion object` singletons with `@Volatile` / `synchronized` or `lazy { }` patterns.

## Documentation Catalog

### Module-Level Documentation
- Crate-level doc (`lib.rs`): Comprehensive overview of Gordian Envelope with examples
- Expressions module doc: Envelope expression syntax and semantics
- Attachments module doc: Metadata attachment infrastructure
- Edges module doc: Edge container for verifiable claims (BCR-2026-003)

### Type-Level Documentation
- All public types have KDoc-style documentation
- Key concepts: digest tree, selective disclosure, elision, obscuration, CBOR encoding rules
- Referenced specs: BCR-2023-003 (known values), BCR-2023-004 (encryption), BCR-2023-005 (compression), BCR-2023-006 (attachments), BCR-2023-012 (expressions), BCR-2026-003 (edges)

## Test Inventory

### Integration Tests (21 files, 139 tests)
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

### Total: 158 tests

### Test Infrastructure
- `tests/common/mod.rs`: `assert_actual_expected!` macro
- `tests/common/check_encoding.rs`: `CheckEncoding` trait — CBOR round-trip verification
- `tests/common/test_data.rs`: Helper functions: `hello_envelope()`, `alice_*()`, `bob_*()`, `carol_*()`, `fake_content_key()`, `fake_nonce()`, `credential()`, `redacted_credential()`
- `tests/common/test_seed.rs`: `Seed` domain object example with Envelope <-> Seed conversion

### Expected Text Output Rubric
Found in **18 of 21** test files. Tests extensively validate complex rendered text output (tree format, diagnostic notation, Mermaid diagrams). These must match byte-for-byte.

## Translation Units (Dependency Order)

### Unit 1: Error Types & Result
- `base/error.rs` → `EnvelopeException.kt`
- Sealed class hierarchy mirroring all ~30 Error variants
- `Error::msg(String)` → `EnvelopeException.General(message)` factory
- `From<Error> for dcbor::Error` conversion

### Unit 2: Core Envelope Structure
- `base/envelope.rs` → `Envelope.kt` (partial), `EnvelopeCase.kt`
- `EnvelopeCase` sealed class with 8 variants
- `Envelope` class wrapping `EnvelopeCase`
- Constructors: `new`, `new_assertion`, `new_leaf`, `new_wrapped`, `new_elided`, `new_with_known_value`, `new_with_encrypted`, `new_with_compressed`
- `new_or_null`, `new_or_none` factory helpers
- `null()`, `true()`, `false()`, `unit()`, `unknown()`, `ok()` static factories

### Unit 3: Assertion
- `base/assertion.rs` → `Assertion.kt`
- Predicate + object envelope pair with computed digest
- CBOR encoding as single-element Map
- DigestProvider implementation

### Unit 4: EnvelopeEncodable
- `base/envelope_encodable.rs` + `base/envelope_decodable.rs` → `EnvelopeEncodable.kt`
- Interface: `intoEnvelope()` and `toEnvelope()`
- Implementations for all primitive types, collections, bc-components types
- Decodable: extension functions or companion `fromEnvelope()` methods

### Unit 5: Leaf Helpers
- `base/leaf.rs` → integrated into `Envelope.kt`
- Boolean, null, number, byte-string, collection access helpers
- Known-value helpers (`asKnownValue`, `isKnownValue`, `isSubjectUnit`, etc.)

### Unit 6: Queries
- `base/queries.rs` → integrated into `Envelope.kt`
- Type checks: `isAssertion()`, `isEncrypted()`, `isCompressed()`, etc.
- Subject type checks: `isSubjectAssertion()`, `isSubjectEncrypted()`, etc.
- Content extraction: `extractSubject<T>()`, `objectForPredicate()`, `assertionsWithPredicate()`, etc.
- `elementsCount()` recursive counter
- Position methods: `setPosition()`, `position()`, `removePosition()`

### Unit 7: Digest Operations
- `base/digest.rs` → integrated into `Envelope.kt`
- DigestProvider implementation for Envelope
- `digests()`, `deepDigests()`, `shallowDigests()`, `structuralDigest()`
- `isEquivalentTo()`, `isIdenticalTo()`
- `equals()` / `hashCode()` using `isIdenticalTo()`

### Unit 8: CBOR Serialization
- `base/cbor.rs` → integrated into `Envelope.kt`
- `CBORTagged`, `CBORTaggedEncodable`, `CBORTaggedDecodable` implementations
- Tag: `TAG_ENVELOPE` (#6.200)
- Per-case encoding/decoding rules

### Unit 9: Walk / Visitor
- `base/walk.rs` → `EdgeType.kt` + walk methods in `Envelope.kt`
- `EdgeType` enum class
- Visitor lambda type: `(Envelope, Int, EdgeType, S) -> Pair<S, Boolean>`
- `walk()`, `walkStructure()`, `walkTree()` methods

### Unit 10: Elision & Obscuration
- `base/elide.rs` → `ObscureAction.kt`, `ObscureType.kt` + elision methods in `Envelope.kt`
- `ObscureAction` sealed class, `ObscureType` enum class
- Core: `elide()`, `elideSetWithAction()`, `elideRemovingSet()`, `elideRevealingSet()`, etc.
- Walk-based: `unelide()`, `walkUnelide()`, `walkReplace()`, `walkDecrypt()`, `walkDecompress()`
- `nodesMatching()` helper

### Unit 11: Wrap / Unwrap
- `base/wrap.rs` → integrated into `Envelope.kt`
- `wrap()`, `unwrap()` (throws NotWrapped)

### Unit 12: Assertion Management
- `base/assertions.rs` → integrated into `Envelope.kt`
- All `addAssertion*`, `removeAssertion`, `replaceAssertion`, `replaceSubject` methods
- Conditional and optional assertion variants
- Salted assertion variants (with `_using` variants for deterministic testing)

### Unit 13: String Utilities
- `string_utils.rs` → `StringUtils.kt` (internal)
- `flankedBy(left, right)` extension on String

### Unit 14: Format Context & Tag Registration
- `format/format_context.rs` → `FormatContext.kt`
- `FormatContextOpt` sealed class
- Global context singleton
- `registerTags()` / `registerTagsIn(context)` functions
- `withFormatContext {}` / `withFormatContextMut {}` helper functions

### Unit 15: Envelope Notation (Format)
- `format/notation.rs` → `EnvelopeNotation.kt` or integrated format methods
- Core tree-format rendering with known-value, function, parameter annotation
- `EnvelopeFormatOpts` internal configuration

### Unit 16: Tree Format
- `format/tree.rs` → tree format methods in `Envelope.kt`
- `TreeFormatOpts` data class
- `treeFormat()` / `treeFormatOpt()` methods

### Unit 17: Envelope Summary
- `format/envelope_summary.rs` → `EnvelopeSummary.kt`
- Short text summaries per case

### Unit 18: Diagnostic & Hex Format
- `format/diagnostic.rs` → diagnostic methods in `Envelope.kt`
- `format/hex.rs` → hex methods in `Envelope.kt`
- `diagnostic()`, `diagnosticAnnotated()`, `hex()` methods

### Unit 19: Mermaid Format
- `format/mermaid.rs` → `MermaidFormat.kt` or methods in `Envelope.kt`
- `MermaidFormatOpts`, `MermaidOrientation`, `MermaidTheme`
- `mermaid()` method

### Unit 20: Encryption Extension
- `extension/encrypt.rs` → `EnvelopeEncrypt.kt` (extension functions on Envelope)
- `encryptSubject(key)` / `encryptSubject(key, nonce)` / `decryptSubject(key)`
- `encrypt(key)` / `decrypt(key)` convenience wrappers

### Unit 21: Compression Extension
- `extension/compress.rs` → `EnvelopeCompress.kt`
- `compress()` / `decompress()` / `compressSubject()` / `decompressSubject()`

### Unit 22: Salt Extension
- `extension/salt.rs` → `EnvelopeSalt.kt`
- `addSalt()` / `addSaltInstance(salt)` / `addSaltWithLen(count)` / `addSaltInRange(range)`
- Internal `_using` variants for deterministic RNG

### Unit 23: Signature Extension
- `extension/signature/signature_impl.rs` → `EnvelopeSignature.kt`
- `extension/signature/signature_metadata.rs` → `SignatureMetadata.kt`
- All `addSignature*`, `verifySignature*`, `hasSignatureFrom*` methods
- `sign()` / `verify()` convenience wrappers
- `SigningOptions` data class

### Unit 24: Recipient Extension
- `extension/recipient.rs` → `EnvelopeRecipient.kt`
- `addRecipient()`, `recipients()`, `encryptSubjectToRecipients()`, etc.

### Unit 25: Secret Extension
- `extension/secret.rs` → `EnvelopeSecret.kt`
- `lockSubject()` / `unlockSubject()` / `lock()` / `unlock()` / `addSecret()`
- `isLockedWithPassword()` / `isLockedWithSshAgent()`

### Unit 26: SSKR Extension
- `extension/sskr.rs` → `EnvelopeSskr.kt`
- `sskrSplit()` / `sskrSplitFlattened()` / `sskrJoin()` (companion)

### Unit 27: Proof Extension
- `extension/proof.rs` → `EnvelopeProof.kt`
- `proofContainsSet()` / `proofContainsTarget()` / `confirmContainsSet()` / `confirmContainsTarget()`

### Unit 28: Types Extension
- `extension/types.rs` → `EnvelopeTypes.kt`
- `addType()` / `types()` / `getType()` / `hasType()` / `hasTypeValue()` / `checkTypeValue()` / `checkType()`

### Unit 29: Attachment Extension
- `extension/attachment/` → `Attachments.kt`, `Attachable.kt`
- `Attachments` container class
- `Attachable` interface with default implementations
- Envelope methods: `newAttachment()`, `attachments()`, `attachmentPayload()`, etc.

### Unit 30: Edge Extension
- `extension/edge/` → `Edges.kt`, `Edgeable.kt`
- `Edges` container class
- `Edgeable` interface with default implementations
- Envelope methods: `newEdge()`, `edges()`, `edgePayload()`, etc.

### Unit 31: Expression System
- `extension/expressions/function.rs` → `Function.kt`
- `extension/expressions/functions.rs` → `Functions.kt` (constants + global store)
- `extension/expressions/functions_store.rs` → `FunctionsStore.kt`
- `extension/expressions/parameter.rs` → `Parameter.kt`
- `extension/expressions/parameters.rs` → `Parameters.kt` (constants + global store)
- `extension/expressions/parameters_store.rs` → `ParametersStore.kt`
- `extension/expressions/expression.rs` → `Expression.kt`, `ExpressionBehavior.kt`
- `extension/expressions/request.rs` → `Request.kt`, `RequestBehavior.kt`
- `extension/expressions/response.rs` → `Response.kt`, `ResponseBehavior.kt`
- `extension/expressions/event.rs` → `Event.kt`, `EventBehavior.kt`

### Unit 32: Seal
- `seal.rs` → `EnvelopeSeal.kt`
- `seal()` / `sealOpt()` / `unseal()` combined sign+encrypt operations

### Unit 33: Prelude
- `prelude.rs` → Not needed in Kotlin (use package-level exports)
- Ensure all public types are accessible from `com.blockchaincommons.bcenvelope`

### Unit 34: Test Infrastructure
- `tests/common/mod.rs` → test utilities
- `tests/common/check_encoding.rs` → `CheckEncoding.kt` (extension functions)
- `tests/common/test_data.rs` → `TestData.kt` (test helpers)
- `tests/common/test_seed.rs` → `TestSeed.kt` (domain object example)

### Unit 35: Integration Tests
- All 21 test files translated to JUnit 5 test classes
- Expected text output assertions via `trimIndent()` comparisons

## Translation Hazards

### H1: Reference Counting → JVM References
Rust uses `Rc<EnvelopeCase>` (or `Arc` with multithreaded). On JVM, all objects are reference-counted by the GC. No special handling needed, but `Envelope` should still be an immutable value type (all mutation returns new instances).

### H2: Rust `paste!` Macro for Constants
The `function_constant!` and `parameter_constant!` macros use `paste!` to generate companion `_VALUE` constants. In Kotlin, declare both constants manually in companion objects.

### H3: TypeId-Based Runtime Type Extraction
`extract_subject<T>()` in Rust uses `TypeId::of::<T>()` for runtime type matching against special types (CBOR, Envelope, Assertion, KnownValue, Function, Parameter). In Kotlin, use `reified` inline functions with `when (T::class)` dispatch or `is` checks.

### H4: Visitor Closures with Mutable State
`walk()` takes a `Fn` closure as `Visitor`. In Kotlin, use `(Envelope, Int, EdgeType, S) -> Pair<S, Boolean>` lambda type. The Rust implementation passes state by value and returns it; Kotlin lambdas can capture mutable state directly if needed.

### H5: Blanket Trait Implementations
`EnvelopeEncodable` has a blanket impl for `T: Into<Envelope> + Clone`. Kotlin can't do blanket impls. Instead, provide explicit `EnvelopeEncodable` implementations or extension functions for each supported type.

### H6: Macro-Generated Type Conversions
`impl_envelope_encodable!` and `impl_envelope_decodable!` macros generate conversions for ~25 types. In Kotlin, write explicit implementations or use a code-generation approach.

### H7: Feature-Gated Code
All Rust features are conditionally compiled. In Kotlin, all code is always compiled (features always enabled). Remove all `#[cfg(feature = "...")]` guards.

### H8: Thread-Safe Global State
`GLOBAL_FORMAT_CONTEXT`, `GLOBAL_FUNCTIONS`, `GLOBAL_PARAMETERS` use `Mutex` + `Once`. In Kotlin, use `object` singletons with thread-safe lazy initialization or `@Volatile` + `synchronized`.

### H9: Generic Event Type
`Event<T>` requires `T: EnvelopeEncodable + TryFrom<Envelope> + Debug + Clone + PartialEq`. In Kotlin, use `Event<T>` with type bounds `where T : EnvelopeEncodable` and runtime checks.

### H10: Complex Equality Semantics
`Envelope` implements `PartialEq` via `is_identical_to()` which is O(m+n) structural comparison. `Eq` is NOT implemented because of the cost. In Kotlin, override `equals()` to match Rust behavior. Consider the implications for `hashCode()` consistency.

### H11: Assertion Sorting by Digest
Assertions in a Node are sorted by their digest for deterministic output. Ensure the sorting uses the same byte-comparison order as Rust.

### H12: CBOR Encoding/Decoding Fidelity
Leaf envelopes encode their content as tagged CBOR (#6.24 wrapping encoded CBOR bytes). This double-encoding must be preserved exactly for cross-language compatibility.

### H13: Expected Text Output
18 of 21 test files use expected text output assertions. The formatting must exactly match Rust output including whitespace, indentation, Unicode bracket characters (double-angle brackets for functions, heavy-angle brackets for parameters), and digest abbreviations.

### H14: `impl_attachable!` and `impl_edgeable!` Macros
These macros generate boilerplate for types implementing `Attachable` and `Edgeable`. In Kotlin, implement the interfaces directly with delegation or manual boilerplate.

### H15: Response Uses Rust `Result` Internally
`Response` wraps `std::result::Result<(ARID, Envelope), (Option<ARID>, Envelope)>`. In Kotlin, model as a sealed class with `Success(id, result)` and `Failure(id?, error)` variants, or use a similar internal representation.

### H16: Deterministic Test RNG
Many tests use `fake_random_data()`, `fake_content_key()`, `fake_nonce()` based on a deterministic seed. The internal `_using` variants of salting/SSKR functions accept an RNG parameter. These must produce identical bytes to Rust for test vector compatibility.

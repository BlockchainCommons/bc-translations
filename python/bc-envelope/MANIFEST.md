# Translation Manifest: bc-envelope 0.43.0 → Python

## Crate Overview

Gordian Envelope for Rust. A hierarchical binary data format built on deterministic CBOR (dCBOR) with a Merkle-like digest tree. Supports selective disclosure via elision, encryption, and compression; digital signatures; SSKR social recovery; expression-based RPC; metadata attachments; typed edges; and inclusion proofs. All default features enabled.

Total source: ~12,000 lines (base ~5,100, format ~1,870, extensions ~5,000+).

## External Dependencies

| Rust Crate | Python Equivalent |
|---|---|
| bc-rand | `bc_rand` (sibling) |
| bc-crypto | `bc_crypto` (sibling) |
| dcbor | `dcbor` (sibling) |
| bc-ur | `bc_ur` (sibling) |
| bc-components | `bc_components` (sibling) |
| known-values | `known_values` (sibling) |
| paste | Not needed (Python has no macro equivalent) |
| hex | `bytes.fromhex()` / `.hex()` |
| itertools | Python stdlib `itertools` or builtins |
| thiserror | Python exception hierarchy |
| bytes | `bytes` / `bytearray` |
| ssh-key | Handled by bc_components SSH support |
| hex-literal | `bytes.fromhex()` |
| lazy_static | Not needed (Python module-level globals are lazy) |
| indoc | `textwrap.dedent()` |

## Feature Mapping

| Rust Feature | Default? | Python Approach |
|---|---|---|
| attachment | Yes | Always enabled |
| compress | Yes | Always enabled |
| edge | Yes | Always enabled |
| ed25519 | Yes | Always enabled (bc_crypto/bc_components have ed25519) |
| encrypt | Yes | Always enabled |
| expression | Yes | Always enabled |
| known_value | Yes | Always enabled |
| known-values-directory-loading | Yes | Always enabled |
| multithreaded | Yes | N/A (Python GIL; no Rc vs Arc distinction) |
| pqcrypto | Yes | Always enabled (bc_components has ML-KEM/ML-DSA) |
| proof | Yes | Always enabled |
| recipient | Yes | Always enabled |
| salt | Yes | Always enabled |
| secp256k1 | Yes | Always enabled (bc_crypto/bc_components have secp256k1) |
| secret | Yes | Always enabled |
| signature | Yes | Always enabled |
| ssh | Yes | Always enabled (bc_components has SSH support) |
| sskr | Yes | Always enabled |
| types | Yes | Always enabled |

## Public API Surface

### Core Types

| Rust Type | Python Type | Notes |
|---|---|---|
| `Envelope` (struct wrapping `RefCounted<EnvelopeCase>`) | `Envelope` (class) | Python uses reference semantics by default; no Rc/Arc needed |
| `EnvelopeCase` (enum: Node, Leaf, Wrapped, Assertion, Elided, KnownValue, Encrypted, Compressed) | `EnvelopeCase` (class hierarchy or tagged union) | 8 variants; use dataclasses or `__match_args__` for pattern matching |
| `Assertion` (struct: predicate, object, digest) | `Assertion` (class) | Digest computed from predicate+object digests |
| `Error` (thiserror enum, ~30 variants) | `EnvelopeError` (exception hierarchy) | Feature-gated variants always present in Python |
| `Result<T>` (type alias) | Standard Python exceptions | Raise `EnvelopeError` subclasses |
| `ObscureAction` (enum: Elide, Encrypt, Compress) | `ObscureAction` (class hierarchy or enum) | Encrypt carries SymmetricKey |
| `ObscureType` (enum: Elided, Encrypted, Compressed) | `ObscureType` (enum) | `from enum import Enum` |
| `EdgeType` (enum: None, Subject, Assertion, Predicate, Object, Content) | `EdgeType` (enum) | Walk traversal edge labels |
| `FormatContext` | `FormatContext` (class) | Module-level global singleton + custom instances |
| `FormatContextOpt` (enum: None, Global, Custom) | `FormatContextOpt` (class hierarchy or enum) | |
| `TreeFormatOpts` | `TreeFormatOpts` (dataclass) | `@dataclass` |
| `MermaidFormatOpts` | `MermaidFormatOpts` (dataclass) | `@dataclass` |
| `MermaidOrientation` (enum) | `MermaidOrientation` (enum) | |
| `MermaidTheme` (enum) | `MermaidTheme` (enum) | |
| `DigestDisplayFormat` (enum) | `DigestDisplayFormat` (enum) | |
| `EnvelopeSummary` | `EnvelopeSummary` (class) | |
| `SignatureMetadata` (struct) | `SignatureMetadata` (class) | |
| `SigningOptions` (struct) | `SigningOptions` (dataclass) | `@dataclass` |

### Expression Types

| Rust Type | Python Type | Notes |
|---|---|---|
| `Expression` (struct: function, envelope) | `Expression` (class) | |
| `ExpressionBehavior` (trait) | Protocol or mixin class | `typing.Protocol` or base class with default impls |
| `IntoExpression` (trait) | Not needed (use `to_expression()` method) | Python doesn't need blanket-impl pattern |
| `Function` (enum: Known, Named) | `Function` (class) | CBOR tag #6.40006 |
| `FunctionName` (enum: Static, Dynamic) | Not needed (just use `str` in Python) | |
| `FunctionsStore` | `FunctionsStore` (class) | |
| `Parameter` (enum: Known, Named) | `Parameter` (class) | CBOR tag #6.40007 |
| `ParametersStore` | `ParametersStore` (class) | |
| `Request` (struct: body, id, note, date) | `Request` (class) | CBOR tag #6.40010 |
| `RequestBehavior` (trait) | Protocol or mixin class | |
| `Response` (struct wrapping Result) | `Response` (class) | CBOR tag #6.40011 |
| `ResponseBehavior` (trait) | Protocol or mixin class | |
| `Event<T>` (generic struct) | `Event[T]` (generic class) | `typing.Generic[T]` |
| `EventBehavior<T>` (trait) | Protocol or mixin class | |

### Attachment & Edge Types

| Rust Type | Python Type | Notes |
|---|---|---|
| `Attachments` (struct) | `Attachments` (class) | `dict[Digest, Envelope]` container |
| `Attachable` (trait) | Protocol or mixin class | |
| `Edges` (struct) | `Edges` (class) | `dict[Digest, Envelope]` container |
| `Edgeable` (trait) | Protocol or mixin class | |

### Traits → Protocols / Mixins

| Rust Trait | Python Equivalent | Notes |
|---|---|---|
| `EnvelopeEncodable` | `EnvelopeEncodable` (Protocol or mixin) | `into_envelope()` / `to_envelope()` |
| `DigestProvider` | `DigestProvider` (from bc_components) | Already exists |
| `CBORTagged` | `CBORTagged` (from dcbor) | Already exists |
| `CBORTaggedEncodable` | `CBORTaggedEncodable` (from dcbor) | Already exists |
| `CBORTaggedDecodable` | `CBORTaggedDecodable` (from dcbor) | Already exists |
| `ExpressionBehavior` | `ExpressionBehavior` | 8 methods |
| `RequestBehavior` | `RequestBehavior` extends `ExpressionBehavior` | |
| `ResponseBehavior` | `ResponseBehavior` | 12+ methods |
| `EventBehavior` | `EventBehavior` | |
| `Attachable` | `Attachable` | |
| `Edgeable` | `Edgeable` | |

### EnvelopeEncodable Implementations

The `EnvelopeEncodable` trait has blanket and explicit implementations for these types. In Python, implement via `envelope_encodable()` helper with type dispatch or monkey-patching `to_envelope()` onto types:

- Primitives: `int`, `bool`, `float`
- Strings: `str`
- Binary: `bytes`, `bytearray`
- CBOR: `CBOR`
- Collections: `list`, `dict`, `set`
- bc_components types: `Digest`, `Salt`, `Nonce`, `ARID`, `URI`, `UUID`, `XID`, `Reference`, `Date`, `JSON`
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
- `null()`, `true_value()`, `false_value()`, `unit()` — static factory helpers (avoid shadowing Python builtins)

#### Assertion Management
- `add_assertion(predicate, object)` → `add_assertion(predicate, object)`
- `add_assertion_envelope(assertion)` → `add_assertion_envelope(assertion)`
- `add_assertion_envelopes(assertions)` → `add_assertion_envelopes(assertions)`
- `add_optional_assertion_envelope(assertion?)` → `add_optional_assertion_envelope(assertion)`
- `add_optional_assertion(predicate, object?)` → `add_optional_assertion(predicate, object)`
- `add_nonempty_string_assertion(predicate, string)` → `add_nonempty_string_assertion(predicate, string)`
- `add_assertions(predicate, objects)` → `add_assertions(predicate, objects)`
- `add_assertion_if(condition, predicate, object)` → `add_assertion_if(condition, predicate, object)`
- `add_assertion_envelope_if(condition, assertion)` → `add_assertion_envelope_if(condition, assertion)`
- `remove_assertion(assertion)` → `remove_assertion(assertion)`
- `replace_assertion(old, new)` → `replace_assertion(old, new)`
- `replace_subject(new_subject)` → `replace_subject(new_subject)`

#### Salted Assertions (salt feature)
- `add_assertion_salted(predicate, object)` → `add_assertion_salted(predicate, object)`
- `add_assertion_envelope_salted(assertion)` → `add_assertion_envelope_salted(assertion)`
- `add_optional_assertion_envelope_salted(assertion?)` → `add_optional_assertion_envelope_salted(assertion)`
- `add_assertions_salted(predicate, objects)` → `add_assertions_salted(predicate, objects)`
- Internal `_using` variants for deterministic RNG testing

#### Structural Queries
- `subject()` → `subject` (property)
- `assertions()` → `assertions` (property)
- `has_assertions()` → `has_assertions` (property)
- `is_assertion` / `is_encrypted` / `is_compressed` / `is_elided` / `is_leaf` / `is_node` / `is_wrapped` (properties)
- `is_subject_assertion` / `is_subject_encrypted` / `is_subject_compressed` / `is_subject_elided` / `is_subject_obscured` (properties)
- `as_assertion()` / `try_assertion()` — access as Assertion
- `as_predicate()` / `try_predicate()` — access predicate
- `as_object()` / `try_object()` — access object
- `as_leaf()` / `try_leaf()` — access leaf CBOR
- `elements_count` — recursive element count (property)

#### Content Extraction
- `extract_subject(type)` → `extract_subject(cls)` — runtime type dispatch
- `extract_object(type)` / `extract_predicate(type)`
- `object_for_predicate(predicate)` → `object_for_predicate(predicate)`
- `objects_for_predicate(predicate)` → `objects_for_predicate(predicate)`
- `assertions_with_predicate(predicate)` → `assertions_with_predicate(predicate)`
- `assertion_with_predicate(predicate)` → `assertion_with_predicate(predicate)`
- `extract_object_for_predicate(type, predicate)` → `extract_object_for_predicate(cls, predicate)`
- `extract_optional_object_for_predicate(type, predicate)` → `extract_optional_object_for_predicate(cls, predicate)`
- `extract_object_for_predicate_with_default(type, predicate, default)` → `extract_object_for_predicate_with_default(cls, predicate, default)`
- `extract_objects_for_predicate(type, predicate)` → `extract_objects_for_predicate(cls, predicate)`

#### Leaf Helpers
- `is_null` / `is_true` / `is_false` / `is_bool` / `is_number` / `is_nan` (properties)
- `is_subject_number` / `is_subject_nan` (properties)
- `try_byte_string()` / `as_byte_string()`
- `as_array()` / `as_map()` / `as_text()`

#### Known Value Helpers
- `as_known_value()` / `try_known_value()` / `is_known_value` (property)
- `is_subject_unit` / `check_subject_unit()` (property + method)

#### Digest Operations
- `digest` → `digest` (property via DigestProvider)
- `digests(level_limit)` → `digests(level_limit)`
- `deep_digests()` → `deep_digests()`
- `shallow_digests()` → `shallow_digests()`
- `structural_digest()` → `structural_digest()`
- `is_equivalent_to(other)` → `is_equivalent_to(other)` (O(1) via digests)
- `is_identical_to(other)` → `is_identical_to(other)` (O(m+n) structural)

#### Wrapping
- `wrap()` → `wrap()`
- `try_unwrap()` → `unwrap()` (raises on failure)

#### Elision & Obscuration
- `elide()` → `elide()`
- `elide_set_with_action(set, removing, action)` — core recursive impl
- `elide_removing_set(set)` / `elide_revealing_set(set)`
- `elide_removing_array(array)` / `elide_revealing_array(array)`
- `elide_removing_target(target)` / `elide_revealing_target(target)`
- Variants with `_with_action` suffix for Encrypt/Compress obscuration
- `unelide(original)` → `unelide(original)`
- `nodes_matching(set, types)` → `nodes_matching(set, types)`
- `walk_unelide(lookup_set)` → `walk_unelide(lookup_set)`
- `walk_replace(target_digests, replacement)` → `walk_replace(target_digests, replacement)`
- `walk_decrypt(keys)` → `walk_decrypt(keys)` — recursive decrypt
- `walk_decompress(targets?)` → `walk_decompress(targets)` — recursive decompress

#### Walk/Visitor
- `walk(hide_nodes, state, visit)` → `walk(hide_nodes, state, visit)`
- Visitor type: `Callable[[Envelope, int, EdgeType, S], tuple[S, bool]]`

#### Encryption (encrypt feature)
- `encrypt_subject(key)` / `encrypt_subject_opt(key, nonce)` → `encrypt_subject(key)` / `encrypt_subject(key, nonce=...)`
- `decrypt_subject(key)` → `decrypt_subject(key)`
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
- `sskr_split(spec, key?)` → `sskr_split(spec, key=None)`
- `sskr_split_flattened(threshold, share_count, key?)` → `sskr_split_flattened(threshold, share_count, key=None)`
- `sskr_join(shares)` → `Envelope.sskr_join(shares)` (classmethod)

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
- `hex_format()` — hex-encoded CBOR bytes
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
- `_GLOBAL_FORMAT_CONTEXT` — module-level global FormatContext (thread-safe via `threading.Lock` if needed)
- `_GLOBAL_FUNCTIONS` — module-level global FunctionsStore
- `_GLOBAL_PARAMETERS` — module-level global ParametersStore
- `register_tags()` — registers standard tags in global format context
- `register_tags_in(context)` — registers tags in a specific context
- `with_format_context` / `with_format_context_mut` — context manager or helper functions for accessing global context

In Python, use module-level globals initialized at import time. Python module-level globals are inherently lazy (loaded on first import). Use `threading.Lock` for mutation if needed.

## Documentation Catalog

### Module-Level Documentation
- Crate-level doc (`lib.rs`): Comprehensive overview of Gordian Envelope with examples
- Expressions module doc: Envelope expression syntax and semantics
- Attachments module doc: Metadata attachment infrastructure
- Edges module doc: Edge container for verifiable claims (BCR-2026-003)

### Type-Level Documentation
- All public types have docstrings
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
- `tests/common/mod.rs`: `assert_actual_expected!` macro → `assert actual == expected` or helper function
- `tests/common/check_encoding.rs`: `CheckEncoding` trait → `check_encoding()` helper function for CBOR round-trip verification
- `tests/common/test_data.rs`: Helper functions: `hello_envelope()`, `alice_*()`, `bob_*()`, `carol_*()`, `fake_content_key()`, `fake_nonce()`, `credential()`, `redacted_credential()`
- `tests/common/test_seed.rs`: `Seed` domain object example with Envelope <-> Seed conversion

## Expected Text Output Rubric

Applicable: yes

Source signals: 18 of 21 Rust test files use multi-line expected text output assertions.
Target test areas: format_tests, elision_tests, obscuring_tests, proof_tests, edge_tests, crypto_tests, signature_tests, compression_tests, non_correlation_tests, type_tests, attachment_tests, expression/request/response/event tests.

## Translation Units (Dependency Order)

### Unit 1: Error Types
- `base/error.rs` → `_error.py`
- Exception hierarchy mirroring all ~30 Error variants
- `Error::msg(String)` → `EnvelopeError(message)` base class
- `From<Error> for dcbor::Error` conversion

### Unit 2: Core Envelope Structure
- `base/envelope.rs` → `_envelope.py` (partial), `_envelope_case.py`
- `EnvelopeCase` class hierarchy with 8 variants (use dataclasses or `__match_args__`)
- `Envelope` class wrapping `EnvelopeCase`
- Constructors: `__init__`, class methods for `new_assertion`, `new_leaf`, `new_wrapped`, `new_elided`, `new_with_known_value`, `new_with_encrypted`, `new_with_compressed`
- `new_or_null`, `new_or_none` factory helpers
- `null()`, `true_value()`, `false_value()`, `unit()`, `unknown()`, `ok()` class methods

### Unit 3: Assertion
- `base/assertion.rs` → `_assertion.py`
- Predicate + object envelope pair with computed digest
- CBOR encoding as single-element Map
- DigestProvider implementation

### Unit 4: EnvelopeEncodable
- `base/envelope_encodable.rs` + `base/envelope_decodable.rs` → `_envelope_encodable.py`
- `envelope_encodable()` dispatch function or monkey-patched `to_envelope()` methods
- Implementations for all primitive types, collections, bc_components types
- Decodable: `extract_subject(cls)` with type-based dispatch

### Unit 5: Leaf Helpers
- `base/leaf.rs` → `_leaf.py`
- Boolean, null, number, byte-string, collection access helpers
- Known-value helpers (`as_known_value`, `is_known_value`, `is_subject_unit`, etc.)

### Unit 6: Queries
- `base/queries.rs` → `_queries.py`
- Type checks: `is_assertion`, `is_encrypted`, `is_compressed`, etc. (properties)
- Subject type checks: `is_subject_assertion`, `is_subject_encrypted`, etc. (properties)
- Content extraction: `extract_subject(cls)`, `object_for_predicate()`, `assertions_with_predicate()`, etc.
- `elements_count` recursive counter (property)
- Position methods: `set_position()`, `position()`, `remove_position()`

### Unit 7: Digest Operations
- `base/digest.rs` → `_digest_ops.py`
- DigestProvider implementation for Envelope
- `digests()`, `deep_digests()`, `shallow_digests()`, `structural_digest()`
- `is_equivalent_to()`, `is_identical_to()`
- `__eq__()` / `__hash__()` using `is_identical_to()`

### Unit 8: CBOR Serialization
- `base/cbor.rs` → `_cbor.py`
- `CBORTagged`, `CBORTaggedEncodable`, `CBORTaggedDecodable` implementations
- Tag: `TAG_ENVELOPE` (#6.200)
- Per-case encoding/decoding rules

### Unit 9: Walk / Visitor
- `base/walk.rs` → `_walk.py`
- `EdgeType` enum
- Visitor callable type: `Callable[[Envelope, int, EdgeType, S], tuple[S, bool]]`
- `walk()`, `walk_structure()`, `walk_tree()` methods

### Unit 10: Elision & Obscuration
- `base/elide.rs` → `_elide.py`
- `ObscureAction` class hierarchy, `ObscureType` enum
- Core: `elide()`, `elide_set_with_action()`, `elide_removing_set()`, `elide_revealing_set()`, etc.
- Walk-based: `unelide()`, `walk_unelide()`, `walk_replace()`, `walk_decrypt()`, `walk_decompress()`
- `nodes_matching()` helper

### Unit 11: Wrap / Unwrap
- `base/wrap.rs` → `_wrap.py`
- `wrap()`, `unwrap()` (raises `EnvelopeError.NotWrapped`)

### Unit 12: Assertion Management
- `base/assertions.rs` → `_assertions.py`
- All `add_assertion*`, `remove_assertion`, `replace_assertion`, `replace_subject` methods
- Conditional and optional assertion variants
- Salted assertion variants (with `_using` variants for deterministic testing)

### Unit 13: String Utilities
- `string_utils.rs` → `_string_utils.py` (internal)
- `flanked_by(text, left, right)` utility function

### Unit 14: Format Context & Tag Registration
- `format/format_context.rs` → `_format_context.py`
- `FormatContextOpt` class hierarchy
- Module-level global context singleton
- `register_tags()` / `register_tags_in(context)` functions
- `with_format_context()` / `with_format_context_mut()` helper functions or context managers

### Unit 15: Envelope Notation (Format)
- `format/notation.rs` → `_notation.py`
- Core tree-format rendering with known-value, function, parameter annotation
- Internal format options configuration

### Unit 16: Tree Format
- `format/tree.rs` → `_tree_format.py`
- `TreeFormatOpts` dataclass
- `tree_format()` / `tree_format_opt()` methods

### Unit 17: Envelope Summary
- `format/envelope_summary.rs` → `_envelope_summary.py`
- Short text summaries per case

### Unit 18: Diagnostic & Hex Format
- `format/diagnostic.rs` → `_diagnostic.py`
- `format/hex.rs` → `_hex_format.py`
- `diagnostic()`, `diagnostic_annotated()`, `hex_format()` methods

### Unit 19: Mermaid Format
- `format/mermaid.rs` → `_mermaid.py`
- `MermaidFormatOpts`, `MermaidOrientation`, `MermaidTheme`
- `mermaid()` method

### Unit 20: Encryption Extension
- `extension/encrypt.rs` → `_encrypt.py`
- `encrypt_subject(key)` / `encrypt_subject(key, nonce=...)` / `decrypt_subject(key)`
- `encrypt(key)` / `decrypt(key)` convenience wrappers

### Unit 21: Compression Extension
- `extension/compress.rs` → `_compress.py`
- `compress()` / `decompress()` / `compress_subject()` / `decompress_subject()`

### Unit 22: Salt Extension
- `extension/salt.rs` → `_salt.py`
- `add_salt()` / `add_salt_instance(salt)` / `add_salt_with_len(count)` / `add_salt_in_range(range)`
- Internal `_using` variants for deterministic RNG

### Unit 23: Signature Extension
- `extension/signature/signature_impl.rs` → `_signature.py`
- `extension/signature/signature_metadata.rs` → `_signature_metadata.py`
- All `add_signature*`, `verify_signature*`, `has_signature_from*` methods
- `sign()` / `verify()` convenience wrappers
- `SigningOptions` dataclass

### Unit 24: Recipient Extension
- `extension/recipient.rs` → `_recipient.py`
- `add_recipient()`, `recipients()`, `encrypt_subject_to_recipients()`, etc.

### Unit 25: Secret Extension
- `extension/secret.rs` → `_secret.py`
- `lock_subject()` / `unlock_subject()` / `lock()` / `unlock()` / `add_secret()`
- `is_locked_with_password()` / `is_locked_with_ssh_agent()`

### Unit 26: SSKR Extension
- `extension/sskr.rs` → `_sskr.py`
- `sskr_split()` / `sskr_split_flattened()` / `sskr_join()` (classmethod)

### Unit 27: Proof Extension
- `extension/proof.rs` → `_proof.py`
- `proof_contains_set()` / `proof_contains_target()` / `confirm_contains_set()` / `confirm_contains_target()`

### Unit 28: Types Extension
- `extension/types.rs` → `_types_ext.py`
- `add_type()` / `types()` / `get_type()` / `has_type()` / `has_type_value()` / `check_type_value()` / `check_type()`

### Unit 29: Attachment Extension
- `extension/attachment/` → `_attachment.py`
- `Attachments` container class
- `Attachable` protocol/mixin with default implementations
- Envelope methods: `new_attachment()`, `attachments()`, `attachment_payload()`, etc.

### Unit 30: Edge Extension
- `extension/edge/` → `_edge.py`
- `Edges` container class
- `Edgeable` protocol/mixin with default implementations
- Envelope methods: `new_edge()`, `edges()`, `edge_payload()`, etc.

### Unit 31: Expression System
- `extension/expressions/function.rs` → `_function.py`
- `extension/expressions/functions.rs` → `_functions.py` (constants + global store)
- `extension/expressions/functions_store.rs` → (integrated into `_functions.py`)
- `extension/expressions/parameter.rs` → `_parameter.py`
- `extension/expressions/parameters.rs` → `_parameters.py` (constants + global store)
- `extension/expressions/parameters_store.rs` → (integrated into `_parameters.py`)
- `extension/expressions/expression.rs` → `_expression.py`
- `extension/expressions/request.rs` → `_request.py`
- `extension/expressions/response.rs` → `_response.py`
- `extension/expressions/event.rs` → `_event.py`

### Unit 32: Seal
- `seal.rs` → `_seal.py`
- `seal()` / `seal_opt()` / `unseal()` combined sign+encrypt operations

### Unit 33: Package Init
- `prelude.rs` → `__init__.py`
- Re-export all public types and functions
- Monkey-patch `to_envelope()` / `from_envelope()` onto bc_components types at import time

### Unit 34: Test Infrastructure
- `tests/common/mod.rs` → `tests/conftest.py` + `tests/test_helpers.py`
- `tests/common/check_encoding.rs` → `check_encoding()` helper function
- `tests/common/test_data.rs` → `tests/test_data.py` (test helpers)
- `tests/common/test_seed.rs` → `tests/test_seed.py` (domain object example)

### Unit 35: Integration Tests
- All 21 test files translated to pytest test modules
- Expected text output assertions via `textwrap.dedent()` comparisons

## Translation Hazards

### H1: Reference Counting → Python References
Rust uses `Rc<EnvelopeCase>` (or `Arc` with multithreaded). In Python, all objects are reference-counted by the GC. No special handling needed, but `Envelope` should still be an immutable value type (all mutation returns new instances).

### H2: Rust `paste!` Macro for Constants
The `function_constant!` and `parameter_constant!` macros use `paste!` to generate companion `_VALUE` constants. In Python, declare both constants manually as module-level variables.

### H3: TypeId-Based Runtime Type Extraction
`extract_subject<T>()` in Rust uses `TypeId::of::<T>()` for runtime type matching against special types (CBOR, Envelope, Assertion, KnownValue, Function, Parameter). In Python, use `isinstance()` checks or a type-dispatch dict with explicit class keys.

### H4: Visitor Closures with Mutable State
`walk()` takes a `Fn` closure as `Visitor`. In Python, use `Callable[[Envelope, int, EdgeType, S], tuple[S, bool]]` type. Python closures can capture mutable state from enclosing scope directly (via nonlocal or mutable containers).

### H5: Blanket Trait Implementations
`EnvelopeEncodable` has a blanket impl for `T: Into<Envelope> + Clone`. Python can't do blanket impls. Instead, use a dispatch function (`envelope_encodable()`) with `isinstance()` checks, or monkey-patch `to_envelope()` onto supported types at import time.

### H6: Macro-Generated Type Conversions
`impl_envelope_encodable!` and `impl_envelope_decodable!` macros generate conversions for ~25 types. In Python, write explicit dispatch logic or a registration decorator.

### H7: Feature-Gated Code
All Rust features are conditionally compiled. In Python, all code is always imported (features always enabled). Remove all `#[cfg(feature = "...")]` guards.

### H8: Thread-Safe Global State
`GLOBAL_FORMAT_CONTEXT`, `GLOBAL_FUNCTIONS`, `GLOBAL_PARAMETERS` use `Mutex` + `Once`. In Python, use module-level globals. The GIL provides basic thread safety for simple operations. Use `threading.Lock` for mutation sequences that need atomicity.

### H9: Generic Event Type
`Event<T>` requires `T: EnvelopeEncodable + TryFrom<Envelope> + Debug + Clone + PartialEq`. In Python, use `Event[T]` with `typing.Generic[T]` and runtime type checks.

### H10: Complex Equality Semantics
`Envelope` implements `PartialEq` via `is_identical_to()` which is O(m+n) structural comparison. `Eq` is NOT implemented because of the cost. In Python, override `__eq__()` to match Rust behavior. Implement `__hash__()` based on digest for dict/set usage.

### H11: Assertion Sorting by Digest
Assertions in a Node are sorted by their digest for deterministic output. Ensure the sorting uses the same byte-comparison order as Rust.

### H12: CBOR Encoding/Decoding Fidelity
Leaf envelopes encode their content as tagged CBOR (#6.24 wrapping encoded CBOR bytes). This double-encoding must be preserved exactly for cross-language compatibility.

### H13: Expected Text Output
18 of 21 test files use expected text output assertions. The formatting must exactly match Rust output including whitespace, indentation, Unicode bracket characters (double-angle brackets for functions, heavy-angle brackets for parameters), and digest abbreviations.

### H14: `impl_attachable!` and `impl_edgeable!` Macros
These macros generate boilerplate for types implementing `Attachable` and `Edgeable`. In Python, implement as mixin classes with default method implementations or use monkey-patching.

### H15: Response Uses Rust `Result` Internally
`Response` wraps `std::result::Result<(ARID, Envelope), (Option<ARID>, Envelope)>`. In Python, model with a `_result` attribute that is either a success tuple `(ARID, Envelope)` or a failure tuple `(Optional[ARID], Envelope)`, with `is_success` / `is_failure` properties.

### H16: Deterministic Test RNG
Many tests use `fake_random_data()`, `fake_content_key()`, `fake_nonce()` based on a deterministic seed. The internal `_using` variants of salting/SSKR functions accept an RNG parameter. These must produce identical bytes to Rust for test vector compatibility.

### H17: Python Built-in Name Shadowing
Rust's `Envelope::true()`, `Envelope::false()`, `Envelope::null()` shadow Python keywords. Use `Envelope.true_value()`, `Envelope.false_value()` or similar naming to avoid conflicts. Also watch for `type` (use `type_` or `add_type`).

### H18: Monkey-Patching for EnvelopeEncodable
Python lacks extension methods. The idiomatic approach is to monkey-patch `to_envelope()` / `from_envelope()` methods onto bc_components types in `__init__.py` at import time, similar to how the sibling bc_components package handles cross-cutting concerns.

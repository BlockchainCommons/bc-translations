# Translation Manifest: bc-tags v0.12.0 (TypeScript)

## Crate Overview

`bc-tags` defines Blockchain Commons CBOR semantic tag constants and helper registration functions built on top of `dcbor`.

Rust source: `rust/bc-tags`

## Dependencies

### Internal BC Dependencies

- `dcbor` (required) — TypeScript package `@bc/dcbor`

### External Dependencies (Rust -> TypeScript)

| Rust crate | Purpose | TypeScript equivalent |
|---|---|---|
| `paste` | Macro identifier composition (`const_cbor_tag!`, `cbor_tag!`) | Not required; declare constants directly |

### TypeScript-Specific Notes

- Runtime dependency on `@bc/dcbor` (local file reference).
- No third-party dependency beyond dcbor.
- Package name: `@bc/tags`.

## Feature Flags

Rust features:

- No crate-defined feature flags.
- `dcbor` feature behavior is inherited from the dependency.

Initial TypeScript scope:

- Translate full default behavior (entire crate surface).

## Public API Surface

### Re-Exports

- `pub use dcbor::prelude::*;` in Rust: TypeScript package does not re-export dcbor types. Users import @bc/dcbor separately. This matches TypeScript convention (explicit imports).

### Functions

- `registerTagsIn(tagsStore: TagsStore): void` — registers all 75 bc-tags plus dcbor base tags
- `registerTags(): void` — registers all tags in the global tag store

### Constants

150 constants total (75 numeric `TAG_*` + 75 string `TAG_NAME_*`), following TypeScript SCREAMING_SNAKE_CASE convention.

| Symbol | Value |
|---|---|
| `TAG_URI` | `32` |
| `TAG_NAME_URI` | `"url"` |
| `TAG_UUID` | `37` |
| `TAG_NAME_UUID` | `"uuid"` |
| `TAG_ENCODED_CBOR` | `24` |
| `TAG_NAME_ENCODED_CBOR` | `"encoded-cbor"` |
| `TAG_ENVELOPE` | `200` |
| `TAG_NAME_ENVELOPE` | `"envelope"` |
| `TAG_LEAF` | `201` |
| `TAG_NAME_LEAF` | `"leaf"` |
| `TAG_JSON` | `262` |
| `TAG_NAME_JSON` | `"json"` |
| `TAG_KNOWN_VALUE` | `40000` |
| `TAG_NAME_KNOWN_VALUE` | `"known-value"` |
| `TAG_DIGEST` | `40001` |
| `TAG_NAME_DIGEST` | `"digest"` |
| `TAG_ENCRYPTED` | `40002` |
| `TAG_NAME_ENCRYPTED` | `"encrypted"` |
| `TAG_COMPRESSED` | `40003` |
| `TAG_NAME_COMPRESSED` | `"compressed"` |
| `TAG_REQUEST` | `40004` |
| `TAG_NAME_REQUEST` | `"request"` |
| `TAG_RESPONSE` | `40005` |
| `TAG_NAME_RESPONSE` | `"response"` |
| `TAG_FUNCTION` | `40006` |
| `TAG_NAME_FUNCTION` | `"function"` |
| `TAG_PARAMETER` | `40007` |
| `TAG_NAME_PARAMETER` | `"parameter"` |
| `TAG_PLACEHOLDER` | `40008` |
| `TAG_NAME_PLACEHOLDER` | `"placeholder"` |
| `TAG_REPLACEMENT` | `40009` |
| `TAG_NAME_REPLACEMENT` | `"replacement"` |
| `TAG_X25519_PRIVATE_KEY` | `40010` |
| `TAG_NAME_X25519_PRIVATE_KEY` | `"agreement-private-key"` |
| `TAG_X25519_PUBLIC_KEY` | `40011` |
| `TAG_NAME_X25519_PUBLIC_KEY` | `"agreement-public-key"` |
| `TAG_ARID` | `40012` |
| `TAG_NAME_ARID` | `"arid"` |
| `TAG_PRIVATE_KEYS` | `40013` |
| `TAG_NAME_PRIVATE_KEYS` | `"crypto-prvkeys"` |
| `TAG_NONCE` | `40014` |
| `TAG_NAME_NONCE` | `"nonce"` |
| `TAG_PASSWORD` | `40015` |
| `TAG_NAME_PASSWORD` | `"password"` |
| `TAG_PRIVATE_KEY_BASE` | `40016` |
| `TAG_NAME_PRIVATE_KEY_BASE` | `"crypto-prvkey-base"` |
| `TAG_PUBLIC_KEYS` | `40017` |
| `TAG_NAME_PUBLIC_KEYS` | `"crypto-pubkeys"` |
| `TAG_SALT` | `40018` |
| `TAG_NAME_SALT` | `"salt"` |
| `TAG_SEALED_MESSAGE` | `40019` |
| `TAG_NAME_SEALED_MESSAGE` | `"crypto-sealed"` |
| `TAG_SIGNATURE` | `40020` |
| `TAG_NAME_SIGNATURE` | `"signature"` |
| `TAG_SIGNING_PRIVATE_KEY` | `40021` |
| `TAG_NAME_SIGNING_PRIVATE_KEY` | `"signing-private-key"` |
| `TAG_SIGNING_PUBLIC_KEY` | `40022` |
| `TAG_NAME_SIGNING_PUBLIC_KEY` | `"signing-public-key"` |
| `TAG_SYMMETRIC_KEY` | `40023` |
| `TAG_NAME_SYMMETRIC_KEY` | `"crypto-key"` |
| `TAG_XID` | `40024` |
| `TAG_NAME_XID` | `"xid"` |
| `TAG_REFERENCE` | `40025` |
| `TAG_NAME_REFERENCE` | `"reference"` |
| `TAG_EVENT` | `40026` |
| `TAG_NAME_EVENT` | `"event"` |
| `TAG_ENCRYPTED_KEY` | `40027` |
| `TAG_NAME_ENCRYPTED_KEY` | `"encrypted-key"` |
| `TAG_MLKEM_PRIVATE_KEY` | `40100` |
| `TAG_NAME_MLKEM_PRIVATE_KEY` | `"mlkem-private-key"` |
| `TAG_MLKEM_PUBLIC_KEY` | `40101` |
| `TAG_NAME_MLKEM_PUBLIC_KEY` | `"mlkem-public-key"` |
| `TAG_MLKEM_CIPHERTEXT` | `40102` |
| `TAG_NAME_MLKEM_CIPHERTEXT` | `"mlkem-ciphertext"` |
| `TAG_MLDSA_PRIVATE_KEY` | `40103` |
| `TAG_NAME_MLDSA_PRIVATE_KEY` | `"mldsa-private-key"` |
| `TAG_MLDSA_PUBLIC_KEY` | `40104` |
| `TAG_NAME_MLDSA_PUBLIC_KEY` | `"mldsa-public-key"` |
| `TAG_MLDSA_SIGNATURE` | `40105` |
| `TAG_NAME_MLDSA_SIGNATURE` | `"mldsa-signature"` |
| `TAG_SEED` | `40300` |
| `TAG_NAME_SEED` | `"seed"` |
| `TAG_HDKEY` | `40303` |
| `TAG_NAME_HDKEY` | `"hdkey"` |
| `TAG_DERIVATION_PATH` | `40304` |
| `TAG_NAME_DERIVATION_PATH` | `"keypath"` |
| `TAG_USE_INFO` | `40305` |
| `TAG_NAME_USE_INFO` | `"coin-info"` |
| `TAG_EC_KEY` | `40306` |
| `TAG_NAME_EC_KEY` | `"eckey"` |
| `TAG_ADDRESS` | `40307` |
| `TAG_NAME_ADDRESS` | `"address"` |
| `TAG_OUTPUT_DESCRIPTOR` | `40308` |
| `TAG_NAME_OUTPUT_DESCRIPTOR` | `"output-descriptor"` |
| `TAG_SSKR_SHARE` | `40309` |
| `TAG_NAME_SSKR_SHARE` | `"sskr"` |
| `TAG_PSBT` | `40310` |
| `TAG_NAME_PSBT` | `"psbt"` |
| `TAG_ACCOUNT_DESCRIPTOR` | `40311` |
| `TAG_NAME_ACCOUNT_DESCRIPTOR` | `"account-descriptor"` |
| `TAG_SSH_TEXT_PRIVATE_KEY` | `40800` |
| `TAG_NAME_SSH_TEXT_PRIVATE_KEY` | `"ssh-private"` |
| `TAG_SSH_TEXT_PUBLIC_KEY` | `40801` |
| `TAG_NAME_SSH_TEXT_PUBLIC_KEY` | `"ssh-public"` |
| `TAG_SSH_TEXT_SIGNATURE` | `40802` |
| `TAG_NAME_SSH_TEXT_SIGNATURE` | `"ssh-signature"` |
| `TAG_SSH_TEXT_CERTIFICATE` | `40803` |
| `TAG_NAME_SSH_TEXT_CERTIFICATE` | `"ssh-certificate"` |
| `TAG_PROVENANCE_MARK` | `1347571542` |
| `TAG_NAME_PROVENANCE_MARK` | `"provenance"` |
| `TAG_SEED_V1` | `300` |
| `TAG_NAME_SEED_V1` | `"crypto-seed"` |
| `TAG_EC_KEY_V1` | `306` |
| `TAG_NAME_EC_KEY_V1` | `"crypto-eckey"` |
| `TAG_SSKR_SHARE_V1` | `309` |
| `TAG_NAME_SSKR_SHARE_V1` | `"crypto-sskr"` |
| `TAG_HDKEY_V1` | `303` |
| `TAG_NAME_HDKEY_V1` | `"crypto-hdkey"` |
| `TAG_DERIVATION_PATH_V1` | `304` |
| `TAG_NAME_DERIVATION_PATH_V1` | `"crypto-keypath"` |
| `TAG_USE_INFO_V1` | `305` |
| `TAG_NAME_USE_INFO_V1` | `"crypto-coin-info"` |
| `TAG_OUTPUT_DESCRIPTOR_V1` | `307` |
| `TAG_NAME_OUTPUT_DESCRIPTOR_V1` | `"crypto-output"` |
| `TAG_PSBT_V1` | `310` |
| `TAG_NAME_PSBT_V1` | `"crypto-psbt"` |
| `TAG_ACCOUNT_V1` | `311` |
| `TAG_NAME_ACCOUNT_V1` | `"crypto-account"` |
| `TAG_OUTPUT_SCRIPT_HASH` | `400` |
| `TAG_NAME_OUTPUT_SCRIPT_HASH` | `"output-script-hash"` |
| `TAG_OUTPUT_WITNESS_SCRIPT_HASH` | `401` |
| `TAG_NAME_OUTPUT_WITNESS_SCRIPT_HASH` | `"output-witness-script-hash"` |
| `TAG_OUTPUT_PUBLIC_KEY` | `402` |
| `TAG_NAME_OUTPUT_PUBLIC_KEY` | `"output-public-key"` |
| `TAG_OUTPUT_PUBLIC_KEY_HASH` | `403` |
| `TAG_NAME_OUTPUT_PUBLIC_KEY_HASH` | `"output-public-key-hash"` |
| `TAG_OUTPUT_WITNESS_PUBLIC_KEY_HASH` | `404` |
| `TAG_NAME_OUTPUT_WITNESS_PUBLIC_KEY_HASH` | `"output-witness-public-key-hash"` |
| `TAG_OUTPUT_COMBO` | `405` |
| `TAG_NAME_OUTPUT_COMBO` | `"output-combo"` |
| `TAG_OUTPUT_MULTISIG` | `406` |
| `TAG_NAME_OUTPUT_MULTISIG` | `"output-multisig"` |
| `TAG_OUTPUT_SORTED_MULTISIG` | `407` |
| `TAG_NAME_OUTPUT_SORTED_MULTISIG` | `"output-sorted-multisig"` |
| `TAG_OUTPUT_RAW_SCRIPT` | `408` |
| `TAG_NAME_OUTPUT_RAW_SCRIPT` | `"output-raw-script"` |
| `TAG_OUTPUT_TAPROOT` | `409` |
| `TAG_NAME_OUTPUT_TAPROOT` | `"output-taproot"` |
| `TAG_OUTPUT_COSIGNER` | `410` |
| `TAG_NAME_OUTPUT_COSIGNER` | `"output-cosigner"` |

### Registration Set

`registerTagsIn` inserts **75** bc-tags tags into `TagsStore` after invoking dcbor's `registerTagsIn`.

Registration order (must be preserved):

1. URI, 2. UUID, 3. EncodedCBOR, 4. Envelope, 5. Leaf, 6. JSON,
7. KnownValue, 8. Digest, 9. Encrypted, 10. Compressed,
11. Request, 12. Response, 13. Function, 14. Parameter,
15. Placeholder, 16. Replacement, 17. Event,
18. SeedV1, 19. ECKeyV1, 20. SSKRShareV1,
21. Seed, 22. ECKey, 23. SSKRShare,
24. X25519PrivateKey, 25. X25519PublicKey, 26. ARID,
27. PrivateKeys, 28. Nonce, 29. Password, 30. PrivateKeyBase,
31. PublicKeys, 32. Salt, 33. SealedMessage, 34. Signature,
35. SigningPrivateKey, 36. SigningPublicKey, 37. SymmetricKey,
38. XID, 39. Reference, 40. EncryptedKey,
41. MLKEMPrivateKey, 42. MLKEMPublicKey, 43. MLKEMCiphertext,
44. MLDSAPrivateKey, 45. MLDSAPublicKey, 46. MLDSASignature,
47. HDKeyV1, 48. DerivationPathV1, 49. UseInfoV1,
50. OutputDescriptorV1, 51. PSBTV1, 52. AccountV1,
53. HDKey, 54. DerivationPath, 55. UseInfo, 56. Address,
57. OutputDescriptor, 58. PSBT, 59. AccountDescriptor,
60. SSHTextPrivateKey, 61. SSHTextPublicKey, 62. SSHTextSignature,
63. SSHTextCertificate,
64. OutputScriptHash, 65. OutputWitnessScriptHash, 66. OutputPublicKey,
67. OutputPublicKeyHash, 68. OutputWitnessPublicKeyHash,
69. OutputCombo, 70. OutputMultisig, 71. OutputSortedMultisig,
72. OutputRawScript, 73. OutputTaproot, 74. OutputCosigner,
75. ProvenanceMark

## Test Inventory

Rust test sources:

- No `#[test]` functions in `rust/bc-tags/src/`.
- No integration tests under `rust/bc-tags/tests/`.

Tests authored in TypeScript:

- Constant parity spot-checks for representative `TAG_*` and `TAG_NAME_*` pairs.
- `bcTags` array length and content match against expected tags.
- `registerTagsIn` behavior: dcbor base tags, forward lookup, reverse lookup.
- `registerTagsIn` idempotency.
- `registerTags` global-store behavior.
- Uniqueness of all tag values and names.
- `nameForValue` lookups for specific values.

## Translation Unit Order

1. Project scaffold (`.gitignore`, `package.json`, `tsconfig.json`, `vitest.config.ts`)
2. Tag constant registry (`src/tags-registry.ts`)
3. Barrel exports (`src/index.ts`)
4. Tests (`tests/tags-registry.test.ts`)

## Translation Hazards

1. Rust macros generate two constants per tag; TypeScript must declare each pair explicitly.
2. `registerTagsIn` must call dcbor's `registerTagsIn` first to preserve inherited tag behavior (date, bignum tags).
3. `registerTags` must mutate the global dcbor tag store via `getGlobalTagsStore()`.
4. Tag insertion order and membership must match Rust list exactly (75 entries).
5. Deprecated tags are still actively registered; do not omit them.
6. TypeScript constant naming: SCREAMING_SNAKE_CASE `TAG_*` prefix (matching dcbor TS convention).
7. Some TAG_* values overlap with dcbor's exports (TAG_ENCODED_CBOR = 24, TAG_URI = 32, TAG_UUID = 37); bc-tags defines its own with name constants.

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: no

Reason:

- `bc-tags` has no Rust formatting/rendering tests and no complex textual output behavior; scope is constants + registry side effects.

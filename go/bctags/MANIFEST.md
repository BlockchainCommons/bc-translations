# Translation Manifest: bc-tags v0.12.0 (Go)

## Crate Overview

`bc-tags` defines Blockchain Commons CBOR semantic tag constants and helper registration functions built on top of `dcbor`.

Rust source: `rust/bc-tags`

## Dependencies

### Internal BC Dependencies

- `dcbor` (required) — Go package `github.com/nickel-blockchaincommons/dcbor-go`

### External Dependencies (Rust -> Go)

| Rust crate | Purpose | Go equivalent |
|---|---|---|
| `paste` | Macro identifier composition (`const_cbor_tag!`, `cbor_tag!`) | Not required; declare constants directly |

### Go-Specific Notes

- Runtime dependency on `github.com/nickel-blockchaincommons/dcbor-go` (Go dcbor package).
- No third-party dependency beyond dcbor and stdlib.
- Module path: `github.com/nickel-blockchaincommons/bctags-go`, package `bctags`.

## Feature Flags

Rust features:

- No crate-defined feature flags.
- `dcbor` feature behavior is inherited from the dependency.

Initial Go scope:

- Translate full default behavior (entire crate surface).

## Public API Surface

### Re-Exports

- `pub use dcbor::prelude::*;` in Rust: Go package does not re-export dcbor types. Users import dcbor separately. This matches Go convention (no wildcard imports/re-exports).

### Functions

- `RegisterTagsIn(tagsStore *dcbor.TagsStore)` — registers all 75 bc-tags plus dcbor base tags
- `RegisterTags()` — registers all tags in the global tag store

### Constants

150 constants total (75 numeric `Tag*` + 75 string `TagName*`), following Go dcbor naming convention (PascalCase, no underscore separators).

| Symbol | Value |
|---|---|
| `TagURI` | `32` |
| `TagNameURI` | `"url"` |
| `TagUUID` | `37` |
| `TagNameUUID` | `"uuid"` |
| `TagEncodedCBOR` | `24` |
| `TagNameEncodedCBOR` | `"encoded-cbor"` |
| `TagEnvelope` | `200` |
| `TagNameEnvelope` | `"envelope"` |
| `TagLeaf` | `201` |
| `TagNameLeaf` | `"leaf"` |
| `TagJSON` | `262` |
| `TagNameJSON` | `"json"` |
| `TagKnownValue` | `40000` |
| `TagNameKnownValue` | `"known-value"` |
| `TagDigest` | `40001` |
| `TagNameDigest` | `"digest"` |
| `TagEncrypted` | `40002` |
| `TagNameEncrypted` | `"encrypted"` |
| `TagCompressed` | `40003` |
| `TagNameCompressed` | `"compressed"` |
| `TagRequest` | `40004` |
| `TagNameRequest` | `"request"` |
| `TagResponse` | `40005` |
| `TagNameResponse` | `"response"` |
| `TagFunction` | `40006` |
| `TagNameFunction` | `"function"` |
| `TagParameter` | `40007` |
| `TagNameParameter` | `"parameter"` |
| `TagPlaceholder` | `40008` |
| `TagNamePlaceholder` | `"placeholder"` |
| `TagReplacement` | `40009` |
| `TagNameReplacement` | `"replacement"` |
| `TagX25519PrivateKey` | `40010` |
| `TagNameX25519PrivateKey` | `"agreement-private-key"` |
| `TagX25519PublicKey` | `40011` |
| `TagNameX25519PublicKey` | `"agreement-public-key"` |
| `TagARID` | `40012` |
| `TagNameARID` | `"arid"` |
| `TagPrivateKeys` | `40013` |
| `TagNamePrivateKeys` | `"crypto-prvkeys"` |
| `TagNonce` | `40014` |
| `TagNameNonce` | `"nonce"` |
| `TagPassword` | `40015` |
| `TagNamePassword` | `"password"` |
| `TagPrivateKeyBase` | `40016` |
| `TagNamePrivateKeyBase` | `"crypto-prvkey-base"` |
| `TagPublicKeys` | `40017` |
| `TagNamePublicKeys` | `"crypto-pubkeys"` |
| `TagSalt` | `40018` |
| `TagNameSalt` | `"salt"` |
| `TagSealedMessage` | `40019` |
| `TagNameSealedMessage` | `"crypto-sealed"` |
| `TagSignature` | `40020` |
| `TagNameSignature` | `"signature"` |
| `TagSigningPrivateKey` | `40021` |
| `TagNameSigningPrivateKey` | `"signing-private-key"` |
| `TagSigningPublicKey` | `40022` |
| `TagNameSigningPublicKey` | `"signing-public-key"` |
| `TagSymmetricKey` | `40023` |
| `TagNameSymmetricKey` | `"crypto-key"` |
| `TagXID` | `40024` |
| `TagNameXID` | `"xid"` |
| `TagReference` | `40025` |
| `TagNameReference` | `"reference"` |
| `TagEvent` | `40026` |
| `TagNameEvent` | `"event"` |
| `TagEncryptedKey` | `40027` |
| `TagNameEncryptedKey` | `"encrypted-key"` |
| `TagMLKEMPrivateKey` | `40100` |
| `TagNameMLKEMPrivateKey` | `"mlkem-private-key"` |
| `TagMLKEMPublicKey` | `40101` |
| `TagNameMLKEMPublicKey` | `"mlkem-public-key"` |
| `TagMLKEMCiphertext` | `40102` |
| `TagNameMLKEMCiphertext` | `"mlkem-ciphertext"` |
| `TagMLDSAPrivateKey` | `40103` |
| `TagNameMLDSAPrivateKey` | `"mldsa-private-key"` |
| `TagMLDSAPublicKey` | `40104` |
| `TagNameMLDSAPublicKey` | `"mldsa-public-key"` |
| `TagMLDSASignature` | `40105` |
| `TagNameMLDSASignature` | `"mldsa-signature"` |
| `TagSeed` | `40300` |
| `TagNameSeed` | `"seed"` |
| `TagHDKey` | `40303` |
| `TagNameHDKey` | `"hdkey"` |
| `TagDerivationPath` | `40304` |
| `TagNameDerivationPath` | `"keypath"` |
| `TagUseInfo` | `40305` |
| `TagNameUseInfo` | `"coin-info"` |
| `TagECKey` | `40306` |
| `TagNameECKey` | `"eckey"` |
| `TagAddress` | `40307` |
| `TagNameAddress` | `"address"` |
| `TagOutputDescriptor` | `40308` |
| `TagNameOutputDescriptor` | `"output-descriptor"` |
| `TagSSKRShare` | `40309` |
| `TagNameSSKRShare` | `"sskr"` |
| `TagPSBT` | `40310` |
| `TagNamePSBT` | `"psbt"` |
| `TagAccountDescriptor` | `40311` |
| `TagNameAccountDescriptor` | `"account-descriptor"` |
| `TagSSHTextPrivateKey` | `40800` |
| `TagNameSSHTextPrivateKey` | `"ssh-private"` |
| `TagSSHTextPublicKey` | `40801` |
| `TagNameSSHTextPublicKey` | `"ssh-public"` |
| `TagSSHTextSignature` | `40802` |
| `TagNameSSHTextSignature` | `"ssh-signature"` |
| `TagSSHTextCertificate` | `40803` |
| `TagNameSSHTextCertificate` | `"ssh-certificate"` |
| `TagProvenanceMark` | `1347571542` |
| `TagNameProvenanceMark` | `"provenance"` |
| `TagSeedV1` | `300` |
| `TagNameSeedV1` | `"crypto-seed"` |
| `TagECKeyV1` | `306` |
| `TagNameECKeyV1` | `"crypto-eckey"` |
| `TagSSKRShareV1` | `309` |
| `TagNameSSKRShareV1` | `"crypto-sskr"` |
| `TagHDKeyV1` | `303` |
| `TagNameHDKeyV1` | `"crypto-hdkey"` |
| `TagDerivationPathV1` | `304` |
| `TagNameDerivationPathV1` | `"crypto-keypath"` |
| `TagUseInfoV1` | `305` |
| `TagNameUseInfoV1` | `"crypto-coin-info"` |
| `TagOutputDescriptorV1` | `307` |
| `TagNameOutputDescriptorV1` | `"crypto-output"` |
| `TagPSBTV1` | `310` |
| `TagNamePSBTV1` | `"crypto-psbt"` |
| `TagAccountV1` | `311` |
| `TagNameAccountV1` | `"crypto-account"` |
| `TagOutputScriptHash` | `400` |
| `TagNameOutputScriptHash` | `"output-script-hash"` |
| `TagOutputWitnessScriptHash` | `401` |
| `TagNameOutputWitnessScriptHash` | `"output-witness-script-hash"` |
| `TagOutputPublicKey` | `402` |
| `TagNameOutputPublicKey` | `"output-public-key"` |
| `TagOutputPublicKeyHash` | `403` |
| `TagNameOutputPublicKeyHash` | `"output-public-key-hash"` |
| `TagOutputWitnessPublicKeyHash` | `404` |
| `TagNameOutputWitnessPublicKeyHash` | `"output-witness-public-key-hash"` |
| `TagOutputCombo` | `405` |
| `TagNameOutputCombo` | `"output-combo"` |
| `TagOutputMultisig` | `406` |
| `TagNameOutputMultisig` | `"output-multisig"` |
| `TagOutputSortedMultisig` | `407` |
| `TagNameOutputSortedMultisig` | `"output-sorted-multisig"` |
| `TagOutputRawScript` | `408` |
| `TagNameOutputRawScript` | `"output-raw-script"` |
| `TagOutputTaproot` | `409` |
| `TagNameOutputTaproot` | `"output-taproot"` |
| `TagOutputCosigner` | `410` |
| `TagNameOutputCosigner` | `"output-cosigner"` |

### Registration Set

`RegisterTagsIn` inserts **75** bc-tags tags into `TagsStore` after invoking `dcbor.RegisterTagsIn`.

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

Tests to author in Go:

- Constant parity checks for all `Tag*` and `TagName*` pairs.
- `RegisterTagsIn` behavior checks (including inherited `dcbor` tag registration and idempotency).
- `RegisterTags` global-store behavior checks.

## Translation Unit Order

1. Project scaffold (`.gitignore`, `go.mod`)
2. Tag constant registry (`tags_registry.go`)
3. Package documentation (`doc.go`)
4. Tests (`tags_registry_test.go`)

## Translation Hazards

1. Rust macros generate two constants per tag; Go must declare each pair explicitly.
2. `RegisterTagsIn` must call `dcbor.RegisterTagsIn` first to preserve inherited tag behavior (date, bignum tags).
3. `RegisterTags` must mutate the global dcbor tag store via `dcbor.WithTags`.
4. Tag insertion order and membership must match Rust list exactly (75 entries).
5. Deprecated tags are still actively registered; do not omit them.
6. Go constant naming: PascalCase `Tag*` prefix (matching dcbor convention), not `TAG_*` snake_case.

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: no

Reason:

- `bc-tags` has no Rust formatting/rendering tests and no complex textual output behavior; scope is constants + registry side effects.

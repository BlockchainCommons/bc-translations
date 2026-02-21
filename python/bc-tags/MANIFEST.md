# Translation Manifest: bc-tags v0.12.0 (Python)

## Crate Overview

`bc-tags` defines Blockchain Commons CBOR semantic tag constants and helper registration functions built on top of `dcbor`.

Rust source: `rust/bc-tags`

## Dependencies

### Internal BC Dependencies

- `dcbor` (required)

### External Dependencies (Rust -> Python)

| Rust crate | Purpose | Python equivalent |
|---|---|---|
| `paste` | Macro identifier composition (`const_cbor_tag!`, `cbor_tag!`) | Not required in Python; generate constants and `Tag(...)` objects directly |

### Python-Specific Notes

- Runtime dependency on `dcbor==0.25.1`.
- No third-party dependency beyond `dcbor` and stdlib.

## Feature Flags

Rust features:

- No crate-defined feature flags.
- `dcbor` feature behavior is inherited from the dependency.

Initial Python scope:

- Translate full default behavior (entire crate surface).

## Public API Surface

### Re-Exports

- `pub use dcbor::prelude::*;` in Rust: Python package should re-export the `dcbor` public API for parity.

### Functions

- `register_tags_in(tags_store: TagsStore) -> None`
- `register_tags() -> None`

### Constants

Macro-generated constant pairs: **150** total (`TAG_*` numeric + `TAG_NAME_*` string).

| Symbol | Value |
|---|---|
| `TAG_URI` | `32` |
| `TAG_NAME_URI` | `url` |
| `TAG_UUID` | `37` |
| `TAG_NAME_UUID` | `uuid` |
| `TAG_ENCODED_CBOR` | `24` |
| `TAG_NAME_ENCODED_CBOR` | `encoded-cbor` |
| `TAG_ENVELOPE` | `200` |
| `TAG_NAME_ENVELOPE` | `envelope` |
| `TAG_LEAF` | `201` |
| `TAG_NAME_LEAF` | `leaf` |
| `TAG_JSON` | `262` |
| `TAG_NAME_JSON` | `json` |
| `TAG_KNOWN_VALUE` | `40000` |
| `TAG_NAME_KNOWN_VALUE` | `known-value` |
| `TAG_DIGEST` | `40001` |
| `TAG_NAME_DIGEST` | `digest` |
| `TAG_ENCRYPTED` | `40002` |
| `TAG_NAME_ENCRYPTED` | `encrypted` |
| `TAG_COMPRESSED` | `40003` |
| `TAG_NAME_COMPRESSED` | `compressed` |
| `TAG_REQUEST` | `40004` |
| `TAG_NAME_REQUEST` | `request` |
| `TAG_RESPONSE` | `40005` |
| `TAG_NAME_RESPONSE` | `response` |
| `TAG_FUNCTION` | `40006` |
| `TAG_NAME_FUNCTION` | `function` |
| `TAG_PARAMETER` | `40007` |
| `TAG_NAME_PARAMETER` | `parameter` |
| `TAG_PLACEHOLDER` | `40008` |
| `TAG_NAME_PLACEHOLDER` | `placeholder` |
| `TAG_REPLACEMENT` | `40009` |
| `TAG_NAME_REPLACEMENT` | `replacement` |
| `TAG_X25519_PRIVATE_KEY` | `40010` |
| `TAG_NAME_X25519_PRIVATE_KEY` | `agreement-private-key` |
| `TAG_X25519_PUBLIC_KEY` | `40011` |
| `TAG_NAME_X25519_PUBLIC_KEY` | `agreement-public-key` |
| `TAG_ARID` | `40012` |
| `TAG_NAME_ARID` | `arid` |
| `TAG_PRIVATE_KEYS` | `40013` |
| `TAG_NAME_PRIVATE_KEYS` | `crypto-prvkeys` |
| `TAG_NONCE` | `40014` |
| `TAG_NAME_NONCE` | `nonce` |
| `TAG_PASSWORD` | `40015` |
| `TAG_NAME_PASSWORD` | `password` |
| `TAG_PRIVATE_KEY_BASE` | `40016` |
| `TAG_NAME_PRIVATE_KEY_BASE` | `crypto-prvkey-base` |
| `TAG_PUBLIC_KEYS` | `40017` |
| `TAG_NAME_PUBLIC_KEYS` | `crypto-pubkeys` |
| `TAG_SALT` | `40018` |
| `TAG_NAME_SALT` | `salt` |
| `TAG_SEALED_MESSAGE` | `40019` |
| `TAG_NAME_SEALED_MESSAGE` | `crypto-sealed` |
| `TAG_SIGNATURE` | `40020` |
| `TAG_NAME_SIGNATURE` | `signature` |
| `TAG_SIGNING_PRIVATE_KEY` | `40021` |
| `TAG_NAME_SIGNING_PRIVATE_KEY` | `signing-private-key` |
| `TAG_SIGNING_PUBLIC_KEY` | `40022` |
| `TAG_NAME_SIGNING_PUBLIC_KEY` | `signing-public-key` |
| `TAG_SYMMETRIC_KEY` | `40023` |
| `TAG_NAME_SYMMETRIC_KEY` | `crypto-key` |
| `TAG_XID` | `40024` |
| `TAG_NAME_XID` | `xid` |
| `TAG_REFERENCE` | `40025` |
| `TAG_NAME_REFERENCE` | `reference` |
| `TAG_EVENT` | `40026` |
| `TAG_NAME_EVENT` | `event` |
| `TAG_ENCRYPTED_KEY` | `40027` |
| `TAG_NAME_ENCRYPTED_KEY` | `encrypted-key` |
| `TAG_MLKEM_PRIVATE_KEY` | `40100` |
| `TAG_NAME_MLKEM_PRIVATE_KEY` | `mlkem-private-key` |
| `TAG_MLKEM_PUBLIC_KEY` | `40101` |
| `TAG_NAME_MLKEM_PUBLIC_KEY` | `mlkem-public-key` |
| `TAG_MLKEM_CIPHERTEXT` | `40102` |
| `TAG_NAME_MLKEM_CIPHERTEXT` | `mlkem-ciphertext` |
| `TAG_MLDSA_PRIVATE_KEY` | `40103` |
| `TAG_NAME_MLDSA_PRIVATE_KEY` | `mldsa-private-key` |
| `TAG_MLDSA_PUBLIC_KEY` | `40104` |
| `TAG_NAME_MLDSA_PUBLIC_KEY` | `mldsa-public-key` |
| `TAG_MLDSA_SIGNATURE` | `40105` |
| `TAG_NAME_MLDSA_SIGNATURE` | `mldsa-signature` |
| `TAG_SEED` | `40300` |
| `TAG_NAME_SEED` | `seed` |
| `TAG_HDKEY` | `40303` |
| `TAG_NAME_HDKEY` | `hdkey` |
| `TAG_DERIVATION_PATH` | `40304` |
| `TAG_NAME_DERIVATION_PATH` | `keypath` |
| `TAG_USE_INFO` | `40305` |
| `TAG_NAME_USE_INFO` | `coin-info` |
| `TAG_EC_KEY` | `40306` |
| `TAG_NAME_EC_KEY` | `eckey` |
| `TAG_ADDRESS` | `40307` |
| `TAG_NAME_ADDRESS` | `address` |
| `TAG_OUTPUT_DESCRIPTOR` | `40308` |
| `TAG_NAME_OUTPUT_DESCRIPTOR` | `output-descriptor` |
| `TAG_SSKR_SHARE` | `40309` |
| `TAG_NAME_SSKR_SHARE` | `sskr` |
| `TAG_PSBT` | `40310` |
| `TAG_NAME_PSBT` | `psbt` |
| `TAG_ACCOUNT_DESCRIPTOR` | `40311` |
| `TAG_NAME_ACCOUNT_DESCRIPTOR` | `account-descriptor` |
| `TAG_SSH_TEXT_PRIVATE_KEY` | `40800` |
| `TAG_NAME_SSH_TEXT_PRIVATE_KEY` | `ssh-private` |
| `TAG_SSH_TEXT_PUBLIC_KEY` | `40801` |
| `TAG_NAME_SSH_TEXT_PUBLIC_KEY` | `ssh-public` |
| `TAG_SSH_TEXT_SIGNATURE` | `40802` |
| `TAG_NAME_SSH_TEXT_SIGNATURE` | `ssh-signature` |
| `TAG_SSH_TEXT_CERTIFICATE` | `40803` |
| `TAG_NAME_SSH_TEXT_CERTIFICATE` | `ssh-certificate` |
| `TAG_PROVENANCE_MARK` | `1347571542` |
| `TAG_NAME_PROVENANCE_MARK` | `provenance` |
| `TAG_SEED_V1` | `300` |
| `TAG_NAME_SEED_V1` | `crypto-seed` |
| `TAG_EC_KEY_V1` | `306` |
| `TAG_NAME_EC_KEY_V1` | `crypto-eckey` |
| `TAG_SSKR_SHARE_V1` | `309` |
| `TAG_NAME_SSKR_SHARE_V1` | `crypto-sskr` |
| `TAG_HDKEY_V1` | `303` |
| `TAG_NAME_HDKEY_V1` | `crypto-hdkey` |
| `TAG_DERIVATION_PATH_V1` | `304` |
| `TAG_NAME_DERIVATION_PATH_V1` | `crypto-keypath` |
| `TAG_USE_INFO_V1` | `305` |
| `TAG_NAME_USE_INFO_V1` | `crypto-coin-info` |
| `TAG_OUTPUT_DESCRIPTOR_V1` | `307` |
| `TAG_NAME_OUTPUT_DESCRIPTOR_V1` | `crypto-output` |
| `TAG_PSBT_V1` | `310` |
| `TAG_NAME_PSBT_V1` | `crypto-psbt` |
| `TAG_ACCOUNT_V1` | `311` |
| `TAG_NAME_ACCOUNT_V1` | `crypto-account` |
| `TAG_OUTPUT_SCRIPT_HASH` | `400` |
| `TAG_NAME_OUTPUT_SCRIPT_HASH` | `output-script-hash` |
| `TAG_OUTPUT_WITNESS_SCRIPT_HASH` | `401` |
| `TAG_NAME_OUTPUT_WITNESS_SCRIPT_HASH` | `output-witness-script-hash` |
| `TAG_OUTPUT_PUBLIC_KEY` | `402` |
| `TAG_NAME_OUTPUT_PUBLIC_KEY` | `output-public-key` |
| `TAG_OUTPUT_PUBLIC_KEY_HASH` | `403` |
| `TAG_NAME_OUTPUT_PUBLIC_KEY_HASH` | `output-public-key-hash` |
| `TAG_OUTPUT_WITNESS_PUBLIC_KEY_HASH` | `404` |
| `TAG_NAME_OUTPUT_WITNESS_PUBLIC_KEY_HASH` | `output-witness-public-key-hash` |
| `TAG_OUTPUT_COMBO` | `405` |
| `TAG_NAME_OUTPUT_COMBO` | `output-combo` |
| `TAG_OUTPUT_MULTISIG` | `406` |
| `TAG_NAME_OUTPUT_MULTISIG` | `output-multisig` |
| `TAG_OUTPUT_SORTED_MULTISIG` | `407` |
| `TAG_NAME_OUTPUT_SORTED_MULTISIG` | `output-sorted-multisig` |
| `TAG_OUTPUT_RAW_SCRIPT` | `408` |
| `TAG_NAME_OUTPUT_RAW_SCRIPT` | `output-raw-script` |
| `TAG_OUTPUT_TAPROOT` | `409` |
| `TAG_NAME_OUTPUT_TAPROOT` | `output-taproot` |
| `TAG_OUTPUT_COSIGNER` | `410` |
| `TAG_NAME_OUTPUT_COSIGNER` | `output-cosigner` |

### Registration Set

`register_tags_in` inserts **75** `bc-tags` tags into `TagsStore` after invoking `dcbor::register_tags_in`.

Registration order (must be preserved):

1. `TAG_URI` / `TAG_NAME_URI`
2. `TAG_UUID` / `TAG_NAME_UUID`
3. `TAG_ENCODED_CBOR` / `TAG_NAME_ENCODED_CBOR`
4. `TAG_ENVELOPE` / `TAG_NAME_ENVELOPE`
5. `TAG_LEAF` / `TAG_NAME_LEAF`
6. `TAG_JSON` / `TAG_NAME_JSON`
7. `TAG_KNOWN_VALUE` / `TAG_NAME_KNOWN_VALUE`
8. `TAG_DIGEST` / `TAG_NAME_DIGEST`
9. `TAG_ENCRYPTED` / `TAG_NAME_ENCRYPTED`
10. `TAG_COMPRESSED` / `TAG_NAME_COMPRESSED`
11. `TAG_REQUEST` / `TAG_NAME_REQUEST`
12. `TAG_RESPONSE` / `TAG_NAME_RESPONSE`
13. `TAG_FUNCTION` / `TAG_NAME_FUNCTION`
14. `TAG_PARAMETER` / `TAG_NAME_PARAMETER`
15. `TAG_PLACEHOLDER` / `TAG_NAME_PLACEHOLDER`
16. `TAG_REPLACEMENT` / `TAG_NAME_REPLACEMENT`
17. `TAG_EVENT` / `TAG_NAME_EVENT`
18. `TAG_SEED_V1` / `TAG_NAME_SEED_V1`
19. `TAG_EC_KEY_V1` / `TAG_NAME_EC_KEY_V1`
20. `TAG_SSKR_SHARE_V1` / `TAG_NAME_SSKR_SHARE_V1`
21. `TAG_SEED` / `TAG_NAME_SEED`
22. `TAG_EC_KEY` / `TAG_NAME_EC_KEY`
23. `TAG_SSKR_SHARE` / `TAG_NAME_SSKR_SHARE`
24. `TAG_X25519_PRIVATE_KEY` / `TAG_NAME_X25519_PRIVATE_KEY`
25. `TAG_X25519_PUBLIC_KEY` / `TAG_NAME_X25519_PUBLIC_KEY`
26. `TAG_ARID` / `TAG_NAME_ARID`
27. `TAG_PRIVATE_KEYS` / `TAG_NAME_PRIVATE_KEYS`
28. `TAG_NONCE` / `TAG_NAME_NONCE`
29. `TAG_PASSWORD` / `TAG_NAME_PASSWORD`
30. `TAG_PRIVATE_KEY_BASE` / `TAG_NAME_PRIVATE_KEY_BASE`
31. `TAG_PUBLIC_KEYS` / `TAG_NAME_PUBLIC_KEYS`
32. `TAG_SALT` / `TAG_NAME_SALT`
33. `TAG_SEALED_MESSAGE` / `TAG_NAME_SEALED_MESSAGE`
34. `TAG_SIGNATURE` / `TAG_NAME_SIGNATURE`
35. `TAG_SIGNING_PRIVATE_KEY` / `TAG_NAME_SIGNING_PRIVATE_KEY`
36. `TAG_SIGNING_PUBLIC_KEY` / `TAG_NAME_SIGNING_PUBLIC_KEY`
37. `TAG_SYMMETRIC_KEY` / `TAG_NAME_SYMMETRIC_KEY`
38. `TAG_XID` / `TAG_NAME_XID`
39. `TAG_REFERENCE` / `TAG_NAME_REFERENCE`
40. `TAG_ENCRYPTED_KEY` / `TAG_NAME_ENCRYPTED_KEY`
41. `TAG_MLKEM_PRIVATE_KEY` / `TAG_NAME_MLKEM_PRIVATE_KEY`
42. `TAG_MLKEM_PUBLIC_KEY` / `TAG_NAME_MLKEM_PUBLIC_KEY`
43. `TAG_MLKEM_CIPHERTEXT` / `TAG_NAME_MLKEM_CIPHERTEXT`
44. `TAG_MLDSA_PRIVATE_KEY` / `TAG_NAME_MLDSA_PRIVATE_KEY`
45. `TAG_MLDSA_PUBLIC_KEY` / `TAG_NAME_MLDSA_PUBLIC_KEY`
46. `TAG_MLDSA_SIGNATURE` / `TAG_NAME_MLDSA_SIGNATURE`
47. `TAG_HDKEY_V1` / `TAG_NAME_HDKEY_V1`
48. `TAG_DERIVATION_PATH_V1` / `TAG_NAME_DERIVATION_PATH_V1`
49. `TAG_USE_INFO_V1` / `TAG_NAME_USE_INFO_V1`
50. `TAG_OUTPUT_DESCRIPTOR_V1` / `TAG_NAME_OUTPUT_DESCRIPTOR_V1`
51. `TAG_PSBT_V1` / `TAG_NAME_PSBT_V1`
52. `TAG_ACCOUNT_V1` / `TAG_NAME_ACCOUNT_V1`
53. `TAG_HDKEY` / `TAG_NAME_HDKEY`
54. `TAG_DERIVATION_PATH` / `TAG_NAME_DERIVATION_PATH`
55. `TAG_USE_INFO` / `TAG_NAME_USE_INFO`
56. `TAG_ADDRESS` / `TAG_NAME_ADDRESS`
57. `TAG_OUTPUT_DESCRIPTOR` / `TAG_NAME_OUTPUT_DESCRIPTOR`
58. `TAG_PSBT` / `TAG_NAME_PSBT`
59. `TAG_ACCOUNT_DESCRIPTOR` / `TAG_NAME_ACCOUNT_DESCRIPTOR`
60. `TAG_SSH_TEXT_PRIVATE_KEY` / `TAG_NAME_SSH_TEXT_PRIVATE_KEY`
61. `TAG_SSH_TEXT_PUBLIC_KEY` / `TAG_NAME_SSH_TEXT_PUBLIC_KEY`
62. `TAG_SSH_TEXT_SIGNATURE` / `TAG_NAME_SSH_TEXT_SIGNATURE`
63. `TAG_SSH_TEXT_CERTIFICATE` / `TAG_NAME_SSH_TEXT_CERTIFICATE`
64. `TAG_OUTPUT_SCRIPT_HASH` / `TAG_NAME_OUTPUT_SCRIPT_HASH`
65. `TAG_OUTPUT_WITNESS_SCRIPT_HASH` / `TAG_NAME_OUTPUT_WITNESS_SCRIPT_HASH`
66. `TAG_OUTPUT_PUBLIC_KEY` / `TAG_NAME_OUTPUT_PUBLIC_KEY`
67. `TAG_OUTPUT_PUBLIC_KEY_HASH` / `TAG_NAME_OUTPUT_PUBLIC_KEY_HASH`
68. `TAG_OUTPUT_WITNESS_PUBLIC_KEY_HASH` / `TAG_NAME_OUTPUT_WITNESS_PUBLIC_KEY_HASH`
69. `TAG_OUTPUT_COMBO` / `TAG_NAME_OUTPUT_COMBO`
70. `TAG_OUTPUT_MULTISIG` / `TAG_NAME_OUTPUT_MULTISIG`
71. `TAG_OUTPUT_SORTED_MULTISIG` / `TAG_NAME_OUTPUT_SORTED_MULTISIG`
72. `TAG_OUTPUT_RAW_SCRIPT` / `TAG_NAME_OUTPUT_RAW_SCRIPT`
73. `TAG_OUTPUT_TAPROOT` / `TAG_NAME_OUTPUT_TAPROOT`
74. `TAG_OUTPUT_COSIGNER` / `TAG_NAME_OUTPUT_COSIGNER`
75. `TAG_PROVENANCE_MARK` / `TAG_NAME_PROVENANCE_MARK`

## Documentation Catalog

- Crate-level doc comment in `src/lib.rs`: no.
- Module-level doc comment in `src/tags_registry.rs`: yes (`//! # CBOR Tags Registry` with IANA range notes and Envelope/deprecated-tag rationale).
- Public items with doc comments: none (functions/constants are undocumented in Rust source).
- Package metadata description: `Blockchain Commons CBOR Tags`.
- README: yes (`rust/bc-tags/README.md`) with intro, getting started, and version history.

## Test Inventory

Rust test sources:

- No `#[test]` functions in `rust/bc-tags/src/`.
- No integration tests under `rust/bc-tags/tests/`.

Translation tests to author in Python:

- Constant parity checks for all `TAG_*` and `TAG_NAME_*` pairs.
- `register_tags_in` behavior checks (including inherited `dcbor` tag registration and idempotency).
- `register_tags` global-store behavior checks.

## Translation Unit Order

1. Project scaffold (`.gitignore`, `pyproject.toml`, package/test directories)
2. Tag constant registry module (`src/bc_tags/tags_registry.py`)
3. Public package exports (`src/bc_tags/__init__.py`)
4. Tests (`tests/test_tags_registry.py`)
5. Completeness and fluency passes

## Translation Hazards

1. Rust macros generate two constants per tag (`TAG_*`, `TAG_NAME_*`); Python must preserve exact names/values.
2. `register_tags_in` must call `dcbor.register_tags_in` first to preserve inherited tag behavior (e.g., date tag).
3. `register_tags` must mutate the global `dcbor` tag store via `with_tags_mut` equivalent.
4. Tag insertion order and membership must match Rust list exactly (75 entries).
5. Deprecated tags are still actively registered; do not omit them.

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: no

Reason:

- `bc-tags` has no Rust formatting/rendering tests and no complex textual output behavior; scope is constants + registry side effects.

## Completion Targets

- All 150 macro-generated constants translated exactly.
- `register_tags_in` and `register_tags` behavior matches Rust semantics.
- Python test suite validates constants, registration set, and global-store integration.
- Package metadata and module-level docs preserved where present in Rust.

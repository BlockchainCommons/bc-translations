# Translation Manifest: bc-tags → Kotlin (bc-tags)

## Crate Metadata
- Name: `bc-tags`
- Version: `0.12.0`
- Rust edition: `2024`
- Description: `Blockchain Commons CBOR Tags`
- Repository: <https://github.com/BlockchainCommons/bc-tags-rust>

## Dependencies
### Internal BC Dependencies
- `dcbor = ^0.25.0`
  - Kotlin equivalent: `com.blockchaincommons:dcbor:0.25.1`
  - Local development: composite build via `includeBuild("../dcbor")`

### External Dependencies
- `paste = ^1.0.12`
  - Rust-only macro helper used by `const_cbor_tag!`/`cbor_tag!` expansion.
  - Kotlin equivalent: none required; declare constants and `Tag(...)` construction directly.

## Feature Flags
- No crate features declared.
- Translation scope: default behavior only (entire crate).

## Public API Surface
### Type Catalog
- No public structs/enums/traits/type aliases in this crate.

### Function Catalog
- `register_tags_in(tags_store: &mut TagsStore)`
  - Kotlin: `fun registerTagsIn(tagsStore: TagsStore)`
  - Behavior: first delegates to `dcbor::register_tags_in`, then inserts all bc-tags tags.
- `register_tags()`
  - Kotlin: `fun registerTags()`
  - Behavior: mutates global tag store via `GlobalTags.withTagsMut`.

### Constant Catalog
`const_cbor_tag!(value, NAME, "name")` expands to two public constants per tag:
- `TAG_NAME: u64` (Kotlin `ULong`)
- `TAG_NAME_NAME: &str` (Kotlin `String`)

Total required public constants: **150** (`75` numeric + `75` name constants).

| Numeric Constant | Value | Name Constant | Name Literal |
|---|---:|---|---|
| `TAG_URI` | `32uL` | `TAG_NAME_URI` | `url` |
| `TAG_UUID` | `37uL` | `TAG_NAME_UUID` | `uuid` |
| `TAG_ENCODED_CBOR` | `24uL` | `TAG_NAME_ENCODED_CBOR` | `encoded-cbor` |
| `TAG_ENVELOPE` | `200uL` | `TAG_NAME_ENVELOPE` | `envelope` |
| `TAG_LEAF` | `201uL` | `TAG_NAME_LEAF` | `leaf` |
| `TAG_JSON` | `262uL` | `TAG_NAME_JSON` | `json` |
| `TAG_KNOWN_VALUE` | `40000uL` | `TAG_NAME_KNOWN_VALUE` | `known-value` |
| `TAG_DIGEST` | `40001uL` | `TAG_NAME_DIGEST` | `digest` |
| `TAG_ENCRYPTED` | `40002uL` | `TAG_NAME_ENCRYPTED` | `encrypted` |
| `TAG_COMPRESSED` | `40003uL` | `TAG_NAME_COMPRESSED` | `compressed` |
| `TAG_REQUEST` | `40004uL` | `TAG_NAME_REQUEST` | `request` |
| `TAG_RESPONSE` | `40005uL` | `TAG_NAME_RESPONSE` | `response` |
| `TAG_FUNCTION` | `40006uL` | `TAG_NAME_FUNCTION` | `function` |
| `TAG_PARAMETER` | `40007uL` | `TAG_NAME_PARAMETER` | `parameter` |
| `TAG_PLACEHOLDER` | `40008uL` | `TAG_NAME_PLACEHOLDER` | `placeholder` |
| `TAG_REPLACEMENT` | `40009uL` | `TAG_NAME_REPLACEMENT` | `replacement` |
| `TAG_X25519_PRIVATE_KEY` | `40010uL` | `TAG_NAME_X25519_PRIVATE_KEY` | `agreement-private-key` |
| `TAG_X25519_PUBLIC_KEY` | `40011uL` | `TAG_NAME_X25519_PUBLIC_KEY` | `agreement-public-key` |
| `TAG_ARID` | `40012uL` | `TAG_NAME_ARID` | `arid` |
| `TAG_PRIVATE_KEYS` | `40013uL` | `TAG_NAME_PRIVATE_KEYS` | `crypto-prvkeys` |
| `TAG_NONCE` | `40014uL` | `TAG_NAME_NONCE` | `nonce` |
| `TAG_PASSWORD` | `40015uL` | `TAG_NAME_PASSWORD` | `password` |
| `TAG_PRIVATE_KEY_BASE` | `40016uL` | `TAG_NAME_PRIVATE_KEY_BASE` | `crypto-prvkey-base` |
| `TAG_PUBLIC_KEYS` | `40017uL` | `TAG_NAME_PUBLIC_KEYS` | `crypto-pubkeys` |
| `TAG_SALT` | `40018uL` | `TAG_NAME_SALT` | `salt` |
| `TAG_SEALED_MESSAGE` | `40019uL` | `TAG_NAME_SEALED_MESSAGE` | `crypto-sealed` |
| `TAG_SIGNATURE` | `40020uL` | `TAG_NAME_SIGNATURE` | `signature` |
| `TAG_SIGNING_PRIVATE_KEY` | `40021uL` | `TAG_NAME_SIGNING_PRIVATE_KEY` | `signing-private-key` |
| `TAG_SIGNING_PUBLIC_KEY` | `40022uL` | `TAG_NAME_SIGNING_PUBLIC_KEY` | `signing-public-key` |
| `TAG_SYMMETRIC_KEY` | `40023uL` | `TAG_NAME_SYMMETRIC_KEY` | `crypto-key` |
| `TAG_XID` | `40024uL` | `TAG_NAME_XID` | `xid` |
| `TAG_REFERENCE` | `40025uL` | `TAG_NAME_REFERENCE` | `reference` |
| `TAG_EVENT` | `40026uL` | `TAG_NAME_EVENT` | `event` |
| `TAG_ENCRYPTED_KEY` | `40027uL` | `TAG_NAME_ENCRYPTED_KEY` | `encrypted-key` |
| `TAG_MLKEM_PRIVATE_KEY` | `40100uL` | `TAG_NAME_MLKEM_PRIVATE_KEY` | `mlkem-private-key` |
| `TAG_MLKEM_PUBLIC_KEY` | `40101uL` | `TAG_NAME_MLKEM_PUBLIC_KEY` | `mlkem-public-key` |
| `TAG_MLKEM_CIPHERTEXT` | `40102uL` | `TAG_NAME_MLKEM_CIPHERTEXT` | `mlkem-ciphertext` |
| `TAG_MLDSA_PRIVATE_KEY` | `40103uL` | `TAG_NAME_MLDSA_PRIVATE_KEY` | `mldsa-private-key` |
| `TAG_MLDSA_PUBLIC_KEY` | `40104uL` | `TAG_NAME_MLDSA_PUBLIC_KEY` | `mldsa-public-key` |
| `TAG_MLDSA_SIGNATURE` | `40105uL` | `TAG_NAME_MLDSA_SIGNATURE` | `mldsa-signature` |
| `TAG_SEED` | `40300uL` | `TAG_NAME_SEED` | `seed` |
| `TAG_HDKEY` | `40303uL` | `TAG_NAME_HDKEY` | `hdkey` |
| `TAG_DERIVATION_PATH` | `40304uL` | `TAG_NAME_DERIVATION_PATH` | `keypath` |
| `TAG_USE_INFO` | `40305uL` | `TAG_NAME_USE_INFO` | `coin-info` |
| `TAG_EC_KEY` | `40306uL` | `TAG_NAME_EC_KEY` | `eckey` |
| `TAG_ADDRESS` | `40307uL` | `TAG_NAME_ADDRESS` | `address` |
| `TAG_OUTPUT_DESCRIPTOR` | `40308uL` | `TAG_NAME_OUTPUT_DESCRIPTOR` | `output-descriptor` |
| `TAG_SSKR_SHARE` | `40309uL` | `TAG_NAME_SSKR_SHARE` | `sskr` |
| `TAG_PSBT` | `40310uL` | `TAG_NAME_PSBT` | `psbt` |
| `TAG_ACCOUNT_DESCRIPTOR` | `40311uL` | `TAG_NAME_ACCOUNT_DESCRIPTOR` | `account-descriptor` |
| `TAG_SSH_TEXT_PRIVATE_KEY` | `40800uL` | `TAG_NAME_SSH_TEXT_PRIVATE_KEY` | `ssh-private` |
| `TAG_SSH_TEXT_PUBLIC_KEY` | `40801uL` | `TAG_NAME_SSH_TEXT_PUBLIC_KEY` | `ssh-public` |
| `TAG_SSH_TEXT_SIGNATURE` | `40802uL` | `TAG_NAME_SSH_TEXT_SIGNATURE` | `ssh-signature` |
| `TAG_SSH_TEXT_CERTIFICATE` | `40803uL` | `TAG_NAME_SSH_TEXT_CERTIFICATE` | `ssh-certificate` |
| `TAG_PROVENANCE_MARK` | `1347571542uL` | `TAG_NAME_PROVENANCE_MARK` | `provenance` |
| `TAG_SEED_V1` | `300uL` | `TAG_NAME_SEED_V1` | `crypto-seed` |
| `TAG_EC_KEY_V1` | `306uL` | `TAG_NAME_EC_KEY_V1` | `crypto-eckey` |
| `TAG_SSKR_SHARE_V1` | `309uL` | `TAG_NAME_SSKR_SHARE_V1` | `crypto-sskr` |
| `TAG_HDKEY_V1` | `303uL` | `TAG_NAME_HDKEY_V1` | `crypto-hdkey` |
| `TAG_DERIVATION_PATH_V1` | `304uL` | `TAG_NAME_DERIVATION_PATH_V1` | `crypto-keypath` |
| `TAG_USE_INFO_V1` | `305uL` | `TAG_NAME_USE_INFO_V1` | `crypto-coin-info` |
| `TAG_OUTPUT_DESCRIPTOR_V1` | `307uL` | `TAG_NAME_OUTPUT_DESCRIPTOR_V1` | `crypto-output` |
| `TAG_PSBT_V1` | `310uL` | `TAG_NAME_PSBT_V1` | `crypto-psbt` |
| `TAG_ACCOUNT_V1` | `311uL` | `TAG_NAME_ACCOUNT_V1` | `crypto-account` |
| `TAG_OUTPUT_SCRIPT_HASH` | `400uL` | `TAG_NAME_OUTPUT_SCRIPT_HASH` | `output-script-hash` |
| `TAG_OUTPUT_WITNESS_SCRIPT_HASH` | `401uL` | `TAG_NAME_OUTPUT_WITNESS_SCRIPT_HASH` | `output-witness-script-hash` |
| `TAG_OUTPUT_PUBLIC_KEY` | `402uL` | `TAG_NAME_OUTPUT_PUBLIC_KEY` | `output-public-key` |
| `TAG_OUTPUT_PUBLIC_KEY_HASH` | `403uL` | `TAG_NAME_OUTPUT_PUBLIC_KEY_HASH` | `output-public-key-hash` |
| `TAG_OUTPUT_WITNESS_PUBLIC_KEY_HASH` | `404uL` | `TAG_NAME_OUTPUT_WITNESS_PUBLIC_KEY_HASH` | `output-witness-public-key-hash` |
| `TAG_OUTPUT_COMBO` | `405uL` | `TAG_NAME_OUTPUT_COMBO` | `output-combo` |
| `TAG_OUTPUT_MULTISIG` | `406uL` | `TAG_NAME_OUTPUT_MULTISIG` | `output-multisig` |
| `TAG_OUTPUT_SORTED_MULTISIG` | `407uL` | `TAG_NAME_OUTPUT_SORTED_MULTISIG` | `output-sorted-multisig` |
| `TAG_OUTPUT_RAW_SCRIPT` | `408uL` | `TAG_NAME_OUTPUT_RAW_SCRIPT` | `output-raw-script` |
| `TAG_OUTPUT_TAPROOT` | `409uL` | `TAG_NAME_OUTPUT_TAPROOT` | `output-taproot` |
| `TAG_OUTPUT_COSIGNER` | `410uL` | `TAG_NAME_OUTPUT_COSIGNER` | `output-cosigner` |

### Trait Catalog
- No public traits.

## Documentation Catalog
- Crate-level docs: yes (`//! # CBOR Tags Registry` block in `src/tags_registry.rs`).
- Module-level docs: yes (`tags_registry` module has top-level docs).
- Public items with doc comments: none.
- Public items without doc comments: all generated constants, `register_tags_in`, `register_tags`.
- Package metadata description: present in `Cargo.toml`.
- README: present; describes crate purpose and tag usage context.

## Test Inventory
- Rust `#[test]` count: **0** (no unit or integration tests in crate).
- Required translation parity: no source tests to port.
- Kotlin translation should still include sanity tests for registration behavior and representative constants.

## EXPECTED TEXT OUTPUT RUBRIC
- Applicable: no
- Source signals: none (no tests in source crate)
- Reason: crate has no rendered text/diagnostic output tests.

## Translation Unit Order
1. Build/config scaffold (`.gitignore`, `build.gradle.kts`, `settings.gradle.kts`).
2. `TagsRegistry.kt`: all `TAG_*` and `TAG_NAME_*` constants.
3. `TagsRegistry.kt`: `registerTagsIn` and `registerTags` functions.
4. `TagsRegistryTest.kt`: registration and constant sanity tests.

## Translation Hazards
- Macro expansion hazard: Rust macros create public API implicitly; Kotlin must define every generated constant explicitly.
- Registry behavior hazard: `register_tags_in` must call `dcbor::register_tags_in` equivalent before adding bc-tags entries.
- Compatibility hazard: deprecated `*_V1` and account-bundle tags remain part of public API and must not be removed.
- Numeric type hazard: use `ULong` constants to preserve Rust `u64` semantics.

## Completeness Targets
- API items: 152 total (`150` constants + `2` functions).
- Rust test parity: 0/0.
- Documentation: preserve crate-level intent without inventing Rust docs on undocumented items.

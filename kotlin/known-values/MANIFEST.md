# Translation Manifest: known-values → Kotlin (known-values)

## Crate Metadata
- Name: `known-values`
- Version: `0.15.4`
- Rust edition: `2024`
- Description: `Blockchain Commons Known Values.`
- Repository: <https://github.com/BlockchainCommons/known-values-rust>

## Dependencies
### Internal BC Dependencies
- `bc-components = ^0.31.0`
  - Kotlin equivalent: `com.blockchaincommons:bc-components:0.31.1`
  - Local development: composite build via `includeBuild("../bc-components")`
- `dcbor = ^0.25.0` (Rust enables `multithreaded` feature)
  - Kotlin equivalent: `com.blockchaincommons:dcbor:0.25.1`
  - Local development: composite build via `includeBuild("../dcbor")`

### External Dependencies
- `paste = ^1.0.12`
  - Rust-only macro helper for `const_known_value!` expansion.
  - Kotlin equivalent: no runtime dependency; constants are declared directly.
- `serde = 1.0` + `serde_json = 1.0` (optional, via default `directory-loading` feature)
  - Purpose: parse JSON registry files.
  - Kotlin equivalent: `com.fasterxml.jackson.module:jackson-module-kotlin` + `com.fasterxml.jackson.core:jackson-databind`.
- `dirs = 5.0` (optional, via default `directory-loading` feature)
  - Purpose: resolve home directory for `~/.known-values` default path.
  - Kotlin equivalent: `System.getProperty("user.home")` + `java.nio.file.Path`.
- `tempfile = 3.10` (dev-dependency)
  - Purpose: temporary directories in tests.
  - Kotlin equivalent: `Files.createTempDirectory(...)`.

## Feature Flags
- `default = ["directory-loading"]`
- `directory-loading` enables JSON directory registry loading and exports:
  - Types: `ConfigError`, `DirectoryConfig`, `LoadError`, `LoadResult`, `RegistryEntry`, `RegistryFile`
  - Functions: `add_search_paths`, `load_from_config`, `load_from_directory`, `set_directory_config`
  - `KnownValuesStore::load_from_directory` and `KnownValuesStore::load_from_config`
- Translation scope for initial Kotlin target: **default features only**, so directory-loading APIs are included.

## Public API Surface
### Type Catalog
- `KnownValue` (`struct`)
  - Fields (private): `value: u64`, `assigned_name: Option<KnownValueName>`
  - Derives/traits: `Clone`, `Debug`, `PartialEq` (manual by value only), `Eq`, `Hash`, `Display`, `DigestProvider`, `CBORTagged`, `CBORTaggedEncodable`, `CBORTaggedDecodable`
- `KnownValuesStore` (`struct`)
  - Fields (private): maps by raw codepoint and by assigned name
  - Derives: `Clone`, `Debug`, `Default`
- `LazyKnownValues` (`struct`, `#[doc(hidden)]`)
  - Fields: `init: Once`, `data: Mutex<Option<KnownValuesStore>>`
- `RegistryEntry` (`struct`)
  - Public fields: `codepoint`, `name`, `entry_type`, `uri`, `description`
- `OntologyInfo` (`struct`)
  - Public fields: `name`, `source_url`, `start_code_point`, `processing_strategy`
- `RegistryFile` (`struct`)
  - Public fields: `ontology`, `generated`, `entries`, `statistics`
- `GeneratedInfo` (`struct`)
  - Public fields: `tool`
- `LoadError` (`enum`)
  - Variants: `Io(io::Error)`, `Json { file: PathBuf, error: serde_json::Error }`
- `LoadResult` (`struct`)
  - Public fields: `values`, `files_processed`, `errors`
- `DirectoryConfig` (`struct`)
  - Field: private `paths: Vec<PathBuf>`
  - Derives: `Debug`, `Clone`, `Default`
- `ConfigError` (`enum`)
  - Variant: `AlreadyInitialized`

### Function Catalog
- `KnownValue::new(value: u64) -> Self`
- `KnownValue::new_with_name<T: Into<u64>>(value: T, assigned_name: String) -> Self`
- `KnownValue::new_with_static_name(value: u64, name: &'static str) -> Self` (`const fn`)
- `KnownValue::value(&self) -> u64`
- `KnownValue::assigned_name(&self) -> Option<&str>`
- `KnownValue::name(&self) -> String`
- `KnownValuesStore::new<T: IntoIterator<Item = KnownValue>>(known_values: T) -> Self`
- `KnownValuesStore::insert(&mut self, known_value: KnownValue)`
- `KnownValuesStore::assigned_name(&self, known_value: &KnownValue) -> Option<&str>`
- `KnownValuesStore::name(&self, known_value: KnownValue) -> String`
- `KnownValuesStore::known_value_named(&self, assigned_name: &str) -> Option<&KnownValue>`
- `KnownValuesStore::known_value_for_raw_value(raw_value: u64, known_values: Option<&Self>) -> KnownValue`
- `KnownValuesStore::known_value_for_name(name: &str, known_values: Option<&Self>) -> Option<KnownValue>`
- `KnownValuesStore::name_for_known_value(known_value: KnownValue, known_values: Option<&Self>) -> String`
- `KnownValuesStore::load_from_directory(&mut self, path: &Path) -> Result<usize, LoadError>` (default feature)
- `KnownValuesStore::load_from_config(&mut self, config: &DirectoryConfig) -> LoadResult` (default feature)
- `LazyKnownValues::get(&self) -> MutexGuard<Option<KnownValuesStore>>`
- `LoadResult::values_count(&self) -> usize`
- `LoadResult::values_iter(&self) -> impl Iterator<Item = &KnownValue>`
- `LoadResult::into_values(self) -> impl Iterator<Item = KnownValue>`
- `LoadResult::has_errors(&self) -> bool`
- `DirectoryConfig::new() -> Self`
- `DirectoryConfig::default_only() -> Self`
- `DirectoryConfig::with_paths(paths: Vec<PathBuf>) -> Self`
- `DirectoryConfig::with_paths_and_default(paths: Vec<PathBuf>) -> Self`
- `DirectoryConfig::default_directory() -> PathBuf`
- `DirectoryConfig::paths(&self) -> &[PathBuf]`
- `DirectoryConfig::add_path(&mut self, path: PathBuf)`
- `load_from_directory(path: &Path) -> Result<Vec<KnownValue>, LoadError>`
- `load_from_config(config: &DirectoryConfig) -> LoadResult`
- `set_directory_config(config: DirectoryConfig) -> Result<(), ConfigError>`
- `add_search_paths(paths: Vec<PathBuf>) -> Result<(), ConfigError>`

### Constant / Static Catalog
- Macro-exported Rust macro: `const_known_value!` (generates `<NAME>_RAW` + `<NAME>` constants).
- Known Value constants generated by `const_known_value!`: **104** entries, each with both `*_RAW` and `KnownValue` constant.

| Value | Constant | Display Name | Raw Constant |
|---:|---|---|---|
| `0` | `UNIT` | `` | `UNIT_RAW` |
| `1` | `IS_A` | `isA` | `IS_A_RAW` |
| `2` | `ID` | `id` | `ID_RAW` |
| `3` | `SIGNED` | `signed` | `SIGNED_RAW` |
| `4` | `NOTE` | `note` | `NOTE_RAW` |
| `5` | `HAS_RECIPIENT` | `hasRecipient` | `HAS_RECIPIENT_RAW` |
| `6` | `SSKR_SHARE` | `sskrShare` | `SSKR_SHARE_RAW` |
| `7` | `CONTROLLER` | `controller` | `CONTROLLER_RAW` |
| `8` | `KEY` | `key` | `KEY_RAW` |
| `9` | `DEREFERENCE_VIA` | `dereferenceVia` | `DEREFERENCE_VIA_RAW` |
| `10` | `ENTITY` | `entity` | `ENTITY_RAW` |
| `11` | `NAME` | `name` | `NAME_RAW` |
| `12` | `LANGUAGE` | `language` | `LANGUAGE_RAW` |
| `13` | `ISSUER` | `issuer` | `ISSUER_RAW` |
| `14` | `HOLDER` | `holder` | `HOLDER_RAW` |
| `15` | `SALT` | `salt` | `SALT_RAW` |
| `16` | `DATE` | `date` | `DATE_RAW` |
| `17` | `UNKNOWN_VALUE` | `Unknown` | `UNKNOWN_VALUE_RAW` |
| `18` | `VERSION_VALUE` | `version` | `VERSION_VALUE_RAW` |
| `19` | `HAS_SECRET` | `hasSecret` | `HAS_SECRET_RAW` |
| `20` | `DIFF_EDITS` | `edits` | `DIFF_EDITS_RAW` |
| `21` | `VALID_FROM` | `validFrom` | `VALID_FROM_RAW` |
| `22` | `VALID_UNTIL` | `validUntil` | `VALID_UNTIL_RAW` |
| `23` | `POSITION` | `position` | `POSITION_RAW` |
| `24` | `NICKNAME` | `nickname` | `NICKNAME_RAW` |
| `25` | `VALUE` | `value` | `VALUE_RAW` |
| `26` | `ATTESTATION` | `attestation` | `ATTESTATION_RAW` |
| `27` | `VERIFIABLE_AT` | `verifiableAt` | `VERIFIABLE_AT_RAW` |
| `50` | `ATTACHMENT` | `attachment` | `ATTACHMENT_RAW` |
| `51` | `VENDOR` | `vendor` | `VENDOR_RAW` |
| `52` | `CONFORMS_TO` | `conformsTo` | `CONFORMS_TO_RAW` |
| `60` | `ALLOW` | `allow` | `ALLOW_RAW` |
| `61` | `DENY` | `deny` | `DENY_RAW` |
| `62` | `ENDPOINT` | `endpoint` | `ENDPOINT_RAW` |
| `63` | `DELEGATE` | `delegate` | `DELEGATE_RAW` |
| `64` | `PROVENANCE` | `provenance` | `PROVENANCE_RAW` |
| `65` | `PRIVATE_KEY` | `privateKey` | `PRIVATE_KEY_RAW` |
| `66` | `SERVICE` | `service` | `SERVICE_RAW` |
| `67` | `CAPABILITY` | `capability` | `CAPABILITY_RAW` |
| `68` | `PROVENANCE_GENERATOR` | `provenanceGenerator` | `PROVENANCE_GENERATOR_RAW` |
| `70` | `PRIVILEGE_ALL` | `All` | `PRIVILEGE_ALL_RAW` |
| `71` | `PRIVILEGE_AUTH` | `Authorize` | `PRIVILEGE_AUTH_RAW` |
| `72` | `PRIVILEGE_SIGN` | `Sign` | `PRIVILEGE_SIGN_RAW` |
| `73` | `PRIVILEGE_ENCRYPT` | `Encrypt` | `PRIVILEGE_ENCRYPT_RAW` |
| `74` | `PRIVILEGE_ELIDE` | `Elide` | `PRIVILEGE_ELIDE_RAW` |
| `75` | `PRIVILEGE_ISSUE` | `Issue` | `PRIVILEGE_ISSUE_RAW` |
| `76` | `PRIVILEGE_ACCESS` | `Access` | `PRIVILEGE_ACCESS_RAW` |
| `80` | `PRIVILEGE_DELEGATE` | `Delegate` | `PRIVILEGE_DELEGATE_RAW` |
| `81` | `PRIVILEGE_VERIFY` | `Verify` | `PRIVILEGE_VERIFY_RAW` |
| `82` | `PRIVILEGE_UPDATE` | `Update` | `PRIVILEGE_UPDATE_RAW` |
| `83` | `PRIVILEGE_TRANSFER` | `Transfer` | `PRIVILEGE_TRANSFER_RAW` |
| `84` | `PRIVILEGE_ELECT` | `Elect` | `PRIVILEGE_ELECT_RAW` |
| `85` | `PRIVILEGE_BURN` | `Burn` | `PRIVILEGE_BURN_RAW` |
| `86` | `PRIVILEGE_REVOKE` | `Revoke` | `PRIVILEGE_REVOKE_RAW` |
| `100` | `BODY` | `body` | `BODY_RAW` |
| `101` | `RESULT` | `result` | `RESULT_RAW` |
| `102` | `ERROR` | `error` | `ERROR_RAW` |
| `103` | `OK_VALUE` | `OK` | `OK_VALUE_RAW` |
| `104` | `PROCESSING_VALUE` | `Processing` | `PROCESSING_VALUE_RAW` |
| `105` | `SENDER` | `sender` | `SENDER_RAW` |
| `106` | `SENDER_CONTINUATION` | `senderContinuation` | `SENDER_CONTINUATION_RAW` |
| `107` | `RECIPIENT_CONTINUATION` | `recipientContinuation` | `RECIPIENT_CONTINUATION_RAW` |
| `108` | `CONTENT` | `content` | `CONTENT_RAW` |
| `200` | `SEED_TYPE` | `Seed` | `SEED_TYPE_RAW` |
| `201` | `PRIVATE_KEY_TYPE` | `PrivateKey` | `PRIVATE_KEY_TYPE_RAW` |
| `202` | `PUBLIC_KEY_TYPE` | `PublicKey` | `PUBLIC_KEY_TYPE_RAW` |
| `203` | `MASTER_KEY_TYPE` | `MasterKey` | `MASTER_KEY_TYPE_RAW` |
| `300` | `ASSET` | `asset` | `ASSET_RAW` |
| `301` | `BITCOIN_VALUE` | `Bitcoin` | `BITCOIN_VALUE_RAW` |
| `302` | `ETHEREUM_VALUE` | `Ethereum` | `ETHEREUM_VALUE_RAW` |
| `303` | `TEZOS_VALUE` | `Tezos` | `TEZOS_VALUE_RAW` |
| `400` | `NETWORK` | `network` | `NETWORK_RAW` |
| `401` | `MAIN_NET_VALUE` | `MainNet` | `MAIN_NET_VALUE_RAW` |
| `402` | `TEST_NET_VALUE` | `TestNet` | `TEST_NET_VALUE_RAW` |
| `500` | `BIP32_KEY_TYPE` | `BIP32Key` | `BIP32_KEY_TYPE_RAW` |
| `501` | `CHAIN_CODE` | `chainCode` | `CHAIN_CODE_RAW` |
| `502` | `DERIVATION_PATH_TYPE` | `DerivationPath` | `DERIVATION_PATH_TYPE_RAW` |
| `503` | `PARENT_PATH` | `parentPath` | `PARENT_PATH_RAW` |
| `504` | `CHILDREN_PATH` | `childrenPath` | `CHILDREN_PATH_RAW` |
| `505` | `PARENT_FINGERPRINT` | `parentFingerprint` | `PARENT_FINGERPRINT_RAW` |
| `506` | `PSBT_TYPE` | `PSBT` | `PSBT_TYPE_RAW` |
| `507` | `OUTPUT_DESCRIPTOR_TYPE` | `OutputDescriptor` | `OUTPUT_DESCRIPTOR_TYPE_RAW` |
| `508` | `OUTPUT_DESCRIPTOR` | `outputDescriptor` | `OUTPUT_DESCRIPTOR_RAW` |
| `600` | `GRAPH` | `Graph` | `GRAPH_RAW` |
| `601` | `SOURCE_TARGET_GRAPH` | `SourceTargetGraph` | `SOURCE_TARGET_GRAPH_RAW` |
| `602` | `PARENT_CHILD_GRAPH` | `ParentChildGraph` | `PARENT_CHILD_GRAPH_RAW` |
| `603` | `DIGRAPH` | `Digraph` | `DIGRAPH_RAW` |
| `604` | `ACYCLIC_GRAPH` | `AcyclicGraph` | `ACYCLIC_GRAPH_RAW` |
| `605` | `MULTIGRAPH` | `Multigraph` | `MULTIGRAPH_RAW` |
| `606` | `PSEUDOGRAPH` | `Pseudograph` | `PSEUDOGRAPH_RAW` |
| `607` | `GRAPH_FRAGMENT` | `GraphFragment` | `GRAPH_FRAGMENT_RAW` |
| `608` | `DAG` | `DAG` | `DAG_RAW` |
| `609` | `TREE` | `Tree` | `TREE_RAW` |
| `610` | `FOREST` | `Forest` | `FOREST_RAW` |
| `611` | `COMPOUND_GRAPH` | `CompoundGraph` | `COMPOUND_GRAPH_RAW` |
| `612` | `HYPERGRAPH` | `Hypergraph` | `HYPERGRAPH_RAW` |
| `613` | `DIHYPERGRAPH` | `Dihypergraph` | `DIHYPERGRAPH_RAW` |
| `700` | `NODE` | `node` | `NODE_RAW` |
| `701` | `EDGE` | `edge` | `EDGE_RAW` |
| `702` | `SOURCE` | `source` | `SOURCE_RAW` |
| `703` | `TARGET` | `target` | `TARGET_RAW` |
| `704` | `PARENT` | `parent` | `PARENT_RAW` |
| `705` | `CHILD` | `child` | `CHILD_RAW` |
| `706` | `SELF` | `Self` | `SELF_RAW` |

- Global singleton static:
  - `KNOWN_VALUES: LazyKnownValues`
- Registry initialization list in `KNOWN_VALUES.get()` contains **102** entries.
  - Deliberate source behavior to mirror: constants defined but not inserted into initial store: `VALUE`, `SELF`.

### Trait / Conversion Catalog
- `impl From<KnownValue> for CBOR`
- `impl TryFrom<CBOR> for KnownValue`
- `impl From<u64> for KnownValue`
- `impl From<i32> for KnownValue`
- `impl From<usize> for KnownValue`
- `impl Display for KnownValue`
- `impl PartialEq/Eq/Hash for KnownValue` (numeric value only)

## Documentation Catalog
- Crate-level docs in `lib.rs`: extensive usage + feature docs and JSON format example.
- Module-level docs:
  - `directory_loader.rs` has module docs with overview and JSON schema.
- Public items with doc comments:
  - Most structs, enums, and methods in `known_value.rs`, `known_value_store.rs`, `directory_loader.rs`, and `known_values_registry.rs`.
- Public items without doc comments:
  - Macro-generated constants in `known_values_registry.rs`.
- Package metadata description: present in `Cargo.toml` (`Blockchain Commons Known Values.`).
- README: present and current; includes version history and directory-loading notes.

## Test Inventory
Rust tests to translate (default features):
- `src/known_values_registry.rs`
  - `test_1`
- `src/directory_loader.rs`
  - `test_parse_registry_json`
  - `test_parse_minimal_registry`
  - `test_parse_full_entry`
  - `test_directory_config_default`
  - `test_directory_config_custom_paths`
  - `test_directory_config_with_default`
  - `test_load_from_nonexistent_directory`
  - `test_load_result_methods`
- `tests/directory_loading.rs` (`#[cfg(feature = "directory-loading")]`)
  - `test_global_registry_still_works`
  - `test_load_from_temp_directory`
  - `test_override_hardcoded_value`
  - `test_multiple_files_in_directory`
  - `test_directory_config_custom_paths`
  - `test_later_directory_overrides_earlier`
  - `test_nonexistent_directory_is_ok`
  - `test_invalid_json_is_error`
  - `test_tolerant_loading_continues_on_error`
  - `test_full_registry_format`
  - `test_load_result_methods`
  - `test_empty_entries_array`
  - `test_non_json_files_ignored`

Total Rust tests in scope: **22**.

## EXPECTED TEXT OUTPUT RUBRIC
- Applicable: no
- Source signals: no diagnostic-rendering or complex formatted-text assertions in Rust tests.
- Reason: tests are structural/value assertions over objects, maps, and filesystem loading behavior.

## Translation Unit Order
1. Build/config scaffold (`.gitignore`, `build.gradle.kts`, `settings.gradle.kts`).
2. Core type translation: `KnownValue` (`known_value.rs`).
3. Store translation: `KnownValuesStore` (`known_value_store.rs`).
4. Registry constants + lazy global store (`known_values_registry.rs`).
5. Directory loader types/errors/config/functions (`directory_loader.rs`).
6. Re-export surface in package-level Kotlin API.
7. Translate tests from `known_values_registry.rs` and `directory_loader.rs`.
8. Translate integration tests from `tests/directory_loading.rs`.

## Translation Hazards
- Macro surface hazard: Rust exports a macro (`const_known_value!`) that auto-generates constants; Kotlin must provide equivalent explicit constants and preserve names/values.
- Scale hazard: 104 known-value constants plus raw-value companions are prone to transcription errors; generate and verify programmatically.
- Registry parity hazard: `KNOWN_VALUES` intentionally initializes 102 constants (missing `VALUE`, `SELF` despite declarations). Translation must mirror source behavior unless Rust changes.
- Equality hazard: `KnownValue` equality/hash must ignore assigned name and use numeric codepoint only.
- Directory-loading behavior hazard:
  - strict loader (`load_from_directory`) fails fast on malformed JSON;
  - tolerant loader (`load_from_config`) accumulates per-file errors and continues.
- Configuration lock hazard: calling `KNOWN_VALUES.get()` must lock further configuration changes (`AlreadyInitialized` path).
- File-order hazard: directory iteration order can affect overrides in same directory; avoid assertions that depend on OS iteration order.

## Completeness Targets
- Public types/enums/structs/statics/macros represented: 11 public types + 1 static + macro-generated constant surface.
- Public constants: 208 (`104` raw numeric + `104` `KnownValue` values).
- Public methods/functions: 33 in scope (including free functions and associated methods).
- Test parity target: 22/22 Rust tests translated and passing.
- Docs target: translate public docs where present in Rust; do not invent docs where Rust has none.

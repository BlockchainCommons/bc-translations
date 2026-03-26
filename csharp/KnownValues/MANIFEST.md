# Translation Manifest: known-values → C# (KnownValues)

## Crate Summary

- Rust crate: `known-values`
- Rust version in source: `0.15.5`
- Package description: `Blockchain Commons Known Values.`
- Default features: `directory-loading`
- Internal BC dependencies:
  - `bc-components`
  - `dcbor`
- External dependencies:
  - `paste`
    - Purpose: macro-generated paired constants in the registry
    - C# equivalent: expand the registry constants directly in source
  - `serde`
    - Purpose: JSON data binding for directory-loaded registries
    - C# equivalent: `System.Text.Json`
  - `serde_json`
    - Purpose: JSON parsing for registry files
    - C# equivalent: `System.Text.Json`
  - `dirs`
    - Purpose: discover the home directory for `~/.known-values`
    - C# equivalent: `Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)`
  - `tempfile` (dev dependency)
    - Purpose: temporary directories for tests
    - C# equivalent: `Path.GetTempPath()` plus per-test temporary directories

## Feature Flags

- `default = ["directory-loading"]`
  - Translate: yes
  - Notes: the C# package should include directory-loading support by default.
- `directory-loading`
  - Gates: JSON registry file loading, configuration APIs, tolerant loading types, and re-exports from `directory_loader.rs`
  - Translate: yes
- Non-default-only code
  - None beyond disabling `directory-loading`

## Type Catalog

### Public Types

- `KnownValue`
  - Kind: struct
  - Fields: `value: u64`, `assigned_name: Option<KnownValueName>` (internal enum)
  - Derives: `Clone`, `Debug`
  - Implements: `PartialEq`, `Eq`, `Hash`, `Display`, `DigestProvider`, `CBORTagged`, `CBORTaggedEncodable`, `CBORTaggedDecodable`, `From<u64>`, `From<i32>`, `From<usize>`, `TryFrom<CBOR>`, `Into<CBOR>`
- `KnownValuesStore`
  - Kind: struct
  - Fields: `known_values_by_raw_value: HashMap<u64, KnownValue>`, `known_values_by_assigned_name: HashMap<String, KnownValue>`
  - Derives: `Clone`, `Debug`
  - Implements: `Default`
- `LazyKnownValues`
  - Kind: struct
  - Fields: `init: Once`, `data: Mutex<Option<KnownValuesStore>>`
  - Derives: `Debug`
- `RegistryEntry`
  - Kind: struct
  - Fields: `codepoint`, `name`, `entry_type`, `uri`, `description`
  - Derives: `Debug`, `Deserialize`
- `OntologyInfo`
  - Kind: struct
  - Fields: `name`, `source_url`, `start_code_point`, `processing_strategy`
  - Derives: `Debug`, `Deserialize`
- `RegistryFile`
  - Kind: struct
  - Fields: `ontology`, `generated`, `entries`, `statistics`
  - Derives: `Debug`, `Deserialize`
- `GeneratedInfo`
  - Kind: struct
  - Fields: `tool`
  - Derives: `Debug`, `Deserialize`
- `LoadError`
  - Kind: enum
  - Variants:
    - `Io(io::Error)`
    - `Json { file: PathBuf, error: serde_json::Error }`
  - Implements: `Display`, `Error`, `From<io::Error>`
- `LoadResult`
  - Kind: struct
  - Fields: `values`, `files_processed`, `errors`
  - Derives: `Debug`, `Default`
- `DirectoryConfig`
  - Kind: struct
  - Fields: `paths: Vec<PathBuf>`
  - Derives: `Debug`, `Clone`, `Default`
- `ConfigError`
  - Kind: enum
  - Variants:
    - `AlreadyInitialized`
  - Derives: `Debug`, `Clone`, `PartialEq`, `Eq`
  - Implements: `Display`, `Error`

## Function Catalog

### `KnownValue`

- `fn new(value: u64) -> Self`
- `fn new_with_name<T: Into<u64>>(value: T, assigned_name: String) -> Self`
- `const fn new_with_static_name(value: u64, name: &'static str) -> Self`
- `fn value(&self) -> u64`
- `fn assigned_name(&self) -> Option<&str>`
- `fn name(&self) -> String`
- Conversion methods implied by trait impls:
  - `From<u64>`
  - `From<i32>`
  - `From<usize>`
  - `TryFrom<CBOR>`
  - `Into<CBOR>`
- Digest / CBOR methods implied by trait impls:
  - `digest(&self) -> Digest`
  - `cbor_tags() -> Vec<Tag>`
  - `untagged_cbor(&self) -> CBOR`
  - `from_untagged_cbor(cbor: CBOR) -> dcbor::Result<Self>`

### `KnownValuesStore`

- `fn new<T: IntoIterator<Item = KnownValue>>(known_values: T) -> Self`
- `fn insert(&mut self, known_value: KnownValue)`
- `fn assigned_name(&self, known_value: &KnownValue) -> Option<&str>`
- `fn name(&self, known_value: KnownValue) -> String`
- `fn known_value_named(&self, assigned_name: &str) -> Option<&KnownValue>`
- `fn known_value_for_raw_value(raw_value: u64, known_values: Option<&Self>) -> KnownValue`
- `fn known_value_for_name(name: &str, known_values: Option<&Self>) -> Option<KnownValue>`
- `fn name_for_known_value(known_value: KnownValue, known_values: Option<&Self>) -> String`
- `fn load_from_directory(&mut self, path: &Path) -> Result<usize, LoadError>` (default feature)
- `fn load_from_config(&mut self, config: &DirectoryConfig) -> LoadResult` (default feature)

### `LazyKnownValues`

- `fn get(&self) -> MutexGuard<'_, Option<KnownValuesStore>>`

### `LoadResult`

- `fn values_count(&self) -> usize`
- `fn values_iter(&self) -> impl Iterator<Item = &KnownValue>`
- `fn into_values(self) -> impl Iterator<Item = KnownValue>`
- `fn has_errors(&self) -> bool`

### `DirectoryConfig`

- `fn new() -> Self`
- `fn default_only() -> Self`
- `fn with_paths(paths: Vec<PathBuf>) -> Self`
- `fn with_paths_and_default(paths: Vec<PathBuf>) -> Self`
- `fn default_directory() -> PathBuf`
- `fn paths(&self) -> &[PathBuf]`
- `fn add_path(&mut self, path: PathBuf)`

### Free Functions Re-exported by `lib.rs`

- `fn load_from_directory(path: &Path) -> Result<Vec<KnownValue>, LoadError>`
- `fn load_from_config(config: &DirectoryConfig) -> LoadResult`
- `fn set_directory_config(config: DirectoryConfig) -> Result<(), ConfigError>`
- `fn add_search_paths(paths: Vec<PathBuf>) -> Result<(), ConfigError>`

## Constant Catalog

### Registry Macro Surface

- Public macro: `const_known_value!`
  - Translation note: C# has no equivalent public macro system requirement here; expand the generated constants directly.

### Public Statics

- `KNOWN_VALUES: LazyKnownValues`

### Registry Constants

Every Rust `const_known_value!` expands to two public items:

- `<NAME>_RAW: u64`
- `<NAME>: KnownValue`

The full registry inventory is:

| Raw Constant | KnownValue Constant | Value | Display Name |
|---|---|---:|---|
| `UNIT_RAW` | `UNIT` | 0 | `""` |
| `IS_A_RAW` | `IS_A` | 1 | `"isA"` |
| `ID_RAW` | `ID` | 2 | `"id"` |
| `SIGNED_RAW` | `SIGNED` | 3 | `"signed"` |
| `NOTE_RAW` | `NOTE` | 4 | `"note"` |
| `HAS_RECIPIENT_RAW` | `HAS_RECIPIENT` | 5 | `"hasRecipient"` |
| `SSKR_SHARE_RAW` | `SSKR_SHARE` | 6 | `"sskrShare"` |
| `CONTROLLER_RAW` | `CONTROLLER` | 7 | `"controller"` |
| `KEY_RAW` | `KEY` | 8 | `"key"` |
| `DEREFERENCE_VIA_RAW` | `DEREFERENCE_VIA` | 9 | `"dereferenceVia"` |
| `ENTITY_RAW` | `ENTITY` | 10 | `"entity"` |
| `NAME_RAW` | `NAME` | 11 | `"name"` |
| `LANGUAGE_RAW` | `LANGUAGE` | 12 | `"language"` |
| `ISSUER_RAW` | `ISSUER` | 13 | `"issuer"` |
| `HOLDER_RAW` | `HOLDER` | 14 | `"holder"` |
| `SALT_RAW` | `SALT` | 15 | `"salt"` |
| `DATE_RAW` | `DATE` | 16 | `"date"` |
| `UNKNOWN_VALUE_RAW` | `UNKNOWN_VALUE` | 17 | `"Unknown"` |
| `VERSION_VALUE_RAW` | `VERSION_VALUE` | 18 | `"version"` |
| `HAS_SECRET_RAW` | `HAS_SECRET` | 19 | `"hasSecret"` |
| `DIFF_EDITS_RAW` | `DIFF_EDITS` | 20 | `"edits"` |
| `VALID_FROM_RAW` | `VALID_FROM` | 21 | `"validFrom"` |
| `VALID_UNTIL_RAW` | `VALID_UNTIL` | 22 | `"validUntil"` |
| `POSITION_RAW` | `POSITION` | 23 | `"position"` |
| `NICKNAME_RAW` | `NICKNAME` | 24 | `"nickname"` |
| `VALUE_RAW` | `VALUE` | 25 | `"value"` |
| `ATTESTATION_RAW` | `ATTESTATION` | 26 | `"attestation"` |
| `VERIFIABLE_AT_RAW` | `VERIFIABLE_AT` | 27 | `"verifiableAt"` |
| `ATTACHMENT_RAW` | `ATTACHMENT` | 50 | `"attachment"` |
| `VENDOR_RAW` | `VENDOR` | 51 | `"vendor"` |
| `CONFORMS_TO_RAW` | `CONFORMS_TO` | 52 | `"conformsTo"` |
| `ALLOW_RAW` | `ALLOW` | 60 | `"allow"` |
| `DENY_RAW` | `DENY` | 61 | `"deny"` |
| `ENDPOINT_RAW` | `ENDPOINT` | 62 | `"endpoint"` |
| `DELEGATE_RAW` | `DELEGATE` | 63 | `"delegate"` |
| `PROVENANCE_RAW` | `PROVENANCE` | 64 | `"provenance"` |
| `PRIVATE_KEY_RAW` | `PRIVATE_KEY` | 65 | `"privateKey"` |
| `SERVICE_RAW` | `SERVICE` | 66 | `"service"` |
| `CAPABILITY_RAW` | `CAPABILITY` | 67 | `"capability"` |
| `PROVENANCE_GENERATOR_RAW` | `PROVENANCE_GENERATOR` | 68 | `"provenanceGenerator"` |
| `PRIVILEGE_ALL_RAW` | `PRIVILEGE_ALL` | 70 | `"All"` |
| `PRIVILEGE_AUTH_RAW` | `PRIVILEGE_AUTH` | 71 | `"Authorize"` |
| `PRIVILEGE_SIGN_RAW` | `PRIVILEGE_SIGN` | 72 | `"Sign"` |
| `PRIVILEGE_ENCRYPT_RAW` | `PRIVILEGE_ENCRYPT` | 73 | `"Encrypt"` |
| `PRIVILEGE_ELIDE_RAW` | `PRIVILEGE_ELIDE` | 74 | `"Elide"` |
| `PRIVILEGE_ISSUE_RAW` | `PRIVILEGE_ISSUE` | 75 | `"Issue"` |
| `PRIVILEGE_ACCESS_RAW` | `PRIVILEGE_ACCESS` | 76 | `"Access"` |
| `PRIVILEGE_DELEGATE_RAW` | `PRIVILEGE_DELEGATE` | 80 | `"Delegate"` |
| `PRIVILEGE_VERIFY_RAW` | `PRIVILEGE_VERIFY` | 81 | `"Verify"` |
| `PRIVILEGE_UPDATE_RAW` | `PRIVILEGE_UPDATE` | 82 | `"Update"` |
| `PRIVILEGE_TRANSFER_RAW` | `PRIVILEGE_TRANSFER` | 83 | `"Transfer"` |
| `PRIVILEGE_ELECT_RAW` | `PRIVILEGE_ELECT` | 84 | `"Elect"` |
| `PRIVILEGE_BURN_RAW` | `PRIVILEGE_BURN` | 85 | `"Burn"` |
| `PRIVILEGE_REVOKE_RAW` | `PRIVILEGE_REVOKE` | 86 | `"Revoke"` |
| `BODY_RAW` | `BODY` | 100 | `"body"` |
| `RESULT_RAW` | `RESULT` | 101 | `"result"` |
| `ERROR_RAW` | `ERROR` | 102 | `"error"` |
| `OK_VALUE_RAW` | `OK_VALUE` | 103 | `"OK"` |
| `PROCESSING_VALUE_RAW` | `PROCESSING_VALUE` | 104 | `"Processing"` |
| `SENDER_RAW` | `SENDER` | 105 | `"sender"` |
| `SENDER_CONTINUATION_RAW` | `SENDER_CONTINUATION` | 106 | `"senderContinuation"` |
| `RECIPIENT_CONTINUATION_RAW` | `RECIPIENT_CONTINUATION` | 107 | `"recipientContinuation"` |
| `CONTENT_RAW` | `CONTENT` | 108 | `"content"` |
| `SEED_TYPE_RAW` | `SEED_TYPE` | 200 | `"Seed"` |
| `PRIVATE_KEY_TYPE_RAW` | `PRIVATE_KEY_TYPE` | 201 | `"PrivateKey"` |
| `PUBLIC_KEY_TYPE_RAW` | `PUBLIC_KEY_TYPE` | 202 | `"PublicKey"` |
| `MASTER_KEY_TYPE_RAW` | `MASTER_KEY_TYPE` | 203 | `"MasterKey"` |
| `ASSET_RAW` | `ASSET` | 300 | `"asset"` |
| `BITCOIN_VALUE_RAW` | `BITCOIN_VALUE` | 301 | `"Bitcoin"` |
| `ETHEREUM_VALUE_RAW` | `ETHEREUM_VALUE` | 302 | `"Ethereum"` |
| `TEZOS_VALUE_RAW` | `TEZOS_VALUE` | 303 | `"Tezos"` |
| `NETWORK_RAW` | `NETWORK` | 400 | `"network"` |
| `MAIN_NET_VALUE_RAW` | `MAIN_NET_VALUE` | 401 | `"MainNet"` |
| `TEST_NET_VALUE_RAW` | `TEST_NET_VALUE` | 402 | `"TestNet"` |
| `BIP32_KEY_TYPE_RAW` | `BIP32_KEY_TYPE` | 500 | `"BIP32Key"` |
| `CHAIN_CODE_RAW` | `CHAIN_CODE` | 501 | `"chainCode"` |
| `DERIVATION_PATH_TYPE_RAW` | `DERIVATION_PATH_TYPE` | 502 | `"DerivationPath"` |
| `PARENT_PATH_RAW` | `PARENT_PATH` | 503 | `"parentPath"` |
| `CHILDREN_PATH_RAW` | `CHILDREN_PATH` | 504 | `"childrenPath"` |
| `PARENT_FINGERPRINT_RAW` | `PARENT_FINGERPRINT` | 505 | `"parentFingerprint"` |
| `PSBT_TYPE_RAW` | `PSBT_TYPE` | 506 | `"PSBT"` |
| `OUTPUT_DESCRIPTOR_TYPE_RAW` | `OUTPUT_DESCRIPTOR_TYPE` | 507 | `"OutputDescriptor"` |
| `OUTPUT_DESCRIPTOR_RAW` | `OUTPUT_DESCRIPTOR` | 508 | `"outputDescriptor"` |
| `GRAPH_RAW` | `GRAPH` | 600 | `"Graph"` |
| `SOURCE_TARGET_GRAPH_RAW` | `SOURCE_TARGET_GRAPH` | 601 | `"SourceTargetGraph"` |
| `PARENT_CHILD_GRAPH_RAW` | `PARENT_CHILD_GRAPH` | 602 | `"ParentChildGraph"` |
| `DIGRAPH_RAW` | `DIGRAPH` | 603 | `"Digraph"` |
| `ACYCLIC_GRAPH_RAW` | `ACYCLIC_GRAPH` | 604 | `"AcyclicGraph"` |
| `MULTIGRAPH_RAW` | `MULTIGRAPH` | 605 | `"Multigraph"` |
| `PSEUDOGRAPH_RAW` | `PSEUDOGRAPH` | 606 | `"Pseudograph"` |
| `GRAPH_FRAGMENT_RAW` | `GRAPH_FRAGMENT` | 607 | `"GraphFragment"` |
| `DAG_RAW` | `DAG` | 608 | `"DAG"` |
| `TREE_RAW` | `TREE` | 609 | `"Tree"` |
| `FOREST_RAW` | `FOREST` | 610 | `"Forest"` |
| `COMPOUND_GRAPH_RAW` | `COMPOUND_GRAPH` | 611 | `"CompoundGraph"` |
| `HYPERGRAPH_RAW` | `HYPERGRAPH` | 612 | `"Hypergraph"` |
| `DIHYPERGRAPH_RAW` | `DIHYPERGRAPH` | 613 | `"Dihypergraph"` |
| `NODE_RAW` | `NODE` | 700 | `"node"` |
| `EDGE_RAW` | `EDGE` | 701 | `"edge"` |
| `SOURCE_RAW` | `SOURCE` | 702 | `"source"` |
| `TARGET_RAW` | `TARGET` | 703 | `"target"` |
| `PARENT_RAW` | `PARENT` | 704 | `"parent"` |
| `CHILD_RAW` | `CHILD` | 705 | `"child"` |
| `SELF_RAW` | `SELF` | 706 | `"Self"` |

## Trait / Protocol Coverage Requirements

- `KnownValue`
  - equality must ignore assigned name and compare only numeric value
  - hashing must use only numeric value
  - display / string form must prefer assigned name and fall back to numeric string
  - digest provider must hash tagged CBOR bytes
  - CBOR tagged encoding must use the known-value tag (`40000`)
- `KnownValuesStore`
  - default constructor returns an empty store
  - clone-equivalent behavior must preserve both lookup maps
- `LoadError`
  - string rendering must distinguish I/O failures from JSON parse failures and include file paths for JSON parse failures
- `ConfigError`
  - must compare equal for `AlreadyInitialized`

## Doc Catalog

- Crate-level doc comment: yes
  - Summary: overview, basic usage, directory-loading behavior, JSON format, configuration, disabling default features, specification link
- Module-level doc comments:
  - `directory_loader.rs`
- Public items with doc comments:
  - `KnownValue`
  - all `KnownValue` constructors and accessors
  - `KnownValuesStore`
  - all `KnownValuesStore` public methods
  - `LazyKnownValues`
  - `KNOWN_VALUES`
  - `RegistryEntry`
  - `OntologyInfo`
  - `RegistryFile`
  - `GeneratedInfo`
  - `LoadError`
  - `LoadResult`
  - `DirectoryConfig`
  - `ConfigError`
  - free functions in `directory_loader.rs`
- Public items without doc comments:
  - registry macro expansion constants do not have per-constant doc comments
- Package metadata description:
  - `Blockchain Commons Known Values.`
- README: yes
  - Summary: package introduction, dependency snippet, version history, community review status

## Test Catalog

### Inline Unit Tests

- `known_values_registry.rs`
  - `test_1`
    - Covers: `IS_A`, `KNOWN_VALUES.get()`, lookup by name
    - Vectors: numeric value `1`, string `"isA"`
- `directory_loader.rs`
  - `test_parse_registry_json`
  - `test_parse_minimal_registry`
  - `test_parse_full_entry`
  - `test_directory_config_default`
  - `test_directory_config_custom_paths`
  - `test_directory_config_with_default`
  - `test_load_from_nonexistent_directory`
  - `test_load_result_methods`

### Integration Tests

- `tests/directory_loading.rs`
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

### Coverage Gaps To Fill In Target Tests

Rust does not include dedicated unit tests for all of the `KnownValue` and `KnownValuesStore` API surface. The C# translation should add direct tests for:

- `KnownValue` construction with and without names
- equality ignoring assigned names
- `ToString`, name fallback, and implicit conversions
- CBOR tagged round-trips and digest generation
- `KnownValuesStore` helper methods:
  - `AssignedName`
  - `Name`
  - `KnownValueForRawValue`
  - `KnownValueForName`
  - `NameForKnownValue`
- configuration locking APIs:
  - `SetDirectoryConfig`
  - `AddSearchPaths`

## EXPECTED TEXT OUTPUT RUBRIC

- Applicable: no
- Reason: the Rust test suite validates scalar values, JSON parsing, directory traversal, and registry lookup behavior; it does not include complex rendered text output that benefits from whole-text assertions.

## Translation Unit Order

1. Project config and package scaffold
2. `KnownValue`
3. `KnownValuesStore`
4. `DirectoryLoader` support types and functions
5. Registry constants and `LazyKnownValues`
6. Core API tests for `KnownValue` and `KnownValuesStore`
7. Registry tests
8. Directory-loading tests

## Hazards

- Rust macro expansion
  - `const_known_value!` generates paired raw/value constants; expand them manually in C# and keep the inventory exact.
- Rust free functions and crate-root re-exports
  - C# needs a static utility class because it has no crate-level free functions.
- Rust `const fn` plus `'static` string distinction
  - C# cannot express custom object constants; model registry entries with `static readonly` values while preserving public raw numeric constants.
- Rust `Once + Mutex<Option<T>>`
  - C# should preserve thread-safe lazy initialization and one-time config locking for the global registry.
- Rust `Result`-returning config functions
  - Use idiomatic C# exceptions, but preserve the public `LoadError` / `ConfigError` type surface in a form callers can catch and inspect.
- Optional directory-loading feature
  - It is enabled by default in Rust; the initial C# translation should include it unconditionally.
- Name-sensitive override behavior
  - When a codepoint is replaced, the old name must be removed from the name index to avoid stale lookups.
- Global registry initializer quirks
  - The current Rust `KNOWN_VALUES` initializer omits `VALUE` and `SELF` even though both constants are public; the C# default registry must preserve that exact behavior until the Rust source changes.
- Path handling
  - Nonexistent directories are success cases, not errors.

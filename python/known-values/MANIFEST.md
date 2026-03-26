# Manifest: known-values → Python

## Crate Summary

- Crate: `known-values`
- Version: `0.15.5`
- Package description: `Blockchain Commons Known Values.`
- Python package name: `known-values`
- Python import package: `known_values`
- Internal BC dependencies:
  - `bc-components` `^0.31.0` with default features disabled
  - `dcbor` `^0.25.0`
- Default feature set:
  - `directory-loading`
- Non-default / optional Rust dependencies gated by the default feature:
  - `serde` for JSON deserialization
  - `serde_json` for JSON parsing
  - `dirs` for home-directory lookup
- Additional Rust-only implementation helper:
  - `paste` macro for generating `<NAME>_RAW` constants
- Dev dependency:
  - `tempfile`

## Type Catalog

- `KnownValue`
  - Kind: struct
  - Fields:
    - `value: u64`
    - `assigned_name: Option<KnownValueName>` (private helper enum; not public)
  - Derives / behavior:
    - `Clone`, `Debug`
    - `PartialEq`, `Eq`, and `Hash` implemented manually using only `value`
    - `Display` returns assigned name if present, otherwise decimal value
  - Protocol / trait behavior to translate:
    - `DigestProvider`
    - `CBORTagged`
    - `CBORTaggedEncodable`
    - `CBORTaggedDecodable`
    - conversions from `u64`, `i32`, `usize`, and `CBOR`

- `KnownValuesStore`
  - Kind: struct
  - Fields:
    - `known_values_by_raw_value: HashMap<u64, KnownValue>`
    - `known_values_by_assigned_name: HashMap<String, KnownValue>`
  - Derives / behavior:
    - `Clone`, `Debug`
    - `Default` creates an empty store

- `LazyKnownValues`
  - Kind: struct
  - Fields:
    - `init: Once`
    - `data: Mutex<Option<KnownValuesStore>>`
  - Derives / behavior:
    - `Debug`
    - public but `#[doc(hidden)]`

- `RegistryEntry`
  - Kind: struct
  - Fields:
    - `codepoint: u64`
    - `name: String`
    - `entry_type: Option<String>` (`type` in JSON)
    - `uri: Option<String>`
    - `description: Option<String>`
  - Derives / behavior:
    - `Debug`, `Deserialize`

- `OntologyInfo`
  - Kind: struct
  - Fields:
    - `name: Option<String>`
    - `source_url: Option<String>`
    - `start_code_point: Option<u64>`
    - `processing_strategy: Option<String>`
  - Derives / behavior:
    - `Debug`, `Deserialize`

- `GeneratedInfo`
  - Kind: struct
  - Fields:
    - `tool: Option<String>`
  - Derives / behavior:
    - `Debug`, `Deserialize`

- `RegistryFile`
  - Kind: struct
  - Fields:
    - `ontology: Option<OntologyInfo>`
    - `generated: Option<GeneratedInfo>`
    - `entries: Vec<RegistryEntry>`
    - `statistics: Option<serde_json::Value>`
  - Derives / behavior:
    - `Debug`, `Deserialize`

- `LoadError`
  - Kind: enum
  - Variants:
    - `Io(io::Error)`
    - `Json { file: PathBuf, error: serde_json::Error }`
  - Trait behavior:
    - `Display`
    - `std::error::Error`
    - `From<io::Error>`

- `LoadResult`
  - Kind: struct
  - Fields:
    - `values: HashMap<u64, KnownValue>`
    - `files_processed: Vec<PathBuf>`
    - `errors: Vec<(PathBuf, LoadError)>`
  - Derives / behavior:
    - `Debug`, `Default`

- `DirectoryConfig`
  - Kind: struct
  - Fields:
    - `paths: Vec<PathBuf>`
  - Derives / behavior:
    - `Debug`, `Clone`, `Default`

- `ConfigError`
  - Kind: enum
  - Variants:
    - `AlreadyInitialized`
  - Trait behavior:
    - `Debug`, `Clone`, `PartialEq`, `Eq`
    - `Display`
    - `std::error::Error`

## Function Catalog

### `KnownValue`

- `new(value: u64) -> Self`
- `new_with_name<T: Into<u64>>(value: T, assigned_name: String) -> Self`
- `new_with_static_name(value: u64, name: &'static str) -> Self`
- `value(&self) -> u64`
- `assigned_name(&self) -> Option<&str>`
- `name(&self) -> String`
- `digest(&self) -> Digest`
- `cbor_tags() -> Vec<Tag>`
- `untagged_cbor(&self) -> CBOR`
- `from_untagged_cbor(cbor: CBOR) -> dcbor::Result<Self>`

### `KnownValuesStore`

- `new<T: IntoIterator<Item = KnownValue>>(known_values: T) -> Self`
- `insert(&mut self, known_value: KnownValue)`
- `assigned_name(&self, known_value: &KnownValue) -> Option<&str>`
- `name(&self, known_value: KnownValue) -> String`
- `known_value_named(&self, assigned_name: &str) -> Option<&KnownValue>`
- `known_value_for_raw_value(raw_value: u64, known_values: Option<&Self>) -> KnownValue`
- `known_value_for_name(name: &str, known_values: Option<&Self>) -> Option<KnownValue>`
- `name_for_known_value(known_value: KnownValue, known_values: Option<&Self>) -> String`
- default constructor equivalent for `Default`
- default-feature-only methods:
  - `load_from_directory(&mut self, path: &Path) -> Result<usize, LoadError>`
  - `load_from_config(&mut self, config: &DirectoryConfig) -> LoadResult`

### `LazyKnownValues`

- `get(&self) -> MutexGuard<'_, Option<KnownValuesStore>>`

### `LoadResult`

- `values_count(&self) -> usize`
- `values_iter(&self) -> impl Iterator<Item = &KnownValue>`
- `into_values(self) -> impl Iterator<Item = KnownValue>`
- `has_errors(&self) -> bool`

### `DirectoryConfig`

- `new() -> Self`
- `default_only() -> Self`
- `with_paths(paths: Vec<PathBuf>) -> Self`
- `with_paths_and_default(paths: Vec<PathBuf>) -> Self`
- `default_directory() -> PathBuf`
- `paths(&self) -> &[PathBuf]`
- `add_path(&mut self, path: PathBuf)`

### Free functions

- `load_from_directory(path: &Path) -> Result<Vec<KnownValue>, LoadError>`
- `load_from_config(config: &DirectoryConfig) -> LoadResult`
- `set_directory_config(config: DirectoryConfig) -> Result<(), ConfigError>`
- `add_search_paths(paths: Vec<PathBuf>) -> Result<(), ConfigError>`

## Constant Catalog

### Singleton / state

- `KNOWN_VALUES: LazyKnownValues`

### Registry constant pairs

Each `const_known_value!(value, NAME, display_name)` expands to:

- `NAME_RAW: u64`
- `NAME: KnownValue`

These public constant pairs are required for all 104 registry entries:

- General:
  - `UNIT`, `IS_A`, `ID`, `SIGNED`, `NOTE`, `HAS_RECIPIENT`, `SSKR_SHARE`, `CONTROLLER`, `KEY`, `DEREFERENCE_VIA`, `ENTITY`, `NAME`, `LANGUAGE`, `ISSUER`, `HOLDER`, `SALT`, `DATE`, `UNKNOWN_VALUE`, `VERSION_VALUE`, `HAS_SECRET`, `DIFF_EDITS`, `VALID_FROM`, `VALID_UNTIL`, `POSITION`, `NICKNAME`, `VALUE`, `ATTESTATION`, `VERIFIABLE_AT`
- Attachments:
  - `ATTACHMENT`, `VENDOR`, `CONFORMS_TO`
- XID Documents:
  - `ALLOW`, `DENY`, `ENDPOINT`, `DELEGATE`, `PROVENANCE`, `PRIVATE_KEY`, `SERVICE`, `CAPABILITY`, `PROVENANCE_GENERATOR`
- XID Privileges:
  - `PRIVILEGE_ALL`, `PRIVILEGE_AUTH`, `PRIVILEGE_SIGN`, `PRIVILEGE_ENCRYPT`, `PRIVILEGE_ELIDE`, `PRIVILEGE_ISSUE`, `PRIVILEGE_ACCESS`, `PRIVILEGE_DELEGATE`, `PRIVILEGE_VERIFY`, `PRIVILEGE_UPDATE`, `PRIVILEGE_TRANSFER`, `PRIVILEGE_ELECT`, `PRIVILEGE_BURN`, `PRIVILEGE_REVOKE`
- Expression and Function Calls:
  - `BODY`, `RESULT`, `ERROR`, `OK_VALUE`, `PROCESSING_VALUE`, `SENDER`, `SENDER_CONTINUATION`, `RECIPIENT_CONTINUATION`, `CONTENT`
- Cryptography:
  - `SEED_TYPE`, `PRIVATE_KEY_TYPE`, `PUBLIC_KEY_TYPE`, `MASTER_KEY_TYPE`
- Cryptocurrency Assets:
  - `ASSET`, `BITCOIN_VALUE`, `ETHEREUM_VALUE`, `TEZOS_VALUE`
- Cryptocurrency Networks:
  - `NETWORK`, `MAIN_NET_VALUE`, `TEST_NET_VALUE`
- Bitcoin:
  - `BIP32_KEY_TYPE`, `CHAIN_CODE`, `DERIVATION_PATH_TYPE`, `PARENT_PATH`, `CHILDREN_PATH`, `PARENT_FINGERPRINT`, `PSBT_TYPE`, `OUTPUT_DESCRIPTOR_TYPE`, `OUTPUT_DESCRIPTOR`
- Graphs:
  - `GRAPH`, `SOURCE_TARGET_GRAPH`, `PARENT_CHILD_GRAPH`, `DIGRAPH`, `ACYCLIC_GRAPH`, `MULTIGRAPH`, `PSEUDOGRAPH`, `GRAPH_FRAGMENT`, `DAG`, `TREE`, `FOREST`, `COMPOUND_GRAPH`, `HYPERGRAPH`, `DIHYPERGRAPH`, `NODE`, `EDGE`, `SOURCE`, `TARGET`, `PARENT`, `CHILD`, `SELF`

## Trait / Protocol Coverage

- `KnownValue`
  - digest provider protocol
  - CBOR tagged encoding / decoding protocol
  - numeric conversion equivalents
- `LoadError`
  - exception / error representation preserving IO-vs-JSON context
- `ConfigError`
  - equality-capable exception / error representation

## Doc Catalog

- Crate-level docs: yes
  - `lib.rs` includes crate overview, basic usage, directory loading behavior, JSON format, configuration examples, and compile-time disabling note.
- Module-level docs:
  - `directory_loader.rs` has extensive module docs.
- README: yes
  - Short introduction plus version history and project metadata.
- Package metadata description: yes
  - `Blockchain Commons Known Values.`
- Public items with doc comments:
  - `KnownValue`
  - all public `KnownValue` methods
  - `KnownValuesStore`
  - all public `KnownValuesStore` methods
  - `LazyKnownValues`
  - `LazyKnownValues::get`
  - `KNOWN_VALUES`
  - `RegistryEntry`
  - `OntologyInfo`
  - `RegistryFile`
  - `GeneratedInfo`
  - `LoadError`
  - `LoadResult`
  - all public `LoadResult` methods
  - `DirectoryConfig`
  - all public `DirectoryConfig` methods
  - free directory-loading functions
  - `ConfigError`
- Public items without meaningful standalone docs:
  - the individual registry constants are not individually documented
  - generated `*_RAW` constants are not individually documented

## External Dependency Equivalents

- `bc-components`
  - Purpose: provides `Digest` and `DigestProvider`; Rust also re-exports `bc_tags` as `tags`
  - Python mapping: use in-repo `bc-components` translation for `Digest` / `DigestProvider`
  - Note: Python `bc-components` does not currently mirror Rust’s `tags` re-export, so known-values should import tag constants/helpers from `bc_tags` directly for now
- `dcbor`
  - Purpose: deterministic CBOR type, tags, and error model
  - Python mapping: use in-repo `dcbor` translation
- `paste`
  - Purpose: macro expansion for `<NAME>_RAW`
  - Python mapping: manual constant expansion; no runtime dependency
- `serde`
  - Purpose: JSON deserialization
  - Python mapping: `dataclasses` + `json` + manual field conversion
- `serde_json`
  - Purpose: JSON parsing
  - Python mapping: stdlib `json`
- `dirs`
  - Purpose: locate home directory
  - Python mapping: `pathlib.Path.home()` with fallback to `Path(".")`
- `tempfile` (tests)
  - Purpose: temporary directories for registry-file tests
  - Python mapping: stdlib `tempfile.TemporaryDirectory`

## Feature Flags

- `default = ["directory-loading"]`
  - Gates:
    - `directory_loader` module
    - public re-exports from `directory_loader`
    - `KnownValuesStore.load_from_directory`
    - `KnownValuesStore.load_from_config`
    - automatic directory loading inside `KNOWN_VALUES.get()`
- Translation decision:
  - Translate default feature only.
  - Model directory-loading as always available in Python.
  - Do not implement a no-directory-loading package variant in this translation.

## Test Inventory

- `src/known_values_registry.rs`
  - `test_1`
    - Verifies `IS_A` numeric value and name
    - Verifies `KNOWN_VALUES.get()` lookup by name

- `src/directory_loader.rs`
  - `test_parse_registry_json`
    - Parses JSON with ontology + entries + statistics
  - `test_parse_minimal_registry`
    - Parses minimal JSON with only entries
  - `test_parse_full_entry`
    - Parses optional entry fields
  - `test_directory_config_default`
    - Default directory path contains `.known-values`
  - `test_directory_config_custom_paths`
    - Custom path ordering preserved
  - `test_directory_config_with_default`
    - Custom paths plus appended default path
  - `test_load_from_nonexistent_directory`
    - Missing directory returns empty success
  - `test_load_result_methods`
    - `values_count` / `has_errors` behavior

- `tests/directory_loading.rs`
  - `test_global_registry_still_works`
    - `KNOWN_VALUES` still resolves hardcoded values with directory feature enabled
  - `test_load_from_temp_directory`
    - Loads one JSON file into a store while preserving existing entries
  - `test_override_hardcoded_value`
    - Later loaded codepoint replaces prior name mapping
  - `test_multiple_files_in_directory`
    - Multiple JSON files from one directory
  - `test_directory_config_custom_paths`
    - Multi-directory loading through config
  - `test_later_directory_overrides_earlier`
    - Later directory wins on codepoint collision
  - `test_nonexistent_directory_is_ok`
    - Store-level loader treats missing directory as success with count `0`
  - `test_invalid_json_is_error`
    - Store-level single-directory loader is strict on invalid JSON
  - `test_tolerant_loading_continues_on_error`
    - Config loader continues and records errors
  - `test_full_registry_format`
    - Full-format file with optional metadata and statistics
  - `test_load_result_methods`
    - `LoadResult` counts, processed files, iterators
  - `test_empty_entries_array`
    - Empty entries file returns count `0`
  - `test_non_json_files_ignored`
    - Non-JSON files skipped

## Expected Text Output Rubric

- Applicable: no
- Reason: the Rust crate has no tests for rendered diagnostics, pretty-printing, multiline formatting, or other complex text output; all current tests are scalar, structural, or JSON-loading behavior.

## Translation Unit Order

1. Package scaffolding and metadata
2. `KnownValue`
3. `KnownValuesStore`
4. Directory-loading data types and error types
5. Directory-loading functions and config globals
6. Registry constants and `LazyKnownValues` / `KNOWN_VALUES`
7. Package exports
8. Unit tests from `src/directory_loader.rs` and `src/known_values_registry.rs`
9. Integration tests from `tests/directory_loading.rs`

## Translation Hazards

- `const_known_value!` must be manually expanded into 104 raw integer constants plus 104 `KnownValue` instances.
- `KnownValue` equality and hashing intentionally ignore the assigned name.
- `KNOWN_VALUES` initialization in Rust does **not** include the public constants `VALUE` or `SELF`; the Python singleton must preserve that source behavior.
- `load_from_directory` is strict: one invalid JSON file aborts with `LoadError`.
- `load_from_config` is tolerant: it continues across file-level failures and records them in `LoadResult.errors`.
- Directory configuration can only be mutated before first access to `KNOWN_VALUES.get()`.
- Rust uses `bc_components::tags` via re-export; the Python dependency surface differs and needs careful mapping to the existing Python packages.
- `Path.home()` can raise; Rust falls back to `"."` if the home directory cannot be determined.
- `KNOWN_VALUES.get()` must be effectively lazy and idempotent across repeated calls.

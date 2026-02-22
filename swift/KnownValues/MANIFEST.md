# Translation Manifest: known-values

Rust crate version: 0.15.4

## Public API Surface

### Core Types

1. **KnownValue** (struct)
   - `value: u64` — numeric identifier
   - `assigned_name: Option<KnownValueName>` — optional name (Static or Dynamic)
   - Constructors: `new(u64)`, `new_with_name(T: Into<u64>, String)`, `new_with_static_name(u64, &'static str)`
   - Methods: `value() -> u64`, `assigned_name() -> Option<&str>`, `name() -> String`
   - Traits: Clone, Debug, PartialEq, Eq, Hash, Display, DigestProvider, CBORTagged, CBORTaggedEncodable, CBORTaggedDecodable, From<u64>, From<i32>, From<usize>

2. **KnownValuesStore** (struct)
   - Bidirectional mapping: raw value ↔ KnownValue, name ↔ KnownValue
   - Constructors: `new(IntoIterator<Item=KnownValue>)`, `default()` (empty)
   - Instance methods: `insert()`, `assigned_name()`, `name()`, `known_value_named()`
   - Static methods: `known_value_for_raw_value()`, `known_value_for_name()`, `name_for_known_value()`
   - Directory loading: `load_from_directory()`, `load_from_config()`
   - Traits: Clone, Debug, Default

3. **LazyKnownValues** (struct, doc(hidden))
   - `get() -> MutexGuard<Option<KnownValuesStore>>` — thread-safe lazy init
   - Uses `Once` + `Mutex` for thread safety

### Registry Constants (80+ values)

All created via `const_known_value!` macro producing two public constants each:
- `CONST_NAME: KnownValue` — the value
- `CONST_NAME_RAW: u64` — the raw numeric value

Categories: General (0–27), Attachments (50–52), XID Documents (60–68), XID Privileges (70–86), Expression/Functions (100–108), Cryptography (200–203), Assets (300–303), Networks (400–402), Bitcoin (500–508), Graphs (600–706)

### Directory Loading API (default feature)

4. **DirectoryConfig** (struct)
   - Constructors: `new()`, `default_only()`, `with_paths()`, `with_paths_and_default()`
   - Methods: `default_directory()`, `paths()`, `add_path()`

5. **RegistryFile** (struct) — JSON root
6. **RegistryEntry** (struct) — JSON entry (codepoint, name, type, uri, description)
7. **OntologyInfo** (struct) — JSON metadata
8. **GeneratedInfo** (struct) — JSON tool info
9. **LoadError** (enum) — Io, Json
10. **LoadResult** (struct) — values, files_processed, errors
    - Methods: `values_count()`, `values_iter()`, `into_values()`, `has_errors()`
11. **ConfigError** (enum) — AlreadyInitialized

### Free Functions

12. `set_directory_config(DirectoryConfig) -> Result<(), ConfigError>`
13. `add_search_paths(Vec<PathBuf>) -> Result<(), ConfigError>`
14. `load_from_directory(&Path) -> Result<Vec<KnownValue>, LoadError>`
15. `load_from_config(&DirectoryConfig) -> LoadResult`

### Global Static

16. `KNOWN_VALUES: LazyKnownValues` — global singleton

## Internal Dependencies

- `dcbor` ^0.25.0 (CBOR encoding/decoding)
- `bc-components` ^0.31.0 (Digest, DigestProvider, tags)
- `paste` ^1.0.12 (macro helper — not needed in target languages)

## Feature Flags

- `default = ["directory-loading"]` — enables JSON registry file loading
- For initial translation: include directory-loading (it's default)

## Test Inventory

### Unit Tests (src/known_values_registry.rs)
1. `test_1` — basic IS_A value and KNOWN_VALUES store lookup

### Unit Tests (src/directory_loader.rs)
2. `test_parse_registry_json` — parse JSON with ontology and entries
3. `test_parse_minimal_registry` — minimal JSON (entries only)
4. `test_parse_full_entry` — entry with all fields
5. `test_directory_config_default` — default config has one path
6. `test_directory_config_custom_paths` — custom paths
7. `test_directory_config_with_default` — custom + default
8. `test_load_from_nonexistent_directory` — returns empty
9. `test_load_result_methods` — values_count, has_errors

### Integration Tests (tests/directory_loading.rs)
10. `test_global_registry_still_works` — KNOWN_VALUES with feature enabled
11. `test_load_from_temp_directory` — load from temp dir
12. `test_override_hardcoded_value` — directory values override hardcoded
13. `test_multiple_files_in_directory` — multiple JSON files
14. `test_directory_config_custom_paths` — custom paths via config
15. `test_later_directory_overrides_earlier` — later dirs win
16. `test_nonexistent_directory_is_ok` — graceful nonexistent dir
17. `test_invalid_json_is_error` — invalid JSON produces error
18. `test_tolerant_loading_continues_on_error` — fault-tolerant loading
19. `test_full_registry_format` — full BlockchainCommons format
20. `test_load_result_methods` — iteration and counts
21. `test_empty_entries_array` — empty entries
22. `test_non_json_files_ignored` — non-JSON files skipped

## Translation Hazards

1. **Static vs Dynamic name distinction** — Rust uses enum for const-fn support; target languages can use a single String since they don't need const-fn
2. **Macro-generated constants** — `const_known_value!` creates both a KnownValue and a raw u64; target languages need both forms
3. **Thread-safe lazy singleton** — Rust uses `Once` + `Mutex`; each language has its own idiom
4. **Home directory lookup** — Rust uses `dirs::home_dir()`; each platform has its own approach
5. **JSON parsing** — Rust uses `serde_json`; each language has native JSON support
6. **`SELF` constant** — `SELF` is a keyword/reserved word in most languages; needs escaping or renaming
7. **Feature flags** — Rust's `cfg(feature)` doesn't exist in most languages; include directory loading unconditionally
8. **`VALUE` and `SELF` omitted from global store** — The Rust code intentionally omits these from the KNOWN_VALUES initialization; translations must match

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: no — This crate has no complex rendered text output formats. Tests verify values, names, and CBOR encoding rather than formatted text.

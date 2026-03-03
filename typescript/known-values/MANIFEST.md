# Translation Manifest: known-values → TypeScript (@bc/known-values)

## Crate Metadata
- Name: `known-values`
- Version: `0.15.4`
- Rust edition: `2024`
- Description: `Blockchain Commons Known Values.`
- Repository: <https://github.com/BlockchainCommons/known-values-rust>

## Dependencies
### Internal BC Dependencies
- `bc-components = ^0.31.0`
  - TypeScript equivalent: `@bc/components` (file:../bc-components)
  - Used for: `Digest`, `DigestProvider`, `tags` (TAG_KNOWN_VALUE)
- `dcbor = ^0.25.0` (Rust enables `multithreaded` feature)
  - TypeScript equivalent: `@bc/dcbor` (file:../dcbor)
  - Used for: CBOR encoding/decoding, `Tag`, `Cbor`, `tagsForValues`
- `bc-tags` (transitive via bc-components)
  - TypeScript equivalent: `@bc/tags` (file:../bc-tags)
  - Used for: `TAG_KNOWN_VALUE` constant

### External Dependencies
- `paste = ^1.0.12`
  - Rust-only macro helper. No TypeScript equivalent needed; constants declared directly.
- `serde = 1.0` + `serde_json = 1.0` (via default `directory-loading` feature)
  - TypeScript equivalent: built-in `JSON.parse()`.
- `dirs = 5.0` (via default `directory-loading` feature)
  - TypeScript equivalent: `os.homedir()` from Node.js stdlib.
- `tempfile = 3.10` (dev-dependency)
  - TypeScript equivalent: `fs.mkdtempSync()` from Node.js stdlib.

## Feature Flags
- `default = ["directory-loading"]`
- Translation scope: **default features only**, so directory-loading APIs are included.

## Public API Surface
### Type Catalog
- `KnownValue` — class with `value: bigint`, optional `assignedName: string | undefined`
- `KnownValuesStore` — class with bidirectional Map<bigint, KnownValue> + Map<string, KnownValue>
- `KNOWN_VALUES` — lazy singleton (module-level)
- `RegistryEntry` — interface: `codepoint`, `name`, `entryType?`, `uri?`, `description?`
- `OntologyInfo` — interface: `name?`, `sourceUrl?`, `startCodePoint?`, `processingStrategy?`
- `RegistryFile` — interface: `ontology?`, `generated?`, `entries`, `statistics?`
- `GeneratedInfo` — interface: `tool?`
- `LoadError` — class with `kind: 'io' | 'json'`, `message`, `filePath?`
- `LoadResult` — class: `values`, `filesProcessed`, `errors`
- `DirectoryConfig` — class: private `paths: string[]`
- `ConfigError` — class with `kind: 'alreadyInitialized'`

### Function Catalog
- `KnownValue` constructor and factory methods
- `KnownValuesStore` constructor, `insert()`, `assignedName()`, `name()`, `knownValueNamed()`
- `KnownValuesStore` static: `knownValueForRawValue()`, `knownValueForName()`, `nameForKnownValue()`
- `KnownValuesStore` directory: `loadFromDirectory()`, `loadFromConfig()`
- `LoadResult`: `valuesCount()`, `hasErrors()`
- `DirectoryConfig`: `defaultOnly()`, `withPaths()`, `withPathsAndDefault()`, `defaultDirectory()`, `paths`, `addPath()`
- Free functions: `loadFromDirectory()`, `loadFromConfig()`, `setDirectoryConfig()`, `addSearchPaths()`

### Constant Catalog
104 known value constants (each with `_RAW` bigint companion). See Kotlin MANIFEST.md for full table.

### Trait / Conversion Catalog
- `DigestProvider` interface implementation on `KnownValue`
- CBOR tagged encoding/decoding (`cborTags()`, `untaggedCbor()`, `taggedCbor()`, static `fromUntaggedCbor()`, static `fromCbor()`)
- `toString()` on `KnownValue`
- Equality by numeric value only (custom `equals()`)

## Test Inventory
Total Rust tests in scope: **22**.
- `src/known_values_registry.rs`: `test_1` (1 test)
- `src/directory_loader.rs`: 8 unit tests
- `tests/directory_loading.rs`: 13 integration tests

## EXPECTED TEXT OUTPUT RUBRIC
- Applicable: no
- Reason: tests are structural/value assertions, no complex formatted-text output.

## Translation Unit Order
1. Build/config scaffold (.gitignore, package.json, tsconfig.json, vitest.config.ts)
2. Core type: `KnownValue` (known-value.ts)
3. Store: `KnownValuesStore` (known-values-store.ts)
4. Registry constants + lazy singleton (known-values-registry.ts)
5. Directory loader types/errors/config/functions (directory-loader.ts)
6. Re-export surface (index.ts)
7. Unit tests (known-values-registry.test.ts, directory-loader.test.ts)
8. Integration tests (directory-loading.test.ts)

## Translation Hazards
- **Macro surface**: Rust's `const_known_value!` → explicit TypeScript constants.
- **Scale**: 104 constants prone to transcription errors.
- **Registry parity**: KNOWN_VALUES initializes 102 constants (missing VALUE, SELF).
- **Equality**: KnownValue equality/hash ignores assigned name, uses numeric value only.
- **Directory loading**: strict loader fails fast; tolerant loader accumulates errors.
- **Configuration lock**: accessing KNOWN_VALUES locks further config changes.
- **File order**: directory iteration order varies by OS; don't depend on specific order.
- **bigint**: TypeScript uses `bigint` for u64; raw constants must be `bigint` literals.

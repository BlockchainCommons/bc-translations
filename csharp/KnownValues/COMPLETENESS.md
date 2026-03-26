# Completeness: known-values → C# (KnownValues)

## Build & Config
- [x] .gitignore
- [x] KnownValues.slnx
- [x] KnownValues/KnownValues.csproj
- [x] KnownValues.Tests/KnownValues.Tests.csproj

## Source Files
- [x] KnownValue.cs — core known value type, display, equality, digest, CBOR
- [x] KnownValuesStore.cs — bidirectional store and directory-loading helpers
- [x] KnownValuesRegistry.cs — registry constants and global lazy store
  - [x] Preserves the Rust global-registry omission of `VALUE` and `SELF`
- [x] DirectoryLoader.cs — JSON models, directory config, tolerant loading, global config

## Tests
- [x] KnownValueTests.cs — construction, naming, equality, conversions, digest, CBOR
- [x] KnownValuesStoreTests.cs — lookup, overrides, helper methods, default store behavior
- [x] KnownValuesRegistryTests.cs — constant inventory and lazy global registry
- [x] DirectoryLoaderTests.cs — JSON parsing, config behavior, tolerant loading, integration loading

## Documentation Coverage
- [x] Crate-level/package metadata description
- [x] Public API XML doc comments

## Derive/Protocol Coverage
- [x] KnownValue — equality, hashing, display, digest provider, CBOR tagging
- [x] KnownValuesStore — clone-equivalent behavior and default constructor
- [x] DirectoryConfig / LoadResult / error types — default/clone/equality semantics where applicable

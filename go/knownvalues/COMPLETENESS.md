# Completeness: known-values → Go (knownvalues)

## Build & Config
- [x] .gitignore
- [x] go.mod (bctags-go promoted to direct dependency)
- [x] go.sum

## Source Files
- [x] doc.go — package documentation
- [x] known_value.go — core known value type, display, digest, and CBOR support (uses bctags.TagKnownValue)
- [x] known_values_store.go — bidirectional store helpers and directory-loading integration
- [x] registry.go — registry constants (104) and lazy global store
- [x] directory_loader.go — JSON models, directory config, tolerant loading, and global config

## Tests
- [x] known_value_test.go — construction, naming, equality, digest, and CBOR decoding
- [x] known_values_store_test.go — lookup helpers, overrides, cloning, and default behavior
- [x] registry_test.go — registry inventory and lazy global store behavior
- [x] directory_loader_test.go — JSON parsing, directory loading, tolerant loading, and config locking

## Documentation Coverage
- [x] Crate-level/package metadata description
- [x] Public API Go doc comments

## Derive/Protocol Coverage
- [x] KnownValue — equality semantics, string form, digest provider, tagged CBOR encode/decode
- [x] KnownValuesStore — clone-equivalent behavior and zero/default initialization
- [x] DirectoryConfig / LoadResult / error types — config, iteration helpers, and error semantics

## Cross-Model Fluency (Claude Opus 4.6, 2026-03-26)
- [x] bctags.TagKnownValue used instead of hardcoded tag value 40000
- [x] normalizePath removed — paths stored as-is matching Rust
- [x] NewKnownValueWithStaticName unexported (Go has no const fn)
- [x] KnownValue.Clone() removed (value type copies on assignment)
- [x] IntoValues/ValuesIter consolidated into Values()
- [x] FileLoadError removed — LoadError already carries Path
- [x] LoadResult.Values field renamed to ValuesMap (avoids method collision)
- [x] errors.As used consistently in ensureLoadError
- [x] slices.SortFunc used instead of sort.Slice (Go 1.22)
- [x] bctags-go promoted from indirect to direct dependency

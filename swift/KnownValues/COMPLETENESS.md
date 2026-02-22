# Completeness: known-values → Swift (KnownValues)

## Source Files
- [x] KnownValue.swift — core KnownValue struct with CBOR and DigestProvider
- [x] KnownValuesStore.swift — bidirectional lookup store
- [x] KnownValuesRegistry.swift — 80+ constants, raw values, LazyKnownValues singleton
- [x] DirectoryLoader.swift — JSON registry parsing, directory config, loading

## Public API
- [x] KnownValue struct (value, assignedName, name, init variants)
- [x] KnownValue: Equatable, Hashable, CustomStringConvertible
- [x] KnownValue: DigestProvider
- [x] KnownValue: CBORTaggedCodable (encode + decode)
- [x] KnownValuesStore (init, insert, lookup by name/value)
- [x] KnownValuesStore static methods (knownValue(forRawValue:), knownValue(forName:), name(for:))
- [x] KnownValuesStore directory loading (loadFromDirectory, loadFromConfig)
- [x] LazyKnownValues singleton (knownValues.get())
- [x] DirectoryConfig (defaultOnly, withPaths, withPathsAndDefault, defaultDirectory, addPath)
- [x] RegistryFile, RegistryEntry, OntologyInfo, GeneratedInfo (Decodable)
- [x] LoadError enum (io, json)
- [x] ConfigError enum (alreadyInitialized)
- [x] LoadResult struct (values, filesProcessed, errors, valuesCount, hasErrors)
- [x] Free functions: setDirectoryConfig, addSearchPaths, loadFromDirectory, loadFromConfig
- [x] All 80+ known value constants
- [x] All 80+ raw value constants

## Tests
- [x] KnownValuesRegistryTests — 16 tests (basic values, equality, hashing, store, constants)
- [x] DirectoryLoadingTests — 21 tests (temp dirs, override, multi-file, config, JSON parsing)
- [x] CBORTests — 6 tests (round-trip, encoding, tags, digest)

## Build & Config
- [x] Package.swift
- [x] .gitignore

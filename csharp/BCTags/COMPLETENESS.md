# Completeness: bc-tags → C# (BCTags)

## Build & Config
- [x] .gitignore
- [x] BCTags.slnx
- [x] BCTags/BCTags.csproj
- [x] BCTags.Tests/BCTags.Tests.csproj

## Source Files
- [x] Tags.cs — CBOR tag constants and registry helpers
  - [x] 75 Rust `const_cbor_tag!` declarations translated
  - [x] 150 public constants (`Tag*` + `TagName*`) generated
  - [x] `RegisterTagsIn(TagsStore)` translated
  - [x] `RegisterTags()` translated
  - [x] Registration order preserved (75 entries)
  - [x] `dcbor::register_tags_in` behavior preserved via `GlobalTags.RegisterTagsIn`

## Tests
- [x] TagsTests.cs — registration and tag name/value parity checks
  - [x] `TagConstantsMatchRustRegistry`
  - [x] `RegisterTagsInRegistersDcborAndBcTags`
  - [x] `RegisterTagsRegistersInGlobalStore`

## Documentation Coverage
- [x] Public API docs translated from Rust doc comments
- [x] XML doc comments added for all 150 public constants and 2 public methods
- [x] Package metadata description present in `BCTags.csproj`

## Rust Test Inventory Mapping
- [x] Rust tests: 0 discovered in `rust/bc-tags`
- [x] C# parity tests added to validate constant and registration behavior

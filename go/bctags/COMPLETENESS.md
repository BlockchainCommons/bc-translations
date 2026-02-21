# Completeness: bc-tags → Go (bctags)

## Source Files
- [x] tags_registry.go — 150 tag constants + registration functions
- [x] doc.go — package documentation

## Tests
- [x] tags_registry_test.go — constant parity, registration, global store
  - [x] TestConstantCount (75 entries)
  - [x] TestConstantValues (spot-checks)
  - [x] TestBcTagsSliceLength
  - [x] TestBcTagsSliceMatchesExpected
  - [x] TestRegisterTagsIn (dcbor + bc-tags in custom store)
  - [x] TestRegisterTagsInIdempotent
  - [x] TestRegisterTagsGlobal
  - [x] TestFirstAndLastTags
  - [x] TestMidRangeSpotChecks
  - [x] TestUniqueValues
  - [x] TestUniqueNames

## Build & Config
- [x] go.mod
- [x] go.sum
- [x] .gitignore

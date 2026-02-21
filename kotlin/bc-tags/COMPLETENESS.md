# Completeness: bc-tags → Kotlin

## Build & Config
- [x] .gitignore
- [x] build.gradle.kts
- [x] settings.gradle.kts

## Source Files
- [x] TagsRegistry.kt — tag constants and registration helpers

## Tests
- [x] TagsRegistryTest.kt — registration behavior and constant sanity checks (3 tests)

## API Coverage
- [x] All public constants from Rust translated (150 constants from 75 tags)
- [x] registerTagsIn(tagsStore: TagsStore)
- [x] registerTags()

## Documentation Coverage
- [x] Crate/module-level docs translated for public entry points
- [x] Public API doc comments translated where present in Rust (none on Rust public API)

## Checker Summary
- [x] API coverage validated at 152/152
- [x] Signature compatibility validated (0 mismatches)
- [x] Rust test parity validated at 0/0 (plus Kotlin sanity tests passing)

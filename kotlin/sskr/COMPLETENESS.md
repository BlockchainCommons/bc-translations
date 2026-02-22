# Completeness: sskr → Kotlin (sskr)

## Source Files
- [x] SskrException.kt — error hierarchy matching Rust `Error`
- [x] Secret.kt — validated secret wrapper
- [x] Spec.kt — split spec and group spec types
- [x] SskrShare.kt — internal share metadata container
- [x] Sskr.kt — constants and public generate/combine API

## Tests
- [x] SskrTest.kt — translated Rust behavioral tests
  - [x] testSplit35
  - [x] testSplit27
  - [x] testSplit2323
  - [x] testShuffle
  - [x] fuzzTest
  - [x] exampleEncode
  - [x] exampleEncode3
  - [x] exampleEncode4
  - [x] omitted Rust metadata-only tests documented (`test_readme_deps`, `test_html_root_url`)

## Build & Config
- [x] build.gradle.kts
- [x] settings.gradle.kts
- [x] .gitignore
- [x] package metadata description present

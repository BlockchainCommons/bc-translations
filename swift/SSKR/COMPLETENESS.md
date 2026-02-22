# Completeness: sskr → Swift (SSKR)

## Source Files
- [x] SSKRError.swift — error enum (15 cases)
- [x] Secret.swift — Secret type with validation
- [x] GroupSpec.swift — group specification type
- [x] Spec.swift — split specification type
- [x] SSKRShare.swift — internal share type
- [x] SSKR.swift — public constants and generate/combine functions

## Tests
- [x] SSKRTests.swift — all test cases
  - [x] testSplit3Of5
  - [x] testSplit2Of7
  - [x] testSplit2Of3_2Of3
  - [x] testShuffle
  - [x] fuzzTest (100 iterations)
  - [x] exampleEncode
  - [x] exampleEncode3
  - [x] exampleEncode4

## Build & Config
- [x] Package.swift
- [x] .gitignore

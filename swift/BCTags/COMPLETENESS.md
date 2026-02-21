# Completeness: bc-tags → Swift (BCTags)

## Source Files
- [x] Tag.swift — Tag struct with Sendable, Hashable, ExpressibleByIntegerLiteral, CustomStringConvertible
- [x] TagsStore.swift — TagsStoreProtocol, TagsStore, TagsIterator, globalTags, name(for:knownTags:)
- [x] Tags.swift — 75 tag constants + date base tag + registerTagsIn + registerTags

## Tests
- [x] TagsTests.swift — tag constant validation
  - [x] All 75 tag value/name pairs verified
  - [x] All tag values are unique
  - [x] All tag names are unique
  - [x] registerTagsIn populates store with all tags (including dcbor date tag)
  - [x] registerTags mutates global store
  - [x] Tag equality/hashing behavior (same value = equal, different value = not equal)
  - [x] Tag description output (name when available, value when not)
  - [x] Tag integer literal conformance
  - [x] Tag with multiple names uses first as preferred
  - [x] TagsStore lookup by value and name
  - [x] TagsStore assigned name and name fallback
  - [x] TagsStore iteration in numeric order
  - [x] Free function name(for:knownTags:)

## Build & Config
- [x] .gitignore
- [x] Package.swift

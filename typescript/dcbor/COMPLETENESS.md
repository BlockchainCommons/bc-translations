# Completeness: dcbor → TypeScript (@bc/dcbor)

## Source Files
- [x] src/index.ts
- [x] src/cbor.ts
- [x] src/byte-string.ts
- [x] src/map.ts
- [x] src/set.ts
- [x] src/simple.ts
- [x] src/date.ts
- [x] src/tag.ts
- [x] src/tags.ts
- [x] src/tags-store.ts
- [x] src/error.ts
- [x] src/decode.ts
- [x] src/diag.ts
- [x] src/dump.ts
- [x] src/conveniences.ts
- [x] src/walk.ts
- [x] src/cbor-codable.ts
- [x] src/cbor-tagged.ts
- [x] src/cbor-tagged-encodable.ts
- [x] src/cbor-tagged-decodable.ts
- [x] src/cbor-tagged-codable.ts
- [x] src/exact.ts
- [x] src/float.ts
- [x] src/varint.ts
- [x] src/string-util.ts
- [x] src/prelude.ts
- [x] src/bignum.ts
- [x] src/stdlib.ts
- [x] src/global.d.ts
- [x] src/globals.d.ts
- [x] src/collections.d.ts

## API Surface
- [x] Core CBOR model and major types (`Cbor`, `MajorType`, encode/decode helpers)
- [x] Deterministic map and set behavior (`CborMap`, `CborSet`) with canonical key ordering checks
- [x] Error model equivalent to Rust `Error` (`CborError`, discriminated union error variants)
- [x] Tag model and registry constants (`TAG_DATE`, bignum tags, `registerTags`, `tagsForValues`)
- [x] Global/local tag store and summarizer support (`TagsStore`, `withTags`, `withTagsMut`)
- [x] Date tagged type support (`CborDate`)
- [x] Trait-equivalent protocols (`CborEncodable`, `CborDecodable`, `CborTagged*`)
- [x] Diagnostic and summary rendering (`diagnosticOpt`, `summary`)
- [x] Hex rendering including annotated form (`hexOpt`, `hexAnnotated`)
- [x] Traversal API (`walk`, `WalkElement`, `EdgeType`, edge labels)
- [x] Numeric exactness helpers (`exact.ts`) and float canonicalization helpers
- [x] Big integer tagged conversion helpers (`bignum.ts`, tags 2/3)
- [x] Convenience extraction/type-guard helpers (map/array/tag/number/bool/text/bytes APIs)

## Tests
- [x] tests/encode.test.ts — 68 tests
- [x] tests/format.test.ts — 20 tests
- [x] tests/walk.test.ts — 12 tests
- [x] tests/error.test.ts — 37 tests
- [x] tests/tags-store.test.ts — 16 tests
- [x] tests/bignum.test.ts — 58 tests
- [x] tests/cli.test.ts — present (excluded from default run; requires external `dcbor` CLI tool)

## Build & Config
- [x] .gitignore
- [x] package.json (`@bc/dcbor` v0.25.1)
- [x] tsconfig.json (ES2022, strict, declaration output)
- [x] vitest.config.ts (excludes CLI integration test by default)
- [x] npm install lockfile (`package-lock.json`)

## Verification
- [x] `npm run build` passes
- [x] `npm test` passes (211/211 tests across 6 files)
- [x] Expected-text output rubric applied in formatting/traversal parity tests (full expected strings)

## Stage Gates
- [x] Stage 1 manifest completed
- [x] Stage 2 code translated and tests passing
- [x] Stage 3 completeness checker passed (no unchecked required items)
- [x] Stage 4 fluency critique completed with fixes and green tests
- [x] Public API doc comments added across exported surface

# Completeness: bc-ur → TypeScript (@bc/ur)

## Source Files
- [x] src/index.ts — package exports and docs
- [x] src/error.ts — error types and result helpers
- [x] src/ur-type.ts — UR type validation
- [x] src/utils.ts — UR type character/string predicates
- [x] src/ur.ts — single-part UR wrapper
- [x] src/bytewords.ts — bytewords wrappers and constants
- [x] src/ur-encodable.ts — UR encoding trait-equivalent helpers
- [x] src/ur-decodable.ts — UR decoding trait-equivalent helpers
- [x] src/ur-codable.ts — marker trait-equivalent
- [x] src/multipart-encoder.ts — fountain multipart encoder wrapper
- [x] src/multipart-decoder.ts — fountain multipart decoder wrapper
- [x] src/internal/fountain.ts — Rust-faithful xoshiro/weighted sampler/fountain internals
- [x] src/prelude.ts — common re-exports

## Tests
- [x] tests/ur.test.ts — UR constructors, parsing, serialization
- [x] tests/bytewords.test.ts — bytemoji uniqueness/length + encode/decode helpers
- [x] tests/ur-codable.test.ts — URCodable round-trip helper behavior
- [x] tests/examples.test.ts — README examples and multipart fountain decode

## Build & Config
- [x] .gitignore
- [x] package.json
- [x] tsconfig.json
- [x] vitest.config.ts

## Verification
- [x] `npm run build` passes
- [x] `npm test` passes (10/10 tests)
- [x] Rust vector parity checks pass (`ur:test/lsadaoaxjygonesw`, `ur:leaf/iejyihjkjygupyltla`, fountain indexes 5/61/110/507)

## Stage Gates
- [x] Stage 1 manifest completed
- [x] Stage 2 code translated and tests passing
- [x] Stage 3 completeness checker passed (no unchecked required items)
- [x] Stage 4 fluency review completed with green tests
- [x] Stage 5 status/log updates completed
- [x] Stage 6 lessons captured in memory files

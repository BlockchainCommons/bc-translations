# Completeness: bc-lifehash -> TypeScript (@bc/lifehash)

## Public API
- [x] `Version` enum
- [x] `Image` interface
- [x] `makeFromUtf8(s, version, moduleSize, hasAlpha)`
- [x] `makeFromData(data, version, moduleSize, hasAlpha)`
- [x] `makeFromDigest(digest, version, moduleSize, hasAlpha)`

## Source Files
- [x] `src/bit-enumerator.ts` - bit stream reader/writer primitives
- [x] `src/grid.ts` - toroidal grid storage and neighborhood traversal
- [x] `src/change-grid.ts` - changed-cell neighborhood tracking
- [x] `src/cell-grid.ts` - Conway generation stepping
- [x] `src/frac-grid.ts` - generation history accumulation
- [x] `src/color.ts` - color math and f32-compat numeric helpers
- [x] `src/hsb-color.ts` - HSB to RGB conversion
- [x] `src/color-func.ts` - gradient function composition
- [x] `src/patterns.ts` - symmetry pattern selection
- [x] `src/gradients.ts` - version-specific gradient selection
- [x] `src/color-grid.ts` - symmetry application and pixel color expansion
- [x] `src/lifehash.ts` - public image generation pipeline
- [x] `src/version.ts` - public version enum
- [x] `src/index.ts` - package documentation and public exports

## Tests
- [x] `tests/test-vectors.test.ts` - 35 exact vector parity cases plus vector count assertion
- [x] `tests/generate-pngs.test.ts` - 5 PNG generation tests matching Rust coverage

## Derives & Documentation
- [x] `Version` equality/value semantics map naturally to a string enum
- [x] Package-level documentation is present in `src/index.ts`
- [x] Public API documentation is present in `src/version.ts` and `src/lifehash.ts`

## Build & Config
- [x] `.gitignore`
- [x] `package.json`
- [x] `tsconfig.json`
- [x] `vitest.config.ts`

## Cross-Check Findings (2026-03-06)
- [x] Re-verified the manifest, Rust public API surface, and Rust test inventory against the TypeScript package with no completeness gaps.
- [x] Added a package-local `.gitignore` so generated `dist/`, `out/`, and dependency artifacts are ignored within the target package itself.

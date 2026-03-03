# Completeness: known-values → TypeScript (@bc/known-values)

## Source Files
- [x] known-value.ts — KnownValue class (core type)
- [x] known-values-store.ts — KnownValuesStore class (bidirectional lookup)
- [x] known-values-registry.ts — 104 constants + KNOWN_VALUES singleton
- [x] directory-loader.ts — JSON directory loading types and functions
- [x] config-state.ts — shared config lock state
- [x] index.ts — public API re-exports

## Tests
- [x] known-values-registry.test.ts — registry constant lookup (1 test)
- [x] directory-loader.test.ts — JSON parsing and config tests (8 tests)
- [x] directory-loading.test.ts — integration tests (13 tests)

## Build & Config
- [x] .gitignore
- [x] package.json
- [x] tsconfig.json
- [x] vitest.config.ts

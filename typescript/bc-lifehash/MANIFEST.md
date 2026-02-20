# Translation Manifest: bc-lifehash

Source: `rust/bc-lifehash/` v0.1.0
Target: `typescript/bc-lifehash/` package `@bc/lifehash`

## Cargo Summary

- Crate: `bc-lifehash`
- Version: `0.1.0`
- Rust edition: `2024`
- Description: `LifeHash visual hashing algorithm`
- Internal BC dependencies: none
- Feature flags: none (all code is default)

## External Dependencies

| Rust Crate | Purpose | TypeScript Equivalent | Notes |
|---|---|---|---|
| `sha2` | SHA-256 digest | `node:crypto` (createHash) | stdlib |
| `serde` (dev) | JSON deserialize | built-in `JSON.parse` | stdlib |
| `serde_json` (dev) | JSON parser | built-in `JSON.parse` | stdlib |
| `hex` (dev) | Hex decode in tests | `Buffer.from(hex, 'hex')` | stdlib |
| `png` (dev) | PNG generation | `pngjs` | dev dependency for test |

## Public API Catalog

### TYPE CATALOG

- name: `Version`
  - kind: enum
  - variants: `Version1`, `Version2`, `Detailed`, `Fiducial`, `GrayscaleFiducial`

- name: `Image`
  - kind: interface
  - fields:
    - `width: number`
    - `height: number`
    - `colors: Uint8Array`

### FUNCTION CATALOG

- name: `makeFromUtf8`
  - signature: `(s: string, version: Version, moduleSize: number, hasAlpha: boolean) => Image`

- name: `makeFromData`
  - signature: `(data: Uint8Array, version: Version, moduleSize: number, hasAlpha: boolean) => Image`

- name: `makeFromDigest`
  - signature: `(digest: Uint8Array, version: Version, moduleSize: number, hasAlpha: boolean) => Image`

### CONSTANT CATALOG

- none

### TRAIT CATALOG

- none

## Internal Module Inventory

- `grid.ts`: generic toroidal grid storage and neighborhood traversal
- `bit-enumerator.ts`: bit stream reader/writer (BitEnumerator, BitAggregator)
- `change-grid.ts`: mark changed cell neighborhoods
- `cell-grid.ts`: Conway generation step with change-grid optimization
- `frac-grid.ts`: overlays generation history fractions
- `color.ts`: color math + C++-compatible numeric helpers (modulo, luminance)
- `hsb-color.ts`: HSB/HSV to RGB conversion
- `color-func.ts`: gradient function composition
- `patterns.ts`: symmetry pattern selection from entropy
- `gradients.ts`: gradient family generation from entropy and version
- `color-grid.ts`: applies gradients and symmetry transforms to FracGrid
- `lifehash.ts`: orchestration pipeline and hashing

## Test Inventory

### Unit/Integration Tests

1. `test_all_vectors`
- Location: `tests/test-vectors.test.ts`
- Behavior: loads `test-vectors.json` and validates all generated pixels exactly
- Coverage: public API via `makeFromUtf8` and `makeFromData`
- Vector criticality: high; exact byte parity required
- Expected vector count: 35

2. `generate_pngs`
- Location: `tests/generate-pngs.test.ts`
- Behavior: generates PNG files for each version and input `0..99`
- Coverage: smoke/visual utility
- Translation note: included as a standard (not ignored) test

### Test Assets

- `tests/test-vectors.json` (~808 KB)
- Symlinked from `rust/bc-lifehash/tests/test-vectors.json`

## Translation Unit Order

1. Core primitives: Grid, BitEnumerator/BitAggregator, Color + numeric helpers
2. Life simulation support: ChangeGrid, CellGrid, FracGrid
3. Color system: HSBColor, color function combinators, patterns, gradients, ColorGrid
4. Public surface and orchestration: Version, Image, makeFromUtf8/Data/Digest
5. Tests: vector parity test, PNG generation

## Translation Hazards

1. **f32 precision emulation** — Use `Math.fround()` for f32 casts in modulo, HSB sector floor, and luminance.
2. **Toroidal indexing** — Use `((i % m) + m) % m` for safe modular wrapping with negative offsets.
3. **Bit stream edge behavior** — BitEnumerator mask/index transitions must match exactly.
4. **Hash history dedup** — Use a `Set<string>` of hex-encoded SHA-256 hashes.
5. **Version-specific entropy consumption** — Version1 skips one bit; Version2 skips one uint2.
6. **Normalization path exclusion** — FracGrid normalization skipped only for Version1.
7. **Image scaling loop orientation** — Preserve C++ swapped loop order.
8. **Module size and digest guards** — Throw `RangeError` for invalid preconditions.

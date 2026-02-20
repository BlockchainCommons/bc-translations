# Translation Manifest: bc-lifehash

Source: `rust/bc-lifehash/` v0.1.0
Target: `kotlin/bc-lifehash/` package `com.blockchaincommons.bclifehash`

## Cargo Summary

- Crate: `bc-lifehash`
- Version: `0.1.0`
- Rust edition: `2024`
- Description: `LifeHash visual hashing algorithm`
- Internal BC dependencies: none
- Feature flags: none (all code is default)

## External Dependencies

| Rust Crate | Purpose | Kotlin Equivalent | Notes |
|---|---|---|---|
| `sha2` | SHA-256 digest | `java.security.MessageDigest` | Use `MessageDigest.getInstance("SHA-256")` |
| `serde` (dev) | JSON deserialize | `com.fasterxml.jackson.module:jackson-module-kotlin` (test) | For test vector model parsing |
| `serde_json` (dev) | JSON parser | Jackson `ObjectMapper` (test) | Parse `test-vectors.json` |
| `hex` (dev) | Hex decode in tests | small local hex decoder in tests | Keep dependency surface minimal |
| `png` (dev) | PNG generation (ignored test) | `javax.imageio.ImageIO` + `BufferedImage` (optional test helper) | Ignored utility test only |

## Public API Catalog

### TYPE CATALOG

- name: `Version`
  - kind: enum
  - variants: `Version1`, `Version2`, `Detailed`, `Fiducial`, `GrayscaleFiducial`
  - derives: `Clone`, `Copy`, `Debug`, `PartialEq`, `Eq`

- name: `Image`
  - kind: struct
  - fields:
    - `width: usize`
    - `height: usize`
    - `colors: Vec<u8>`

### FUNCTION CATALOG

- name: `make_from_utf8`
  - signature: `fn make_from_utf8(s: &str, version: Version, module_size: usize, has_alpha: bool) -> Image`
  - is_method: no

- name: `make_from_data`
  - signature: `fn make_from_data(data: &[u8], version: Version, module_size: usize, has_alpha: bool) -> Image`
  - is_method: no

- name: `make_from_digest`
  - signature: `fn make_from_digest(digest: &[u8], version: Version, module_size: usize, has_alpha: bool) -> Image`
  - is_method: no

### CONSTANT CATALOG

- none

### TRAIT CATALOG

- none

## Internal Module Inventory (Implementation Required)

These are not public modules in Rust, but they are required for equivalent behavior.

- `grid.rs`: generic toroidal grid storage and neighborhood traversal
- `bit_enumerator.rs`: bit stream reader/writer (`BitEnumerator`, `BitAggregator`)
- `change_grid.rs`: mark changed cell neighborhoods
- `cell_grid.rs`: Conway generation step with change-grid optimization
- `frac_grid.rs`: overlays generation history fractions
- `color.rs`: color math + C++-compatible numeric helpers (`modulo`, luminance)
- `hsb_color.rs`: HSB/HSV to RGB conversion
- `color_func.rs`: gradient function composition
- `patterns.rs`: symmetry pattern selection from entropy
- `gradients.rs`: gradient family generation from entropy and version
- `color_grid.rs`: applies gradients and symmetry transforms to `FracGrid`
- `lib.rs`: orchestration pipeline and hashing

## Documentation Catalog

- Crate-level docs: present (`//!` intro, versions, and usage example)
- Module-level docs: none in `src/*.rs`
- Public items with doc comments: none (`Version`, `Image`, public functions have no direct doc comments)
- Public items without doc comments: `Version`, `Image`, `make_from_utf8`, `make_from_data`, `make_from_digest`
- Package metadata description: present in Cargo.toml (`LifeHash visual hashing algorithm`)
- README: present (`rust/bc-lifehash/README.md`) with algorithm background and examples

## Test Inventory

### Unit/Integration Tests

1. `test_all_vectors`
- Location: `rust/bc-lifehash/tests/test_vectors.rs`
- Behavior: loads `tests/test-vectors.json` and validates all generated pixels exactly
- Coverage: public API via `make_from_utf8` and `make_from_data`
- Vector criticality: high; exact byte parity required
- Expected vector count: 35

2. `generate_pngs` (`#[ignore]`)
- Location: `rust/bc-lifehash/tests/generate_pngs.rs`
- Behavior: generates PNG files for each version and input `0..99`
- Coverage: smoke/visual utility; not run in normal test pass
- Translation note: optional/ignored helper test

### Test Assets

- `rust/bc-lifehash/tests/test-vectors.json` (~808 KB)
- Must be copied into Kotlin test resources and consumed byte-for-byte

## Translation Unit Order

1. Core primitives:
- `Grid<T>`
- `BitEnumerator` / `BitAggregator`
- `Color` + numeric helper functions

2. Life simulation support:
- `ChangeGrid`
- `CellGrid`
- `FracGrid`

3. Color system:
- `HSBColor`
- color function combinators (`blend`, `reverse`)
- pattern selection
- gradient selection
- `ColorGrid`

4. Public surface and orchestration:
- `Version`
- `Image`
- `makeFromUtf8`, `makeFromData`, `makeFromDigest`

5. Tests:
- vector parity test (`test_all_vectors`)
- ignored PNG generation helper (`generate_pngs`)

## Translation Hazards

1. **C++ precision emulation is intentional**
- Rust intentionally uses f32 behavior for `modulo`, HSB sector floor, and luminance (`powi` + `sqrt`).
- Kotlin must preserve this precision behavior (`Float` intermediates) to maintain test-vector parity.

2. **Toroidal indexing with negative offsets**
- Neighborhood wraps around grid bounds.
- `%` semantics differ for negatives; use safe modular wrapping: `((i % m) + m) % m`.

3. **Bit stream edge behavior**
- `BitEnumerator.has_next()` and `next()` logic depends on mask/index transitions; off-by-one errors will cascade into all outputs.

4. **Hash history dedup semantics**
- Rust uses `BTreeSet<Vec<u8>>` of SHA-256 hashes of each generation state.
- Kotlin should store digest bytes in a deterministic comparable container (e.g., wrapped `ByteArray` key object with content-based equality/hash).

5. **Version-specific entropy consumption**
- `Version1` consumes one extra bit; `Version2` consumes one extra `next_uint2`; others do not.
- Any mismatch shifts all downstream random choices and breaks vectors.

6. **Normalization path exclusion**
- Frac-grid normalization is skipped only for `Version1`.
- Kotlin must preserve this branch exactly.

7. **Image scaling loop orientation**
- Rust preserves a historically swapped loop orientation from C++ (safe because outputs are square).
- Keep loop order and indexing consistent for parity.

8. **Module size and digest guards**
- Rust asserts `module_size > 0` and digest length is exactly 32 bytes.
- Kotlin should enforce equivalent preconditions and exception behavior.

## Default-Feature Scope

- Full crate is translated in this initial pass (no optional features exist).
- No deferred feature-gated work required.

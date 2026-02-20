# Translation Manifest: bc-lifehash

- **Crate:** `bc-lifehash`
- **Version:** `0.1.0`
- **Rust Edition:** `2024`
- **Target:** `swift/BCLifeHash`
- **Package Description:** `LifeHash visual hashing algorithm`
- **Internal BC Dependencies:** none
- **Feature Flags:** none

## External Dependency Equivalents

| Rust crate | Purpose | Swift equivalent |
|---|---|---|
| `sha2` | SHA-256 | `CryptoKit.SHA256` (stdlib framework) |

Test-only equivalents:

| Rust crate | Purpose | Swift equivalent |
|---|---|---|
| `serde` + `serde_json` | JSON parsing | `Foundation` + `JSONDecoder` |
| `hex` | Hex decoding vectors | small local test helper |
| `png` (`#[ignore]` test) | PNG generation helper | optional skipped XCTest |

## Public API Catalog

### TYPE CATALOG

- `Version`
  - kind: enum
  - variants: `Version1`, `Version2`, `Detailed`, `Fiducial`, `GrayscaleFiducial`
  - derives: `Clone`, `Copy`, `Debug`, `PartialEq`, `Eq`

- `Image`
  - kind: struct
  - fields:
    - `width: usize`
    - `height: usize`
    - `colors: Vec<u8>`

### FUNCTION CATALOG

- `make_from_utf8(s: &str, version: Version, module_size: usize, has_alpha: bool) -> Image`
- `make_from_data(data: &[u8], version: Version, module_size: usize, has_alpha: bool) -> Image`
- `make_from_digest(digest: &[u8], version: Version, module_size: usize, has_alpha: bool) -> Image`

### CONSTANT CATALOG

- none (public)

### TRAIT CATALOG

- none (public)

## Internal Module Inventory (Required for Semantic Parity)

- `grid.rs`: generic toroidal 2D grid
- `bit_enumerator.rs`: bit reader/writer (`BitEnumerator`, `BitAggregator`)
- `change_grid.rs`: neighborhood change tracker
- `cell_grid.rs`: Conway evolution step
- `frac_grid.rs`: accumulates per-cell generation fraction
- `color.rs`: RGB model + clamped/modulo/lerp helpers + luminance
- `hsb_color.rs`: HSB → RGB conversion
- `color_func.rs`: gradient function composition (`reverse`, `blend2`, `blend`)
- `patterns.rs`: pattern selection (`Snowflake`, `Pinwheel`, `Fiducial`)
- `gradients.rs`: deterministic gradient construction from entropy
- `color_grid.rs`: applies pattern transforms and gradients
- `lib.rs`: orchestration pipeline and public entry points

## Documentation Catalog

- Crate-level docs: present (`//!` introduction, versions, usage)
- Module-level docs: none
- Public items with doc comments: none (`Version`, `Image`, and public functions have no direct docs)
- Notable internal doc comment: `Color::luminance` notes f32 precision requirement
- Cargo description: present (`LifeHash visual hashing algorithm`)
- README: present (`rust/bc-lifehash/README.md`)

## Test Inventory

### Integration tests

1. `test_all_vectors`
- file: `rust/bc-lifehash/tests/test_vectors.rs`
- coverage: public API behavior across all versions/input types
- vectors: **35** entries from `test-vectors.json` (byte-identical output required)
- includes: UTF-8 and hex inputs, module size variants, alpha/no-alpha variants

2. `generate_pngs` (`#[ignore]`)
- file: `rust/bc-lifehash/tests/generate_pngs.rs`
- purpose: utility visual PNG generation for manual inspection
- translation requirement: represent as skipped test for parity

### Test assets

- `rust/bc-lifehash/tests/test-vectors.json` (~808 KB)
- must be copied into Swift test resources and parsed as raw numeric arrays

## Translation Unit Order

1. Numeric/color helpers (`clamped`, `modulo`, lerp helpers)
2. `Color`
3. `HSBColor`
4. `Grid<T>`
5. `BitEnumerator` + `BitAggregator`
6. `ChangeGrid`
7. `CellGrid`
8. `FracGrid`
9. Color function combinators (`ColorFunc`, `reverse`, `blend2`, `blend`)
10. `Pattern` + `selectPattern`
11. `Gradients`
12. `ColorGrid`
13. Public API (`Version`, `Image`, `sha256`, `makeImage`, `makeFrom*`)
14. Tests + vector resource handling

## Translation Hazards

1. **f32 precision emulation is required (critical)**
- Rust intentionally casts to `f32` for C++ compatibility (`modulo`, `HSBColor.color`, `Color.luminance`).
- Swift must mirror via `Float` intermediates and convert back to `Double`.

2. **Modulo semantics with negative values**
- Toroidal wrapping and hue modulo depend on positive modulo normalization.
- Use normalized modulo `((a % m) + m) % m` equivalents.

3. **Bit enumerator state transitions are fragile**
- `mask == 0` branch and `has_next` semantics must match exactly.
- Off-by-one errors will shift entropy consumption and break all vectors.

4. **History dedup uses content equality on 32-byte digests**
- Rust uses `BTreeSet<Vec<u8>>`.
- Swift should store digest bytes in a `Set<Data>` or equivalent content-hashable container.

5. **Version-dependent entropy consumption**
- `Detailed` consumes one extra bit (`next()`)
- `Version2` consumes one extra `next_uint2()`
- Others consume none

6. **Normalization branch excludes Version1**
- Frac-grid normalization to `[0,1]` is intentionally skipped only for `Version1`.

7. **Image scaling loop orientation must be preserved**
- Rust intentionally keeps C++ swapped outer/inner loop orientation in `make_image`.
- Keep loop/indexing behavior identical for byte parity.

8. **Public preconditions should be explicit**
- `module_size > 0`
- `digest.count == 32`
- Prefer Swift `precondition` to mirror Rust `assert!` contract behavior.

## Default Feature Scope

- Full crate is default feature set (no conditional compilation)
- No deferred non-default feature work

# Translation Manifest: bc-lifehash

- **Crate:** bc-lifehash
- **Version:** 0.1.0
- **Description:** LifeHash visual hashing algorithm
- **Internal BC Dependencies:** none
- **External Dependencies:** sha2 (SHA-256)
- **Feature Flags:** none

## External Dependency Equivalents

| Rust crate | Purpose | C# equivalent |
|------------|---------|---------------|
| `sha2` | SHA-256 hashing | `System.Security.Cryptography.SHA256` (stdlib) |

Dev-dependencies (test-only):
| Rust crate | Purpose | C# equivalent |
|------------|---------|---------------|
| `serde` / `serde_json` | JSON test vector parsing | `System.Text.Json` (stdlib) |
| `hex` | Hex decoding in tests | Manual `Convert.FromHexString()` (.NET 5+) |
| `png` | PNG generation (ignored test) | N/A (skip — test is `#[ignore]`) |

## Type Catalog

### Public Types

```
- name: Version
  kind: enum
  variants: [Version1, Version2, Detailed, Fiducial, GrayscaleFiducial]
  derives: [Clone, Copy, Debug, PartialEq, Eq]

- name: Image
  kind: struct
  fields:
    - width: usize (pub)
    - height: usize (pub)
    - colors: Vec<u8> (pub)
```

### Internal Types (translate but keep internal)

```
- name: Grid<T>
  kind: generic struct
  file: grid.rs
  fields: width (usize), height (usize), storage (Vec<T>)
  constraint: T: Clone + Default
  methods: new, set_all, set_value, get_value, for_all, for_neighborhood
  private: offset, circular_index

- name: BitEnumerator
  kind: struct
  file: bit_enumerator.rs
  fields: data (Vec<u8>), index (usize), mask (u8)
  methods: new, has_next, next, next_uint2, next_uint8, next_uint16, next_frac, for_all

- name: BitAggregator
  kind: struct
  file: bit_enumerator.rs
  fields: data (Vec<u8>), bit_mask (u8)
  methods: new, append, data

- name: CellGrid
  kind: struct (wraps Grid<bool>)
  file: cell_grid.rs
  methods: new, data, set_data, next_generation
  private: is_alive_in_next_generation, count_neighbors

- name: ChangeGrid
  kind: struct (wraps Grid<bool>)
  file: change_grid.rs
  methods: new, set_changed

- name: FracGrid
  kind: struct (wraps Grid<f64>)
  file: frac_grid.rs
  methods: new, overlay

- name: Color
  kind: struct
  file: color.rs
  fields: r (f64), g (f64), b (f64)
  derives: [Clone, Copy, Debug]
  implements: Default (→ BLACK)
  constants: WHITE, BLACK, RED, GREEN, BLUE, CYAN, MAGENTA, YELLOW
  methods: new, from_uint8_values, lerp_to, lighten, darken, burn, luminance

- name: HSBColor
  kind: struct
  file: hsb_color.rs
  fields: hue (f64), saturation (f64), brightness (f64)
  methods: new, from_hue, color (→ Color)

- name: ColorFunc
  kind: type alias → Box<dyn Fn(f64) -> Color>
  file: color_func.rs
  note: In C# use Func<double, Color> or delegate

- name: Pattern
  kind: enum
  file: patterns.rs
  variants: [Snowflake, Pinwheel, Fiducial]
  derives: [Clone, Copy, PartialEq, Eq]

- name: Transform
  kind: struct (private, in color_grid.rs)
  fields: transpose (bool), reflect_x (bool), reflect_y (bool)

- name: ColorGrid
  kind: struct (wraps Grid<Color>)
  file: color_grid.rs
  methods: new, colors
```

## Function Catalog

### Public Functions

```
- name: make_from_utf8
  signature: fn(s: &str, version: Version, module_size: usize, has_alpha: bool) -> Image
  file: lib.rs

- name: make_from_data
  signature: fn(data: &[u8], version: Version, module_size: usize, has_alpha: bool) -> Image
  file: lib.rs

- name: make_from_digest
  signature: fn(digest: &[u8], version: Version, module_size: usize, has_alpha: bool) -> Image
  file: lib.rs
  precondition: digest.len() == 32
```

### Internal Free Functions

```
- name: sha256
  signature: fn(data: &[u8]) -> Vec<u8>
  file: lib.rs

- name: make_image
  signature: fn(width, height, float_colors, module_size, has_alpha) -> Image
  file: lib.rs

- name: clamped
  signature: fn(n: f64) -> f64
  file: color.rs

- name: modulo
  signature: fn(dividend: f64, divisor: f64) -> f64
  file: color.rs
  HAZARD: Uses f32 intermediate precision (fmodf emulation for C++ compat)

- name: lerp_to
  signature: fn(to_a: f64, to_b: f64, t: f64) -> f64
  file: color.rs

- name: lerp_from
  signature: fn(from_a: f64, from_b: f64, t: f64) -> f64
  file: color.rs

- name: lerp
  signature: fn(from_a: f64, from_b: f64, to_c: f64, to_d: f64, t: f64) -> f64
  file: color.rs

- name: reverse (ColorFunc)
  signature: fn(c: ColorFunc) -> ColorFunc
  file: color_func.rs

- name: blend2
  signature: fn(color1: Color, color2: Color) -> ColorFunc
  file: color_func.rs

- name: blend
  signature: fn(colors: Vec<Color>) -> ColorFunc
  file: color_func.rs

- name: select_gradient
  signature: fn(entropy: &mut BitEnumerator, version: Version) -> ColorFunc
  file: gradients.rs

- name: select_pattern
  signature: fn(entropy: &mut BitEnumerator, version: Version) -> Pattern
  file: patterns.rs
```

### Internal Gradient Functions (all in gradients.rs)

```
- grayscale, select_grayscale, make_hue, spectrum, spectrum_cmyk_safe
- adjust_for_luminance
- monochromatic, monochromatic_fiducial
- complementary, complementary_fiducial
- triadic, triadic_fiducial
- analogous, analogous_fiducial
```

## Documentation Catalog

```
- Crate-level doc comment: yes (detailed description of LifeHash algorithm)
- Module-level doc comments: none
- Public items with doc comments: Color.luminance has doc comment
- Public items WITHOUT doc comments: Version, Image, make_from_utf8, make_from_data, make_from_digest
- Package metadata description: "LifeHash visual hashing algorithm"
- README: no
```

## Feature Flags

None. No conditional compilation.

## Test Inventory

### Integration Tests

```
- name: test_all_vectors
  file: tests/test_vectors.rs
  tests: All public API (make_from_utf8, make_from_data, make_from_digest via hex inputs)
  test_vectors: YES — 35 vectors from test-vectors.json
  vector_coverage:
    - version2 utf8 inputs: 8 (empty, "Hello", "0", long string, "Blockchain Commons", "deadbeef", emoji, accented)
    - version2 module_size=2: 2
    - version2 has_alpha=true: 2
    - version1 utf8: 5
    - detailed utf8: 5
    - fiducial utf8: 5
    - grayscale_fiducial utf8: 5
    - hex inputs: 3 (empty, "00ff80", "deadbeef")
  format: JSON array of {input, input_type, version, module_size, has_alpha, width, height, colors}
  CRITICAL: Byte-identical color output required for cross-language validation

- name: generate_pngs
  file: tests/generate_pngs.rs
  IGNORED: #[ignore] — PNG visual test, translate as skip-marked test
  purpose: Generates 100 PNGs per version (500 total) for visual inspection
  dev_dep: png crate (Rust) — use target-language PNG library
```

### Inline Tests

None.

## Translation Unit Order

1. **Color math utilities** — `clamped`, `modulo`, `lerp_to`, `lerp_from`, `lerp` (free functions in color.rs)
2. **Color struct** — `Color` with constants, constructors, methods (`lerp_to`, `lighten`, `darken`, `burn`, `luminance`)
3. **HSBColor struct** — depends on Color, clamped, modulo
4. **Grid<T>** — generic grid data structure
5. **BitEnumerator / BitAggregator** — bit-level I/O
6. **CellGrid** — depends on Grid, BitEnumerator, BitAggregator, ChangeGrid
7. **ChangeGrid** — depends on Grid
8. **FracGrid** — depends on Grid, CellGrid
9. **ColorFunc** — type alias + `reverse`, `blend2`, `blend` functions
10. **Pattern enum + select_pattern** — depends on BitEnumerator, Version
11. **Gradients** — depends on BitEnumerator, Color, HSBColor, ColorFunc, Version
12. **ColorGrid** — depends on Grid, Color, ColorFunc, FracGrid, Pattern
13. **Version enum + Image struct** — public types
14. **Top-level functions** — `sha256`, `make_image`, `make_from_digest`, `make_from_data`, `make_from_utf8`
15. **Tests** — test_all_vectors using JSON test vectors

## Hazards

### H1: f32 Precision Emulation (CRITICAL)
The Rust code deliberately uses f32 intermediate precision in several places to match the C++ reference implementation:
- `modulo()` casts to f32 for fmodf emulation
- `HSBColor.color()` uses `(h as f32).floor()` for floorf emulation
- `Color.luminance()` uses f32 intermediates for sqrtf/powf emulation
**C# equivalent:** Cast to `float` for intermediate operations, then back to `double`. Use `MathF.Floor()`, `MathF.Sqrt()`, `MathF.Pow()` etc. for single-precision.

### H2: ColorFunc as Box<dyn Fn>
`ColorFunc = Box<dyn Fn(f64) -> Color>` uses dynamic dispatch closures. In C# use `Func<double, Color>` delegates with lambda expressions. This maps naturally.

### H3: Grid Generic Type
`Grid<T>` is generic over `T: Clone + Default`. C# generics with `where T : struct` or `new()` constraint. Since T is always bool, f64, or Color (all value types in C#), this works cleanly.

### H4: BTreeSet for History Dedup
`make_from_digest` uses `BTreeSet<Vec<u8>>` for cycle detection. In C# use `HashSet<string>` with hex-encoded data or a custom byte-array comparer. Alternatively, convert byte arrays to Base64 strings for set membership.

### H5: std::mem::swap
Used for double-buffering cell/change grids and for transpose in ColorGrid. C# has tuple swap: `(a, b) = (b, a)`.

### H6: Closure Captures in Gradient Functions
Gradient functions create closures that capture `Vec<Color>` by move. C# lambdas capture by reference but since the colors are value types (structs), this is fine — local variable capture works correctly.

### H7: Test Vectors JSON File
The 35 test vectors in `test-vectors.json` must be included in the C# test project. Use embedded resource or file copy. The colors arrays are large (up to 12,288 bytes each). `System.Text.Json` can deserialize directly.

### H8: assert vs ArgumentException
Rust uses `assert!` / `assert_eq!` for preconditions (digest length, module size). C# should use `ArgumentException` / `ArgumentOutOfRangeException` for public API, and `Debug.Assert` or direct checks for internal code.

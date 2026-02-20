# bc-lifehash → Go Translation Manifest

## Crate Overview

**bc-lifehash** (v0.1.0) implements the LifeHash visual hashing algorithm — a hash visualization method based on Conway's Game of Life that creates deterministic, unique, beautiful icons from input data.

The algorithm:
1. SHA-256 hashes input data to get a 256-bit digest
2. Uses the digest as a seed for Conway's Game of Life on a 16×16 or 32×32 grid
3. Runs the cellular automaton until the pattern stabilizes or repeats (up to 150 or 300 generations)
4. Compiles a grayscale history image from all generations
5. Applies deterministic symmetry (snowflake/pinwheel/fiducial) and color gradients based on entropy bits from the digest

## External Dependencies

| Rust crate | Go equivalent |
|------------|---------------|
| `sha2` (SHA-256) | `crypto/sha256` (stdlib) |
| `serde` + `serde_json` (dev) | `encoding/json` (stdlib) |
| `hex` (dev) | `encoding/hex` (stdlib) |
| `png` (dev, ignored) | Not needed — generate_pngs test is `#[ignore]` |

**Zero external dependencies.** All Go dependencies are stdlib.

## Internal BC Dependencies

None. bc-lifehash is a standalone crate.

## Feature Flags

None.

## Public API Surface

### Types

| Rust | Go | Notes |
|------|-----|-------|
| `enum Version { Version1, Version2, Detailed, Fiducial, GrayscaleFiducial }` | `type Version int` + iota constants | 5 variants |
| `struct Image { width: usize, height: usize, colors: Vec<u8> }` | `type Image struct { Width, Height int; Colors []byte }` | Public fields |

### Functions

| Rust | Go | Notes |
|------|-----|-------|
| `make_from_utf8(s, version, module_size, has_alpha) → Image` | `MakeFromUTF8(s string, version Version, moduleSize int, hasAlpha bool) Image` | UTF-8 string input |
| `make_from_data(data, version, module_size, has_alpha) → Image` | `MakeFromData(data []byte, version Version, moduleSize int, hasAlpha bool) Image` | Raw bytes input |
| `make_from_digest(digest, version, module_size, has_alpha) → Image` | `MakeFromDigest(digest []byte, version Version, moduleSize int, hasAlpha bool) Image` | Pre-hashed 32-byte digest |

## Internal Modules (Translation Units)

All internal — no Go exports beyond the public API above.

### Translation Order

1. **grid** — Generic 2D grid with toroidal (wrapping) neighborhood
2. **bitEnumerator** — Bit-level reader/writer for entropy extraction
3. **color** — RGB color type + math helpers (clamped, modulo, lerp variants)
4. **hsbColor** — HSB-to-RGB conversion
5. **colorFunc** — Gradient function type + blend/reverse combinators
6. **cellGrid** — Conway's Game of Life cell grid
7. **changeGrid** — Tracks which cells changed (optimization)
8. **fracGrid** — Fractional overlay grid for history accumulation
9. **patterns** — Snowflake/Pinwheel/Fiducial symmetry patterns
10. **gradients** — Color gradient selection (monochromatic, complementary, triadic, analogous, fiducial variants)
11. **colorGrid** — Applies gradient + symmetry to produce final color grid
12. **lifehash** — Top-level orchestration + public API

## Translation Hazards

### Critical: f32 Precision Matching

The Rust code deliberately uses f32 (float32) intermediate precision in several places to match the C++ reference implementation. These MUST be preserved in Go:

- **`modulo(dividend, divisor)`** — Casts to `float32` for fmod: `(float32(dividend) % float32(divisor) + float32(divisor)) % float32(divisor)`
- **`Color.luminance()`** — Casts RGB components to `float32`, uses `float32` powi(2) and sqrt
- **`HSBColor.color()`** — Uses `float32(h).Floor()` for the integer part of hue conversion

Without f32 precision matching, pixel values will differ from the reference.

### Grid Generics

Rust uses `Grid<T: Clone + Default>`. In Go 1.21 with generics, use `Grid[T any]` — Go's zero values naturally handle the Default requirement for `bool`, `float64`, and `Color`.

### ColorFunc Closures

Rust uses `Box<dyn Fn(f64) -> Color>`. Go equivalent: `type colorFunc func(float64) color`.

### BTreeSet for History Dedup

Rust uses `BTreeSet<Vec<u8>>` for loop detection. Go: use a `map[string]struct{}` with SHA-256 hash strings as keys.

### No Panic in Public API

Rust uses `assert!` for precondition violations. Go: use `panic()` for the same (consistent with bcrand pattern).

## Test Inventory

### test_vectors.rs — 35 test vectors

JSON-based test vectors covering all 5 versions with various inputs:
- 8 version2 UTF-8 inputs (empty, "Hello", "0", long text, emoji, multibyte)
- 2 version2 with moduleSize=2
- 2 version2 with hasAlpha=true
- 5 version1 UTF-8 inputs
- 5 detailed UTF-8 inputs
- 5 fiducial UTF-8 inputs
- 5 grayscale_fiducial UTF-8 inputs
- 3 hex inputs (empty, "00ff80", "deadbeef")

Each vector contains full pixel data (colors array). The test-vectors.json file (~800KB) must be embedded as a test fixture.

### generate_pngs.rs — Ignored

PNG generation test, marked `#[ignore]`. Not translated (no Go PNG dep needed for core functionality).

## Go Package Structure

```
go/bclifehash/
├── .gitignore
├── MANIFEST.md
├── LOG.md
├── go.mod
├── grid.go
├── bit_enumerator.go
├── color.go
├── hsb_color.go
├── color_func.go
├── cell_grid.go
├── change_grid.go
├── frac_grid.go
├── patterns.go
├── gradients.go
├── color_grid.go
├── lifehash.go          # public API
├── testdata/
│   └── test-vectors.json
└── lifehash_test.go
```

Module path: `github.com/nickel-blockchaincommons/bclifehash-go`
Package name: `bclifehash`

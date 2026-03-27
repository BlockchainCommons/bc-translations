# Translation Manifest: bc-envelope 0.43.0 → C#

## Crate Overview

Gordian Envelope for .NET. A hierarchical binary data format built on deterministic CBOR (dCBOR) with a Merkle-like digest tree. Supports selective disclosure via elision, encryption, and compression; digital signatures; SSKR social recovery; expression-based RPC; metadata attachments; typed edges; and inclusion proofs. All default features enabled.

Total source: ~16,000 lines (base ~5,100, format ~1,870, extensions ~5,000+).

## External Dependencies

| Rust Crate | C# Equivalent |
|---|---|
| bc-rand | `BlockchainCommons.BCRand` (sibling) |
| bc-crypto | `BlockchainCommons.BCCrypto` (sibling) |
| dcbor | `BlockchainCommons.DCbor` (sibling) |
| bc-ur | `BlockchainCommons.BCUR` (sibling) |
| bc-components | `BlockchainCommons.BCComponents` (sibling) |
| known-values | `BlockchainCommons.KnownValues` (sibling) |
| paste | Not needed (C# has no equivalent; use explicit constants) |
| hex | `Convert.ToHexString()` / `Convert.FromHexString()` (.NET 5+) |
| itertools | LINQ (System.Linq) |
| thiserror | `EnvelopeException` class hierarchy |
| bytes | `byte[]` / `ReadOnlySpan<byte>` |
| ssh-key | SSH support via BCComponents SSH helper |
| hex-literal (dev) | `Convert.FromHexString()` |
| lazy_static (dev) | `Lazy<T>` / static fields |
| indoc (dev) | C# raw string literals or multiline strings with `.TrimStart()` |

## Feature Mapping

All Rust features are default-enabled. In C#, all code is always compiled (no conditional compilation needed).

## Public API Surface

See Kotlin MANIFEST.md for complete API listing — the API surface is identical. Key C# naming conventions:

- Rust `snake_case` → C# `PascalCase` for methods and properties
- Rust `is_*` → C# `Is*` properties or methods
- Rust `try_*` → C# methods that throw exceptions
- Rust `Result<T>` → C# exceptions (`EnvelopeException`)
- Rust `Option<T>` → C# nullable `T?`
- Rust traits → C# interfaces with `I` prefix where avoiding collision

## Translation Units (Dependency Order)

### Unit 1: Error Types
- `base/error.rs` → `EnvelopeException.cs`
- Class hierarchy extending `Exception`

### Unit 2: Core Envelope Structure
- `base/envelope.rs` → `Envelope.cs` (partial class), `EnvelopeCase.cs`

### Unit 3: Assertion
- `base/assertion.rs` → `Assertion.cs`

### Unit 4: EnvelopeEncodable
- `base/envelope_encodable.rs` → `EnvelopeEncodable.cs` (extension methods)
- `base/envelope_decodable.rs` → `EnvelopeDecodable.cs`

### Unit 5-8: Leaf, Queries, Digest, CBOR — integrated into `Envelope.cs` partial classes

### Unit 9: Walk/Visitor
- `base/walk.rs` → `EdgeType.cs`, `EnvelopeWalk.cs`

### Unit 10: Elision
- `base/elide.rs` → `ObscureAction.cs`, `ObscureType.cs`, `EnvelopeElide.cs`

### Unit 11-12: Wrap, Assertions — integrated into `Envelope.cs`

### Unit 13: String Utils
- `string_utils.rs` → `StringUtils.cs`

### Unit 14-19: Format — `FormatContext.cs`, `EnvelopeNotation.cs`, `EnvelopeTreeFormat.cs`, etc.

### Unit 20-32: Extensions — separate extension method files

### Unit 33-35: Tests

## Translation Hazards

Same hazards as Kotlin manifest (H1-H16). C#-specific notes:
- H1: C# objects are reference types by default; immutable pattern via returning new instances
- H3: Use `typeof(T)` / pattern matching for type extraction
- H5: Extension methods instead of blanket impls
- H6: Explicit extension methods per type
- H8: `Lazy<T>`, `lock()`, or `ConcurrentDictionary` for thread safety

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: yes
- 18 of 21 test files use expected text output assertions
- Tree format, diagnostic notation, and Mermaid diagrams must match byte-for-byte
- Use multiline string comparisons in xUnit

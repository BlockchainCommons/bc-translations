---
name: rust-to-csharp
description: >-
  Reference for translating Rust crates to idiomatic C#. Covers type mappings,
  idiom translations, error handling, naming conventions, and project structure.
  Use when translating Rust code to C# or working in the csharp/ directory.
user-invocable: false
---

# Rust to C# Translation Guide

## API Evolution Policy (De Novo)

- This repository is de novo; there are no external consumers requiring backward compatibility.
- When correctness or fluency work changes an API, apply the new API directly.
- Never add deprecated aliases, compatibility wrappers, or transitional shims.
- If an API change breaks dependent targets in this monorepo, update those dependents in the same work stream and re-run tests.

## Naming Conventions

| Rust              | C#                  |
|-------------------|---------------------|
| `snake_case`      | `PascalCase` (methods, properties, classes) |
| `snake_case` vars | `camelCase` (locals, parameters) |
| `SCREAMING_SNAKE` | `PascalCase` (constants) |
| `mod my_mod`      | `namespace MyMod`   |
| `my_crate`        | `MyCrate` (assembly/package name) |
| `type_name`       | `TypeName`          |

## Type Mappings

| Rust           | C#                    | Notes                          |
|----------------|-----------------------|--------------------------------|
| `u8`           | `byte`                |                                |
| `u16`          | `ushort`              |                                |
| `u32`          | `uint`                |                                |
| `u64`          | `ulong`               |                                |
| `i8`           | `sbyte`               |                                |
| `i16`          | `short`               |                                |
| `i32`          | `int`                 |                                |
| `i64`          | `long`                |                                |
| `f32`          | `float`               |                                |
| `f64`          | `double`              |                                |
| `bool`         | `bool`                |                                |
| `char`         | `char`                | C# char is UTF-16, not Unicode scalar |
| `String`       | `string`              |                                |
| `&str`         | `string`              | No distinction needed          |
| `&[u8]`        | `ReadOnlySpan<byte>` or `byte[]` | Span for perf, array for storage |
| `Vec<T>`       | `List<T>` or `T[]`    | Array when fixed-size          |
| `Vec<u8>`      | `byte[]`              | Prefer byte array for binary data |
| `HashMap<K,V>` | `Dictionary<K,V>`     |                                |
| `HashSet<T>`   | `HashSet<T>`          |                                |
| `Option<T>`    | `T?`                  | Nullable reference/value types |
| `Result<T,E>`  | Throw exceptions or use a Result type | See error handling below |
| `Box<T>`       | `T` (reference type)  | GC handles heap allocation     |
| `Rc<T>`/`Arc<T>` | `T` (reference type) | GC handles shared ownership   |
| `()`           | `void`                |                                |
| `(A, B)`       | `(A, B)` (ValueTuple) |                                |
| `usize`        | `int` or `nint`       | Use `int` unless pointer-sized needed |

## Idiom Translations

### Ownership and Borrowing

C# uses garbage collection. Drop ownership/borrowing distinctions. Use `IDisposable` for types that hold unmanaged resources (crypto keys, native handles).

```csharp
// Rust: impl Drop for SecretKey { fn drop(&mut self) { zeroize... } }
public class SecretKey : IDisposable {
    public void Dispose() { /* zeroize buffer */ }
}
```

### Result<T, E> and Error Handling

Use exceptions for recoverable errors. Define a custom exception hierarchy per crate.

```csharp
// Rust: pub enum ShamirError { InvalidThreshold, ... }
public class ShamirException : Exception { ... }
public class InvalidThresholdException : ShamirException { ... }

// Rust: fn split(...) -> Result<Vec<Share>, ShamirError>
public static List<Share> Split(...) { // throws ShamirException
```

For Result types used as return values in tight loops or where exceptions are inappropriate, use a lightweight `Result<T>` record or return `bool` with `out` parameter.

### Option<T>

Use nullable types (`T?`). Enable nullable reference types project-wide (`<Nullable>enable</Nullable>`).

### Enums with Data (Algebraic Data Types)

Use abstract records with sealed inheritance:

```csharp
// Rust: enum CborValue { UInt(u64), NInt(i64), Bytes(Vec<u8>), Text(String), ... }
public abstract record CborValue {
    public sealed record UInt(ulong Value) : CborValue;
    public sealed record NInt(long Value) : CborValue;
    public sealed record Bytes(byte[] Value) : CborValue;
    public sealed record Text(string Value) : CborValue;
}
```

### Pattern Matching

Use C# switch expressions with pattern matching:

```csharp
// Rust: match value { CborValue::UInt(n) => ..., CborValue::Text(s) => ... }
var result = value switch {
    CborValue.UInt(var n) => ...,
    CborValue.Text(var s) => ...,
    _ => throw new InvalidOperationException()
};
```

### Traits

Use interfaces. Use default interface methods for provided methods. Use extension methods for utility trait methods.

```csharp
// Rust: trait CBOREncodable { fn cbor(&self) -> CBOR; }
public interface ICborEncodable {
    Cbor ToCbor();
}

// Rust: trait CBORDecodable { fn from_cbor(cbor: &CBOR) -> Result<Self, Error>; }
public interface ICborDecodable<T> {
    static abstract T FromCbor(Cbor cbor); // C# 11 static abstract
}
```

### Iterators and Closures

Use LINQ and `Func<>`/`Action<>` delegates:

```csharp
// Rust: items.iter().filter(|x| x.is_valid()).map(|x| x.value()).collect()
items.Where(x => x.IsValid).Select(x => x.Value).ToList();
```

### Derive Macros

| Rust derive     | C# equivalent                            |
|-----------------|------------------------------------------|
| `Clone`         | `ICloneable` or record copy              |
| `Debug`         | Override `ToString()`                    |
| `Display`       | Override `ToString()`                    |
| `PartialEq/Eq`  | `IEquatable<T>`, override `Equals`/`GetHashCode` |
| `PartialOrd/Ord` | `IComparable<T>`                        |
| `Hash`          | Override `GetHashCode()`                 |
| `Default`       | Parameterless constructor or `default`   |
| `Serialize`/`Deserialize` | `ICborEncodable`/`ICborDecodable` (project-specific) |

### Impl Blocks

Methods go directly in the class. Separate logical groups with `#region`.

### Visibility

| Rust            | C#         |
|-----------------|------------|
| `pub`           | `public`   |
| `pub(crate)`    | `internal` |
| (default)       | `private`  |
| `pub(super)`    | `protected` or `internal` |

## Project Structure

```
csharp/BCRand/
├── BCRand.csproj
├── src/
│   └── *.cs
└── BCRand.Tests/
    ├── BCRand.Tests.csproj
    └── *.cs
```

- Use xUnit for tests. Mirror Rust test names in PascalCase.
- Use `[Fact]` for unit tests, `[Theory]` with `[InlineData]` for parameterized tests.
- Target .NET 8+.

## Resources

- [C# Language Reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/)
- [.NET API Reference](https://learn.microsoft.com/en-us/dotnet/api/)
- [Rust Reference](https://doc.rust-lang.org/reference/)
- [System.Security.Cryptography](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography)

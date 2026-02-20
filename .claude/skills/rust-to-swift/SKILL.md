---
name: rust-to-swift
description: >-
  Reference for translating Rust crates to idiomatic Swift. Covers type mappings,
  idiom translations, error handling, naming conventions, and project structure.
  Use when translating Rust code to Swift or working in the swift/ directory.
user-invocable: false
---

# Rust to Swift Translation Guide

## Naming Conventions

| Rust              | Swift                      |
|-------------------|----------------------------|
| `snake_case` func | `camelCase` (functions, methods, properties) |
| `MyStruct`        | `MyStruct`                 |
| `SCREAMING_SNAKE` | `camelCase` (static let)   |
| `mod my_mod`      | No direct equivalent; use files or extensions |
| `my_crate`        | `MyCrate` (module/target name) |
| `new()`           | `init()`                   |
| `is_valid()`      | `isValid`  (computed property if no args) |
| `to_string()`     | `description` (via `CustomStringConvertible`) |

Swift uses argument labels: `func encode(value: CBOR, using encoder: Encoder)`.

## Type Mappings

| Rust           | Swift               | Notes                          |
|----------------|----------------------|--------------------------------|
| `u8`           | `UInt8`             |                                |
| `u16`          | `UInt16`            |                                |
| `u32`          | `UInt32`            |                                |
| `u64`          | `UInt64`            |                                |
| `i8`           | `Int8`              |                                |
| `i16`          | `Int16`             |                                |
| `i32`          | `Int32`             |                                |
| `i64`          | `Int64`             |                                |
| `f32`          | `Float`             |                                |
| `f64`          | `Double`            |                                |
| `bool`         | `Bool`              |                                |
| `char`         | `Character`         | Unicode scalar value           |
| `String`       | `String`            |                                |
| `&str`         | `String`            | Swift strings are value types  |
| `&[u8]`        | `Data` or `[UInt8]` | `Data` for binary blobs        |
| `Vec<T>`       | `[T]`               |                                |
| `Vec<u8>`      | `Data` or `[UInt8]` |                                |
| `HashMap<K,V>` | `[K: V]`            | Dictionary literal syntax      |
| `HashSet<T>`   | `Set<T>`            |                                |
| `Option<T>`    | `T?`                | Optionals                      |
| `Result<T,E>`  | `throws` or `Result<T, E>` | See error handling      |
| `Box<T>`       | `T` (class) or `T` (protocol existential) |             |
| `Rc<T>`/`Arc<T>` | class (ARC)       | Swift ARC is automatic         |
| `()`           | `Void`              |                                |
| `(A, B)`       | `(A, B)`            | Tuples                         |
| `usize`        | `Int`               | Swift convention uses Int for indices |

## Idiom Translations

### Ownership and Borrowing

Swift uses ARC (Automatic Reference Counting) for classes. Structs are value types (copied on assignment like Rust's `Copy` types). Map Rust structs with `Clone` to Swift structs. Map Rust types with shared ownership to Swift classes.

Use `inout` parameters where Rust uses `&mut`:

```swift
// Rust: fn update(&mut self, data: &[u8])
mutating func update(_ data: Data)   // on a struct
func update(_ data: Data)            // on a class (implicit &mut self)
```

### Result<T, E> and Error Handling

Use Swift's `throws` mechanism:

```swift
// Rust: pub enum ShamirError { InvalidThreshold, ... }
enum ShamirError: Error {
    case invalidThreshold
    case invalidShareCount
}

// Rust: fn split(...) -> Result<Vec<Share>, ShamirError>
func split(...) throws(ShamirError) -> [Share] {
```

Use typed throws (Swift 6+) when the error type matters to callers. Use untyped `throws` otherwise.

### Option<T>

Swift optionals map directly:

```swift
// Rust: value.map(|v| v.encode())
value.map { $0.encode() }

// Rust: value.unwrap_or(default)
value ?? defaultValue

// Rust: value.ok_or(Error::Missing)?
guard let value else { throw Error.missing }
```

### Enums with Data (Algebraic Data Types)

Swift enums with associated values are a direct match:

```swift
// Rust: enum CborValue { UInt(u64), NInt(i64), Bytes(Vec<u8>), Text(String), ... }
enum CborValue {
    case uint(UInt64)
    case nint(Int64)
    case bytes(Data)
    case text(String)
}
```

### Pattern Matching

Swift `switch` is exhaustive and supports pattern matching:

```swift
// Rust: match value { CborValue::UInt(n) => ..., CborValue::Text(s) => ... }
switch value {
case .uint(let n):
    ...
case .text(let s):
    ...
}
```

Also use `if case` / `guard case` for single-pattern matching:

```swift
// Rust: if let CborValue::Text(s) = value { ... }
if case .text(let s) = value { ... }
```

### Traits

Use protocols:

```swift
// Rust: trait CBOREncodable { fn cbor(&self) -> CBOR; }
protocol CBOREncodable {
    func toCBOR() -> CBOR
}

// Rust: trait CBORDecodable { fn from_cbor(cbor: &CBOR) -> Result<Self, Error>; }
protocol CBORDecodable {
    init(cbor: CBOR) throws
}
```

Use protocol extensions for default implementations (like Rust's provided methods):

```swift
extension CBOREncodable {
    var cborData: Data { toCBOR().encode() }
}
```

### Iterators and Closures

Swift's collection methods map closely to Rust iterators:

```swift
// Rust: items.iter().filter(|x| x.is_valid()).map(|x| x.value()).collect()
items.filter { $0.isValid }.map { $0.value }
```

| Rust             | Swift                |
|------------------|----------------------|
| `.iter()`        | (implicit)           |
| `.map(f)`        | `.map { }`           |
| `.filter(f)`     | `.filter { }`        |
| `.flat_map(f)`   | `.flatMap { }`       |
| `.collect()`     | `Array(...)` or implicit |
| `.enumerate()`   | `.enumerated()`      |
| `.zip()`         | `zip(a, b)`          |
| `.fold(init, f)` | `.reduce(init) { }` |
| `.any(f)`        | `.contains { }`      |
| `.all(f)`        | `.allSatisfy { }`    |
| `.find(f)`       | `.first { }`         |
| `.count()`       | `.count`             |

### Derive Macros

| Rust derive       | Swift equivalent                     |
|--------------------|--------------------------------------|
| `Clone`            | Struct (value type, auto-copies) or explicit `copy()` |
| `Debug`            | `CustomDebugStringConvertible`       |
| `Display`          | `CustomStringConvertible`            |
| `PartialEq/Eq`    | `Equatable`                          |
| `Hash`             | `Hashable`                           |
| `PartialOrd/Ord`   | `Comparable`                         |
| `Default`          | Static factory `default()` or default init |

### Visibility

| Rust            | Swift             |
|-----------------|-------------------|
| `pub`           | `public`          |
| `pub(crate)`    | `internal` (default) |
| (default)       | `private` or `fileprivate` |
| `pub(super)`    | `fileprivate`     |

## Project Structure

```
swift/BCRand/
├── Package.swift
├── Sources/BCRand/
│   └── *.swift
└── Tests/BCRandTests/
    └── *Tests.swift
```

- Swift Package Manager (`Package.swift`). Mirror Rust test names in camelCase.
- Use XCTest: `func testSplitValidThreshold() throws { }`.
- Minimum deployment: macOS 13+ / iOS 16+ (or as appropriate).

## Resources

- [The Swift Programming Language](https://docs.swift.org/swift-book/)
- [Swift Standard Library](https://developer.apple.com/documentation/swift/swift-standard-library)
- [Swift API Design Guidelines](https://www.swift.org/documentation/api-design-guidelines/)
- [Rust Reference](https://doc.rust-lang.org/reference/)
- [Apple CryptoKit](https://developer.apple.com/documentation/cryptokit)

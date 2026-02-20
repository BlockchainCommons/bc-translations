---
name: rust-to-go
description: >-
  Reference for translating Rust crates to idiomatic Go. Covers type mappings,
  idiom translations, error handling, naming conventions, and project structure.
  Use when translating Rust code to Go or working in the go/ directory.
user-invocable: false
---

# Rust to Go Translation Guide

## Naming Conventions

| Rust              | Go                        |
|-------------------|---------------------------|
| `snake_case`      | `PascalCase` (exported), `camelCase` (unexported) |
| `SCREAMING_SNAKE` | `PascalCase` (exported const) |
| `mod my_mod`      | `package mymod`           |
| `my_crate`        | `mymod` (package name, single lowercase word) |
| `MyStruct`        | `MyStruct`                |
| `my_method`       | `MyMethod` / `myMethod`   |
| Getter `value()`  | `Value()` (no `Get` prefix) |
| Setter `set_value()` | `SetValue()`           |
| `is_valid()`      | `IsValid()`               |

Acronyms are all-caps: `URL`, `ID`, `HTTP`, `CBOR`, `UR`.

## Type Mappings

| Rust           | Go                  | Notes                          |
|----------------|---------------------|--------------------------------|
| `u8`           | `byte` (`uint8`)    |                                |
| `u16`          | `uint16`            |                                |
| `u32`          | `uint32`            |                                |
| `u64`          | `uint64`            |                                |
| `i8`           | `int8`              |                                |
| `i16`          | `int16`             |                                |
| `i32`          | `int32`             |                                |
| `i64`          | `int64`             |                                |
| `f32`          | `float32`           |                                |
| `f64`          | `float64`           |                                |
| `bool`         | `bool`              |                                |
| `char`         | `rune`              |                                |
| `String`       | `string`            |                                |
| `&str`         | `string`            | Go strings are immutable       |
| `&[u8]`        | `[]byte`            |                                |
| `Vec<T>`       | `[]T`               |                                |
| `Vec<u8>`      | `[]byte`            |                                |
| `HashMap<K,V>` | `map[K]V`           |                                |
| `HashSet<T>`   | `map[T]struct{}`    |                                |
| `Option<T>`    | `*T` or `(T, bool)` | Pointer for reference types; comma-ok for value types |
| `Result<T,E>`  | `(T, error)`        |                                |
| `Box<T>`       | `*T`                |                                |
| `Rc<T>`/`Arc<T>` | `*T`              | GC handles it                  |
| `()`           | (no return)         |                                |
| `(A, B)`       | Return a struct or multiple values |                   |
| `usize`        | `int`               | Idiomatic Go uses `int` for indices |

## Idiom Translations

### Ownership and Borrowing

Go uses garbage collection. No ownership translation needed. Use `sync.Mutex` where Rust uses `Mutex<T>`. For crypto secrets that need zeroing, zero the byte slice manually in a `Clear()` method.

### Result<T, E> and Error Handling

Use Go's `(value, error)` return convention:

```go
// Rust: fn split(secret: &[u8], threshold: u8, count: u8) -> Result<Vec<Share>, ShamirError>
func Split(secret []byte, threshold, count uint8) ([]Share, error) {
    if threshold < 1 {
        return nil, ErrInvalidThreshold
    }
    ...
}
```

Define sentinel errors with `errors.New` or structured error types:

```go
var (
    ErrInvalidThreshold = errors.New("shamir: invalid threshold")
    ErrInvalidShareCount = errors.New("shamir: invalid share count")
)
```

### Option<T>

Use pointer types for "maybe absent" values. For value types in maps, use the comma-ok idiom:

```go
// Rust: fn get(&self, key: &str) -> Option<&Value>
func (m *Map) Get(key string) *Value { // nil means absent
```

### Enums with Data (Algebraic Data Types)

Go has no sum types. Use an interface with unexported marker method:

```go
// Rust: enum CborValue { UInt(u64), NInt(i64), Bytes([]byte), Text(string), ... }
type CborValue interface {
    cborValue() // unexported marker — seals the interface
}

type CborUInt struct{ Value uint64 }
type CborNInt struct{ Value int64 }
type CborBytes struct{ Value []byte }
type CborText struct{ Value string }

func (CborUInt) cborValue()  {}
func (CborNInt) cborValue()  {}
func (CborBytes) cborValue() {}
func (CborText) cborValue()  {}
```

### Pattern Matching

Use type switches:

```go
// Rust: match value { CborValue::UInt(n) => ..., CborValue::Text(s) => ... }
switch v := value.(type) {
case CborUInt:
    // use v.Value
case CborText:
    // use v.Value
default:
    return fmt.Errorf("unexpected CBOR type: %T", value)
}
```

### Traits

Use interfaces. Go interfaces are implicit (structural typing).

```go
// Rust: trait CBOREncodable { fn cbor(&self) -> CBOR; }
type CBOREncodable interface {
    CBOR() CBOR
}

// Rust: trait CBORDecodable { fn from_cbor(cbor: &CBOR) -> Result<Self, Error>; }
// Go doesn't have static interface methods — use a standalone function:
func DecodeCBOR[T any](cbor CBOR) (T, error) { ... }
// or per-type: func ShareFromCBOR(cbor CBOR) (Share, error)
```

### Iterators and Closures

Go uses `for range` loops. No direct iterator chain equivalent. Write explicit loops:

```go
// Rust: items.iter().filter(|x| x.is_valid()).map(|x| x.value()).collect()
var result []Value
for _, item := range items {
    if item.IsValid() {
        result = append(result, item.Value())
    }
}
```

### Derive Macros

| Rust derive       | Go equivalent                          |
|--------------------|----------------------------------------|
| `Clone`            | Not needed (value types copy); implement `Clone()` for deep copy |
| `Debug`            | `fmt.Stringer` (`String() string`) or `fmt.GoStringer` |
| `Display`          | `fmt.Stringer`                         |
| `PartialEq/Eq`    | `Equal(other T) bool` method           |
| `Hash`             | Not needed for map keys (built-in for comparable types) |
| `Default`          | Zero value, or `NewDefault()` constructor |

### Visibility

| Rust            | Go                   |
|-----------------|----------------------|
| `pub`           | `PascalCase` (exported) |
| `pub(crate)`    | `camelCase` (unexported) |
| (default)       | `camelCase` (unexported) |

## Project Structure

```
go/bcrand/
├── go.mod
├── rand.go
├── rand_test.go
└── ...
```

- One package per crate. Test files sit alongside source (`_test.go` suffix).
- Use `go test ./...` to run tests.
- Use `testify/assert` or standard `testing` package.
- Module path: `github.com/nickel-blockchaincommons/<name>-go`

## Resources

- [Effective Go](https://go.dev/doc/effective_go)
- [Go Standard Library](https://pkg.go.dev/std)
- [Go Code Review Comments](https://github.com/golang/go/wiki/CodeReviewComments)
- [Rust Reference](https://doc.rust-lang.org/reference/)
- [golang.org/x/crypto](https://pkg.go.dev/golang.org/x/crypto)

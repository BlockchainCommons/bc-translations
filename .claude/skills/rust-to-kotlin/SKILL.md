---
name: rust-to-kotlin
description: >-
  Reference for translating Rust crates to idiomatic Kotlin. Covers type mappings,
  idiom translations, error handling, naming conventions, and project structure.
  Use when translating Rust code to Kotlin or working in the kotlin/ directory.
user-invocable: false
---

# Rust to Kotlin Translation Guide

## Naming Conventions

| Rust              | Kotlin                    |
|-------------------|---------------------------|
| `snake_case`      | `camelCase` (functions, properties) |
| `MyStruct`        | `MyStruct` (class)        |
| `SCREAMING_SNAKE` | `SCREAMING_SNAKE` (companion const) or `PascalCase` (enum entries) |
| `mod my_mod`      | `package com.blockchaincommons.mymod` |
| `my_crate`        | `my-crate` (artifact), `com.blockchaincommons.mycrate` (package) |

## Type Mappings

| Rust           | Kotlin               | Notes                          |
|----------------|-----------------------|--------------------------------|
| `u8`           | `UByte`              | Or `Byte` if sign doesn't matter |
| `u16`          | `UShort`             |                                |
| `u32`          | `UInt`               |                                |
| `u64`          | `ULong`              |                                |
| `i8`           | `Byte`               |                                |
| `i16`          | `Short`              |                                |
| `i32`          | `Int`                |                                |
| `i64`          | `Long`               |                                |
| `f32`          | `Float`              |                                |
| `f64`          | `Double`             |                                |
| `bool`         | `Boolean`            |                                |
| `char`         | `Char`               |                                |
| `String`       | `String`             |                                |
| `&str`         | `String`             |                                |
| `&[u8]`        | `ByteArray`          |                                |
| `Vec<T>`       | `List<T>` / `MutableList<T>` |                         |
| `Vec<u8>`      | `ByteArray`          |                                |
| `HashMap<K,V>` | `Map<K,V>` / `MutableMap<K,V>` |                       |
| `HashSet<T>`   | `Set<T>` / `MutableSet<T>` |                            |
| `Option<T>`    | `T?`                 | Nullable types                 |
| `Result<T,E>`  | Throw exceptions or `Result<T>` | See error handling   |
| `Box<T>`       | `T` (reference type)  |                               |
| `Rc<T>`/`Arc<T>` | `T`                |                                |
| `()`           | `Unit`               |                                |
| `(A, B)`       | `Pair<A, B>`         | Or a data class                |
| `usize`        | `Int`                | Kotlin collections use Int indices |

## Idiom Translations

### Ownership and Borrowing

Kotlin uses garbage collection. No ownership translation needed. Use `Closeable`/`AutoCloseable` for resources that need cleanup (crypto keys).

```kotlin
// Rust: impl Drop for SecretKey { fn drop(&mut self) { zeroize... } }
class SecretKey(...) : AutoCloseable {
    override fun close() { /* zeroize buffer */ }
}
// Usage: secretKey.use { key -> ... }
```

### Result<T, E> and Error Handling

Prefer exceptions. Define a sealed exception hierarchy:

```kotlin
// Rust: pub enum ShamirError { InvalidThreshold, ... }
sealed class ShamirException(message: String) : Exception(message) {
    class InvalidThreshold : ShamirException("Invalid threshold")
    class InvalidShareCount : ShamirException("Invalid share count")
}

// Rust: fn split(...) -> Result<Vec<Share>, ShamirError>
fun split(...): List<Share> { // throws ShamirException
```

For functional-style error handling, use Kotlin's built-in `Result<T>` or `runCatching { }`.

### Option<T>

Use nullable types with safe-call operators:

```kotlin
// Rust: value.map(|v| v.encode())
value?.encode()

// Rust: value.unwrap_or(default)
value ?: default

// Rust: value.ok_or(Error::Missing)?
value ?: throw MissingValueException()
```

### Enums with Data (Algebraic Data Types)

Use sealed classes or sealed interfaces:

```kotlin
// Rust: enum CborValue { UInt(u64), NInt(i64), Bytes(Vec<u8>), Text(String), ... }
sealed interface CborValue {
    data class UInt(val value: ULong) : CborValue
    data class NInt(val value: Long) : CborValue
    data class Bytes(val value: ByteArray) : CborValue
    data class Text(val value: String) : CborValue
}
```

### Pattern Matching

Use `when` expressions (exhaustive on sealed types):

```kotlin
// Rust: match value { CborValue::UInt(n) => ..., CborValue::Text(s) => ... }
when (value) {
    is CborValue.UInt -> // use value.value
    is CborValue.NInt -> // use value.value
    is CborValue.Bytes -> // use value.value
    is CborValue.Text -> // use value.value
}
```

### Traits

Use interfaces. Kotlin supports default method implementations:

```kotlin
// Rust: trait CBOREncodable { fn cbor(&self) -> CBOR; }
interface CborEncodable {
    fun toCbor(): Cbor
}

// Rust: trait CBORDecodable { fn from_cbor(cbor: &CBOR) -> Result<Self, Error>; }
interface CborDecodable<T> {
    companion object // Use companion for factory
}
// Implement via companion extension or abstract factory
```

### Iterators and Closures

Kotlin has rich collection operations that map closely to Rust iterators:

```kotlin
// Rust: items.iter().filter(|x| x.is_valid()).map(|x| x.value()).collect()
items.filter { it.isValid }.map { it.value }
```

| Rust             | Kotlin             |
|------------------|--------------------|
| `.iter()`        | (implicit)         |
| `.map()`         | `.map { }`         |
| `.filter()`      | `.filter { }`      |
| `.flat_map()`    | `.flatMap { }`     |
| `.collect()`     | `.toList()` / `.toSet()` |
| `.enumerate()`   | `.withIndex()`     |
| `.zip()`         | `.zip()`           |
| `.fold()`        | `.fold()`          |
| `.any()`         | `.any { }`         |
| `.all()`         | `.all { }`         |
| `.find()`        | `.find { }`        |
| `.count()`       | `.count()`         |

### Derive Macros

| Rust derive       | Kotlin equivalent                     |
|--------------------|---------------------------------------|
| `Clone`            | `data class` (auto-generates `copy()`) |
| `Debug`            | `data class` (auto-generates `toString()`) |
| `Display`          | Override `toString()`                 |
| `PartialEq/Eq`    | `data class` (auto-generates `equals()`/`hashCode()`) |
| `Hash`             | `data class` (auto-generates `hashCode()`) |
| `Default`          | Default parameter values or factory function |

### Visibility

| Rust            | Kotlin      |
|-----------------|-------------|
| `pub`           | `public` (default) |
| `pub(crate)`    | `internal`  |
| (default)       | `private`   |
| `pub(super)`    | `protected` |

## Project Structure

```
kotlin/bc-rand/
├── build.gradle.kts
├── src/
│   ├── main/kotlin/com/blockchaincommons/bcrand/
│   │   └── *.kt
│   └── test/kotlin/com/blockchaincommons/bcrand/
│       └── *Test.kt
```

- Use Gradle with Kotlin DSL. Group: `com.blockchaincommons`.
- Use JUnit 5 / kotlin.test for tests. Mirror Rust test names in camelCase.
- Target JVM 17+. Consider Kotlin Multiplatform for JVM + Native.

## Resources

- [Kotlin Language Reference](https://kotlinlang.org/docs/reference/)
- [Kotlin Standard Library](https://kotlinlang.org/api/latest/jvm/stdlib/)
- [Kotlin Coding Conventions](https://kotlinlang.org/docs/coding-conventions.html)
- [Rust Reference](https://doc.rust-lang.org/reference/)

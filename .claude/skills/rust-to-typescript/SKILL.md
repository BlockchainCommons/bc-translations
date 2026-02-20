---
name: rust-to-typescript
description: >-
  Reference for translating Rust crates to idiomatic TypeScript. Covers type mappings,
  idiom translations, error handling, naming conventions, and project structure.
  Use when translating Rust code to TypeScript or working in the typescript/ directory.
user-invocable: false
---

# Rust to TypeScript Translation Guide

## Naming Conventions

| Rust              | TypeScript                 |
|-------------------|----------------------------|
| `snake_case`      | `camelCase` (functions, variables, methods) |
| `MyStruct`        | `MyStruct` (classes), `MyInterface` (interfaces) |
| `SCREAMING_SNAKE` | `SCREAMING_SNAKE` (constants) |
| `mod my_mod`      | `my-mod.ts` (file) or `my-mod/index.ts` (directory) |
| `my_crate`        | `@bc/name` (npm scoped package) |

## Type Mappings

| Rust           | TypeScript           | Notes                          |
|----------------|----------------------|--------------------------------|
| `u8`–`u32`    | `number`             |                                |
| `u64`          | `bigint`             | Number only safe to 2^53       |
| `i8`–`i32`    | `number`             |                                |
| `i64`          | `bigint`             |                                |
| `f32`/`f64`   | `number`             | Always 64-bit IEEE 754         |
| `bool`        | `boolean`            |                                |
| `char`        | `string`             |                                |
| `String`      | `string`             |                                |
| `&str`        | `string`             |                                |
| `&[u8]`       | `Uint8Array`         |                                |
| `Vec<T>`      | `T[]`                |                                |
| `Vec<u8>`     | `Uint8Array`         |                                |
| `HashMap<K,V>`| `Map<K, V>`          |                                |
| `HashSet<T>`  | `Set<T>`             |                                |
| `Option<T>`   | `T \| undefined`     | Or `T \| null` depending on context |
| `Result<T,E>` | Throw errors or return discriminated union | See below |
| `Box<T>`      | `T`                  |                                |
| `Rc<T>`/`Arc<T>` | `T`               |                                |
| `()`          | `void`               |                                |
| `(A, B)`      | `[A, B]`             | Tuple types                    |
| `usize`       | `number`             |                                |

**Important:** For `u64`/`i64` values, decide per-crate whether to use `bigint` (exact) or `number` (lossy above 2^53). Crypto and tag values should use `bigint`. Counts and indices can use `number`.

## Idiom Translations

### Ownership and Borrowing

TypeScript uses garbage collection. No ownership needed. For crypto secrets, provide an explicit `destroy()` or `zeroize()` method using `Disposable`:

```typescript
// Rust: impl Drop for SecretKey { fn drop(&mut self) { zeroize... } }
class SecretKey implements Disposable {
    [Symbol.dispose](): void { /* zeroize buffer */ }
}
// Usage: using key = new SecretKey(...);
```

### Result<T, E> and Error Handling

Use exceptions with typed error classes:

```typescript
// Rust: pub enum ShamirError { InvalidThreshold, ... }
class ShamirError extends Error {
    constructor(message: string) { super(message); this.name = "ShamirError"; }
}
class InvalidThresholdError extends ShamirError {
    constructor() { super("Invalid threshold"); }
}

// Rust: fn split(...) -> Result<Vec<Share>, ShamirError>
function split(...): Share[] { // throws ShamirError
```

For functional-style code, use discriminated unions:

```typescript
type Result<T, E> =
    | { ok: true; value: T }
    | { ok: false; error: E };
```

### Option<T>

Use `T | undefined`. Use nullish coalescing and optional chaining:

```typescript
// Rust: value.map(|v| v.encode())
value?.encode()

// Rust: value.unwrap_or(default)
value ?? defaultValue

// Rust: value.ok_or(Error::Missing)?
if (value === undefined) throw new MissingError();
```

### Enums with Data (Algebraic Data Types)

Use discriminated unions:

```typescript
// Rust: enum CborValue { UInt(u64), NInt(i64), Bytes(Vec<u8>), Text(string), ... }
type CborValue =
    | { type: "uint"; value: bigint }
    | { type: "nint"; value: bigint }
    | { type: "bytes"; value: Uint8Array }
    | { type: "text"; value: string };
```

Or use classes with a shared discriminant when methods are needed:

```typescript
abstract class CborValue {
    abstract readonly type: string;
}
class CborUInt extends CborValue {
    readonly type = "uint" as const;
    constructor(readonly value: bigint) { super(); }
}
```

### Pattern Matching

Use narrowing with `switch` on discriminant:

```typescript
// Rust: match value { CborValue::UInt(n) => ..., CborValue::Text(s) => ... }
switch (value.type) {
    case "uint": /* value.value is bigint */ break;
    case "text": /* value.value is string */ break;
    default: {
        const _exhaustive: never = value;
        throw new Error(`Unexpected CBOR type`);
    }
}
```

### Traits

Use interfaces and standalone functions:

```typescript
// Rust: trait CBOREncodable { fn cbor(&self) -> CBOR; }
interface CborEncodable {
    toCbor(): Cbor;
}

// Rust: trait CBORDecodable { fn from_cbor(cbor: &CBOR) -> Result<Self, Error>; }
// TypeScript can't have static interface methods. Use standalone functions:
function shareFromCbor(cbor: Cbor): Share { ... }
```

### Iterators and Closures

Array methods map closely:

```typescript
// Rust: items.iter().filter(|x| x.is_valid()).map(|x| x.value()).collect()
items.filter(x => x.isValid).map(x => x.value);
```

| Rust             | TypeScript             |
|------------------|------------------------|
| `.iter()`        | (implicit on arrays)   |
| `.map(f)`        | `.map(f)`              |
| `.filter(f)`     | `.filter(f)`           |
| `.flat_map(f)`   | `.flatMap(f)`          |
| `.collect()`     | (already an array)     |
| `.enumerate()`   | `.entries()` or `.map((v, i) => ...)` |
| `.zip()`         | Manual: `a.map((v, i) => [v, b[i]])` |
| `.fold(init, f)` | `.reduce(f, init)`     |
| `.any(f)`        | `.some(f)`             |
| `.all(f)`        | `.every(f)`            |
| `.find(f)`       | `.find(f)`             |
| `.count()`       | `.length`              |

### Derive Macros

| Rust derive       | TypeScript equivalent               |
|--------------------|--------------------------------------|
| `Clone`            | Spread/`structuredClone()` or manual `clone()` |
| `Debug`            | `toString()` or `[Symbol.for("nodejs.util.inspect.custom")]` |
| `Display`          | `toString()`                         |
| `PartialEq/Eq`    | Manual `equals(other: T): boolean`   |
| `Hash`             | Manual `hashCode(): number` if needed |
| `Default`          | Static factory or default constructor |

### Visibility

| Rust            | TypeScript      |
|-----------------|-----------------|
| `pub`           | `export`        |
| `pub(crate)`    | No `export` (module-private) |
| (default)       | No `export`     |

Use `#private` fields (ES private) for true private class members.

## Project Structure

```
typescript/rand/
├── package.json          # name: "@bc/rand"
├── tsconfig.json
├── src/
│   ├── index.ts          # public API re-exports
│   └── *.ts
└── tests/
    └── *.test.ts
```

- Use Vitest or Jest for tests. Mirror Rust test names in camelCase.
- Target ES2022+ (for `using`, private fields, top-level await).
- Emit both ESM and CJS if needed. Use `"type": "module"` in package.json.
- Use strict TypeScript (`"strict": true`).
- Use `Uint8Array` consistently for binary data, not `Buffer` (Node-specific).

## Resources

- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/)
- [TypeScript Standard Library (lib.es2022)](https://www.typescriptlang.org/docs/)
- [MDN Web APIs](https://developer.mozilla.org/en-US/docs/Web/API) (Uint8Array, SubtleCrypto, etc.)
- [Rust Reference](https://doc.rust-lang.org/reference/)
- [Web Crypto API](https://developer.mozilla.org/en-US/docs/Web/API/Web_Crypto_API)

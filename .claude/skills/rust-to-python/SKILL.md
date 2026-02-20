---
name: rust-to-python
description: >-
  Reference for translating Rust crates to idiomatic Python. Covers type mappings,
  idiom translations, error handling, naming conventions, and project structure.
  Use when translating Rust code to Python or working in the python/ directory.
user-invocable: false
---

# Rust to Python Translation Guide

## Naming Conventions

| Rust              | Python                     |
|-------------------|----------------------------|
| `snake_case`      | `snake_case` (functions, methods, variables) |
| `MyStruct`        | `MyClass` (classes)        |
| `SCREAMING_SNAKE` | `SCREAMING_SNAKE` (module constants) |
| `mod my_mod`      | `my_mod.py` or `my_mod/`  |
| `my_crate`        | `my_crate` (package)      |
| `my-crate`        | `my-crate` (PyPI name), `my_crate` (import name) |

## Type Mappings

| Rust           | Python              | Notes                          |
|----------------|----------------------|--------------------------------|
| `u8`–`u64`    | `int`               | Python int is arbitrary precision |
| `i8`–`i64`    | `int`               |                                |
| `f32`/`f64`   | `float`             | Always 64-bit in CPython       |
| `bool`        | `bool`              |                                |
| `char`        | `str` (len 1)       |                                |
| `String`      | `str`               |                                |
| `&str`        | `str`               |                                |
| `&[u8]`       | `bytes` or `memoryview` |                             |
| `Vec<T>`      | `list[T]`           |                                |
| `Vec<u8>`     | `bytes` / `bytearray` | `bytes` immutable, `bytearray` mutable |
| `HashMap<K,V>`| `dict[K, V]`        |                                |
| `HashSet<T>`  | `set[T]`            |                                |
| `Option<T>`   | `T | None`          | Use `Optional[T]` for <3.10   |
| `Result<T,E>` | Raise exceptions    |                                |
| `Box<T>`      | `T`                 |                                |
| `Rc<T>`/`Arc<T>` | `T`              |                                |
| `()`          | `None`              |                                |
| `(A, B)`      | `tuple[A, B]`       |                                |
| `usize`       | `int`               |                                |

## Idiom Translations

### Ownership and Borrowing

Python uses garbage collection and reference counting. No ownership translation needed. For crypto secrets, use `__del__` or context managers for cleanup:

```python
# Rust: impl Drop for SecretKey { fn drop(&mut self) { zeroize... } }
class SecretKey:
    def __enter__(self):
        return self
    def __exit__(self, *args):
        # zeroize buffer
        ...
```

### Result<T, E> and Error Handling

Use exceptions. Define a hierarchy per module:

```python
# Rust: pub enum ShamirError { InvalidThreshold, ... }
class ShamirError(Exception): ...
class InvalidThresholdError(ShamirError): ...

# Rust: fn split(...) -> Result<Vec<Share>, ShamirError>
def split(...) -> list[Share]:  # raises ShamirError
```

### Option<T>

Use `None` with type hints:

```python
# Rust: fn get(&self, key: &str) -> Option<&Value>
def get(self, key: str) -> Value | None:
```

### Enums with Data (Algebraic Data Types)

Use dataclasses with a union type, or use a base class with `__match_args__`:

```python
# Rust: enum CborValue { UInt(u64), NInt(i64), Bytes(Vec<u8>), Text(str), ... }
from dataclasses import dataclass

class CborValue:
    pass

@dataclass(frozen=True)
class CborUInt(CborValue):
    value: int

@dataclass(frozen=True)
class CborNInt(CborValue):
    value: int

@dataclass(frozen=True)
class CborBytes(CborValue):
    value: bytes

@dataclass(frozen=True)
class CborText(CborValue):
    value: str
```

### Pattern Matching

Use Python 3.10+ structural pattern matching:

```python
# Rust: match value { CborValue::UInt(n) => ..., CborValue::Text(s) => ... }
match value:
    case CborUInt(value=n):
        ...
    case CborText(value=s):
        ...
    case _:
        raise ValueError(f"Unexpected CBOR type: {type(value)}")
```

### Traits

Use `Protocol` (structural) or `ABC` (nominal):

```python
# Rust: trait CBOREncodable { fn cbor(&self) -> CBOR; }
from typing import Protocol

class CborEncodable(Protocol):
    def to_cbor(self) -> Cbor: ...

# Or with ABC:
from abc import ABC, abstractmethod

class CborEncodable(ABC):
    @abstractmethod
    def to_cbor(self) -> Cbor: ...
```

Use `Protocol` when the trait is purely structural (duck typing). Use `ABC` when you need guaranteed implementation and want `isinstance()` checks.

### Iterators and Closures

Python has comprehensions and generator expressions:

```python
# Rust: items.iter().filter(|x| x.is_valid()).map(|x| x.value()).collect()
[item.value for item in items if item.is_valid]
```

| Rust             | Python                     |
|------------------|----------------------------|
| `.iter()`        | (implicit iteration)       |
| `.map(f)`        | `[f(x) for x in ...]` or `map(f, ...)` |
| `.filter(f)`     | `[x for x in ... if f(x)]` |
| `.flat_map(f)`   | `[y for x in ... for y in f(x)]` |
| `.collect()`     | `list(...)` / wrap in constructor |
| `.enumerate()`   | `enumerate(...)`           |
| `.zip()`         | `zip(...)`                 |
| `.fold(init, f)` | `functools.reduce(f, ..., init)` |
| `.any()`         | `any(...)`                 |
| `.all()`         | `all(...)`                 |
| `.find()`        | `next((x for x in ... if f(x)), None)` |
| `.count()`       | `len(list(...))` or `sum(1 for ...)` |

### Derive Macros

| Rust derive       | Python equivalent                    |
|--------------------|--------------------------------------|
| `Clone`            | `copy.deepcopy()` or `dataclass`     |
| `Debug`            | `__repr__`                           |
| `Display`          | `__str__`                            |
| `PartialEq/Eq`    | `__eq__` (auto in `dataclass`)       |
| `Hash`             | `__hash__` (auto in frozen dataclass) |
| `PartialOrd/Ord`   | `__lt__`, `__le__`, etc. or `@functools.total_ordering` |
| `Default`          | Default arguments or `@classmethod` factory |

### Visibility

Python has no access control. Use underscore prefix convention:

| Rust            | Python          |
|-----------------|-----------------|
| `pub`           | No prefix       |
| `pub(crate)`    | `_name`         |
| (default)       | `_name`         |
| (truly private) | `__name` (name-mangled) |

## Project Structure

```
python/bc-rand/
├── pyproject.toml
├── src/
│   └── bc_rand/
│       ├── __init__.py
│       └── *.py
└── tests/
    └── test_*.py
```

- Use `pyproject.toml` with a build backend (hatchling, setuptools, etc.).
- Use pytest for tests. Mirror Rust test names: `test_split_valid_threshold`.
- Use type hints throughout. Run mypy for type checking.
- Target Python 3.10+ (for `match` statements and `X | Y` type syntax).

## Resources

- [Python Language Reference](https://docs.python.org/3/reference/)
- [Python Standard Library](https://docs.python.org/3/library/)
- [Python Typing](https://docs.python.org/3/library/typing.html)
- [Rust Reference](https://doc.rust-lang.org/reference/)
- [cryptography (pyca)](https://cryptography.io/en/latest/)

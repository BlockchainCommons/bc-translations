# C# Translation Memory

- For Rust crypto code that zeroes non-byte buffers (for example `u32` working arrays), use `CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(span))` instead of `Array.Clear` to preserve explicit secure-wipe intent.
- In consensus/vector-sensitive translations, keep Rust wrapping-cast behavior (`as u8`) with explicit `unchecked` casts in C# where needed rather than adding stricter range checks that alter behavior.
- For Rust crates that rely on macro-generated constant surfaces, derive C# constants and registration lists directly from source declarations to prevent silent omissions or value/name drift.
- For Rust algorithms over `usize` ranges (for example Fisher-Yates shuffle indices), prefer the 64-bit RNG path (`NextWithUpperBound(ulong)`) in C# tests to preserve deterministic vector parity with Rust on 64-bit targets.
- When exposing Rust slice-like collections from immutable C# models, avoid returning raw arrays typed as `IReadOnlyList<T>`; wrap with `Array.AsReadOnly(...)` to prevent mutation leaks.

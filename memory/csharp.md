# C# Translation Memory

- For Rust crypto code that zeroes non-byte buffers (for example `u32` working arrays), use `CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(span))` instead of `Array.Clear` to preserve explicit secure-wipe intent.
- In consensus/vector-sensitive translations, keep Rust wrapping-cast behavior (`as u8`) with explicit `unchecked` casts in C# where needed rather than adding stricter range checks that alter behavior.

# Translation Lessons

- Before selecting Go crypto dependencies, verify module `go` version requirements against the repo baseline; avoid versions that force a higher toolchain than the monorepo currently uses.
- When Rust secp256k1 APIs accept variable-length Schnorr messages, confirm target-language library semantics early; many wrappers only accept 32-byte hashes and need a compatibility layer for byte-for-byte vector parity.
- Validate test counts directly from Rust sources (`#[test]` inventory) before finalizing manifests to prevent catalog drift in completeness checks.
- For Node-based translations of Rust crypto crates, validate runtime and type-definition support separately; modern `node:crypto` APIs may exist at runtime while `@types/node` lags and requires typed wrappers.
- On macOS/Homebrew Python setups, expect PEP 668 restrictions; use a disposable virtual environment for translation test runs to keep execution reproducible without mutating system Python.
- When porting Rust cryptographic code to C#, preserve secure cleanup semantics for both `byte[]` and non-byte work buffers; for `uint[]`/`ulong[]`, wipe through `MemoryMarshal.AsBytes(...)` with `CryptographicOperations.ZeroMemory`.
- In this parallel monorepo, refresh the exact `AGENTS.md` row immediately before writing final status changes to avoid clobbering concurrent updates from other agents.

# Translation Lessons

- Before selecting Go crypto dependencies, verify module `go` version requirements against the repo baseline; avoid versions that force a higher toolchain than the monorepo currently uses.
- When Rust secp256k1 APIs accept variable-length Schnorr messages, confirm target-language library semantics early; many wrappers only accept 32-byte hashes and need a compatibility layer for byte-for-byte vector parity.
- Validate test counts directly from Rust sources (`#[test]` inventory) before finalizing manifests to prevent catalog drift in completeness checks.

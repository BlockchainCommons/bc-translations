# Translation Lessons

- Before selecting Go crypto dependencies, verify module `go` version requirements against the repo baseline; avoid versions that force a higher toolchain than the monorepo currently uses.
- When Rust secp256k1 APIs accept variable-length Schnorr messages, confirm target-language library semantics early; many wrappers only accept 32-byte hashes and need a compatibility layer for byte-for-byte vector parity.
- Validate test counts directly from Rust sources (`#[test]` inventory) before finalizing manifests to prevent catalog drift in completeness checks.
- For Node-based translations of Rust crypto crates, validate runtime and type-definition support separately; modern `node:crypto` APIs may exist at runtime while `@types/node` lags and requires typed wrappers.
- On macOS/Homebrew Python setups, expect PEP 668 restrictions; use a disposable virtual environment for translation test runs to keep execution reproducible without mutating system Python.
- When porting Rust cryptographic code to C#, preserve secure cleanup semantics for both `byte[]` and non-byte work buffers; for `uint[]`/`ulong[]`, wipe through `MemoryMarshal.AsBytes(...)` with `CryptographicOperations.ZeroMemory`.
- In this parallel monorepo, refresh the exact `AGENTS.md` row immediately before writing final status changes to avoid clobbering concurrent updates from other agents.
- For Go targets, lock `GOTOOLCHAIN=local` during `go mod tidy` and tests when the repo baseline is Go 1.21; otherwise dependency resolution can silently bump the module `go` directive.
- For dCBOR translations, remember map-key deterministic ordering is by encoded CBOR bytes (including major-type/length header), which can differ from lexical source-key order and should be asserted explicitly in tests.
- For any Stage 4 fluency review pass (including reruns on completed targets), always append a root `LOG.md` table row with task text starting `Fluency critique`, not just the per-target `<lang>/<package>/LOG.md` entries.
- When importing a mature upstream translation into this monorepo, immediately replace workspace-specific build config (`extends`, workspace deps) with local package settings, then run both build and tests before starting completeness sign-off.
- If TypeScript module typings fail for package subpaths, prefer a package-local `.d.ts` shim near source instead of relying on external DefinitelyTyped coverage.
- In Swift translations, if the public API includes a type named `Set`, qualify call sites as `<Module>.Set` in tests to avoid collisions with `Swift.Set`.
- For Swift Testing with warnings-as-errors, avoid bare `.none` when comparing optional enums; use the explicit enum case (for example `EdgeType.none`) to prevent ambiguity warnings.
- For crates with macro-generated APIs, script symbol extraction from Rust source and run count-based parity checks before completion to avoid manual transcription drift.
- For macro-heavy Rust crates (for example large `const_*` registries), generate translated constant tables from source declarations and verify counts against the registration list before finalizing completeness.
- For Python packages layered on `dcbor`, initialize the global tags store with `with_tags(lambda _: None)` before calling `with_tags_mut(...)`; otherwise first-use registration can deadlock on the current non-reentrant lock path.
- For Swift targets, always run both `swift test` and `swift test -Xswiftc -warnings-as-errors` before completion; strict builds catch symbol ambiguities and API-surface warnings that normal test runs can miss.

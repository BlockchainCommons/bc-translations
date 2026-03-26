# Go Translation Memory

- Pin `github.com/btcsuite/btcd/btcec/v2` to `v2.3.4` for this repo's Go 1.21 baseline; `v2.3.5+` raises module `go` to 1.22 and can fail in environments without toolchain auto-download.
- For `bc-crypto` Schnorr, do not use `schnorr.Sign` directly from btcec for the exported API because it enforces 32-byte hashes; implement BIP340 sign/verify over arbitrary message bytes to match Rust vectors and behavior.
- For Go 1.21 crate translations, pin `golang.org/x/text` to a 1.21-compatible release (e.g. `v0.21.0`) and run `GOTOOLCHAIN=local go mod tidy` to prevent accidental `go` directive upgrades.
- For new Go target modules, do not run `go mod tidy` in parallel with `go test`; tidy mutates `go.mod`/`go.sum`, and a concurrent test run can fail against stale module metadata.
- In dCBOR map tests, key ordering must follow lexicographic order of encoded CBOR key bytes (length-prefixed text headers), not plain alphabetical string order.
- For new Go crates depending on in-repo modules, mirror all required local `replace` directives in the top-level target `go.mod` (including transitive in-repo modules like `bccrypto-go`), because dependency-module `replace` directives are not applied by the main module.
- When translating Rust tests that use `rng_next_in_closed_range` with `usize` bounds, use `bcrand.NextInClosedRange(..., 64)` in Go on 64-bit targets; using 32-bit width changes deterministic shuffle/vector outputs.

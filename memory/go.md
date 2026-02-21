# Go Translation Memory

- Pin `github.com/btcsuite/btcd/btcec/v2` to `v2.3.4` for this repo's Go 1.21 baseline; `v2.3.5+` raises module `go` to 1.22 and can fail in environments without toolchain auto-download.
- For `bc-crypto` Schnorr, do not use `schnorr.Sign` directly from btcec for the exported API because it enforces 32-byte hashes; implement BIP340 sign/verify over arbitrary message bytes to match Rust vectors and behavior.
- For Go 1.21 crate translations, pin `golang.org/x/text` to a 1.21-compatible release (e.g. `v0.21.0`) and run `GOTOOLCHAIN=local go mod tidy` to prevent accidental `go` directive upgrades.
- In dCBOR map tests, key ordering must follow lexicographic order of encoded CBOR key bytes (length-prefixed text headers), not plain alphabetical string order.

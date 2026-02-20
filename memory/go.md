# Go Translation Memory

- Pin `github.com/btcsuite/btcd/btcec/v2` to `v2.3.4` for this repo's Go 1.21 baseline; `v2.3.5+` raises module `go` to 1.22 and can fail in environments without toolchain auto-download.
- For `bc-crypto` Schnorr, do not use `schnorr.Sign` directly from btcec for the exported API because it enforces 32-byte hashes; implement BIP340 sign/verify over arbitrary message bytes to match Rust vectors and behavior.

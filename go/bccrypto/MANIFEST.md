# bc-crypto Translation Manifest (Go)

## Source Crate
- Rust crate: `rust/bc-crypto`
- Version: `0.14.0`
- Rust edition: `2024`
- Package description: "A uniform API for cryptographic primitives used in Blockchain Commons projects"

## Feature Flags
- `default = ["secp256k1", "ed25519"]`
- `secp256k1`: gates `ecdsa_keys`, `ecdsa_signing`, `schnorr_signing`
- `ed25519`: gates `ed25519_signing`
- Initial Go scope: default features only (both enabled)

## Internal Dependencies
- `bc-rand ^0.5.0` (use Go package `github.com/nickel-blockchaincommons/bcrand-go`)

## External Dependency Mapping (Rust -> Go)
- `sha2` -> `crypto/sha256`, `crypto/sha512`
- `hmac` -> `crypto/hmac`
- `pbkdf2` -> `golang.org/x/crypto/pbkdf2`
- `hkdf` -> `golang.org/x/crypto/hkdf`
- `crc32fast` -> `hash/crc32`
- `chacha20poly1305` -> `golang.org/x/crypto/chacha20poly1305`
- `x25519-dalek` -> `golang.org/x/crypto/curve25519`
- `secp256k1` -> `github.com/btcsuite/btcd/btcec/v2` (pin `v2.3.4` for Go 1.21 compatibility)
- `ed25519-dalek` -> `crypto/ed25519`
- `scrypt` -> `golang.org/x/crypto/scrypt`
- `argon2` -> `golang.org/x/crypto/argon2`
- `thiserror` -> idiomatic Go `error`

## Documentation Catalog
- Crate-level docs: yes (`src/lib.rs`), includes algorithm/provider table and getting-started section.
- Module-level docs: sparse; most behavior docs are on public functions/constants.
- Public item docs: present on nearly all public functions in `hash`, `public_key_encryption`, `symmetric_encryption`, and KDF modules.
- Public items without explicit docs: several constants and feature-gated exports in `lib.rs` rely on module docs.
- README: exists (`rust/bc-crypto/README.md`), mirrors introduction and dependency table.

## Public API Inventory

### Public Types
- `Error` enum (AEAD error wrapper)
- `Result<T>` alias (`std::result::Result<T, Error>`)

### Public Constants
- `CRC32_SIZE = 4`
- `SHA256_SIZE = 32`
- `SHA512_SIZE = 64`
- `SYMMETRIC_KEY_SIZE = 32`
- `SYMMETRIC_NONCE_SIZE = 12`
- `SYMMETRIC_AUTH_SIZE = 16`
- `X25519_PRIVATE_KEY_SIZE = 32`
- `X25519_PUBLIC_KEY_SIZE = 32`
- `ECDSA_PRIVATE_KEY_SIZE = 32`
- `ECDSA_PUBLIC_KEY_SIZE = 33`
- `ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE = 65`
- `ECDSA_MESSAGE_HASH_SIZE = 32`
- `ECDSA_SIGNATURE_SIZE = 64`
- `SCHNORR_PUBLIC_KEY_SIZE = 32`
- `SCHNORR_SIGNATURE_SIZE = 64`
- `ED25519_PUBLIC_KEY_SIZE = 32`
- `ED25519_PRIVATE_KEY_SIZE = 32`
- `ED25519_SIGNATURE_SIZE = 64`

### Public Functions

#### Hash (`src/hash.rs`)
- `crc32(data) -> u32`
- `crc32_data_opt(data, little_endian) -> [u8; 4]`
- `crc32_data(data) -> [u8; 4]`
- `sha256(data) -> [u8; 32]`
- `double_sha256(message) -> [u8; 32]`
- `sha512(data) -> [u8; 64]`
- `hmac_sha256(key, message) -> [u8; 32]`
- `hmac_sha512(key, message) -> [u8; 64]`
- `pbkdf2_hmac_sha256(pass, salt, iterations, key_len) -> Vec<u8>`
- `pbkdf2_hmac_sha512(pass, salt, iterations, key_len) -> Vec<u8>`
- `hkdf_hmac_sha256(key_material, salt, key_len) -> Vec<u8>`
- `hkdf_hmac_sha512(key_material, salt, key_len) -> Vec<u8>`

#### Memzero (`src/memzero.rs`)
- `memzero<T>(s: &mut [T])`
- `memzero_vec_vec_u8(s: &mut [Vec<u8>])`

#### Symmetric Encryption (`src/symmetric_encryption.rs`)
- `aead_chacha20_poly1305_encrypt_with_aad(plaintext, key, nonce, aad) -> (Vec<u8>, [u8; 16])`
- `aead_chacha20_poly1305_encrypt(plaintext, key, nonce) -> (Vec<u8>, [u8; 16])`
- `aead_chacha20_poly1305_decrypt_with_aad(ciphertext, key, nonce, aad, auth) -> Result<Vec<u8>>`
- `aead_chacha20_poly1305_decrypt(ciphertext, key, nonce, auth) -> Result<Vec<u8>>`

#### Public-Key Encryption / Derivation (`src/public_key_encryption.rs`)
- `derive_agreement_private_key(key_material) -> [u8; 32]`
- `derive_signing_private_key(key_material) -> [u8; 32]`
- `x25519_new_private_key_using(rng) -> [u8; 32]`
- `x25519_public_key_from_private_key(private_key) -> [u8; 32]`
- `x25519_shared_key(private_key, public_key) -> [u8; 32]`

#### secp256k1 Keys (`src/ecdsa_keys.rs`)
- `ecdsa_new_private_key_using(rng) -> [u8; 32]`
- `ecdsa_public_key_from_private_key(private_key) -> [u8; 33]`
- `ecdsa_decompress_public_key(compressed) -> [u8; 65]`
- `ecdsa_compress_public_key(uncompressed) -> [u8; 33]`
- `ecdsa_derive_private_key(key_material) -> Vec<u8>`
- `schnorr_public_key_from_private_key(private_key) -> [u8; 32]`

#### secp256k1 ECDSA (`src/ecdsa_signing.rs`)
- `ecdsa_sign(private_key, message) -> [u8; 64]`
- `ecdsa_verify(public_key, signature, message) -> bool`

#### secp256k1 Schnorr (`src/schnorr_signing.rs`)
- `schnorr_sign(private_key, message) -> [u8; 64]`
- `schnorr_sign_using(private_key, message, rng) -> [u8; 64]`
- `schnorr_sign_with_aux_rand(private_key, message, aux_rand) -> [u8; 64]`
- `schnorr_verify(public_key_xonly, signature, message) -> bool`

#### Ed25519 (`src/ed25519_signing.rs`)
- `ed25519_new_private_key_using(rng) -> [u8; 32]`
- `ed25519_public_key_from_private_key(private_key) -> [u8; 32]`
- `ed25519_sign(private_key, message) -> [u8; 64]`
- `ed25519_verify(public_key, message, signature) -> bool`

#### Password KDFs
- `scrypt(pass, salt, output_len) -> Vec<u8>`
- `scrypt_opt(pass, salt, output_len, log_n, r, p) -> Vec<u8>`
- `argon2id(pass, salt, output_len) -> Vec<u8>`

## Rust Test Inventory (Default Features)
- `src/lib.rs`:
  - `test_readme_deps` (Rust metadata sync)
  - `test_html_root_url` (Rust metadata sync)
- `src/hash.rs`: 6 tests
  - `test_crc32`, `test_sha256`, `test_sha512`, `test_hmac_sha`, `test_pbkdf2_hmac_sha256`, `test_hkdf_hmac_sha256`
- `src/symmetric_encryption.rs`: 3 tests
  - `test_rfc_test_vector`, `test_random_key_and_nonce`, `test_empty_data`
- `src/public_key_encryption.rs`: 2 tests
  - `test_x25519_keys`, `test_key_agreement`
- `src/ecdsa_keys.rs`: 1 test
  - `test_ecdsa_keys`
- `src/ecdsa_signing.rs`: 1 test
  - `test_ecdsa_signing`
- `src/schnorr_signing.rs`: 21 tests
  - `test_schnorr_sign`, `test_0`..`test_18`, `test_verify_tweaked`
  - `test_5` and `test_14` are panic-path invalid x-only pubkey cases
- `src/ed25519_signing.rs`: 2 tests
  - `test_ed25519_signing`, `test_ed25519_vectors`
- `src/scrypt.rs`: 4 tests
  - `test_scrypt_basic`, `test_scrypt_different_salt`, `test_scrypt_opt_basic`, `test_scrypt_output_length`
- `src/argon.rs`: 2 tests
  - `test_argon2id_basic`, `test_argon2id_different_salt`
- Total Rust tests: 44 (including 2 Rust-only metadata tests)

## Translation Unit Order
1. `error.rs`
2. `hash.rs`
3. `memzero.rs`
4. `symmetric_encryption.rs`
5. `public_key_encryption.rs`
6. `ecdsa_keys.rs`
7. `ecdsa_signing.rs`
8. `schnorr_signing.rs`
9. `ed25519_signing.rs`
10. `scrypt.rs`
11. `argon.rs`
12. `lib.rs` exports (Go package surface)
13. Tests

## Translation Hazards
- Schnorr API in Rust signs and verifies **arbitrary-length message bytes** (not fixed 32-byte digests); Go implementation must preserve this behavior and pass BIP340 vectors.
- Rust uses strict parsing with `expect` in several secp256k1 paths; panic/invalid-input behavior in tests (`test_5`, `test_14`) must be preserved.
- ECDSA signs `double_sha256(message)`, not single SHA-256.
- ECDSA signature output must be compact 64-byte `r||s` form, not DER.
- X25519 shared key output is HKDF-HMAC-SHA256-derived with salt `"agreement"`.
- Domain separation salts (`"agreement"`, `"signing"`) are consensus-relevant vectors.
- Rust fixed-size arrays require exact length checks at Go API boundaries.
- Keep deterministic vector paths wired to `bcrand.NewFakeRandomNumberGenerator()`.
- Rust metadata tests are crate-specific; replace with Go-equivalent package/export sanity tests.

## Completion Targets
- API coverage: 100% of listed constants/functions/types.
- Test coverage: translate all behavior tests and vector checks (Rust metadata tests replaced with Go-appropriate checks).
- Build/test gate: `go test ./...` passes in `go/bccrypto`.

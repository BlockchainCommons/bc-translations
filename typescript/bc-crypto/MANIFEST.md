# Translation Manifest: bc-crypto → TypeScript (@bc/crypto)

Source: `rust/bc-crypto/` v0.14.0
Target: `typescript/bc-crypto/` package `@bc/crypto`

## Crate Metadata

- Rust crate: `bc-crypto`
- Version: `0.14.0`
- Description: "A uniform API for cryptographic primitives used in Blockchain Commons projects"
- Default features: `secp256k1`, `ed25519`
- Internal BC dependencies: `bc-rand ^0.5.0`

## Feature Flags

- `default = ["secp256k1", "ed25519"]`
- `secp256k1`: gates ECDSA key/signing modules and Schnorr module
- `ed25519`: gates Ed25519 module

Translation scope for this target includes default-feature code only.

## External Dependency Equivalents

| Rust crate           | Purpose                                | TypeScript equivalent |
|----------------------|----------------------------------------|-----------------------|
| `bc-rand`            | RNG trait + fake/secure RNG            | `@bc/rand`            |
| `sha2`, `hmac`       | SHA-256/512 + HMAC                     | Node `crypto`         |
| `pbkdf2`, `hkdf`     | PBKDF2/HKDF                            | Node `crypto`         |
| `crc32fast`          | CRC-32 checksum                        | `crc-32`              |
| `chacha20poly1305`   | ChaCha20-Poly1305 AEAD                 | Node `crypto`         |
| `secp256k1`          | ECDSA + BIP340 Schnorr                 | `@noble/curves`       |
| `x25519-dalek`       | X25519 key agreement                   | `@noble/curves`       |
| `ed25519-dalek`      | Ed25519 sign/verify                    | `@noble/curves`       |
| `scrypt`             | scrypt KDF                             | Node `crypto`         |
| `argon2`             | Argon2id KDF                           | `argon2`              |
| `thiserror`          | error derive                           | custom TS error class |
| `hex`                | hex encoding in tests                  | test helper functions |

## Documentation Catalog

- Crate-level docs: present in `src/lib.rs` (`//!` intro and provider table).
- Module-level docs: sparse; mostly item-level `///` docs in `hash.rs`, `symmetric_encryption.rs`, `public_key_encryption.rs`, `scrypt.rs`.
- Public items with docs: hashing/KDF functions, AEAD functions, key-derivation functions, some size constants.
- Public items without docs in Rust: many ECDSA/Schnorr/Ed25519 functions and constants.
- README: present in Rust crate, dependency usage docs.

Rule for translation: preserve doc comments where Rust has them; do not invent docs for items that have none in Rust.

## Public API Surface

### Error

- Type: `Error` (`AEAD error` case)
- Alias: `Result<T>` (maps to throws + return values in TypeScript)

### Constants

- `CRC32_SIZE`
- `SHA256_SIZE`
- `SHA512_SIZE`
- `SYMMETRIC_KEY_SIZE`
- `SYMMETRIC_NONCE_SIZE`
- `SYMMETRIC_AUTH_SIZE`
- `GENERIC_PRIVATE_KEY_SIZE`
- `GENERIC_PUBLIC_KEY_SIZE`
- `X25519_PRIVATE_KEY_SIZE`
- `X25519_PUBLIC_KEY_SIZE`
- `ECDSA_PRIVATE_KEY_SIZE`
- `ECDSA_PUBLIC_KEY_SIZE`
- `ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE`
- `ECDSA_MESSAGE_HASH_SIZE`
- `ECDSA_SIGNATURE_SIZE`
- `SCHNORR_PUBLIC_KEY_SIZE`
- `SCHNORR_SIGNATURE_SIZE`
- `ED25519_PUBLIC_KEY_SIZE`
- `ED25519_PRIVATE_KEY_SIZE`
- `ED25519_SIGNATURE_SIZE`

### Hashing / KDF

- `crc32(data)`
- `crc32_data_opt(data, little_endian)`
- `crc32_data(data)`
- `sha256(data)`
- `double_sha256(message)`
- `sha512(data)`
- `hmac_sha256(key, message)`
- `hmac_sha512(key, message)`
- `pbkdf2_hmac_sha256(pass, salt, iterations, key_len)`
- `pbkdf2_hmac_sha512(pass, salt, iterations, key_len)`
- `hkdf_hmac_sha256(key_material, salt, key_len)`
- `hkdf_hmac_sha512(key_material, salt, key_len)`

### Memory utilities

- `memzero(slice)`
- `memzero_vec_vec_u8(slice_of_slices)`

### Symmetric encryption

- `aead_chacha20_poly1305_encrypt_with_aad(plaintext, key, nonce, aad)`
- `aead_chacha20_poly1305_encrypt(plaintext, key, nonce)`
- `aead_chacha20_poly1305_decrypt_with_aad(ciphertext, key, nonce, aad, auth)`
- `aead_chacha20_poly1305_decrypt(ciphertext, key, nonce, auth)`

### Public-key agreement / derivation

- `derive_agreement_private_key(key_material)`
- `derive_signing_private_key(key_material)`
- `x25519_new_private_key_using(rng)`
- `x25519_public_key_from_private_key(private_key)`
- `x25519_shared_key(private_key, public_key)`

### ECDSA keys

- `ecdsa_new_private_key_using(rng)`
- `ecdsa_public_key_from_private_key(private_key)`
- `ecdsa_decompress_public_key(compressed_public_key)`
- `ecdsa_compress_public_key(uncompressed_public_key)`
- `ecdsa_derive_private_key(key_material)`
- `schnorr_public_key_from_private_key(private_key)`

### ECDSA signing

- `ecdsa_sign(private_key, message)`
- `ecdsa_verify(public_key, signature, message)`

### Schnorr signing

- `schnorr_sign(ecdsa_private_key, message)`
- `schnorr_sign_using(ecdsa_private_key, message, rng)`
- `schnorr_sign_with_aux_rand(ecdsa_private_key, message, aux_rand)`
- `schnorr_verify(schnorr_public_key, schnorr_signature, message)`

### Ed25519 signing

- `ed25519_new_private_key_using(rng)`
- `ed25519_public_key_from_private_key(private_key)`
- `ed25519_sign(private_key, message)`
- `ed25519_verify(public_key, message, signature)`

### Password KDF

- `scrypt(pass, salt, output_len)`
- `scrypt_opt(pass, salt, output_len, log_n, r, p)`
- `argon2id(pass, salt, output_len)`

### Root exports from `lib.rs`

Direct root re-exports in Rust to preserve from package root:
- `Error`, `Result`
- `CRC32_SIZE`, `SHA256_SIZE`, `SHA512_SIZE`
- `sha256`, `sha512`, `double_sha256`
- `hmac_sha256`, `hmac_sha512`
- `pbkdf2_hmac_sha256`, `hkdf_hmac_sha256`
- `memzero`, `memzero_vec_vec_u8`
- `SYMMETRIC_KEY_SIZE`, `SYMMETRIC_NONCE_SIZE`, `SYMMETRIC_AUTH_SIZE`
- all AEAD helpers
- `X25519_PRIVATE_KEY_SIZE`, `X25519_PUBLIC_KEY_SIZE`
- key derivation + X25519 helpers
- ECDSA/Schnorr constants + key/sign/verify helpers
- Ed25519 constants + key/sign/verify helpers
- `scrypt`, `scrypt_opt`, `argon2id`

## Translation Unit Order

1. `error.rs` → `src/error.ts`
2. `hash.rs` → `src/hash.ts`
3. `memzero.rs` → `src/memzero.ts`
4. `symmetric_encryption.rs` → `src/symmetric-encryption.ts`
5. `public_key_encryption.rs` → `src/public-key-encryption.ts`
6. `ecdsa_keys.rs` → `src/ecdsa-keys.ts`
7. `ecdsa_signing.rs` → `src/ecdsa-signing.ts`
8. `schnorr_signing.rs` → `src/schnorr-signing.ts`
9. `ed25519_signing.rs` → `src/ed25519-signing.ts`
10. `scrypt.rs` → `src/scrypt.ts`
11. `argon.rs` → `src/argon.ts`
12. `lib.rs` exports → `src/index.ts`
13. Tests translated into `tests/*.test.ts`

## Test Inventory (Rust)

### `hash.rs` (6)
- `test_crc32`
- `test_sha256`
- `test_sha512`
- `test_hmac_sha`
- `test_pbkdf2_hmac_sha256`
- `test_hkdf_hmac_sha256`

### `symmetric_encryption.rs` (3)
- `test_rfc_test_vector`
- `test_random_key_and_nonce`
- `test_empty_data`

### `public_key_encryption.rs` (2)
- `test_x25519_keys`
- `test_key_agreement`

### `ecdsa_keys.rs` (1)
- `test_ecdsa_keys`

### `ecdsa_signing.rs` (1)
- `test_ecdsa_signing`

### `schnorr_signing.rs` (21)
- `test_schnorr_sign`
- `test_0` through `test_18`
- `test_verify_tweaked`

### `ed25519_signing.rs` (2)
- `test_ed25519_signing`
- `test_ed25519_vectors`

### `scrypt.rs` (4)
- `test_scrypt_basic`
- `test_scrypt_different_salt`
- `test_scrypt_opt_basic`
- `test_scrypt_output_length`

### `argon.rs` (2)
- `test_argon2id_basic`
- `test_argon2id_different_salt`

### `lib.rs` metadata tests (2)
- `test_readme_deps` (Rust-specific, adapt as package metadata checks)
- `test_html_root_url` (Rust-specific, omit)

Total Rust tests inventoried: 44

## Translation Hazards

1. **Schnorr behavior must match BIP340 vectors exactly**
   - Verification includes known failure vectors and invalid public-key panic equivalents.

2. **ECDSA output format**
   - Rust returns compact 64-byte `(r||s)` signatures, not DER.

3. **X25519 + HKDF post-processing**
   - Shared secret must be passed through HKDF-SHA256 with salt `"agreement"`.

4. **Deterministic vectors rely on `@bc/rand` fake RNG**
   - Keep byte-for-byte compatibility with existing `@bc/rand` implementation.

5. **AEAD API shape**
   - Rust returns `(ciphertext, authTag)` separately; TypeScript should keep this split.

6. **Rust panic semantics**
   - Invalid inputs often `expect`/`unwrap`; TypeScript should throw deterministic errors on invalid key material/decrypt failures.

7. **`memzero` best-effort semantics**
   - JavaScript cannot guarantee volatile zeroization; implement in-place overwrite semantics.

8. **Argon2 defaults**
   - Keep deterministic behavior for same input and non-equality for different salt; exact Rust parameter parity is preferred where library allows.

## Completion Criteria

- All default-feature public constants/functions are translated and exported.
- All vectors and deterministic tests are translated with identical expected bytes.
- TypeScript build is clean (`tsc`).
- Vitest suite passes with parity coverage.

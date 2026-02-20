# bc-crypto Translation Manifest (Python)

## Source Crate
- Rust crate: `rust/bc-crypto`
- Version: `0.14.0`
- Default features: `secp256k1`, `ed25519`
- Internal dependency: `bc-rand ^0.5.0`

## Scope
Translate default-feature Rust API and tests into Python under `python/bc-crypto` with equivalent behavior and vectors.

## Public API Inventory

### Root exports (`lib.rs`)
- `Error`, `Result`
- Hash:
  - `CRC32_SIZE`, `SHA256_SIZE`, `SHA512_SIZE`
  - `sha256`, `sha512`, `double_sha256`
  - `hmac_sha256`, `hmac_sha512`
  - `pbkdf2_hmac_sha256`
  - `hkdf_hmac_sha256`
- Memory zeroing:
  - `memzero`, `memzero_vec_vec_u8`
- Symmetric encryption:
  - `SYMMETRIC_KEY_SIZE`, `SYMMETRIC_NONCE_SIZE`, `SYMMETRIC_AUTH_SIZE`
  - `aead_chacha20_poly1305_encrypt_with_aad`, `aead_chacha20_poly1305_encrypt`
  - `aead_chacha20_poly1305_decrypt_with_aad`, `aead_chacha20_poly1305_decrypt`
- Public-key encryption and derivation:
  - `X25519_PRIVATE_KEY_SIZE`, `X25519_PUBLIC_KEY_SIZE`
  - `derive_agreement_private_key`, `derive_signing_private_key`
  - `x25519_new_private_key_using`, `x25519_public_key_from_private_key`, `x25519_shared_key`
- ECDSA key primitives (default feature):
  - `ECDSA_MESSAGE_HASH_SIZE`, `ECDSA_PRIVATE_KEY_SIZE`, `ECDSA_PUBLIC_KEY_SIZE`
  - `ECDSA_SIGNATURE_SIZE`, `ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE`
  - `SCHNORR_PUBLIC_KEY_SIZE`
  - `ecdsa_new_private_key_using`, `ecdsa_public_key_from_private_key`
  - `ecdsa_decompress_public_key`, `ecdsa_compress_public_key`
  - `ecdsa_derive_private_key`, `schnorr_public_key_from_private_key`
- ECDSA signing (default feature):
  - `ecdsa_sign`, `ecdsa_verify`
- Schnorr signing (default feature):
  - `SCHNORR_SIGNATURE_SIZE`
  - `schnorr_sign`, `schnorr_sign_using`, `schnorr_sign_with_aux_rand`, `schnorr_verify`
- Ed25519 signing (default feature):
  - `ED25519_PRIVATE_KEY_SIZE`, `ED25519_PUBLIC_KEY_SIZE`, `ED25519_SIGNATURE_SIZE`
  - `ed25519_new_private_key_using`, `ed25519_public_key_from_private_key`
  - `ed25519_sign`, `ed25519_verify`
- KDFs:
  - `scrypt`, `scrypt_opt`, `argon2id`

### Additional public module API to preserve
`hash` module additionally exposes:
- `crc32`, `crc32_data_opt`, `crc32_data`
- `pbkdf2_hmac_sha512`, `hkdf_hmac_sha512`

## Rust File → Python Unit Plan
1. `src/error.rs` → `src/bc_crypto/error.py`
2. `src/hash.rs` → `src/bc_crypto/hash.py`
3. `src/memzero.rs` → `src/bc_crypto/memzero.py`
4. `src/symmetric_encryption.rs` → `src/bc_crypto/symmetric_encryption.py`
5. `src/public_key_encryption.rs` → `src/bc_crypto/public_key_encryption.py`
6. `src/ecdsa_keys.rs` → `src/bc_crypto/ecdsa_keys.py`
7. `src/ecdsa_signing.rs` → `src/bc_crypto/ecdsa_signing.py`
8. `src/schnorr_signing.rs` → `src/bc_crypto/schnorr_signing.py`
9. `src/ed25519_signing.rs` → `src/bc_crypto/ed25519_signing.py`
10. `src/scrypt.rs` → `src/bc_crypto/scrypt.py`
11. `src/argon.rs` → `src/bc_crypto/argon.py`
12. `src/lib.rs` exports → `src/bc_crypto/__init__.py`

## Dependency Mapping (Rust → Python)
- `bc-rand` → local package dependency (`bc-rand==0.5.0` via path install during development)
- `sha2`, `hmac`, `pbkdf2`, `hkdf`, `crc32fast` → `hashlib`, `hmac`, `binascii`
- `chacha20poly1305`, `x25519-dalek`, `ed25519-dalek` → `cryptography`
- `secp256k1` (ECDSA + BIP340 Schnorr) → `btclib` + `btclib-libsecp256k1`
- `scrypt` → `hashlib.scrypt`
- `argon2` → `argon2-cffi`

## Test Inventory (Rust)
- `lib.rs`: 2 metadata/version-sync tests (Rust-specific; adapt/omit)
- `hash.rs`: 6 tests
  - CRC32, SHA-256, SHA-512, HMAC-SHA256/SHA512, PBKDF2-HMAC-SHA256, HKDF-HMAC-SHA256 vectors
- `symmetric_encryption.rs`: 3 tests
  - RFC ChaCha20-Poly1305 vector + random and empty-data round trips
- `public_key_encryption.rs`: 2 tests
  - deterministic X25519/derivation vectors + key agreement vector
- `ecdsa_keys.rs`: 1 test
  - deterministic private/public/compress/decompress/x-only vectors + derived private key vector
- `ecdsa_signing.rs`: 1 test
  - deterministic compact signature vector + verify
- `schnorr_signing.rs`: 20 tests
  - deterministic local vector
  - BIP-340 vectors #0..#18 including failure cases and panic-equivalent invalid-key cases
  - one additional `test_verify_tweaked` smoke verification
- `ed25519_signing.rs`: 2 tests
  - deterministic local vector
  - RFC 8032 vectors
- `scrypt.rs`: 4 tests (shape/determinism/different-salt/length)
- `argon.rs`: 2 tests (shape/determinism/different-salt)
- Total Rust unit tests: 44

## Translation Hazards
- ECDSA output must be compact 64-byte `(r||s)` signatures to match Rust vectors.
- Schnorr signing/verification must match Rust `secp256k1` semantics (BIP-340 vectors, aux randomness behavior, invalid key handling).
- `double_sha256` is required for ECDSA signing/verification input hashing.
- X25519 shared secret must be HKDF-HMAC-SHA256-derived with salt `"agreement"`.
- Rust frequently panics (`expect/unwrap`) on invalid key material; Python should raise deterministic exceptions where relevant while keeping positive-path behavior unchanged.
- Preserve deterministic vectors tied to `bc-rand` fake RNG seed.

## Non-default Features
- Not applicable beyond defaults; this translation includes both default-gated modules (`secp256k1`, `ed25519`).

## Completion Criteria
- All default-feature public constants/functions translated and exported.
- Python tests cover Rust test vectors and behavior-equivalent cases.
- `pytest` passes in `python/bc-crypto`.

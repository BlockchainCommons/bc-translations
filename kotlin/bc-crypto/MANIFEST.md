# bc-crypto Translation Manifest (Kotlin)

## Source Crate
- Rust crate: `rust/bc-crypto`
- Version: `0.14.0`
- Default features: `secp256k1`, `ed25519`
- Internal dependency: `bc-rand ^0.5.0`

## Scope
Translate default-feature Rust API and tests into Kotlin under `kotlin/bc-crypto` with equivalent behavior and vectors.

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

## Rust File → Kotlin Unit Plan
1. `src/error.rs` → `BcCryptoException.kt`
2. `src/hash.rs` → `Hash.kt`
3. `src/memzero.rs` → `Memzero.kt`
4. `src/symmetric_encryption.rs` → `SymmetricEncryption.kt`
5. `src/public_key_encryption.rs` → `PublicKeyEncryption.kt`
6. `src/ecdsa_keys.rs` → `EcdsaKeys.kt`
7. `src/ecdsa_signing.rs` → `EcdsaSigning.kt`
8. `src/schnorr_signing.rs` → `SchnorrSigning.kt`
9. `src/ed25519_signing.rs` → `Ed25519Signing.kt`
10. `src/scrypt.rs` → `Scrypt.kt`
11. `src/argon.rs` → `Argon.kt`

## Dependency Mapping (Rust → Kotlin)
- `bc-rand` → local composite build dependency (`com.blockchaincommons:bc-rand:0.5.0`)
- `sha2`, `hmac`, `pbkdf2` → JDK `java.security.MessageDigest`, `javax.crypto.Mac`, Bouncy Castle PKCS5S2
- `hkdf` → Bouncy Castle `HKDFBytesGenerator`
- `crc32fast` → `java.util.zip.CRC32`
- `chacha20poly1305` → JDK `javax.crypto.Cipher("ChaCha20-Poly1305")`
- `x25519-dalek` → Bouncy Castle `X25519PrivateKeyParameters`/`X25519Agreement`
- `secp256k1` (ECDSA + Schnorr) → `fr.acinq.secp256k1:secp256k1-kmp-jni-jvm`
- `ed25519-dalek` → Bouncy Castle `Ed25519PrivateKeyParameters`/`Ed25519Signer`
- `scrypt` → Bouncy Castle `SCrypt`
- `argon2` → Bouncy Castle `Argon2BytesGenerator`

## Test Inventory (Rust)
- `hash.rs`: 6 tests (CRC32, SHA-256, SHA-512, HMAC, PBKDF2, HKDF)
- `symmetric_encryption.rs`: 3 tests (RFC vector, random round-trip, empty data)
- `public_key_encryption.rs`: 2 tests (X25519 keys, key agreement)
- `ecdsa_keys.rs`: 1 test (key generation/derivation vectors)
- `ecdsa_signing.rs`: 1 test (compact signature vector + verify)
- `schnorr_signing.rs`: 20 tests (1 local + 19 BIP-340 vectors)
- `ed25519_signing.rs`: 2 tests (local vector + RFC 8032 vectors)
- `scrypt.rs`: 4 tests (basic, different salt, opt, output length)
- `argon.rs`: 2 tests (basic, different salt)
- Total: 41 tests (excluding 2 Rust-specific metadata tests)

## Translation Hazards
- ECDSA output must be compact 64-byte `(r||s)` signatures to match Rust vectors.
- Schnorr signing/verification must match BIP-340 semantics exactly (same libsecp256k1 C library).
- `double_sha256` is required for ECDSA signing/verification input hashing.
- X25519 shared secret must be HKDF-HMAC-SHA256-derived with salt `"agreement"`.
- JDK ChaCha20-Poly1305 returns ciphertext||tag combined; must split for separate return.
- PBKDF2 must use raw bytes (Bouncy Castle PKCS5S2), not JDK char[]-based API.
- Argon2id defaults: m_cost=19456, t_cost=2, p_cost=1 (must match Rust `Argon2::default()`).
- Scrypt recommended params: log_n=15, r=8, p=1.
- BIP-340 tests 5 and 14 should throw exceptions (invalid public keys).

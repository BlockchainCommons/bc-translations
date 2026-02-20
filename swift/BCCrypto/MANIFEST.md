# bc-crypto Translation Manifest

## Crate Metadata
- Crate: `bc-crypto`
- Version: `0.14.0`
- Rust edition: `2024`
- Rust path: `rust/bc-crypto/`
- Internal BC dependencies:
  - `bc-rand` (`^0.5.0`)
- External dependencies:
  - `rand`
  - `sha2`
  - `hmac`
  - `pbkdf2`
  - `hkdf`
  - `crc32fast`
  - `chacha20poly1305`
  - `secp256k1` (feature-gated, enabled by default)
  - `x25519-dalek`
  - `ed25519-dalek` (feature-gated, enabled by default)
  - `scrypt`
  - `argon2`
  - `thiserror`
  - `hex`

## Feature Flags
- `default = ["secp256k1", "ed25519"]`
- `secp256k1`:
  - Gates: `ecdsa_keys`, `ecdsa_signing`, `schnorr_signing` modules
- `ed25519`:
  - Gates: `ed25519_signing` module
- Translation scope for initial Swift target:
  - Translate default features only (both `secp256k1` and `ed25519` included)

## Public API Surface

### Type Catalog
- `Error`
  - kind: enum
  - variants:
    - `Aead(AeadError)`
- `Result<T>`
  - kind: type alias
  - signature: `std::result::Result<T, Error>`

### Constant Catalog
- `CRC32_SIZE: usize = 4`
- `SHA256_SIZE: usize = 32`
- `SHA512_SIZE: usize = 64`
- `SYMMETRIC_KEY_SIZE: usize = 32`
- `SYMMETRIC_NONCE_SIZE: usize = 12`
- `SYMMETRIC_AUTH_SIZE: usize = 16`
- `X25519_PRIVATE_KEY_SIZE: usize = 32`
- `X25519_PUBLIC_KEY_SIZE: usize = 32`
- `ECDSA_PRIVATE_KEY_SIZE: usize = 32`
- `ECDSA_PUBLIC_KEY_SIZE: usize = 33`
- `ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE: usize = 65`
- `ECDSA_MESSAGE_HASH_SIZE: usize = 32`
- `ECDSA_SIGNATURE_SIZE: usize = 64`
- `SCHNORR_PUBLIC_KEY_SIZE: usize = 32`
- `SCHNORR_SIGNATURE_SIZE: usize = 64`
- `ED25519_PUBLIC_KEY_SIZE: usize = 32`
- `ED25519_PRIVATE_KEY_SIZE: usize = 32`
- `ED25519_SIGNATURE_SIZE: usize = 64`

### Function Catalog

#### hash module (`pub mod hash`)
- `crc32(data: impl AsRef<[u8]>) -> u32`
- `crc32_data_opt(data: impl AsRef<[u8]>, little_endian: bool) -> [u8; 4]`
- `crc32_data(data: impl AsRef<[u8]>) -> [u8; 4]`
- `sha256(data: impl AsRef<[u8]>) -> [u8; 32]`
- `double_sha256(message: &[u8]) -> [u8; 32]`
- `sha512(data: impl AsRef<[u8]>) -> [u8; 64]`
- `hmac_sha256(key: impl AsRef<[u8]>, message: impl AsRef<[u8]>) -> [u8; 32]`
- `hmac_sha512(key: impl AsRef<[u8]>, message: impl AsRef<[u8]>) -> [u8; 64]`
- `pbkdf2_hmac_sha256(pass, salt, iterations: u32, key_len: usize) -> Vec<u8>`
- `pbkdf2_hmac_sha512(pass, salt, iterations: u32, key_len: usize) -> Vec<u8>`
- `hkdf_hmac_sha256(key_material, salt, key_len: usize) -> Vec<u8>`
- `hkdf_hmac_sha512(key_material, salt, key_len: usize) -> Vec<u8>`

#### memory zeroing
- `memzero<T>(s: &mut [T])`
- `memzero_vec_vec_u8(s: &mut [Vec<u8>])`

#### symmetric encryption
- `aead_chacha20_poly1305_encrypt_with_aad(plaintext, key, nonce, aad) -> (Vec<u8>, [u8; 16])`
- `aead_chacha20_poly1305_encrypt(plaintext, key, nonce) -> (Vec<u8>, [u8; 16])`
- `aead_chacha20_poly1305_decrypt_with_aad(ciphertext, key, nonce, aad, auth) -> Result<Vec<u8>>`
- `aead_chacha20_poly1305_decrypt(ciphertext, key, nonce, auth) -> Result<Vec<u8>>`

#### X25519 and key derivation
- `derive_agreement_private_key(key_material) -> [u8; 32]`
- `derive_signing_private_key(key_material) -> [u8; 32]`
- `x25519_new_private_key_using(rng) -> [u8; 32]`
- `x25519_public_key_from_private_key(private_key) -> [u8; 32]`
- `x25519_shared_key(private_key, public_key) -> [u8; 32]`

#### secp256k1 keys/signing (default feature)
- `ecdsa_new_private_key_using(rng) -> [u8; 32]`
- `ecdsa_public_key_from_private_key(private_key) -> [u8; 33]`
- `ecdsa_decompress_public_key(compressed_public_key) -> [u8; 65]`
- `ecdsa_compress_public_key(uncompressed_public_key) -> [u8; 33]`
- `ecdsa_derive_private_key(key_material) -> Vec<u8>`
- `schnorr_public_key_from_private_key(private_key) -> [u8; 32]`
- `ecdsa_sign(private_key, message) -> [u8; 64]`
- `ecdsa_verify(public_key, signature, message) -> bool`
- `schnorr_sign(ecdsa_private_key, message) -> [u8; 64]`
- `schnorr_sign_using(ecdsa_private_key, message, rng) -> [u8; 64]`
- `schnorr_sign_with_aux_rand(ecdsa_private_key, message, aux_rand) -> [u8; 64]`
- `schnorr_verify(schnorr_public_key, schnorr_signature, message) -> bool`

#### ed25519 signing (default feature)
- `ed25519_new_private_key_using(rng) -> [u8; 32]`
- `ed25519_public_key_from_private_key(private_key) -> [u8; 32]`
- `ed25519_sign(private_key, message) -> [u8; 64]`
- `ed25519_verify(public_key, message, signature) -> bool`

#### password KDFs
- `scrypt(pass, salt, output_len) -> Vec<u8>`
- `scrypt_opt(pass, salt, output_len, log_n, r, p) -> Vec<u8>`
- `argon2id(pass, salt, output_len) -> Vec<u8>`

## External Dependency Equivalents (Swift)
- `sha2`, `hmac`, `hkdf`, `chacha20poly1305`, `x25519-dalek`, `ed25519-dalek`:
  - Preferred: `CryptoKit` / `swift-crypto` API surface
- `pbkdf2`, `scrypt`, `crc32fast`:
  - Recommended: `CryptoSwift`
- `secp256k1` + BIP340 Schnorr:
  - Recommended: `swift-secp256k1` (`P256K` target)
- `argon2`:
  - Recommended: Argon2id-capable Swift/C binding (or equivalent wrapper)
- `thiserror`:
  - Swift `Error` enum

## Test Inventory
- `src/lib.rs`
  - `test_readme_deps` (version/doc sync; Rust-only, no Swift equivalent)
  - `test_html_root_url` (Rust-only, no Swift equivalent)
- `src/hash.rs`
  - `test_crc32` (CRC32 big/little-endian vectors)
  - `test_sha256` (SHA-256 vector)
  - `test_sha512` (SHA-512 vector)
  - `test_hmac_sha` (HMAC-SHA-256 + HMAC-SHA-512 vectors)
  - `test_pbkdf2_hmac_sha256` (PBKDF2 vector)
  - `test_hkdf_hmac_sha256` (HKDF vector)
- `src/symmetric_encryption.rs`
  - `test_rfc_test_vector` (ChaCha20-Poly1305 RFC vector)
  - `test_random_key_and_nonce` (round-trip with deterministic RNG)
  - `test_empty_data` (empty payload round-trip)
- `src/public_key_encryption.rs`
  - `test_x25519_keys` (deterministic private/public + derived-key vectors)
  - `test_key_agreement` (Alice/Bob shared secret equality + vector)
- `src/ecdsa_keys.rs`
  - `test_ecdsa_keys` (private/public/compression/x-only/derived key vectors)
- `src/ecdsa_signing.rs`
  - `test_ecdsa_signing` (double-SHA256 + compact signature vector)
- `src/schnorr_signing.rs`
  - `test_schnorr_sign` (deterministic RNG signature vector)
  - `test_0` ... `test_18` (BIP340 vectors)
  - `test_5` and `test_14` are panic-path malformed public key cases
  - `test_verify_tweaked` (smoke verification call; no assertion)
- `src/ed25519_signing.rs`
  - `test_ed25519_signing` (deterministic key/signature vector)
  - `test_ed25519_vectors` (RFC8032 vectors, verify + public-key derivation)
- `src/scrypt.rs`
  - `test_scrypt_basic`
  - `test_scrypt_different_salt`
  - `test_scrypt_opt_basic`
  - `test_scrypt_output_length`
- `src/argon.rs`
  - `test_argon2id_basic`
  - `test_argon2id_different_salt`

## Translation Hazards
- Rust fixed-size arrays (`[u8; N]`) map to `Data`/`[UInt8]` with runtime length checks in Swift.
- `impl AsRef<[u8]>` in Rust implies wide input flexibility; Swift API should accept `Data` (plus helpers) to preserve ergonomics.
- `Result`-based error on AEAD decrypt maps to `throws` in Swift.
- secp256k1 behavior includes strict parsing and panic paths in tests (`test_5`, `test_14`); Swift should represent malformed-key behavior explicitly (throw or deterministic failure path).
- Deterministic test vectors rely on `bc-rand` fake RNG byte sequence; must use `BCRand.makeFakeRandomNumberGenerator()`.
- ECDSA signs `double_sha256(message)`, not single hash.
- Schnorr signing API signs raw message bytes (variable length) in this crate; do not force 32-byte input.
- Domain separation salts (`"agreement"`, `"signing"`) are consensus-relevant test vectors.
- `memzero` uses unsafe volatile writes in Rust; Swift equivalent should avoid optimizer elision.
- Rust `version-sync` tests are package-metadata checks and should be omitted/replaced with Swift package metadata checks.

## Translation Unit Order
1. `error` + base aliases
2. hash primitives and constants
3. memzero utilities
4. symmetric encryption
5. X25519 and key derivation
6. secp256k1 key utilities
7. ECDSA signing
8. Schnorr signing
9. Ed25519 signing
10. scrypt
11. argon2id
12. test translation (vector-first)

## Expected Coverage Targets
- API coverage: 100% of public constants/functions/types above
- Test translation: all crypto behavior tests translated; Rust-only metadata tests replaced with Swift package checks
- Vectors: byte-for-byte identical outputs for hash/HMAC/HKDF/PBKDF2/AEAD/X25519/ECDSA/Schnorr/Ed25519 vectors

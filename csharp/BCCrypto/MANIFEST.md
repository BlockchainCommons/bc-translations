# bc-crypto Translation Manifest (C#)

## Crate Metadata
- Crate: `bc-crypto`
- Version: `0.14.0`
- Rust edition: `2024`
- Rust path: `rust/bc-crypto/`
- Internal BC dependencies:
  - `bc-rand` (`^0.5.0`) → `BlockchainCommons.BCRand` (project reference)
- External dependencies:
  - `rand`, `sha2`, `hmac`, `pbkdf2`, `hkdf`, `crc32fast`, `chacha20poly1305`,
    `secp256k1` (default), `x25519-dalek`, `ed25519-dalek` (default),
    `scrypt`, `argon2`, `thiserror`, `hex`

## Feature Flags
- `default = ["secp256k1", "ed25519"]`
- `secp256k1`: gates `ecdsa_keys`, `ecdsa_signing`, `schnorr_signing` modules
- `ed25519`: gates `ed25519_signing` module
- Translation scope: default features only (both `secp256k1` and `ed25519` included as always-on)

## Public API Surface

### Type Catalog
- `Error`
  - kind: enum
  - variants: `Aead(AeadError)`
  - C# mapping: `BCCryptoException` (or nested exception types)

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
- `hmac_sha256(key, message) -> [u8; 32]`
- `hmac_sha512(key, message) -> [u8; 64]`
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

#### secp256k1 keys (default feature)
- `ecdsa_new_private_key_using(rng) -> [u8; 32]`
- `ecdsa_public_key_from_private_key(private_key) -> [u8; 33]`
- `ecdsa_decompress_public_key(compressed) -> [u8; 65]`
- `ecdsa_compress_public_key(uncompressed) -> [u8; 33]`
- `ecdsa_derive_private_key(key_material) -> Vec<u8>`
- `schnorr_public_key_from_private_key(private_key) -> [u8; 32]`

#### ECDSA signing (default feature)
- `ecdsa_sign(private_key, message) -> [u8; 64]`
- `ecdsa_verify(public_key, signature, message) -> bool`

#### Schnorr signing (default feature)
- `schnorr_sign(ecdsa_private_key, message) -> [u8; 64]`
- `schnorr_sign_using(ecdsa_private_key, message, rng) -> [u8; 64]`
- `schnorr_sign_with_aux_rand(ecdsa_private_key, message, aux_rand) -> [u8; 64]`
- `schnorr_verify(schnorr_public_key, signature, message) -> bool`

#### Ed25519 signing (default feature)
- `ed25519_new_private_key_using(rng: CryptoRngCore) -> [u8; 32]`
- `ed25519_public_key_from_private_key(private_key) -> [u8; 32]`
- `ed25519_sign(private_key, message) -> [u8; 64]`
- `ed25519_verify(public_key, message, signature) -> bool`

#### password KDFs
- `scrypt(pass, salt, output_len) -> Vec<u8>`
- `scrypt_opt(pass, salt, output_len, log_n, r, p) -> Vec<u8>`
- `argon2id(pass, salt, output_len) -> Vec<u8>`

## Documentation Catalog
- Crate-level doc comment: yes (algorithm/provider table in `lib.rs`)
- Module-level docs: `hash` module has `///` comment
- Most public functions have `///` doc comments
- Package metadata description: "A uniform API for cryptographic primitives used in Blockchain Commons projects"

## Dependency Mapping (Rust → C#)

### .NET 10 Stdlib (`System.Security.Cryptography`)
| Rust crate | C# equivalent |
|---|---|
| `sha2` (SHA-256/512) | `SHA256.HashData()`, `SHA512.HashData()` |
| `hmac` (HMAC-SHA256/512) | `HMACSHA256`, `HMACSHA512` |
| `pbkdf2` | `Rfc2898DeriveBytes.Pbkdf2()` |
| `hkdf` | `HKDF.DeriveKey()` / `HKDF.Expand()` |
| `chacha20poly1305` | `ChaCha20Poly1305` class |
| `crc32fast` | `System.IO.Hashing.Crc32` |
| `thiserror` | C# exceptions (no equivalent needed) |
| `hex` | `Convert.ToHexString()` / `Convert.FromHexString()` |

### NuGet Packages Required
| Rust crate | NuGet package | Purpose |
|---|---|---|
| `secp256k1` | `NBitcoin.Secp256k1` | ECDSA sign/verify, Schnorr BIP340, key operations |
| `x25519-dalek` | `NSec.Cryptography` | X25519 key agreement |
| `ed25519-dalek` | `NSec.Cryptography` | Ed25519 sign/verify |
| `scrypt` | `CryptSharpOfficial` or `BouncyCastle.Cryptography` | Scrypt KDF |
| `argon2` | `Konscious.Security.Cryptography.Argon2` | Argon2id KDF |

### Internal BC dependency
| Rust crate | C# project reference |
|---|---|
| `bc-rand` | `BlockchainCommons.BCRand` (project ref to `../BCRand/BCRand/BCRand.csproj`) |

## C# File → Rust Source Mapping

| # | Rust source | C# target file |
|---|---|---|
| 1 | `src/error.rs` | `BCCrypto/BCCryptoException.cs` |
| 2 | `src/hash.rs` | `BCCrypto/Hash.cs` |
| 3 | `src/memzero.rs` | `BCCrypto/Memzero.cs` |
| 4 | `src/symmetric_encryption.rs` | `BCCrypto/SymmetricEncryption.cs` |
| 5 | `src/public_key_encryption.rs` | `BCCrypto/PublicKeyEncryption.cs` |
| 6 | `src/ecdsa_keys.rs` | `BCCrypto/EcdsaKeys.cs` |
| 7 | `src/ecdsa_signing.rs` | `BCCrypto/EcdsaSigning.cs` |
| 8 | `src/schnorr_signing.rs` | `BCCrypto/SchnorrSigning.cs` |
| 9 | `src/ed25519_signing.rs` | `BCCrypto/Ed25519Signing.cs` |
| 10 | `src/scrypt.rs` | `BCCrypto/Scrypt.cs` |
| 11 | `src/argon.rs` | `BCCrypto/Argon.cs` |

## Test Inventory

| Rust test | Rust file | Test vectors | Uses fake RNG |
|---|---|---|---|
| `test_crc32` | `hash.rs` | CRC32 big/little-endian | No |
| `test_sha256` | `hash.rs` | SHA-256 | No |
| `test_sha512` | `hash.rs` | SHA-512 | No |
| `test_hmac_sha` | `hash.rs` | HMAC-SHA256 + HMAC-SHA512 | No |
| `test_pbkdf2_hmac_sha256` | `hash.rs` | PBKDF2 | No |
| `test_hkdf_hmac_sha256` | `hash.rs` | HKDF | No |
| `test_rfc_test_vector` | `symmetric_encryption.rs` | ChaCha20-Poly1305 RFC vector | No |
| `test_random_key_and_nonce` | `symmetric_encryption.rs` | Round-trip | Yes (bc_rand::random_data) |
| `test_empty_data` | `symmetric_encryption.rs` | Empty payload round-trip | Yes |
| `test_x25519_keys` | `public_key_encryption.rs` | X25519 private/public + derived | Yes (make_fake) |
| `test_key_agreement` | `public_key_encryption.rs` | Alice/Bob shared secret | Yes (make_fake) |
| `test_ecdsa_keys` | `ecdsa_keys.rs` | Private/public/compress/x-only/derived | Yes (make_fake) |
| `test_ecdsa_signing` | `ecdsa_signing.rs` | Double-SHA256 compact signature | Yes (make_fake) |
| `test_schnorr_sign` | `schnorr_signing.rs` | Deterministic RNG signature | Yes (make_fake) |
| `test_0` to `test_18` | `schnorr_signing.rs` | BIP-340 official vectors | No |
| `test_5` | `schnorr_signing.rs` | Panic: invalid public key | No |
| `test_14` | `schnorr_signing.rs` | Panic: field-size public key | No |
| `test_verify_tweaked` | `schnorr_signing.rs` | Smoke verification (no assertion) | No |
| `test_ed25519_signing` | `ed25519_signing.rs` | Deterministic key/signature | Yes (fake_random_data) |
| `test_ed25519_vectors` | `ed25519_signing.rs` | RFC 8032 vectors | No |
| `test_scrypt_basic` | `scrypt.rs` | Determinism check | No |
| `test_scrypt_different_salt` | `scrypt.rs` | Different salt ≠ output | No |
| `test_scrypt_opt_basic` | `scrypt.rs` | Custom params | No |
| `test_scrypt_output_length` | `scrypt.rs` | Various output lengths | No |
| `test_argon2id_basic` | `argon.rs` | Determinism check | No |
| `test_argon2id_different_salt` | `argon.rs` | Different salt ≠ output | No |

- Rust-only metadata tests (`test_readme_deps`, `test_html_root_url`) → omit
- Total Rust behavior tests to translate: **~42** (counting BIP-340 test_0..test_18 as 19)

## Translation Hazards

### API Design
- Rust `impl AsRef<[u8]>` → C# `ReadOnlySpan<byte>` or `byte[]`; prefer `ReadOnlySpan<byte>` for public API with `byte[]` overloads where ergonomic
- Rust returns fixed-size arrays `[u8; N]` → C# returns `byte[]` (no stack-allocated fixed-size return); document expected lengths via constants
- Rust `Result<T>` → C# `throws BCCryptoException` for AEAD decrypt; other errors throw `ArgumentException` or `CryptographicException`
- Rust tuple return `(Vec<u8>, [u8; 16])` → C# use `out byte[] tag` parameter or return a named tuple `(byte[] Ciphertext, byte[] Tag)`

### Crypto-specific
- ECDSA output must be compact 64-byte `(r||s)` signatures — verify NBitcoin.Secp256k1 produces this format
- ECDSA signs `double_sha256(message)`, not the raw message
- Schnorr signing uses raw message bytes (variable length); do NOT force 32-byte input
- Domain separation salts `"agreement"` and `"signing"` are consensus-relevant constants
- X25519 shared secret is HKDF-HMAC-SHA256-derived with salt `"agreement"`
- `test_5` and `test_14` (Schnorr) are panic paths for malformed public keys → C# should throw or assert

### NSec.Cryptography key handling
- NSec uses strongly-typed `Key` objects, not raw byte arrays
- Must use `Key.Import()` to create keys from raw bytes and `Key.Export()` to extract raw bytes
- NSec `X25519` uses `KeyAgreementAlgorithm.X25519` — the shared secret is the raw Diffie-Hellman output; we must then apply HKDF ourselves
- NSec `Ed25519` uses `SignatureAlgorithm.Ed25519` — verify it uses the standard seed-based key (32-byte private key → signing key)

### Ed25519 key generation
- Rust `ed25519_new_private_key_using` takes `CryptoRngCore`, not `RandomNumberGenerator`
- In Rust, `SigningKey::generate(rng)` generates a random 32-byte seed
- C# translation should generate 32 random bytes from `IRandomNumberGenerator.RandomData(32)` since NSec's key generation uses its own RNG
- Test uses `fake_random_data(32)` directly as the private key bytes

### memzero
- Rust uses `ptr::write_volatile` to defeat optimizer
- C# should use `CryptographicOperations.ZeroMemory()` (.NET 6+) or `Array.Clear()` + volatile pattern
- `CryptographicOperations.ZeroMemory(Span<byte>)` is the idiomatic .NET equivalent

### Test infrastructure
- `make_fake_random_number_generator()` → `SeededRandomNumberGenerator.CreateFake()` from BCRand
- `fake_random_data(N)` → `SeededRandomNumberGenerator.FakeRandomData(N)` from BCRand
- `bc_rand::random_data(N)` (secure) → `SecureRandomNumberGenerator.Shared.RandomData(N)` from BCRand

## Translation Unit Order
1. Error type (`BCCryptoException`)
2. Constants (as `public const int` in relevant classes or a shared `Constants` class)
3. Hash functions (`Hash.cs`)
4. Memzero utilities (`Memzero.cs`)
5. Symmetric encryption (`SymmetricEncryption.cs`)
6. Public key encryption / X25519 (`PublicKeyEncryption.cs`)
7. ECDSA key utilities (`EcdsaKeys.cs`)
8. ECDSA signing (`EcdsaSigning.cs`)
9. Schnorr signing (`SchnorrSigning.cs`)
10. Ed25519 signing (`Ed25519Signing.cs`)
11. Scrypt (`Scrypt.cs`)
12. Argon2id (`Argon.cs`)
13. Tests (vector-first)

## Project Structure
```
csharp/BCCrypto/
├── BCCrypto.slnx
├── BCCrypto/
│   ├── BCCrypto.csproj
│   ├── BCCryptoException.cs
│   ├── Hash.cs
│   ├── Memzero.cs
│   ├── SymmetricEncryption.cs
│   ├── PublicKeyEncryption.cs
│   ├── EcdsaKeys.cs
│   ├── EcdsaSigning.cs
│   ├── SchnorrSigning.cs
│   ├── Ed25519Signing.cs
│   ├── Scrypt.cs
│   └── Argon.cs
├── BCCrypto.Tests/
│   ├── BCCrypto.Tests.csproj
│   ├── HashTests.cs
│   ├── SymmetricEncryptionTests.cs
│   ├── PublicKeyEncryptionTests.cs
│   ├── EcdsaKeysTests.cs
│   ├── EcdsaSigningTests.cs
│   ├── SchnorrSigningTests.cs
│   ├── Ed25519SigningTests.cs
│   ├── ScryptTests.cs
│   └── ArgonTests.cs
├── .gitignore
├── LOG.md
└── MANIFEST.md
```

## Expected Coverage Targets
- API coverage: 100% of public constants/functions/types
- Test translation: all crypto behavior tests; Rust-only metadata tests omitted
- Vectors: byte-for-byte identical outputs for hash/HMAC/HKDF/PBKDF2/AEAD/X25519/ECDSA/Schnorr/Ed25519 vectors

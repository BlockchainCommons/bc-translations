# Translation Manifest: bc-components 0.31.1 → Kotlin

## Crate Overview
Collection of cryptographic primitives, identifiers, and serialization types for Blockchain Commons. All types are CBOR-serializable; many are UR-encodable. Default features: secp256k1, ed25519, pqcrypto, ssh.

## External Dependencies

| Rust Crate | Kotlin Equivalent |
|---|---|
| bc-rand | `com.blockchaincommons:bc-rand:0.5.0` (sibling) |
| bc-crypto | `com.blockchaincommons:bc-crypto:0.14.0` (sibling) |
| dcbor | `com.blockchaincommons:dcbor:0.25.1` (sibling) |
| bc-tags | `com.blockchaincommons:bc-tags:0.12.0` (sibling) |
| bc-ur | `com.blockchaincommons:bc-ur:0.19.0` (sibling) |
| sskr | `com.blockchaincommons:sskr:0.12.0` (sibling) |
| hex | Kotlin stdlib `String.hexToByteArray()` / `ByteArray.toHexString()` |
| miniz_oxide | `java.util.zip.Deflater` / `java.util.zip.Inflater` |
| url | `java.net.URI` |
| zeroize | `ByteArray.fill(0)` |
| rand_core | `com.blockchaincommons.bcrand.RandomNumberGenerator` |
| thiserror | Kotlin sealed class hierarchy |
| pqcrypto-mlkem | `org.bouncycastle:bcprov-jdk18on:1.79` (ML-KEM) |
| pqcrypto-mldsa | `org.bouncycastle:bcprov-jdk18on:1.79` (ML-DSA) |
| ssh-key | Manual OpenSSH format implementation + Bouncy Castle |
| ssh-agent-client-rs | NOT TRANSLATED (optional feature, not default) |

## Feature Mapping

| Rust Feature | Kotlin Approach |
|---|---|
| secp256k1 | Always enabled (bc-crypto has secp256k1 support) |
| ed25519 | Always enabled (bc-crypto has ed25519 support) |
| pqcrypto | Always enabled (Bouncy Castle ML-KEM/ML-DSA) |
| ssh | Always enabled (manual OpenSSH format + Bouncy Castle) |
| ssh-agent | NOT TRANSLATED (optional, not default) |
| ssh-agent-tests | NOT TRANSLATED |

## Translation Units (Dependency Order)

### Unit 1: Error Types
- `error.rs` → `BcComponentsException.kt`
- Sealed class hierarchy mirroring Rust Error enum
- Factory methods for each variant

### Unit 2: Core Value Types
- `digest.rs` → `Digest.kt` (32-byte SHA-256 hash)
- `digest_provider.rs` → `DigestProvider.kt` (interface)
- `nonce.rs` → `Nonce.kt` (12-byte nonce)
- `salt.rs` → `Salt.kt` (variable-length salt)
- `compressed.rs` → `Compressed.kt` (DEFLATE with CRC32)
- `json.rs` → `Json.kt` (UTF-8 JSON wrapper, renamed to avoid stdlib collision)
- `reference.rs` → `Reference.kt`, `ReferenceProvider.kt` (32-byte ref)
- `seed.rs` → `Seed.kt` (master seed with metadata)

### Unit 3: Symmetric Cryptography
- `symmetric/symmetric_key.rs` → `SymmetricKey.kt`
- `symmetric/authentication_tag.rs` → `AuthenticationTag.kt`
- `symmetric/encrypted_message.rs` → `EncryptedMessage.kt`

### Unit 4: X25519 Key Agreement
- `x25519/x25519_private_key.rs` → `X25519PrivateKey.kt`
- `x25519/x25519_public_key.rs` → `X25519PublicKey.kt`

### Unit 5: Ed25519 Signing
- `ed25519/ed25519_private_key.rs` → `Ed25519PrivateKey.kt`
- `ed25519/ed25519_public_key.rs` → `Ed25519PublicKey.kt`

### Unit 6: EC Key (secp256k1)
- `ec_key/ec_key_base.rs` → `ECKeyBase.kt` (interface)
- `ec_key/ec_private_key.rs` → `ECPrivateKey.kt`
- `ec_key/ec_public_key_base.rs` → `ECPublicKeyBase.kt` (interface)
- `ec_key/ec_public_key.rs` → `ECPublicKey.kt` (33-byte compressed)
- `ec_key/ec_uncompressed_public_key.rs` → `ECUncompressedPublicKey.kt`
- `ec_key/schnorr_public_key.rs` → `SchnorrPublicKey.kt`

### Unit 7: Signing Framework
- `signing/signature_scheme.rs` → `SignatureScheme.kt`
- `signing/signature.rs` → `Signature.kt`
- `signing/signing_private_key.rs` → `SigningPrivateKey.kt`
- `signing/signing_public_key.rs` → `SigningPublicKey.kt`
- `signing/signer.rs` → `Signer.kt`, `Verifier.kt`

### Unit 8: Post-Quantum ML-DSA
- `mldsa/mldsa_level.rs` → `MLDSA.kt`
- `mldsa/mldsa_private_key.rs` → `MLDSAPrivateKey.kt`
- `mldsa/mldsa_public_key.rs` → `MLDSAPublicKey.kt`
- `mldsa/mldsa_signature.rs` → `MLDSASignature.kt`

### Unit 9: Post-Quantum ML-KEM
- `mlkem/mlkem_level.rs` → `MLKEM.kt`
- `mlkem/mlkem_private_key.rs` → `MLKEMPrivateKey.kt`
- `mlkem/mlkem_public_key.rs` → `MLKEMPublicKey.kt`
- `mlkem/mlkem_ciphertext.rs` → `MLKEMCiphertext.kt`

### Unit 10: Encapsulation Framework
- `encapsulation/encapsulation_scheme.rs` → `EncapsulationScheme.kt`
- `encapsulation/encapsulation_private_key.rs` → `EncapsulationPrivateKey.kt`
- `encapsulation/encapsulation_public_key.rs` → `EncapsulationPublicKey.kt`
- `encapsulation/encapsulation_ciphertext.rs` → `EncapsulationCiphertext.kt`
- `encapsulation/sealed_message.rs` → `SealedMessage.kt`
- `encrypter.rs` → `Encrypter.kt`, `Decrypter.kt`

### Unit 11: Key Encryption
- `encrypted_key/hash_type.rs` → `HashType.kt`
- `encrypted_key/key_derivation.rs` → `KeyDerivation.kt` (interface)
- `encrypted_key/key_derivation_method.rs` → `KeyDerivationMethod.kt`
- `encrypted_key/hkdf_params.rs` → `HKDFParams.kt`
- `encrypted_key/pbkdf2_params.rs` → `PBKDF2Params.kt`
- `encrypted_key/scrypt_params.rs` → `ScryptParams.kt`
- `encrypted_key/argon2id_params.rs` → `Argon2idParams.kt`
- `encrypted_key/key_derivation_params.rs` → `KeyDerivationParams.kt`
- `encrypted_key/encrypted_key_impl.rs` → `EncryptedKey.kt`
- `encrypted_key/ssh_agent_params.rs` → NOT TRANSLATED (ssh-agent feature)

### Unit 12: SSKR Wrapper
- `sskr_mod.rs` → `SSKRShare.kt`, `Sskr.kt` (wrapper functions)

### Unit 13: Key Management
- `private_key_data_provider.rs` → `PrivateKeyDataProvider.kt`
- `hkdf_rng.rs` → `HKDFRng.kt`
- `private_key_base.rs` → `PrivateKeyBase.kt`
- `private_keys.rs` → `PrivateKeys.kt`, `PrivateKeysProvider.kt`
- `public_keys.rs` → `PublicKeys.kt`, `PublicKeysProvider.kt`
- `keypair.rs` → `Keypair.kt`

### Unit 14: Identifier Types
- `id/arid.rs` → `ARID.kt`
- `id/uri.rs` → `URI.kt`
- `id/uuid.rs` → `UUID.kt`
- `id/xid.rs` → `XID.kt`, `XIDProvider.kt`

### Unit 15: Tags Registry
- `tags_registry.rs` → `TagsRegistry.kt`

## CBOR Tag Numbers (from bc-tags)

| Tag | Value | Name | Type |
|---|---|---|---|
| TAG_DIGEST | 40001 | digest | Digest |
| TAG_ENCRYPTED | 40002 | encrypted | EncryptedMessage |
| TAG_COMPRESSED | 40003 | compressed | Compressed |
| TAG_X25519_PRIVATE_KEY | 40010 | agreement-private-key | X25519PrivateKey |
| TAG_X25519_PUBLIC_KEY | 40011 | agreement-public-key | X25519PublicKey |
| TAG_ARID | 40012 | arid | ARID |
| TAG_PRIVATE_KEYS | 40013 | crypto-prvkeys | PrivateKeys |
| TAG_NONCE | 40014 | nonce | Nonce |
| TAG_PRIVATE_KEY_BASE | 40016 | crypto-prvkey-base | PrivateKeyBase |
| TAG_PUBLIC_KEYS | 40017 | crypto-pubkeys | PublicKeys |
| TAG_SALT | 40018 | salt | Salt |
| TAG_SEALED_MESSAGE | 40019 | crypto-sealed | SealedMessage |
| TAG_SIGNATURE | 40020 | signature | Signature |
| TAG_SIGNING_PRIVATE_KEY | 40021 | signing-private-key | SigningPrivateKey |
| TAG_SIGNING_PUBLIC_KEY | 40022 | signing-public-key | SigningPublicKey |
| TAG_SYMMETRIC_KEY | 40023 | crypto-key | SymmetricKey |
| TAG_XID | 40024 | xid | XID |
| TAG_REFERENCE | 40025 | reference | Reference |
| TAG_ENCRYPTED_KEY | 40027 | encrypted-key | EncryptedKey |
| TAG_MLKEM_PRIVATE_KEY | 40100 | mlkem-private-key | MLKEMPrivateKey |
| TAG_MLKEM_PUBLIC_KEY | 40101 | mlkem-public-key | MLKEMPublicKey |
| TAG_MLKEM_CIPHERTEXT | 40102 | mlkem-ciphertext | MLKEMCiphertext |
| TAG_MLDSA_PRIVATE_KEY | 40103 | mldsa-private-key | MLDSAPrivateKey |
| TAG_MLDSA_PUBLIC_KEY | 40104 | mldsa-public-key | MLDSAPublicKey |
| TAG_MLDSA_SIGNATURE | 40105 | mldsa-signature | MLDSASignature |
| TAG_SEED | 40300 | seed | Seed |
| TAG_EC_KEY | 40306 | eckey | ECPrivateKey |
| TAG_SSKR_SHARE | 40309 | sskr | SSKRShare |
| TAG_SSH_TEXT_PRIVATE_KEY | 40800 | ssh-private | SSH key text |
| TAG_SSH_TEXT_PUBLIC_KEY | 40801 | ssh-public | SSH key text |
| TAG_SSH_TEXT_SIGNATURE | 40802 | ssh-signature | SSH signature |
| TAG_URI | 32 | url | URI |
| TAG_UUID | 37 | uuid | UUID |
| TAG_JSON | 262 | json | JSON |
| TAG_SEED_V1 | 300 | - | Legacy Seed |
| TAG_EC_KEY_V1 | 306 | - | Legacy ECKey |
| TAG_SSKR_SHARE_V1 | 309 | - | Legacy SSKR |

## Test Inventory

### Inline Tests (per module)
- digest.rs: 8 tests (from_image, from_hex, cbor roundtrip, ur roundtrip, display, validation)
- compressed.rs: 4 tests (compress/decompress, cbor roundtrip, empty data, compression ratio)
- nonce.rs: 9 tests (creation, cbor roundtrip, uniqueness, ur roundtrip)
- json.rs: 8 tests (from_string, cbor roundtrip, ur roundtrip, display)
- hkdf_rng.rs: 6 tests (determinism, page fill, next_u32/u64, fill_bytes)
- private_key_base.rs: 1 test (secp256k1 key derivation)
- private_keys.rs: 1 test (keypair roundtrip)
- public_keys.rs: 1 test (public key derivation)
- signing/mod.rs: ~10 tests (per-scheme signing, CBOR, UR)
- ec_key/: ~5 tests (key derivation, CBOR, UR)
- ed25519/: ~4 tests (key gen, sign/verify)
- x25519/: ~4 tests (key gen, agreement, CBOR)
- symmetric/: ~3 tests (encrypt/decrypt, RFC-8439 vector)
- encapsulation/: ~4 tests (sealed message roundtrip per scheme)
- mldsa/: ~3 tests (per-level signing)
- mlkem/: ~3 tests (per-level encapsulation)
- encrypted_key/: ~5 tests (per-method roundtrip)
- id/arid.rs: ~3 tests (creation, cbor, ur)
- id/uuid.rs: ~3 tests (creation, format, cbor)
- id/uri.rs: ~2 tests (creation, cbor)
- id/xid.rs: ~5 tests (derivation, validation, bytewords, cbor, ur)
- sskr_mod.rs: ~2 tests (generate, combine, cbor)

### lib.rs Integration Tests
- test_x25519_keys: X25519 key creation and UR serialization
- test_agreement: X25519 key agreement
- test_ecdsa_signing_keys: ECDSA/Schnorr key creation and UR (secp256k1)
- test_ecdsa_signing: ECDSA and Schnorr sign/verify (secp256k1)
- test_ssh_dsa_signing: SSH DSA key gen and signing (ssh)
- test_ssh_ed25519_signing: SSH Ed25519 key gen and signing (ssh)
- test_ssh_dsa_nistp256_signing: ECDSA P-256 SSH (ssh, ignored)
- test_ssh_dsa_nistp384_signing: ECDSA P-384 SSH (ssh, ignored)
- test_ssh_dsa_nistp521_signing: ECDSA P-521 SSH (ssh, ignored)

## Translation Hazards

### H1: SSH Key Generation (High Risk)
The SSH feature requires generating SSH keys from deterministic HKDFRng that produce identical OpenSSH format output. The Rust `ssh-key` crate's `PrivateKey::random()` implementation determines the exact byte layout. For Ed25519 this is straightforward (32-byte seed). For DSA, the parameter generation algorithm must match exactly. Use Bouncy Castle's DSA parameter generator with the same HKDFRng bytes.

### H2: SSH Signature Format (Medium Risk)
SSH signing uses the `sshsig` format (OpenSSH 8.0+). Must implement the sshsig envelope format for signing arbitrary data with namespace and hash algorithm.

### H3: Post-Quantum Key Sizes (Low Risk)
Bouncy Castle's ML-KEM and ML-DSA implementations should produce compatible key/ciphertext sizes, but byte-level compatibility with pqcrypto Rust crate needs verification.

### H4: DEFLATE Compression Compatibility (Medium Risk)
The Rust crate uses miniz_oxide for DEFLATE. Java's `Deflater` with `nowrap=true` should produce compatible output, but compression parameters must match (level, window bits).

### H5: Bytewords/Bytemoji (Low Risk)
`Reference.bytewords_identifier()` and `bytemoji_identifier()` depend on bc-ur Bytewords support. Verify Kotlin bc-ur provides these.

### H6: SSH Agent (Not Applicable)
ssh-agent feature is NOT default and will NOT be translated. The SSHAgentParams type will be omitted.

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: yes

Source signals: The Rust tests include expected OpenSSH private/public key strings (PEM format), UR strings, hex strings, and diagnostic CBOR output. The SSH tests use `indoc!` macro for multi-line expected output.

Target test areas:
- SSH key generation tests: Expected OpenSSH format strings
- UR encoding tests: Expected UR strings
- CBOR diagnostic tests: Expected diagnostic output
- XID tests: Expected hex, bytewords, bytemoji identifier strings

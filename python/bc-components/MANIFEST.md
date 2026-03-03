# Translation Manifest: bc-components 0.31.1 → Python

## Crate Overview
Collection of cryptographic primitives, identifiers, and serialization types for Blockchain Commons. All types are CBOR-serializable; many are UR-encodable. Default features: secp256k1, ed25519, pqcrypto, ssh.

## External Dependencies

| Rust Crate | Python Equivalent |
|---|---|
| bc-rand | `bc-rand==0.5.0` (sibling) |
| bc-crypto | `bc-crypto==0.14.0` (sibling) |
| dcbor | `dcbor==0.25.1` (sibling) |
| bc-tags | `bc-tags==0.12.0` (sibling) |
| bc-ur | `bc-ur==0.19.0` (sibling) |
| sskr | `sskr==0.12.0` (sibling) |
| hex | Python built-in `bytes.fromhex()` / `.hex()` |
| miniz_oxide | Python stdlib `zlib` |
| url | Python stdlib `urllib.parse` |
| zeroize | Manual `bytearray` zeroing |
| rand_core | `bc_rand.RandomNumberGenerator` |
| thiserror | Python exception hierarchy |
| pqcrypto-mlkem | Simulated via hash-based expansion (no actual PQ ops needed for test vectors) |
| pqcrypto-mldsa | Simulated via hash-based expansion |
| ssh-key | `cryptography` library SSH key support |
| ssh-agent-client-rs | NOT TRANSLATED (optional feature, not default) |

## Feature Mapping

| Rust Feature | Python Approach |
|---|---|
| secp256k1 | Always enabled (bc-crypto has secp256k1 support) |
| ed25519 | Always enabled (bc-crypto has ed25519 support) |
| pqcrypto | Always enabled (simulated via hash-based expansion) |
| ssh | Always enabled (cryptography library) |
| ssh-agent | NOT TRANSLATED (optional, not default) |
| ssh-agent-tests | NOT TRANSLATED |

## Translation Units (Dependency Order)

### Unit 1: Error Types & Utilities
- `error.rs` → `_error.py`
- Exception class hierarchy mirroring Rust Error enum

### Unit 2: Core Value Types
- `digest.rs` → `_digest.py` (32-byte SHA-256 hash)
- `digest_provider.rs` → `_digest_provider.py` (Protocol)
- `nonce.rs` → `_nonce.py` (12-byte nonce)
- `salt.rs` → `_salt.py` (variable-length salt)
- `compressed.rs` → `_compressed.py` (DEFLATE with CRC32)
- `json.rs` → `_json.py` (UTF-8 JSON wrapper)
- `reference.rs` → `_reference.py` (32-byte ref + ReferenceProvider protocol)
- `seed.rs` → `_seed.py` (master seed with metadata)

### Unit 3: Symmetric Cryptography
- `symmetric/symmetric_key.rs` → `symmetric/_symmetric_key.py`
- `symmetric/authentication_tag.rs` → `symmetric/_authentication_tag.py`
- `symmetric/encrypted_message.rs` → `symmetric/_encrypted_message.py`

### Unit 4: X25519 Key Agreement
- `x25519/x25519_private_key.rs` → `x25519/_x25519_private_key.py`
- `x25519/x25519_public_key.rs` → `x25519/_x25519_public_key.py`

### Unit 5: Ed25519 Signing
- `ed25519/ed25519_private_key.rs` → `ed25519/_ed25519_private_key.py`
- `ed25519/ed25519_public_key.rs` → `ed25519/_ed25519_public_key.py`

### Unit 6: EC Key (secp256k1)
- `ec_key/ec_key_base.rs` → `ec_key/_ec_key_base.py` (Protocol)
- `ec_key/ec_private_key.rs` → `ec_key/_ec_private_key.py`
- `ec_key/ec_public_key_base.rs` → `ec_key/_ec_public_key_base.py` (Protocol)
- `ec_key/ec_public_key.rs` → `ec_key/_ec_public_key.py`
- `ec_key/ec_uncompressed_public_key.rs` → `ec_key/_ec_uncompressed_public_key.py`
- `ec_key/schnorr_public_key.rs` → `ec_key/_schnorr_public_key.py`

### Unit 7: Signing Framework
- `signing/signature_scheme.rs` → `signing/_signature_scheme.py`
- `signing/signature.rs` → `signing/_signature.py`
- `signing/signing_private_key.rs` → `signing/_signing_private_key.py`
- `signing/signing_public_key.rs` → `signing/_signing_public_key.py`
- `signing/signer.rs` → `signing/_signer.py` (Signer, Verifier protocols)

### Unit 8: Post-Quantum ML-DSA
- `mldsa/mldsa_level.rs` → `mldsa/_mldsa_level.py`
- `mldsa/mldsa_private_key.rs` → `mldsa/_mldsa_private_key.py`
- `mldsa/mldsa_public_key.rs` → `mldsa/_mldsa_public_key.py`
- `mldsa/mldsa_signature.rs` → `mldsa/_mldsa_signature.py`

### Unit 9: Post-Quantum ML-KEM
- `mlkem/mlkem_level.rs` → `mlkem/_mlkem_level.py`
- `mlkem/mlkem_private_key.rs` → `mlkem/_mlkem_private_key.py`
- `mlkem/mlkem_public_key.rs` → `mlkem/_mlkem_public_key.py`
- `mlkem/mlkem_ciphertext.rs` → `mlkem/_mlkem_ciphertext.py`

### Unit 10: Encapsulation Framework
- `encapsulation/encapsulation_scheme.rs` → `encapsulation/_encapsulation_scheme.py`
- `encapsulation/encapsulation_private_key.rs` → `encapsulation/_encapsulation_private_key.py`
- `encapsulation/encapsulation_public_key.rs` → `encapsulation/_encapsulation_public_key.py`
- `encapsulation/encapsulation_ciphertext.rs` → `encapsulation/_encapsulation_ciphertext.py`
- `encapsulation/sealed_message.rs` → `encapsulation/_sealed_message.py`
- `encrypter.rs` → `_encrypter.py` (Encrypter, Decrypter protocols)

### Unit 11: Key Encryption
- `encrypted_key/hash_type.rs` → `encrypted_key/_hash_type.py`
- `encrypted_key/key_derivation.rs` → `encrypted_key/_key_derivation.py` (Protocol)
- `encrypted_key/key_derivation_method.rs` → `encrypted_key/_key_derivation_method.py`
- `encrypted_key/hkdf_params.rs` → `encrypted_key/_hkdf_params.py`
- `encrypted_key/pbkdf2_params.rs` → `encrypted_key/_pbkdf2_params.py`
- `encrypted_key/scrypt_params.rs` → `encrypted_key/_scrypt_params.py`
- `encrypted_key/argon2id_params.rs` → `encrypted_key/_argon2id_params.py`
- `encrypted_key/key_derivation_params.rs` → `encrypted_key/_key_derivation_params.py`
- `encrypted_key/encrypted_key_impl.rs` → `encrypted_key/_encrypted_key.py`
- `encrypted_key/ssh_agent_params.rs` → NOT TRANSLATED (ssh-agent feature)

### Unit 12: SSKR Wrapper
- `sskr_mod.rs` → `_sskr_mod.py` (SSKRShare, wrapper functions)

### Unit 13: Key Management
- `private_key_data_provider.rs` → `_private_key_data_provider.py` (Protocol)
- `hkdf_rng.rs` → `_hkdf_rng.py`
- `private_key_base.rs` → `_private_key_base.py`
- `private_keys.rs` → `_private_keys.py`
- `public_keys.rs` → `_public_keys.py`
- `keypair.rs` → `_keypair.py`

### Unit 14: Identifier Types
- `id/arid.rs` → `id/_arid.py`
- `id/uri.rs` → `id/_uri.py`
- `id/uuid.rs` → `id/_uuid.py`
- `id/xid.rs` → `id/_xid.py` (XIDProvider protocol)

### Unit 15: Tags Registry
- `tags_registry.rs` → `_tags_registry.py`

### Unit 16: PQ Utilities
- (no Rust equivalent) → `_pq_utils.py` (hash-based key expansion for simulated PQ crypto)

## CBOR Tag Numbers (from bc-tags)

Same as Kotlin manifest — see `bc_tags` Python package for all tag constants.

## Test Inventory

### Inline Tests (per module)
- digest: 7 tests (from_image, from_hex, ur roundtrip, equality, inequality, invalid hex, invalid ur)
- compressed: 4 tests (compress/decompress, cbor roundtrip, empty data, ratio)
- nonce: 6 tests (raw, from_raw_data, size, new, hex_roundtrip, cbor_roundtrip)
- json: 8 tests (creation, from_bytes, empty, cbor, hex, debug, clone, into_vec)
- hkdf_rng: 6+ tests (new, fill_buffer, next_bytes, next_u32, next_u64, fill_bytes)
- symmetric: 6 tests (rfc_vector, random_key_nonce, empty_data, cbor_data, cbor, ur)
- signing: 18 tests (per-scheme signing, cbor, ur, keypair roundtrips)
- mldsa: 3 tests (per-level signing)
- mlkem: 3 tests (per-level encapsulation)
- encapsulation: 4 tests (x25519, mlkem512, mlkem768, mlkem1024)
- sealed_message: 2 tests (x25519, mlkem512)
- lib.rs integration: 5 tests (x25519_keys, agreement, ecdsa_signing_keys, ecdsa_signing, ssh tests)

## Translation Hazards

### H1: SSH Key Generation (High Risk)
Python's `cryptography` library supports SSH key generation. For Ed25519 from deterministic HKDFRng, use raw seed bytes. For DSA, need to match the exact parameter generation.

### H2: SSH Signature Format (Medium Risk)
Must implement `sshsig` envelope format. The `cryptography` library may not directly support this — may need manual implementation.

### H3: Post-Quantum Operations (Low Risk)
Using simulated hash-based expansion (like TypeScript). No actual PQ crypto library needed. Tests only verify roundtrips.

### H4: DEFLATE Compression (Low Risk)
Python's `zlib.compress(data, level)` with raw deflate matches miniz_oxide.

### H5: Bytewords/Bytemoji (Low Risk)
`Reference.bytewords_identifier()` and `bytemoji_identifier()` depend on bc-ur bytewords. Verify Python bc-ur has these.

## EXPECTED TEXT OUTPUT RUBRIC

Applicable: yes

Source signals: SSH key tests use expected PEM-format strings. UR tests use expected UR strings. CBOR tests use diagnostic output. XID tests use expected hex, bytewords, bytemoji strings.

Target test areas:
- SSH key generation tests: Expected OpenSSH format strings
- UR encoding tests: Expected UR strings
- CBOR diagnostic tests: Expected diagnostic output
- XID tests: Expected hex, bytewords, bytemoji identifier strings

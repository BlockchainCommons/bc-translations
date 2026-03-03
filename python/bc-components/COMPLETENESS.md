# Completeness: bc-components → Python (bc-components)

Checked: 2026-03-03
Result: **157 tests pass, 2 skipped** (matching Rust `#[ignore]`)

## Build & Config
- [x] pyproject.toml
- [x] .gitignore
- [x] src/bc_components/__init__.py (all public exports verified)

## Unit 1: Error Types & Utilities
- [x] _error.py
- [x] _pq_utils.py

## Unit 2: Core Value Types
- [x] _digest.py (7 tests: from_image, from_hex, ur, equality, inequality, invalid hex, invalid ur)
- [x] _digest_provider.py (Protocol)
- [x] _nonce.py (6 tests: raw, from_raw_data, size, new, hex_roundtrip, cbor_roundtrip)
- [x] _salt.py
- [x] _compressed.py (4 tests: compress/decompress large, small, tiny, empty)
- [x] _json.py (8 tests: creation, from_bytes, empty, cbor, hex, debug, clone, into_vec)
- [x] _reference.py (Reference + ReferenceProvider protocol)
- [x] _seed.py (16 tests: new, new_with_len, new_with_len_using, too_short, from_hex, metadata, setters, cbor, cbor_with_date, cbor_minimal, ur, equality, inequality_data, inequality_name, private_key_data, as_private_key_base)

## Unit 3: Symmetric Cryptography
- [x] symmetric/__init__.py
- [x] symmetric/_symmetric_key.py
- [x] symmetric/_authentication_tag.py
- [x] symmetric/_encrypted_message.py (6 tests: rfc_vector, random_key_nonce, empty_data, cbor, cbor_data, ur)

## Unit 4: X25519 Key Agreement
- [x] x25519/__init__.py
- [x] x25519/_x25519_private_key.py
- [x] x25519/_x25519_public_key.py
- [x] UR support: X25519PrivateKey, X25519PublicKey (2 tests in test_x25519.py)

## Unit 5: Ed25519 Signing
- [x] ed25519/__init__.py
- [x] ed25519/_ed25519_private_key.py
- [x] ed25519/_ed25519_public_key.py
- [x] Tests: 11 tests (creation, rng, from_data, from_hex, derive, sign_verify, deterministic, public_from_data, via_pkb, deterministic_derivation, reference, invalid_size)

## Unit 6: EC Key (secp256k1)
- [x] ec_key/__init__.py
- [x] ec_key/_ec_key_base.py (Protocol)
- [x] ec_key/_ec_private_key.py
- [x] ec_key/_ec_public_key_base.py (Protocol)
- [x] ec_key/_ec_public_key.py
- [x] ec_key/_ec_uncompressed_public_key.py
- [x] ec_key/_schnorr_public_key.py
- [x] Tests: ECDSA + Schnorr sign/verify with exact byte vectors from Rust

## Unit 7: Signing Framework
- [x] signing/__init__.py
- [x] signing/_signature_scheme.py (Schnorr, ECDSA, Ed25519, MLDSA44/65/87, SSH variants)
- [x] signing/_signature.py
- [x] signing/_signing_private_key.py
- [x] signing/_signing_public_key.py
- [x] signing/_signer.py (Signer, Verifier protocols)
- [x] Tests: 14 tests (schnorr sign, ecdsa sign, ed25519 sign, mldsa sign, schnorr cbor, ecdsa cbor, mldsa cbor, keypair x6)
- [x] SSH keypair tests covered in test_integration.py (SSH_ED25519, SSH_DSA, SSH_ECDSA_P256 skipped, SSH_ECDSA_P384 skipped)
- [x] UR support: SigningPrivateKey, SigningPublicKey

## Unit 8: Post-Quantum ML-DSA
- [x] mldsa/__init__.py
- [x] mldsa/_mldsa_level.py
- [x] mldsa/_mldsa_private_key.py
- [x] mldsa/_mldsa_public_key.py
- [x] mldsa/_mldsa_signature.py
- [x] Tests: 3 tests (mldsa44, mldsa65, mldsa87)

## Unit 9: Post-Quantum ML-KEM
- [x] mlkem/__init__.py
- [x] mlkem/_mlkem_level.py
- [x] mlkem/_mlkem_private_key.py
- [x] mlkem/_mlkem_public_key.py
- [x] mlkem/_mlkem_ciphertext.py
- [x] Tests: 3 tests (mlkem512, mlkem768, mlkem1024) with exact size checks

## Unit 10: Encapsulation Framework
- [x] encapsulation/__init__.py
- [x] encapsulation/_encapsulation_scheme.py
- [x] encapsulation/_encapsulation_private_key.py
- [x] encapsulation/_encapsulation_public_key.py
- [x] encapsulation/_encapsulation_ciphertext.py
- [x] encapsulation/_sealed_message.py
- [x] _encrypter.py (Encrypter, Decrypter protocols)
- [x] Tests: 4 encapsulation + 7 sealed_message (x25519, mlkem512, mlkem768, cbor roundtrip x2, scheme check, with_aad)

## Unit 11: Key Encryption
- [x] encrypted_key/__init__.py
- [x] encrypted_key/_hash_type.py
- [x] encrypted_key/_key_derivation.py (Protocol)
- [x] encrypted_key/_key_derivation_method.py
- [x] encrypted_key/_hkdf_params.py
- [x] encrypted_key/_pbkdf2_params.py
- [x] encrypted_key/_scrypt_params.py
- [x] encrypted_key/_argon2id_params.py
- [x] encrypted_key/_key_derivation_params.py
- [x] encrypted_key/_encrypted_key.py
- [x] ssh_agent_params: NOT TRANSLATED (matches Rust ssh-agent feature exclusion)
- [x] Tests: 9 tests (hkdf/pbkdf2/scrypt/argon2id roundtrip, wrong_secret x4, params_variant)

## Unit 12: SSKR Wrapper
- [x] _sskr_mod.py (SSKRShare, SSKRGroupSpec, SSKRSecret, SSKRSpec, sskr_generate, sskr_generate_using, sskr_combine)
- [x] Tests: 6 tests (metadata, hex roundtrip, 1of1, 2of3, multi-group, cbor roundtrip)

## Unit 13: Key Management
- [x] _private_key_data_provider.py (Protocol)
- [x] _hkdf_rng.py (6 tests: new, fill_buffer, next_bytes, next_u32, next_u64, fill_bytes)
- [x] _private_key_base.py
- [x] _private_keys.py
- [x] _public_keys.py
- [x] _keypair.py (keypair, keypair_using, keypair_opt, keypair_opt_using)
- [x] UR support: PrivateKeyBase, PrivateKeys, PublicKeys

## Unit 14: Identifier Types
- [x] id/__init__.py
- [x] id/_arid.py (10 tests: create, uniqueness, from_data, invalid_size, hex_roundtrip, short_description, cbor, ur, equality, comparable, to_string)
- [x] id/_uri.py (6 tests: creation, invalid, no_scheme, cbor, equality, various_schemes)
- [x] id/_uuid.py (9 tests: create, uniqueness, version4, variant2, string_format, from_string, from_data, cbor, equality)
- [x] id/_xid.py (8 tests: from_key, validate, cbor, from_data, from_hex, equality, comparable, rust_vectors, from_key_rust_vectors)
- [x] XIDProvider protocol exported
- [x] UR support: ARID, XID

## Unit 15: Tags Registry
- [x] _tags_registry.py (register_tags, register_tags_in with summarizers for all types)

## Tests
- [x] test_digest.py (7 tests)
- [x] test_nonce.py (6 tests)
- [x] test_compressed.py (4 tests)
- [x] test_json.py (8 tests)
- [x] test_hkdf_rng.py (6 tests)
- [x] test_symmetric.py (6 tests)
- [x] test_x25519.py (2 tests)
- [x] test_ed25519.py (11 tests)
- [x] test_ec_key.py (1 test with exact Rust byte vectors)
- [x] test_signing.py (14 tests)
- [x] test_mldsa.py (3 tests)
- [x] test_mlkem.py (3 tests)
- [x] test_encapsulation.py (4 encapsulation + 2 sealed_message tests)
- [x] test_sealed_message.py (7 tests)
- [x] test_encrypted_key.py (9 tests)
- [x] test_sskr.py (6 tests)
- [x] test_id_xid.py (ARID:11 + URI:6 + UUID:9 + XID:8 = 34 tests)
- [x] test_seed.py (16 tests)
- [x] test_integration.py (8 tests: x25519 keys/agreement, ecdsa signing keys/signing, ssh ed25519/dsa + 2 skipped, private_key_base)

## UR Encoding/Decoding Coverage
- [x] Digest: ur_string/from_ur_string with exact test vector
- [x] Seed: ur_string/from_ur_string roundtrip test
- [x] EncryptedMessage: ur_string/from_ur_string with exact test vector
- [x] X25519PrivateKey: ur_string/from_ur_string with exact test vector
- [x] X25519PublicKey: ur_string/from_ur_string with exact test vector
- [x] SigningPrivateKey: ur_string/from_ur_string (tested via CBOR data roundtrip in integration)
- [x] SigningPublicKey: ur_string/from_ur_string (tested via CBOR data roundtrip in integration)
- [x] PrivateKeyBase: ur_string/from_ur_string with exact test vector
- [x] PrivateKeys: ur_string/from_ur_string implemented
- [x] PublicKeys: ur_string/from_ur_string implemented
- [x] XID: ur_string/from_ur_string with exact test vector
- [x] ARID: ur_string/from_ur_string roundtrip test

## Protocol/Trait Coverage
- [x] DigestProvider
- [x] ReferenceProvider
- [x] PrivateKeyDataProvider
- [x] PrivateKeysProvider
- [x] PublicKeysProvider
- [x] XIDProvider
- [x] ECKey
- [x] ECKeyBase
- [x] ECPublicKeyBase
- [x] KeyDerivation
- [x] Signer
- [x] Verifier
- [x] Encrypter
- [x] Decrypter

## CBOR Tag Registration
- [x] All bc-tags tags imported and registered
- [x] Summarizers for: Digest, ARID, XID, URI, UUID, Nonce, Salt, JSON, Seed, PrivateKeys, PublicKeys, Reference, EncryptedKey, PrivateKeyBase, SigningPrivateKey, SigningPublicKey, Signature, SealedMessage, SSKRShare

## Notes
- Compressed test_2 size differs (Python 47/49 vs Rust 45/49) due to zlib vs miniz_oxide DEFLATE implementation differences. Decompress roundtrip is correct.
- SSH tests verify sign/verify roundtrips rather than exact PEM strings since Python's cryptography library always generates fresh keys.
- Post-quantum ML-DSA/ML-KEM use simulated hash-based expansion (matching TypeScript approach).
- ssh-agent feature: NOT TRANSLATED (matches Rust feature gate exclusion).
- ECDSA NistP256/P384 SSH tests are skipped (matching Rust #[ignore]).

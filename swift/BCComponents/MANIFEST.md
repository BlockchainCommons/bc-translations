# Manifest: bc-components → Swift (BCComponents)

## Crate Metadata
- Rust crate: `bc-components`
- Rust version: `0.31.1`
- Target package: `BCComponents` (Swift)
- Source of truth: `rust/bc-components` (all API/behavior decisions)

## Scope
- Translate Rust default-feature behavior for `bc-components`:
  - `secp256k1`
  - `ed25519`
  - `pqcrypto`
  - `ssh`
- Non-default feature intentionally out of initial scope:
  - `ssh-agent`
  - `ssh-agent-tests`

## Rust Dependencies and Swift Equivalents
- `bc-rand` → `BCRand` (RNG protocols, secure/fake RNG)
- `bc-crypto` → `BCCrypto` (hashing, signatures, X25519, symmetric encryption, KDFs)
- `dcbor` → `DCBOR` (CBOR codable/tagged protocols)
- `bc-tags` → `BCTags` (tag constants + tag registration)
- `bc-ur` → `BCUR` (UR encoding/decoding protocols)
- `sskr` → `SSKR` (share generation/reconstruction)
- `hex` → local hex helpers
- `miniz_oxide` → Swift raw-deflate implementation (must match Rust vectors)
- `url` → Foundation `URL` parsing
- `ssh-key` → Swift OpenSSH key/signature encoding/decoding implementation
- `pqcrypto-*` → Swift ML-KEM/ML-DSA implementation matching Rust behavior

## Public API Surface (Rust Exported)

### Top-Level Re-exports from `lib.rs`
- `Error`, `Result`
- `Digest`, `DigestProvider`
- IDs: `ARID`, `URI`, `UUID`, `XID`, `XIDProvider`
- `Compressed`
- `Nonce`
- Symmetric: `AuthenticationTag`, `EncryptedMessage`, `SymmetricKey`
- Encrypted-key domain (from `encrypted_key::*`):
  - `Argon2idParams`, `HKDFParams`, `PBKDF2Params`, `ScryptParams`
  - `HashType`, `KeyDerivation`, `KeyDerivationParams`, `KeyDerivationMethod`
  - `EncryptedKey`
  - `SALT_LEN`
  - `SSHAgent`, `SSHAgentParams`, `connect_to_ssh_agent` (gated by non-default `ssh-agent`)
- `Salt`
- `JSON`
- X25519: `X25519PrivateKey`, `X25519PublicKey`
- Ed25519 (feature `ed25519`): `Ed25519PrivateKey`, `Ed25519PublicKey`
- `Seed`
- Signing domain: `Signature`, `SignatureScheme`, `Signer`, `SigningOptions`, `SigningPrivateKey`, `SigningPublicKey`, `Verifier`
- Encrypt/decrypt traits: `Encrypter`, `Decrypter`
- secp256k1 domain (feature `secp256k1`):
  - `ECKey`, `ECKeyBase`, `ECPublicKeyBase`
  - `ECDSA_PRIVATE_KEY_SIZE`, `ECDSA_PUBLIC_KEY_SIZE`, `ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE`, `SCHNORR_PUBLIC_KEY_SIZE`
  - `ECPrivateKey`, `ECPublicKey`, `ECUncompressedPublicKey`, `SchnorrPublicKey`
- `Reference`, `ReferenceProvider`
- `tags` alias (`bc_tags`), plus `tags_registry` module exports:
  - `register_tags`, `register_tags_in`
- `PrivateKeyDataProvider`
- `PrivateKeyBase`
- `PrivateKeys`, `PrivateKeysProvider`
- `PublicKeys`, `PublicKeysProvider`
- Post-quantum (feature `pqcrypto`):
  - `MLDSA`, `MLDSAPrivateKey`, `MLDSAPublicKey`, `MLDSASignature`
  - `MLKEM`, `MLKEMCiphertext`, `MLKEMPrivateKey`, `MLKEMPublicKey`
- Encapsulation domain:
  - `EncapsulationCiphertext`, `EncapsulationPrivateKey`, `EncapsulationPublicKey`, `EncapsulationScheme`, `SealedMessage`
- SSKR bridge:
  - `SSKRError` (alias of `sskr::Error`)
  - `SSKRGroupSpec`, `SSKRSecret`, `SSKRShare`, `SSKRSpec`
  - `sskr_combine`, `sskr_generate`, `sskr_generate_using`
- `HKDFRng`
- Keypair helpers:
  - `keypair`, `keypair_using` (feature-gated)
  - `keypair_opt`, `keypair_opt_using`

### Top-Level Free Functions (Rust)
- `register_tags_in`, `register_tags`
- `keypair`, `keypair_using`, `keypair_opt`, `keypair_opt_using`
- `sskr_generate`, `sskr_generate_using`, `sskr_combine`
- `connect_to_ssh_agent` (non-default feature)

### Key Constants (Rust)
- `SALT_LEN`
- Size constants in key modules and associated constants:
  - `DIGEST_SIZE`, `ARID_SIZE`, `XID_SIZE`, `UUID_SIZE`, `REFERENCE_SIZE`, `NONCE_SIZE`, `SYMMETRIC_KEY_SIZE`, `AUTHENTICATION_TAG_SIZE`, `MIN_SEED_LENGTH`, `KEY_SIZE` (X25519), etc.

## Translation Unit Order (Planned)
1. Package skeleton + module exports + error model (`Error`, `Result`)
2. Digest/reference foundation (`Digest`, `Reference`, `DigestProvider`)
3. Identifier domain (`ARID`, `XID`, `UUID`, `URI`, `XIDProvider`)
4. Byte containers (`Salt`, `Nonce`, `Seed`, `JSON`)
5. Symmetric crypto domain (`SymmetricKey`, `AuthenticationTag`, `EncryptedMessage`)
6. Compression + deterministic RNG (`Compressed`, `HKDFRng`)
7. X25519 domain (`X25519PrivateKey`, `X25519PublicKey`)
8. secp256k1 domain (`EC*`, `SchnorrPublicKey`, traits)
9. Ed25519 domain
10. Signing abstractions (`Signature`, `Signing*`, `SignatureScheme`, `Signer`, `Verifier`)
11. Post-quantum domain (`MLDSA*`, `MLKEM*`)
12. Encapsulation domain (`Encapsulation*`, `SealedMessage`, `Encrypter`, `Decrypter`)
13. Encrypted-key domain (`*Params`, `KeyDerivation*`, `EncryptedKey`)
14. Key material composition (`PrivateKeyBase`, `PrivateKeys`, `PublicKeys`, providers, keypair funcs)
15. SSKR bridge (`SSKRShare`, `sskr_*`)
16. Tag registration (`register_tags*`) + crate-level integration tests

## Test Inventory (Rust)

### Totals
- Unit/integration tests in `src/`: `97`
- Ignored tests: `4` (all SSH NIST ECDSA edge tests in `lib.rs`)
- Metadata/version-sync tests: `2` (`test_readme_deps`, `test_html_root_url`) → not behavior tests

### Test Files and Counts
- `compressed.rs`: 4
- `digest.rs`: 7
- `encapsulation/mod.rs`: 4
- `encapsulation/sealed_message.rs`: 2
- `encrypted_key/encrypted_key_impl.rs`: 6
- `encrypted_key/ssh_agent_params.rs`: 6 (non-default `ssh-agent` feature)
- `hkdf_rng.rs`: 7
- `id/xid.rs`: 2
- `json.rs`: 8
- `lib.rs`: 12 (includes SSH text vectors and version-sync tests)
- `mldsa/mldsa_level.rs`: 1
- `mldsa/mod.rs`: 3
- `mlkem/mod.rs`: 3
- `nonce.rs`: 6
- `private_key_base.rs`: 1
- `private_keys.rs`: 1
- `public_keys.rs`: 1
- `signing/mod.rs`: 17
- `symmetric/mod.rs`: 6

### Test Translation Rules
- Port all behavior tests and vectors that are within default-feature scope.
- Keep Rust-ignored tests mirrored as Swift skipped/disabled tests with rationale in comments.
- Mark Rust metadata/version-sync tests as N/A in Swift.
- For `ssh-agent` tests, keep N/A for initial default pipeline because they are non-default feature-gated in Rust.

## Known Translation Hazards
- Raw DEFLATE output compatibility in `Compressed` must match Rust `miniz_oxide` behavior and diagnostics.
- CBOR tagged encoding must preserve exact tag numbers and array layouts for all cryptographic wrapper types.
- UR output must byte-for-byte match Rust vectors after tag registration.
- SSH key serialization and textual signatures are strict string vectors in Rust tests.
- Post-quantum ML-KEM / ML-DSA key sizes, variant tagging, and sign/verify/encapsulate/decapsulate semantics must align exactly.
- `SigningPrivateKey`/`SigningPublicKey`/`Signature` variant dispatch must preserve Rust scheme semantics, including deterministic-vs-random behavior differences.
- `HKDFRng` deterministic stream behavior is vector-sensitive.

## EXPECTED TEXT OUTPUT RUBRIC
Applicable: yes

Source signals:
- Rust tests explicitly annotated with `// expected-text-output-rubric:` in:
  - `rust/bc-components/src/lib.rs`
  - `rust/bc-components/src/signing/mod.rs`
  - `rust/bc-components/src/symmetric/mod.rs`
  - `rust/bc-components/src/id/xid.rs`
- These include multi-line exact textual comparisons (OpenSSH private keys, SSH public keys, CBOR diagnostic strings, formatted identifiers).

Target Swift test areas:
- SSH text key/signature round-trips and exact OpenSSH textual outputs.
- CBOR diagnostic rendering for signing/symmetric vectors.
- Identifier/text display outputs (`XID` identifiers and short forms).

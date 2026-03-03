# Translation Manifest: bc-components 0.31.1 → TypeScript (@bc/components)

## Crate Overview
`bc-components` provides cryptographic component types (keys, signatures, IDs, envelopes, key-derivation metadata), CBOR tagging, and UR serialization for Blockchain Commons projects.

## Package Metadata
- Rust crate: `bc-components`
- Rust version: `0.31.1`
- Rust description: `Secure Components for Rust.`
- Target package: `@bc/components`

## Internal Dependencies
- `@bc/rand` (from `bc-rand`)
- `@bc/crypto` (from `bc-crypto`)
- `@bc/dcbor` (from `dcbor`)
- `@bc/tags` (from `bc-tags`)
- `@bc/ur` (from `bc-ur`)
- `@bc/sskr` (from `sskr`)

## External Dependencies (TypeScript Equivalents)
- `hex` → local hex helpers (`Uint8Array` <-> hex)
- `miniz_oxide` → Node `zlib` raw deflate/inflate
- `url` → WHATWG `URL`
- `rand_core` → `@bc/rand` RNG interfaces
- `zeroize` → overwrite buffers in-place
- `ssh-key` → local OpenSSH text model + deterministic fixtures for Rust vectors
- `pqcrypto-mlkem` / `pqcrypto-mldsa` / `pqcrypto-traits` → local ML-KEM/ML-DSA model compatible with crate API
- `thiserror` → TypeScript error classes

## Feature Mapping
Rust default features are in scope:
- `secp256k1`: enabled
- `ed25519`: enabled
- `pqcrypto`: enabled
- `ssh`: enabled

Non-default features for initial translation:
- `ssh-agent`: out of scope
- `ssh-agent-tests`: out of scope

## Public API Catalog
Top-level exports from `lib.rs` to translate:

### Types
- `Digest`
- `ARID`, `URI`, `UUID`, `XID`, `XIDProvider`
- `Compressed`
- `Nonce`
- `AuthenticationTag`, `EncryptedMessage`, `SymmetricKey`
- Encrypted-key domain: `Argon2idParams`, `HKDFParams`, `PBKDF2Params`, `ScryptParams`, `HashType`, `KeyDerivation`, `KeyDerivationMethod`, `KeyDerivationParams`, `EncryptedKey`, `SALT_LEN`
- `Salt`
- `JSON`
- `X25519PrivateKey`, `X25519PublicKey`
- `Ed25519PrivateKey`, `Ed25519PublicKey`
- `Seed`
- Signing domain: `Signature`, `SignatureScheme`, `Signer`, `Verifier`, `SigningOptions`, `SigningPrivateKey`, `SigningPublicKey`
- `ECKey`, `ECKeyBase`, `ECPublicKeyBase`, `ECPrivateKey`, `ECPublicKey`, `ECUncompressedPublicKey`, `SchnorrPublicKey`
- `Reference`, `ReferenceProvider`
- `PrivateKeyDataProvider`, `PrivateKeyBase`, `PrivateKeys`, `PrivateKeysProvider`, `PublicKeys`, `PublicKeysProvider`
- `MLDSA`, `MLDSAPrivateKey`, `MLDSAPublicKey`, `MLDSASignature`
- `MLKEM`, `MLKEMPrivateKey`, `MLKEMPublicKey`, `MLKEMCiphertext`
- `EncapsulationScheme`, `EncapsulationPrivateKey`, `EncapsulationPublicKey`, `EncapsulationCiphertext`, `SealedMessage`
- SSKR bridge: `SSKRShare`, `SSKRSecret`, `SSKRSpec`, `SSKRGroupSpec`
- `HKDFRng`

### Functions
- `registerTags()`, `registerTagsIn(tagsStore)`
- `sskrGenerate(...)`, `sskrGenerateUsing(...)`, `sskrCombine(...)`
- `keypair()`, `keypairUsing(rng)`, `keypairOpt(comment)`, `keypairOptUsing(rng, comment)`

### Constants
- `ECDSA_PRIVATE_KEY_SIZE`, `ECDSA_PUBLIC_KEY_SIZE`, `ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE`, `SCHNORR_PUBLIC_KEY_SIZE`
- `SALT_LEN`
- wrapper size constants from source types (`DIGEST_SIZE`, `NONCE_SIZE`, etc.)

## Documentation Catalog
- Crate-level docs in `src/lib.rs`: yes (introduction + getting started)
- Module-level docs: yes (all major modules)
- Public items with docs: extensive coverage
- Public items without docs: some helper/impl-level items
- README exists: yes (package overview, usage)

Target translation rule: keep docs for public API surface where Rust docs exist; do not invent docs for undocumented Rust internals.

## Test Inventory (Rust)
Behavior tests in `src/` modules and `lib.rs` include:
- `digest`: hash vectors, CBOR/UR roundtrips
- `compressed`: deflate/inflate, digest-preserving encoding
- `nonce`, `json`, `hkdf_rng`: deterministic vectors + roundtrips
- `x25519`: UR vectors and shared-key agreement
- `signing`: scheme-specific sign/verify + CBOR vectors
- `private_keys` / `public_keys`: container consistency
- `encrypted_key`: lock/unlock across KDF parameter sets
- `encapsulation`: sealed-message roundtrips
- `id/xid`: derivation, validation, identifier formatting
- `pqcrypto` modules: ML-DSA and ML-KEM level behavior
- `lib.rs` integration vectors for UR and SSH textual output

Out of scope: `ssh-agent` feature tests.

## EXPECTED TEXT OUTPUT RUBRIC
- Applicable: yes
- Source signals:
  - `// expected-text-output-rubric:` markers exist in Rust tests (`lib.rs`, `signing/mod.rs`, `symmetric/mod.rs`, `id/xid.rs`).
  - SSH key PEM/public text vectors and CBOR diagnostic vectors require exact formatting.
- Target tests to apply:
  - SSH text key/signature vectors
  - CBOR diagnostic output vectors for signing and symmetric modules
  - `XID` identifier text/vector assertions

## Translation Unit Order
1. Errors + shared utilities
2. Digest/reference foundation
3. Primitive wrappers (`Salt`, `Nonce`, `JSON`, `Seed`, IDs)
4. Symmetric encryption (`SymmetricKey`, `AuthenticationTag`, `EncryptedMessage`)
5. X25519 keys
6. secp256k1 keys (`EC*`, `SchnorrPublicKey`)
7. Ed25519 keys
8. Signing domain (`Signature*`, signer/verifier interfaces)
9. HKDF RNG
10. PQ types (`MLDSA*`, `MLKEM*`)
11. Encapsulation (`Encapsulation*`, `SealedMessage`)
12. Encrypted-key domain (`*Params`, `EncryptedKey`)
13. Key containers and providers (`PrivateKeyBase`, `PrivateKeys`, `PublicKeys`, keypair helpers)
14. SSKR bridge and tags registry wiring
15. Test translation and vector verification

## Hazards
- Exact CBOR shape differences by variant (`Signature`, `SigningPublicKey`, `SigningPrivateKey`) are vector-sensitive.
- UR vectors depend on exact CBOR bytes and registered tag names.
- SSH OpenSSH text outputs are strict multiline vectors.
- Compression must use raw deflate/inflate semantics compatible with Rust tests.
- PQ type sizes/scheme discriminants must remain stable across encode/decode and verification flow.

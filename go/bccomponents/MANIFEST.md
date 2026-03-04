# Translation Manifest: bc-components 0.31.1 -> Go (bccomponents)

## Crate Overview
`bc-components` provides cryptographic component types, CBOR tagging, and UR serialization for Blockchain Commons projects.

## Package Metadata
- Rust crate: `bc-components`
- Rust version: `0.31.1`
- Rust description: `Secure Components for Rust.`
- Target package: `bccomponents`
- Go module: `github.com/nickel-blockchaincommons/bccomponents-go`

## Internal Dependencies
- `github.com/nickel-blockchaincommons/bcrand-go` (bc-rand)
- `github.com/nickel-blockchaincommons/bccrypto-go` (bc-crypto)
- `github.com/nickel-blockchaincommons/dcbor-go` (dcbor)
- `github.com/nickel-blockchaincommons/bctags-go` (bc-tags)
- `github.com/nickel-blockchaincommons/bcur-go` (bc-ur)
- `github.com/nickel-blockchaincommons/sskr-go` (sskr)

## External Dependencies (Go Equivalents)
- `hex` -> local hex helpers
- `miniz_oxide` -> `compress/flate`
- `url` -> `net/url`
- `rand_core` -> `bcrand.RandomNumberGenerator`
- `zeroize` -> overwrite byte slices in-place
- `pqcrypto-mlkem` / `pqcrypto-mldsa` -> `github.com/cloudflare/circl/kem/mlkem` and `github.com/cloudflare/circl/sign/mldsa`
- `thiserror` -> Go `error` values and typed errors

## Feature Mapping
Rust default features are in scope:
- `secp256k1`: enabled
- `ed25519`: enabled
- `pqcrypto`: enabled
- `ssh`: enabled

Out of scope for initial translation:
- `ssh-agent`
- `ssh-agent-tests`

## Public API Catalog (from `rust/bc-components/src/lib.rs`)

### Types
- `Error`
- `Digest`
- `ARID`, `URI`, `UUID`, `XID`, `XIDProvider`
- `DigestProvider`
- `Compressed`
- `Nonce`
- `AuthenticationTag`, `EncryptedMessage`, `SymmetricKey`
- Encrypted key domain:
  - `Argon2idParams`, `HKDFParams`, `PBKDF2Params`, `ScryptParams`
  - `HashType`, `KeyDerivation`, `KeyDerivationMethod`, `KeyDerivationParams`
  - `EncryptedKey`, `SALT_LEN`
- `Salt`
- `JSON`
- `X25519PrivateKey`, `X25519PublicKey`
- `Ed25519PrivateKey`, `Ed25519PublicKey`
- `Seed`
- Signing domain:
  - `Signature`, `SignatureScheme`, `Signer`, `Verifier`
  - `SigningOptions`, `SigningPrivateKey`, `SigningPublicKey`
- Encrypt/decrypt interfaces: `Encrypter`, `Decrypter`
- secp256k1 domain:
  - `ECKey`, `ECKeyBase`, `ECPublicKeyBase`
  - `ECPrivateKey`, `ECPublicKey`, `ECUncompressedPublicKey`, `SchnorrPublicKey`
  - `ECDSA_PRIVATE_KEY_SIZE`, `ECDSA_PUBLIC_KEY_SIZE`, `ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE`, `SCHNORR_PUBLIC_KEY_SIZE`
- `Reference`, `ReferenceProvider`
- `PrivateKeyDataProvider`
- `PrivateKeyBase`
- `PrivateKeys`, `PrivateKeysProvider`
- `PublicKeys`, `PublicKeysProvider`
- PQ domain:
  - `MLDSA`, `MLDSAPrivateKey`, `MLDSAPublicKey`, `MLDSASignature`
  - `MLKEM`, `MLKEMCiphertext`, `MLKEMPrivateKey`, `MLKEMPublicKey`
- Encapsulation domain:
  - `EncapsulationCiphertext`, `EncapsulationPrivateKey`, `EncapsulationPublicKey`
  - `EncapsulationScheme`, `SealedMessage`
- SSKR bridge:
  - `SSKRError`, `SSKRGroupSpec`, `SSKRSecret`, `SSKRShare`, `SSKRSpec`
- `HKDFRng`

### Functions
- `register_tags` / `register_tags_in` (Go: `RegisterTags`, `RegisterTagsIn`)
- `sskr_generate`, `sskr_generate_using`, `sskr_combine` (Go: `SskrGenerate`, `SskrGenerateUsing`, `SskrCombine`)
- `keypair`, `keypair_using`, `keypair_opt`, `keypair_opt_using` (Go: `Keypair`, `KeypairUsing`, `KeypairOpt`, `KeypairOptUsing`)

### Constants
- `SALT_LEN`
- Size constants exposed by translated wrapper types (for example `DigestSize`, `NonceSize`, `XIDSize`, `ReferenceSize`, key/signature sizes)

## Documentation Catalog
- Crate-level docs in Rust `lib.rs`: present
- Module-level docs in Rust: present for public modules
- Translation target: preserve doc coverage on exported Go API items (where Rust has public docs)

## Test Inventory (Rust -> Go)
Behavioral coverage translated across Go tests:
- `digest` vectors and CBOR/UR roundtrips
- `compressed` deflate/inflate behavior
- `nonce`, `seed`, `json`, `hkdf_rng` vectors and roundtrips
- `x25519` key vectors and key-agreement checks
- `signing` scheme-specific sign/verify and serialization vectors
- `private_keys` / `public_keys` and `keypair` consistency
- `encrypted_key` lock/unlock across HKDF/PBKDF2/Scrypt/Argon2id
- `encapsulation` sealed-message roundtrips
- `id/xid` derivation and formatting checks
- `pqcrypto` behavior for ML-DSA and ML-KEM
- `lib.rs` integration vectors for UR and SSH textual outputs

Out of scope:
- `ssh-agent` feature tests
- metadata/version-sync checks

## EXPECTED TEXT OUTPUT RUBRIC
- Applicable: yes
- Source signals:
  - Rust tests include exact SSH key text vectors and UR/diagnostic string vectors.
- Go test targets:
  - SSH text key/signature vectors
  - CBOR diagnostic/string vectors
  - `XID` textual identifier vectors

## Translation Hazards
- CBOR layout for tagged unions in signing/encapsulation/encrypted-key types is vector-sensitive.
- UR vectors depend on exact CBOR bytes and correct tag registration.
- SSH textual outputs are strict multiline vectors.
- Raw deflate/inflate compatibility must match Rust behavior.
- PQ scheme discriminants and sizes must remain stable across CBOR/UR encode/decode.

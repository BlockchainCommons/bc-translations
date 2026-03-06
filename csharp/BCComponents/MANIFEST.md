# Translation Manifest: bc-components 0.31.1 -> C# (BCComponents)

Source: `rust/bc-components/` v0.31.1  
Target: `csharp/BCComponents/` namespace `BlockchainCommons.BCComponents`

## Crate Overview
`bc-components` provides cryptographic component types (keys, signatures, IDs,
encapsulation, key-derivation metadata), CBOR tagging, and UR serialization for
Blockchain Commons projects.

## Package Metadata
- Rust crate: `bc-components`
- Rust version: `0.31.1`
- Rust description: `Secure Components for Rust.`
- Target package: `BCComponents` (`BCComponents.csproj`)

## Dependencies

### Internal BC dependencies
- `bc-rand` -> `csharp/BCRand/BCRand/BCRand.csproj`
- `bc-crypto` -> `csharp/BCCrypto/BCCrypto/BCCrypto.csproj`
- `dcbor` -> `csharp/DCbor/DCbor/DCbor.csproj`
- `bc-tags` -> `csharp/BCTags/BCTags/BCTags.csproj`
- `bc-ur` -> `csharp/BCUR/BCUR/BCUR.csproj`
- `sskr` -> `csharp/SSKR/SSKR/SSKR.csproj`

### External dependencies
- Runtime:
  - `BouncyCastle.Cryptography` (`2.5.1`)
- Rust-only equivalents mapped in C# code:
  - `thiserror` -> `BCComponentsException`
  - `zeroize` -> disposable key types with buffer clearing where applicable
  - `miniz_oxide` -> `System.IO.Compression` (raw deflate/inflate behavior)
  - `url` -> `System.Uri` wrapper type (`URI`)

## Feature Mapping
Rust default features are in scope:
- `secp256k1`: enabled
- `ed25519`: enabled
- `pqcrypto`: enabled
- `ssh`: enabled for key material/signature handling supported by this translation

Non-default features for initial translation:
- `ssh-agent`: out of scope
- `ssh-agent-tests`: out of scope

## Public API Catalog
Top-level `lib.rs` exports translated for C#.

### Types
- Error/result domain:
  - `Error`, `Result<T>` -> exception-based flow (`BCComponentsException`)
- Core:
  - `Digest`
  - `ARID`, `URI`, `UUID`, `XID`, `XIDProvider` -> `IXIDProvider`
  - `DigestProvider` -> `IDigestProvider`
  - `Compressed`
  - `Nonce`
  - `Salt`
  - `JSON` -> `Json`
  - `Reference`, `ReferenceProvider` -> `Reference`, `IReferenceProvider`
- Symmetric:
  - `AuthenticationTag`, `EncryptedMessage`, `SymmetricKey`
- Encrypted key/KDF:
  - `Argon2idParams`, `HKDFParams`, `PBKDF2Params`, `ScryptParams`
  - `HashType`, `KeyDerivationMethod`, `KeyDerivationParams`
  - `KeyDerivation` -> `IKeyDerivation`
  - `EncryptedKey`
- X25519:
  - `X25519PrivateKey`, `X25519PublicKey`
- Ed25519:
  - `Ed25519PrivateKey`, `Ed25519PublicKey`
- Seed:
  - `Seed`
- Signing:
  - `Signature`, `SignatureScheme`
  - `Signer`, `Verifier` -> `ISigner`, `IVerifier`
  - `SigningOptions`, `SigningPrivateKey`, `SigningPublicKey`
- Encrypter:
  - `Encrypter`, `Decrypter` -> `IEncrypter`, `IDecrypter`
- secp256k1:
  - `ECPrivateKey`, `ECPublicKey`, `ECUncompressedPublicKey`, `SchnorrPublicKey`
- Provider/key container domain:
  - `PrivateKeyDataProvider` -> `IPrivateKeyDataProvider`
  - `PrivateKeyBase`
  - `PrivateKeys`, `PrivateKeysProvider` -> `IPrivateKeysProvider`
  - `PublicKeys`, `PublicKeysProvider` -> `IPublicKeysProvider`
- PQ:
  - `MLDSA`, `MLDSAPrivateKey`, `MLDSAPublicKey`, `MLDSASignature`
    -> `MLDSALevel`, `MLDSAPrivateKey`, `MLDSAPublicKey`, `MLDSASignature`
  - `MLKEM`, `MLKEMPrivateKey`, `MLKEMPublicKey`, `MLKEMCiphertext`
    -> `MLKEMLevel`, `MLKEMPrivateKey`, `MLKEMPublicKey`, `MLKEMCiphertext`
- Encapsulation:
  - `EncapsulationScheme`, `EncapsulationPrivateKey`, `EncapsulationPublicKey`,
    `EncapsulationCiphertext`, `SealedMessage`
- SSKR bridge:
  - `SSKRShare` -> `SSKRShare`
  - `SSKRSecret`, `SSKRSpec`, `SSKRGroupSpec`, `SSKRError`
    -> provided by dependency namespace `BlockchainCommons.SSKR`
  - `HKDFRng`

### Functions
- `register_tags()` -> `TagsRegistry.RegisterTags()`
- `register_tags_in(tags_store)` -> `TagsRegistry.RegisterTagsIn(TagsStore)`
- `sskr_generate(...)` -> `SSKRShare.SskrGenerate(...)`
- `sskr_generate_using(...)` -> `SSKRShare.SskrGenerateUsing(...)`
- `sskr_combine(...)` -> `SSKRShare.SskrCombine(...)`
- `keypair()` -> `Keypair.Generate()`
- `keypair_using(rng)` -> `Keypair.GenerateUsing(IRandomNumberGenerator)`
- `keypair_opt(...)` -> `Keypair.GenerateUsing(rng, signingScheme, encapsulationScheme)`
- `keypair_opt_using(...)` -> `Keypair.GenerateUsing(rng, signingScheme, encapsulationScheme)`

### Constants
- `SALT_LEN` and other size constants represented as public constants/properties
  on translated C# types (`Size`, `MinSize`, key-size constants).

## Documentation Catalog
- Crate-level Rust docs exist in `src/lib.rs`.
- C# public API includes XML doc comments for exposed types and methods.
- Package metadata includes description in `BCComponents.csproj`.

## Test Inventory (Rust -> C#)
Rust tests are primarily module tests in `rust/bc-components/src/**` plus
integration-style vectors in `src/lib.rs`.

C# translated coverage (`BCComponents.Tests`):
- digest, nonce, symmetric encryption, EC keys, X25519, signing, ML-DSA, ML-KEM
- encapsulation + sealed message behavior
- compressed, encrypted key (HKDF/PBKDF2/Scrypt/Argon2id), HKDF RNG
- JSON, private key container, SSKR share, ARID/XID/UUID/URI

Out-of-scope test areas:
- `ssh-agent` feature tests
- Rust metadata-only tests (`test_readme_deps`, `test_html_root_url`)

## Expected Text Output Rubric
- Applicable where exact text vectors are required (UR/CBOR diagnostic or SSH text).
- In this translation, vector-sensitive tests are retained where implemented;
  SSH-agent specific vectors are intentionally excluded with feature rationale.

## Translation Hazards
1. CBOR tagging and discriminant layout must stay byte-for-byte compatible.
2. UR strings and key/signature vectors are exact-output sensitive.
3. PQ level/type mappings (`MLDSA*`, `MLKEM*`) must preserve scheme identity.
4. KDF parameter variants must roundtrip without shape drift.
5. API evolution policy is de novo: no compatibility wrappers or shims.

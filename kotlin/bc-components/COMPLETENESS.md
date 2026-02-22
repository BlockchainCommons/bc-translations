# Completeness: bc-components → Kotlin (bc-components)

## Build & Config
- [x] .gitignore
- [x] build.gradle.kts
- [x] settings.gradle.kts

## Source Files — Unit 1: Error Types
- [x] BcComponentsException.kt

## Source Files — Unit 2: Core Value Types
- [x] Digest.kt
- [x] DigestProvider.kt
- [x] Nonce.kt
- [x] Salt.kt
- [x] Compressed.kt
- [x] CborJson.kt
- [x] Reference.kt / ReferenceProvider.kt
- [x] Seed.kt

## Source Files — Unit 3: Symmetric Cryptography
- [x] SymmetricKey.kt
- [x] AuthenticationTag.kt
- [x] EncryptedMessage.kt

## Source Files — Unit 4: X25519 Key Agreement
- [x] X25519PrivateKey.kt
- [x] X25519PublicKey.kt

## Source Files — Unit 5: Ed25519 Signing
- [x] Ed25519PrivateKey.kt
- [x] Ed25519PublicKey.kt

## Source Files — Unit 6: EC Key (secp256k1)
- [x] ECKeyBase.kt (includes ECKeyBase, ECKey, ECPublicKeyBase interfaces)
- [x] ECPrivateKey.kt
- [x] ECPublicKey.kt
- [x] ECUncompressedPublicKey.kt
- [x] SchnorrPublicKey.kt

## Source Files — Unit 7: Signing Framework
- [x] SignatureScheme.kt
- [x] Signature.kt (Schnorr, ECDSA, Ed25519, MLDSA)
- [x] SigningPrivateKey.kt (Schnorr, ECDSA, Ed25519, MLDSA)
- [x] SigningPublicKey.kt (Schnorr, ECDSA, Ed25519, MLDSA)
- [x] Signer.kt / Verifier.kt
- [x] SigningOptions.kt
- [ ] SSH variants (SSHKey in SigningPrivateKey/PublicKey/Signature) — deferred

## Source Files — Unit 8: Post-Quantum ML-DSA
- [x] MLDSA.kt
- [x] MLDSAPrivateKey.kt
- [x] MLDSAPublicKey.kt
- [x] MLDSASignature.kt

## Source Files — Unit 9: Post-Quantum ML-KEM
- [x] MLKEM.kt
- [x] MLKEMPrivateKey.kt
- [x] MLKEMPublicKey.kt
- [x] MLKEMCiphertext.kt

## Source Files — Unit 10: Encapsulation Framework
- [x] EncapsulationScheme.kt
- [x] EncapsulationPrivateKey.kt
- [x] EncapsulationPublicKey.kt
- [x] EncapsulationCiphertext.kt
- [x] SealedMessage.kt
- [x] Encrypter.kt / Decrypter.kt

## Source Files — Unit 11: Key Encryption
- [x] HashType.kt
- [x] KeyDerivation.kt (interface)
- [x] KeyDerivationMethod.kt
- [x] HKDFParams.kt
- [x] PBKDF2Params.kt
- [x] ScryptParams.kt
- [x] Argon2idParams.kt
- [x] KeyDerivationParams.kt
- [x] EncryptedKey.kt

## Source Files — Unit 12: SSKR Wrapper
- [x] SSKRShare.kt (includes SSKR functions)

## Source Files — Unit 13: Key Management
- [x] PrivateKeyDataProvider.kt
- [x] HKDFRng.kt
- [x] PrivateKeyBase.kt
- [x] PrivateKeys.kt / PrivateKeysProvider.kt
- [x] PublicKeys.kt / PublicKeysProvider.kt
- [x] Keypair.kt

## Source Files — Unit 14: Identifier Types
- [x] ARID.kt
- [x] URI.kt
- [x] UUID.kt
- [x] XID.kt / XIDProvider.kt

## Source Files — Unit 15: Tags Registry
- [x] TagsRegistry.kt

## Tests (19 files, 131 tests)
- [x] DigestTest.kt (8 tests)
- [x] CompressedTest.kt (7 tests)
- [x] NonceTest.kt (6 tests)
- [x] CborJsonTest.kt (8 tests)
- [x] SymmetricKeyTest.kt (6 tests)
- [x] X25519Test.kt (5 tests)
- [x] ECKeyTest.kt (12 tests)
- [x] SigningTest.kt (9 tests)
- [x] MLDSATest.kt (4 tests)
- [x] MLKEMTest.kt (3 tests)
- [x] EncapsulationTest.kt (6 tests)
- [x] EncryptedKeyTest.kt (8 tests)
- [x] SSKRShareTest.kt (5 tests)
- [x] PrivateKeyBaseTest.kt (6 tests)
- [x] XIDTest.kt (7 tests)
- [x] HKDFRngTest.kt (4 tests)
- [x] URITest.kt (6 tests)
- [x] UUIDTest.kt (9 tests)
- [x] ARIDTest.kt (11 tests)
- [ ] SSH-related tests — deferred with SSH implementation

## Known Gaps
- SSH support (SSHPrivateKey, SSHPublicKey, SshSig, SSH variants in signing framework) — the manifest lists SSH as "always enabled" but it requires significant manual implementation of OpenSSH format parsing/formatting (Hazards H1 and H2). Deferred as future work.

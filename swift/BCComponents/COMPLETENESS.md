# Completeness: bc-components → Swift (BCComponents)

## Source Files
- [x] Rust API surface mapped in `MANIFEST.md`
- [x] Core error and utility modules translated (`Error.swift`, `Utils.swift`)
- [x] ID baseline translated (`ARID`, `UUID`, `URI`, `XID`)
- [x] XID integrations with signing/public-key aggregates (`SigningPublicKey`, `PublicKeys`, `PrivateKeyBase`) translated
- [x] Digest and reference baseline translated (`Digest.swift`, `Reference.swift`, `DigestProvider.swift`)
- [x] `Compressed` translated
- [x] Symmetric encryption modules translated (`SymmetricKey`, `EncryptedMessage`, `AuthenticationTag`)
- [x] Key material baseline translated (`Salt`, `Nonce`, `JSON`)
- [x] `Seed` translated
- [x] `PrivateKeyBase` translated (including SSH Ed25519/ECDSA P-256/P-384 paths)
- [x] SSKR wrapper baseline translated (`SSKRShare`, `sskrGenerate`, `sskrGenerateUsing`, `sskrCombine`)
- [x] Tag registration translated (`registerTagsIn`, `registerTags`)
- [x] Full Rust tags-registry summarizer behavior translated for non-SSH BC component types
- [x] `HKDFRng` translated
- [x] Signing key modules translated (`Signature`, `SigningPrivateKey`, `SigningPublicKey`, `SignatureScheme`, traits) including SSH Ed25519/ECDSA P-256/P-384
- [x] secp256k1 key modules translated (`ECPrivateKey`, `ECPublicKey`, `SchnorrPublicKey`, etc.)
- [x] Ed25519 key wrapper modules translated
- [x] Encapsulation modules translated (`Encapsulation*`, `SealedMessage`, `Encrypter`, `Decrypter`)
- [x] Encrypted-key derivation modules translated (`EncryptedKey`, params, methods)
- [x] Key aggregates translated (`PrivateKeys`, `PublicKeys`, providers, `keypair*`)
- [x] Post-quantum modules translated (`MLDSA*`, `MLKEM*`)
- [ ] SSH-related modules fully translated (remaining: DSA, P-521, and Rust parity vectors)

## Tests
- [x] Rust test inventory fully mapped in `MANIFEST.md`
- [ ] All translatable behavior tests ported to Swift (`85/97` currently ported)
- [x] Rust metadata/version-sync tests marked N/A with reason in `MANIFEST.md`
- [x] Ported vector tests match Rust outputs byte-for-byte (digest, x25519, symmetric, signing, hkdf)

## Build & Config
- [x] `.gitignore`
- [x] `Package.swift`
- [x] Swift package builds successfully
- [x] All Swift tests pass

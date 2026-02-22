# Completeness: bc-components → Swift (BCComponents)

## Source Files
- [x] Rust API surface mapped in `MANIFEST.md`
- [x] Core error and utility modules translated (`Error.swift`, `Utils.swift`)
- [x] ID baseline translated (`ARID`, `UUID`, `URI`, `XID`)
- [ ] XID integrations with signing/public-key aggregates (`SigningPublicKey`, `PublicKeys`, `PrivateKeyBase`) translated
- [x] Digest and reference baseline translated (`Digest.swift`, `Reference.swift`, `DigestProvider.swift`)
- [ ] `Compressed` translated
- [x] Symmetric encryption modules translated (`SymmetricKey`, `EncryptedMessage`, `AuthenticationTag`)
- [x] Key material baseline translated (`Salt`, `Nonce`, `JSON`)
- [ ] `Seed` translated
- [ ] `PrivateKeyBase` translated
- [x] SSKR wrapper baseline translated (`SSKRShare`, `sskrGenerate`, `sskrGenerateUsing`, `sskrCombine`)
- [x] Tag registration translated (`registerTagsIn`, `registerTags`)
- [ ] Full Rust tags-registry summarizer behavior translated
- [ ] `HKDFRng` translated
- [ ] Signing key modules translated (`Signature`, `SigningPrivateKey`, `SigningPublicKey`, `SignatureScheme`, traits)
- [ ] secp256k1 key modules translated (`ECPrivateKey`, `ECPublicKey`, `SchnorrPublicKey`, etc.)
- [ ] Ed25519 key wrapper modules translated
- [ ] Encapsulation modules translated (`Encapsulation*`, `SealedMessage`, `Encrypter`, `Decrypter`)
- [ ] Encrypted-key derivation modules translated (`EncryptedKey`, params, methods)
- [ ] Key aggregates translated (`PrivateKeys`, `PublicKeys`, providers, `keypair*`)
- [ ] Post-quantum modules translated (`MLDSA*`, `MLKEM*`)
- [ ] SSH-related modules translated (default-feature scope in Rust)

## Tests
- [x] Rust test inventory fully mapped in `MANIFEST.md`
- [ ] All translatable behavior tests ported to Swift (`7/97` currently ported)
- [x] Rust metadata/version-sync tests marked N/A with reason in `MANIFEST.md`
- [x] Ported vector tests match Rust outputs byte-for-byte (digest, x25519, symmetric)

## Build & Config
- [x] `.gitignore`
- [x] `Package.swift`
- [x] Swift package builds successfully
- [x] All Swift tests pass

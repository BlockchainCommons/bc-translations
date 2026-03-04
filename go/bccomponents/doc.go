// Package bccomponents provides cryptographic primitives, identifiers, and
// serialization types for Blockchain Commons.
//
// All types are CBOR-serializable using dCBOR (deterministic CBOR) via the
// dcbor package. Many types also support UR (Uniform Resource) encoding via
// the bcur package.
//
// The package includes:
//   - Core value types: Digest, Nonce, Salt, Compressed, JSON, Reference, Seed
//   - Symmetric encryption: SymmetricKey, EncryptedMessage (ChaCha20-Poly1305)
//   - Key agreement: X25519PrivateKey, X25519PublicKey
//   - Digital signatures: Ed25519, ECDSA/Schnorr (secp256k1), SSH, ML-DSA
//   - Key encapsulation: X25519, ML-KEM (post-quantum)
//   - Sealed messages: SealedMessage (hybrid encrypt)
//   - Key derivation: HKDF, PBKDF2, Scrypt, Argon2id
//   - Identifiers: ARID, UUID, URI, XID
//   - Key management: PrivateKeyBase, PrivateKeys, PublicKeys
//   - SSKR: Sharded secret key reconstruction wrapper
//
// This is a translation of the Rust bc-components crate v0.31.1.
package bccomponents

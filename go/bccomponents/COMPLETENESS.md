# Completeness: bc-components → Go (bccomponents)

## Source Files — Crypto Primitives
- [x] symmetric_key.go — SymmetricKey type with CBOR/UR support
- [x] nonce.go — Nonce type with CBOR support
- [x] salt.go — Salt type with CBOR support
- [x] authentication_tag.go — AuthenticationTag type
- [x] digest.go — Digest type with CBOR/UR support
- [x] compressed.go — Compressed (zlib) with CBOR/UR support

## Source Files — EC Keys
- [x] ec_private_key.go — secp256k1 EC private key derivation
- [x] ec_public_key.go — secp256k1 EC public key with compressed/uncompressed formats
- [x] schnorr_public_key.go — Schnorr (x-only) public key

## Source Files — X25519 Key Agreement
- [x] x25519_private_key.go — X25519 private key with CBOR support
- [x] x25519_public_key.go — X25519 public key with CBOR support

## Source Files — Signing
- [x] signature_scheme.go — SignatureScheme enum (ECDSA, Schnorr, Ed25519, SSH variants, ML-DSA)
- [x] signing_options.go — SigningOptions type
- [x] signing_private_key.go — SigningPrivateKey (multi-scheme) with CBOR/UR support
- [x] signing_public_key.go — SigningPublicKey (multi-scheme) with CBOR/UR support
- [x] signature.go — Signature type with CBOR/UR support
- [x] ed25519_keys.go — Ed25519 key pair with CBOR support
- [x] ssh_keys.go — SSH key generation (Ed25519), sshsig signing/verification

## Source Files — Post-Quantum
- [x] mldsa.go — ML-DSA (44/65/87) key generation, signing, verification via circl
- [x] mlkem.go — ML-KEM (512/768/1024) key encapsulation/decapsulation via circl

## Source Files — Encryption
- [x] encrypted_message.go — EncryptedMessage (IETF ChaCha20-Poly1305) with CBOR/UR support
- [x] encapsulation.go — EncapsulationPrivateKey/PublicKey (X25519/ML-KEM) with CBOR support

## Source Files — Key Management
- [x] private_key_base.go — PrivateKeyBase with key derivation methods
- [x] private_keys.go — PrivateKeys (signing + encapsulation pair)
- [x] public_keys.go — PublicKeys (signing + encapsulation pair)
- [x] keypair.go — Keypair convenience functions
- [x] hkdf_rng.go — HKDF-based deterministic RNG for key derivation

## Source Files — Encrypted Keys
- [x] hash_type.go — HashType enum (SHA256, SHA512)
- [x] key_derivation_method.go — KDMethod enum (HKDF, PBKDF2, Scrypt, Argon2id)
- [x] key_derivation_params.go — KeyDerivationParams discriminated union
- [x] hkdf_params.go — HKDF parameters with Lock/Unlock
- [x] pbkdf2_params.go — PBKDF2 parameters with Lock/Unlock
- [x] scrypt_params.go — Scrypt parameters with Lock/Unlock
- [x] argon2id_params.go — Argon2id parameters with Lock/Unlock
- [x] encrypted_key.go — EncryptedKey with CBOR/UR support

## Source Files — Identifiers
- [x] arid.go — ARID (Apparently Random Identifier) with CBOR/UR support
- [x] uuid.go — UUID (RFC 4122 Type 4) with CBOR/UR support
- [x] xid.go — XID (eXtensible Identifier) with CBOR/UR support
- [x] uri.go — URI type with CBOR support

## Source Files — Miscellaneous
- [x] seed.go — Seed type with CBOR/UR support
- [x] json_type.go — JSON wrapper type with CBOR support
- [x] reference.go — Reference type for content-addressed identifiers
- [x] interfaces.go — Shared interfaces (Encrypter, Signer, etc.)
- [x] error.go — Error types
- [x] doc.go — Package documentation
- [x] tags_registry.go — CBOR tag registration for all types

## Tests
- [x] bccomponents_test.go — X25519 keys, agreement, ECDSA signing keys, ECDSA signing
- [x] encrypted_key_test.go — HKDF/PBKDF2/Scrypt/Argon2id roundtrips, wrong secret, params variant
- [x] id_test.go — XID, XIDFromKey, ARID, UUID, URI
- [x] sskr_test.go — SSKR roundtrip, multi-group, share accessors, share CBOR roundtrip
- [x] additional_test.go — Symmetric encryption, encrypted message CBOR, Ed25519, ML-DSA, ML-KEM, sealed message, digest, nonce, seed, compressed, keypair

## Build & Config
- [x] go.mod
- [x] go.sum
- [x] .gitignore

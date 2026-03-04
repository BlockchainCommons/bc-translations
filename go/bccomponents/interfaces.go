package bccomponents

// DigestProvider is implemented by types that can compute a cryptographic digest
// of themselves.
type DigestProvider interface {
	Digest() Digest
}

// KeyDerivation is implemented by parameter types that can derive an
// encryption key from a secret to lock and unlock content keys.
type KeyDerivation interface {
	Lock(contentKey SymmetricKey, secret []byte) (EncryptedMessage, error)
	Unlock(encryptedMessage *EncryptedMessage, secret []byte) (SymmetricKey, error)
	Method() KeyDerivationMethod
}

// ReferenceProvider is implemented by types that can produce a unique reference
// identifier derived from their content.
type ReferenceProvider interface {
	Reference() Reference
}

// PrivateKeyDataProvider is implemented by types that can provide private key
// material as raw bytes.
type PrivateKeyDataProvider interface {
	PrivateKeyData() []byte
}

// PrivateKeysProvider is implemented by types that can provide a complete
// private key bundle.
type PrivateKeysProvider interface {
	PrivateKeys() PrivateKeys
}

// PublicKeysProvider is implemented by types that can provide a complete
// public key bundle.
type PublicKeysProvider interface {
	PublicKeys() PublicKeys
}

// Signer is implemented by types that can produce digital signatures.
type Signer interface {
	Sign(message []byte) (Signature, error)
	SignWithOptions(message []byte, options *SigningOptions) (Signature, error)
}

// Verifier is implemented by types that can verify digital signatures.
type Verifier interface {
	Verify(signature Signature, message []byte) bool
}

// Encrypter is implemented by types that can encapsulate a shared secret for
// a recipient (e.g., public keys in key encapsulation mechanisms).
type Encrypter interface {
	EncapsulateNewSharedSecret() (SymmetricKey, EncapsulationCiphertext, error)
}

// Decrypter is implemented by types that can decapsulate a shared secret from
// a ciphertext (e.g., private keys in key encapsulation mechanisms).
type Decrypter interface {
	DecapsulateSharedSecret(ciphertext EncapsulationCiphertext) (SymmetricKey, error)
}

// ECKeyBase is implemented by elliptic-curve key wrappers that expose binary
// and hexadecimal key material.
type ECKeyBase interface {
	Bytes() []byte
	Hex() string
}

// ECKey is implemented by EC key wrappers that can provide a compressed
// ECDSA public key.
type ECKey interface {
	ECKeyBase
	PublicKey() ECPublicKey
}

// ECPublicKeyBase is implemented by EC public key wrappers that can provide an
// uncompressed representation.
type ECPublicKeyBase interface {
	ECKey
	UncompressedPublicKey() ECUncompressedPublicKey
}

var (
	_ KeyDerivation       = (*HKDFParams)(nil)
	_ KeyDerivation       = (*PBKDF2Params)(nil)
	_ KeyDerivation       = (*ScryptParams)(nil)
	_ KeyDerivation       = (*Argon2idParams)(nil)
	_ PrivateKeysProvider = PrivateKeys{}
	_ PrivateKeysProvider = (*PrivateKeyBase)(nil)
	_ PublicKeysProvider  = PublicKeys{}
	_ PublicKeysProvider  = (*PrivateKeyBase)(nil)
	_ ECKey               = ECPrivateKey{}
	_ ECPublicKeyBase     = ECPublicKey{}
	_ ECPublicKeyBase     = ECUncompressedPublicKey{}
)

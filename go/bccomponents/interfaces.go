package bccomponents

// DigestProvider is implemented by types that can compute a cryptographic digest
// of themselves.
type DigestProvider interface {
	Digest() Digest
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

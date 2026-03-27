package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// AddSignature creates a signature for the envelope's subject and returns a
// new envelope with a 'signed': Signature assertion.
func (e *Envelope) AddSignature(privateKey bccomponents.Signer) *Envelope {
	return e.AddSignatureOpt(privateKey, nil, nil)
}

// AddSignatureOpt creates a signature for the envelope's subject with optional
// signing options and metadata. If metadata is provided and has assertions,
// the metadata will also be signed.
func (e *Envelope) AddSignatureOpt(
	privateKey bccomponents.Signer,
	options *bccomponents.SigningOptions,
	metadata *SignatureMetadata,
) *Envelope {
	digestData := e.Subject().Digest().Bytes()
	sig, err := privateKey.SignWithOptions(digestData, options)
	if err != nil {
		panic("bcenvelope: AddSignatureOpt: sign failed: " + err.Error())
	}
	signature := NewEnvelope(sig)

	if metadata != nil && metadata.HasAssertions() {
		signatureWithMetadata := signature
		for _, assertion := range metadata.Assertions() {
			var err error
			signatureWithMetadata, err = signatureWithMetadata.AddAssertionEnvelope(assertion.ToEnvelope())
			if err != nil {
				panic("bcenvelope: AddSignatureOpt: add assertion failed: " + err.Error())
			}
		}

		signatureWithMetadata = signatureWithMetadata.Wrap()

		outerSig, err := privateKey.SignWithOptions(signatureWithMetadata.Digest().Bytes(), options)
		if err != nil {
			panic("bcenvelope: AddSignatureOpt: outer sign failed: " + err.Error())
		}
		outerSignature := NewEnvelope(outerSig)
		signature = signatureWithMetadata.AddAssertion(knownvalues.Signed, outerSignature)
	}

	return e.AddAssertion(knownvalues.Signed, signature)
}

// AddSignatures creates several signatures for the envelope's subject and
// returns a new envelope with additional 'signed': Signature assertions.
func (e *Envelope) AddSignatures(privateKeys []bccomponents.Signer) *Envelope {
	result := e
	for _, key := range privateKeys {
		result = result.AddSignature(key)
	}
	return result
}

// AddSignaturesOpt creates several signatures with optional signing options
// and metadata.
func (e *Envelope) AddSignaturesOpt(
	entries []SignatureEntry,
) *Envelope {
	result := e
	for _, entry := range entries {
		result = result.AddSignatureOpt(entry.Signer, entry.Options, entry.Metadata)
	}
	return result
}

// SignatureEntry holds a signer along with optional signing options and metadata.
type SignatureEntry struct {
	Signer   bccomponents.Signer
	Options  *bccomponents.SigningOptions
	Metadata *SignatureMetadata
}

// MakeSignedAssertion creates a convenience 'signed': Signature assertion
// envelope with an optional note.
func (e *Envelope) MakeSignedAssertion(
	signature bccomponents.Signature,
	note *string,
) *Envelope {
	envelope := NewAssertionEnvelope(knownvalues.Signed, signature)
	if note != nil {
		envelope = envelope.AddAssertion(knownvalues.Note, *note)
	}
	return envelope
}

// IsVerifiedSignature returns whether the given signature is valid for this
// envelope's subject with the given public key.
func (e *Envelope) IsVerifiedSignature(
	signature bccomponents.Signature,
	publicKey bccomponents.Verifier,
) bool {
	return e.isSignatureFromKey(signature, publicKey)
}

// VerifySignature checks whether the given signature is valid for the given
// public key. Returns the envelope if valid, or ErrUnverifiedSignature.
func (e *Envelope) VerifySignature(
	signature bccomponents.Signature,
	publicKey bccomponents.Verifier,
) (*Envelope, error) {
	if !e.isSignatureFromKey(signature, publicKey) {
		return nil, ErrUnverifiedSignature
	}
	return e, nil
}

// HasSignatureFrom returns whether the envelope's subject has a valid
// signature from the given public key.
func (e *Envelope) HasSignatureFrom(publicKey bccomponents.Verifier) (bool, error) {
	return e.hasSomeSignatureFromKey(publicKey)
}

// HasSignatureFromReturningMetadata returns the metadata envelope if the
// envelope's subject has a valid signature from the given public key, or
// nil otherwise.
func (e *Envelope) HasSignatureFromReturningMetadata(
	publicKey bccomponents.Verifier,
) (*Envelope, error) {
	return e.hasSomeSignatureFromKeyReturningMetadata(publicKey)
}

// VerifySignatureFrom checks whether the envelope's subject has a valid
// signature from the given public key. Returns the envelope if valid, or
// ErrUnverifiedSignature.
func (e *Envelope) VerifySignatureFrom(publicKey bccomponents.Verifier) (*Envelope, error) {
	ok, err := e.hasSomeSignatureFromKey(publicKey)
	if err != nil {
		return nil, err
	}
	if !ok {
		return nil, ErrUnverifiedSignature
	}
	return e, nil
}

// VerifySignatureFromReturningMetadata verifies the signature and returns the
// metadata envelope. Returns ErrUnverifiedSignature if no valid signature is
// found.
func (e *Envelope) VerifySignatureFromReturningMetadata(
	publicKey bccomponents.Verifier,
) (*Envelope, error) {
	metadata, err := e.hasSomeSignatureFromKeyReturningMetadata(publicKey)
	if err != nil {
		return nil, err
	}
	if metadata == nil {
		return nil, ErrUnverifiedSignature
	}
	return metadata, nil
}

// HasSignaturesFrom checks whether the envelope's subject has valid signatures
// from all of the given public keys.
func (e *Envelope) HasSignaturesFrom(publicKeys []bccomponents.Verifier) (bool, error) {
	return e.HasSignaturesFromThreshold(publicKeys, 0)
}

// HasSignaturesFromThreshold returns whether the envelope's subject has at
// least threshold valid signatures from the given public keys. If threshold
// is 0, all signers must have signed.
func (e *Envelope) HasSignaturesFromThreshold(
	publicKeys []bccomponents.Verifier,
	threshold int,
) (bool, error) {
	if threshold == 0 {
		threshold = len(publicKeys)
	}
	count := 0
	for _, key := range publicKeys {
		ok, err := e.hasSomeSignatureFromKey(key)
		if err != nil {
			return false, err
		}
		if ok {
			count++
			if count >= threshold {
				return true, nil
			}
		}
	}
	return false, nil
}

// VerifySignaturesFromThreshold checks whether the envelope's subject has at
// least threshold valid signatures. Returns the envelope if met, or
// ErrUnverifiedSignature.
func (e *Envelope) VerifySignaturesFromThreshold(
	publicKeys []bccomponents.Verifier,
	threshold int,
) (*Envelope, error) {
	ok, err := e.HasSignaturesFromThreshold(publicKeys, threshold)
	if err != nil {
		return nil, err
	}
	if !ok {
		return nil, ErrUnverifiedSignature
	}
	return e, nil
}

// VerifySignaturesFrom checks whether the envelope's subject has valid
// signatures from all of the given public keys. Returns the envelope if
// valid, or ErrUnverifiedSignature.
func (e *Envelope) VerifySignaturesFrom(publicKeys []bccomponents.Verifier) (*Envelope, error) {
	return e.VerifySignaturesFromThreshold(publicKeys, 0)
}

// --- Internal helpers ---

func (e *Envelope) isSignatureFromKey(
	signature bccomponents.Signature,
	key bccomponents.Verifier,
) bool {
	return key.Verify(signature, e.Subject().Digest().Bytes())
}

func (e *Envelope) hasSomeSignatureFromKey(key bccomponents.Verifier) (bool, error) {
	metadata, err := e.hasSomeSignatureFromKeyReturningMetadata(key)
	if err != nil {
		return false, err
	}
	return metadata != nil, nil
}

func (e *Envelope) hasSomeSignatureFromKeyReturningMetadata(
	key bccomponents.Verifier,
) (*Envelope, error) {
	signatureObjects := e.ObjectsForPredicate(knownvalues.Signed)

	for _, signatureObject := range signatureObjects {
		signatureObjectSubject := signatureObject.Subject()

		if signatureObjectSubject.IsWrapped() {
			// Metadata-bearing signature: wrapped envelope with outer signature
			outerSigObj, err := signatureObject.ObjectForPredicate(knownvalues.Signed)
			if err == nil {
				outerSig, err := ExtractSubject[bccomponents.Signature](outerSigObj)
				if err != nil {
					return nil, ErrInvalidOuterSignatureType
				}
				if !signatureObjectSubject.isSignatureFromKey(outerSig, key) {
					continue
				}
			}

			signatureMetadataEnvelope, err := signatureObjectSubject.Unwrap()
			if err != nil {
				continue
			}

			sig, err := ExtractSubject[bccomponents.Signature](signatureMetadataEnvelope)
			if err != nil {
				return nil, ErrInvalidInnerSignatureType
			}

			if !e.Subject().isSignatureFromKey(sig, key) {
				return nil, ErrUnverifiedInnerSignature
			}

			return signatureMetadataEnvelope, nil
		}

		// Simple signature (no metadata)
		sig, err := ExtractSubject[bccomponents.Signature](signatureObject)
		if err != nil {
			return nil, ErrInvalidSignatureType
		}

		if !e.isSignatureFromKey(sig, key) {
			continue
		}

		return signatureObject, nil
	}

	return nil, nil
}

// --- Convenience methods for sign/verify entire envelopes ---

// Sign signs the entire envelope (subject and assertions) by wrapping it first
// and adding a signature assertion.
func (e *Envelope) Sign(signer bccomponents.Signer) *Envelope {
	return e.SignOpt(signer, nil)
}

// SignOpt signs the entire envelope with optional signing options.
func (e *Envelope) SignOpt(signer bccomponents.Signer, options *bccomponents.SigningOptions) *Envelope {
	return e.Wrap().AddSignatureOpt(signer, options, nil)
}

// Verify verifies that the envelope has a valid signature from the specified
// verifier, then unwraps the envelope.
func (e *Envelope) Verify(verifier bccomponents.Verifier) (*Envelope, error) {
	verified, err := e.VerifySignatureFrom(verifier)
	if err != nil {
		return nil, err
	}
	return verified.Unwrap()
}

// VerifyReturningMetadata verifies the envelope's signature and returns both
// the unwrapped envelope and the signature metadata envelope.
func (e *Envelope) VerifyReturningMetadata(
	verifier bccomponents.Verifier,
) (*Envelope, *Envelope, error) {
	metadata, err := e.VerifySignatureFromReturningMetadata(verifier)
	if err != nil {
		return nil, nil, err
	}
	unwrapped, err := e.Unwrap()
	if err != nil {
		return nil, nil, err
	}
	return unwrapped, metadata, nil
}

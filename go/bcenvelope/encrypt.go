package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// EncryptSubject returns a new envelope with its subject encrypted using the
// given symmetric key. The encryption preserves the envelope's digest,
// allowing signatures and proofs to remain valid.
func (e *Envelope) EncryptSubject(key bccomponents.SymmetricKey) (*Envelope, error) {
	return e.EncryptSubjectWithNonce(key, nil)
}

// EncryptSubjectWithNonce is an internal method for encrypting with an optional
// test nonce. Production code should use EncryptSubject.
func (e *Envelope) EncryptSubjectWithNonce(key bccomponents.SymmetricKey, testNonce *bccomponents.Nonce) (*Envelope, error) {
	var result *Envelope
	var originalDigest bccomponents.Digest

	switch c := e.Case().(type) {
	case *NodeCase:
		if c.Subject.IsEncrypted() {
			return nil, ErrAlreadyEncrypted
		}
		encodedCBOR := c.Subject.TaggedCBOR().ToCBORData()
		digest := c.Subject.Digest()
		encryptedMessage := key.EncryptWithDigest(encodedCBOR, digest, testNonce)
		encryptedSubject := NewEncryptedEnvelope(&encryptedMessage)
		result = newWithUncheckedAssertions(encryptedSubject, c.Assertions)
		originalDigest = *c.Digest

	case *LeafCase:
		encodedCBOR := dcbor.ToTaggedValue(TagEnvelope, dcbor.ToTaggedValue(TagLeaf, *c.CBOR)).ToCBORData()
		encryptedMessage := key.EncryptWithDigest(encodedCBOR, *c.Digest, testNonce)
		result = NewEncryptedEnvelope(&encryptedMessage)
		originalDigest = *c.Digest

	case *WrappedCase:
		encodedCBOR := e.TaggedCBOR().ToCBORData()
		encryptedMessage := key.EncryptWithDigest(encodedCBOR, *c.Digest, testNonce)
		result = NewEncryptedEnvelope(&encryptedMessage)
		originalDigest = *c.Digest

	case *KnownValueCase:
		encodedCBOR := dcbor.ToTaggedValue(TagEnvelope, c.Value.UntaggedCBOR()).ToCBORData()
		encryptedMessage := key.EncryptWithDigest(encodedCBOR, *c.Digest, testNonce)
		result = NewEncryptedEnvelope(&encryptedMessage)
		originalDigest = *c.Digest

	case *AssertionCase:
		digest := c.Assertion.Digest()
		encodedCBOR := dcbor.ToTaggedValue(TagEnvelope, c.Assertion.TaggedCBOR()).ToCBORData()
		encryptedMessage := key.EncryptWithDigest(encodedCBOR, *digest, testNonce)
		result = NewEncryptedEnvelope(&encryptedMessage)
		originalDigest = *digest

	case *EncryptedCase:
		return nil, ErrAlreadyEncrypted

	case *CompressedCase:
		digest := c.Compressed.Digest()
		encodedCBOR := dcbor.ToTaggedValue(TagEnvelope, c.Compressed.TaggedCBOR()).ToCBORData()
		encryptedMessage := key.EncryptWithDigest(encodedCBOR, digest, testNonce)
		result = NewEncryptedEnvelope(&encryptedMessage)
		originalDigest = digest

	case *ElidedCase:
		return nil, ErrAlreadyElided

	default:
		return nil, ErrInvalidFormat
	}

	if !result.Digest().Equal(originalDigest) {
		panic("bcenvelope: encrypted digest mismatch")
	}
	return result, nil
}

// DecryptSubject returns a new envelope with its subject decrypted using the
// given symmetric key.
func (e *Envelope) DecryptSubject(key bccomponents.SymmetricKey) (*Envelope, error) {
	subjectCase := e.Subject().Case()
	enc, ok := subjectCase.(*EncryptedCase)
	if !ok {
		return nil, ErrNotEncrypted
	}

	encodedCBOR, err := key.Decrypt(enc.EncryptedMessage)
	if err != nil {
		return nil, err
	}

	subjectDigest := enc.EncryptedMessage.AADDigest()
	if subjectDigest == nil {
		return nil, ErrMissingDigest
	}

	cbor, err := dcbor.TryFromData(encodedCBOR)
	if err != nil {
		return nil, err
	}

	resultSubject, err := FromTaggedCBOR(cbor)
	if err != nil {
		return nil, err
	}

	if !resultSubject.Digest().Equal(*subjectDigest) {
		return nil, ErrInvalidDigest
	}

	if node, ok := e.Case().(*NodeCase); ok {
		result := newWithUncheckedAssertions(resultSubject, node.Assertions)
		if !result.Digest().Equal(*node.Digest) {
			return nil, ErrInvalidDigest
		}
		return result, nil
	}

	return resultSubject, nil
}

// Encrypt is a convenience method that wraps the envelope and then encrypts
// its subject, effectively encrypting the entire envelope including assertions.
func (e *Envelope) Encrypt(key bccomponents.SymmetricKey) *Envelope {
	result, err := e.Wrap().EncryptSubject(key)
	if err != nil {
		panic("bcenvelope: Encrypt: " + err.Error())
	}
	return result
}

// Decrypt is a convenience method that decrypts the subject and unwraps the
// resulting envelope, reversing the Encrypt operation.
func (e *Envelope) Decrypt(key bccomponents.SymmetricKey) (*Envelope, error) {
	decrypted, err := e.DecryptSubject(key)
	if err != nil {
		return nil, err
	}
	return decrypted.Unwrap()
}

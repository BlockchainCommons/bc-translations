package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// AddRecipient returns a new envelope with an added 'hasRecipient': SealedMessage
// assertion. The content key is encrypted to the recipient's public key using
// key encapsulation.
func (e *Envelope) AddRecipient(
	recipient bccomponents.EncapsulationPublicKey,
	contentKey bccomponents.SymmetricKey,
) *Envelope {
	return e.AddRecipientOpt(recipient, contentKey, nil)
}

// AddRecipientOpt adds a recipient assertion with an optional test nonce for
// deterministic testing.
func (e *Envelope) AddRecipientOpt(
	recipient bccomponents.EncapsulationPublicKey,
	contentKey bccomponents.SymmetricKey,
	testNonce *bccomponents.Nonce,
) *Envelope {
	assertion := makeHasRecipient(recipient, contentKey, testNonce)
	result, err := e.AddAssertionEnvelope(assertion)
	if err != nil {
		panic("bcenvelope: AddRecipientOpt: " + err.Error())
	}
	return result
}

// Recipients returns all SealedMessages from the envelope's 'hasRecipient'
// assertions.
func (e *Envelope) Recipients() ([]*bccomponents.SealedMessage, error) {
	assertions := e.AssertionsWithPredicate(knownvalues.HasRecipient)
	var result []*bccomponents.SealedMessage
	for _, assertion := range assertions {
		obj := assertion.AsObject()
		if obj == nil || obj.IsObscured() {
			continue
		}
		sm, err := ExtractSubject[*bccomponents.SealedMessage](obj)
		if err != nil {
			return nil, err
		}
		result = append(result, sm)
	}
	return result, nil
}

// EncryptSubjectToRecipients encrypts the envelope's subject with a random
// content key and adds recipient assertions for multiple recipients.
func (e *Envelope) EncryptSubjectToRecipients(
	recipients []bccomponents.EncapsulationPublicKey,
) (*Envelope, error) {
	return e.EncryptSubjectToRecipientsOpt(recipients, nil)
}

// EncryptSubjectToRecipientsOpt encrypts the envelope's subject with a random
// content key and adds recipient assertions, with an optional test nonce.
func (e *Envelope) EncryptSubjectToRecipientsOpt(
	recipients []bccomponents.EncapsulationPublicKey,
	testNonce *bccomponents.Nonce,
) (*Envelope, error) {
	contentKey := bccomponents.NewSymmetricKey()
	encrypted, err := e.EncryptSubject(contentKey)
	if err != nil {
		return nil, err
	}
	for _, recipient := range recipients {
		encrypted = encrypted.AddRecipientOpt(recipient, contentKey, testNonce)
	}
	return encrypted, nil
}

// EncryptSubjectToRecipient encrypts the envelope's subject and adds a
// recipient assertion for a single recipient.
func (e *Envelope) EncryptSubjectToRecipient(
	recipient bccomponents.EncapsulationPublicKey,
) (*Envelope, error) {
	return e.EncryptSubjectToRecipientOpt(recipient, nil)
}

// EncryptSubjectToRecipientOpt encrypts the envelope's subject for a single
// recipient with an optional test nonce.
func (e *Envelope) EncryptSubjectToRecipientOpt(
	recipient bccomponents.EncapsulationPublicKey,
	testNonce *bccomponents.Nonce,
) (*Envelope, error) {
	return e.EncryptSubjectToRecipientsOpt(
		[]bccomponents.EncapsulationPublicKey{recipient},
		testNonce,
	)
}

// DecryptSubjectToRecipient decrypts an envelope's subject using the
// recipient's private key. It finds the appropriate sealed message, extracts
// the content key, and decrypts the subject.
func (e *Envelope) DecryptSubjectToRecipient(
	recipient bccomponents.EncapsulationPrivateKey,
) (*Envelope, error) {
	sealedMessages, err := e.Recipients()
	if err != nil {
		return nil, err
	}
	contentKeyData, err := firstPlaintextInSealedMessages(sealedMessages, recipient)
	if err != nil {
		return nil, err
	}
	contentKeyCBOR, err := dcbor.TryFromData(contentKeyData)
	if err != nil {
		return nil, err
	}
	contentKey, err := bccomponents.DecodeTaggedSymmetricKey(contentKeyCBOR)
	if err != nil {
		return nil, err
	}
	return e.DecryptSubject(contentKey)
}

// EncryptToRecipient wraps and encrypts an envelope to a single recipient.
// This is a convenience method combining Wrap and EncryptSubjectToRecipient.
func (e *Envelope) EncryptToRecipient(recipient bccomponents.EncapsulationPublicKey) *Envelope {
	result, err := e.Wrap().EncryptSubjectToRecipient(recipient)
	if err != nil {
		panic("bcenvelope: EncryptToRecipient: " + err.Error())
	}
	return result
}

// DecryptToRecipient decrypts an envelope that was encrypted to a recipient
// and unwraps it. This is a convenience method combining
// DecryptSubjectToRecipient and Unwrap.
func (e *Envelope) DecryptToRecipient(
	recipient bccomponents.EncapsulationPrivateKey,
) (*Envelope, error) {
	decrypted, err := e.DecryptSubjectToRecipient(recipient)
	if err != nil {
		return nil, err
	}
	return decrypted.Unwrap()
}

// --- Internal helpers ---

func firstPlaintextInSealedMessages(
	sealedMessages []*bccomponents.SealedMessage,
	privateKey bccomponents.EncapsulationPrivateKey,
) ([]byte, error) {
	for _, sm := range sealedMessages {
		plaintext, err := sm.Decrypt(privateKey)
		if err == nil {
			return plaintext, nil
		}
	}
	return nil, ErrUnknownRecipient
}

func makeHasRecipient(
	recipient bccomponents.EncapsulationPublicKey,
	contentKey bccomponents.SymmetricKey,
	testNonce *bccomponents.Nonce,
) *Envelope {
	sm, err := bccomponents.NewSealedMessageOpt(
		contentKey.TaggedCBOR().ToCBORData(),
		recipient,
		nil,
		testNonce,
	)
	if err != nil {
		panic("bcenvelope: makeHasRecipient: " + err.Error())
	}
	return NewAssertionEnvelope(knownvalues.HasRecipient, sm)
}

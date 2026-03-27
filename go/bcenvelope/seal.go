package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
)

// Seal seals an envelope by signing it with the sender's key and then
// encrypting it to the recipient. This combines signing and encryption
// in one step, creating a secure, authenticated envelope.
func (e *Envelope) Seal(
	sender bccomponents.Signer,
	recipient bccomponents.EncapsulationPublicKey,
) *Envelope {
	return e.Sign(sender).EncryptToRecipient(recipient)
}

// SealOpt seals an envelope with optional signing options.
func (e *Envelope) SealOpt(
	sender bccomponents.Signer,
	recipient bccomponents.EncapsulationPublicKey,
	options *bccomponents.SigningOptions,
) *Envelope {
	return e.SignOpt(sender, options).EncryptToRecipient(recipient)
}

// Unseal unseals an envelope by decrypting it with the recipient's private
// key and then verifying the signature using the sender's public key. This
// reverses the Seal operation.
func (e *Envelope) Unseal(
	sender bccomponents.Verifier,
	recipient bccomponents.EncapsulationPrivateKey,
) (*Envelope, error) {
	decrypted, err := e.DecryptToRecipient(recipient)
	if err != nil {
		return nil, err
	}
	return decrypted.Verify(sender)
}

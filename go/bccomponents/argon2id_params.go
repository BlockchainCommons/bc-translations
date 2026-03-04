package bccomponents

import (
	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// Argon2idParams holds parameters for Argon2id key derivation.
//
// CDDL: Argon2idParams = [3, Salt]
type Argon2idParams struct {
	salt Salt
}

// NewArgon2idParams creates Argon2idParams with a random salt.
func NewArgon2idParams() Argon2idParams {
	s, _ := NewSaltWithLen(SaltLen)
	return NewArgon2idParamsOpt(s)
}

// NewArgon2idParamsOpt creates Argon2idParams with the given salt.
func NewArgon2idParamsOpt(salt Salt) Argon2idParams {
	return Argon2idParams{salt: salt}
}

// Salt returns the salt.
func (p *Argon2idParams) Salt() Salt { return p.salt }

// Method returns the key derivation method discriminator.
func (p *Argon2idParams) Method() KeyDerivationMethod { return KDMethodArgon2id }

// Lock derives a key from the secret, then encrypts contentKey.
func (p *Argon2idParams) Lock(contentKey SymmetricKey, secret []byte) (EncryptedMessage, error) {
	derivedKeyData := bccrypto.Argon2ID(secret, p.salt.Bytes(), SymmetricKeySize)
	derivedKey, err := SymmetricKeyFromDataRef(derivedKeyData)
	if err != nil {
		return EncryptedMessage{}, err
	}
	encodedMethod := argon2idParamsToCBOR(*p).ToCBORData()
	return derivedKey.Encrypt(contentKey.Bytes(), encodedMethod, nil), nil
}

// Unlock derives a key from the secret, then decrypts the encrypted message.
func (p *Argon2idParams) Unlock(msg *EncryptedMessage, secret []byte) (SymmetricKey, error) {
	derivedKeyData := bccrypto.Argon2ID(secret, p.salt.Bytes(), SymmetricKeySize)
	derivedKey, err := SymmetricKeyFromDataRef(derivedKeyData)
	if err != nil {
		return SymmetricKey{}, err
	}
	plaintext, err := derivedKey.Decrypt(msg)
	if err != nil {
		return SymmetricKey{}, err
	}
	return SymmetricKeyFromDataRef(plaintext)
}

// String returns a human-readable representation.
func (p *Argon2idParams) String() string {
	return "Argon2id"
}

// --- CBOR support ---

func argon2idParamsToCBOR(p Argon2idParams) dcbor.CBOR {
	return dcbor.NewCBORArray([]dcbor.CBOR{
		dcbor.NewCBORUnsigned(uint64(KDMethodArgon2id)),
		p.salt.TaggedCBOR(),
	})
}

func argon2idParamsFromCBOR(cbor dcbor.CBOR) (Argon2idParams, error) {
	a, err := cbor.TryIntoArray()
	if err != nil {
		return Argon2idParams{}, err
	}
	if len(a) != 2 {
		return Argon2idParams{}, dcbor.NewErrorf("Invalid Argon2idParams: expected 2 elements, got %d", len(a))
	}
	salt, err := DecodeTaggedSalt(a[1])
	if err != nil {
		return Argon2idParams{}, err
	}
	return Argon2idParams{salt: salt}, nil
}

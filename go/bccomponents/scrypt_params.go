package bccomponents

import (
	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const (
	defaultScryptLogN uint8  = 15
	defaultScryptR    uint32 = 8
	defaultScryptP    uint32 = 1
)

// ScryptParams holds parameters for scrypt key derivation.
//
// CDDL: ScryptParams = [2, Salt, log_n: uint, r: uint, p: uint]
type ScryptParams struct {
	salt Salt
	logN uint8
	r    uint32
	p    uint32
}

// NewScryptParams creates ScryptParams with defaults (random salt, logN=15, r=8, p=1).
func NewScryptParams() ScryptParams {
	s, _ := NewSaltWithLen(SaltLen)
	return NewScryptParamsOpt(s, defaultScryptLogN, defaultScryptR, defaultScryptP)
}

// NewScryptParamsOpt creates ScryptParams with the given parameters.
func NewScryptParamsOpt(salt Salt, logN uint8, r, p uint32) ScryptParams {
	return ScryptParams{salt: salt, logN: logN, r: r, p: p}
}

// Salt returns the salt.
func (p *ScryptParams) Salt() Salt { return p.salt }

// LogN returns the log2 of the work factor N.
func (p *ScryptParams) LogN() uint8 { return p.logN }

// R returns the block size parameter.
func (p *ScryptParams) R() uint32 { return p.r }

// P returns the parallelism parameter.
func (p *ScryptParams) P() uint32 { return p.p }

// Lock derives a key from the secret, then encrypts contentKey.
func (sp *ScryptParams) Lock(contentKey SymmetricKey, secret []byte) (EncryptedMessage, error) {
	derivedKeyData := bccrypto.ScryptWithParams(secret, sp.salt.Bytes(), SymmetricKeySize, sp.logN, sp.r, sp.p)
	derivedKey, err := SymmetricKeyFromDataRef(derivedKeyData)
	if err != nil {
		return EncryptedMessage{}, err
	}
	encodedMethod := scryptParamsToCBOR(*sp).ToCBORData()
	return derivedKey.Encrypt(contentKey.Bytes(), encodedMethod, nil), nil
}

// Unlock derives a key from the secret, then decrypts the encrypted message.
func (sp *ScryptParams) Unlock(msg *EncryptedMessage, secret []byte) (SymmetricKey, error) {
	derivedKeyData := bccrypto.ScryptWithParams(secret, sp.salt.Bytes(), SymmetricKeySize, sp.logN, sp.r, sp.p)
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
func (sp *ScryptParams) String() string {
	return "Scrypt"
}

// --- CBOR support ---

func scryptParamsToCBOR(sp ScryptParams) dcbor.CBOR {
	return dcbor.NewCBORArray([]dcbor.CBOR{
		dcbor.NewCBORUnsigned(uint64(KDMethodScrypt)),
		sp.salt.TaggedCBOR(),
		dcbor.NewCBORUnsigned(uint64(sp.logN)),
		dcbor.NewCBORUnsigned(uint64(sp.r)),
		dcbor.NewCBORUnsigned(uint64(sp.p)),
	})
}

func scryptParamsFromCBOR(cbor dcbor.CBOR) (ScryptParams, error) {
	a, err := cbor.TryIntoArray()
	if err != nil {
		return ScryptParams{}, err
	}
	if len(a) != 5 {
		return ScryptParams{}, dcbor.NewErrorf("Invalid ScryptParams: expected 5 elements, got %d", len(a))
	}
	salt, err := DecodeTaggedSalt(a[1])
	if err != nil {
		return ScryptParams{}, err
	}
	logN, err := a[2].TryIntoUInt8()
	if err != nil {
		return ScryptParams{}, err
	}
	r, err := a[3].TryIntoUInt32()
	if err != nil {
		return ScryptParams{}, err
	}
	p, err := a[4].TryIntoUInt32()
	if err != nil {
		return ScryptParams{}, err
	}
	return ScryptParams{salt: salt, logN: logN, r: r, p: p}, nil
}

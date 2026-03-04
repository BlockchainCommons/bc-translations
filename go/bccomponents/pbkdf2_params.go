package bccomponents

import (
	"fmt"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const defaultPBKDF2Iterations uint32 = 100_000

// PBKDF2Params holds parameters for PBKDF2 key derivation.
//
// CDDL: PBKDF2Params = [1, Salt, iterations: uint, HashType]
type PBKDF2Params struct {
	salt       Salt
	iterations uint32
	hashType   HashType
}

// NewPBKDF2Params creates PBKDF2Params with defaults (random salt, 100k iterations, SHA-256).
func NewPBKDF2Params() PBKDF2Params {
	s, _ := NewSaltWithLen(SaltLen)
	return NewPBKDF2ParamsOpt(s, defaultPBKDF2Iterations, HashTypeSHA256)
}

// NewPBKDF2ParamsOpt creates PBKDF2Params with the given parameters.
func NewPBKDF2ParamsOpt(salt Salt, iterations uint32, hashType HashType) PBKDF2Params {
	return PBKDF2Params{salt: salt, iterations: iterations, hashType: hashType}
}

// Salt returns the salt.
func (p *PBKDF2Params) Salt() Salt { return p.salt }

// Iterations returns the iteration count.
func (p *PBKDF2Params) Iterations() uint32 { return p.iterations }

// HashType returns the hash type.
func (p *PBKDF2Params) HashType() HashType { return p.hashType }

// Method returns the key derivation method discriminator.
func (p *PBKDF2Params) Method() KeyDerivationMethod { return KDMethodPBKDF2 }

// Lock derives a key from the secret, then encrypts contentKey.
func (p *PBKDF2Params) Lock(contentKey SymmetricKey, secret []byte) (EncryptedMessage, error) {
	derivedKeyData := p.deriveKey(secret)
	derivedKey, err := SymmetricKeyFromDataRef(derivedKeyData)
	if err != nil {
		return EncryptedMessage{}, err
	}
	encodedMethod := pbkdf2ParamsToCBOR(*p).ToCBORData()
	return derivedKey.Encrypt(contentKey.Bytes(), encodedMethod, nil), nil
}

// Unlock derives a key from the secret, then decrypts the encrypted message.
func (p *PBKDF2Params) Unlock(msg *EncryptedMessage, secret []byte) (SymmetricKey, error) {
	derivedKeyData := p.deriveKey(secret)
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

func (p *PBKDF2Params) deriveKey(secret []byte) []byte {
	switch p.hashType {
	case HashTypeSHA256:
		return bccrypto.PBKDF2HMACSHA256(secret, p.salt.Bytes(), p.iterations, SymmetricKeySize)
	case HashTypeSHA512:
		return bccrypto.PBKDF2HMACSHA512(secret, p.salt.Bytes(), p.iterations, SymmetricKeySize)
	default:
		return bccrypto.PBKDF2HMACSHA256(secret, p.salt.Bytes(), p.iterations, SymmetricKeySize)
	}
}

// String returns a human-readable representation.
func (p *PBKDF2Params) String() string {
	return fmt.Sprintf("PBKDF2(%s)", p.hashType)
}

// --- CBOR support ---

func pbkdf2ParamsToCBOR(p PBKDF2Params) dcbor.CBOR {
	return dcbor.NewCBORArray([]dcbor.CBOR{
		dcbor.NewCBORUnsigned(uint64(KDMethodPBKDF2)),
		p.salt.TaggedCBOR(),
		dcbor.NewCBORUnsigned(uint64(p.iterations)),
		hashTypeToCBOR(p.hashType),
	})
}

func pbkdf2ParamsFromCBOR(cbor dcbor.CBOR) (PBKDF2Params, error) {
	a, err := cbor.TryIntoArray()
	if err != nil {
		return PBKDF2Params{}, err
	}
	if len(a) != 4 {
		return PBKDF2Params{}, dcbor.NewErrorf("Invalid PBKDF2Params: expected 4 elements, got %d", len(a))
	}
	salt, err := DecodeTaggedSalt(a[1])
	if err != nil {
		return PBKDF2Params{}, err
	}
	iterations, err := a[2].TryIntoUInt32()
	if err != nil {
		return PBKDF2Params{}, err
	}
	hashType, err := hashTypeFromCBOR(a[3])
	if err != nil {
		return PBKDF2Params{}, err
	}
	return PBKDF2Params{salt: salt, iterations: iterations, hashType: hashType}, nil
}

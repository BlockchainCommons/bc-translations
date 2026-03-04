package bccomponents

import (
	"fmt"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// SaltLen is the default salt length in bytes for key derivation.
const SaltLen = 16

// HKDFParams holds parameters for HKDF key derivation.
//
// CDDL: HKDFParams = [0, Salt, HashType]
type HKDFParams struct {
	salt     Salt
	hashType HashType
}

// NewHKDFParams creates HKDFParams with a random salt and SHA-256.
func NewHKDFParams() HKDFParams {
	s, _ := NewSaltWithLen(SaltLen)
	return NewHKDFParamsOpt(s, HashTypeSHA256)
}

// NewHKDFParamsOpt creates HKDFParams with the given salt and hash type.
func NewHKDFParamsOpt(salt Salt, hashType HashType) HKDFParams {
	return HKDFParams{salt: salt, hashType: hashType}
}

// Salt returns the salt.
func (p *HKDFParams) Salt() Salt { return p.salt }

// HashType returns the hash type.
func (p *HKDFParams) HashType() HashType { return p.hashType }

// Lock derives a key from the secret, then encrypts contentKey using that
// derived key. The CBOR encoding of the params is used as AAD.
func (p *HKDFParams) Lock(contentKey SymmetricKey, secret []byte) (EncryptedMessage, error) {
	derivedKeyData := p.deriveKey(secret)
	derivedKey, err := SymmetricKeyFromDataRef(derivedKeyData)
	if err != nil {
		return EncryptedMessage{}, err
	}
	encodedMethod := hkdfParamsToCBOR(*p).ToCBORData()
	return derivedKey.Encrypt(contentKey.Bytes(), encodedMethod, nil), nil
}

// Unlock derives a key from the secret, then decrypts the encrypted message.
func (p *HKDFParams) Unlock(msg *EncryptedMessage, secret []byte) (SymmetricKey, error) {
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

func (p *HKDFParams) deriveKey(secret []byte) []byte {
	switch p.hashType {
	case HashTypeSHA256:
		return bccrypto.HKDFHMACSHA256(secret, p.salt.Bytes(), SymmetricKeySize)
	case HashTypeSHA512:
		return bccrypto.HKDFHMACSHA512(secret, p.salt.Bytes(), SymmetricKeySize)
	default:
		return bccrypto.HKDFHMACSHA256(secret, p.salt.Bytes(), SymmetricKeySize)
	}
}

// String returns a human-readable representation.
func (p *HKDFParams) String() string {
	return fmt.Sprintf("HKDF(%s)", p.hashType)
}

// --- CBOR support ---

func hkdfParamsToCBOR(p HKDFParams) dcbor.CBOR {
	return dcbor.NewCBORArray([]dcbor.CBOR{
		dcbor.NewCBORUnsigned(uint64(KDMethodHKDF)),
		p.salt.TaggedCBOR(),
		hashTypeToCBOR(p.hashType),
	})
}

func hkdfParamsFromCBOR(cbor dcbor.CBOR) (HKDFParams, error) {
	a, err := cbor.TryIntoArray()
	if err != nil {
		return HKDFParams{}, err
	}
	if len(a) != 3 {
		return HKDFParams{}, dcbor.NewErrorf("Invalid HKDFParams: expected 3 elements, got %d", len(a))
	}
	// a[0] is the method index, already consumed by caller
	salt, err := DecodeTaggedSalt(a[1])
	if err != nil {
		return HKDFParams{}, err
	}
	hashType, err := hashTypeFromCBOR(a[2])
	if err != nil {
		return HKDFParams{}, err
	}
	return HKDFParams{salt: salt, hashType: hashType}, nil
}

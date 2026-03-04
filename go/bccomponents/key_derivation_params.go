package bccomponents

import (
	"fmt"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// KeyDerivationParams is a discriminated union holding one of the supported
// key derivation parameter types.
type KeyDerivationParams struct {
	method   KeyDerivationMethod
	hkdf     *HKDFParams
	pbkdf2   *PBKDF2Params
	scrypt   *ScryptParams
	argon2id *Argon2idParams
}

// NewKDParamsHKDF creates KeyDerivationParams for HKDF.
func NewKDParamsHKDF(p HKDFParams) KeyDerivationParams {
	return KeyDerivationParams{method: KDMethodHKDF, hkdf: &p}
}

// NewKDParamsPBKDF2 creates KeyDerivationParams for PBKDF2.
func NewKDParamsPBKDF2(p PBKDF2Params) KeyDerivationParams {
	return KeyDerivationParams{method: KDMethodPBKDF2, pbkdf2: &p}
}

// NewKDParamsScrypt creates KeyDerivationParams for scrypt.
func NewKDParamsScrypt(p ScryptParams) KeyDerivationParams {
	return KeyDerivationParams{method: KDMethodScrypt, scrypt: &p}
}

// NewKDParamsArgon2id creates KeyDerivationParams for Argon2id.
func NewKDParamsArgon2id(p Argon2idParams) KeyDerivationParams {
	return KeyDerivationParams{method: KDMethodArgon2id, argon2id: &p}
}

// Method returns the key derivation method.
func (p *KeyDerivationParams) Method() KeyDerivationMethod { return p.method }

// IsPasswordBased returns true if the method is password-based (PBKDF2, scrypt, or Argon2id).
func (p *KeyDerivationParams) IsPasswordBased() bool {
	return p.method == KDMethodPBKDF2 || p.method == KDMethodScrypt || p.method == KDMethodArgon2id
}

// IsSSHAgent returns true if the method is SSH agent. Not supported in default features.
func (p *KeyDerivationParams) IsSSHAgent() bool {
	return false
}

// Lock derives a key from the secret and encrypts the content key.
func (p *KeyDerivationParams) Lock(contentKey SymmetricKey, secret []byte) (EncryptedMessage, error) {
	switch p.method {
	case KDMethodHKDF:
		return p.hkdf.Lock(contentKey, secret)
	case KDMethodPBKDF2:
		return p.pbkdf2.Lock(contentKey, secret)
	case KDMethodScrypt:
		return p.scrypt.Lock(contentKey, secret)
	case KDMethodArgon2id:
		return p.argon2id.Lock(contentKey, secret)
	default:
		return EncryptedMessage{}, errGeneral("unsupported key derivation method")
	}
}

// String returns a human-readable representation.
func (p *KeyDerivationParams) String() string {
	switch p.method {
	case KDMethodHKDF:
		return p.hkdf.String()
	case KDMethodPBKDF2:
		return p.pbkdf2.String()
	case KDMethodScrypt:
		return p.scrypt.String()
	case KDMethodArgon2id:
		return p.argon2id.String()
	default:
		return fmt.Sprintf("KeyDerivationParams(%d)", p.method)
	}
}

// --- CBOR support ---

func kdParamsToCBOR(p KeyDerivationParams) dcbor.CBOR {
	switch p.method {
	case KDMethodHKDF:
		return hkdfParamsToCBOR(*p.hkdf)
	case KDMethodPBKDF2:
		return pbkdf2ParamsToCBOR(*p.pbkdf2)
	case KDMethodScrypt:
		return scryptParamsToCBOR(*p.scrypt)
	case KDMethodArgon2id:
		return argon2idParamsToCBOR(*p.argon2id)
	default:
		return dcbor.NewCBORArray(nil)
	}
}

func kdParamsFromCBOR(cbor dcbor.CBOR) (KeyDerivationParams, error) {
	a, err := cbor.TryIntoArray()
	if err != nil {
		return KeyDerivationParams{}, err
	}
	if len(a) == 0 {
		return KeyDerivationParams{}, dcbor.NewErrorf("KeyDerivationParams: empty array")
	}
	index, err := a[0].TryIntoUInt()
	if err != nil {
		return KeyDerivationParams{}, err
	}
	method, ok := KDMethodFromIndex(int(index))
	if !ok {
		return KeyDerivationParams{}, dcbor.NewErrorf("Invalid KeyDerivationMethod: %d", index)
	}

	// Re-parse the full array using the appropriate params decoder.
	// We need to clone the CBOR since TryIntoArray consumed it.
	paramsCBOR := kdParamsArrayToCBOR(a)

	switch method {
	case KDMethodHKDF:
		p, err := hkdfParamsFromCBOR(paramsCBOR)
		if err != nil {
			return KeyDerivationParams{}, err
		}
		return NewKDParamsHKDF(p), nil
	case KDMethodPBKDF2:
		p, err := pbkdf2ParamsFromCBOR(paramsCBOR)
		if err != nil {
			return KeyDerivationParams{}, err
		}
		return NewKDParamsPBKDF2(p), nil
	case KDMethodScrypt:
		p, err := scryptParamsFromCBOR(paramsCBOR)
		if err != nil {
			return KeyDerivationParams{}, err
		}
		return NewKDParamsScrypt(p), nil
	case KDMethodArgon2id:
		p, err := argon2idParamsFromCBOR(paramsCBOR)
		if err != nil {
			return KeyDerivationParams{}, err
		}
		return NewKDParamsArgon2id(p), nil
	default:
		return KeyDerivationParams{}, dcbor.NewErrorf("Unsupported KeyDerivationMethod: %d", index)
	}
}

// kdParamsArrayToCBOR reconstructs a CBOR array from already-decoded elements.
func kdParamsArrayToCBOR(elements []dcbor.CBOR) dcbor.CBOR {
	return dcbor.NewCBORArray(elements)
}

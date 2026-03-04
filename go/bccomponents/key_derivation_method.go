package bccomponents

import (
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// KeyDerivationMethod identifies the key derivation algorithm.
type KeyDerivationMethod int

const (
	// KDMethodHKDF selects HKDF key derivation.
	KDMethodHKDF KeyDerivationMethod = 0
	// KDMethodPBKDF2 selects PBKDF2 key derivation.
	KDMethodPBKDF2 KeyDerivationMethod = 1
	// KDMethodScrypt selects scrypt key derivation.
	KDMethodScrypt KeyDerivationMethod = 2
	// KDMethodArgon2id selects Argon2id key derivation.
	KDMethodArgon2id KeyDerivationMethod = 3
)

// Index returns the zero-based index of the key derivation method.
func (m KeyDerivationMethod) Index() int { return int(m) }

// KDMethodFromIndex attempts to create a KeyDerivationMethod from a zero-based index.
// Returns the method and true on success, or zero and false for unknown indices.
func KDMethodFromIndex(index int) (KeyDerivationMethod, bool) {
	switch index {
	case 0:
		return KDMethodHKDF, true
	case 1:
		return KDMethodPBKDF2, true
	case 2:
		return KDMethodScrypt, true
	case 3:
		return KDMethodArgon2id, true
	default:
		return 0, false
	}
}

// String returns a human-readable name for the method.
func (m KeyDerivationMethod) String() string {
	switch m {
	case KDMethodHKDF:
		return "HKDF"
	case KDMethodPBKDF2:
		return "PBKDF2"
	case KDMethodScrypt:
		return "Scrypt"
	case KDMethodArgon2id:
		return "Argon2id"
	default:
		return "Unknown"
	}
}

// kdMethodFromCBOR decodes a KeyDerivationMethod from a CBOR value.
func kdMethodFromCBOR(cbor dcbor.CBOR) (KeyDerivationMethod, error) {
	v, err := cbor.TryIntoUInt()
	if err != nil {
		return 0, err
	}
	m, ok := KDMethodFromIndex(int(v))
	if !ok {
		return 0, dcbor.NewErrorf("Invalid KeyDerivationMethod: %d", v)
	}
	return m, nil
}

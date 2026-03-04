package bccomponents

import (
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// HashType represents the supported hash algorithm types for key derivation.
type HashType int

const (
	// HashTypeSHA256 selects SHA-256.
	HashTypeSHA256 HashType = 0
	// HashTypeSHA512 selects SHA-512.
	HashTypeSHA512 HashType = 1
)

// String returns a human-readable name for the hash type.
func (h HashType) String() string {
	switch h {
	case HashTypeSHA256:
		return "SHA256"
	case HashTypeSHA512:
		return "SHA512"
	default:
		return "Unknown"
	}
}

// hashTypeToCBOR converts a HashType to its CBOR representation.
func hashTypeToCBOR(h HashType) dcbor.CBOR {
	return dcbor.NewCBORUnsigned(uint64(h))
}

// hashTypeFromCBOR decodes a HashType from CBOR.
func hashTypeFromCBOR(cbor dcbor.CBOR) (HashType, error) {
	v, err := cbor.TryIntoUInt8()
	if err != nil {
		return 0, err
	}
	switch v {
	case 0:
		return HashTypeSHA256, nil
	case 1:
		return HashTypeSHA512, nil
	default:
		return 0, dcbor.NewErrorf("Invalid HashType: %d", v)
	}
}

package bcur

import (
	"strings"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// UR represents a Uniform Resource — a URI-encoded CBOR object.
type UR struct {
	urType URType
	cbor   dcbor.CBOR
}

// NewUR creates a new UR from the provided type string and CBOR value.
func NewUR(urType string, cbor dcbor.CBOR) (*UR, error) {
	t, err := NewURType(urType)
	if err != nil {
		return nil, err
	}
	return &UR{urType: t, cbor: cbor}, nil
}

// FromURString parses a UR string into a UR value.
func FromURString(urString string) (*UR, error) {
	lower := strings.ToLower(urString)
	stripScheme, ok := strings.CutPrefix(lower, "ur:")
	if !ok {
		return nil, ErrInvalidScheme
	}

	typePart, _, found := strings.Cut(stripScheme, "/")
	if !found {
		return nil, ErrTypeUnspecified
	}

	urType, err := NewURType(typePart)
	if err != nil {
		return nil, err
	}

	kind, data, err := urDecode(lower)
	if err != nil {
		return nil, err
	}
	if kind != urKindSinglePart {
		return nil, ErrNotSinglePart
	}

	cbor, err := dcbor.TryFromData(data)
	if err != nil {
		return nil, err
	}

	return &UR{urType: urType, cbor: cbor}, nil
}

// URString returns the UR string representation.
func (u *UR) URString() string {
	data := dcbor.ToCBORData(u.cbor)
	return urEncode(data, u.urType.String())
}

// String implements the Stringer interface, returning the UR string.
func (u *UR) String() string {
	return u.URString()
}

// QRString returns the uppercase UR string, optimized for QR codes.
func (u *UR) QRString() string {
	return strings.ToUpper(u.URString())
}

// QRData returns the uppercase UR string as bytes, optimized for QR codes.
func (u *UR) QRData() []byte {
	return []byte(u.QRString())
}

// CheckType verifies that this UR's type matches the given type string.
func (u *UR) CheckType(otherType string) error {
	other, err := NewURType(otherType)
	if err != nil {
		return err
	}
	if u.urType.value != other.value {
		return &UnexpectedTypeError{Expected: other.value, Found: u.urType.value}
	}
	return nil
}

// URType returns the UR type.
func (u *UR) URType() URType {
	return u.urType
}

// URTypeStr returns the UR type as a string.
func (u *UR) URTypeStr() string {
	return u.urType.String()
}

// CBOR returns the CBOR value.
func (u *UR) CBOR() dcbor.CBOR {
	return u.cbor.Clone()
}

// Equal returns true if two URs are equal.
func (u *UR) Equal(other *UR) bool {
	if u == nil || other == nil {
		return u == other
	}
	return u.urType.value == other.urType.value && u.cbor.Equal(other.cbor)
}

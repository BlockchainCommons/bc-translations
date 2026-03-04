package bccomponents

import (
	"encoding/hex"
	"fmt"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

const AuthenticationTagSize = 16

// AuthenticationTag is a 16-byte Poly1305 MAC used for message authentication
// in ChaCha20-Poly1305 AEAD encryption.
type AuthenticationTag struct {
	data [AuthenticationTagSize]byte
}

// AuthenticationTagFromData creates an AuthenticationTag from a 16-byte array.
func AuthenticationTagFromData(data [AuthenticationTagSize]byte) AuthenticationTag {
	return AuthenticationTag{data: data}
}

// AuthenticationTagFromDataRef creates an AuthenticationTag from a byte slice.
func AuthenticationTagFromDataRef(data []byte) (AuthenticationTag, error) {
	if len(data) != AuthenticationTagSize {
		return AuthenticationTag{}, errInvalidSize("authentication tag", AuthenticationTagSize, len(data))
	}
	var t AuthenticationTag
	copy(t.data[:], data)
	return t, nil
}

// Data returns the 16-byte tag data.
func (t AuthenticationTag) Data() [AuthenticationTagSize]byte { return t.data }

// Bytes returns the tag as a byte slice.
func (t AuthenticationTag) Bytes() []byte { return t.data[:] }

// String returns a human-readable representation.
func (t AuthenticationTag) String() string {
	return fmt.Sprintf("AuthenticationTag(%s)", hex.EncodeToString(t.data[:]))
}

// Equal reports whether two authentication tags are equal.
func (t AuthenticationTag) Equal(other AuthenticationTag) bool { return t.data == other.data }

// ToCBOR encodes the tag as a CBOR byte string (not tagged).
func (t AuthenticationTag) ToCBOR() dcbor.CBOR { return dcbor.ToByteString(t.data[:]) }

// DecodeAuthenticationTag decodes an AuthenticationTag from CBOR.
func DecodeAuthenticationTag(cbor dcbor.CBOR) (AuthenticationTag, error) {
	data, err := cbor.TryIntoByteString()
	if err != nil {
		return AuthenticationTag{}, err
	}
	return AuthenticationTagFromDataRef(data)
}

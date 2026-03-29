package provenancemark

import (
	"encoding/hex"
	"encoding/json"
	"fmt"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// RngStateLength is the fixed byte length of a serialized Xoshiro state.
const RngStateLength = 32

// RngState stores a fixed 32-byte deterministic RNG snapshot.
type RngState struct {
	data [RngStateLength]byte
}

// RngStateFromBytes wraps a fixed state block.
func RngStateFromBytes(bytes [RngStateLength]byte) RngState {
	return RngState{data: bytes}
}

// RngStateFromSlice decodes a state from a byte slice.
func RngStateFromSlice(bytes []byte) (RngState, error) {
	if len(bytes) != RngStateLength {
		return RngState{}, fmt.Errorf("invalid RNG state length: expected %d bytes, got %d bytes", RngStateLength, len(bytes))
	}
	var out [RngStateLength]byte
	copy(out[:], bytes)
	return RngStateFromBytes(out), nil
}

// Bytes returns a copy of the underlying state bytes.
func (r RngState) Bytes() [RngStateLength]byte {
	return r.data
}

// Hex returns the state as lower-case hex.
func (r RngState) Hex() string {
	return hex.EncodeToString(r.data[:])
}

// ToCBOR encodes the state as an untagged CBOR byte string.
func (r RngState) ToCBOR() dcbor.CBOR {
	return dcbor.ToByteString(r.data[:])
}

// DecodeRngState decodes a state from CBOR.
func DecodeRngState(cbor dcbor.CBOR) (RngState, error) {
	bytes, err := cbor.TryIntoByteString()
	if err != nil {
		return RngState{}, wrapCBORError(err)
	}
	return RngStateFromSlice(bytes)
}

// MarshalJSON encodes the state as a base64 string.
func (r RngState) MarshalJSON() ([]byte, error) {
	return marshalString(SerializeBlock(r.data))
}

// UnmarshalJSON decodes the state from a base64 string.
func (r *RngState) UnmarshalJSON(data []byte) error {
	value, err := unmarshalString(data)
	if err != nil {
		return err
	}
	block, err := DeserializeBlock(value)
	if err != nil {
		return err
	}
	*r = RngStateFromBytes(block)
	return nil
}

var _ json.Marshaler = RngState{}
var _ json.Unmarshaler = (*RngState)(nil)

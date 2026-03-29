package provenancemark

import (
	"encoding/json"
	"fmt"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// ProvenanceMarkResolution describes how much entropy and metadata a mark stores.
type ProvenanceMarkResolution uint8

const (
	ProvenanceMarkResolutionLow ProvenanceMarkResolution = iota
	ProvenanceMarkResolutionMedium
	ProvenanceMarkResolutionQuartile
	ProvenanceMarkResolutionHigh
)

// ProvenanceMarkResolutionFromByte decodes a resolution from its numeric form.
func ProvenanceMarkResolutionFromByte(value uint8) (ProvenanceMarkResolution, error) {
	switch value {
	case 0:
		return ProvenanceMarkResolutionLow, nil
	case 1:
		return ProvenanceMarkResolutionMedium, nil
	case 2:
		return ProvenanceMarkResolutionQuartile, nil
	case 3:
		return ProvenanceMarkResolutionHigh, nil
	default:
		return 0, newResolutionError(fmt.Sprintf("invalid provenance mark resolution value: %d", value))
	}
}

// ToCBOR encodes the resolution as a CBOR unsigned integer.
func (r ProvenanceMarkResolution) ToCBOR() dcbor.CBOR {
	return dcbor.MustFromAny(uint8(r))
}

// MarshalJSON encodes the resolution as a JSON number.
func (r ProvenanceMarkResolution) MarshalJSON() ([]byte, error) {
	return json.Marshal(uint8(r))
}

// UnmarshalJSON decodes the resolution from a JSON number.
func (r *ProvenanceMarkResolution) UnmarshalJSON(data []byte) error {
	var value uint8
	if err := json.Unmarshal(data, &value); err != nil {
		return err
	}
	decoded, err := ProvenanceMarkResolutionFromByte(value)
	if err != nil {
		return err
	}
	*r = decoded
	return nil
}

// DecodeProvenanceMarkResolution decodes a resolution from CBOR.
func DecodeProvenanceMarkResolution(cbor dcbor.CBOR) (ProvenanceMarkResolution, error) {
	value, err := cbor.TryIntoUInt8()
	if err != nil {
		return 0, wrapCBORError(err)
	}
	return ProvenanceMarkResolutionFromByte(value)
}

// LinkLength returns the byte length of key/hash/chain ID fields.
func (r ProvenanceMarkResolution) LinkLength() int {
	switch r {
	case ProvenanceMarkResolutionLow:
		return 4
	case ProvenanceMarkResolutionMedium:
		return 8
	case ProvenanceMarkResolutionQuartile:
		return 16
	case ProvenanceMarkResolutionHigh:
		return 32
	default:
		panic(fmt.Sprintf("invalid provenance mark resolution value: %d", r))
	}
}

// SeqBytesLength returns the byte length of the encoded sequence number.
func (r ProvenanceMarkResolution) SeqBytesLength() int {
	switch r {
	case ProvenanceMarkResolutionLow:
		return 2
	case ProvenanceMarkResolutionMedium, ProvenanceMarkResolutionQuartile, ProvenanceMarkResolutionHigh:
		return 4
	default:
		panic(fmt.Sprintf("invalid provenance mark resolution value: %d", r))
	}
}

// DateBytesLength returns the byte length of the encoded date.
func (r ProvenanceMarkResolution) DateBytesLength() int {
	switch r {
	case ProvenanceMarkResolutionLow:
		return 2
	case ProvenanceMarkResolutionMedium:
		return 4
	case ProvenanceMarkResolutionQuartile, ProvenanceMarkResolutionHigh:
		return 6
	default:
		panic(fmt.Sprintf("invalid provenance mark resolution value: %d", r))
	}
}

// FixedLength returns the minimum length of a mark message without info bytes.
func (r ProvenanceMarkResolution) FixedLength() int {
	return r.LinkLength()*3 + r.SeqBytesLength() + r.DateBytesLength()
}

// KeyRange returns the key span within a message.
func (r ProvenanceMarkResolution) KeyRange() ByteRange {
	return ByteRange{Start: 0, End: r.LinkLength()}
}

// ChainIDRange returns the chain-ID span within the obfuscated payload.
func (r ProvenanceMarkResolution) ChainIDRange() ByteRange {
	return ByteRange{Start: 0, End: r.LinkLength()}
}

// HashRange returns the hash span within the obfuscated payload.
func (r ProvenanceMarkResolution) HashRange() ByteRange {
	chain := r.ChainIDRange()
	return ByteRange{Start: chain.End, End: chain.End + r.LinkLength()}
}

// SeqBytesRange returns the sequence-byte span within the obfuscated payload.
func (r ProvenanceMarkResolution) SeqBytesRange() ByteRange {
	hash := r.HashRange()
	return ByteRange{Start: hash.End, End: hash.End + r.SeqBytesLength()}
}

// DateBytesRange returns the date-byte span within the obfuscated payload.
func (r ProvenanceMarkResolution) DateBytesRange() ByteRange {
	seq := r.SeqBytesRange()
	return ByteRange{Start: seq.End, End: seq.End + r.DateBytesLength()}
}

// InfoRange returns the trailing info-byte span within the obfuscated payload.
func (r ProvenanceMarkResolution) InfoRange() ByteRange {
	return ByteRange{Start: r.DateBytesRange().End, End: -1}
}

// SerializeDate encodes a date according to the resolution.
func (r ProvenanceMarkResolution) SerializeDate(date dcbor.Date) ([]byte, error) {
	return serializeDateByResolution(r, date)
}

// DeserializeDate decodes a date according to the resolution.
func (r ProvenanceMarkResolution) DeserializeDate(data []byte) (dcbor.Date, error) {
	return deserializeDateByResolution(r, data)
}

// SerializeSeq encodes a sequence number according to the resolution.
func (r ProvenanceMarkResolution) SerializeSeq(seq uint32) ([]byte, error) {
	switch r.SeqBytesLength() {
	case 2:
		if seq > uint32(^uint16(0)) {
			return nil, newResolutionError(fmt.Sprintf("sequence number %d out of range for 2-byte format (max %d)", seq, ^uint16(0)))
		}
		return []byte{byte(seq >> 8), byte(seq)}, nil
	case 4:
		return []byte{byte(seq >> 24), byte(seq >> 16), byte(seq >> 8), byte(seq)}, nil
	default:
		panic("unreachable")
	}
}

// DeserializeSeq decodes a sequence number according to the resolution.
func (r ProvenanceMarkResolution) DeserializeSeq(data []byte) (uint32, error) {
	switch r.SeqBytesLength() {
	case 2:
		if len(data) != 2 {
			return 0, newResolutionError(fmt.Sprintf("invalid sequence number length: expected 2 or 4 bytes, got %d", len(data)))
		}
		return uint32(data[0])<<8 | uint32(data[1]), nil
	case 4:
		if len(data) != 4 {
			return 0, newResolutionError(fmt.Sprintf("invalid sequence number length: expected 2 or 4 bytes, got %d", len(data)))
		}
		return uint32(data[0])<<24 | uint32(data[1])<<16 | uint32(data[2])<<8 | uint32(data[3]), nil
	default:
		panic("unreachable")
	}
}

// String returns the lower-case Rust display form.
func (r ProvenanceMarkResolution) String() string {
	switch r {
	case ProvenanceMarkResolutionLow:
		return "low"
	case ProvenanceMarkResolutionMedium:
		return "medium"
	case ProvenanceMarkResolutionQuartile:
		return "quartile"
	case ProvenanceMarkResolutionHigh:
		return "high"
	default:
		return fmt.Sprintf("resolution(%d)", r)
	}
}

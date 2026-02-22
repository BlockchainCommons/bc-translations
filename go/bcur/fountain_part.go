package bcur

import (
	"encoding/binary"
	"fmt"
)

// fountainPart represents a single part in fountain-coded communication.
type fountainPart struct {
	sequence      int
	sequenceCount int
	messageLength int
	checksum      uint32
	data          []byte
}

// indexes returns the fragment indices combined in this part.
func (p *fountainPart) indexes() []int {
	return chooseFragments(p.sequence, p.sequenceCount, p.checksum)
}

// isSimple returns true if this part represents a single original fragment.
func (p *fountainPart) isSimple() bool {
	return len(p.indexes()) == 1
}

// sequenceID returns a string like "1-9" identifying this part.
func (p *fountainPart) sequenceID() string {
	return fmt.Sprintf("%d-%d", p.sequence, p.sequenceCount)
}

// clone returns a deep copy of this part.
func (p *fountainPart) clone() *fountainPart {
	dataCopy := make([]byte, len(p.data))
	copy(dataCopy, p.data)
	return &fountainPart{
		sequence:      p.sequence,
		sequenceCount: p.sequenceCount,
		messageLength: p.messageLength,
		checksum:      p.checksum,
		data:          dataCopy,
	}
}

// toCBOR encodes this part as a CBOR byte array. Uses manual CBOR encoding
// to match minicbor's non-deterministic output format.
func (p *fountainPart) toCBOR() []byte {
	var buf []byte
	// Array of 5 items
	buf = append(buf, 0x85)
	buf = appendCBORUnsigned(buf, uint64(p.sequence))
	buf = appendCBORUnsigned(buf, uint64(p.sequenceCount))
	buf = appendCBORUnsigned(buf, uint64(p.messageLength))
	buf = appendCBORUnsigned(buf, uint64(p.checksum))
	buf = appendCBORBytes(buf, p.data)
	return buf
}

// fountainPartFromCBOR decodes a CBOR-encoded fountain part.
func fountainPartFromCBOR(data []byte) (*fountainPart, error) {
	if len(data) == 0 {
		return nil, ErrCBORDecode
	}

	offset := 0

	// Must be array of 5
	if offset >= len(data) {
		return nil, ErrCBORDecode
	}
	majorType := data[offset] >> 5
	if majorType != 4 { // major type 4 = array
		return nil, ErrCBORDecode
	}
	arrayLen, n, err := decodeCBORUint64(data, offset)
	if err != nil {
		return nil, err
	}
	offset = n
	if arrayLen != 5 {
		return nil, ErrCBORArrayLength
	}

	// Decode 4 unsigned integers
	values := make([]uint32, 4)
	for i := 0; i < 4; i++ {
		v, n, err := decodeCBORUint(data, offset)
		if err != nil {
			return nil, err
		}
		offset = n
		if v > 0xFFFFFFFF {
			return nil, fmt.Errorf("value too large for u32 at position %d", offset)
		}
		values[i] = uint32(v)
	}

	// Decode byte string
	partData, _, err := decodeCBORByteString(data, offset)
	if err != nil {
		return nil, err
	}

	return &fountainPart{
		sequence:      int(values[0]),
		sequenceCount: int(values[1]),
		messageLength: int(values[2]),
		checksum:      values[3],
		data:          partData,
	}, nil
}

// appendCBORUnsigned encodes an unsigned integer in CBOR format.
func appendCBORUnsigned(buf []byte, v uint64) []byte {
	if v <= 23 {
		return append(buf, byte(v))
	} else if v <= 0xFF {
		return append(buf, 0x18, byte(v))
	} else if v <= 0xFFFF {
		b := make([]byte, 2)
		binary.BigEndian.PutUint16(b, uint16(v))
		return append(append(buf, 0x19), b...)
	} else if v <= 0xFFFFFFFF {
		b := make([]byte, 4)
		binary.BigEndian.PutUint32(b, uint32(v))
		return append(append(buf, 0x1a), b...)
	}
	b := make([]byte, 8)
	binary.BigEndian.PutUint64(b, v)
	return append(append(buf, 0x1b), b...)
}

// appendCBORBytes encodes a byte string in CBOR format (major type 2).
func appendCBORBytes(buf []byte, data []byte) []byte {
	length := uint64(len(data))
	if length <= 23 {
		buf = append(buf, byte(0x40+length))
	} else if length <= 0xFF {
		buf = append(buf, 0x58, byte(length))
	} else if length <= 0xFFFF {
		b := make([]byte, 2)
		binary.BigEndian.PutUint16(b, uint16(length))
		buf = append(append(buf, 0x59), b...)
	} else {
		b := make([]byte, 4)
		binary.BigEndian.PutUint32(b, uint32(length))
		buf = append(append(buf, 0x5a), b...)
	}
	return append(buf, data...)
}

// decodeCBORUint64 decodes a CBOR header (any major type) and returns the additional value.
// Used for array/map lengths where major type is not 0.
func decodeCBORUint64(data []byte, offset int) (uint64, int, error) {
	if offset >= len(data) {
		return 0, offset, ErrCBORDecode
	}
	additional := data[offset] & 0x1f
	offset++

	if additional <= 23 {
		return uint64(additional), offset, nil
	} else if additional == 24 {
		if offset >= len(data) {
			return 0, offset, ErrCBORDecode
		}
		v := uint64(data[offset])
		return v, offset + 1, nil
	} else if additional == 25 {
		if offset+2 > len(data) {
			return 0, offset, ErrCBORDecode
		}
		v := uint64(binary.BigEndian.Uint16(data[offset : offset+2]))
		return v, offset + 2, nil
	} else if additional == 26 {
		if offset+4 > len(data) {
			return 0, offset, ErrCBORDecode
		}
		v := uint64(binary.BigEndian.Uint32(data[offset : offset+4]))
		return v, offset + 4, nil
	} else if additional == 27 {
		if offset+8 > len(data) {
			return 0, offset, ErrCBORDecode
		}
		v := binary.BigEndian.Uint64(data[offset : offset+8])
		return v, offset + 8, nil
	}
	return 0, offset, ErrCBORDecode
}

// decodeCBORUint decodes a CBOR unsigned integer (major type 0) starting at offset.
// Returns value and new offset.
func decodeCBORUint(data []byte, offset int) (uint64, int, error) {
	if offset >= len(data) {
		return 0, offset, ErrCBORDecode
	}
	majorType := data[offset] >> 5
	if majorType != 0 { // major type 0 = unsigned int
		return 0, offset, ErrCBORDecode
	}
	return decodeCBORUint64(data, offset)
}

// decodeCBORByteString decodes a CBOR byte string starting at offset.
func decodeCBORByteString(data []byte, offset int) ([]byte, int, error) {
	if offset >= len(data) {
		return nil, offset, ErrCBORDecode
	}
	majorType := data[offset] >> 5
	if majorType != 2 { // major type 2 = byte string
		return nil, offset, ErrCBORDecode
	}

	length, newOffset, err := decodeCBORUint64(data, offset)
	if err != nil {
		return nil, offset, err
	}

	end := newOffset + int(length)
	if end > len(data) {
		return nil, newOffset, ErrCBORDecode
	}
	result := make([]byte, length)
	copy(result, data[newOffset:end])
	return result, end, nil
}

package dcbor

import "bytes"

// ByteString is a CBOR byte string wrapper.
type ByteString struct {
	data []byte
}

func NewByteString(data []byte) ByteString {
	copied := make([]byte, len(data))
	copy(copied, data)
	return ByteString{data: copied}
}

func (b ByteString) Data() []byte {
	copied := make([]byte, len(b.data))
	copy(copied, b.data)
	return copied
}

func (b ByteString) Len() int {
	return len(b.data)
}

func (b ByteString) IsEmpty() bool {
	return len(b.data) == 0
}

func (b *ByteString) Extend(other []byte) {
	b.data = append(b.data, other...)
}

func (b ByteString) ToVec() []byte {
	return b.Data()
}

func (b ByteString) Iter() []byte {
	return b.Data()
}

func (b ByteString) AsRef() []byte {
	return b.Data()
}

// Equal reports byte-for-byte equality.
func (b ByteString) Equal(other ByteString) bool {
	return bytes.Equal(b.data, other.data)
}

// String returns the diagnostic-style hex form of the byte string.
func (b ByteString) String() string {
	return NewCBORByteString(b).DiagnosticFlat()
}

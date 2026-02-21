package dcbor

import "bytes"

// ByteString is a CBOR byte string wrapper.
type ByteString struct {
	data []byte
}

// NewByteString constructs a byte-string wrapper from a copied slice.
func NewByteString(data []byte) ByteString {
	copied := make([]byte, len(data))
	copy(copied, data)
	return ByteString{data: copied}
}

// Data returns a copy of the underlying bytes.
func (b ByteString) Data() []byte {
	copied := make([]byte, len(b.data))
	copy(copied, b.data)
	return copied
}

// Len returns the number of bytes.
func (b ByteString) Len() int {
	return len(b.data)
}

// IsEmpty reports whether the byte string has zero length.
func (b ByteString) IsEmpty() bool {
	return len(b.data) == 0
}

// Extend appends bytes to the receiver.
func (b *ByteString) Extend(other []byte) {
	b.data = append(b.data, other...)
}

// Bytes returns a copy of the underlying bytes.
// This is an alias for Data provided for idiomatic Go naming.
func (b ByteString) Bytes() []byte {
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

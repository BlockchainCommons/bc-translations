package sskr

import "bytes"

// Secret is a secret to be split into shares.
type Secret struct {
	data []byte
}

// NewSecret creates a new Secret with the given data.
func NewSecret(data []byte) (Secret, error) {
	length := len(data)
	if length < MinSecretLen {
		return Secret{}, ErrSecretTooShort
	}
	if length > MaxSecretLen {
		return Secret{}, ErrSecretTooLong
	}
	if length&1 != 0 {
		return Secret{}, ErrSecretLengthNotEven
	}
	copied := make([]byte, len(data))
	copy(copied, data)
	return Secret{data: copied}, nil
}

// Len returns the length of the secret.
func (s Secret) Len() int {
	return len(s.data)
}

// IsEmpty reports whether the secret is empty.
func (s Secret) IsEmpty() bool {
	return s.Len() == 0
}

// Data returns a copy of the secret bytes.
func (s Secret) Data() []byte {
	copied := make([]byte, len(s.data))
	copy(copied, s.data)
	return copied
}

// bytes returns the underlying byte slice without copying.
// Internal use only; callers must not modify the returned slice.
func (s Secret) bytes() []byte {
	return s.data
}

// Clone returns a deep copy of the secret.
func (s Secret) Clone() Secret {
	copied := make([]byte, len(s.data))
	copy(copied, s.data)
	return Secret{data: copied}
}

// Equal reports whether two secrets contain identical bytes.
func (s Secret) Equal(other Secret) bool {
	return bytes.Equal(s.data, other.data)
}

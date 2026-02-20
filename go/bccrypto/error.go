package bccrypto

// Error represents package-specific errors.
type Error string

func (e Error) Error() string {
	return string(e)
}

const (
	// ErrAEAD indicates authenticated decryption failure.
	ErrAEAD Error = "AEAD error"
)

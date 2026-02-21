package bccrypto

// Error represents package-specific errors.
type Error string

func (e Error) Error() string {
	return string(e)
}

const (
	// ErrAEAD indicates that authenticated decryption failed.
	ErrAEAD Error = "bccrypto: AEAD decryption failed"
)

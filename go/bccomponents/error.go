package bccomponents

import "fmt"

// Sentinel errors for bc-components.
var (
	// ErrLevelMismatch indicates that a signature's security level does not match
	// the key's security level.
	ErrLevelMismatch = fmt.Errorf("bccomponents: signature level does not match key level")
)

func errInvalidSize(dataType string, expected, actual int) error {
	return fmt.Errorf("bccomponents: invalid %s size: expected %d, got %d", dataType, expected, actual)
}

func errInvalidData(dataType, reason string) error {
	return fmt.Errorf("bccomponents: invalid %s: %s", dataType, reason)
}

func errDataTooShort(dataType string, minimum, actual int) error {
	return fmt.Errorf("bccomponents: %s data too short: expected at least %d, got %d", dataType, minimum, actual)
}

func errCrypto(msg string) error {
	return fmt.Errorf("bccomponents: cryptographic operation failed: %s", msg)
}

func errSSH(msg string) error {
	return fmt.Errorf("bccomponents: SSH operation failed: %s", msg)
}

func errCompression(msg string) error {
	return fmt.Errorf("bccomponents: compression error: %s", msg)
}

func errPostQuantum(msg string) error {
	return fmt.Errorf("bccomponents: post-quantum cryptography error: %s", msg)
}

func errGeneral(msg string) error {
	return fmt.Errorf("bccomponents: %s", msg)
}

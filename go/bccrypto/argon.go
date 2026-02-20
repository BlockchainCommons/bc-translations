package bccrypto

import "golang.org/x/crypto/argon2"

const (
	argon2IDTime    = 2
	argon2IDMemory  = 19456
	argon2IDThreads = 1
)

// Argon2ID computes Argon2id with Rust argon2 default parameters.
func Argon2ID(pass, salt []byte, outputLen int) []byte {
	if outputLen < 0 {
		panic("argon2 failed")
	}
	return argon2.IDKey(
		pass,
		salt,
		argon2IDTime,
		argon2IDMemory,
		argon2IDThreads,
		uint32(outputLen),
	)
}

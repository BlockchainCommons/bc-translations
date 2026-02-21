package bccrypto

import "golang.org/x/crypto/argon2"

const (
	argon2IDTime    = 2
	argon2IDMemory  = 19456
	argon2IDThreads = 1
)

// Argon2ID computes Argon2id with standard parameters (time=2, memory=19456 KiB, threads=1).
func Argon2ID(pass, salt []byte, outputLen int) []byte {
	if outputLen < 0 {
		panic("bccrypto: argon2id: output length must be non-negative")
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

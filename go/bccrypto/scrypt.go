package bccrypto

import (
	"math/bits"

	"golang.org/x/crypto/scrypt"
)

// Scrypt computes the scrypt KDF with recommended parameters.
func Scrypt(pass, salt []byte, outputLen int) []byte {
	return ScryptWithParams(pass, salt, outputLen, 15, 8, 1)
}

// ScryptWithParams computes the scrypt KDF with explicit parameters.
func ScryptWithParams(
	pass, salt []byte,
	outputLen int,
	logN uint8,
	r, p uint32,
) []byte {
	if outputLen < 0 {
		panic("bccrypto: scrypt: output length must be non-negative")
	}
	if int(logN) >= bits.UintSize {
		panic("bccrypto: scrypt: logN exceeds platform word size")
	}

	n := 1 << logN
	out, err := scrypt.Key(pass, salt, n, int(r), int(p), outputLen)
	if err != nil {
		panic("bccrypto: scrypt: invalid parameters: " + err.Error())
	}
	return out
}

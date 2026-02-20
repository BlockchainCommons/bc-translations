package bccrypto

import (
	"math/bits"

	"golang.org/x/crypto/scrypt"
)

// Scrypt computes the scrypt KDF with recommended parameters.
func Scrypt(pass, salt []byte, outputLen int) []byte {
	return ScryptOpt(pass, salt, outputLen, 15, 8, 1)
}

// ScryptOpt computes scrypt KDF with explicit parameters.
func ScryptOpt(
	pass, salt []byte,
	outputLen int,
	logN uint8,
	r, p uint32,
) []byte {
	if outputLen < 0 {
		panic("Invalid Scrypt parameters")
	}
	if int(logN) >= bits.UintSize {
		panic("Invalid Scrypt parameters")
	}

	n := 1 << logN
	out, err := scrypt.Key(pass, salt, n, int(r), int(p), outputLen)
	if err != nil {
		panic("Invalid Scrypt parameters")
	}
	return out
}

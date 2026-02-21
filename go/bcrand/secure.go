package bcrand

import (
	"crypto/rand"
	"encoding/binary"
)

// SecureRandomNumberGenerator is a random number generator that can be used as
// a source of cryptographically-strong randomness.
type SecureRandomNumberGenerator struct{}

// NewSecureRandomNumberGenerator returns a new SecureRandomNumberGenerator.
func NewSecureRandomNumberGenerator() *SecureRandomNumberGenerator {
	return &SecureRandomNumberGenerator{}
}

func (s *SecureRandomNumberGenerator) NextU32() uint32 {
	var buf [4]byte
	fillCryptoRandom(buf[:])
	return binary.LittleEndian.Uint32(buf[:])
}

func (s *SecureRandomNumberGenerator) NextU64() uint64 {
	var buf [8]byte
	fillCryptoRandom(buf[:])
	return binary.LittleEndian.Uint64(buf[:])
}

func (s *SecureRandomNumberGenerator) RandomData(size int) []byte {
	data := make([]byte, size)
	fillCryptoRandom(data)
	return data
}

func (s *SecureRandomNumberGenerator) FillRandomData(data []byte) {
	fillCryptoRandom(data)
}

// RandomData returns a slice of cryptographically strong random bytes of the
// given size.
func RandomData(size int) []byte {
	data := make([]byte, size)
	fillCryptoRandom(data)
	return data
}

// FillRandomData fills the given slice with cryptographically strong random bytes.
func FillRandomData(data []byte) {
	fillCryptoRandom(data)
}

func fillCryptoRandom(data []byte) {
	if _, err := rand.Read(data); err != nil {
		panic("bcrand: crypto/rand read failed: " + err.Error())
	}
}

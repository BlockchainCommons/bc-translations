package bcrand

import (
	"crypto/rand"
	"encoding/binary"
)

// SecureRandomNumberGenerator is a random number generator backed by the
// operating system's cryptographic random number source.
type SecureRandomNumberGenerator struct{}

// NewSecureRandomNumberGenerator returns a new SecureRandomNumberGenerator.
func NewSecureRandomNumberGenerator() *SecureRandomNumberGenerator {
	return &SecureRandomNumberGenerator{}
}

func (s *SecureRandomNumberGenerator) NextU32() uint32 {
	return uint32(s.NextU64())
}

func (s *SecureRandomNumberGenerator) NextU64() uint64 {
	var buf [8]byte
	if _, err := rand.Read(buf[:]); err != nil {
		panic("failed to read from crypto/rand: " + err.Error())
	}
	return binary.LittleEndian.Uint64(buf[:])
}

func (s *SecureRandomNumberGenerator) RandomData(size int) []byte {
	data := make([]byte, size)
	if _, err := rand.Read(data); err != nil {
		panic("failed to read from crypto/rand: " + err.Error())
	}
	return data
}

func (s *SecureRandomNumberGenerator) FillRandomData(data []byte) {
	if _, err := rand.Read(data); err != nil {
		panic("failed to read from crypto/rand: " + err.Error())
	}
}

// RandomData returns a slice of cryptographically strong random bytes.
func RandomData(size int) []byte {
	data := make([]byte, size)
	if _, err := rand.Read(data); err != nil {
		panic("failed to read from crypto/rand: " + err.Error())
	}
	return data
}

// FillRandomData fills the given slice with cryptographically strong random bytes.
func FillRandomData(data []byte) {
	if _, err := rand.Read(data); err != nil {
		panic("failed to read from crypto/rand: " + err.Error())
	}
}

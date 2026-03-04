package bccomponents

import (
	"crypto/hmac"
	"crypto/sha256"
	"encoding/binary"
	"io"

	"golang.org/x/crypto/hkdf"
)

const hkdfRNGDefaultPageLength = 32

// HKDFRng is a deterministic RNG based on HKDF-HMAC-SHA256.
// It produces repeatable byte sequences from key material and info.
type HKDFRng struct {
	keyMaterial []byte
	info        string
	pageLength  int
	pageIndex   uint32
	buffer      []byte
	offset      int
}

// NewHKDFRng creates a new HKDF-based RNG with the given key material and info string.
func NewHKDFRng(keyMaterial []byte, info string) *HKDFRng {
	return NewHKDFRngWithPageLength(keyMaterial, info, hkdfRNGDefaultPageLength)
}

// NewHKDFRngWithPageLength creates a new HKDF-based RNG with a specific page length.
func NewHKDFRngWithPageLength(keyMaterial []byte, info string, pageLength int) *HKDFRng {
	rng := &HKDFRng{
		keyMaterial: make([]byte, len(keyMaterial)),
		info:        info,
		pageLength:  pageLength,
	}
	copy(rng.keyMaterial, keyMaterial)
	rng.fillBuffer()
	return rng
}

// fillBuffer generates a new page of random bytes.
func (r *HKDFRng) fillBuffer() {
	// Compute salt = HMAC-SHA256(info, big-endian page_index)
	var indexBuf [4]byte
	binary.BigEndian.PutUint32(indexBuf[:], r.pageIndex)
	mac := hmac.New(sha256.New, []byte(r.info))
	mac.Write(indexBuf[:])
	salt := mac.Sum(nil)

	// Derive page using HKDF
	reader := hkdf.New(sha256.New, r.keyMaterial, salt, nil)
	r.buffer = make([]byte, r.pageLength)
	if _, err := io.ReadFull(reader, r.buffer); err != nil {
		panic("bccomponents: HKDF RNG derivation failed")
	}
	r.offset = 0
	r.pageIndex++
}

// NextBytes returns the next n bytes from the RNG.
func (r *HKDFRng) NextBytes(n int) []byte {
	result := make([]byte, n)
	filled := 0
	for filled < n {
		if r.offset >= len(r.buffer) {
			r.fillBuffer()
		}
		toCopy := len(r.buffer) - r.offset
		if toCopy > n-filled {
			toCopy = n - filled
		}
		copy(result[filled:], r.buffer[r.offset:r.offset+toCopy])
		r.offset += toCopy
		filled += toCopy
	}
	return result
}

// NextU32 returns the next 32-bit unsigned integer.
func (r *HKDFRng) NextU32() uint32 {
	b := r.NextBytes(4)
	return binary.LittleEndian.Uint32(b)
}

// NextU64 returns the next 64-bit unsigned integer.
func (r *HKDFRng) NextU64() uint64 {
	b := r.NextBytes(8)
	return binary.LittleEndian.Uint64(b)
}

// FillRandomData fills the provided slice with random bytes.
func (r *HKDFRng) FillRandomData(buf []byte) {
	data := r.NextBytes(len(buf))
	copy(buf, data)
}

// RandomData implements bcrand.RandomNumberGenerator.
func (r *HKDFRng) RandomData(count int) []byte {
	return r.NextBytes(count)
}

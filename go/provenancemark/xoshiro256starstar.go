package provenancemark

import (
	"encoding/binary"
)

// Xoshiro256StarStar is the deterministic PRNG used by provenance mark generation.
type Xoshiro256StarStar struct {
	s [4]uint64
}

// Xoshiro256StarStarFromState constructs a PRNG from its four 64-bit lanes.
func Xoshiro256StarStarFromState(state [4]uint64) *Xoshiro256StarStar {
	return &Xoshiro256StarStar{s: state}
}

// Xoshiro256StarStarFromData constructs a PRNG from 32 little-endian bytes.
func Xoshiro256StarStarFromData(data [32]byte) *Xoshiro256StarStar {
	var state [4]uint64
	for i := 0; i < 4; i++ {
		state[i] = binary.LittleEndian.Uint64(data[i*8 : (i+1)*8])
	}
	return &Xoshiro256StarStar{s: state}
}

// State returns the current 64-bit lanes.
func (x *Xoshiro256StarStar) State() [4]uint64 {
	return x.s
}

// Data returns the current state as 32 little-endian bytes.
func (x *Xoshiro256StarStar) Data() [32]byte {
	var data [32]byte
	for i := 0; i < 4; i++ {
		binary.LittleEndian.PutUint64(data[i*8:(i+1)*8], x.s[i])
	}
	return data
}

// Clone returns an independent copy of the PRNG.
func (x *Xoshiro256StarStar) Clone() *Xoshiro256StarStar {
	if x == nil {
		return nil
	}
	cloned := *x
	return &cloned
}

// NextByte returns the next byte from the low 8 bits of the next u64 output.
func (x *Xoshiro256StarStar) NextByte() byte {
	return byte(x.NextU64())
}

// NextBytes returns len deterministic bytes.
func (x *Xoshiro256StarStar) NextBytes(length int) []byte {
	data := make([]byte, length)
	for i := range data {
		data[i] = x.NextByte()
	}
	return data
}

// NextU32 returns the upper 32 bits of the next u64 output.
func (x *Xoshiro256StarStar) NextU32() uint32 {
	return uint32(x.NextU64() >> 32)
}

// NextU64 advances the PRNG and returns the next value.
func (x *Xoshiro256StarStar) NextU64() uint64 {
	result := x.s[1] * 5
	result = bitsRotateLeft64(result, 7) * 9

	t := x.s[1] << 17
	x.s[2] ^= x.s[0]
	x.s[3] ^= x.s[1]
	x.s[1] ^= x.s[2]
	x.s[0] ^= x.s[3]
	x.s[2] ^= t
	x.s[3] = bitsRotateLeft64(x.s[3], 45)

	return result
}

// FillBytes fills dest using repeated NextU64 outputs in little-endian order.
func (x *Xoshiro256StarStar) FillBytes(dest []byte) {
	for len(dest) >= 8 {
		next := x.NextU64()
		binary.LittleEndian.PutUint64(dest[:8], next)
		dest = dest[8:]
	}
	if len(dest) == 0 {
		return
	}
	next := x.NextU64()
	var buf [8]byte
	binary.LittleEndian.PutUint64(buf[:], next)
	copy(dest, buf[:len(dest)])
}

func bitsRotateLeft64(value uint64, shift uint) uint64 {
	return value<<shift | value>>(64-shift)
}

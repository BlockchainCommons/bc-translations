// Package bcrand provides random number generators and helper utilities.
//
// The package exposes two generators:
//   - [SecureRandomNumberGenerator] for cryptographically strong randomness.
//   - [SeededRandomNumberGenerator] for deterministic test vectors.
//
// Both satisfy [RandomNumberGenerator], which makes them interchangeable in
// callers that need either secure randomness or deterministic output.
package bcrand

import (
	"math/bits"
)

// RandomNumberGenerator defines the interface for random number generation.
type RandomNumberGenerator interface {
	NextU32() uint32
	NextU64() uint64
	// RandomData returns a slice of random bytes of the given size.
	RandomData(size int) []byte
	FillRandomData(data []byte)
}

// RandomDataFrom returns a slice of random bytes using rng.
func RandomDataFrom(rng RandomNumberGenerator, size int) []byte {
	data := make([]byte, size)
	rng.FillRandomData(data)
	return data
}

// FillRandomDataFrom fills data with random bytes using rng.
func FillRandomDataFrom(rng RandomNumberGenerator, data []byte) {
	rng.FillRandomData(data)
}

// bitMask returns the bit mask for bitWidth.
//
// bitWidth must be one of: 8, 16, 32, or 64.
func bitMask(bitWidth uint) uint64 {
	switch bitWidth {
	case 8:
		return 0xFF
	case 16:
		return 0xFFFF
	case 32:
		return 0xFFFFFFFF
	case 64:
		return ^uint64(0)
	default:
		panic("bit width must be 8, 16, 32, or 64")
	}
}

// wideMul performs a full-width multiplication of two values,
// returning (lo, hi) components at the specified bit width.
func wideMul(a, b uint64, bitWidth uint) (lo, hi uint64) {
	switch bitWidth {
	case 8:
		product := uint16(a) * uint16(b)
		return uint64(product & 0xFF), uint64(product >> 8)
	case 16:
		product := uint32(a) * uint32(b)
		return uint64(product & 0xFFFF), uint64(product >> 16)
	case 32:
		product := a * b
		return product & 0xFFFFFFFF, product >> 32
	case 64:
		hi, lo := bits.Mul64(a, b)
		return lo, hi
	default:
		panic("bit width must be 8, 16, 32, or 64")
	}
}

// NextWithUpperBound returns a random value that is less than the given
// upper bound. The upperBound must be non-zero. Every value in [0, upperBound)
// is equally likely to be returned.
//
// bitWidth specifies the working bit width (8, 16, 32, or 64).
func NextWithUpperBound(rng RandomNumberGenerator, upperBound uint64, bitWidth uint) uint64 {
	if upperBound == 0 {
		panic("upper bound must be non-zero")
	}

	mask := bitMask(bitWidth)
	random := rng.NextU64() & mask
	lo, hi := wideMul(random, upperBound, bitWidth)

	if lo < upperBound {
		var t uint64
		if bitWidth >= 64 {
			t = (0 - upperBound) % upperBound
		} else {
			t = ((1 << bitWidth) - upperBound) % upperBound
		}
		for lo < t {
			random = rng.NextU64() & mask
			lo, hi = wideMul(random, upperBound, bitWidth)
		}
	}

	return hi
}

// NextInRange returns a random value within the specified range using rng as
// the randomness source. The range is half-open: [start, end).
//
// bitWidth specifies the working bit width (8, 16, 32, or 64).
func NextInRange(rng RandomNumberGenerator, start, end int64, bitWidth uint) int64 {
	if start >= end {
		panic("start must be less than end")
	}

	delta := uint64(end - start)
	mask := bitMask(bitWidth)

	if delta == mask {
		if bitWidth >= 64 {
			return int64(rng.NextU64())
		}
		return start + int64(rng.NextU64()&mask)
	}

	random := NextWithUpperBound(rng, delta, bitWidth)
	return start + int64(random)
}

// NextInClosedRange returns a random value in the closed range [start, end].
//
// bitWidth specifies the working bit width (8, 16, 32, or 64).
func NextInClosedRange(rng RandomNumberGenerator, start, end int64, bitWidth uint) int64 {
	if start > end {
		panic("start must not be greater than end")
	}

	delta := uint64(end - start)
	mask := bitMask(bitWidth)

	if delta == mask {
		if bitWidth >= 64 {
			return int64(rng.NextU64())
		}
		return start + int64(rng.NextU64()&mask)
	}

	random := NextWithUpperBound(rng, delta+1, bitWidth)
	return start + int64(random)
}

// RandomArrayFrom returns a slice of random bytes using rng.
func RandomArrayFrom(rng RandomNumberGenerator, size int) []byte {
	data := make([]byte, size)
	rng.FillRandomData(data)
	return data
}

// RandomBool returns a random boolean using rng.
func RandomBool(rng RandomNumberGenerator) bool {
	return rng.NextU32()%2 == 0
}

// RandomU32 returns a random uint32 using rng.
func RandomU32(rng RandomNumberGenerator) uint32 {
	return rng.NextU32()
}

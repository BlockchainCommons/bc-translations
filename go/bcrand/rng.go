// Package bcrand provides random number utilities for Blockchain Commons projects.
//
// It exposes a uniform API for the random number primitives used in
// higher-level Blockchain Commons projects, including a cryptographically
// strong [SecureRandomNumberGenerator] and a deterministic
// [SeededRandomNumberGenerator].
//
// Both generators implement the [RandomNumberGenerator] interface to produce
// random numbers compatible with the RandomNumberGenerator Swift protocol
// used in macOS and iOS, which is important when using the deterministic
// random number generator for cross-platform testing.
//
// The package also includes several convenience functions for generating
// secure and deterministic random numbers.
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

// RngRandomData returns a slice of random bytes of the given size.
func RngRandomData(rng RandomNumberGenerator, size int) []byte {
	data := make([]byte, size)
	rng.FillRandomData(data)
	return data
}

// RngFillRandomData fills the given slice with random bytes.
func RngFillRandomData(rng RandomNumberGenerator, data []byte) {
	rng.FillRandomData(data)
}

// bitMask returns the bitmask for the given bit width.
func bitMask(bw uint) uint64 {
	if bw >= 64 {
		return ^uint64(0)
	}
	return (1 << bw) - 1
}

// wideMul performs a full-width multiplication of two values,
// returning (lo, hi) components at the specified bit width.
func wideMul(a, b uint64, bw uint) (lo, hi uint64) {
	switch bw {
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
		panic("unsupported bit width")
	}
}

// RngNextWithUpperBound returns a random value that is less than the given
// upper bound. The upperBound must be non-zero. Every value in [0, upperBound)
// is equally likely to be returned.
//
// The bw parameter specifies the working bit width (8, 16, 32, or 64).
func RngNextWithUpperBound(rng RandomNumberGenerator, upperBound uint64, bw uint) uint64 {
	if upperBound == 0 {
		panic("upper bound must be non-zero")
	}

	mask := bitMask(bw)
	random := rng.NextU64() & mask
	lo, hi := wideMul(random, upperBound, bw)

	if lo < upperBound {
		var t uint64
		if bw >= 64 {
			t = (0 - upperBound) % upperBound
		} else {
			t = ((1 << bw) - upperBound) % upperBound
		}
		for lo < t {
			random = rng.NextU64() & mask
			lo, hi = wideMul(random, upperBound, bw)
		}
	}

	return hi
}

// RngNextInRange returns a random value within the specified range, using the
// given generator as a source for randomness. The range is half-open: [start, end).
//
// The bw parameter specifies the working bit width (8, 16, 32, or 64).
func RngNextInRange(rng RandomNumberGenerator, start, end int64, bw uint) int64 {
	if start >= end {
		panic("start must be less than end")
	}

	delta := uint64(end - start)
	mask := bitMask(bw)

	if delta == mask {
		if bw >= 64 {
			return int64(rng.NextU64())
		}
		return start + int64(rng.NextU64()&mask)
	}

	random := RngNextWithUpperBound(rng, delta, bw)
	return start + int64(random)
}

// RngNextInClosedRange returns a random value in the closed range [start, end].
//
// The bits parameter specifies the working bit width (8, 16, 32, or 64).
func RngNextInClosedRange(rng RandomNumberGenerator, start, end int64, bw uint) int64 {
	if start > end {
		panic("start must not be greater than end")
	}

	delta := uint64(end - start)
	mask := bitMask(bw)

	if delta == mask {
		if bw >= 64 {
			return int64(rng.NextU64())
		}
		return start + int64(rng.NextU64()&mask)
	}

	random := RngNextWithUpperBound(rng, delta+1, bw)
	return start + int64(random)
}

// RngRandomArray returns a slice of random bytes of the given size.
func RngRandomArray(rng RandomNumberGenerator, size int) []byte {
	data := make([]byte, size)
	rng.FillRandomData(data)
	return data
}

// RngRandomBool returns a random boolean value.
func RngRandomBool(rng RandomNumberGenerator) bool {
	return rng.NextU32()%2 == 0
}

// RngRandomU32 returns a random uint32 value.
func RngRandomU32(rng RandomNumberGenerator) uint32 {
	return rng.NextU32()
}

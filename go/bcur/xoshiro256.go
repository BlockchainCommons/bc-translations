package bcur

import (
	"crypto/sha256"
	"encoding/binary"
	"hash/crc32"
	"math/bits"
)

// xoshiro256 implements the Xoshiro256** PRNG algorithm.
type xoshiro256 struct {
	s [4]uint64
}

// newXoshiro256FromBytes creates a new Xoshiro256** PRNG seeded from arbitrary bytes.
// The bytes are hashed with SHA-256, then the 32-byte hash is interpreted as
// 4 big-endian uint64 values for the state.
func newXoshiro256FromBytes(data []byte) *xoshiro256 {
	hash := sha256.Sum256(data)
	return newXoshiro256FromHash(hash)
}

// newXoshiro256FromString creates a new Xoshiro256** PRNG seeded from a string.
func newXoshiro256FromString(s string) *xoshiro256 {
	return newXoshiro256FromBytes([]byte(s))
}

// newXoshiro256FromHash creates a new Xoshiro256** PRNG from a 32-byte SHA-256 hash.
func newXoshiro256FromHash(hash [32]byte) *xoshiro256 {
	x := &xoshiro256{}
	for i := 0; i < 4; i++ {
		x.s[i] = binary.BigEndian.Uint64(hash[8*i : 8*i+8])
	}
	return x
}

// newXoshiro256FromCRC creates a new Xoshiro256** PRNG seeded from the CRC32 of the given bytes.
func newXoshiro256FromCRC(data []byte) *xoshiro256 {
	checksum := crc32.ChecksumIEEE(data)
	var buf [4]byte
	binary.BigEndian.PutUint32(buf[:], checksum)
	return newXoshiro256FromBytes(buf[:])
}

// next returns the next uint64 from the PRNG.
func (x *xoshiro256) next() uint64 {
	result := bits.RotateLeft64(x.s[1]*5, 7) * 9

	t := x.s[1] << 17

	x.s[2] ^= x.s[0]
	x.s[3] ^= x.s[1]
	x.s[1] ^= x.s[2]
	x.s[0] ^= x.s[3]

	x.s[2] ^= t
	x.s[3] = bits.RotateLeft64(x.s[3], 45)

	return result
}

// nextDouble returns a uniformly distributed float64 in [0, 1).
func (x *xoshiro256) nextDouble() float64 {
	return float64(x.next()) / (float64(^uint64(0)) + 1.0)
}

// nextInt returns a uniformly distributed uint64 in [low, high].
func (x *xoshiro256) nextInt(low, high uint64) uint64 {
	return uint64(x.nextDouble()*float64(high-low+1)) + low
}

// nextByte returns a random byte (used for test utilities).
func (x *xoshiro256) nextByte() byte {
	return byte(x.nextInt(0, 255))
}

// nextBytes returns n random bytes (used for test utilities).
func (x *xoshiro256) nextBytes(n int) []byte {
	result := make([]byte, n)
	for i := range result {
		result[i] = x.nextByte()
	}
	return result
}

// shuffled returns a shuffled copy of items using Fisher-Yates.
func (x *xoshiro256) shuffled(items []int) []int {
	remaining := make([]int, len(items))
	copy(remaining, items)
	shuffled := make([]int, 0, len(items))
	for len(remaining) > 0 {
		index := int(x.nextInt(0, uint64(len(remaining)-1)))
		shuffled = append(shuffled, remaining[index])
		remaining = append(remaining[:index], remaining[index+1:]...)
	}
	return shuffled
}

// chooseDegree selects a degree for fountain encoding using harmonic weights.
func (x *xoshiro256) chooseDegree(length int) int {
	weights := make([]float64, length)
	for i := 0; i < length; i++ {
		weights[i] = 1.0 / float64(i+1)
	}
	sampler := newWeightedSampler(weights)
	return sampler.next(x) + 1
}

// makeMessage generates a deterministic pseudo-random byte sequence for test vectors.
func makeMessage(seed string, size int) []byte {
	rng := newXoshiro256FromString(seed)
	return rng.nextBytes(size)
}

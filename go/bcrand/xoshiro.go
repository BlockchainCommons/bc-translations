package bcrand

// xoshiro256StarStar implements the Xoshiro256** PRNG algorithm.
// Reference: https://prng.di.unimi.it/xoshiro256starstar.c
type xoshiro256StarStar struct {
	s [4]uint64
}

func newXoshiro256StarStar(s0, s1, s2, s3 uint64) *xoshiro256StarStar {
	return &xoshiro256StarStar{s: [4]uint64{s0, s1, s2, s3}}
}

func rotl64(x uint64, k uint) uint64 {
	return (x << k) | (x >> (64 - k))
}

func (x *xoshiro256StarStar) nextU64() uint64 {
	result := rotl64(x.s[1]*5, 7) * 9

	t := x.s[1] << 17

	x.s[2] ^= x.s[0]
	x.s[3] ^= x.s[1]
	x.s[1] ^= x.s[2]
	x.s[0] ^= x.s[3]

	x.s[2] ^= t
	x.s[3] = rotl64(x.s[3], 45)

	return result
}

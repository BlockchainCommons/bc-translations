package bcrand

// SeededRandomNumberGenerator is a deterministic pseudo-random number generator
// for testing purposes. It uses the Xoshiro256** algorithm internally.
//
// This is NOT cryptographically secure and should only be used for testing.
type SeededRandomNumberGenerator struct {
	rng *xoshiro256StarStar
}

// NewSeededRandomNumberGenerator creates a new seeded random number generator.
//
// The seed is a 256-bit value represented as an array of 4 uint64 values.
// For the output distribution to look random, the seed should not have any
// obvious patterns like all zeroes or all ones.
//
// This is not cryptographically secure, and should only be used for testing
// purposes.
func NewSeededRandomNumberGenerator(seed [4]uint64) *SeededRandomNumberGenerator {
	return &SeededRandomNumberGenerator{
		rng: newXoshiro256StarStar(seed[0], seed[1], seed[2], seed[3]),
	}
}

func (s *SeededRandomNumberGenerator) NextU32() uint32 {
	return uint32(s.NextU64())
}

func (s *SeededRandomNumberGenerator) NextU64() uint64 {
	return s.rng.nextU64()
}

// RandomData returns a slice of deterministic pseudo-random bytes.
//
// Each byte is generated individually as byte(NextU64()) for cross-platform
// test vector compatibility.
func (s *SeededRandomNumberGenerator) RandomData(size int) []byte {
	data := make([]byte, size)
	for i := range data {
		data[i] = byte(s.NextU64())
	}
	return data
}

// FillRandomData fills the given slice with deterministic pseudo-random bytes.
//
// Each byte is generated individually as byte(NextU64()) for cross-platform
// test vector compatibility.
func (s *SeededRandomNumberGenerator) FillRandomData(data []byte) {
	for i := range data {
		data[i] = byte(s.NextU64())
	}
}

var fakeSeed = [4]uint64{
	17295166580085024720,
	422929670265678780,
	5577237070365765850,
	7953171132032326923,
}

// MakeFakeRandomNumberGenerator creates a seeded random number generator with a
// fixed seed.
func MakeFakeRandomNumberGenerator() *SeededRandomNumberGenerator {
	return NewSeededRandomNumberGenerator(fakeSeed)
}

// FakeRandomData creates a slice of random data with a fixed seed.
func FakeRandomData(size int) []byte {
	return MakeFakeRandomNumberGenerator().RandomData(size)
}

package bcshamir

import (
	"bytes"
	"encoding/hex"
	"testing"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

// fakeRandomNumberGenerator produces sequential bytes starting at 0 with
// step 17 (wrapping). Used for deterministic test vectors.
type fakeRandomNumberGenerator struct{}

func (f *fakeRandomNumberGenerator) NextU32() uint32 { panic("not implemented") }
func (f *fakeRandomNumberGenerator) NextU64() uint64 { panic("not implemented") }

func (f *fakeRandomNumberGenerator) RandomData(size int) []byte {
	b := make([]byte, size)
	f.FillRandomData(b)
	return b
}

func (f *fakeRandomNumberGenerator) FillRandomData(data []byte) {
	var b byte
	for i := range data {
		data[i] = b
		b += 17
	}
}

func mustDecodeHex(s string) []byte {
	b, err := hex.DecodeString(s)
	if err != nil {
		panic(err)
	}
	return b
}

func TestSplitSecret3_5(t *testing.T) {
	rng := &fakeRandomNumberGenerator{}
	secret := mustDecodeHex("0ff784df000c4380a5ed683f7e6e3dcf")

	shares, err := SplitSecret(3, 5, secret, rng)
	if err != nil {
		t.Fatalf("SplitSecret failed: %v", err)
	}
	if len(shares) != 5 {
		t.Fatalf("expected 5 shares, got %d", len(shares))
	}

	expected := []string{
		"00112233445566778899aabbccddeeff",
		"d43099fe444807c46921a4f33a2a798b",
		"d9ad4e3bec2e1a7485698823abf05d36",
		"0d8cf5f6ec337bc764d1866b5d07ca42",
		"1aa7fe3199bc5092ef3816b074cabdf2",
	}
	for i, exp := range expected {
		want := mustDecodeHex(exp)
		if !bytes.Equal(shares[i], want) {
			t.Errorf("share[%d] = %x, want %x", i, shares[i], want)
		}
	}

	recoveredIndexes := []int{1, 2, 4}
	recoveredShares := make([][]byte, len(recoveredIndexes))
	for i, idx := range recoveredIndexes {
		recoveredShares[i] = shares[idx]
	}
	recoveredSecret, err := RecoverSecret(recoveredIndexes, recoveredShares)
	if err != nil {
		t.Fatalf("RecoverSecret failed: %v", err)
	}
	if !bytes.Equal(recoveredSecret, secret) {
		t.Errorf("recovered secret = %x, want %x", recoveredSecret, secret)
	}
}

func TestSplitSecret2_7(t *testing.T) {
	rng := &fakeRandomNumberGenerator{}
	secret := mustDecodeHex("204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a")

	shares, err := SplitSecret(2, 7, secret, rng)
	if err != nil {
		t.Fatalf("SplitSecret failed: %v", err)
	}
	if len(shares) != 7 {
		t.Fatalf("expected 7 shares, got %d", len(shares))
	}

	expected := []string{
		"2dcd14c2252dc8489af3985030e74d5a48e8eff1478ab86e65b43869bf39d556",
		"a1dfdd798388aada635b9974472b4fc59a32ae520c42c9f6a0af70149b882487",
		"2ee99daf727c0c7773b89a18de64497ff7476dacd1015a45f482a893f7402cef",
		"a2fb5414d4d96ee58a109b3ca9a84be0259d2c0f9ac92bdd3199e0eed3f1dd3e",
		"2b851d188b8f5b3653659cc0f7fa45102dadf04b708767385cd803862fcb3c3f",
		"a797d4a32d2a39a4aacd9de48036478fff77b1e83b4f16a099c34bfb0b7acdee",
		"28a19475dcde9f09ba2e9e881979413592027216e60c8513cdee937c67b2c586",
	}
	for i, exp := range expected {
		want := mustDecodeHex(exp)
		if !bytes.Equal(shares[i], want) {
			t.Errorf("share[%d] = %x, want %x", i, shares[i], want)
		}
	}

	recoveredIndexes := []int{3, 4}
	recoveredShares := make([][]byte, len(recoveredIndexes))
	for i, idx := range recoveredIndexes {
		recoveredShares[i] = shares[idx]
	}
	recoveredSecret, err := RecoverSecret(recoveredIndexes, recoveredShares)
	if err != nil {
		t.Fatalf("RecoverSecret failed: %v", err)
	}
	if !bytes.Equal(recoveredSecret, secret) {
		t.Errorf("recovered secret = %x, want %x", recoveredSecret, secret)
	}
}

func TestExampleSplit(t *testing.T) {
	rng := bcrand.NewSecureRandomNumberGenerator()
	secret := []byte("my secret belongs to me.")

	shares, err := SplitSecret(2, 3, secret, rng)
	if err != nil {
		t.Fatalf("SplitSecret failed: %v", err)
	}
	if len(shares) != 3 {
		t.Fatalf("expected 3 shares, got %d", len(shares))
	}
}

func TestExampleRecover(t *testing.T) {
	indexes := []int{0, 2}
	shares := [][]byte{
		{
			47, 165, 102, 232, 218, 99, 6, 94, 39, 6, 253, 215, 12, 88, 64, 32,
			105, 40, 222, 146, 93, 197, 48, 129,
		},
		{
			221, 174, 116, 201, 90, 99, 136, 33, 64, 215, 60, 84, 207, 28, 74,
			10, 111, 243, 43, 224, 48, 64, 199, 172,
		},
	}

	secret, err := RecoverSecret(indexes, shares)
	if err != nil {
		t.Fatalf("RecoverSecret failed: %v", err)
	}
	expected := []byte("my secret belongs to me.")
	if !bytes.Equal(secret, expected) {
		t.Errorf("recovered secret = %q, want %q", secret, expected)
	}
}

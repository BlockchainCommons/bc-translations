package bcrand

import (
	"bytes"
	"encoding/hex"
	"testing"
)

var testSeed = [4]uint64{
	17295166580085024720,
	422929670265678780,
	5577237070365765850,
	7953171132032326923,
}

func newTestRNG() *SeededRandomNumberGenerator {
	return NewSeededRandomNumberGenerator(testSeed)
}

func TestNextU64(t *testing.T) {
	rng := newTestRNG()
	got := rng.NextU64()
	want := uint64(1104683000648959614)
	if got != want {
		t.Errorf("NextU64() = %d, want %d", got, want)
	}
}

func TestNext50(t *testing.T) {
	rng := newTestRNG()
	expected := []uint64{
		1104683000648959614,
		9817345228149227957,
		546276821344993881,
		15870950426333349563,
		830653509032165567,
		14772257893953840492,
		3512633850838187726,
		6358411077290857510,
		7897285047238174514,
		18314839336815726031,
		4978716052961022367,
		17373022694051233817,
		663115362299242570,
		9811238046242345451,
		8113787839071393872,
		16155047452816275860,
		673245095821315645,
		1610087492396736743,
		1749670338128618977,
		3927771759340679115,
		9610589375631783853,
		5311608497352460372,
		11014490817524419548,
		6320099928172676090,
		12513554919020212402,
		6823504187935853178,
		1215405011954300226,
		8109228150255944821,
		4122548551796094879,
		16544885818373129566,
		5597102191057004591,
		11690994260783567085,
		9374498734039011409,
		18246806104446739078,
		2337407889179712900,
		12608919248151905477,
		7641631838640172886,
		8421574250687361351,
		8697189342072434208,
		8766286633078002696,
		14800090277885439654,
		17865860059234099833,
		4673315107448681522,
		14288183874156623863,
		7587575203648284614,
		9109213819045273474,
		11817665411945280786,
		1745089530919138651,
		5730370365819793488,
		5496865518262805451,
	}
	for i, want := range expected {
		got := rng.NextU64()
		if got != want {
			t.Errorf("NextU64() #%d = %d, want %d", i, got, want)
		}
	}
}

func TestFakeRandomData(t *testing.T) {
	got := FakeRandomData(100)
	want, err := hex.DecodeString(
		"7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed" +
			"518684c556472008a67932f7c682125b50cb72e8216f6906358fdaf28d354553" +
			"2daee0c5bb5023f50cd8e71ec14901ac746c576c481b893be6656b80622b3a56" +
			"4e59b4e2")
	if err != nil {
		t.Fatal(err)
	}
	if !bytes.Equal(got, want) {
		t.Errorf("FakeRandomData(100) =\n  %x\nwant\n  %x", got, want)
	}
}

func TestNextWithUpperBound(t *testing.T) {
	rng := newTestRNG()
	got := NextWithUpperBound(rng, 10000, 32)
	want := uint64(745)
	if got != want {
		t.Errorf("NextWithUpperBound(rng, 10000, 32) = %d, want %d", got, want)
	}
}

func TestInRange(t *testing.T) {
	rng := newTestRNG()
	expected := []int64{
		7, 44, 92, 16, 16, 67, 41, 74, 66, 20, 18, 6, 62, 34, 4, 69, 99,
		19, 0, 85, 22, 27, 56, 23, 19, 5, 23, 76, 80, 27, 74, 69, 17, 92,
		31, 32, 55, 36, 49, 23, 53, 2, 46, 6, 43, 66, 34, 71, 64, 69, 25,
		14, 17, 23, 32, 6, 23, 65, 35, 11, 21, 37, 58, 92, 98, 8, 38, 49,
		7, 24, 24, 71, 37, 63, 91, 21, 11, 66, 52, 54, 55, 19, 76, 46, 89,
		38, 91, 95, 33, 25, 4, 30, 66, 51, 5, 91, 62, 27, 92, 39,
	}
	for i, want := range expected {
		got := NextInRange(rng, 0, 100, 32)
		if got != want {
			t.Errorf("NextInRange #%d = %d, want %d", i, got, want)
		}
	}
}

func TestFillRandomData(t *testing.T) {
	rng1 := NewSeededRandomNumberGenerator(testSeed)
	v1 := rng1.RandomData(100)

	rng2 := NewSeededRandomNumberGenerator(testSeed)
	v2 := make([]byte, 100)
	rng2.FillRandomData(v2)

	if !bytes.Equal(v1, v2) {
		t.Errorf("RandomData and FillRandomData produced different results")
	}
}

func TestFakeNumbers(t *testing.T) {
	rng := NewFakeRandomNumberGenerator()
	expected := []int64{
		-43, -6, 43, -34, -34, 17, -9, 24, 17, -29, -32, -44, 12, -15, -46,
		20, 50, -31, -50, 36, -28, -23, 6, -27, -31, -45, -27, 26, 31, -23,
		24, 19, -32, 43, -18, -17, 6, -13, -1, -27, 4, -48, -4, -44, -6, 17,
		-15, 22, 15, 20, -25, -35, -33, -27, -17, -44, -27, 15, -14, -38,
		-29, -12, 8, 43, 49, -42, -11, -1, -42, -26, -25, 22, -13, 14, 42,
		-29, -38, 17, 2, 5, 5, -31, 27, -3, 39, -12, 42, 46, -17, -25, -46,
		-19, 16, 2, -45, 41, 12, -22, 43, -11,
	}
	for i, want := range expected {
		got := NextInClosedRange(rng, -50, 50, 32)
		if got != want {
			t.Errorf("NextInClosedRange #%d = %d, want %d", i, got, want)
		}
	}
}

func TestRandomData(t *testing.T) {
	data1 := RandomData(32)
	data2 := RandomData(32)
	data3 := RandomData(32)

	if len(data1) != 32 {
		t.Errorf("RandomData(32) length = %d, want 32", len(data1))
	}
	if bytes.Equal(data1, data2) {
		t.Error("RandomData produced identical results on consecutive calls")
	}
	if bytes.Equal(data1, data3) {
		t.Error("RandomData produced identical results on non-consecutive calls")
	}
}

// TestInterfaceCompliance verifies that both RNG types satisfy the interface.
func TestInterfaceCompliance(t *testing.T) {
	var _ RandomNumberGenerator = &SecureRandomNumberGenerator{}
	var _ RandomNumberGenerator = &SeededRandomNumberGenerator{}
}

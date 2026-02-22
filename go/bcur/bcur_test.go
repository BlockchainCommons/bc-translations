package bcur

import (
	"bytes"
	"encoding/hex"
	"fmt"
	"slices"
	"testing"
)

// ============= CRC32 Tests =============

func TestCRC(t *testing.T) {
	if got := crc32Checksum([]byte("Hello, world!")); got != 0xebe6c6e6 {
		t.Errorf("CRC32('Hello, world!') = 0x%08x, want 0xebe6c6e6", got)
	}
	if got := crc32Checksum([]byte("Wolf")); got != 0x598c84dc {
		t.Errorf("CRC32('Wolf') = 0x%08x, want 0x598c84dc", got)
	}
}

// ============= Xoshiro256 Tests =============

func TestRNG1(t *testing.T) {
	rng := newXoshiro256FromString("Wolf")
	expected := []uint64{
		42, 81, 85, 8, 82, 84, 76, 73, 70, 88, 2, 74, 40, 48, 77, 54, 88, 7, 5, 88, 37, 25, 82,
		13, 69, 59, 30, 39, 11, 82, 19, 99, 45, 87, 30, 15, 32, 22, 89, 44, 92, 77, 29, 78, 4,
		92, 44, 68, 92, 69, 1, 42, 89, 50, 37, 84, 63, 34, 32, 3, 17, 62, 40, 98, 82, 89, 24,
		43, 85, 39, 15, 3, 99, 29, 20, 42, 27, 10, 85, 66, 50, 35, 69, 70, 70, 74, 30, 13, 72,
		54, 11, 5, 70, 55, 91, 52, 10, 43, 43, 52,
	}
	for i, e := range expected {
		got := rng.next() % 100
		if got != e {
			t.Errorf("rng1[%d] = %d, want %d", i, got, e)
		}
	}
}

func TestRNG2(t *testing.T) {
	rng := newXoshiro256FromCRC([]byte("Wolf"))
	expected := []uint64{
		88, 44, 94, 74, 0, 99, 7, 77, 68, 35, 47, 78, 19, 21, 50, 15, 42, 36, 91, 11, 85, 39,
		64, 22, 57, 11, 25, 12, 1, 91, 17, 75, 29, 47, 88, 11, 68, 58, 27, 65, 21, 54, 47, 54,
		73, 83, 23, 58, 75, 27, 26, 15, 60, 36, 30, 21, 55, 57, 77, 76, 75, 47, 53, 76, 9, 91,
		14, 69, 3, 95, 11, 73, 20, 99, 68, 61, 3, 98, 36, 98, 56, 65, 14, 80, 74, 57, 63, 68,
		51, 56, 24, 39, 53, 80, 57, 51, 81, 3, 1, 30,
	}
	for i, e := range expected {
		got := rng.next() % 100
		if got != e {
			t.Errorf("rng2[%d] = %d, want %d", i, got, e)
		}
	}
}

func TestRNG3(t *testing.T) {
	rng := newXoshiro256FromString("Wolf")
	expected := []uint64{
		6, 5, 8, 4, 10, 5, 7, 10, 4, 9, 10, 9, 7, 7, 1, 1, 2, 9, 9, 2, 6, 4, 5, 7, 8, 5, 4, 2,
		3, 8, 7, 4, 5, 1, 10, 9, 3, 10, 2, 6, 8, 5, 7, 9, 3, 1, 5, 2, 7, 1, 4, 4, 4, 4, 9, 4,
		5, 5, 6, 9, 5, 1, 2, 8, 3, 3, 2, 8, 4, 3, 2, 1, 10, 8, 9, 3, 10, 8, 5, 5, 6, 7, 10, 5,
		8, 9, 4, 6, 4, 2, 10, 2, 1, 7, 9, 6, 7, 4, 2, 5,
	}
	for i, e := range expected {
		got := rng.nextInt(1, 10)
		if got != e {
			t.Errorf("rng3[%d] = %d, want %d", i, got, e)
		}
	}
}

func TestShuffle(t *testing.T) {
	rng := newXoshiro256FromString("Wolf")
	values := []int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}
	expected := [][]int{
		{6, 4, 9, 3, 10, 5, 7, 8, 1, 2},
		{10, 8, 6, 5, 1, 2, 3, 9, 7, 4},
		{6, 4, 5, 8, 9, 3, 2, 1, 7, 10},
		{7, 3, 5, 1, 10, 9, 4, 8, 2, 6},
		{8, 5, 7, 10, 2, 1, 4, 3, 9, 6},
		{4, 3, 5, 6, 10, 2, 7, 8, 9, 1},
		{5, 1, 3, 9, 4, 6, 2, 10, 7, 8},
		{2, 1, 10, 8, 9, 4, 7, 6, 3, 5},
		{6, 7, 10, 4, 8, 9, 2, 3, 1, 5},
		{10, 2, 1, 7, 9, 5, 6, 3, 4, 8},
	}
	for i, e := range expected {
		input := make([]int, len(values))
		copy(input, values)
		shuffled := rng.shuffled(input)
		for j := range e {
			if shuffled[j] != e[j] {
				t.Errorf("shuffle[%d][%d] = %d, want %d", i, j, shuffled[j], e[j])
				break
			}
		}
	}
}

// ============= Sampler Tests =============

func TestSampler(t *testing.T) {
	weights := []float64{1.0, 2.0, 4.0, 8.0}
	rng := newXoshiro256FromString("Wolf")
	sampler := newWeightedSampler(weights)

	expectedSamples := []int{
		3, 3, 3, 3, 3, 3, 3, 0, 2, 3, 3, 3, 3, 1, 2, 2, 1, 3, 3, 2, 3, 3, 1, 1, 2, 1, 1, 3, 1,
		3, 1, 2, 0, 2, 1, 0, 3, 3, 3, 1, 3, 3, 3, 3, 1, 3, 2, 3, 2, 2, 3, 3, 3, 3, 2, 3, 3, 0,
		3, 3, 3, 3, 1, 2, 3, 3, 2, 2, 2, 1, 2, 2, 1, 2, 3, 1, 3, 0, 3, 2, 3, 3, 3, 3, 3, 3, 3,
		3, 2, 3, 1, 3, 3, 2, 0, 2, 2, 3, 1, 1, 2, 3, 2, 3, 3, 3, 3, 2, 3, 3, 3, 3, 3, 2, 3, 1,
		2, 1, 1, 3, 1, 3, 2, 2, 3, 3, 3, 1, 3, 3, 3, 3, 3, 3, 3, 3, 2, 3, 2, 3, 3, 1, 2, 3, 3,
		1, 3, 2, 3, 3, 3, 2, 3, 1, 3, 0, 3, 2, 1, 1, 3, 1, 3, 2, 3, 3, 3, 3, 2, 0, 3, 3, 1, 3,
		0, 2, 1, 3, 3, 1, 1, 3, 1, 2, 3, 3, 3, 0, 2, 3, 2, 0, 1, 3, 3, 3, 2, 2, 2, 3, 3, 3, 3,
		3, 2, 3, 3, 3, 3, 2, 3, 3, 2, 0, 2, 3, 3, 3, 3, 2, 1, 1, 1, 2, 1, 3, 3, 3, 2, 2, 3, 3,
		1, 2, 3, 0, 3, 2, 3, 3, 3, 3, 0, 2, 2, 3, 2, 2, 3, 3, 3, 3, 1, 3, 2, 3, 3, 3, 3, 3, 2,
		2, 3, 1, 3, 0, 2, 1, 3, 3, 3, 3, 3, 3, 3, 3, 1, 3, 3, 3, 3, 2, 2, 2, 3, 1, 1, 3, 2, 2,
		0, 3, 2, 1, 2, 1, 0, 3, 3, 3, 2, 2, 3, 2, 1, 2, 0, 0, 3, 3, 2, 3, 3, 2, 3, 3, 3, 3, 3,
		2, 2, 2, 3, 3, 3, 3, 3, 1, 1, 3, 2, 2, 3, 1, 1, 0, 1, 3, 2, 3, 3, 2, 3, 3, 2, 3, 3, 2,
		2, 2, 2, 3, 2, 2, 2, 2, 2, 1, 2, 3, 3, 2, 2, 2, 2, 3, 3, 2, 0, 2, 1, 3, 3, 3, 3, 0, 3,
		3, 3, 3, 2, 2, 3, 1, 3, 3, 3, 2, 3, 3, 3, 2, 3, 3, 3, 3, 2, 3, 2, 1, 3, 3, 3, 3, 2, 2,
		0, 1, 2, 3, 2, 0, 3, 3, 3, 3, 3, 3, 1, 3, 3, 2, 3, 2, 2, 3, 3, 3, 3, 3, 2, 2, 3, 3, 2,
		2, 2, 1, 3, 3, 3, 3, 1, 2, 3, 2, 3, 3, 2, 3, 2, 3, 3, 3, 2, 3, 1, 2, 3, 2, 1, 1, 3, 3,
		2, 3, 3, 2, 3, 3, 0, 0, 1, 3, 3, 2, 3, 3, 3, 3, 1, 3, 3, 0, 3, 2, 3, 3, 1, 3, 3, 3, 3,
		3, 3, 3, 0, 3, 3, 2,
	}
	for i, e := range expectedSamples {
		got := sampler.next(rng)
		if got != e {
			t.Errorf("sampler[%d] = %d, want %d", i, got, e)
		}
	}
}

func TestChooseDegree(t *testing.T) {
	message := makeMessage("Wolf", 1024)
	fragLen := fragmentLength(len(message), 100)
	fragments := partition(message, fragLen)
	expectedDegrees := []int{
		11, 3, 6, 5, 2, 1, 2, 11, 1, 3, 9, 10, 10, 4, 2, 1, 1, 2, 1, 1, 5, 2, 4, 10, 3, 2, 1,
		1, 3, 11, 2, 6, 2, 9, 9, 2, 6, 7, 2, 5, 2, 4, 3, 1, 6, 11, 2, 11, 3, 1, 6, 3, 1, 4, 5,
		3, 6, 1, 1, 3, 1, 2, 2, 1, 4, 5, 1, 1, 9, 1, 1, 6, 4, 1, 5, 1, 2, 2, 3, 1, 1, 5, 2, 6,
		1, 7, 11, 1, 8, 1, 5, 1, 1, 2, 2, 6, 4, 10, 1, 2, 5, 5, 5, 1, 1, 4, 1, 1, 1, 3, 5, 5,
		5, 1, 4, 3, 3, 5, 1, 11, 3, 2, 8, 1, 2, 1, 1, 4, 5, 2, 1, 1, 1, 5, 6, 11, 10, 7, 4, 7,
		1, 5, 3, 1, 1, 9, 1, 2, 5, 5, 2, 2, 3, 10, 1, 3, 2, 3, 3, 1, 1, 2, 1, 3, 2, 2, 1, 3, 8,
		4, 1, 11, 6, 3, 1, 1, 1, 1, 1, 3, 1, 2, 1, 10, 1, 1, 8, 2, 7, 1, 2, 1, 9, 2, 10, 2, 1,
		3, 4, 10,
	}
	for nonce := 1; nonce <= 200; nonce++ {
		rng := newXoshiro256FromString(fmt.Sprintf("Wolf-%d", nonce))
		got := rng.chooseDegree(len(fragments))
		if got != expectedDegrees[nonce-1] {
			t.Errorf("chooseDegree(nonce=%d) = %d, want %d", nonce, got, expectedDegrees[nonce-1])
		}
	}
}

// ============= Bytewords Tests =============

func TestBytewords(t *testing.T) {
	input := []byte{0, 1, 2, 128, 255}

	std := BytewordsEncode(input, BytewordsStandard)
	if std != "able acid also lava zoom jade need echo taxi" {
		t.Errorf("Standard encode = %q", std)
	}

	uri := BytewordsEncode(input, BytewordsURI)
	if uri != "able-acid-also-lava-zoom-jade-need-echo-taxi" {
		t.Errorf("URI encode = %q", uri)
	}

	min := BytewordsEncode(input, BytewordsMinimal)
	if min != "aeadaolazmjendeoti" {
		t.Errorf("Minimal encode = %q", min)
	}

	// Decode
	decoded, err := BytewordsDecode("able acid also lava zoom jade need echo taxi", BytewordsStandard)
	if err != nil || !bytes.Equal(decoded, input) {
		t.Errorf("Standard decode failed: %v %v", decoded, err)
	}

	decoded, err = BytewordsDecode("able-acid-also-lava-zoom-jade-need-echo-taxi", BytewordsURI)
	if err != nil || !bytes.Equal(decoded, input) {
		t.Errorf("URI decode failed")
	}

	decoded, err = BytewordsDecode("aeadaolazmjendeoti", BytewordsMinimal)
	if err != nil || !bytes.Equal(decoded, input) {
		t.Errorf("Minimal decode failed")
	}

	// Empty payload is allowed
	empty := BytewordsEncode([]byte{}, BytewordsMinimal)
	_, err = BytewordsDecode(empty, BytewordsMinimal)
	if err != nil {
		t.Errorf("empty decode failed: %v", err)
	}

	// Bad checksum
	_, err = BytewordsDecode("able acid also lava zero jade need echo wolf", BytewordsStandard)
	if err != ErrInvalidChecksum {
		t.Errorf("expected ErrInvalidChecksum, got %v", err)
	}

	_, err = BytewordsDecode("able-acid-also-lava-zero-jade-need-echo-wolf", BytewordsURI)
	if err != ErrInvalidChecksum {
		t.Errorf("expected ErrInvalidChecksum, got %v", err)
	}

	_, err = BytewordsDecode("aeadaolazojendeowf", BytewordsMinimal)
	if err != ErrInvalidChecksum {
		t.Errorf("expected ErrInvalidChecksum, got %v", err)
	}

	// Too short
	_, err = BytewordsDecode("wolf", BytewordsStandard)
	if err != ErrInvalidChecksum {
		t.Errorf("expected ErrInvalidChecksum for 'wolf', got %v", err)
	}

	_, err = BytewordsDecode("", BytewordsStandard)
	if err != ErrInvalidWord {
		t.Errorf("expected ErrInvalidWord for empty, got %v", err)
	}

	// Invalid length
	_, err = BytewordsDecode("aea", BytewordsMinimal)
	if err != ErrInvalidLength {
		t.Errorf("expected ErrInvalidLength, got %v", err)
	}

	// Non-ASCII
	_, err = BytewordsDecode("\u20bf", BytewordsStandard)
	if err != ErrNonASCII {
		t.Errorf("expected ErrNonASCII, got %v", err)
	}
}

func TestBytewordsEncoding(t *testing.T) {
	input := []byte{
		245, 215, 20, 198, 241, 235, 69, 59, 209, 205, 165, 18, 150, 158, 116, 135, 229, 212,
		19, 159, 17, 37, 239, 240, 253, 11, 109, 191, 37, 242, 38, 120, 223, 41, 156, 189, 242,
		254, 147, 204, 66, 163, 216, 175, 191, 72, 169, 54, 32, 60, 144, 230, 210, 137, 184,
		197, 33, 113, 88, 14, 157, 31, 177, 46, 1, 115, 205, 69, 225, 150, 65, 235, 58, 144,
		65, 240, 133, 69, 113, 247, 63, 53, 242, 165, 160, 144, 26, 13, 79, 237, 133, 71, 82,
		69, 254, 165, 138, 41, 85, 24,
	}

	expectedStandard := "yank toys bulb skew when warm free fair tent swan " +
		"open brag mint noon jury list view tiny brew note " +
		"body data webs what zinc bald join runs data whiz " +
		"days keys user diet news ruby whiz zone menu surf " +
		"flew omit trip pose runs fund part even crux fern " +
		"math visa tied loud redo silk curl jugs hard beta " +
		"next cost puma drum acid junk swan free very mint " +
		"flap warm fact math flap what limp free jugs yell " +
		"fish epic whiz open numb math city belt glow wave " +
		"limp fuel grim free zone open love diet gyro cats " +
		"fizz holy city puff"

	expectedMinimal := "yktsbbswwnwmfefrttsnonbgmtnnjyltvwtybwne" +
		"bydawswtzcbdjnrsdawzdsksurdtnsrywzzemusf" +
		"fwottppersfdptencxfnmhvatdldroskcljshdba" +
		"ntctpadmadjksnfevymtfpwmftmhfpwtlpfejsyl" +
		"fhecwzonnbmhcybtgwwelpflgmfezeonledtgocs" +
		"fzhycypf"

	decoded, err := BytewordsDecode(expectedStandard, BytewordsStandard)
	if err != nil {
		t.Fatalf("Standard decode failed: %v", err)
	}
	if !bytes.Equal(decoded, input) {
		t.Error("Standard decode mismatch")
	}

	decoded, err = BytewordsDecode(expectedMinimal, BytewordsMinimal)
	if err != nil {
		t.Fatalf("Minimal decode failed: %v", err)
	}
	if !bytes.Equal(decoded, input) {
		t.Error("Minimal decode mismatch")
	}

	if got := BytewordsEncode(input, BytewordsStandard); got != expectedStandard {
		t.Errorf("Standard encode mismatch:\ngot:  %s\nwant: %s", got, expectedStandard)
	}
	if got := BytewordsEncode(input, BytewordsMinimal); got != expectedMinimal {
		t.Errorf("Minimal encode mismatch:\ngot:  %s\nwant: %s", got, expectedMinimal)
	}
}

func TestBytemojiUniqueness(t *testing.T) {
	seen := make(map[string]int)
	for _, emoji := range Bytemojis {
		seen[emoji]++
	}
	for emoji, count := range seen {
		if count > 1 {
			t.Errorf("duplicate bytemoji: %s (count=%d)", emoji, count)
		}
	}
}

func TestBytemojiLengths(t *testing.T) {
	for i, emoji := range Bytemojis {
		if len(emoji) > 4 {
			t.Errorf("bytemoji[%d] %s is %d bytes, want <= 4", i, emoji, len(emoji))
		}
	}
}

// ============= Fountain Tests =============

func TestFragmentLength(t *testing.T) {
	tests := []struct {
		dataLen, maxFrag, want int
	}{
		{12345, 1955, 1764},
		{12345, 30000, 12345},
		{10, 4, 4},
		{10, 5, 5},
		{10, 6, 5},
		{10, 10, 10},
	}
	for _, tt := range tests {
		got := fragmentLength(tt.dataLen, tt.maxFrag)
		if got != tt.want {
			t.Errorf("fragmentLength(%d, %d) = %d, want %d", tt.dataLen, tt.maxFrag, got, tt.want)
		}
	}
}

func TestPartitionAndJoin(t *testing.T) {
	message := makeMessage("Wolf", 1024)
	fragLen := fragmentLength(len(message), 100)
	fragments := partition(message, fragLen)

	expectedFragments := []string{
		"916ec65cf77cadf55cd7f9cda1a1030026ddd42e905b77adc36e4f2d3ccba44f7f04f2de44f42d84c374a0e149136f25b01852545961d55f7f7a8cde6d0e2ec43f3b2dcb644a2209e8c9e34af5c4747984a5e873c9cf5f965e25ee29039f",
		"df8ca74f1c769fc07eb7ebaec46e0695aea6cbd60b3ec4bbff1b9ffe8a9e7240129377b9d3711ed38d412fbb4442256f1e6f595e0fc57fed451fb0a0101fb76b1fb1e1b88cfdfdaa946294a47de8fff173f021c0e6f65b05c0a494e50791",
		"270a0050a73ae69b6725505a2ec8a5791457c9876dd34aadd192a53aa0dc66b556c0c215c7ceb8248b717c22951e65305b56a3706e3e86eb01c803bbf915d80edcd64d4d41977fa6f78dc07eecd072aae5bc8a852397e06034dba6a0b570",
		"797c3a89b16673c94838d884923b8186ee2db5c98407cab15e13678d072b43e406ad49477c2e45e85e52ca82a94f6df7bbbe7afbed3a3a830029f29090f25217e48d1f42993a640a67916aa7480177354cc7440215ae41e4d02eae9a1912",
		"33a6d4922a792c1b7244aa879fefdb4628dc8b0923568869a983b8c661ffab9b2ed2c149e38d41fba090b94155adbed32f8b18142ff0d7de4eeef2b04adf26f2456b46775c6c20b37602df7da179e2332feba8329bbb8d727a138b4ba7a5",
		"03215eda2ef1e953d89383a382c11d3f2cad37a4ee59a91236a3e56dcf89f6ac81dd4159989c317bd649d9cbc617f73fe10033bd288c60977481a09b343d3f676070e67da757b86de27bfca74392bac2996f7822a7d8f71a489ec6180390",
		"089ea80a8fcd6526413ec6c9a339115f111d78ef21d456660aa85f790910ffa2dc58d6a5b93705caef1091474938bd312427021ad1eeafbd19e0d916ddb111fabd8dcab5ad6a6ec3a9c6973809580cb2c164e26686b5b98cfb017a337968",
		"c7daaa14ae5152a067277b1b3902677d979f8e39cc2aafb3bc06fcf69160a853e6869dcc09a11b5009f91e6b89e5b927ab1527a735660faa6012b420dd926d940d742be6a64fb01cdc0cff9faa323f02ba41436871a0eab851e7f5782d10",
		"fbefde2a7e9ae9dc1e5c2c48f74f6c824ce9ef3c89f68800d44587bedc4ab417cfb3e7447d90e1e417e6e05d30e87239d3a5d1d45993d4461e60a0192831640aa32dedde185a371ded2ae15f8a93dba8809482ce49225daadfbb0fec629e",
		"23880789bdf9ed73be57fa84d555134630e8d0f7df48349f29869a477c13ccca9cd555ac42ad7f568416c3d61959d0ed568b2b81c7771e9088ad7fd55fd4386bafbf5a528c30f107139249357368ffa980de2c76ddd9ce4191376be0e6b5",
		"170010067e2e75ebe2d2904aeb1f89d5dc98cd4a6f2faaa8be6d03354c990fd895a97feb54668473e9d942bb99e196d897e8f1b01625cf48a7b78d249bb4985c065aa8cd1402ed2ba1b6f908f63dcd84b66425df00000000000000000000",
	}

	if len(fragments) != len(expectedFragments) {
		t.Fatalf("fragment count = %d, want %d", len(fragments), len(expectedFragments))
	}
	for i, expected := range expectedFragments {
		got := hex.EncodeToString(fragments[i])
		if got != expected {
			t.Errorf("fragment[%d] hex mismatch", i)
		}
	}

	// Rejoin
	var combined []byte
	for _, f := range fragments {
		combined = append(combined, f...)
	}
	combined = combined[:len(message)]
	if !bytes.Equal(combined, message) {
		t.Error("rejoin mismatch")
	}
}

func TestChooseFragments(t *testing.T) {
	message := makeMessage("Wolf", 1024)
	checksum := crc32Checksum(message)
	fragLen := fragmentLength(len(message), 100)
	fragments := partition(message, fragLen)
	expectedFragmentIndexes := [][]int{
		{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10},
		{9},
		{2, 5, 6, 8, 9, 10},
		{8},
		{1, 5},
		{1},
		{0, 2, 4, 5, 8, 10},
		{5},
		{2},
		{2},
		{0, 1, 3, 4, 5, 7, 9, 10},
		{0, 1, 2, 3, 5, 6, 8, 9, 10},
		{0, 2, 4, 5, 7, 8, 9, 10},
		{3, 5},
		{4},
		{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10},
		{0, 1, 3, 4, 5, 6, 7, 9, 10},
		{6},
		{5, 6},
		{7},
	}
	for seqNum := 1; seqNum <= 30; seqNum++ {
		indexes := chooseFragments(seqNum, len(fragments), checksum)
		slices.Sort(indexes)
		expected := expectedFragmentIndexes[seqNum-1]
		if len(indexes) != len(expected) {
			t.Errorf("chooseFragments(%d) len = %d, want %d", seqNum, len(indexes), len(expected))
			continue
		}
		for j := range expected {
			if indexes[j] != expected[j] {
				t.Errorf("chooseFragments(%d)[%d] = %d, want %d", seqNum, j, indexes[j], expected[j])
				break
			}
		}
	}
}

func TestXor(t *testing.T) {
	rng := newXoshiro256FromString("Wolf")
	data1 := rng.nextBytes(10)
	if hex.EncodeToString(data1) != "916ec65cf77cadf55cd7" {
		t.Errorf("data1 = %s", hex.EncodeToString(data1))
	}

	data2 := rng.nextBytes(10)
	if hex.EncodeToString(data2) != "f9cda1a1030026ddd42e" {
		t.Errorf("data2 = %s", hex.EncodeToString(data2))
	}

	data3 := make([]byte, len(data1))
	copy(data3, data1)
	xorBytes(data3, data2)
	if hex.EncodeToString(data3) != "68a367fdf47c8b2888f9" {
		t.Errorf("data1^data2 = %s", hex.EncodeToString(data3))
	}

	xorBytes(data3, data1)
	if hex.EncodeToString(data3) != hex.EncodeToString(data2) {
		t.Errorf("(data1^data2)^data1 != data2")
	}
}

func TestFountainEncoder(t *testing.T) {
	message := makeMessage("Wolf", 256)
	encoder, err := newFountainEncoder(message, 30)
	if err != nil {
		t.Fatal(err)
	}

	expectedParts := []string{
		"916ec65cf77cadf55cd7f9cda1a1030026ddd42e905b77adc36e4f2d3c",
		"cba44f7f04f2de44f42d84c374a0e149136f25b01852545961d55f7f7a",
		"8cde6d0e2ec43f3b2dcb644a2209e8c9e34af5c4747984a5e873c9cf5f",
		"965e25ee29039fdf8ca74f1c769fc07eb7ebaec46e0695aea6cbd60b3e",
		"c4bbff1b9ffe8a9e7240129377b9d3711ed38d412fbb4442256f1e6f59",
		"5e0fc57fed451fb0a0101fb76b1fb1e1b88cfdfdaa946294a47de8fff1",
		"73f021c0e6f65b05c0a494e50791270a0050a73ae69b6725505a2ec8a5",
		"791457c9876dd34aadd192a53aa0dc66b556c0c215c7ceb8248b717c22",
		"951e65305b56a3706e3e86eb01c803bbf915d80edcd64d4d0000000000",
		"330f0f33a05eead4f331df229871bee733b50de71afd2e5a79f196de09",
		"3b205ce5e52d8c24a52cffa34c564fa1af3fdffcd349dc4258ee4ee828",
		"dd7bf725ea6c16d531b5f03254783803048ca08b87148daacd1cd7a006",
		"760be7ad1c6187902bbc04f539b9ee5eb8ea6833222edea36031306c01",
		"5bf4031217d2c3254b088fa7553778b5003632f46e21db129416f65b55",
		"73f021c0e6f65b05c0a494e50791270a0050a73ae69b6725505a2ec8a5",
		"b8546ebfe2048541348910267331c643133f828afec9337c318f71b7df",
		"23dedeea74e3a0fb052befabefa13e2f80e4315c9dceed4c8630612e64",
		"d01a8daee769ce34b6b35d3ca0005302724abddae405bdb419c0a6b208",
		"3171c5dc365766eff25ae47c6f10e7de48cfb8474e050e5fe997a6dc24",
		"e055c2433562184fa71b4be94f262e200f01c6f74c284b0dc6fae6673f",
	}

	for i, expected := range expectedParts {
		if encoder.currentSequence != i {
			t.Errorf("currentSequence = %d, want %d", encoder.currentSequence, i)
		}
		part := encoder.nextPart()
		got := hex.EncodeToString(part.data)
		if got != expected {
			t.Errorf("part[%d] data = %s, want %s", i, got, expected)
		}
		if part.sequence != i+1 {
			t.Errorf("part[%d] sequence = %d, want %d", i, part.sequence, i+1)
		}
		if part.sequenceCount != 9 {
			t.Errorf("part[%d] sequenceCount = %d, want 9", i, part.sequenceCount)
		}
		if part.messageLength != 256 {
			t.Errorf("part[%d] messageLength = %d, want 256", i, part.messageLength)
		}
		if part.checksum != 23570951 {
			t.Errorf("part[%d] checksum = %d, want 23570951", i, part.checksum)
		}
	}
}

func TestFountainEncoderCBOR(t *testing.T) {
	message := makeMessage("Wolf", 256)
	encoder, err := newFountainEncoder(message, 30)
	if err != nil {
		t.Fatal(err)
	}

	expectedParts := []string{
		"8501091901001a0167aa07581d916ec65cf77cadf55cd7f9cda1a1030026ddd42e905b77adc36e4f2d3c",
		"8502091901001a0167aa07581dcba44f7f04f2de44f42d84c374a0e149136f25b01852545961d55f7f7a",
		"8503091901001a0167aa07581d8cde6d0e2ec43f3b2dcb644a2209e8c9e34af5c4747984a5e873c9cf5f",
		"8504091901001a0167aa07581d965e25ee29039fdf8ca74f1c769fc07eb7ebaec46e0695aea6cbd60b3e",
		"8505091901001a0167aa07581dc4bbff1b9ffe8a9e7240129377b9d3711ed38d412fbb4442256f1e6f59",
		"8506091901001a0167aa07581d5e0fc57fed451fb0a0101fb76b1fb1e1b88cfdfdaa946294a47de8fff1",
		"8507091901001a0167aa07581d73f021c0e6f65b05c0a494e50791270a0050a73ae69b6725505a2ec8a5",
		"8508091901001a0167aa07581d791457c9876dd34aadd192a53aa0dc66b556c0c215c7ceb8248b717c22",
		"8509091901001a0167aa07581d951e65305b56a3706e3e86eb01c803bbf915d80edcd64d4d0000000000",
		"850a091901001a0167aa07581d330f0f33a05eead4f331df229871bee733b50de71afd2e5a79f196de09",
		"850b091901001a0167aa07581d3b205ce5e52d8c24a52cffa34c564fa1af3fdffcd349dc4258ee4ee828",
		"850c091901001a0167aa07581ddd7bf725ea6c16d531b5f03254783803048ca08b87148daacd1cd7a006",
		"850d091901001a0167aa07581d760be7ad1c6187902bbc04f539b9ee5eb8ea6833222edea36031306c01",
		"850e091901001a0167aa07581d5bf4031217d2c3254b088fa7553778b5003632f46e21db129416f65b55",
		"850f091901001a0167aa07581d73f021c0e6f65b05c0a494e50791270a0050a73ae69b6725505a2ec8a5",
		"8510091901001a0167aa07581db8546ebfe2048541348910267331c643133f828afec9337c318f71b7df",
		"8511091901001a0167aa07581d23dedeea74e3a0fb052befabefa13e2f80e4315c9dceed4c8630612e64",
		"8512091901001a0167aa07581dd01a8daee769ce34b6b35d3ca0005302724abddae405bdb419c0a6b208",
		"8513091901001a0167aa07581d3171c5dc365766eff25ae47c6f10e7de48cfb8474e050e5fe997a6dc24",
		"8514091901001a0167aa07581de055c2433562184fa71b4be94f262e200f01c6f74c284b0dc6fae6673f",
	}

	if encoder.fragmentCount() != 9 {
		t.Errorf("fragmentCount = %d, want 9", encoder.fragmentCount())
	}

	for i, expected := range expectedParts {
		part := encoder.nextPart()
		cbor := part.toCBOR()
		got := hex.EncodeToString(cbor)
		if got != expected {
			t.Errorf("part[%d] cbor = %s, want %s", i, got, expected)
		}
	}
}

func TestFountainDecoder(t *testing.T) {
	message := makeMessage("Wolf", 32767)
	encoder, err := newFountainEncoder(message, 1000)
	if err != nil {
		t.Fatal(err)
	}
	decoder := newFountainDecoder()
	for !decoder.complete() {
		msg, _ := decoder.message()
		if msg != nil {
			t.Error("message should be nil before complete")
		}
		part := encoder.nextPart()
		decoder.receive(part)
	}
	decoded, err := decoder.message()
	if err != nil {
		t.Fatal(err)
	}
	if !bytes.Equal(decoded, message) {
		t.Error("decoded message mismatch")
	}
}

func TestFountainDecoderSkip(t *testing.T) {
	message := makeMessage("Wolf", 32767)
	encoder, err := newFountainEncoder(message, 1000)
	if err != nil {
		t.Fatal(err)
	}
	decoder := newFountainDecoder()
	skip := false
	for !decoder.complete() {
		part := encoder.nextPart()
		if !skip {
			decoder.receive(part)
		}
		skip = !skip
	}
	decoded, err := decoder.message()
	if err != nil {
		t.Fatal(err)
	}
	if !bytes.Equal(decoded, message) {
		t.Error("decoded message mismatch")
	}
}

func TestFountainPartCBOR(t *testing.T) {
	part := &fountainPart{
		sequence:      12,
		sequenceCount: 8,
		messageLength: 100,
		checksum:      0x12345678,
		data:          []byte{1, 5, 3, 3, 5},
	}
	cbor := part.toCBOR()
	part2, err := fountainPartFromCBOR(cbor)
	if err != nil {
		t.Fatal(err)
	}
	cbor2 := part2.toCBOR()
	if hex.EncodeToString(cbor) != hex.EncodeToString(cbor2) {
		t.Error("CBOR roundtrip mismatch")
	}
}

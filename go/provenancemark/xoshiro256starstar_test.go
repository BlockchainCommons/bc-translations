package provenancemark

import (
	"bytes"
	"encoding/hex"
	"testing"
)

func TestXoshiroRNG(t *testing.T) {
	digest := SHA256([]byte("Hello World"))
	rng := Xoshiro256StarStarFromData(digest)
	key := rng.NextBytes(32)
	want, _ := hex.DecodeString("b18b446df414ec00714f19cb0f03e45cd3c3d5d071d2e7483ba8627c65b9926a")
	if !bytes.Equal(key, want) {
		t.Fatalf("NextBytes mismatch: got=%x want=%x", key, want)
	}
}

func TestSaveRNGState(t *testing.T) {
	state := [4]uint64{
		17295166580085024720,
		422929670265678780,
		5577237070365765850,
		7953171132032326923,
	}
	data := Xoshiro256StarStarFromState(state).Data()
	want, _ := hex.DecodeString("d0e72cf15ec604f0bcab28594b8cde05dab04ae79053664d0b9dadc201575f6e")
	if !bytes.Equal(data[:], want) {
		t.Fatalf("Data mismatch: got=%x want=%x", data, want)
	}
	state2 := Xoshiro256StarStarFromData(data).State()
	data2 := Xoshiro256StarStarFromState(state2).Data()
	if data != data2 {
		t.Fatalf("state round-trip mismatch: got=%x want=%x", data2, data)
	}
}

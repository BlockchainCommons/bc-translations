package bcenvelope

import (
	"encoding/hex"
	"fmt"
	"testing"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

func TestKnownValue(t *testing.T) {
	envelope := checkEncoding(t, NewEnvelope(knownvalues.Signed))

	assertActualExpected(t, envelope.String(), ".knownValue(signed)")
	assertActualExpected(t,
		fmt.Sprintf("%s", envelope.Digest()),
		"Digest(d0e39e788c0d8f0343af4588db21d3d51381db454bdf710a9a1891aaa537693c)")
	assertActualExpected(t, envelope.Format(), "'signed'")

	// UR string test omitted: Envelope does not yet implement UREncodable in Go.
	// Expected: "ur:envelope/axgrbdrnem"
}

func TestDate(t *testing.T) {
	date, err := dcbor.DateFromString("2018-01-07")
	if err != nil {
		t.Fatalf("DateFromString failed: %v", err)
	}
	envelope := checkEncoding(t, NewEnvelope(date))
	assertActualExpected(t, envelope.Format(), "2018-01-07")
}

func TestFakeRandomData(t *testing.T) {
	data := bcrand.FakeRandomData(100)
	expected := "7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed518684c556472008a67932f7c682125b50cb72e8216f6906358fdaf28d3545532daee0c5bb5023f50cd8e71ec14901ac746c576c481b893be6656b80622b3a564e59b4e2"
	actual := hex.EncodeToString(data)
	if actual != expected {
		t.Errorf("FakeRandomData mismatch:\nActual:   %s\nExpected: %s", actual, expected)
	}
}

func TestFakeNumbers(t *testing.T) {
	rng := bcrand.NewFakeRandomNumberGenerator()
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
		got := bcrand.NextInClosedRange(rng, -50, 50, 32)
		if got != want {
			t.Errorf("NextInClosedRange #%d = %d, want %d", i, got, want)
		}
	}
}

// Ensure imported packages are used.
var _ = fmt.Sprint
var _ knownvalues.KnownValue

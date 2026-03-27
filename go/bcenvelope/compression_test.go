package bcenvelope

import (
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

const compressionSource = "Lorem ipsum dolor sit amet consectetur adipiscing elit mi nibh ornare proin blandit diam ridiculus, faucibus mus dui eu vehicula nam donec dictumst sed vivamus bibendum aliquet efficitur. Felis imperdiet sodales dictum morbi vivamus augue dis duis aliquet velit ullamcorper porttitor, lobortis dapibus hac purus aliquam natoque iaculis blandit montes nunc pretium."

func TestCompress(t *testing.T) {
	original := NewEnvelope(compressionSource)
	originalLen := len(original.ToCBORData())
	if originalLen != 371 {
		t.Errorf("expected original CBOR data length 371, got %d", originalLen)
	}

	compressed, err := original.Compress()
	if err != nil {
		t.Fatalf("Compress failed: %v", err)
	}
	compressed = checkEncoding(t, compressed)
	compressedLen := len(compressed.ToCBORData())
	// Rust's miniz_oxide DEFLATE produces 283 bytes; Go's compress/flate
	// produces 284 bytes (1 byte DEFLATE stream difference). Both are valid
	// raw DEFLATE streams that decompress to the same original data.
	if compressedLen != 284 {
		t.Errorf("expected compressed CBOR data length 284, got %d", compressedLen)
	}

	if !original.Digest().Equal(compressed.Digest()) {
		t.Error("original and compressed digests should match")
	}

	decompressed, err := compressed.Decompress()
	if err != nil {
		t.Fatalf("Decompress failed: %v", err)
	}
	decompressed = checkEncoding(t, decompressed)
	if !decompressed.Digest().Equal(original.Digest()) {
		t.Error("decompressed and original digests should match")
	}
	if !decompressed.StructuralDigest().Equal(original.StructuralDigest()) {
		t.Error("decompressed and original structural digests should match")
	}
}

func TestCompressSubject(t *testing.T) {
	rng := bcrand.NewFakeRandomNumberGenerator()
	options := &bccomponents.SigningOptions{SchnorrRNG: rng}

	original := NewEnvelope("Alice").
		AddAssertion(knownvalues.Note, compressionSource).
		Wrap().
		AddSignatureOpt(alicePrivateKey().SchnorrSigningPrivateKey(), options, nil)

	if len(original.ToCBORData()) != 458 {
		t.Errorf("expected original CBOR data length 458, got %d", len(original.ToCBORData()))
	}

	s := original.TreeFormat()
	assertActualExpected(t, s, `ec608f27 NODE
    d7183f04 subj WRAPPED
        7f35e345 cont NODE
            13941b48 subj "Alice"
            9fb69539 ASSERTION
                0fcd6a39 pred 'note'
                e343c9b4 obj "Lorem ipsum dolor sit amet consectetur a…"
    0db2ee20 ASSERTION
        d0e39e78 pred 'signed'
        f0d3ce4c obj Signature`)

	compressed, err := original.CompressSubject()
	if err != nil {
		t.Fatalf("CompressSubject failed: %v", err)
	}
	compressed = checkEncoding(t, compressed)

	// Rust miniz_oxide produces 374 bytes; Go's compress/flate may differ slightly
	compressedLen := len(compressed.ToCBORData())
	if compressedLen < 370 || compressedLen > 380 {
		t.Errorf("expected compressed CBOR data length ~374, got %d", compressedLen)
	}

	s = compressed.TreeFormat()
	assertActualExpected(t, s, `ec608f27 NODE
    d7183f04 subj COMPRESSED
    0db2ee20 ASSERTION
        d0e39e78 pred 'signed'
        f0d3ce4c obj Signature`)

	s = compressed.MermaidFormat()
	assertActualExpected(t, s, `%%{ init: { 'theme': 'default', 'flowchart': { 'curve': 'basis' } } }%%
graph LR
0(("NODE<br>ec608f27"))
    0 -- subj --> 1[["COMPRESSED<br>d7183f04"]]
    0 --> 2(["ASSERTION<br>0db2ee20"])
        2 -- pred --> 3[/"'signed'<br>d0e39e78"/]
        2 -- obj --> 4["Signature<br>f0d3ce4c"]
style 0 stroke:red,stroke-width:4px
style 1 stroke:purple,stroke-width:4px
style 2 stroke:green,stroke-width:4px
style 3 stroke:goldenrod,stroke-width:4px
style 4 stroke:teal,stroke-width:4px
linkStyle 0 stroke:red,stroke-width:2px
linkStyle 1 stroke-width:2px
linkStyle 2 stroke:cyan,stroke-width:2px
linkStyle 3 stroke:magenta,stroke-width:2px`)

	decompressed, err := compressed.DecompressSubject()
	if err != nil {
		t.Fatalf("DecompressSubject failed: %v", err)
	}
	decompressed = checkEncoding(t, decompressed)
	if !decompressed.Digest().Equal(original.Digest()) {
		t.Error("decompressed and original digests should match")
	}
	if !decompressed.StructuralDigest().Equal(original.StructuralDigest()) {
		t.Error("decompressed and original structural digests should match")
	}
}

package bcenvelope

import (
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
)

func TestEd25519SignedPlaintext(t *testing.T) {
	RegisterTags()

	aliceEd25519PrivateKey := alicePrivateKey().Ed25519SigningPrivateKey()
	aliceEd25519PublicKey := aliceEd25519PrivateKey.PublicKey()

	// Alice sends a signed plaintext message to Bob.
	envelope := helloEnvelope().
		AddSignature(aliceEd25519PrivateKey)
	envelope = checkEncoding(t, envelope)
	ur := envelope.TaggedCBOR().ToCBORData()

	assertActualExpected(t, envelope.Format(),
		`"Hello." [
    'signed': Signature(Ed25519)
]`)

	// Bob receives the envelope.
	receivedEnvelope, err := EnvelopeFromCBORData(ur)
	if err != nil {
		t.Fatalf("failed to decode: %v", err)
	}
	receivedEnvelope = checkEncoding(t, receivedEnvelope)

	// Bob receives the message, validates Alice's signature, and reads the message.
	verified, err := receivedEnvelope.VerifySignatureFrom(aliceEd25519PublicKey)
	if err != nil {
		t.Fatalf("VerifySignatureFrom failed: %v", err)
	}
	receivedPlaintext, err := ExtractSubject[string](verified)
	if err != nil {
		t.Fatalf("ExtractSubject failed: %v", err)
	}
	if receivedPlaintext != "Hello." {
		t.Errorf("expected %q, got %q", "Hello.", receivedPlaintext)
	}

	// Confirm that it wasn't signed by Carol.
	carolEd25519PublicKey := carolPrivateKey().Ed25519SigningPrivateKey().PublicKey()
	_, err = receivedEnvelope.VerifySignatureFrom(carolEd25519PublicKey)
	if err == nil {
		t.Error("expected error verifying signature from Carol")
	}

	// Confirm that it was signed by Alice OR Carol.
	_, err = receivedEnvelope.VerifySignaturesFromThreshold(
		[]bccomponents.Verifier{aliceEd25519PublicKey, carolEd25519PublicKey},
		1,
	)
	if err != nil {
		t.Fatalf("VerifySignaturesFromThreshold(1) failed: %v", err)
	}

	// Confirm that it was not signed by Alice AND Carol.
	_, err = receivedEnvelope.VerifySignaturesFromThreshold(
		[]bccomponents.Verifier{aliceEd25519PublicKey, carolEd25519PublicKey},
		2,
	)
	if err == nil {
		t.Error("expected error verifying threshold(2)")
	}
}

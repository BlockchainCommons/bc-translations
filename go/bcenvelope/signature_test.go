package bcenvelope

import (
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

func TestSignedPlaintext(t *testing.T) {
	// Alice sends a signed plaintext message to Bob.
	envelope := helloEnvelope().
		AddSignature(alicePrivateKey().PrivateKeys())
	envelope = checkEncoding(t, envelope)
	ur := envelope.TaggedCBOR().ToCBORData()

	assertActualExpected(t, envelope.Format(),
		`"Hello." [
    'signed': Signature
]`)

	// Bob receives the envelope.
	receivedEnvelope, err := EnvelopeFromCBORData(ur)
	if err != nil {
		t.Fatalf("failed to decode: %v", err)
	}
	receivedEnvelope = checkEncoding(t, receivedEnvelope)

	// Bob receives the message, validates Alice's signature, and reads the message.
	verified, err := receivedEnvelope.VerifySignatureFrom(alicePublicKey())
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
	_, err = receivedEnvelope.VerifySignatureFrom(carolPublicKey())
	if err == nil {
		t.Error("expected error verifying signature from Carol")
	}

	// Confirm that it was signed by Alice OR Carol.
	_, err = receivedEnvelope.VerifySignaturesFromThreshold(
		[]bccomponents.Verifier{alicePublicKey(), carolPublicKey()},
		1,
	)
	if err != nil {
		t.Fatalf("VerifySignaturesFromThreshold(1) failed: %v", err)
	}

	// Confirm that it was not signed by Alice AND Carol.
	_, err = receivedEnvelope.VerifySignaturesFromThreshold(
		[]bccomponents.Verifier{alicePublicKey(), carolPublicKey()},
		2,
	)
	if err == nil {
		t.Error("expected error verifying threshold(2)")
	}
}

func TestMultisignedPlaintext(t *testing.T) {
	RegisterTags()

	// Alice and Carol jointly send a signed plaintext message to Bob.
	envelope := helloEnvelope().
		AddSignatures([]bccomponents.Signer{alicePrivateKey().PrivateKeys(), carolPrivateKey().PrivateKeys()})
	envelope = checkEncoding(t, envelope)

	assertActualExpected(t, envelope.Format(),
		`"Hello." [
    'signed': Signature
    'signed': Signature
]`)

	ur := envelope.TaggedCBOR().ToCBORData()

	// Bob receives the envelope and verifies the message was signed by both
	// Alice and Carol.
	received, err := EnvelopeFromCBORData(ur)
	if err != nil {
		t.Fatalf("failed to decode: %v", err)
	}
	received = checkEncoding(t, received)
	verified, err := received.VerifySignaturesFrom(
		[]bccomponents.Verifier{alicePublicKey(), carolPublicKey()},
	)
	if err != nil {
		t.Fatalf("VerifySignaturesFrom failed: %v", err)
	}

	// Bob reads the message.
	receivedPlaintext, err := ExtractSubject[string](verified)
	if err != nil {
		t.Fatalf("ExtractSubject failed: %v", err)
	}
	if receivedPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, receivedPlaintext)
	}
}

func TestSignedWithMetadata(t *testing.T) {
	RegisterTags()

	envelope := helloEnvelope()

	metadata := NewSignatureMetadata().
		WithAssertion(knownvalues.Note, "Alice signed this.")

	envelope = envelope.Wrap().
		AddSignatureOpt(alicePrivateKey().PrivateKeys(), nil, metadata)
	envelope = checkEncoding(t, envelope)

	assertActualExpected(t, envelope.Format(),
		`{
    "Hello."
} [
    'signed': {
        Signature [
            'note': "Alice signed this."
        ]
    } [
        'signed': Signature
    ]
]`)

	ur := envelope.TaggedCBOR().ToCBORData()

	// Bob receives the envelope and verifies the message was signed by Alice.
	received, err := EnvelopeFromCBORData(ur)
	if err != nil {
		t.Fatalf("failed to decode: %v", err)
	}
	received = checkEncoding(t, received)

	unwrapped, metadataEnv, err := received.VerifyReturningMetadata(alicePublicKey())
	if err != nil {
		t.Fatalf("VerifyReturningMetadata failed: %v", err)
	}

	assertActualExpected(t, metadataEnv.Format(),
		`Signature [
    'note': "Alice signed this."
]`)

	noteObj, err := metadataEnv.ObjectForPredicate(knownvalues.Note)
	if err != nil {
		t.Fatalf("ObjectForPredicate failed: %v", err)
	}
	note, err := ExtractSubject[string](noteObj)
	if err != nil {
		t.Fatalf("ExtractSubject failed: %v", err)
	}
	if note != "Alice signed this." {
		t.Errorf("expected %q, got %q", "Alice signed this.", note)
	}

	// Bob reads the message.
	receivedPlaintext, err := ExtractSubject[string](unwrapped)
	if err != nil {
		t.Fatalf("ExtractSubject failed: %v", err)
	}
	if receivedPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, receivedPlaintext)
	}
}

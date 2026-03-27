package bcenvelope

import (
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
)

func TestSSHSignedPlaintext(t *testing.T) {
	RegisterTags()

	aliceSSHPrivateKey, err := alicePrivateKey().SSHSigningPrivateKey(
		bccomponents.SchemeSSHEd25519,
		"alice@example.com",
	)
	if err != nil {
		t.Fatalf("SSHSigningPrivateKey failed: %v", err)
	}
	aliceSSHPublicKey := aliceSSHPrivateKey.PublicKey()

	// Alice sends a signed plaintext message to Bob.
	options := &bccomponents.SigningOptions{
		SSHNamespace: "test",
		SSHHashAlg:   bccomponents.SSHHashSHA256,
	}
	envelope := helloEnvelope().
		AddSignatureOpt(aliceSSHPrivateKey, options, nil)
	envelope = checkEncoding(t, envelope)
	ur := envelope.TaggedCBOR().ToCBORData()

	assertActualExpected(t, envelope.Format(),
		`"Hello." [
    'signed': Signature(SshEd25519)
]`)

	// Bob receives the envelope.
	receivedEnvelope, err := EnvelopeFromCBORData(ur)
	if err != nil {
		t.Fatalf("failed to decode: %v", err)
	}
	receivedEnvelope = checkEncoding(t, receivedEnvelope)

	// Bob receives the message, validates Alice's signature, and reads the message.
	verified, err := receivedEnvelope.VerifySignatureFrom(aliceSSHPublicKey)
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
	carolSSHPrivateKey, err := carolPrivateKey().SSHSigningPrivateKey(
		bccomponents.SchemeSSHEd25519,
		"carol@example.com",
	)
	if err != nil {
		t.Fatalf("Carol SSHSigningPrivateKey failed: %v", err)
	}
	carolSSHPublicKey := carolSSHPrivateKey.PublicKey()
	_, err = receivedEnvelope.VerifySignatureFrom(carolSSHPublicKey)
	if err == nil {
		t.Error("expected error verifying signature from Carol")
	}

	// Confirm that it was signed by Alice OR Carol.
	_, err = receivedEnvelope.VerifySignaturesFromThreshold(
		[]bccomponents.Verifier{aliceSSHPublicKey, carolSSHPublicKey},
		1,
	)
	if err != nil {
		t.Fatalf("VerifySignaturesFromThreshold(1) failed: %v", err)
	}

	// Confirm that it was not signed by Alice AND Carol.
	_, err = receivedEnvelope.VerifySignaturesFromThreshold(
		[]bccomponents.Verifier{aliceSSHPublicKey, carolSSHPublicKey},
		2,
	)
	if err == nil {
		t.Error("expected error verifying threshold(2)")
	}
}

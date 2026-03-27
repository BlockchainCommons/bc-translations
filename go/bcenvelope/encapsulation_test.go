package bcenvelope

import (
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
)

func testEncapsulationScheme(t *testing.T, scheme bccomponents.EncapsulationScheme) {
	t.Helper()

	privateKey, publicKey, err := bccomponents.EncapsulationKeypair(scheme)
	if err != nil {
		t.Fatalf("keypair generation failed for %v: %v", scheme, err)
	}

	envelope := helloEnvelope()
	encryptedEnvelope := envelope.EncryptToRecipient(publicKey)
	encryptedEnvelope = checkEncoding(t, encryptedEnvelope)

	decryptedEnvelope, err := encryptedEnvelope.DecryptToRecipient(privateKey)
	if err != nil {
		t.Fatalf("decrypt failed for %v: %v", scheme, err)
	}
	if !envelope.StructuralDigest().Equal(decryptedEnvelope.StructuralDigest()) {
		t.Fatalf("structural digest mismatch for %v", scheme)
	}
}

func TestEncapsulation(t *testing.T) {
	bccomponents.RegisterTags()

	testEncapsulationScheme(t, bccomponents.EncapsulationX25519)
	testEncapsulationScheme(t, bccomponents.EncapsulationMLKEM512)
	testEncapsulationScheme(t, bccomponents.EncapsulationMLKEM768)
	testEncapsulationScheme(t, bccomponents.EncapsulationMLKEM1024)
}

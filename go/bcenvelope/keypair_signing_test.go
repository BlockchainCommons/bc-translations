package bcenvelope

import (
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
)

func testSigningScheme(t *testing.T, scheme bccomponents.SignatureScheme, options *bccomponents.SigningOptions) {
	t.Helper()
	privateKey, publicKey, err := bccomponents.SigningKeypair(scheme)
	if err != nil {
		t.Fatalf("SigningKeypair(%s) failed: %v", scheme, err)
	}
	envelope := helloEnvelope().SignOpt(privateKey, options)
	envelope = checkEncoding(t, envelope)
	_, err = envelope.Verify(publicKey)
	if err != nil {
		t.Fatalf("Verify(%s) failed: %v", scheme, err)
	}
}

func TestKeypairSigning(t *testing.T) {
	RegisterTags()

	testSigningScheme(t, bccomponents.SchemeSchnorr, nil)
	testSigningScheme(t, bccomponents.SchemeECDSA, nil)
	testSigningScheme(t, bccomponents.SchemeEd25519, nil)
	testSigningScheme(t, bccomponents.SchemeMLDSA44, nil)
	testSigningScheme(t, bccomponents.SchemeMLDSA65, nil)
	testSigningScheme(t, bccomponents.SchemeMLDSA87, nil)
}

func TestKeypairSigningSSH(t *testing.T) {
	RegisterTags()

	options := &bccomponents.SigningOptions{
		SSHNamespace: "test",
		SSHHashAlg:   bccomponents.SSHHashSHA512,
	}
	testSigningScheme(t, bccomponents.SchemeSSHEd25519, options)
}

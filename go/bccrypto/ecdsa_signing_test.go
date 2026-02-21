package bccrypto

import (
	"testing"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

var ecdsaMessage = []byte("Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it.")

func TestECDSASigning(t *testing.T) {
	rng := bcrand.NewFakeRandomNumberGenerator()
	privateKey := ECDSANewPrivateKeyUsing(rng)
	publicKey := ECDSAPublicKeyFromPrivateKey(privateKey)
	signature := ECDSASign(privateKey, ecdsaMessage)

	expected := must64("e75702ed8f645ce7fe510507b2403029e461ef4570d12aa440e4f81385546a13740b7d16878ff0b46b1cbe08bc218ccb0b00937b61c4707de2ca6148508e51fb")
	if signature != expected {
		t.Fatalf("signature = %x, want %x", signature, expected)
	}
	if !ECDSAVerify(publicKey, signature, ecdsaMessage) {
		t.Fatalf("ECDSAVerify returned false")
	}
}

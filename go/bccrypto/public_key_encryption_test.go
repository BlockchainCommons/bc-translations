package bccrypto

import (
	"testing"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

func TestX25519Keys(t *testing.T) {
	rng := bcrand.MakeFakeRandomNumberGenerator()
	privateKey := X25519NewPrivateKeyUsing(rng)
	if privateKey != must32("7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed") {
		t.Fatalf("privateKey = %x", privateKey)
	}

	publicKey := X25519PublicKeyFromPrivateKey(privateKey)
	if publicKey != must32("f1bd7a7e118ea461eba95126a3efef543ebb78439d1574bedcbe7d89174cf025") {
		t.Fatalf("publicKey = %x", publicKey)
	}

	derivedAgreement := DeriveAgreementPrivateKey([]byte("password"))
	if derivedAgreement != must32("7b19769132648ff43ae60cbaa696d5be3f6d53e6645db72e2d37516f0729619f") {
		t.Fatalf("derived agreement = %x", derivedAgreement)
	}

	derivedSigning := DeriveSigningPrivateKey([]byte("password"))
	if derivedSigning != must32("05cc550daa75058e613e606d9898fedf029e395911c43273a208b7e0e88e271b") {
		t.Fatalf("derived signing = %x", derivedSigning)
	}
}

func TestKeyAgreement(t *testing.T) {
	rng := bcrand.MakeFakeRandomNumberGenerator()

	alicePrivate := X25519NewPrivateKeyUsing(rng)
	alicePublic := X25519PublicKeyFromPrivateKey(alicePrivate)
	bobPrivate := X25519NewPrivateKeyUsing(rng)
	bobPublic := X25519PublicKeyFromPrivateKey(bobPrivate)

	aliceShared := X25519SharedKey(alicePrivate, bobPublic)
	bobShared := X25519SharedKey(bobPrivate, alicePublic)

	if aliceShared != bobShared {
		t.Fatalf("shared keys differ\n alice=%x\n bob=%x", aliceShared, bobShared)
	}
	if aliceShared != must32("1e9040d1ff45df4bfca7ef2b4dd2b11101b40d91bf5bf83f8c83d53f0fbb6c23") {
		t.Fatalf("shared key = %x", aliceShared)
	}
}

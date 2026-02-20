package bccrypto

import (
	"bytes"
	"testing"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
)

func TestECDSAKeys(t *testing.T) {
	rng := bcrand.MakeFakeRandomNumberGenerator()
	privateKey := ECDSANewPrivateKeyUsing(rng)
	if privateKey != must32("7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed") {
		t.Fatalf("privateKey = %x", privateKey)
	}

	publicKey := ECDSAPublicKeyFromPrivateKey(privateKey)
	if publicKey != must33("0271b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b") {
		t.Fatalf("publicKey = %x", publicKey)
	}

	decompressed := ECDSADecompressPublicKey(publicKey)
	if decompressed != must65("0471b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b72325f1f3bb69a44d3f1cb6d1fd488220dd502f49c0b1a46cb91ce3718d8334a") {
		t.Fatalf("decompressed = %x", decompressed)
	}

	compressed := ECDSACompressPublicKey(decompressed)
	if !bytes.Equal(compressed[:], publicKey[:]) {
		t.Fatalf("compressed = %x, want %x", compressed, publicKey)
	}

	xOnly := SchnorrPublicKeyFromPrivateKey(privateKey)
	if xOnly != must32("71b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b") {
		t.Fatalf("xOnly = %x", xOnly)
	}

	derived := ECDSADerivePrivateKey([]byte("password"))
	if !bytes.Equal(derived, mustLen("05cc550daa75058e613e606d9898fedf029e395911c43273a208b7e0e88e271b", 32)) {
		t.Fatalf("derived = %x", derived)
	}
}

package bccomponents

import (
	"bytes"
	"encoding/hex"
	"testing"

	bccrypto "github.com/nickel-blockchaincommons/bccrypto-go"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
)

func TestX25519Keys(t *testing.T) {
	RegisterTags()
	rng := bcrand.NewFakeRandomNumberGenerator()

	// Generate private key
	privateKey := NewX25519PrivateKeyUsing(rng)
	privateKeyUR := X25519PrivateKeyToURString(privateKey)
	expectedPrivUR := "ur:agreement-private-key/hdcxkbrehkrkrsjztodseytknecfgewmgdmwfsvdvysbpmghuozsprknfwkpnehydlweynwkrtct"
	if privateKeyUR != expectedPrivUR {
		t.Errorf("private key UR mismatch\ngot:  %s\nwant: %s", privateKeyUR, expectedPrivUR)
	}

	// Decode back and compare
	decodedPriv, err := X25519PrivateKeyFromURString(privateKeyUR)
	if err != nil {
		t.Fatalf("failed to decode private key UR: %v", err)
	}
	if !decodedPriv.Equal(privateKey) {
		t.Error("decoded private key does not match original")
	}

	// Generate and test public key
	publicKey := privateKey.PublicKey()
	publicKeyUR := X25519PublicKeyToURString(publicKey)
	expectedPubUR := "ur:agreement-public-key/hdcxwnryknkbbymnoxhswmptgydsotwswsghfmrkksfxntbzjyrnuornkildchgswtdahehpwkrl"
	if publicKeyUR != expectedPubUR {
		t.Errorf("public key UR mismatch\ngot:  %s\nwant: %s", publicKeyUR, expectedPubUR)
	}

	// Decode back and compare
	decodedPub, err := X25519PublicKeyFromURString(publicKeyUR)
	if err != nil {
		t.Fatalf("failed to decode public key UR: %v", err)
	}
	if !decodedPub.Equal(publicKey) {
		t.Error("decoded public key does not match original")
	}

	// Test derived key
	derivedKey := DeriveX25519PrivateKey([]byte("password"))
	derivedKeyUR := X25519PrivateKeyToURString(derivedKey)
	expectedDerivedUR := "ur:agreement-private-key/hdcxkgcfkomeeyiemywkftvabnrdolmttlrnfhjnguvaiehlrldmdpemgyjlatdthsnecytdoxat"
	if derivedKeyUR != expectedDerivedUR {
		t.Errorf("derived key UR mismatch\ngot:  %s\nwant: %s", derivedKeyUR, expectedDerivedUR)
	}
}

func TestAgreement(t *testing.T) {
	rng := bcrand.NewFakeRandomNumberGenerator()

	alicePriv := NewX25519PrivateKeyUsing(rng)
	alicePub := alicePriv.PublicKey()

	bobPriv := NewX25519PrivateKeyUsing(rng)
	bobPub := bobPriv.PublicKey()

	aliceShared := alicePriv.SharedKeyWith(bobPub)
	bobShared := bobPriv.SharedKeyWith(alicePub)

	if !bytes.Equal(aliceShared.Bytes(), bobShared.Bytes()) {
		t.Errorf("shared keys do not match\nalice: %x\nbob:   %x", aliceShared.Bytes(), bobShared.Bytes())
	}
}

func TestECDSASigningKeys(t *testing.T) {
	RegisterTags()
	rng := bcrand.NewFakeRandomNumberGenerator()

	// Schnorr private key
	schnorrPriv := NewSigningPrivateKeySchnorr(NewECPrivateKeyUsing(rng))
	schnorrPrivUR := bcur.ToURString(schnorrPriv)
	expectedSchnorrPrivUR := "ur:signing-private-key/hdcxkbrehkrkrsjztodseytknecfgewmgdmwfsvdvysbpmghuozsprknfwkpnehydlweynwkrtct"
	if schnorrPrivUR != expectedSchnorrPrivUR {
		t.Errorf("schnorr private key UR mismatch\ngot:  %s\nwant: %s", schnorrPrivUR, expectedSchnorrPrivUR)
	}

	// Decode Schnorr private key back
	decodedSchnorrPriv, err := bcur.DecodeURString(schnorrPrivUR, SigningPrivateKeyCBORTags(), DecodeSigningPrivateKey)
	if err != nil {
		t.Fatalf("failed to decode schnorr private key UR: %v", err)
	}
	if !decodedSchnorrPriv.IsSchnorr() {
		t.Error("decoded key is not Schnorr")
	}

	// ECDSA private key and public key
	ecdsaPriv := NewSigningPrivateKeyECDSA(NewECPrivateKeyUsing(rng))
	ecdsaPub := ecdsaPriv.PublicKey()
	ecdsaPubUR := SigningPublicKeyToURString(ecdsaPub)
	expectedECDSAPubUR := "ur:signing-public-key/lfadhdclaxbzutckgevlpkmdfnuoemlnvsgllokicfdekesswnfdtibkylrskomwgubaahyntaktbksbdt"
	if ecdsaPubUR != expectedECDSAPubUR {
		t.Errorf("ECDSA public key UR mismatch\ngot:  %s\nwant: %s", ecdsaPubUR, expectedECDSAPubUR)
	}

	// Decode ECDSA public key back
	decodedECDSAPub, err := SigningPublicKeyFromURString(ecdsaPubUR)
	if err != nil {
		t.Fatalf("failed to decode ECDSA public key UR: %v", err)
	}
	if decodedECDSAPub.Scheme() != SchemeECDSA {
		t.Errorf("decoded ECDSA public key scheme mismatch: got %v, want ECDSA", decodedECDSAPub.Scheme())
	}

	// Schnorr public key
	schnorrPub := schnorrPriv.PublicKey()
	schnorrPubUR := SigningPublicKeyToURString(schnorrPub)
	expectedSchnorrPubUR := "ur:signing-public-key/hdcxjsrhdnidbgosndmobzwntdglzonnidmwoyrnuomdrpsptkcskerhfljssgaoidjewyjymhcp"
	if schnorrPubUR != expectedSchnorrPubUR {
		t.Errorf("Schnorr public key UR mismatch\ngot:  %s\nwant: %s", schnorrPubUR, expectedSchnorrPubUR)
	}

	// Decode Schnorr public key back
	decodedSchnorrPub, err := SigningPublicKeyFromURString(schnorrPubUR)
	if err != nil {
		t.Fatalf("failed to decode Schnorr public key UR: %v", err)
	}
	if decodedSchnorrPub.Scheme() != SchemeSchnorr {
		t.Errorf("decoded Schnorr public key scheme mismatch: got %v, want Schnorr", decodedSchnorrPub.Scheme())
	}

	// Derived private key
	derivedPriv := NewSigningPrivateKeySchnorr(DeriveECPrivateKey([]byte("password")))
	derivedPrivUR := bcur.ToURString(derivedPriv)
	expectedDerivedPrivUR := "ur:signing-private-key/hdcxahsfgobtpkkpahmnhsfmhnjnmkmkzeuraonneshkbysseyjkoeayrlvtvsmndicwkkvattfs"
	if derivedPrivUR != expectedDerivedPrivUR {
		t.Errorf("derived signing private key UR mismatch\ngot:  %s\nwant: %s", derivedPrivUR, expectedDerivedPrivUR)
	}
}

func TestECDSASigning(t *testing.T) {
	rng := bcrand.NewFakeRandomNumberGenerator()
	privateKeyData := bccrypto.ECDSANewPrivateKeyUsing(rng)
	message := []byte("Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it.")

	// ECDSA signature
	ecdsaPubData := bccrypto.ECDSAPublicKeyFromPrivateKey(privateKeyData)
	ecdsaSigData := bccrypto.ECDSASign(privateKeyData, message)
	expectedECDSAHex := "e75702ed8f645ce7fe510507b2403029e461ef4570d12aa440e4f81385546a13740b7d16878ff0b46b1cbe08bc218ccb0b00937b61c4707de2ca6148508e51fb"
	expectedECDSA, err := hex.DecodeString(expectedECDSAHex)
	if err != nil {
		t.Fatalf("failed to decode expected ECDSA hex: %v", err)
	}
	if !bytes.Equal(ecdsaSigData[:], expectedECDSA) {
		t.Errorf("ECDSA signature mismatch\ngot:  %x\nwant: %s", ecdsaSigData, expectedECDSAHex)
	}
	if !bccrypto.ECDSAVerify(ecdsaPubData, ecdsaSigData, message) {
		t.Error("ECDSA signature verification failed")
	}

	// Schnorr signature
	schnorrPubData := bccrypto.SchnorrPublicKeyFromPrivateKey(privateKeyData)
	schnorrSigData := bccrypto.SchnorrSignUsing(privateKeyData, message, rng)
	expectedSchnorrHex := "df3e33900f0b94e23b6f8685f620ed92705ebfcf885ccb321620acb9927bce1e2218dcfba7cb9c3bba11611446f38774a564f265917899194e82945c8b60a996"
	expectedSchnorr, err := hex.DecodeString(expectedSchnorrHex)
	if err != nil {
		t.Fatalf("failed to decode expected Schnorr hex: %v", err)
	}
	if !bytes.Equal(schnorrSigData[:], expectedSchnorr) {
		t.Errorf("Schnorr signature mismatch\ngot:  %x\nwant: %s", schnorrSigData, expectedSchnorrHex)
	}
	if !bccrypto.SchnorrVerify(schnorrPubData, schnorrSigData, message) {
		t.Error("Schnorr signature verification failed")
	}
}

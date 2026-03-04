package bccomponents

import (
	"bytes"
	"testing"

	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

func TestSymmetricEncryption(t *testing.T) {
	RegisterTags()

	key := NewSymmetricKey()
	plaintext := []byte("Hello, World! This is a test of symmetric encryption.")

	// Encrypt without AAD
	encrypted := key.Encrypt(plaintext, nil, nil)

	// Decrypt
	decrypted, err := key.Decrypt(&encrypted)
	if err != nil {
		t.Fatalf("Decrypt failed: %v", err)
	}
	if !bytes.Equal(decrypted, plaintext) {
		t.Errorf("decrypted text does not match original\ngot:  %x\nwant: %x", decrypted, plaintext)
	}

	// Encrypt with AAD
	aad := []byte("additional authenticated data")
	encryptedWithAAD := key.Encrypt(plaintext, aad, nil)

	decryptedWithAAD, err := key.Decrypt(&encryptedWithAAD)
	if err != nil {
		t.Fatalf("Decrypt with AAD failed: %v", err)
	}
	if !bytes.Equal(decryptedWithAAD, plaintext) {
		t.Errorf("decrypted text with AAD does not match original")
	}

	// Decrypt with wrong key should fail
	wrongKey := NewSymmetricKey()
	_, err = wrongKey.Decrypt(&encrypted)
	if err == nil {
		t.Error("Decrypt with wrong key should have failed")
	}

	// Encrypt with explicit nonce for determinism
	nonce := NewNonce()
	encrypted1 := key.Encrypt(plaintext, nil, &nonce)
	encrypted2 := key.Encrypt(plaintext, nil, &nonce)
	if !bytes.Equal(encrypted1.Ciphertext(), encrypted2.Ciphertext()) {
		t.Error("encrypting same plaintext with same nonce should produce same ciphertext")
	}
}

func TestEncryptedMessageCBOR(t *testing.T) {
	RegisterTags()

	key := NewSymmetricKey()
	plaintext := []byte("test message for CBOR roundtrip")
	encrypted := key.Encrypt(plaintext, nil, nil)

	// Encode to CBOR
	cborData := encrypted.TaggedCBOR().ToCBORData()

	// Decode from CBOR
	cborValue, err := dcbor.TryFromData(cborData)
	if err != nil {
		t.Fatalf("TryFromData failed: %v", err)
	}
	decoded, err := DecodeTaggedEncryptedMessage(cborValue)
	if err != nil {
		t.Fatalf("DecodeTaggedEncryptedMessage failed: %v", err)
	}

	// Verify structural equality
	if !encrypted.Equal(decoded) {
		t.Error("decoded EncryptedMessage does not match original")
	}

	// Verify decryption of the decoded message still works
	decrypted, err := key.Decrypt(decoded)
	if err != nil {
		t.Fatalf("Decrypt of decoded message failed: %v", err)
	}
	if !bytes.Equal(decrypted, plaintext) {
		t.Errorf("decrypted text from decoded message does not match original")
	}
}

func TestEd25519Signing(t *testing.T) {
	rng := bcrand.NewFakeRandomNumberGenerator()
	privKey := NewEd25519PrivateKeyUsing(rng)
	pubKey := privKey.PublicKey()

	message := []byte("test message")
	sig := privKey.Sign(message)
	if !pubKey.Verify(sig, message) {
		t.Fatal("Ed25519 signature verification failed")
	}

	// Verify with wrong message should fail
	wrongMessage := []byte("wrong message")
	if pubKey.Verify(sig, wrongMessage) {
		t.Error("Ed25519 signature should not verify with wrong message")
	}

	// Test via SigningPrivateKey/SigningPublicKey wrappers
	signingPriv := NewSigningPrivateKeyEd25519(privKey)
	signingPub := signingPriv.PublicKey()

	wrappedSig, err := signingPriv.Sign(message)
	if err != nil {
		t.Fatalf("SigningPrivateKey.Sign failed: %v", err)
	}
	if !signingPub.Verify(wrappedSig, message) {
		t.Error("SigningPublicKey.Verify failed for Ed25519 wrapped signature")
	}
	if wrappedSig.Scheme() != SchemeEd25519 {
		t.Errorf("expected Ed25519 scheme, got %v", wrappedSig.Scheme())
	}
}

func TestMLDSASignVerify(t *testing.T) {
	levels := []struct {
		name   string
		level  MLDSA
		scheme SignatureScheme
	}{
		{"MLDSA44", MLDSA44, SchemeMLDSA44},
		{"MLDSA65", MLDSA65, SchemeMLDSA65},
		{"MLDSA87", MLDSA87, SchemeMLDSA87},
	}

	message := []byte("test message for ML-DSA signing")

	for _, tc := range levels {
		t.Run(tc.name, func(t *testing.T) {
			priv, pub, err := tc.level.Keypair()
			if err != nil {
				t.Fatalf("Keypair() failed: %v", err)
			}

			sig, err := priv.Sign(message)
			if err != nil {
				t.Fatalf("Sign() failed: %v", err)
			}

			if !pub.Verify(sig, message) {
				t.Fatal("Verify() failed for valid signature")
			}

			// Wrong message should not verify
			if pub.Verify(sig, []byte("wrong message")) {
				t.Error("Verify() should fail with wrong message")
			}

			// Test via SigningKeypair wrapper
			sigPriv, sigPub, err := SigningKeypair(tc.scheme)
			if err != nil {
				t.Fatalf("SigningKeypair(%s) failed: %v", tc.name, err)
			}

			wrappedSig, err := sigPriv.Sign(message)
			if err != nil {
				t.Fatalf("SigningPrivateKey.Sign(%s) failed: %v", tc.name, err)
			}
			if !sigPub.Verify(wrappedSig, message) {
				t.Errorf("SigningPublicKey.Verify(%s) failed", tc.name)
			}
			if wrappedSig.Scheme() != tc.scheme {
				t.Errorf("expected scheme %v, got %v", tc.scheme, wrappedSig.Scheme())
			}
		})
	}
}

func TestMLKEMEncapsulation(t *testing.T) {
	levels := []struct {
		name   string
		level  MLKEM
		scheme EncapsulationScheme
	}{
		{"MLKEM512", MLKEM512, EncapsulationMLKEM512},
		{"MLKEM768", MLKEM768, EncapsulationMLKEM768},
		{"MLKEM1024", MLKEM1024, EncapsulationMLKEM1024},
	}

	for _, tc := range levels {
		t.Run(tc.name, func(t *testing.T) {
			priv, pub, err := tc.level.Keypair()
			if err != nil {
				t.Fatalf("Keypair() failed: %v", err)
			}

			// Encapsulate a shared secret
			ss, ct, err := pub.EncapsulateNewSharedSecret()
			if err != nil {
				t.Fatalf("EncapsulateNewSharedSecret() failed: %v", err)
			}

			// Decapsulate
			decapSS, err := priv.DecapsulateSharedSecret(ct)
			if err != nil {
				t.Fatalf("DecapsulateSharedSecret() failed: %v", err)
			}

			// Shared secrets must match
			if !bytes.Equal(ss.Bytes(), decapSS.Bytes()) {
				t.Errorf("shared secrets do not match\nencap: %x\ndecap: %x", ss.Bytes(), decapSS.Bytes())
			}

			// Test via EncapsulationKeypair wrapper
			encapPriv, encapPub, err := EncapsulationKeypair(tc.scheme)
			if err != nil {
				t.Fatalf("EncapsulationKeypair(%s) failed: %v", tc.name, err)
			}

			ss2, ct2, err := encapPub.EncapsulateNewSharedSecret()
			if err != nil {
				t.Fatalf("EncapsulationPublicKey.EncapsulateNewSharedSecret() failed: %v", err)
			}

			decapSS2, err := encapPriv.DecapsulateSharedSecret(ct2)
			if err != nil {
				t.Fatalf("EncapsulationPrivateKey.DecapsulateSharedSecret() failed: %v", err)
			}

			if !bytes.Equal(ss2.Bytes(), decapSS2.Bytes()) {
				t.Errorf("wrapped shared secrets do not match")
			}
		})
	}
}

func TestSealedMessage(t *testing.T) {
	RegisterTags()

	// Test X25519 sealed message
	t.Run("X25519", func(t *testing.T) {
		rng := bcrand.NewFakeRandomNumberGenerator()
		senderPriv := NewX25519PrivateKeyUsing(rng)
		_ = senderPriv // sender identity not needed for sealed message

		receiverPriv := NewX25519PrivateKeyUsing(rng)
		receiverPub := receiverPriv.PublicKey()

		plaintext := []byte("secret message for the receiver")

		encapPub := EncapsulationPublicKeyFromX25519(receiverPub)
		sealed, err := NewSealedMessage(plaintext, encapPub)
		if err != nil {
			t.Fatalf("NewSealedMessage failed: %v", err)
		}

		if sealed.EncapsulationScheme() != EncapsulationX25519 {
			t.Errorf("expected X25519 scheme, got %v", sealed.EncapsulationScheme())
		}

		encapPriv := EncapsulationPrivateKeyFromX25519(receiverPriv)
		decrypted, err := sealed.Decrypt(encapPriv)
		if err != nil {
			t.Fatalf("Decrypt failed: %v", err)
		}

		if !bytes.Equal(decrypted, plaintext) {
			t.Errorf("decrypted text does not match original\ngot:  %s\nwant: %s", decrypted, plaintext)
		}

		// CBOR roundtrip
		cborData := sealed.TaggedCBOR().ToCBORData()
		cborValue, err := dcbor.TryFromData(cborData)
		if err != nil {
			t.Fatalf("TryFromData failed: %v", err)
		}
		decoded, err := DecodeTaggedSealedMessage(cborValue)
		if err != nil {
			t.Fatalf("DecodeTaggedSealedMessage failed: %v", err)
		}

		// Decrypt the decoded sealed message
		decrypted2, err := decoded.Decrypt(encapPriv)
		if err != nil {
			t.Fatalf("Decrypt of decoded sealed message failed: %v", err)
		}
		if !bytes.Equal(decrypted2, plaintext) {
			t.Error("decrypted text from decoded sealed message does not match")
		}
	})

	// Test MLKEM sealed message
	t.Run("MLKEM768", func(t *testing.T) {
		priv, pub, err := MLKEM768.Keypair()
		if err != nil {
			t.Fatalf("MLKEM768.Keypair() failed: %v", err)
		}

		plaintext := []byte("post-quantum sealed message")

		encapPub := EncapsulationPublicKeyFromMLKEM(&pub)
		sealed, err := NewSealedMessage(plaintext, encapPub)
		if err != nil {
			t.Fatalf("NewSealedMessage failed: %v", err)
		}

		encapPriv := EncapsulationPrivateKeyFromMLKEM(&priv)
		decrypted, err := sealed.Decrypt(encapPriv)
		if err != nil {
			t.Fatalf("Decrypt failed: %v", err)
		}

		if !bytes.Equal(decrypted, plaintext) {
			t.Errorf("decrypted text does not match original")
		}
	})
}

func TestDigest(t *testing.T) {
	RegisterTags()

	data := []byte("test data for digest")

	// Create digest from data
	digest := DigestFromImage(data)

	// Validate
	if !digest.Validate(data) {
		t.Error("digest should validate against original data")
	}
	if digest.Validate([]byte("different data")) {
		t.Error("digest should not validate against different data")
	}

	// Hex representation
	hexStr := digest.Hex()
	if len(hexStr) != 64 {
		t.Errorf("expected 64-char hex string, got %d chars", len(hexStr))
	}

	// ShortDescription
	shortDesc := digest.ShortDescription()
	if len(shortDesc) != 8 {
		t.Errorf("expected 8-char short description, got %d chars", len(shortDesc))
	}

	// DigestFromHex roundtrip
	reconstructed := DigestFromHex(hexStr)
	if !digest.Equal(reconstructed) {
		t.Error("DigestFromHex roundtrip failed")
	}

	// CBOR roundtrip
	cborData := digest.TaggedCBOR().ToCBORData()
	cborValue, err := dcbor.TryFromData(cborData)
	if err != nil {
		t.Fatalf("TryFromData failed: %v", err)
	}
	decoded, err := DecodeTaggedDigest(cborValue)
	if err != nil {
		t.Fatalf("DecodeTaggedDigest failed: %v", err)
	}
	if !digest.Equal(decoded) {
		t.Error("CBOR roundtrip produced different digest")
	}

	// DigestFromImageParts
	part1 := []byte("test data ")
	part2 := []byte("for digest")
	partsDigest := DigestFromImageParts([][]byte{part1, part2})
	if !digest.Equal(partsDigest) {
		t.Error("DigestFromImageParts should match DigestFromImage of concatenated data")
	}

	// DigestFromDigests
	d1 := DigestFromImage([]byte("a"))
	d2 := DigestFromImage([]byte("b"))
	combinedDigest := DigestFromDigests([]Digest{d1, d2})
	if combinedDigest.Equal(d1) || combinedDigest.Equal(d2) {
		t.Error("DigestFromDigests should produce a different digest")
	}
}

func TestNonce(t *testing.T) {
	RegisterTags()

	// Create a new random nonce
	nonce := NewNonce()
	if len(nonce.Bytes()) != NonceSize {
		t.Errorf("expected nonce size %d, got %d", NonceSize, len(nonce.Bytes()))
	}

	// Nonce from fixed data
	var fixedData [NonceSize]byte
	for i := range fixedData {
		fixedData[i] = byte(i)
	}
	nonce2 := NonceFromData(fixedData)
	if nonce2.Data() != fixedData {
		t.Error("NonceFromData did not preserve data")
	}

	// Hex roundtrip
	hexStr := nonce2.Hex()
	nonce3 := NonceFromHex(hexStr)
	if !nonce2.Equal(nonce3) {
		t.Error("Hex roundtrip failed for Nonce")
	}

	// CBOR roundtrip
	cborData := nonce2.TaggedCBOR().ToCBORData()
	cborValue, err := dcbor.TryFromData(cborData)
	if err != nil {
		t.Fatalf("TryFromData failed: %v", err)
	}
	decoded, err := DecodeTaggedNonce(cborValue)
	if err != nil {
		t.Fatalf("DecodeTaggedNonce failed: %v", err)
	}
	if !nonce2.Equal(decoded) {
		t.Error("CBOR roundtrip produced different nonce")
	}

	// NonceFromDataRef validation
	_, err = NonceFromDataRef([]byte{1, 2, 3}) // too short
	if err == nil {
		t.Error("NonceFromDataRef should fail for wrong-sized data")
	}
}

func TestSeed(t *testing.T) {
	RegisterTags()

	// Create seed from data
	data := make([]byte, 16)
	for i := range data {
		data[i] = byte(i + 1)
	}
	seed, err := NewSeedOpt(data, "test seed", "a test note", nil)
	if err != nil {
		t.Fatalf("NewSeedOpt failed: %v", err)
	}

	if seed.Name() != "test seed" {
		t.Errorf("expected name 'test seed', got %q", seed.Name())
	}
	if seed.Note() != "a test note" {
		t.Errorf("expected note 'a test note', got %q", seed.Note())
	}
	if !bytes.Equal(seed.Bytes(), data) {
		t.Error("seed data does not match input")
	}

	// CBOR roundtrip
	cborData := seed.TaggedCBOR().ToCBORData()
	cborValue, err := dcbor.TryFromData(cborData)
	if err != nil {
		t.Fatalf("TryFromData failed: %v", err)
	}
	decoded, err := DecodeTaggedSeed(cborValue)
	if err != nil {
		t.Fatalf("DecodeTaggedSeed failed: %v", err)
	}
	if !seed.Equal(decoded) {
		t.Error("CBOR roundtrip produced different seed")
	}

	// Minimum length validation
	_, err = NewSeedOpt([]byte{1, 2, 3}, "", "", nil)
	if err == nil {
		t.Error("NewSeedOpt should fail for data shorter than MinSeedLength")
	}

	// SetName/SetNote
	seed.SetName("new name")
	seed.SetNote("new note")
	if seed.Name() != "new name" {
		t.Errorf("SetName did not work, got %q", seed.Name())
	}
	if seed.Note() != "new note" {
		t.Errorf("SetNote did not work, got %q", seed.Note())
	}
}

func TestCompressed(t *testing.T) {
	RegisterTags()

	// Create compressible data (repeated pattern compresses well)
	original := bytes.Repeat([]byte("Hello, World! "), 100)

	compressed := CompressedFromData(original, nil)

	// Verify compression actually reduced the size
	if compressed.CompressedSize() >= len(original) {
		t.Errorf("compressed size (%d) should be less than original (%d)", compressed.CompressedSize(), len(original))
	}

	ratio := compressed.CompressionRatio()
	if ratio >= 1.0 {
		t.Errorf("compression ratio should be < 1.0, got %f", ratio)
	}

	// Decompress and verify
	decompressed, err := compressed.Decompress()
	if err != nil {
		t.Fatalf("Decompress failed: %v", err)
	}
	if !bytes.Equal(decompressed, original) {
		t.Error("decompressed data does not match original")
	}

	// CBOR roundtrip
	cborData := compressed.TaggedCBOR().ToCBORData()
	cborValue, err := dcbor.TryFromData(cborData)
	if err != nil {
		t.Fatalf("TryFromData failed: %v", err)
	}
	decoded, err := DecodeTaggedCompressed(cborValue)
	if err != nil {
		t.Fatalf("DecodeTaggedCompressed failed: %v", err)
	}
	if !compressed.Equal(decoded) {
		t.Error("CBOR roundtrip produced different Compressed")
	}

	// Decompress the decoded value too
	decompressed2, err := decoded.Decompress()
	if err != nil {
		t.Fatalf("Decompress of decoded value failed: %v", err)
	}
	if !bytes.Equal(decompressed2, original) {
		t.Error("decoded decompressed data does not match original")
	}

	// Test with digest
	digest := DigestFromImage(original)
	compressedWithDigest := CompressedFromData(original, &digest)
	if !compressedWithDigest.HasDigest() {
		t.Error("expected HasDigest() to be true")
	}
	if !compressedWithDigest.Digest().Equal(digest) {
		t.Error("digest does not match")
	}

	// Test without digest
	if compressed.HasDigest() {
		t.Error("expected HasDigest() to be false for compressed without digest")
	}

	// Test incompressible data (random bytes are hard to compress)
	rng := bcrand.NewFakeRandomNumberGenerator()
	randomData := rng.RandomData(64)
	compressedRandom := CompressedFromData(randomData, nil)
	decompressedRandom, err := compressedRandom.Decompress()
	if err != nil {
		t.Fatalf("Decompress of random data failed: %v", err)
	}
	if !bytes.Equal(decompressedRandom, randomData) {
		t.Error("decompressed random data does not match original")
	}
}

func TestKeypairGeneration(t *testing.T) {
	RegisterTags()

	// Test default Keypair (Schnorr + X25519)
	privKeys, pubKeys, err := Keypair()
	if err != nil {
		t.Fatalf("Keypair() failed: %v", err)
	}

	// Test signing with the generated keypair
	message := []byte("message to sign with keypair")
	sig, err := privKeys.Sign(message)
	if err != nil {
		t.Fatalf("PrivateKeys.Sign failed: %v", err)
	}
	if !pubKeys.Verify(sig, message) {
		t.Error("PublicKeys.Verify failed for valid signature")
	}
	if !pubKeys.Verify(sig, message) {
		t.Error("second verify call failed")
	}

	// Test that wrong message fails verification
	if pubKeys.Verify(sig, []byte("wrong message")) {
		t.Error("PublicKeys.Verify should fail with wrong message")
	}

	// Test encapsulation with the generated keypair
	sharedKey, ct, err := pubKeys.EncapsulateNewSharedSecret()
	if err != nil {
		t.Fatalf("PublicKeys.EncapsulateNewSharedSecret failed: %v", err)
	}

	decapKey, err := privKeys.DecapsulateSharedSecret(ct)
	if err != nil {
		t.Fatalf("PrivateKeys.DecapsulateSharedSecret failed: %v", err)
	}

	if !bytes.Equal(sharedKey.Bytes(), decapKey.Bytes()) {
		t.Error("shared keys from keypair encapsulation do not match")
	}

	// Test KeypairOpt with Ed25519 + X25519
	privKeysEd, pubKeysEd, err := KeypairOpt(SchemeEd25519, EncapsulationX25519)
	if err != nil {
		t.Fatalf("KeypairOpt(Ed25519, X25519) failed: %v", err)
	}

	sigEd, err := privKeysEd.Sign(message)
	if err != nil {
		t.Fatalf("Ed25519 PrivateKeys.Sign failed: %v", err)
	}
	if !pubKeysEd.Verify(sigEd, message) {
		t.Error("Ed25519 PublicKeys.Verify failed")
	}
	if sigEd.Scheme() != SchemeEd25519 {
		t.Errorf("expected Ed25519 scheme, got %v", sigEd.Scheme())
	}

	// Test CBOR roundtrip for PublicKeys
	pubCBOR := pubKeys.TaggedCBOR().ToCBORData()
	pubCBORValue, err := dcbor.TryFromData(pubCBOR)
	if err != nil {
		t.Fatalf("TryFromData for PublicKeys failed: %v", err)
	}
	decodedPub, err := DecodeTaggedPublicKeys(pubCBORValue)
	if err != nil {
		t.Fatalf("DecodeTaggedPublicKeys failed: %v", err)
	}

	// Verify the decoded public keys can still verify
	if !decodedPub.Verify(sig, message) {
		t.Error("decoded PublicKeys cannot verify original signature")
	}
}

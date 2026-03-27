package bcenvelope

import (
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

func TestCryptoPlaintext(t *testing.T) {
	RegisterTags()

	// Alice sends a plaintext message to Bob.
	envelope := helloEnvelope()
	ur := envelope.TaggedCBOR().ToCBORData()

	assertActualExpected(t, envelope.Format(), `"Hello."`)

	// Bob receives the envelope and reads the message.
	received, err := EnvelopeFromCBORData(ur)
	if err != nil {
		t.Fatalf("failed to decode UR: %v", err)
	}
	received = checkEncoding(t, received)
	receivedPlaintext, err := ExtractSubject[string](received)
	if err != nil {
		t.Fatalf("failed to extract subject: %v", err)
	}
	if receivedPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, receivedPlaintext)
	}
}

func TestCryptoSymmetricEncryption(t *testing.T) {
	RegisterTags()

	// Alice and Bob have agreed to use this key.
	key := bccomponents.NewSymmetricKey()

	// Alice sends a message encrypted with the key to Bob.
	envelope, err := helloEnvelope().EncryptSubject(key)
	if err != nil {
		t.Fatalf("EncryptSubject failed: %v", err)
	}
	envelope = checkEncoding(t, envelope)
	ur := envelope.TaggedCBOR().ToCBORData()

	assertActualExpected(t, envelope.Format(), "ENCRYPTED")

	// Bob receives the envelope.
	receivedEnvelope, err := EnvelopeFromCBORData(ur)
	if err != nil {
		t.Fatalf("failed to decode: %v", err)
	}
	receivedEnvelope = checkEncoding(t, receivedEnvelope)

	// Bob decrypts and reads the message.
	decrypted, err := receivedEnvelope.DecryptSubject(key)
	if err != nil {
		t.Fatalf("DecryptSubject failed: %v", err)
	}
	receivedPlaintext, err := ExtractSubject[string](decrypted)
	if err != nil {
		t.Fatalf("ExtractSubject failed: %v", err)
	}
	if receivedPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, receivedPlaintext)
	}

	// Can't read with no key.
	_, err = ExtractSubject[string](receivedEnvelope)
	if err == nil {
		t.Error("expected error extracting string from encrypted envelope")
	}

	// Can't read with incorrect key.
	_, err = receivedEnvelope.DecryptSubject(bccomponents.NewSymmetricKey())
	if err == nil {
		t.Error("expected error decrypting with wrong key")
	}
}

func roundTripTest(t *testing.T, envelope *Envelope) {
	t.Helper()
	key := bccomponents.NewSymmetricKey()
	plaintextSubject := checkEncoding(t, envelope)
	encryptedSubject, err := plaintextSubject.EncryptSubject(key)
	if err != nil {
		t.Fatalf("EncryptSubject failed: %v", err)
	}
	if !encryptedSubject.IsEquivalentTo(plaintextSubject) {
		t.Error("encrypted should be equivalent to plaintext")
	}
	plaintextSubject2, err := encryptedSubject.DecryptSubject(key)
	if err != nil {
		t.Fatalf("DecryptSubject failed: %v", err)
	}
	plaintextSubject2 = checkEncoding(t, plaintextSubject2)
	if !encryptedSubject.IsEquivalentTo(plaintextSubject2) {
		t.Error("encrypted should be equivalent to decrypted")
	}
	if !plaintextSubject.IsIdenticalTo(plaintextSubject2) {
		t.Error("original and decrypted should be identical")
	}
}

func TestEncryptDecrypt(t *testing.T) {
	// leaf
	roundTripTest(t, NewEnvelope(plaintextHello))

	// node
	roundTripTest(t, NewEnvelope("Alice").AddAssertion("knows", "Bob"))

	// wrapped
	roundTripTest(t, NewEnvelope("Alice").Wrap())

	// known value
	roundTripTest(t, NewEnvelope(knownvalues.IsA))

	// assertion
	roundTripTest(t, NewAssertionEnvelope("knows", "Bob"))
}

func TestSignThenEncrypt(t *testing.T) {
	RegisterTags()

	// Alice and Bob have agreed to use this key.
	key := bccomponents.NewSymmetricKey()

	// Alice signs a plaintext message, then encrypts it.
	signed := helloEnvelope().AddSignature(alicePrivateKey().SchnorrSigningPrivateKey())
	signed = checkEncoding(t, signed)
	wrapped := signed.Wrap()
	wrapped = checkEncoding(t, wrapped)
	envelope, err := wrapped.EncryptSubject(key)
	if err != nil {
		t.Fatalf("EncryptSubject failed: %v", err)
	}
	envelope = checkEncoding(t, envelope)
	ur := envelope.TaggedCBOR().ToCBORData()

	assertActualExpected(t, envelope.Format(), "ENCRYPTED")

	// Bob receives the envelope, decrypts it using the shared key, and then
	// validates Alice's signature.
	received, err := EnvelopeFromCBORData(ur)
	if err != nil {
		t.Fatalf("failed to decode: %v", err)
	}
	received = checkEncoding(t, received)
	decrypted, err := received.DecryptSubject(key)
	if err != nil {
		t.Fatalf("DecryptSubject failed: %v", err)
	}
	decrypted = checkEncoding(t, decrypted)
	unwrapped, err := decrypted.TryUnwrap()
	if err != nil {
		t.Fatalf("TryUnwrap failed: %v", err)
	}
	unwrapped = checkEncoding(t, unwrapped)
	verified, err := unwrapped.VerifySignatureFrom(alicePublicKey())
	if err != nil {
		t.Fatalf("VerifySignatureFrom failed: %v", err)
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

func TestEncryptThenSign(t *testing.T) {
	RegisterTags()

	// Alice and Bob have agreed to use this key.
	key := bccomponents.NewSymmetricKey()

	// Alice encrypts a plaintext message, then signs it.
	envelope, err := helloEnvelope().EncryptSubject(key)
	if err != nil {
		t.Fatalf("EncryptSubject failed: %v", err)
	}
	envelope = envelope.AddSignature(alicePrivateKey().SchnorrSigningPrivateKey())
	envelope = checkEncoding(t, envelope)
	ur := envelope.TaggedCBOR().ToCBORData()

	assertActualExpected(t, envelope.Format(),
		`ENCRYPTED [
    'signed': Signature
]`)

	// Bob receives the envelope, validates Alice's signature, then decrypts the
	// message.
	received, err := EnvelopeFromCBORData(ur)
	if err != nil {
		t.Fatalf("failed to decode: %v", err)
	}
	received = checkEncoding(t, received)
	verified, err := received.VerifySignatureFrom(alicePublicKey())
	if err != nil {
		t.Fatalf("VerifySignatureFrom failed: %v", err)
	}
	decrypted, err := verified.DecryptSubject(key)
	if err != nil {
		t.Fatalf("DecryptSubject failed: %v", err)
	}
	decrypted = checkEncoding(t, decrypted)
	receivedPlaintext, err := ExtractSubject[string](decrypted)
	if err != nil {
		t.Fatalf("ExtractSubject failed: %v", err)
	}
	if receivedPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, receivedPlaintext)
	}
}

func TestMultiRecipient(t *testing.T) {
	// Alice encrypts a message so that it can only be decrypted by Bob or Carol.
	contentKey := bccomponents.NewSymmetricKey()
	envelope, err := helloEnvelope().EncryptSubject(contentKey)
	if err != nil {
		t.Fatalf("EncryptSubject failed: %v", err)
	}
	envelope = envelope.
		AddRecipient(bobPublicKey().EncapsulationPublicKey(), contentKey).
		AddRecipient(carolPublicKey().EncapsulationPublicKey(), contentKey)
	envelope = checkEncoding(t, envelope)
	ur := envelope.TaggedCBOR().ToCBORData()

	assertActualExpected(t, envelope.Format(),
		`ENCRYPTED [
    'hasRecipient': SealedMessage
    'hasRecipient': SealedMessage
]`)

	// The envelope is received
	receivedEnvelope, err := EnvelopeFromCBORData(ur)
	if err != nil {
		t.Fatalf("failed to decode: %v", err)
	}

	// Bob decrypts and reads the message
	bobDecrypted, err := receivedEnvelope.DecryptSubjectToRecipient(bobPrivateKey().PrivateKeys().EncapsulationPrivateKey())
	if err != nil {
		t.Fatalf("Bob DecryptSubjectToRecipient failed: %v", err)
	}
	bobDecrypted = checkEncoding(t, bobDecrypted)
	bobPlaintext, err := ExtractSubject[string](bobDecrypted)
	if err != nil {
		t.Fatalf("Bob ExtractSubject failed: %v", err)
	}
	if bobPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, bobPlaintext)
	}

	// Carol decrypts and reads the message
	carolDecrypted, err := receivedEnvelope.DecryptSubjectToRecipient(carolPrivateKey().PrivateKeys().EncapsulationPrivateKey())
	if err != nil {
		t.Fatalf("Carol DecryptSubjectToRecipient failed: %v", err)
	}
	carolDecrypted = checkEncoding(t, carolDecrypted)
	carolPlaintext, err := ExtractSubject[string](carolDecrypted)
	if err != nil {
		t.Fatalf("Carol ExtractSubject failed: %v", err)
	}
	if carolPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, carolPlaintext)
	}

	// Alice didn't encrypt it to herself, so she can't read it.
	_, err = receivedEnvelope.DecryptSubjectToRecipient(alicePrivateKey().PrivateKeys().EncapsulationPrivateKey())
	if err == nil {
		t.Error("expected error decrypting as Alice")
	}
}

func TestVisibleSignatureMultiRecipient(t *testing.T) {
	// Alice signs a message, and then encrypts it so that it can only be
	// decrypted by Bob or Carol.
	contentKey := bccomponents.NewSymmetricKey()
	envelope, err := helloEnvelope().
		AddSignature(alicePrivateKey().PrivateKeys()).
		EncryptSubject(contentKey)
	if err != nil {
		t.Fatalf("EncryptSubject failed: %v", err)
	}
	envelope = envelope.
		AddRecipient(bobPublicKey().EncapsulationPublicKey(), contentKey).
		AddRecipient(carolPublicKey().EncapsulationPublicKey(), contentKey)
	envelope = checkEncoding(t, envelope)
	ur := envelope.TaggedCBOR().ToCBORData()

	assertActualExpected(t, envelope.Format(),
		`ENCRYPTED [
    'hasRecipient': SealedMessage
    'hasRecipient': SealedMessage
    'signed': Signature
]`)

	// The envelope is received
	receivedEnvelope, err := EnvelopeFromCBORData(ur)
	if err != nil {
		t.Fatalf("failed to decode: %v", err)
	}

	// Bob validates Alice's signature, then decrypts and reads the message
	bobVerified, err := receivedEnvelope.VerifySignatureFrom(alicePublicKey())
	if err != nil {
		t.Fatalf("Bob VerifySignatureFrom failed: %v", err)
	}
	bobDecrypted, err := bobVerified.DecryptSubjectToRecipient(bobPrivateKey().PrivateKeys().EncapsulationPrivateKey())
	if err != nil {
		t.Fatalf("Bob DecryptSubjectToRecipient failed: %v", err)
	}
	bobDecrypted = checkEncoding(t, bobDecrypted)
	bobPlaintext, err := ExtractSubject[string](bobDecrypted)
	if err != nil {
		t.Fatalf("Bob ExtractSubject failed: %v", err)
	}
	if bobPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, bobPlaintext)
	}

	// Carol validates Alice's signature, then decrypts and reads the message
	carolVerified, err := receivedEnvelope.VerifySignatureFrom(alicePublicKey())
	if err != nil {
		t.Fatalf("Carol VerifySignatureFrom failed: %v", err)
	}
	carolDecrypted, err := carolVerified.DecryptSubjectToRecipient(carolPrivateKey().PrivateKeys().EncapsulationPrivateKey())
	if err != nil {
		t.Fatalf("Carol DecryptSubjectToRecipient failed: %v", err)
	}
	carolDecrypted = checkEncoding(t, carolDecrypted)
	carolPlaintext, err := ExtractSubject[string](carolDecrypted)
	if err != nil {
		t.Fatalf("Carol ExtractSubject failed: %v", err)
	}
	if carolPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, carolPlaintext)
	}

	// Alice didn't encrypt it to herself, so she can't read it.
	_, err = receivedEnvelope.DecryptSubjectToRecipient(alicePrivateKey().PrivateKeys().EncapsulationPrivateKey())
	if err == nil {
		t.Error("expected error decrypting as Alice")
	}
}

func TestHiddenSignatureMultiRecipient(t *testing.T) {
	// Alice signs a message, and then encloses it in another envelope before
	// encrypting it so that it can only be decrypted by Bob or Carol.
	contentKey := bccomponents.NewSymmetricKey()
	envelope, err := helloEnvelope().
		AddSignature(alicePrivateKey().PrivateKeys()).
		Wrap().
		EncryptSubject(contentKey)
	if err != nil {
		t.Fatalf("EncryptSubject failed: %v", err)
	}
	envelope = envelope.
		AddRecipient(bobPublicKey().EncapsulationPublicKey(), contentKey).
		AddRecipient(carolPublicKey().EncapsulationPublicKey(), contentKey)
	envelope = checkEncoding(t, envelope)
	ur := envelope.TaggedCBOR().ToCBORData()

	assertActualExpected(t, envelope.Format(),
		`ENCRYPTED [
    'hasRecipient': SealedMessage
    'hasRecipient': SealedMessage
]`)

	// The envelope is received
	receivedEnvelope, err := EnvelopeFromCBORData(ur)
	if err != nil {
		t.Fatalf("failed to decode: %v", err)
	}

	// Bob decrypts the envelope, then extracts the inner envelope and validates
	// Alice's signature, then reads the message
	bobDecrypted, err := receivedEnvelope.DecryptSubjectToRecipient(bobPrivateKey().PrivateKeys().EncapsulationPrivateKey())
	if err != nil {
		t.Fatalf("Bob DecryptSubjectToRecipient failed: %v", err)
	}
	bobUnwrapped, err := bobDecrypted.TryUnwrap()
	if err != nil {
		t.Fatalf("Bob TryUnwrap failed: %v", err)
	}
	bobUnwrapped = checkEncoding(t, bobUnwrapped)
	bobVerified, err := bobUnwrapped.VerifySignatureFrom(alicePublicKey())
	if err != nil {
		t.Fatalf("Bob VerifySignatureFrom failed: %v", err)
	}
	bobPlaintext, err := ExtractSubject[string](bobVerified)
	if err != nil {
		t.Fatalf("Bob ExtractSubject failed: %v", err)
	}
	if bobPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, bobPlaintext)
	}

	// Carol decrypts the envelope, then extracts the inner envelope and
	// validates Alice's signature, then reads the message
	carolDecrypted, err := receivedEnvelope.DecryptSubjectToRecipient(carolPrivateKey().PrivateKeys().EncapsulationPrivateKey())
	if err != nil {
		t.Fatalf("Carol DecryptSubjectToRecipient failed: %v", err)
	}
	carolUnwrapped, err := carolDecrypted.TryUnwrap()
	if err != nil {
		t.Fatalf("Carol TryUnwrap failed: %v", err)
	}
	carolUnwrapped = checkEncoding(t, carolUnwrapped)
	carolVerified, err := carolUnwrapped.VerifySignatureFrom(alicePublicKey())
	if err != nil {
		t.Fatalf("Carol VerifySignatureFrom failed: %v", err)
	}
	carolPlaintext, err := ExtractSubject[string](carolVerified)
	if err != nil {
		t.Fatalf("Carol ExtractSubject failed: %v", err)
	}
	if carolPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, carolPlaintext)
	}

	// Alice didn't encrypt it to herself, so she can't read it.
	_, err = receivedEnvelope.DecryptSubjectToRecipient(alicePrivateKey().PrivateKeys().EncapsulationPrivateKey())
	if err == nil {
		t.Error("expected error decrypting as Alice")
	}
}

func TestSecret1(t *testing.T) {
	RegisterTags()
	bobPassword := []byte("correct horse battery staple")

	// Alice encrypts a message so that it can only be decrypted by Bob's password.
	envelope, err := helloEnvelope().Lock(bccomponents.KDMethodHKDF, bobPassword)
	if err != nil {
		t.Fatalf("Lock failed: %v", err)
	}
	checkEncoding(t, envelope)
	ur := envelope.TaggedCBOR().ToCBORData()

	assertActualExpected(t, envelope.Format(),
		`ENCRYPTED [
    'hasSecret': EncryptedKey(HKDF(SHA256))
]`)

	// The envelope is received
	receivedEnvelope, err := EnvelopeFromCBORData(ur)
	if err != nil {
		t.Fatalf("failed to decode: %v", err)
	}

	// Bob decrypts and reads the message
	bobDecrypted, err := receivedEnvelope.Unlock(bobPassword)
	if err != nil {
		t.Fatalf("Unlock failed: %v", err)
	}
	bobDecrypted = checkEncoding(t, bobDecrypted)
	bobPlaintext, err := ExtractSubject[string](bobDecrypted)
	if err != nil {
		t.Fatalf("ExtractSubject failed: %v", err)
	}
	if bobPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, bobPlaintext)
	}

	// Eve tries to decrypt the message with a different password
	_, err = receivedEnvelope.Unlock([]byte("wrong password"))
	if err == nil {
		t.Error("expected error with wrong password")
	}
}

func TestSecret2(t *testing.T) {
	RegisterTags()

	bobPassword := []byte("correct horse battery staple")
	carolPassword := []byte("Able was I ere I saw Elba")
	gracyPassword := []byte("Madam, in Eden, I'm Adam")
	contentKey := bccomponents.NewSymmetricKey()

	envelope, err := helloEnvelope().EncryptSubject(contentKey)
	if err != nil {
		t.Fatalf("EncryptSubject failed: %v", err)
	}
	envelope, err = envelope.AddSecret(bccomponents.KDMethodHKDF, bobPassword, contentKey)
	if err != nil {
		t.Fatalf("AddSecret HKDF failed: %v", err)
	}
	envelope, err = envelope.AddSecret(bccomponents.KDMethodScrypt, carolPassword, contentKey)
	if err != nil {
		t.Fatalf("AddSecret Scrypt failed: %v", err)
	}
	envelope, err = envelope.AddSecret(bccomponents.KDMethodArgon2id, gracyPassword, contentKey)
	if err != nil {
		t.Fatalf("AddSecret Argon2id failed: %v", err)
	}
	envelope = checkEncoding(t, envelope)
	ur := envelope.TaggedCBOR().ToCBORData()

	assertActualExpected(t, envelope.Format(),
		`ENCRYPTED [
    'hasSecret': EncryptedKey(Argon2id)
    'hasSecret': EncryptedKey(HKDF(SHA256))
    'hasSecret': EncryptedKey(Scrypt)
]`)

	// The envelope is received
	receivedEnvelope, err := EnvelopeFromCBORData(ur)
	if err != nil {
		t.Fatalf("failed to decode: %v", err)
	}

	// Bob decrypts and reads the message
	bobDecrypted, err := receivedEnvelope.UnlockSubject(bobPassword)
	if err != nil {
		t.Fatalf("Bob UnlockSubject failed: %v", err)
	}
	bobDecrypted = checkEncoding(t, bobDecrypted)
	bobPlaintext, err := ExtractSubject[string](bobDecrypted)
	if err != nil {
		t.Fatalf("Bob ExtractSubject failed: %v", err)
	}
	if bobPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, bobPlaintext)
	}

	// Carol decrypts and reads the message
	carolDecrypted, err := receivedEnvelope.UnlockSubject(carolPassword)
	if err != nil {
		t.Fatalf("Carol UnlockSubject failed: %v", err)
	}
	carolDecrypted = checkEncoding(t, carolDecrypted)
	carolPlaintext, err := ExtractSubject[string](carolDecrypted)
	if err != nil {
		t.Fatalf("Carol ExtractSubject failed: %v", err)
	}
	if carolPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, carolPlaintext)
	}

	// Gracy decrypts and reads the message
	gracyDecrypted, err := receivedEnvelope.UnlockSubject(gracyPassword)
	if err != nil {
		t.Fatalf("Gracy UnlockSubject failed: %v", err)
	}
	gracyDecrypted = checkEncoding(t, gracyDecrypted)
	gracyPlaintext, err := ExtractSubject[string](gracyDecrypted)
	if err != nil {
		t.Fatalf("Gracy ExtractSubject failed: %v", err)
	}
	if gracyPlaintext != plaintextHello {
		t.Errorf("expected %q, got %q", plaintextHello, gracyPlaintext)
	}

	// Eve tries to decrypt the message with a different password
	_, err = receivedEnvelope.UnlockSubject([]byte("wrong password"))
	if err == nil {
		t.Error("expected error with wrong password")
	}
}

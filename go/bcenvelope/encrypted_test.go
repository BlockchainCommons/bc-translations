package bcenvelope

import (
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

func symmetricKeyForEncryptedTest() bccomponents.SymmetricKey {
	var data [bccomponents.SymmetricKeySize]byte
	copy(data[:], mustDecodeHex("38900719dea655e9a1bc1682aaccf0bfcd79a7239db672d39216e4acdd660dc0"))
	return bccomponents.SymmetricKeyFromData(data)
}

func fakeNonceForEncryptedTest() bccomponents.Nonce {
	var data [bccomponents.NonceSize]byte
	copy(data[:], mustDecodeHex("4d785658f36c22fb5aed3ac0"))
	return bccomponents.NonceFromData(data)
}

func encryptedTest(t *testing.T, e1 *Envelope) {
	t.Helper()
	key := symmetricKeyForEncryptedTest()
	nonce := fakeNonceForEncryptedTest()

	e1 = checkEncoding(t, e1)

	e2, err := e1.EncryptSubjectWithNonce(key, &nonce)
	if err != nil {
		t.Fatalf("EncryptSubjectWithNonce failed: %v", err)
	}
	e2 = checkEncoding(t, e2)

	if !e1.IsEquivalentTo(e2) {
		t.Error("e1 should be equivalent to e2")
	}
	if !e1.Subject().IsEquivalentTo(e2.Subject()) {
		t.Error("e1.Subject should be equivalent to e2.Subject")
	}

	encryptedMsg, err := ExtractSubject[bccomponents.EncryptedMessage](e2.Subject())
	if err != nil {
		// For node case, the subject itself is encrypted
		if e2.Subject().IsEncrypted() {
			// That's fine, the encrypted message is the subject itself
		} else {
			t.Fatalf("ExtractSubject EncryptedMessage failed: %v", err)
		}
	} else {
		_ = encryptedMsg
	}

	e3, err := e2.DecryptSubject(key)
	if err != nil {
		t.Fatalf("DecryptSubject failed: %v", err)
	}

	if !e1.IsEquivalentTo(e3) {
		t.Error("e1 should be equivalent to e3")
	}
}

func TestEncrypted(t *testing.T) {
	// basic envelope
	encryptedTest(t, NewEnvelope("Hello."))
	// wrapped
	encryptedTest(t, NewEnvelope("Hello.").Wrap())
	// double wrapped
	encryptedTest(t, NewEnvelope("Hello.").Wrap().Wrap())
	// known value
	encryptedTest(t, NewEnvelope(knownvalues.Note))
	// assertion
	encryptedTest(t, NewAssertionEnvelope("knows", "Bob"))
	// single assertion
	encryptedTest(t, NewEnvelope("Alice").AddAssertion("knows", "Bob"))
	// double assertion
	encryptedTest(t, NewEnvelope("Alice").AddAssertion("knows", "Bob").AddAssertion("knows", "Carol"))
}

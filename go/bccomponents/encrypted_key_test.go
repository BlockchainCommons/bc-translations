package bccomponents

import (
	"testing"
)

func testSecret() []byte {
	return []byte("correct horse battery staple")
}

func testContentKey() SymmetricKey {
	return NewSymmetricKey()
}

func TestEncryptedKeyHKDFRoundtrip(t *testing.T) {
	RegisterTags()
	secret := testSecret()
	contentKey := testContentKey()

	encrypted, err := LockEncryptedKey(KDMethodHKDF, secret, contentKey)
	if err != nil {
		t.Fatalf("LockEncryptedKey(HKDF) failed: %v", err)
	}
	if s := encrypted.String(); s != "EncryptedKey(HKDF(SHA256))" {
		t.Errorf("display = %q, want %q", s, "EncryptedKey(HKDF(SHA256))")
	}

	// CBOR roundtrip
	cbor := encrypted.ToCBOR()
	encrypted2, err := DecodeTaggedEncryptedKey(cbor)
	if err != nil {
		t.Fatalf("DecodeTaggedEncryptedKey failed: %v", err)
	}

	decrypted, err := encrypted2.Unlock(secret)
	if err != nil {
		t.Fatalf("Unlock failed: %v", err)
	}
	if !contentKey.Equal(decrypted) {
		t.Errorf("decrypted key does not match original")
	}
}

func TestEncryptedKeyPBKDF2Roundtrip(t *testing.T) {
	RegisterTags()
	secret := testSecret()
	contentKey := testContentKey()

	encrypted, err := LockEncryptedKey(KDMethodPBKDF2, secret, contentKey)
	if err != nil {
		t.Fatalf("LockEncryptedKey(PBKDF2) failed: %v", err)
	}
	if s := encrypted.String(); s != "EncryptedKey(PBKDF2(SHA256))" {
		t.Errorf("display = %q, want %q", s, "EncryptedKey(PBKDF2(SHA256))")
	}

	// CBOR roundtrip
	cbor := encrypted.ToCBOR()
	encrypted2, err := DecodeTaggedEncryptedKey(cbor)
	if err != nil {
		t.Fatalf("DecodeTaggedEncryptedKey failed: %v", err)
	}

	decrypted, err := encrypted2.Unlock(secret)
	if err != nil {
		t.Fatalf("Unlock failed: %v", err)
	}
	if !contentKey.Equal(decrypted) {
		t.Errorf("decrypted key does not match original")
	}
}

func TestEncryptedKeyScryptRoundtrip(t *testing.T) {
	RegisterTags()
	secret := testSecret()
	contentKey := testContentKey()

	encrypted, err := LockEncryptedKey(KDMethodScrypt, secret, contentKey)
	if err != nil {
		t.Fatalf("LockEncryptedKey(Scrypt) failed: %v", err)
	}
	if s := encrypted.String(); s != "EncryptedKey(Scrypt)" {
		t.Errorf("display = %q, want %q", s, "EncryptedKey(Scrypt)")
	}

	// CBOR roundtrip
	cbor := encrypted.ToCBOR()
	encrypted2, err := DecodeTaggedEncryptedKey(cbor)
	if err != nil {
		t.Fatalf("DecodeTaggedEncryptedKey failed: %v", err)
	}

	decrypted, err := encrypted2.Unlock(secret)
	if err != nil {
		t.Fatalf("Unlock failed: %v", err)
	}
	if !contentKey.Equal(decrypted) {
		t.Errorf("decrypted key does not match original")
	}
}

func TestEncryptedKeyArgon2idRoundtrip(t *testing.T) {
	RegisterTags()
	secret := testSecret()
	contentKey := testContentKey()

	encrypted, err := LockEncryptedKey(KDMethodArgon2id, secret, contentKey)
	if err != nil {
		t.Fatalf("LockEncryptedKey(Argon2id) failed: %v", err)
	}
	if s := encrypted.String(); s != "EncryptedKey(Argon2id)" {
		t.Errorf("display = %q, want %q", s, "EncryptedKey(Argon2id)")
	}

	// CBOR roundtrip
	cbor := encrypted.ToCBOR()
	encrypted2, err := DecodeTaggedEncryptedKey(cbor)
	if err != nil {
		t.Fatalf("DecodeTaggedEncryptedKey failed: %v", err)
	}

	decrypted, err := encrypted2.Unlock(secret)
	if err != nil {
		t.Fatalf("Unlock failed: %v", err)
	}
	if !contentKey.Equal(decrypted) {
		t.Errorf("decrypted key does not match original")
	}
}

func TestEncryptedKeyWrongSecretFails(t *testing.T) {
	RegisterTags()
	secret := testSecret()
	wrongSecret := []byte("wrong secret")
	contentKey := testContentKey()

	methods := []struct {
		name   string
		method KeyDerivationMethod
	}{
		{"HKDF", KDMethodHKDF},
		{"PBKDF2", KDMethodPBKDF2},
		{"Scrypt", KDMethodScrypt},
		{"Argon2id", KDMethodArgon2id},
	}

	for _, tc := range methods {
		t.Run(tc.name, func(t *testing.T) {
			encrypted, err := LockEncryptedKey(tc.method, secret, contentKey)
			if err != nil {
				t.Fatalf("LockEncryptedKey(%s) failed: %v", tc.name, err)
			}
			_, err = encrypted.Unlock(wrongSecret)
			if err == nil {
				t.Errorf("Unlock with wrong secret should have failed for %s", tc.name)
			}
		})
	}
}

func TestEncryptedKeyParamsVariant(t *testing.T) {
	RegisterTags()
	secret := testSecret()
	contentKey := testContentKey()

	tests := []struct {
		name           string
		method         KeyDerivationMethod
		expectedMethod KeyDerivationMethod
	}{
		{"HKDF", KDMethodHKDF, KDMethodHKDF},
		{"PBKDF2", KDMethodPBKDF2, KDMethodPBKDF2},
		{"Scrypt", KDMethodScrypt, KDMethodScrypt},
		{"Argon2id", KDMethodArgon2id, KDMethodArgon2id},
	}

	for _, tc := range tests {
		t.Run(tc.name, func(t *testing.T) {
			encrypted, err := LockEncryptedKey(tc.method, secret, contentKey)
			if err != nil {
				t.Fatalf("LockEncryptedKey(%s) failed: %v", tc.name, err)
			}

			// Verify the AAD contains the correct method by doing a CBOR roundtrip
			// and checking that unlock with the correct secret succeeds.
			cbor := encrypted.ToCBOR()
			encrypted2, err := DecodeTaggedEncryptedKey(cbor)
			if err != nil {
				t.Fatalf("DecodeTaggedEncryptedKey(%s) failed: %v", tc.name, err)
			}

			// Verify correct method is encoded in the AAD
			aadCBOR, err := encrypted2.AADCBOR()
			if err != nil {
				t.Fatalf("AADCBOR(%s) failed: %v", tc.name, err)
			}
			a, err := aadCBOR.TryIntoArray()
			if err != nil {
				t.Fatalf("TryIntoArray(%s) failed: %v", tc.name, err)
			}
			if len(a) == 0 {
				t.Fatalf("AAD array is empty for %s", tc.name)
			}
			methodIndex, err := a[0].TryIntoUInt()
			if err != nil {
				t.Fatalf("TryIntoUInt(%s) failed: %v", tc.name, err)
			}
			if int(methodIndex) != tc.expectedMethod.Index() {
				t.Errorf("method index = %d, want %d for %s", methodIndex, tc.expectedMethod.Index(), tc.name)
			}
		})
	}
}

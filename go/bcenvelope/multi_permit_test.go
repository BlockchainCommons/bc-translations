package bcenvelope

import (
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
	sskr "github.com/nickel-blockchaincommons/sskr-go"
)

func TestMultiPermit(t *testing.T) {
	bccomponents.RegisterTags()

	//
	// Alice composes a poem.
	//
	poemText := "At midnight, the clocks sang lullabies to the wandering teacups."

	//
	// Alice creates a new envelope and assigns the text as the envelope's
	// subject.
	//
	date := dcbor.DateFromYMD(2025, 5, 15)
	originalEnvelope := NewEnvelope(poemText).
		AddType("poem").
		AddAssertion("title", "A Song of Ice Cream").
		AddAssertion("author", "Plonkus the Iridescent").
		AddAssertion(knownvalues.Date, date)

	//
	// Alice signs the envelope with her private key.
	//
	alicePrivKeys, alicePubKeys, err := bccomponents.Keypair()
	if err != nil {
		t.Fatalf("keypair failed: %v", err)
	}
	signedEnvelope := originalEnvelope.Sign(alicePrivKeys)

	assertActualExpected(t, signedEnvelope.Format(), `{
    "At midnight, the clocks sang lullabies to the wandering teacups." [
        'isA': "poem"
        "author": "Plonkus the Iridescent"
        "title": "A Song of Ice Cream"
        'date': 2025-05-15
    ]
} [
    'signed': Signature
]`)

	//
	// Alice picks a random symmetric "content key" and uses it to encrypt the
	// signed envelope.
	//
	contentKey := bccomponents.NewSymmetricKey()
	encryptedEnvelope := signedEnvelope.Encrypt(contentKey)

	assertActualExpected(t, encryptedEnvelope.Format(), `ENCRYPTED`)

	//
	// Alice wants to be able to recover the envelope later using a password.
	//
	password := []byte("unicorns_dance_on_mars_while_eating_pizza")
	lockedEnvelope, err := encryptedEnvelope.AddSecret(
		bccomponents.KDMethodArgon2id,
		password,
		contentKey,
	)
	if err != nil {
		t.Fatalf("add_secret failed: %v", err)
	}

	assertActualExpected(t, lockedEnvelope.Format(), `ENCRYPTED [
    'hasSecret': EncryptedKey(Argon2id)
]`)

	//
	// Next, Alice wants to be able to unlock her envelope using her private
	// key, and she also wants Bob to be able to unlock it using his private
	// key.
	//
	bobPrivKeys, bobPubKeys, err := bccomponents.Keypair()
	if err != nil {
		t.Fatalf("bob keypair failed: %v", err)
	}
	lockedEnvelope = lockedEnvelope.
		AddRecipient(alicePubKeys.EncapsulationPublicKey(), contentKey).
		AddRecipient(bobPubKeys.EncapsulationPublicKey(), contentKey)

	assertActualExpected(t, lockedEnvelope.Format(), `ENCRYPTED [
    'hasRecipient': SealedMessage
    'hasRecipient': SealedMessage
    'hasSecret': EncryptedKey(Argon2id)
]`)

	//
	// Alice creates a 2-of-3 SSKR group and "shards" the envelope.
	//
	sskrGroup, err := sskr.NewGroupSpec(2, 3)
	if err != nil {
		t.Fatalf("sskr group spec failed: %v", err)
	}
	sskrSpec, err := sskr.NewSpec(1, []sskr.GroupSpec{sskrGroup})
	if err != nil {
		t.Fatalf("sskr spec failed: %v", err)
	}
	shardedEnvelopes, err := lockedEnvelope.SSKRSplitFlattened(&sskrSpec, contentKey)
	if err != nil {
		t.Fatalf("sskr split failed: %v", err)
	}

	assertActualExpected(t, shardedEnvelopes[0].Format(), `ENCRYPTED [
    'hasRecipient': SealedMessage
    'hasRecipient': SealedMessage
    'hasSecret': EncryptedKey(Argon2id)
    'sskrShare': SSKRShare
]`)

	//
	// Five different ways to unlock:
	//

	// 1. Using the content key.
	receivedEnvelope := shardedEnvelopes[0]
	unlockedEnvelope, err := receivedEnvelope.Decrypt(contentKey)
	if err != nil {
		t.Fatalf("decrypt with content key failed: %v", err)
	}
	if !unlockedEnvelope.IsEquivalentTo(signedEnvelope) {
		t.Fatal("content key unlocked envelope should equal signed envelope")
	}

	// 2. Using the password.
	unlockedEnvelope, err = receivedEnvelope.Unlock(password)
	if err != nil {
		t.Fatalf("unlock with password failed: %v", err)
	}
	if !unlockedEnvelope.IsEquivalentTo(signedEnvelope) {
		t.Fatal("password unlocked envelope should equal signed envelope")
	}

	// 3. Using Alice's private key.
	unlockedEnvelope, err = receivedEnvelope.DecryptToRecipient(alicePrivKeys.EncapsulationPrivateKey())
	if err != nil {
		t.Fatalf("decrypt to Alice recipient failed: %v", err)
	}
	if !unlockedEnvelope.IsEquivalentTo(signedEnvelope) {
		t.Fatal("Alice recipient unlocked envelope should equal signed envelope")
	}

	// 4. Using Bob's private key.
	unlockedEnvelope, err = receivedEnvelope.DecryptToRecipient(bobPrivKeys.EncapsulationPrivateKey())
	if err != nil {
		t.Fatalf("decrypt to Bob recipient failed: %v", err)
	}
	if !unlockedEnvelope.IsEquivalentTo(signedEnvelope) {
		t.Fatal("Bob recipient unlocked envelope should equal signed envelope")
	}

	// 5. Using any two of the three SSKR shares.
	sskrJoined, err := SSKRJoin([]*Envelope{shardedEnvelopes[0], shardedEnvelopes[2]})
	if err != nil {
		t.Fatalf("sskr join failed: %v", err)
	}
	unlockedEnvelope, err = sskrJoined.TryUnwrap()
	if err != nil {
		t.Fatalf("unwrap failed: %v", err)
	}
	if !unlockedEnvelope.IsEquivalentTo(signedEnvelope) {
		t.Fatal("SSKR unlocked envelope should equal signed envelope")
	}

	// Verify Alice's signature
	_, err = unlockedEnvelope.Verify(alicePubKeys)
	if err != nil {
		t.Fatalf("signature verification failed: %v", err)
	}
}

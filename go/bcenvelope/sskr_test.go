package bcenvelope

import (
	"bytes"
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	sskr "github.com/nickel-blockchaincommons/sskr-go"
)

func TestSSKR(t *testing.T) {
	bccomponents.RegisterTags()

	// Dan has a cryptographic seed he wants to backup using a social recovery
	// scheme. The seed includes metadata he wants to back up also, making
	// it too large to fit into a basic SSKR share.
	danSeedData := mustDecodeHex("59f2293a5bce7d4de59e71b4207ac5d2")
	danSeed := NewTestSeed(danSeedData)
	danSeed.SetName("Dark Purple Aqua Love")
	creationDate, err := dcbor.DateFromString("2021-02-24")
	if err != nil {
		t.Fatalf("date parse failed: %v", err)
	}
	danSeed.SetCreationDate(&creationDate)
	danSeed.SetNote("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.")

	// Dan encrypts the seed and then splits the content key into a single group
	// 2-of-3.
	contentKey := bccomponents.NewSymmetricKey()
	seedEnvelope := testSeedToEnvelope(danSeed)
	encryptedSeedEnvelope, err := seedEnvelope.Wrap().EncryptSubject(contentKey)
	if err != nil {
		t.Fatalf("encrypt failed: %v", err)
	}

	group, err := sskr.NewGroupSpec(2, 3)
	if err != nil {
		t.Fatalf("group spec failed: %v", err)
	}
	spec, err := sskr.NewSpec(1, []sskr.GroupSpec{group})
	if err != nil {
		t.Fatalf("sskr spec failed: %v", err)
	}
	envelopes, err := encryptedSeedEnvelope.SSKRSplit(&spec, contentKey)
	if err != nil {
		t.Fatalf("sskr split failed: %v", err)
	}

	// Flattening the array of arrays
	var sentEnvelopes []*Envelope
	for _, group := range envelopes {
		sentEnvelopes = append(sentEnvelopes, group...)
	}

	assertActualExpected(t, sentEnvelopes[0].Format(), `ENCRYPTED [
    'sskrShare': SSKRShare
]`)

	// Round-trip through CBOR
	sentCBORs := make([]dcbor.CBOR, len(sentEnvelopes))
	for i, e := range sentEnvelopes {
		sentCBORs[i] = e.TaggedCBOR()
	}

	// Dan sends one envelope to each of Alice, Bob, and Carol.
	// Alice's share is unrecovered.
	bobEnvelope, err := EnvelopeFromTaggedCBORValue(sentCBORs[1])
	if err != nil {
		t.Fatalf("Bob envelope decode failed: %v", err)
	}
	carolEnvelope, err := EnvelopeFromTaggedCBORValue(sentCBORs[2])
	if err != nil {
		t.Fatalf("Carol envelope decode failed: %v", err)
	}

	// At some future point, Dan retrieves two of the three envelopes so he can
	// recover his seed.
	recoveredEnvelopes := []*Envelope{bobEnvelope, carolEnvelope}
	recoveredEncrypted, err := SSKRJoin(recoveredEnvelopes)
	if err != nil {
		t.Fatalf("sskr join failed: %v", err)
	}

	recoveredSeedEnvelope, err := recoveredEncrypted.TryUnwrap()
	if err != nil {
		t.Fatalf("unwrap failed: %v", err)
	}

	recoveredSeed, err := testSeedFromEnvelope(recoveredSeedEnvelope)
	if err != nil {
		t.Fatalf("seed from envelope failed: %v", err)
	}

	// The recovered seed is correct.
	if !bytes.Equal(danSeed.Data(), recoveredSeed.Data()) {
		t.Fatal("data mismatch")
	}
	if danSeed.Name() != recoveredSeed.Name() {
		t.Fatal("name mismatch")
	}
	if danSeed.Note() != recoveredSeed.Note() {
		t.Fatal("note mismatch")
	}

	// Attempting to recover with only one of the envelopes won't work.
	_, err = SSKRJoin([]*Envelope{bobEnvelope})
	if err == nil {
		t.Fatal("expected error with insufficient shares")
	}
}

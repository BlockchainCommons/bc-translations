package bcenvelope

import (
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

func TestFriendsList(t *testing.T) {
	// This document contains a list of people Alice knows. Each "knows"
	// assertion has been salted so if the assertions have been elided one
	// can't merely guess at who she knows by pairing the "knows" predicate
	// with the names of possibly-known associates and comparing the
	// resulting digests to the elided digests in the document.
	aliceFriends := NewEnvelope("Alice").
		AddAssertionSalted("knows", "Bob", true).
		AddAssertionSalted("knows", "Carol", true).
		AddAssertionSalted("knows", "Dan", true)

	assertActualExpected(t, aliceFriends.Format(), `"Alice" [
    {
        "knows": "Bob"
    } [
        'salt': Salt
    ]
    {
        "knows": "Carol"
    } [
        'salt': Salt
    ]
    {
        "knows": "Dan"
    } [
        'salt': Salt
    ]
]`)

	// Alice provides just the root digest of her document to a third party.
	emptySet := make(map[bccomponents.Digest]struct{})
	aliceFriendsRoot := aliceFriends.ElideRevealingSet(emptySet)
	assertActualExpected(t, aliceFriendsRoot.Format(), "ELIDED")

	// Now Alice wants to prove to the third party that her document contains a
	// "knows Bob" assertion.
	knowsBobAssertion := NewAssertionEnvelope("knows", "Bob")
	aliceKnowsBobProof := aliceFriends.ProofContainsTarget(knowsBobAssertion)
	if aliceKnowsBobProof == nil {
		t.Fatal("expected proof to be non-nil")
	}
	aliceKnowsBobProof = checkEncoding(t, aliceKnowsBobProof)

	assertActualExpected(t, aliceKnowsBobProof.Format(), `ELIDED [
    ELIDED [
        ELIDED
    ]
    ELIDED (2)
]`)

	// The third party then uses the previously known and trusted root to
	// confirm that the envelope does indeed contain a "knows bob" assertion.
	if !aliceFriendsRoot.ConfirmContainsTarget(knowsBobAssertion, aliceKnowsBobProof) {
		t.Fatal("expected confirmation to succeed")
	}
}

func TestMultiPosition(t *testing.T) {
	aliceFriends := NewEnvelope("Alice").
		AddAssertionSalted("knows", "Bob", true).
		AddAssertionSalted("knows", "Carol", true).
		AddAssertionSalted("knows", "Dan", true)

	// In some cases the target of a proof might exist at more than one position
	// in an envelope. An example target from Alice's list of friends would
	// be any envelope containing "knows" as its subject.
	knowsProof := aliceFriends.ProofContainsTarget(NewEnvelope("knows"))
	if knowsProof == nil {
		t.Fatal("expected proof to be non-nil")
	}
	knowsProof = checkEncoding(t, knowsProof)

	assertActualExpected(t, knowsProof.Format(), `ELIDED [
    {
        ELIDED: ELIDED
    } [
        ELIDED
    ]
    {
        ELIDED: ELIDED
    } [
        ELIDED
    ]
    {
        ELIDED: ELIDED
    } [
        ELIDED
    ]
]`)
}

func TestVerifiableCredential(t *testing.T) {
	aliceSeedData := mustDecodeHex("82f32c855d3d542256180810797e0073")
	alicePriv := bccomponents.PrivateKeyBaseFromData(aliceSeedData)

	aridData := mustDecodeHex("4676635a6e6068c2ef3ffd8ff726dd401fd341036e920f136a1d8af5e829496d")
	arid, err := bccomponents.ARIDFromDataRef(aridData)
	if err != nil {
		t.Fatalf("ARID creation failed: %v", err)
	}
	aridEnvelope := NewEnvelope(arid)

	birthDate, err := dcbor.DateFromString("1970-01-01")
	if err != nil {
		t.Fatalf("date parse failed: %v", err)
	}

	cred := aridEnvelope.
		AddAssertionSalted("firstName", "John", true).
		AddAssertionSalted("lastName", "Smith", true).
		AddAssertionSalted("address", "123 Main St.", true).
		AddAssertionSalted("birthDate", birthDate, true).
		AddAssertionSalted("photo", "This is John Smith's photo.", true).
		AddAssertionSalted("dlNumber", "123-456-789", true).
		AddAssertionSalted("nonCommercialVehicleEndorsement", true, true).
		AddAssertionSalted("motorocycleEndorsement", true, true).
		AddAssertion(knownvalues.Issuer, "State of Example").
		AddAssertion(knownvalues.Controller, "State of Example").
		Wrap().
		AddSignature(alicePriv.PrivateKeys()).
		AddAssertion(knownvalues.Note, "Signed by the State of Example")

	emptySet := make(map[bccomponents.Digest]struct{})
	credentialRoot := cred.ElideRevealingSet(emptySet)

	// In this case the holder of a credential wants to prove a single assertion
	// from it, the address.
	addressAssertion := NewAssertionEnvelope("address", "123 Main St.")
	addressProof := cred.ProofContainsTarget(addressAssertion)
	if addressProof == nil {
		t.Fatal("expected address proof to be non-nil")
	}
	addressProof = checkEncoding(t, addressProof)

	assertActualExpected(t, addressProof.Format(), `{
    ELIDED [
        ELIDED [
            ELIDED
        ]
        ELIDED (9)
    ]
} [
    ELIDED (2)
]`)

	// The proof confirms the address, as intended.
	if !credentialRoot.ConfirmContainsTarget(addressAssertion, addressProof) {
		t.Fatal("expected address confirmation to succeed")
	}

	// Assertions without salt can also be confirmed.
	issuerAssertion := NewAssertionEnvelope(knownvalues.Issuer, "State of Example")
	if !credentialRoot.ConfirmContainsTarget(issuerAssertion, addressProof) {
		t.Fatal("expected issuer confirmation to succeed")
	}

	// The proof cannot be used to confirm salted assertions.
	firstNameAssertion := NewAssertionEnvelope("firstName", "John")
	if credentialRoot.ConfirmContainsTarget(firstNameAssertion, addressProof) {
		t.Fatal("expected firstName confirmation to fail")
	}
}

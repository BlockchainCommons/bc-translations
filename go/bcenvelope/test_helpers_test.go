package bcenvelope

import (
	"encoding/hex"
	"testing"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

const plaintextHello = "Hello."

// --- Assert helpers ---

func assertActualExpected(t *testing.T, actual, expected string) {
	t.Helper()
	if actual != expected {
		t.Errorf("Mismatch:\nActual:\n%s\n\nExpected:\n%s", actual, expected)
	}
}

// --- Check encoding (CBOR round-trip) ---

func checkEncoding(t *testing.T, envelope *Envelope) *Envelope {
	t.Helper()
	cbor := envelope.TaggedCBOR()
	restored, err := EnvelopeFromTaggedCBORValue(cbor)
	if err != nil {
		t.Logf("=== EXPECTED\n%s\n=== GOT\n%s\n===", envelope.Format(), cbor.Diagnostic())
		t.Fatalf("Failed to decode: %v", err)
	}
	if !envelope.Digest().Equal(restored.Digest()) {
		t.Logf("=== EXPECTED\n%s\n=== GOT\n%s\n===", envelope.Format(), restored.Format())
		t.Fatalf("Digest mismatch after round-trip")
	}
	return envelope
}

// --- Test data functions ---

func helloEnvelope() *Envelope { return NewEnvelope(plaintextHello) }

func knownValueEnvelope() *Envelope { return NewEnvelope(knownvalues.Note) }

func assertionEnvelope() *Envelope {
	return NewAssertionEnvelope("knows", "Bob")
}

func singleAssertionEnvelope() *Envelope {
	return NewEnvelope("Alice").AddAssertion("knows", "Bob")
}

func doubleAssertionEnvelope() *Envelope {
	return singleAssertionEnvelope().AddAssertion("knows", "Carol")
}

func wrappedEnvelope() *Envelope { return helloEnvelope().Wrap() }

func doubleWrappedEnvelope() *Envelope { return wrappedEnvelope().Wrap() }

// --- Key functions ---

func mustDecodeHex(h string) []byte {
	data, err := hex.DecodeString(h)
	if err != nil {
		panic("mustDecodeHex: " + err.Error())
	}
	return data
}

func aliceSeed() []byte {
	return mustDecodeHex("82f32c855d3d542256180810797e0073")
}

func alicePrivateKey() *bccomponents.PrivateKeyBase {
	return bccomponents.PrivateKeyBaseFromData(aliceSeed())
}

func alicePublicKey() bccomponents.PublicKeys {
	return alicePrivateKey().PublicKeys()
}

func aliceSigner() bccomponents.SigningPrivateKey {
	return alicePrivateKey().SchnorrSigningPrivateKey()
}

func aliceVerifier() bccomponents.SigningPublicKey {
	return aliceSigner().PublicKey()
}

func bobSeed() []byte {
	return mustDecodeHex("187a5973c64d359c836eba466a44db7b")
}

func bobPrivateKey() *bccomponents.PrivateKeyBase {
	return bccomponents.PrivateKeyBaseFromData(bobSeed())
}

func bobPublicKey() bccomponents.PublicKeys {
	return bobPrivateKey().PublicKeys()
}

func carolSeed() []byte {
	return mustDecodeHex("8574afab18e229651c1be8f76ffee523")
}

func carolPrivateKey() *bccomponents.PrivateKeyBase {
	return bccomponents.PrivateKeyBaseFromData(carolSeed())
}

func carolPublicKey() bccomponents.PublicKeys {
	return carolPrivateKey().PublicKeys()
}

func fakeContentKey() bccomponents.SymmetricKey {
	var data [bccomponents.SymmetricKeySize]byte
	copy(data[:], mustDecodeHex("526afd95b2229c5381baec4a1788507a3c4a566ca5cce64543b46ad12aff0035"))
	return bccomponents.SymmetricKeyFromData(data)
}

func fakeNonce() bccomponents.Nonce {
	var data [bccomponents.NonceSize]byte
	copy(data[:], mustDecodeHex("4d785658f36c22fb5aed3ac0"))
	return bccomponents.NonceFromData(data)
}

// --- Credential ---

func credential() *Envelope {
	rng := bcrand.NewFakeRandomNumberGenerator()
	options := &bccomponents.SigningOptions{SchnorrRNG: rng}

	aridHex := "4676635a6e6068c2ef3ffd8ff726dd401fd341036e920f136a1d8af5e829496d"
	arid := bccomponents.ARIDFromHex(aridHex)

	issueDate, _ := dcbor.DateFromString("2020-01-01")
	expirationDate, _ := dcbor.DateFromString("2028-01-01")

	topicsArr := []dcbor.CBOR{
		dcbor.NewCBORText("Subject 1"),
		dcbor.NewCBORText("Subject 2"),
	}
	topicsCBOR := dcbor.NewCBORArray(topicsArr)

	e := NewEnvelope(arid).
		AddAssertion(knownvalues.IsA, "Certificate of Completion").
		AddAssertion(knownvalues.Issuer, "Example Electrical Engineering Board").
		AddAssertion(knownvalues.Controller, "Example Electrical Engineering Board").
		AddAssertion("firstName", "James").
		AddAssertion("lastName", "Maxwell").
		AddAssertion("issueDate", issueDate).
		AddAssertion("expirationDate", expirationDate).
		AddAssertion("photo", "This is James Maxwell's photo.").
		AddAssertion("certificateNumber", "123-456-789").
		AddAssertion("subject", "RF and Microwave Engineering").
		AddAssertion("continuingEducationUnits", 1).
		AddAssertion("professionalDevelopmentHours", 15).
		AddAssertion("topics", topicsCBOR)

	signer := alicePrivateKey().SchnorrSigningPrivateKey()
	e = e.Wrap().
		AddSignatureOpt(signer, options, nil).
		AddAssertion(knownvalues.Note, "Signed by Example Electrical Engineering Board")

	return e
}

func redactedCredential() *Envelope {
	cred := credential()
	target := make(map[bccomponents.Digest]struct{})

	// Include the credential envelope itself
	target[cred.Digest()] = struct{}{}

	// Include all assertions and their deep digests
	for _, assertion := range cred.Assertions() {
		for d := range assertion.DeepDigests() {
			target[d] = struct{}{}
		}
	}

	// Include the subject (the wrapped envelope)
	target[cred.Subject().Digest()] = struct{}{}

	// Unwrap the content
	content, _ := cred.Subject().TryUnwrap()
	target[content.Digest()] = struct{}{}
	target[content.Subject().Digest()] = struct{}{}

	// Include specific assertions by predicate
	firstNameAssertion, _ := content.AssertionWithPredicate("firstName")
	for d := range firstNameAssertion.ShallowDigests() {
		target[d] = struct{}{}
	}

	lastNameAssertion, _ := content.AssertionWithPredicate("lastName")
	for d := range lastNameAssertion.ShallowDigests() {
		target[d] = struct{}{}
	}

	isAAssertion, _ := content.AssertionWithPredicate(knownvalues.IsA)
	for d := range isAAssertion.ShallowDigests() {
		target[d] = struct{}{}
	}

	issuerAssertion, _ := content.AssertionWithPredicate(knownvalues.Issuer)
	for d := range issuerAssertion.ShallowDigests() {
		target[d] = struct{}{}
	}

	subjectAssertion, _ := content.AssertionWithPredicate("subject")
	for d := range subjectAssertion.ShallowDigests() {
		target[d] = struct{}{}
	}

	expirationDateAssertion, _ := content.AssertionWithPredicate("expirationDate")
	for d := range expirationDateAssertion.ShallowDigests() {
		target[d] = struct{}{}
	}

	return cred.ElideRevealingSet(target)
}

// --- Test Seed type ---

// TestSeed is a domain object used to test envelope type system round-tripping.
type TestSeed struct {
	data         []byte
	name         string
	note         string
	creationDate *dcbor.Date
}

func NewTestSeed(data []byte) *TestSeed {
	return &TestSeed{data: data}
}

func NewTestSeedOpt(data []byte, name, note string, creationDate *dcbor.Date) *TestSeed {
	return &TestSeed{
		data:         data,
		name:         name,
		note:         note,
		creationDate: creationDate,
	}
}

func (s *TestSeed) Data() []byte           { return s.data }
func (s *TestSeed) Name() string           { return s.name }
func (s *TestSeed) SetName(name string)    { s.name = name }
func (s *TestSeed) Note() string           { return s.note }
func (s *TestSeed) SetNote(note string)    { s.note = note }
func (s *TestSeed) CreationDate() *dcbor.Date { return s.creationDate }
func (s *TestSeed) SetCreationDate(d *dcbor.Date) { s.creationDate = d }

// TestSeed -> Envelope

func testSeedToEnvelope(seed *TestSeed) *Envelope {
	e := NewEnvelope(seed.data).
		AddType(knownvalues.SeedType)

	if seed.creationDate != nil {
		e = e.AddAssertion(knownvalues.Date, *seed.creationDate)
	}

	if seed.name != "" {
		e = e.AddAssertion(knownvalues.Name, seed.name)
	}

	if seed.note != "" {
		e = e.AddAssertion(knownvalues.Note, seed.note)
	}

	return e
}

// Envelope -> TestSeed

func testSeedFromEnvelope(envelope *Envelope) (*TestSeed, error) {
	if err := envelope.CheckTypeValue(knownvalues.SeedType); err != nil {
		return nil, err
	}

	data, err := ExtractSubjectBytes(envelope)
	if err != nil {
		return nil, err
	}

	name := ""
	nameObj, err := envelope.OptionalObjectForPredicate(knownvalues.Name)
	if err != nil {
		return nil, err
	}
	if nameObj != nil {
		name, err = ExtractSubject[string](nameObj)
		if err != nil {
			return nil, err
		}
	}

	note := ""
	noteObj, err := envelope.OptionalObjectForPredicate(knownvalues.Note)
	if err != nil {
		return nil, err
	}
	if noteObj != nil {
		note, err = ExtractSubject[string](noteObj)
		if err != nil {
			return nil, err
		}
	}

	var creationDate *dcbor.Date
	dateObj, err := envelope.OptionalObjectForPredicate(knownvalues.Date)
	if err != nil {
		return nil, err
	}
	if dateObj != nil {
		cbor, err := dateObj.TryLeaf()
		if err != nil {
			return nil, err
		}
		d, err := dcbor.DecodeDate(cbor)
		if err != nil {
			return nil, err
		}
		creationDate = &d
	}

	return NewTestSeedOpt(data, name, note, creationDate), nil
}

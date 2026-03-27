package bcenvelope

import (
	"bytes"
	"slices"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// EnvelopeCase is a sealed interface representing the structural variant of an Envelope.
// It is sealed by the unexported envelopeCase() method marker.
type EnvelopeCase interface {
	envelopeCase() // unexported marker — seals the interface
}

// NodeCase represents an envelope with a subject and one or more assertions.
type NodeCase struct {
	Subject    *Envelope
	Assertions []*Envelope
	Digest     *bccomponents.Digest
}

func (*NodeCase) envelopeCase() {}

// LeafCase represents an envelope containing a primitive CBOR value.
type LeafCase struct {
	CBOR   *dcbor.CBOR
	Digest *bccomponents.Digest
}

func (*LeafCase) envelopeCase() {}

// WrappedCase represents an envelope that wraps another envelope.
type WrappedCase struct {
	Envelope *Envelope
	Digest   *bccomponents.Digest
}

func (*WrappedCase) envelopeCase() {}

// AssertionCase represents a predicate-object assertion.
type AssertionCase struct {
	Assertion *Assertion
}

func (*AssertionCase) envelopeCase() {}

// ElidedCase represents an envelope that has been elided, leaving only its digest.
type ElidedCase struct {
	Digest *bccomponents.Digest
}

func (*ElidedCase) envelopeCase() {}

// KnownValueCase represents a known value envelope.
type KnownValueCase struct {
	Value  knownvalues.KnownValue
	Digest *bccomponents.Digest
}

func (*KnownValueCase) envelopeCase() {}

// EncryptedCase represents an encrypted envelope.
type EncryptedCase struct {
	EncryptedMessage *bccomponents.EncryptedMessage
}

func (*EncryptedCase) envelopeCase() {}

// CompressedCase represents a compressed envelope.
type CompressedCase struct {
	Compressed *bccomponents.Compressed
}

func (*CompressedCase) envelopeCase() {}

// Envelope is the main Gordian Envelope data structure.
// Envelopes are immutable; operations that appear to modify an envelope return a new one.
type Envelope struct {
	envelopeCase EnvelopeCase
}

// Case returns the underlying EnvelopeCase variant.
func (e *Envelope) Case() EnvelopeCase {
	return e.envelopeCase
}

// newFromCase constructs an Envelope from a given case.
func newFromCase(c EnvelopeCase) *Envelope {
	return &Envelope{envelopeCase: c}
}

// Envelope implements EnvelopeEncodable.
func (e *Envelope) Envelope() *Envelope { return e }

// --- Public constructors ---

// NewEnvelope creates an envelope from any supported subject type.
func NewEnvelope(subject any) *Envelope {
	return AsEnvelopeEncodable(subject).Envelope()
}

// NewEnvelopeOrNull creates an envelope from a subject. If subject is nil, returns a null envelope.
func NewEnvelopeOrNull(subject any) *Envelope {
	if subject == nil {
		return NullEnvelope()
	}
	return NewEnvelope(subject)
}

// NewAssertionEnvelope creates an assertion envelope with the given predicate and object.
func NewAssertionEnvelope(predicate, object any) *Envelope {
	return newWithAssertion(NewAssertion(AsEnvelopeEncodable(predicate), AsEnvelopeEncodable(object)))
}

// NewEncryptedEnvelope creates an envelope from an encrypted message.
// Panics if the encrypted message has no digest.
func NewEncryptedEnvelope(encrypted *bccomponents.EncryptedMessage) *Envelope {
	env, err := newWithEncrypted(encrypted)
	if err != nil {
		panic("NewEncryptedEnvelope: " + err.Error())
	}
	return env
}

// NewCompressedEnvelope creates an envelope from compressed data.
// Panics if the compressed data has no digest.
func NewCompressedEnvelope(compressed *bccomponents.Compressed) *Envelope {
	env, err := newWithCompressed(compressed)
	if err != nil {
		panic("NewCompressedEnvelope: " + err.Error())
	}
	return env
}

// --- Tag constants for use by extension files ---
var (
	TagEnvelope = dcbor.NewTag(tagEnvelopeValue, tagEnvelopeName)
	TagLeaf     = dcbor.NewTag(tagLeafValue, tagLeafName)
)

const (
	tagEnvelopeValue uint64 = 200
	tagEnvelopeName         = "envelope"
	tagLeafValue     uint64 = 201
	tagLeafName             = "leaf"
)

// --- Internal constructors ---

// newWithUncheckedAssertions creates a node envelope from a subject and sorted assertions.
func newWithUncheckedAssertions(subject *Envelope, uncheckedAssertions []*Envelope) *Envelope {
	if len(uncheckedAssertions) == 0 {
		panic("newWithUncheckedAssertions: assertions must not be empty")
	}
	sorted := make([]*Envelope, len(uncheckedAssertions))
	copy(sorted, uncheckedAssertions)
	slices.SortFunc(sorted, func(a, b *Envelope) int {
		return bytes.Compare(a.Digest().Bytes(), b.Digest().Bytes())
	})
	digests := make([]bccomponents.Digest, 0, 1+len(sorted))
	digests = append(digests, subject.Digest())
	for _, a := range sorted {
		digests = append(digests, a.Digest())
	}
	digest := bccomponents.DigestFromDigests(digests)
	return newFromCase(&NodeCase{
		Subject:    subject,
		Assertions: sorted,
		Digest:     &digest,
	})
}

// newWithAssertions validates assertions and creates a node envelope.
func newWithAssertions(subject *Envelope, assertions []*Envelope) (*Envelope, error) {
	for _, a := range assertions {
		if !a.IsSubjectAssertion() && !a.IsSubjectObscured() {
			return nil, ErrInvalidFormat
		}
	}
	return newWithUncheckedAssertions(subject, assertions), nil
}

func newWithAssertion(assertion *Assertion) *Envelope {
	return newFromCase(&AssertionCase{Assertion: assertion})
}

func newWithKnownValue(value knownvalues.KnownValue) *Envelope {
	digest := value.Digest()
	return newFromCase(&KnownValueCase{Value: value, Digest: &digest})
}

func newWithEncrypted(encrypted *bccomponents.EncryptedMessage) (*Envelope, error) {
	if !encrypted.HasDigest() {
		return nil, ErrMissingDigest
	}
	return newFromCase(&EncryptedCase{EncryptedMessage: encrypted}), nil
}

func newWithCompressed(compressed *bccomponents.Compressed) (*Envelope, error) {
	if !compressed.HasDigest() {
		return nil, ErrMissingDigest
	}
	return newFromCase(&CompressedCase{Compressed: compressed}), nil
}

func newElided(digest bccomponents.Digest) *Envelope {
	return newFromCase(&ElidedCase{Digest: &digest})
}

func newLeaf(cbor dcbor.CBOR) *Envelope {
	data := cbor.ToCBORData()
	digest := bccomponents.DigestFromImage(data)
	return newFromCase(&LeafCase{CBOR: &cbor, Digest: &digest})
}

func newWrapped(envelope *Envelope) *Envelope {
	digest := bccomponents.DigestFromDigests([]bccomponents.Digest{envelope.Digest()})
	return newFromCase(&WrappedCase{Envelope: envelope, Digest: &digest})
}

// FromTaggedCBOR decodes a tagged CBOR value into an Envelope. Alias for EnvelopeFromTaggedCBORValue.
func FromTaggedCBOR(cbor dcbor.CBOR) (*Envelope, error) {
	return EnvelopeFromTaggedCBORValue(cbor)
}

// FromTaggedCBORData decodes tagged CBOR bytes into an Envelope.
func FromTaggedCBORData(data []byte) (*Envelope, error) {
	return EnvelopeFromCBORData(data)
}

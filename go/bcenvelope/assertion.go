package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// Assertion is a predicate-object relationship representing an assertion about a subject.
type Assertion struct {
	predicate *Envelope
	object    *Envelope
	digest    *bccomponents.Digest
}

// NewAssertion creates a new Assertion with the given predicate and object.
// Arguments can be any type supported by AsEnvelopeEncodable.
func NewAssertion(predicate, object any) *Assertion {
	pred := AsEnvelopeEncodable(predicate).Envelope()
	obj := AsEnvelopeEncodable(object).Envelope()
	digest := bccomponents.DigestFromDigests([]bccomponents.Digest{pred.Digest(), obj.Digest()})
	return &Assertion{
		predicate: pred,
		object:    obj,
		digest:    &digest,
	}
}

// Predicate returns the predicate envelope.
func (a *Assertion) Predicate() *Envelope { return a.predicate }

// Object returns the object envelope.
func (a *Assertion) Object() *Envelope { return a.object }

// Digest returns a pointer to the assertion's digest.
func (a *Assertion) Digest() *bccomponents.Digest { return a.digest }

// DigestValue returns the assertion's digest as a value.
func (a *Assertion) DigestValue() bccomponents.Digest { return *a.digest }

// Equal compares two assertions by digest equality.
func (a *Assertion) Equal(other *Assertion) bool {
	if a == nil || other == nil {
		return a == other
	}
	return a.digest.Equal(*other.digest)
}

// ToCBOR converts the assertion to its CBOR representation: a single-element Map.
func (a *Assertion) ToCBOR() dcbor.CBOR {
	m := dcbor.NewMap()
	m.Insert(a.predicate.UntaggedCBOR(), a.object.UntaggedCBOR())
	return dcbor.NewCBORMap(m)
}

// TaggedCBOR returns the assertion's CBOR representation (same as ToCBOR since
// assertions are not independently tagged — they appear untagged inside envelopes).
func (a *Assertion) TaggedCBOR() dcbor.CBOR {
	return a.ToCBOR()
}

// ToEnvelope converts this assertion into an assertion envelope.
func (a *Assertion) ToEnvelope() *Envelope {
	return newWithAssertion(a)
}

// Assertion implements EnvelopeEncodable.
func (a *Assertion) Envelope() *Envelope {
	return newWithAssertion(a)
}

// AssertionFromCBOR decodes a CBOR value into an Assertion.
func AssertionFromCBOR(cbor dcbor.CBOR) (*Assertion, error) {
	m, ok := cbor.AsMap()
	if !ok {
		return nil, ErrInvalidAssertion
	}
	return AssertionFromMap(m)
}

// AssertionFromMap decodes a CBOR Map into an Assertion.
func AssertionFromMap(m dcbor.Map) (*Assertion, error) {
	if m.Len() != 1 {
		return nil, ErrInvalidAssertion
	}
	entries := m.AsEntries()
	entry := entries[0]
	predicate, err := EnvelopeFromUntaggedCBOR(entry.Key)
	if err != nil {
		return nil, err
	}
	object, err := EnvelopeFromUntaggedCBOR(entry.Value)
	if err != nil {
		return nil, err
	}
	return NewAssertion(EnvelopeEncodableEnvelope{predicate}, EnvelopeEncodableEnvelope{object}), nil
}

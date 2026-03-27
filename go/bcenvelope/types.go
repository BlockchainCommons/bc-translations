package bcenvelope

import (
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// AddType adds a type assertion to the envelope using the 'isA' predicate.
// The type can be any value that can be converted to an envelope (typically
// a string or a KnownValue).
func (e *Envelope) AddType(object any) *Envelope {
	return e.AddAssertion(knownvalues.IsA, object)
}

// Types returns all type objects from the envelope's 'isA' assertions.
func (e *Envelope) Types() []*Envelope {
	return e.ObjectsForPredicate(knownvalues.IsA)
}

// Type returns the single type object from the envelope's 'isA' assertions.
// Returns ErrAmbiguousType if the envelope has zero or multiple types.
func (e *Envelope) Type() (*Envelope, error) {
	t := e.Types()
	if len(t) == 1 {
		return t[0], nil
	}
	return nil, ErrAmbiguousType
}

// HasType checks if the envelope has a specific type by comparing digests.
func (e *Envelope) HasType(t any) bool {
	te := NewEnvelope(t)
	for _, x := range e.Types() {
		if x.Digest().Equal(te.Digest()) {
			return true
		}
	}
	return false
}

// HasTypeValue checks if the envelope has a specific KnownValue type.
func (e *Envelope) HasTypeValue(t knownvalues.KnownValue) bool {
	te := NewKnownValueEnvelope(t)
	for _, x := range e.Types() {
		if x.Digest().Equal(te.Digest()) {
			return true
		}
	}
	return false
}

// CheckTypeValue verifies that the envelope has a specific KnownValue type.
// Returns ErrInvalidType if the type is not found.
func (e *Envelope) CheckTypeValue(t knownvalues.KnownValue) error {
	if e.HasTypeValue(t) {
		return nil
	}
	return ErrInvalidType
}

// CheckType verifies that the envelope has a specific type. Returns
// ErrInvalidType if the type is not found.
func (e *Envelope) CheckType(t any) error {
	if e.HasType(t) {
		return nil
	}
	return ErrInvalidType
}

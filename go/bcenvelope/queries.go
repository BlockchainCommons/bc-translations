package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// --- Structural queries ---

// Subject returns the envelope's subject.
func (e *Envelope) Subject() *Envelope {
	if c, ok := e.envelopeCase.(*NodeCase); ok {
		return c.Subject
	}
	return e
}

// Assertions returns the envelope's assertions.
func (e *Envelope) Assertions() []*Envelope {
	if c, ok := e.envelopeCase.(*NodeCase); ok {
		result := make([]*Envelope, len(c.Assertions))
		copy(result, c.Assertions)
		return result
	}
	return nil
}

// HasAssertions returns true if the envelope has at least one assertion.
func (e *Envelope) HasAssertions() bool {
	if c, ok := e.envelopeCase.(*NodeCase); ok {
		return len(c.Assertions) > 0
	}
	return false
}

// --- Type queries ---

// IsAssertion reports whether the envelope is an assertion.
func (e *Envelope) IsAssertion() bool {
	_, ok := e.envelopeCase.(*AssertionCase)
	return ok
}

// IsEncrypted reports whether the envelope is encrypted.
func (e *Envelope) IsEncrypted() bool {
	_, ok := e.envelopeCase.(*EncryptedCase)
	return ok
}

// IsCompressed reports whether the envelope is compressed.
func (e *Envelope) IsCompressed() bool {
	_, ok := e.envelopeCase.(*CompressedCase)
	return ok
}

// IsElided reports whether the envelope is elided.
func (e *Envelope) IsElided() bool {
	_, ok := e.envelopeCase.(*ElidedCase)
	return ok
}

// IsLeaf reports whether the envelope is a leaf containing a CBOR value.
func (e *Envelope) IsLeaf() bool {
	_, ok := e.envelopeCase.(*LeafCase)
	return ok
}

// IsNode reports whether the envelope is a node (subject with assertions).
func (e *Envelope) IsNode() bool {
	_, ok := e.envelopeCase.(*NodeCase)
	return ok
}

// IsWrapped reports whether the envelope wraps another envelope.
func (e *Envelope) IsWrapped() bool {
	_, ok := e.envelopeCase.(*WrappedCase)
	return ok
}

// IsKnownValue reports whether the envelope contains a known value.
func (e *Envelope) IsKnownValue() bool {
	_, ok := e.envelopeCase.(*KnownValueCase)
	return ok
}

// --- Subject-level type queries ---

// IsSubjectAssertion reports whether the envelope's subject is an assertion.
func (e *Envelope) IsSubjectAssertion() bool {
	if _, ok := e.envelopeCase.(*AssertionCase); ok {
		return true
	}
	if c, ok := e.envelopeCase.(*NodeCase); ok {
		return c.Subject.IsSubjectAssertion()
	}
	return false
}

// IsSubjectEncrypted reports whether the envelope's subject is encrypted.
func (e *Envelope) IsSubjectEncrypted() bool {
	if _, ok := e.envelopeCase.(*EncryptedCase); ok {
		return true
	}
	if c, ok := e.envelopeCase.(*NodeCase); ok {
		return c.Subject.IsSubjectEncrypted()
	}
	return false
}

// IsSubjectCompressed reports whether the envelope's subject is compressed.
func (e *Envelope) IsSubjectCompressed() bool {
	if _, ok := e.envelopeCase.(*CompressedCase); ok {
		return true
	}
	if c, ok := e.envelopeCase.(*NodeCase); ok {
		return c.Subject.IsSubjectCompressed()
	}
	return false
}

// IsSubjectElided reports whether the envelope's subject is elided.
func (e *Envelope) IsSubjectElided() bool {
	if _, ok := e.envelopeCase.(*ElidedCase); ok {
		return true
	}
	if c, ok := e.envelopeCase.(*NodeCase); ok {
		return c.Subject.IsSubjectElided()
	}
	return false
}

// IsSubjectObscured reports whether the envelope's subject is elided, encrypted, or compressed.
func (e *Envelope) IsSubjectObscured() bool {
	return e.IsSubjectElided() || e.IsSubjectEncrypted() || e.IsSubjectCompressed()
}

// IsInternal reports whether the envelope is a node, wrapped, or assertion case.
func (e *Envelope) IsInternal() bool {
	switch e.envelopeCase.(type) {
	case *NodeCase, *WrappedCase, *AssertionCase:
		return true
	default:
		return false
	}
}

// IsObscured reports whether the envelope is elided, encrypted, or compressed.
func (e *Envelope) IsObscured() bool {
	return e.IsElided() || e.IsEncrypted() || e.IsCompressed()
}

// --- Assertion access ---

// AsAssertion returns the envelope if it is an assertion, or nil otherwise.
func (e *Envelope) AsAssertion() *Envelope {
	if _, ok := e.envelopeCase.(*AssertionCase); ok {
		return e
	}
	return nil
}

// TryAssertion returns the envelope if it is an assertion, or ErrNotAssertion.
func (e *Envelope) TryAssertion() (*Envelope, error) {
	if a := e.AsAssertion(); a != nil {
		return a, nil
	}
	return nil, ErrNotAssertion
}

// AsPredicate returns the assertion's predicate if the subject is an assertion, or nil.
func (e *Envelope) AsPredicate() *Envelope {
	if c, ok := e.Subject().envelopeCase.(*AssertionCase); ok {
		return c.Assertion.Predicate()
	}
	return nil
}

// TryPredicate returns the assertion's predicate if the subject is an assertion, or ErrNotAssertion.
func (e *Envelope) TryPredicate() (*Envelope, error) {
	if p := e.AsPredicate(); p != nil {
		return p, nil
	}
	return nil, ErrNotAssertion
}

// AsObject returns the assertion's object if the subject is an assertion, or nil.
func (e *Envelope) AsObject() *Envelope {
	if c, ok := e.Subject().envelopeCase.(*AssertionCase); ok {
		return c.Assertion.Object()
	}
	return nil
}

// TryObject returns the assertion's object if the subject is an assertion, or ErrNotAssertion.
func (e *Envelope) TryObject() (*Envelope, error) {
	if o := e.AsObject(); o != nil {
		return o, nil
	}
	return nil, ErrNotAssertion
}

// AsLeaf returns the CBOR value if the envelope is a leaf, or nil.
func (e *Envelope) AsLeaf() *dcbor.CBOR {
	if c, ok := e.envelopeCase.(*LeafCase); ok {
		return c.CBOR
	}
	return nil
}

// TryLeaf returns the CBOR value if the envelope is a leaf, or ErrNotLeaf.
func (e *Envelope) TryLeaf() (dcbor.CBOR, error) {
	if l := e.AsLeaf(); l != nil {
		return *l, nil
	}
	return dcbor.CBOR{}, ErrNotLeaf
}

// AsKnownValue returns the known value if the envelope contains one, or nil.
func (e *Envelope) AsKnownValue() *knownvalues.KnownValue {
	if c, ok := e.envelopeCase.(*KnownValueCase); ok {
		return &c.Value
	}
	return nil
}

// TryKnownValue returns the known value if the envelope contains one, or ErrNotKnownValue.
func (e *Envelope) TryKnownValue() (knownvalues.KnownValue, error) {
	if kv := e.AsKnownValue(); kv != nil {
		return *kv, nil
	}
	return knownvalues.KnownValue{}, ErrNotKnownValue
}

// --- Assertion predicate queries ---

// AssertionsWithPredicate returns all assertions with the given predicate.
func (e *Envelope) AssertionsWithPredicate(predicate any) []*Envelope {
	predicateEnv := AsEnvelopeEncodable(predicate).Envelope()
	predicateDigest := predicateEnv.Digest()
	var result []*Envelope
	for _, assertion := range e.Assertions() {
		if p := assertion.Subject().AsPredicate(); p != nil {
			if p.Digest().Equal(predicateDigest) {
				result = append(result, assertion)
			}
		}
	}
	return result
}

// AssertionWithPredicate returns the single assertion with the given predicate.
// Returns ErrNonexistentPredicate or ErrAmbiguousPredicate if not exactly one.
func (e *Envelope) AssertionWithPredicate(predicate any) (*Envelope, error) {
	a := e.AssertionsWithPredicate(predicate)
	if len(a) == 0 {
		return nil, ErrNonexistentPredicate
	}
	if len(a) > 1 {
		return nil, ErrAmbiguousPredicate
	}
	return a[0], nil
}

// OptionalAssertionWithPredicate returns the assertion with the given predicate,
// or nil if none exists. Returns ErrAmbiguousPredicate if more than one exists.
func (e *Envelope) OptionalAssertionWithPredicate(predicate any) (*Envelope, error) {
	a := e.AssertionsWithPredicate(predicate)
	if len(a) == 0 {
		return nil, nil
	}
	if len(a) > 1 {
		return nil, ErrAmbiguousPredicate
	}
	return a[0], nil
}

// ObjectForPredicate returns the object of the single assertion with the given predicate.
func (e *Envelope) ObjectForPredicate(predicate any) (*Envelope, error) {
	assertion, err := e.AssertionWithPredicate(predicate)
	if err != nil {
		return nil, err
	}
	return assertion.AsObject(), nil
}

// OptionalObjectForPredicate returns the object of the assertion with the given predicate,
// or nil if none exists. Returns ErrAmbiguousPredicate if more than one exists.
func (e *Envelope) OptionalObjectForPredicate(predicate any) (*Envelope, error) {
	a := e.AssertionsWithPredicate(predicate)
	if len(a) == 0 {
		return nil, nil
	}
	if len(a) > 1 {
		return nil, ErrAmbiguousPredicate
	}
	return a[0].Subject().AsObject(), nil
}

// ObjectsForPredicate returns the objects of all assertions with the given predicate.
func (e *Envelope) ObjectsForPredicate(predicate any) []*Envelope {
	assertions := e.AssertionsWithPredicate(predicate)
	result := make([]*Envelope, len(assertions))
	for i, a := range assertions {
		result[i] = a.AsObject()
	}
	return result
}

// ElementsCount returns the number of structural elements in the envelope.
func (e *Envelope) ElementsCount() int {
	result := 1
	switch c := e.envelopeCase.(type) {
	case *NodeCase:
		result += c.Subject.ElementsCount()
		for _, a := range c.Assertions {
			result += a.ElementsCount()
		}
	case *AssertionCase:
		result += c.Assertion.Predicate().ElementsCount()
		result += c.Assertion.Object().ElementsCount()
	case *WrappedCase:
		result += c.Envelope.ElementsCount()
	}
	return result
}

// --- Leaf helpers ---

// FalseEnvelope returns an envelope containing CBOR false.
func FalseEnvelope() *Envelope { return newLeaf(dcbor.False()) }

// TrueEnvelope returns an envelope containing CBOR true.
func TrueEnvelope() *Envelope { return newLeaf(dcbor.True()) }

// NullEnvelope returns an envelope containing CBOR null.
func NullEnvelope() *Envelope { return newLeaf(dcbor.Null()) }

// UnitEnvelope returns an envelope containing the 'Unit' known value.
func UnitEnvelope() *Envelope { return EnvelopeFromKnownValue(knownvalues.Unit) }

// IsFalse reports whether the envelope's subject is CBOR false.
func (e *Envelope) IsFalse() bool {
	b, err := ExtractSubjectBool(e)
	return err == nil && !b
}

// IsTrue reports whether the envelope's subject is CBOR true.
func (e *Envelope) IsTrue() bool {
	b, err := ExtractSubjectBool(e)
	return err == nil && b
}

// IsBool reports whether the envelope's subject is a CBOR boolean.
func (e *Envelope) IsBool() bool {
	_, err := ExtractSubjectBool(e)
	return err == nil
}

// IsNull reports whether the envelope's subject is CBOR null.
func (e *Envelope) IsNull() bool {
	if l := e.AsLeaf(); l != nil {
		sv, ok := l.AsSimpleValue()
		if ok && sv.Kind() == dcbor.SimpleNull {
			return true
		}
	}
	return false
}

// IsNumber reports whether the envelope's subject is a CBOR number.
func (e *Envelope) IsNumber() bool {
	if l := e.AsLeaf(); l != nil {
		return l.IsNumber()
	}
	return false
}

// IsSubjectNumber reports whether the envelope's subject is a CBOR number.
func (e *Envelope) IsSubjectNumber() bool { return e.Subject().IsNumber() }

// IsNaN reports whether the envelope's subject is CBOR NaN.
func (e *Envelope) IsNaN() bool {
	if l := e.AsLeaf(); l != nil {
		return l.IsNaN()
	}
	return false
}

// IsSubjectNaN reports whether the envelope's subject is CBOR NaN.
func (e *Envelope) IsSubjectNaN() bool { return e.Subject().IsNaN() }

// TryByteString returns the byte string from a leaf envelope, or an error.
func (e *Envelope) TryByteString() ([]byte, error) {
	cbor, err := e.TryLeaf()
	if err != nil {
		return nil, err
	}
	return cbor.TryIntoByteString()
}

// AsByteString returns the byte string from a leaf envelope, or nil.
func (e *Envelope) AsByteString() []byte {
	if l := e.AsLeaf(); l != nil {
		bs, ok := l.IntoByteString()
		if ok {
			return bs
		}
	}
	return nil
}

// AsArray returns the CBOR array from a leaf envelope, or nil.
func (e *Envelope) AsArray() []dcbor.CBOR {
	if l := e.AsLeaf(); l != nil {
		arr, ok := l.IntoArray()
		if ok {
			return arr
		}
	}
	return nil
}

// AsMap returns the CBOR map from a leaf envelope, or nil.
func (e *Envelope) AsMap() *dcbor.Map {
	if l := e.AsLeaf(); l != nil {
		m, ok := l.IntoMap()
		if ok {
			return &m
		}
	}
	return nil
}

// AsText returns the text string from a leaf envelope using the comma-ok idiom.
func (e *Envelope) AsText() (string, bool) {
	if l := e.AsLeaf(); l != nil {
		return l.IntoText()
	}
	return "", false
}

// IsSubjectUnit reports whether the envelope's subject is the 'Unit' known value.
func (e *Envelope) IsSubjectUnit() bool {
	kv, err := ExtractSubjectKnownValue(e)
	if err != nil {
		return false
	}
	return kv.Equal(knownvalues.Unit)
}

// CheckSubjectUnit returns ErrSubjectNotUnit if the subject is not the 'Unit' known value.
func (e *Envelope) CheckSubjectUnit() error {
	if e.IsSubjectUnit() {
		return nil
	}
	return ErrSubjectNotUnit
}

// SetPosition adds or replaces a Position assertion.
func (e *Envelope) SetPosition(position int) (*Envelope, error) {
	posAssertions := e.AssertionsWithPredicate(knownvalues.Position)
	if len(posAssertions) > 1 {
		return nil, ErrInvalidFormat
	}
	result := e
	if len(posAssertions) == 1 {
		result = result.RemoveAssertion(posAssertions[0])
	}
	result = result.AddAssertion(knownvalues.Position, EnvelopeEncodableInt{Value: position})
	return result, nil
}

// Position retrieves the position value.
func (e *Envelope) Position() (int, error) {
	obj, err := e.ObjectForPredicate(knownvalues.Position)
	if err != nil {
		return 0, err
	}
	return ExtractSubjectInt(obj)
}

// RemovePosition removes the Position assertion.
func (e *Envelope) RemovePosition() (*Envelope, error) {
	posAssertions := e.AssertionsWithPredicate(knownvalues.Position)
	if len(posAssertions) > 1 {
		return nil, ErrInvalidFormat
	}
	if len(posAssertions) == 1 {
		return e.RemoveAssertion(posAssertions[0]), nil
	}
	return e, nil
}

// --- Digest method ---

// Digest returns the envelope's digest.
func (e *Envelope) Digest() bccomponents.Digest {
	switch c := e.envelopeCase.(type) {
	case *NodeCase:
		return *c.Digest
	case *LeafCase:
		return *c.Digest
	case *WrappedCase:
		return *c.Digest
	case *AssertionCase:
		return *c.Assertion.Digest()
	case *ElidedCase:
		return *c.Digest
	case *KnownValueCase:
		return *c.Digest
	case *EncryptedCase:
		return c.EncryptedMessage.Digest()
	case *CompressedCase:
		return c.Compressed.Digest()
	default:
		panic("unknown EnvelopeCase")
	}
}

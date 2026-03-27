package bcenvelope

// SignatureMetadata provides a way to attach additional information to
// signatures, such as the signer's identity, the signing date, or the
// purpose of the signature. When used with the signature extension, this
// metadata is included in a structured way that is also signed, ensuring
// the metadata cannot be tampered with without invalidating the signature.
type SignatureMetadata struct {
	assertions []*Assertion
}

// NewSignatureMetadata creates a new, empty SignatureMetadata instance.
func NewSignatureMetadata() *SignatureMetadata {
	return &SignatureMetadata{assertions: nil}
}

// NewSignatureMetadataWithAssertions creates a new SignatureMetadata with the
// specified assertions.
func NewSignatureMetadataWithAssertions(assertions []*Assertion) *SignatureMetadata {
	cp := make([]*Assertion, len(assertions))
	copy(cp, assertions)
	return &SignatureMetadata{assertions: cp}
}

// Assertions returns the assertions contained in this metadata.
func (m *SignatureMetadata) Assertions() []*Assertion {
	return m.assertions
}

// AddAssertion adds an assertion to this metadata and returns the modified
// metadata for chaining.
func (m *SignatureMetadata) AddAssertion(assertion *Assertion) *SignatureMetadata {
	m.assertions = append(m.assertions, assertion)
	return m
}

// WithAssertion adds a new assertion from the provided predicate and object
// and returns the modified metadata for chaining.
func (m *SignatureMetadata) WithAssertion(predicate, object any) *SignatureMetadata {
	return m.AddAssertion(NewAssertion(NewEnvelope(predicate), NewEnvelope(object)))
}

// HasAssertions returns whether this metadata contains any assertions.
func (m *SignatureMetadata) HasAssertions() bool {
	return len(m.assertions) > 0
}

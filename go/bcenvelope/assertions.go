package bcenvelope

// AddAssertion returns a new envelope with an assertion added.
// predicate and object can be any type supported by AsEnvelopeEncodable.
func (e *Envelope) AddAssertion(predicate, object any) *Envelope {
	assertion := NewAssertionEnvelope(AsEnvelopeEncodable(predicate), AsEnvelopeEncodable(object))
	result, _ := e.AddOptionalAssertionEnvelope(assertion)
	return result
}

// AddAssertionEnvelope adds a pre-constructed assertion envelope.
func (e *Envelope) AddAssertionEnvelope(assertionEnvelope EnvelopeEncodable) (*Envelope, error) {
	return e.AddOptionalAssertionEnvelope(assertionEnvelope.Envelope())
}

// AddAssertionEnvelopes adds multiple assertion envelopes.
func (e *Envelope) AddAssertionEnvelopes(assertions []*Envelope) (*Envelope, error) {
	result := e
	for _, a := range assertions {
		var err error
		result, err = result.AddAssertionEnvelope(EnvelopeEncodableEnvelope{a})
		if err != nil {
			return nil, err
		}
	}
	return result, nil
}

// AddOptionalAssertionEnvelope adds an assertion envelope if not nil and not duplicate.
func (e *Envelope) AddOptionalAssertionEnvelope(assertion *Envelope) (*Envelope, error) {
	if assertion == nil {
		return e, nil
	}
	if !assertion.IsSubjectAssertion() && !assertion.IsSubjectObscured() {
		return nil, ErrInvalidFormat
	}
	if c, ok := e.envelopeCase.(*NodeCase); ok {
		for _, existing := range c.Assertions {
			if existing.Digest().Equal(assertion.Digest()) {
				return e, nil
			}
		}
		assertions := make([]*Envelope, len(c.Assertions)+1)
		copy(assertions, c.Assertions)
		assertions[len(c.Assertions)] = assertion
		return newWithUncheckedAssertions(c.Subject, assertions), nil
	}
	return newWithUncheckedAssertions(e.Subject(), []*Envelope{assertion}), nil
}

// AddOptionalAssertion adds assertion with optional object. If object is nil, returns unchanged.
func (e *Envelope) AddOptionalAssertion(predicate any, object any) *Envelope {
	if object == nil {
		return e
	}
	return e.AddAssertion(predicate, object)
}

// AddNonemptyStringAssertion adds assertion only if string is non-empty.
func (e *Envelope) AddNonemptyStringAssertion(predicate any, str string) *Envelope {
	if str == "" {
		return e
	}
	return e.AddAssertion(predicate, str)
}

// AddAssertions adds an array of assertion envelopes (ignoring errors).
func (e *Envelope) AddAssertions(envelopes []*Envelope) *Envelope {
	result := e
	for _, env := range envelopes {
		r, err := result.AddAssertionEnvelope(EnvelopeEncodableEnvelope{env})
		if err == nil {
			result = r
		}
	}
	return result
}

// AddAssertionIf adds assertion only if condition is true.
func (e *Envelope) AddAssertionIf(condition bool, predicate, object any) *Envelope {
	if condition {
		return e.AddAssertion(predicate, object)
	}
	return e
}

// AddAssertionEnvelopeIf adds assertion envelope only if condition is true.
func (e *Envelope) AddAssertionEnvelopeIf(condition bool, assertionEnvelope *Envelope) (*Envelope, error) {
	if condition {
		return e.AddAssertionEnvelope(EnvelopeEncodableEnvelope{assertionEnvelope})
	}
	return e, nil
}

// --- Salted assertion methods ---

// AddAssertionSalted adds an assertion, optionally salting it.
func (e *Envelope) AddAssertionSalted(predicate, object any, salted bool) *Envelope {
	assertion := NewAssertionEnvelope(AsEnvelopeEncodable(predicate), AsEnvelopeEncodable(object))
	result, _ := e.AddOptionalAssertionEnvelopeSalted(assertion, salted)
	return result
}

// AddAssertionEnvelopeSalted adds an assertion envelope, optionally salting it.
func (e *Envelope) AddAssertionEnvelopeSalted(assertionEnvelope *Envelope, salted bool) (*Envelope, error) {
	return e.AddOptionalAssertionEnvelopeSalted(assertionEnvelope, salted)
}

// AddOptionalAssertionEnvelopeSalted adds an optional assertion, optionally salting it.
func (e *Envelope) AddOptionalAssertionEnvelopeSalted(assertion *Envelope, salted bool) (*Envelope, error) {
	if assertion == nil {
		return e, nil
	}
	if !assertion.IsSubjectAssertion() && !assertion.IsSubjectObscured() {
		return nil, ErrInvalidFormat
	}
	assertionToAdd := assertion
	if salted {
		assertionToAdd = assertionToAdd.AddSalt()
	}
	if c, ok := e.envelopeCase.(*NodeCase); ok {
		for _, existing := range c.Assertions {
			if existing.Digest().Equal(assertionToAdd.Digest()) {
				return e, nil
			}
		}
		assertions := make([]*Envelope, len(c.Assertions)+1)
		copy(assertions, c.Assertions)
		assertions[len(c.Assertions)] = assertionToAdd
		return newWithUncheckedAssertions(c.Subject, assertions), nil
	}
	return newWithUncheckedAssertions(e.Subject(), []*Envelope{assertionToAdd}), nil
}

// AddAssertionsSalted adds multiple assertion envelopes, optionally salting each.
func (e *Envelope) AddAssertionsSalted(assertions []*Envelope, salted bool) *Envelope {
	result := e
	for _, a := range assertions {
		r, err := result.AddAssertionEnvelopeSalted(a, salted)
		if err == nil {
			result = r
		}
	}
	return result
}

// --- Remove / Replace ---

// RemoveAssertion removes the assertion with the matching digest.
func (e *Envelope) RemoveAssertion(target *Envelope) *Envelope {
	assertions := e.Assertions()
	targetDigest := target.Digest()
	idx := -1
	for i, a := range assertions {
		if a.Digest().Equal(targetDigest) {
			idx = i
			break
		}
	}
	if idx < 0 {
		return e
	}
	remaining := make([]*Envelope, 0, len(assertions)-1)
	remaining = append(remaining, assertions[:idx]...)
	remaining = append(remaining, assertions[idx+1:]...)
	if len(remaining) == 0 {
		return e.Subject()
	}
	return newWithUncheckedAssertions(e.Subject(), remaining)
}

// ReplaceAssertion replaces one assertion with another.
func (e *Envelope) ReplaceAssertion(assertion, newAssertion *Envelope) (*Envelope, error) {
	return e.RemoveAssertion(assertion).AddAssertionEnvelope(EnvelopeEncodableEnvelope{newAssertion})
}

// ReplaceSubject returns a new envelope with its subject replaced, preserving all assertions.
func (e *Envelope) ReplaceSubject(subject *Envelope) *Envelope {
	result := subject
	for _, a := range e.Assertions() {
		r, err := result.AddAssertionEnvelope(EnvelopeEncodableEnvelope{a})
		if err == nil {
			result = r
		}
	}
	return result
}

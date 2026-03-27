package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
)

// ObscureType identifies the type of obscuration applied to an element.
type ObscureType int

const (
	ObscureTypeElided     ObscureType = iota
	ObscureTypeEncrypted
	ObscureTypeCompressed
)

// ObscureAction specifies what to do when obscuring an element.
type ObscureAction int

const (
	ObscureActionElide    ObscureAction = iota
	ObscureActionCompress
	ObscureActionEncrypt
)

// ObscureActionEncryptWithKey holds a symmetric key for encryption-based obscuration.
type ObscureActionEncryptWithKey struct {
	Key bccomponents.SymmetricKey
}

// --- Elision ---

// Elide returns the elided variant of this envelope (only its digest remains).
func (e *Envelope) Elide() *Envelope {
	if _, ok := e.envelopeCase.(*ElidedCase); ok {
		return e
	}
	return newElided(e.Digest())
}

// --- Set-based elision ---

// ElideRemovingSet elides elements whose digests are in the target set.
func (e *Envelope) ElideRemovingSet(target map[bccomponents.Digest]struct{}) *Envelope {
	return e.ElideSet(target, false)
}

// ElideRevealingSet elides elements whose digests are not in the target set.
func (e *Envelope) ElideRevealingSet(target map[bccomponents.Digest]struct{}) *Envelope {
	return e.ElideSet(target, true)
}

// ElideSet elides elements based on the target set and revealing flag.
func (e *Envelope) ElideSet(target map[bccomponents.Digest]struct{}, isRevealing bool) *Envelope {
	return e.ElideSetWithAction(target, isRevealing, ObscureActionElide, nil)
}

// ElideSetWithAction applies obscuration based on the target set using the given action.
func (e *Envelope) ElideSetWithAction(
	target map[bccomponents.Digest]struct{},
	isRevealing bool,
	action ObscureAction,
	encryptKey *ObscureActionEncryptWithKey,
) *Envelope {
	selfDigest := e.Digest()
	_, inTarget := target[selfDigest]
	shouldObscure := inTarget != isRevealing

	if shouldObscure {
		switch action {
		case ObscureActionElide:
			return e.Elide()
		case ObscureActionCompress:
			compressed, err := e.Compress()
			if err != nil {
				return e.Elide()
			}
			return compressed
		case ObscureActionEncrypt:
			if encryptKey != nil {
				data := e.TaggedCBOR().ToCBORData()
				msg := encryptKey.Key.EncryptWithDigest(data, selfDigest, nil)
				env, err := newWithEncrypted(&msg)
				if err != nil {
					return e.Elide()
				}
				return env
			}
			return e.Elide()
		default:
			return e.Elide()
		}
	}

	// Not obscuring this node — recurse into children
	switch c := e.envelopeCase.(type) {
	case *AssertionCase:
		pred := c.Assertion.Predicate().ElideSetWithAction(target, isRevealing, action, encryptKey)
		obj := c.Assertion.Object().ElideSetWithAction(target, isRevealing, action, encryptKey)
		elidedAssertion := NewAssertion(
			EnvelopeEncodableEnvelope{pred},
			EnvelopeEncodableEnvelope{obj},
		)
		return newWithAssertion(elidedAssertion)
	case *NodeCase:
		elidedSubject := c.Subject.ElideSetWithAction(target, isRevealing, action, encryptKey)
		elidedAssertions := make([]*Envelope, len(c.Assertions))
		for i, a := range c.Assertions {
			elidedAssertions[i] = a.ElideSetWithAction(target, isRevealing, action, encryptKey)
		}
		return newWithUncheckedAssertions(elidedSubject, elidedAssertions)
	case *WrappedCase:
		elidedEnvelope := c.Envelope.ElideSetWithAction(target, isRevealing, action, encryptKey)
		return newWrapped(elidedEnvelope)
	default:
		return e
	}
}

// --- Target convenience methods ---

// ElideRemovingTarget elides the single target element.
func (e *Envelope) ElideRemovingTarget(target DigestProvider) *Envelope {
	return e.ElideRemovingSet(digestSet(target.Digest()))
}

// ElideRevealingTarget reveals only the single target element, eliding everything else.
func (e *Envelope) ElideRevealingTarget(target DigestProvider) *Envelope {
	return e.ElideRevealingSet(digestSet(target.Digest()))
}

// ElideRemovingArray elides elements whose digests are in the targets slice.
func (e *Envelope) ElideRemovingArray(targets []DigestProvider) *Envelope {
	return e.ElideRemovingSet(digestSetFromArray(targets))
}

// ElideRevealingArray reveals only the target elements, eliding everything else.
func (e *Envelope) ElideRevealingArray(targets []DigestProvider) *Envelope {
	return e.ElideRevealingSet(digestSetFromArray(targets))
}

// ElideArrayWithAction obscures array elements using the given action and reveal direction.
func (e *Envelope) ElideArrayWithAction(
	targets []DigestProvider,
	isRevealing bool,
	action ObscureAction,
	encryptKey *ObscureActionEncryptWithKey,
) *Envelope {
	return e.ElideSetWithAction(digestSetFromArray(targets), isRevealing, action, encryptKey)
}

// ElideArray elides array elements using the given reveal direction.
func (e *Envelope) ElideArray(targets []DigestProvider, isRevealing bool) *Envelope {
	return e.ElideArrayWithAction(targets, isRevealing, ObscureActionElide, nil)
}

// ElideTargetWithAction obscures a single target using the given action and reveal direction.
func (e *Envelope) ElideTargetWithAction(
	target DigestProvider,
	isRevealing bool,
	action ObscureAction,
	encryptKey *ObscureActionEncryptWithKey,
) *Envelope {
	return e.ElideArrayWithAction([]DigestProvider{target}, isRevealing, action, encryptKey)
}

// ElideTarget elides a single target using the given reveal direction.
func (e *Envelope) ElideTarget(target DigestProvider, isRevealing bool) *Envelope {
	return e.ElideTargetWithAction(target, isRevealing, ObscureActionElide, nil)
}

// ElideRemovingArrayWithAction removes array elements using the given action.
func (e *Envelope) ElideRemovingArrayWithAction(
	targets []DigestProvider,
	action ObscureAction,
	encryptKey *ObscureActionEncryptWithKey,
) *Envelope {
	return e.ElideArrayWithAction(targets, false, action, encryptKey)
}

// ElideRevealingArrayWithAction reveals array elements, obscuring everything else.
func (e *Envelope) ElideRevealingArrayWithAction(
	targets []DigestProvider,
	action ObscureAction,
	encryptKey *ObscureActionEncryptWithKey,
) *Envelope {
	return e.ElideArrayWithAction(targets, true, action, encryptKey)
}

// ElideRemovingTargetWithAction removes a single target using the given action.
func (e *Envelope) ElideRemovingTargetWithAction(
	target DigestProvider,
	action ObscureAction,
	encryptKey *ObscureActionEncryptWithKey,
) *Envelope {
	return e.ElideTargetWithAction(target, false, action, encryptKey)
}

// ElideRevealingTargetWithAction reveals a single target, obscuring everything else.
func (e *Envelope) ElideRevealingTargetWithAction(
	target DigestProvider,
	action ObscureAction,
	encryptKey *ObscureActionEncryptWithKey,
) *Envelope {
	return e.ElideTargetWithAction(target, true, action, encryptKey)
}

// --- Unelide ---

// Unelide replaces this elided envelope with the provided envelope if their digests match.
func (e *Envelope) Unelide(envelope *Envelope) (*Envelope, error) {
	if e.Digest().Equal(envelope.Digest()) {
		return envelope, nil
	}
	return nil, ErrInvalidDigest
}

// --- Walk-based operations ---

// WalkUnelide recursively walks the envelope and replaces elided nodes with
// matching envelopes from the provided slice.
func (e *Envelope) WalkUnelide(envelopes []*Envelope) *Envelope {
	envelopeMap := make(map[bccomponents.Digest]*Envelope)
	for _, env := range envelopes {
		envelopeMap[env.Digest()] = env
	}
	return e.walkUnelideWithMap(envelopeMap)
}

func (e *Envelope) walkUnelideWithMap(envelopeMap map[bccomponents.Digest]*Envelope) *Envelope {
	switch c := e.envelopeCase.(type) {
	case *ElidedCase:
		if replacement, ok := envelopeMap[e.Digest()]; ok {
			return replacement
		}
		return e
	case *NodeCase:
		newSubject := c.Subject.walkUnelideWithMap(envelopeMap)
		newAssertions := make([]*Envelope, len(c.Assertions))
		changed := !newSubject.IsIdenticalTo(c.Subject)
		for i, a := range c.Assertions {
			newAssertions[i] = a.walkUnelideWithMap(envelopeMap)
			if !newAssertions[i].IsIdenticalTo(a) {
				changed = true
			}
		}
		if !changed {
			return e
		}
		return newWithUncheckedAssertions(newSubject, newAssertions)
	case *WrappedCase:
		newEnv := c.Envelope.walkUnelideWithMap(envelopeMap)
		if newEnv.IsIdenticalTo(c.Envelope) {
			return e
		}
		return newEnv.Wrap()
	case *AssertionCase:
		newPred := c.Assertion.Predicate().walkUnelideWithMap(envelopeMap)
		newObj := c.Assertion.Object().walkUnelideWithMap(envelopeMap)
		if newPred.IsIdenticalTo(c.Assertion.Predicate()) && newObj.IsIdenticalTo(c.Assertion.Object()) {
			return e
		}
		return NewAssertionEnvelope(EnvelopeEncodableEnvelope{newPred}, EnvelopeEncodableEnvelope{newObj})
	default:
		return e
	}
}

// WalkReplace recursively walks the envelope and replaces nodes whose digests
// are in the target set with the replacement envelope.
func (e *Envelope) WalkReplace(target map[bccomponents.Digest]struct{}, replacement *Envelope) (*Envelope, error) {
	if _, ok := target[e.Digest()]; ok {
		return replacement, nil
	}
	switch c := e.envelopeCase.(type) {
	case *NodeCase:
		newSubject, err := c.Subject.WalkReplace(target, replacement)
		if err != nil {
			return nil, err
		}
		newAssertions := make([]*Envelope, len(c.Assertions))
		changed := !newSubject.IsIdenticalTo(c.Subject)
		for i, a := range c.Assertions {
			newAssertions[i], err = a.WalkReplace(target, replacement)
			if err != nil {
				return nil, err
			}
			if !newAssertions[i].IsIdenticalTo(a) {
				changed = true
			}
		}
		if !changed {
			return e, nil
		}
		return newWithAssertions(newSubject, newAssertions)
	case *WrappedCase:
		newEnv, err := c.Envelope.WalkReplace(target, replacement)
		if err != nil {
			return nil, err
		}
		if newEnv.IsIdenticalTo(c.Envelope) {
			return e, nil
		}
		return newEnv.Wrap(), nil
	case *AssertionCase:
		newPred, err := c.Assertion.Predicate().WalkReplace(target, replacement)
		if err != nil {
			return nil, err
		}
		newObj, err := c.Assertion.Object().WalkReplace(target, replacement)
		if err != nil {
			return nil, err
		}
		if newPred.IsIdenticalTo(c.Assertion.Predicate()) && newObj.IsIdenticalTo(c.Assertion.Object()) {
			return e, nil
		}
		return NewAssertionEnvelope(EnvelopeEncodableEnvelope{newPred}, EnvelopeEncodableEnvelope{newObj}), nil
	default:
		return e, nil
	}
}

// NodesMatching returns digests of nodes matching specified criteria.
func (e *Envelope) NodesMatching(
	targetDigests map[bccomponents.Digest]struct{},
	obscureTypes []ObscureType,
) map[bccomponents.Digest]struct{} {
	result := make(map[bccomponents.Digest]struct{})
	visitor := func(envelope *Envelope, level int, edge EdgeType, state struct{}) (struct{}, bool) {
		digestMatches := true
		if targetDigests != nil {
			_, digestMatches = targetDigests[envelope.Digest()]
		}
		if !digestMatches {
			return struct{}{}, false
		}
		if len(obscureTypes) == 0 {
			result[envelope.Digest()] = struct{}{}
			return struct{}{}, false
		}
		for _, ot := range obscureTypes {
			switch ot {
			case ObscureTypeElided:
				if _, ok := envelope.envelopeCase.(*ElidedCase); ok {
					result[envelope.Digest()] = struct{}{}
				}
			case ObscureTypeEncrypted:
				if _, ok := envelope.envelopeCase.(*EncryptedCase); ok {
					result[envelope.Digest()] = struct{}{}
				}
			case ObscureTypeCompressed:
				if _, ok := envelope.envelopeCase.(*CompressedCase); ok {
					result[envelope.Digest()] = struct{}{}
				}
			}
		}
		return struct{}{}, false
	}
	e.Walk(false, struct{}{}, visitor)
	return result
}

// --- Convenience set-with-action methods ---

// ElideRemovingSetWithAction removes elements in the target set using the given
// obscure action and optional encryption key.
func (e *Envelope) ElideRemovingSetWithAction(
	target map[bccomponents.Digest]struct{},
	action ObscureAction,
	encryptKey *ObscureActionEncryptWithKey,
) *Envelope {
	return e.ElideSetWithAction(target, false, action, encryptKey)
}

// ElideRevealingSetWithAction reveals elements in the target set, obscuring
// everything else using the given action.
func (e *Envelope) ElideRevealingSetWithAction(
	target map[bccomponents.Digest]struct{},
	action ObscureAction,
	encryptKey *ObscureActionEncryptWithKey,
) *Envelope {
	return e.ElideSetWithAction(target, true, action, encryptKey)
}

// --- WalkDecrypt ---

// WalkDecrypt recursively walks the envelope and decrypts any encrypted nodes
// using the provided symmetric keys. Returns a new envelope with decrypted
// content where possible; nodes that cannot be decrypted remain unchanged.
func (e *Envelope) WalkDecrypt(keys []bccomponents.SymmetricKey) *Envelope {
	switch c := e.envelopeCase.(type) {
	case *EncryptedCase:
		for _, key := range keys {
			decrypted, err := e.DecryptSubject(key)
			if err == nil {
				return decrypted.WalkDecrypt(keys)
			}
		}
		return e
	case *NodeCase:
		newSubject := c.Subject.WalkDecrypt(keys)
		newAssertions := make([]*Envelope, len(c.Assertions))
		changed := !newSubject.IsIdenticalTo(c.Subject)
		for i, a := range c.Assertions {
			newAssertions[i] = a.WalkDecrypt(keys)
			if !newAssertions[i].IsIdenticalTo(a) {
				changed = true
			}
		}
		if !changed {
			return e
		}
		return newWithUncheckedAssertions(newSubject, newAssertions)
	case *WrappedCase:
		newEnv := c.Envelope.WalkDecrypt(keys)
		if newEnv.IsIdenticalTo(c.Envelope) {
			return e
		}
		return newEnv.Wrap()
	case *AssertionCase:
		newPred := c.Assertion.Predicate().WalkDecrypt(keys)
		newObj := c.Assertion.Object().WalkDecrypt(keys)
		if newPred.IsIdenticalTo(c.Assertion.Predicate()) && newObj.IsIdenticalTo(c.Assertion.Object()) {
			return e
		}
		return NewAssertionEnvelope(EnvelopeEncodableEnvelope{newPred}, EnvelopeEncodableEnvelope{newObj})
	default:
		return e
	}
}

// --- WalkDecompress ---

// WalkDecompress recursively walks the envelope and decompresses compressed
// nodes. If targetDigests is non-nil, only nodes whose digest is in the set
// are decompressed. If nil, all compressed nodes are decompressed.
func (e *Envelope) WalkDecompress(targetDigests map[bccomponents.Digest]struct{}) *Envelope {
	switch c := e.envelopeCase.(type) {
	case *CompressedCase:
		matchesTarget := true
		if targetDigests != nil {
			_, matchesTarget = targetDigests[e.Digest()]
		}
		if matchesTarget {
			decompressed, err := e.Decompress()
			if err == nil {
				return decompressed.WalkDecompress(targetDigests)
			}
		}
		return e
	case *NodeCase:
		newSubject := c.Subject.WalkDecompress(targetDigests)
		newAssertions := make([]*Envelope, len(c.Assertions))
		changed := !newSubject.IsIdenticalTo(c.Subject)
		for i, a := range c.Assertions {
			newAssertions[i] = a.WalkDecompress(targetDigests)
			if !newAssertions[i].IsIdenticalTo(a) {
				changed = true
			}
		}
		if !changed {
			return e
		}
		return newWithUncheckedAssertions(newSubject, newAssertions)
	case *WrappedCase:
		newEnv := c.Envelope.WalkDecompress(targetDigests)
		if newEnv.IsIdenticalTo(c.Envelope) {
			return e
		}
		return newEnv.Wrap()
	case *AssertionCase:
		newPred := c.Assertion.Predicate().WalkDecompress(targetDigests)
		newObj := c.Assertion.Object().WalkDecompress(targetDigests)
		if newPred.IsIdenticalTo(c.Assertion.Predicate()) && newObj.IsIdenticalTo(c.Assertion.Object()) {
			return e
		}
		return NewAssertionEnvelope(EnvelopeEncodableEnvelope{newPred}, EnvelopeEncodableEnvelope{newObj})
	default:
		return e
	}
}

// --- Helpers ---

func digestSet(d bccomponents.Digest) map[bccomponents.Digest]struct{} {
	return map[bccomponents.Digest]struct{}{d: {}}
}

func digestSetFromArray(providers []DigestProvider) map[bccomponents.Digest]struct{} {
	result := make(map[bccomponents.Digest]struct{}, len(providers))
	for _, p := range providers {
		result[p.Digest()] = struct{}{}
	}
	return result
}

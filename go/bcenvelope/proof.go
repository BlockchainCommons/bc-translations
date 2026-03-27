package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
)

// ProofContainsSet creates a proof that this envelope includes every element
// in the target set. The proof is a minimal envelope that preserves the
// digest tree path from the root to each target element.
//
// Returns nil if not all targets can be proven to exist.
func (e *Envelope) ProofContainsSet(target map[bccomponents.Digest]struct{}) *Envelope {
	revealSet := e.revealSetOfSet(target)
	if !isSubset(target, revealSet) {
		return nil
	}
	return e.ElideRevealingSet(revealSet).ElideRemovingSet(target)
}

// ProofContainsTarget creates a proof that this envelope includes the single
// target element. Returns nil if the target cannot be proven to exist.
func (e *Envelope) ProofContainsTarget(target DigestProvider) *Envelope {
	set := map[bccomponents.Digest]struct{}{
		target.Digest(): {},
	}
	return e.ProofContainsSet(set)
}

// ConfirmContainsSet verifies whether this envelope contains all elements in
// the target set using the given inclusion proof. Verification succeeds only
// if the proof's digest matches this envelope's digest and the proof contains
// all target elements.
func (e *Envelope) ConfirmContainsSet(
	target map[bccomponents.Digest]struct{},
	proof *Envelope,
) bool {
	return e.Digest().Equal(proof.Digest()) && proof.containsAll(target)
}

// ConfirmContainsTarget verifies whether this envelope contains the single
// target element using the given inclusion proof.
func (e *Envelope) ConfirmContainsTarget(target DigestProvider, proof *Envelope) bool {
	set := map[bccomponents.Digest]struct{}{
		target.Digest(): {},
	}
	return e.ConfirmContainsSet(set, proof)
}

// DigestProvider is implemented by types that can provide a digest.
type DigestProvider interface {
	Digest() bccomponents.Digest
}

// --- Internal helpers ---

// revealSetOfSet builds a set of all digests needed to reveal the target set
// (all digests on the path from root to each target element).
func (e *Envelope) revealSetOfSet(target map[bccomponents.Digest]struct{}) map[bccomponents.Digest]struct{} {
	result := make(map[bccomponents.Digest]struct{})
	current := make(map[bccomponents.Digest]struct{})
	e.revealSets(target, current, result)
	return result
}

// containsAll checks whether this envelope's digest tree contains all elements
// in the target set.
func (e *Envelope) containsAll(target map[bccomponents.Digest]struct{}) bool {
	remaining := copyDigestSet(target)
	e.removeAllFound(remaining)
	return len(remaining) == 0
}

// revealSets recursively traverses the envelope to collect all digests on the
// path from root to each target element.
func (e *Envelope) revealSets(
	target map[bccomponents.Digest]struct{},
	current map[bccomponents.Digest]struct{},
	result map[bccomponents.Digest]struct{},
) {
	newCurrent := copyDigestSet(current)
	newCurrent[e.Digest()] = struct{}{}

	if _, found := target[e.Digest()]; found {
		for d := range newCurrent {
			result[d] = struct{}{}
		}
	}

	switch c := e.Case().(type) {
	case *NodeCase:
		c.Subject.revealSets(target, newCurrent, result)
		for _, assertion := range c.Assertions {
			assertion.revealSets(target, newCurrent, result)
		}
	case *WrappedCase:
		c.Envelope.revealSets(target, newCurrent, result)
	case *AssertionCase:
		c.Assertion.Predicate().revealSets(target, newCurrent, result)
		c.Assertion.Object().revealSets(target, newCurrent, result)
	}
}

// removeAllFound recursively traverses the envelope and removes found target
// elements from the set.
func (e *Envelope) removeAllFound(target map[bccomponents.Digest]struct{}) {
	d := e.Digest()
	if _, found := target[d]; found {
		delete(target, d)
	}
	if len(target) == 0 {
		return
	}

	switch c := e.Case().(type) {
	case *NodeCase:
		c.Subject.removeAllFound(target)
		for _, assertion := range c.Assertions {
			assertion.removeAllFound(target)
		}
	case *WrappedCase:
		c.Envelope.removeAllFound(target)
	case *AssertionCase:
		c.Assertion.Predicate().removeAllFound(target)
		c.Assertion.Object().removeAllFound(target)
	}
}

// isSubset returns true if all keys in a are also in b.
func isSubset(a, b map[bccomponents.Digest]struct{}) bool {
	for k := range a {
		if _, ok := b[k]; !ok {
			return false
		}
	}
	return true
}

// copyDigestSet creates a shallow copy of a digest set.
func copyDigestSet(s map[bccomponents.Digest]struct{}) map[bccomponents.Digest]struct{} {
	c := make(map[bccomponents.Digest]struct{}, len(s))
	for k, v := range s {
		c[k] = v
	}
	return c
}

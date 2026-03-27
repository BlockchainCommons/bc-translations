package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// AddEdgeEnvelope returns a new envelope with an added 'edge': <edge>
// assertion.
func (e *Envelope) AddEdgeEnvelope(edge *Envelope) *Envelope {
	return e.AddAssertion(knownvalues.Edge, edge)
}

// Edges returns all edge object envelopes (assertions with predicate 'edge').
func (e *Envelope) Edges() ([]*Envelope, error) {
	return e.ObjectsForPredicate(knownvalues.Edge), nil
}

// ValidateEdge validates an edge envelope's structure per BCR-2026-003. An
// edge may be wrapped (signed) or unwrapped. The inner envelope must have
// exactly three assertion predicates: 'isA', 'source', and 'target'. No
// other assertions are permitted on the edge subject.
func (e *Envelope) ValidateEdge() error {
	inner := e
	if e.Subject().IsWrapped() {
		unwrapped, err := e.Subject().Unwrap()
		if err != nil {
			return err
		}
		inner = unwrapped
	}

	seenIsA := false
	seenSource := false
	seenTarget := false

	for _, assertion := range inner.Assertions() {
		predicate, err := assertion.TryPredicate()
		if err != nil {
			return ErrEdgeUnexpectedAssertion
		}
		kv, err := predicate.TryKnownValue()
		if err != nil {
			return ErrEdgeUnexpectedAssertion
		}

		switch kv.Value() {
		case knownvalues.IsARaw:
			if seenIsA {
				return ErrEdgeDuplicateIsA
			}
			seenIsA = true
		case knownvalues.SourceRaw:
			if seenSource {
				return ErrEdgeDuplicateSource
			}
			seenSource = true
		case knownvalues.TargetRaw:
			if seenTarget {
				return ErrEdgeDuplicateTarget
			}
			seenTarget = true
		default:
			return ErrEdgeUnexpectedAssertion
		}
	}

	if !seenIsA {
		return ErrEdgeMissingIsA
	}
	if !seenSource {
		return ErrEdgeMissingSource
	}
	if !seenTarget {
		return ErrEdgeMissingTarget
	}

	return nil
}

// EdgeIsA extracts the 'isA' assertion object from an edge envelope.
func (e *Envelope) EdgeIsA() (*Envelope, error) {
	inner, err := e.edgeInner()
	if err != nil {
		return nil, err
	}
	return inner.ObjectForPredicate(knownvalues.IsA)
}

// EdgeSource extracts the 'source' assertion object from an edge envelope.
func (e *Envelope) EdgeSource() (*Envelope, error) {
	inner, err := e.edgeInner()
	if err != nil {
		return nil, err
	}
	return inner.ObjectForPredicate(knownvalues.Source)
}

// EdgeTarget extracts the 'target' assertion object from an edge envelope.
func (e *Envelope) EdgeTarget() (*Envelope, error) {
	inner, err := e.edgeInner()
	if err != nil {
		return nil, err
	}
	return inner.ObjectForPredicate(knownvalues.Target)
}

// EdgeSubject extracts the edge's subject identifier (the inner envelope's
// subject).
func (e *Envelope) EdgeSubject() (*Envelope, error) {
	inner, err := e.edgeInner()
	if err != nil {
		return nil, err
	}
	return inner.Subject(), nil
}

// EdgesMatching filters edges by optional criteria. Each parameter is optional
// (nil means match all). Only edges matching all specified criteria are
// returned.
func (e *Envelope) EdgesMatching(
	isA *Envelope,
	source *Envelope,
	target *Envelope,
	subject *Envelope,
) ([]*Envelope, error) {
	allEdges, err := e.Edges()
	if err != nil {
		return nil, err
	}

	var matching []*Envelope
	for _, edge := range allEdges {
		if isA != nil {
			edgeIsA, err := edge.EdgeIsA()
			if err != nil || !edgeIsA.IsEquivalentTo(isA) {
				continue
			}
		}

		if source != nil {
			edgeSource, err := edge.EdgeSource()
			if err != nil || !edgeSource.IsEquivalentTo(source) {
				continue
			}
		}

		if target != nil {
			edgeTarget, err := edge.EdgeTarget()
			if err != nil || !edgeTarget.IsEquivalentTo(target) {
				continue
			}
		}

		if subject != nil {
			edgeSubject, err := edge.EdgeSubject()
			if err != nil || !edgeSubject.IsEquivalentTo(subject) {
				continue
			}
		}

		matching = append(matching, edge)
	}

	return matching, nil
}

// edgeInner returns the inner envelope of an edge (unwrapping if wrapped).
func (e *Envelope) edgeInner() (*Envelope, error) {
	if e.Subject().IsWrapped() {
		return e.Subject().Unwrap()
	}
	return e, nil
}

// --- Edges container ---

// EdgesContainer is a container for edge envelopes keyed by their digest.
type EdgesContainer struct {
	envelopes map[bccomponents.Digest]*Envelope
}

// NewEdgesContainer creates a new empty edges container.
func NewEdgesContainer() *EdgesContainer {
	return &EdgesContainer{
		envelopes: make(map[bccomponents.Digest]*Envelope),
	}
}

// Add adds a pre-constructed edge envelope.
func (c *EdgesContainer) Add(edgeEnvelope *Envelope) {
	c.envelopes[edgeEnvelope.Digest()] = edgeEnvelope
}

// Get retrieves an edge by its digest.
func (c *EdgesContainer) Get(digest bccomponents.Digest) *Envelope {
	return c.envelopes[digest]
}

// Remove removes an edge by its digest and returns it.
func (c *EdgesContainer) Remove(digest bccomponents.Digest) *Envelope {
	env := c.envelopes[digest]
	delete(c.envelopes, digest)
	return env
}

// Clear removes all edges.
func (c *EdgesContainer) Clear() {
	c.envelopes = make(map[bccomponents.Digest]*Envelope)
}

// IsEmpty returns whether the container has any edges.
func (c *EdgesContainer) IsEmpty() bool {
	return len(c.envelopes) == 0
}

// Len returns the number of edges in the container.
func (c *EdgesContainer) Len() int {
	return len(c.envelopes)
}

// AddToEnvelope adds all edges as 'edge' assertion envelopes to the given
// envelope.
func (c *EdgesContainer) AddToEnvelope(envelope *Envelope) *Envelope {
	result := envelope
	for _, edgeEnvelope := range c.envelopes {
		result = result.AddAssertion(knownvalues.Edge, edgeEnvelope)
	}
	return result
}

// EdgesContainerFromEnvelope extracts edges from an envelope's 'edge'
// assertions.
func EdgesContainerFromEnvelope(envelope *Envelope) (*EdgesContainer, error) {
	edgeEnvelopes, err := envelope.Edges()
	if err != nil {
		return nil, err
	}
	container := NewEdgesContainer()
	for _, edge := range edgeEnvelopes {
		container.envelopes[edge.Digest()] = edge
	}
	return container, nil
}

// Edgeable is implemented by types that can have edges.
type Edgeable interface {
	Edges() *EdgesContainer
}

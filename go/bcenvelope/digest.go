package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
)

// Digests returns the set of digests contained in the envelope's elements,
// down to the specified level limit.
func (e *Envelope) Digests(levelLimit int) map[bccomponents.Digest]struct{} {
	result := make(map[bccomponents.Digest]struct{})
	visitor := func(envelope *Envelope, level int, edge EdgeType, state struct{}) (struct{}, bool) {
		if level < levelLimit {
			result[envelope.Digest()] = struct{}{}
			result[envelope.Subject().Digest()] = struct{}{}
		}
		return struct{}{}, false
	}
	e.Walk(false, struct{}{}, visitor)
	return result
}

// DeepDigests returns all digests in the envelope at all levels.
func (e *Envelope) DeepDigests() map[bccomponents.Digest]struct{} {
	return e.Digests(int(^uint(0) >> 1))
}

// ShallowDigests returns digests down to the second level only.
func (e *Envelope) ShallowDigests() map[bccomponents.Digest]struct{} {
	return e.Digests(2)
}

// StructuralDigest returns a digest that captures the structural form of the envelope.
func (e *Envelope) StructuralDigest() bccomponents.Digest {
	var image []byte
	visitor := func(envelope *Envelope, level int, edge EdgeType, state struct{}) (struct{}, bool) {
		switch envelope.envelopeCase.(type) {
		case *ElidedCase:
			image = append(image, 1)
		case *EncryptedCase:
			image = append(image, 0)
		case *CompressedCase:
			image = append(image, 2)
		}
		image = append(image, envelope.Digest().Bytes()...)
		return struct{}{}, false
	}
	e.Walk(false, struct{}{}, visitor)
	return bccomponents.DigestFromImage(image)
}

// IsEquivalentTo tests semantic equivalence (same digest).
func (e *Envelope) IsEquivalentTo(other *Envelope) bool {
	return e.Digest().Equal(other.Digest())
}

// IsIdenticalTo tests structural identity (same digest AND structure).
func (e *Envelope) IsIdenticalTo(other *Envelope) bool {
	if !e.IsEquivalentTo(other) {
		return false
	}
	return e.StructuralDigest().Equal(other.StructuralDigest())
}

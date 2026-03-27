package bcenvelope

import (
	"fmt"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// Envelope CBOR tags.
var envelopeCBORTags = dcbor.TagsForValues([]dcbor.TagValue{bctags.TagEnvelope})

// EnvelopeCBORTags returns the CBOR tags used for Envelope.
func EnvelopeCBORTags() []dcbor.Tag {
	return envelopeCBORTags
}

// CBORTags returns the CBOR tags for this envelope.
func (e *Envelope) CBORTags() []dcbor.Tag {
	return envelopeCBORTags
}

// UntaggedCBOR returns the untagged CBOR representation of the envelope.
func (e *Envelope) UntaggedCBOR() dcbor.CBOR {
	switch c := e.envelopeCase.(type) {
	case *NodeCase:
		result := make([]dcbor.CBOR, 0, 1+len(c.Assertions))
		result = append(result, c.Subject.UntaggedCBOR())
		for _, assertion := range c.Assertions {
			result = append(result, assertion.UntaggedCBOR())
		}
		return dcbor.NewCBORArray(result)
	case *LeafCase:
		return dcbor.ToTaggedValue(dcbor.NewTag(bctags.TagLeaf, bctags.TagNameLeaf), *c.CBOR)
	case *WrappedCase:
		return c.Envelope.TaggedCBOR()
	case *AssertionCase:
		return c.Assertion.ToCBOR()
	case *ElidedCase:
		return dcbor.ToByteString(c.Digest.Bytes())
	case *KnownValueCase:
		return c.Value.UntaggedCBOR()
	case *EncryptedCase:
		return c.EncryptedMessage.TaggedCBOR()
	case *CompressedCase:
		return c.Compressed.TaggedCBOR()
	default:
		panic("unknown EnvelopeCase type")
	}
}

// TaggedCBOR returns the tagged CBOR representation of the envelope.
func (e *Envelope) TaggedCBOR() dcbor.CBOR {
	return dcbor.NewCBORTagged(envelopeCBORTags[0], e.UntaggedCBOR())
}

// ToCBOR returns the tagged CBOR representation.
func (e *Envelope) ToCBOR() dcbor.CBOR {
	return e.TaggedCBOR()
}

// ToCBORData returns the binary CBOR encoding of the envelope.
func (e *Envelope) ToCBORData() []byte {
	return e.TaggedCBOR().ToCBORData()
}

// EnvelopeFromTaggedCBORValue decodes a tagged CBOR value into an Envelope.
func EnvelopeFromTaggedCBORValue(cbor dcbor.CBOR) (*Envelope, error) {
	return dcbor.DecodeTagged(cbor, envelopeCBORTags, EnvelopeFromUntaggedCBOR)
}

// EnvelopeFromUntaggedCBOR decodes an untagged CBOR value into an Envelope.
func EnvelopeFromUntaggedCBOR(cbor dcbor.CBOR) (*Envelope, error) {
	cs := cbor.AsCase()
	switch cs.Kind {
	case dcbor.CBORKindTagged:
		tag, item, ok := cbor.AsTaggedValue()
		if !ok {
			return nil, fmt.Errorf("invalid tagged value in envelope")
		}
		tagVal := tag.Value()
		switch tagVal {
		case bctags.TagLeaf, bctags.TagEncodedCBOR:
			return newLeaf(item), nil
		case bctags.TagEnvelope:
			inner, err := EnvelopeFromTaggedCBORValue(cbor)
			if err != nil {
				return nil, err
			}
			return newWrapped(inner), nil
		case bctags.TagEncrypted:
			encrypted, err := bccomponents.DecodeEncryptedMessage(item)
			if err != nil {
				return nil, err
			}
			return newWithEncrypted(encrypted)
		case bctags.TagCompressed:
			compressed, err := bccomponents.DecodeCompressed(item)
			if err != nil {
				return nil, err
			}
			return newWithCompressed(&compressed)
		default:
			return nil, fmt.Errorf("unknown envelope tag: %d", tagVal)
		}

	case dcbor.CBORKindByteString:
		bs, err := cbor.TryIntoByteString()
		if err != nil {
			return nil, err
		}
		digest, err := bccomponents.DigestFromDataRef(bs)
		if err != nil {
			return nil, err
		}
		return newElided(digest), nil

	case dcbor.CBORKindArray:
		elements, err := cbor.TryIntoArray()
		if err != nil {
			return nil, err
		}
		if len(elements) < 2 {
			return nil, fmt.Errorf("node must have at least two elements")
		}
		subject, err := EnvelopeFromUntaggedCBOR(elements[0])
		if err != nil {
			return nil, err
		}
		assertions := make([]*Envelope, len(elements)-1)
		for i, elem := range elements[1:] {
			a, err := EnvelopeFromUntaggedCBOR(elem)
			if err != nil {
				return nil, err
			}
			assertions[i] = a
		}
		return newWithAssertions(subject, assertions)

	case dcbor.CBORKindMap:
		assertion, err := AssertionFromCBOR(cbor)
		if err != nil {
			return nil, err
		}
		return newWithAssertion(assertion), nil

	case dcbor.CBORKindUnsigned:
		value, _ := cbor.AsUnsigned()
		kv := knownvalues.NewKnownValue(value)
		return newWithKnownValue(kv), nil

	default:
		return nil, fmt.Errorf("invalid envelope")
	}
}

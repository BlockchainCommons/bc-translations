package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// EnvelopeEncodable is the interface for types that can be converted into an Envelope.
type EnvelopeEncodable interface {
	Envelope() *Envelope
}

// --- Wrapper types for EnvelopeEncodable ---

// EnvelopeEncodableEnvelope wraps an *Envelope as EnvelopeEncodable.
type EnvelopeEncodableEnvelope struct{ Value *Envelope }

func (e EnvelopeEncodableEnvelope) Envelope() *Envelope { return e.Value }

// EnvelopeEncodableCBOR wraps a dcbor.CBOR as EnvelopeEncodable.
type EnvelopeEncodableCBOR struct{ Value dcbor.CBOR }

func (e EnvelopeEncodableCBOR) Envelope() *Envelope { return newLeaf(e.Value) }

// EnvelopeEncodableString wraps a string as EnvelopeEncodable.
type EnvelopeEncodableString struct{ Value string }

func (e EnvelopeEncodableString) Envelope() *Envelope { return newLeaf(dcbor.NewCBORText(e.Value)) }

// EnvelopeEncodableInt wraps an int as EnvelopeEncodable.
type EnvelopeEncodableInt struct{ Value int }

func (e EnvelopeEncodableInt) Envelope() *Envelope { return newLeaf(dcbor.MustFromAny(e.Value)) }

// EnvelopeEncodableUint64 wraps a uint64 as EnvelopeEncodable.
type EnvelopeEncodableUint64 struct{ Value uint64 }

func (e EnvelopeEncodableUint64) Envelope() *Envelope { return newLeaf(dcbor.NewCBORUnsigned(e.Value)) }

// EnvelopeEncodableInt64 wraps an int64 as EnvelopeEncodable.
type EnvelopeEncodableInt64 struct{ Value int64 }

func (e EnvelopeEncodableInt64) Envelope() *Envelope { return newLeaf(dcbor.MustFromAny(e.Value)) }

// EnvelopeEncodableBool wraps a bool as EnvelopeEncodable.
type EnvelopeEncodableBool struct{ Value bool }

func (e EnvelopeEncodableBool) Envelope() *Envelope { return newLeaf(dcbor.MustFromAny(e.Value)) }

// EnvelopeEncodableFloat64 wraps a float64 as EnvelopeEncodable.
type EnvelopeEncodableFloat64 struct{ Value float64 }

func (e EnvelopeEncodableFloat64) Envelope() *Envelope { return newLeaf(dcbor.MustFromAny(e.Value)) }

// EnvelopeEncodableBytes wraps a byte slice as EnvelopeEncodable.
type EnvelopeEncodableBytes struct{ Value []byte }

func (e EnvelopeEncodableBytes) Envelope() *Envelope { return newLeaf(dcbor.ToByteString(e.Value)) }

// EnvelopeEncodableAssertion wraps an *Assertion as EnvelopeEncodable.
type EnvelopeEncodableAssertion struct{ Value *Assertion }

func (e EnvelopeEncodableAssertion) Envelope() *Envelope { return newWithAssertion(e.Value) }

// EnvelopeEncodableKnownValue wraps a KnownValue as EnvelopeEncodable.
type EnvelopeEncodableKnownValue struct{ Value knownvalues.KnownValue }

func (e EnvelopeEncodableKnownValue) Envelope() *Envelope { return newWithKnownValue(e.Value) }

// EnvelopeEncodableDigest wraps a Digest as EnvelopeEncodable.
type EnvelopeEncodableDigest struct{ Value bccomponents.Digest }

func (e EnvelopeEncodableDigest) Envelope() *Envelope { return newLeaf(e.Value.TaggedCBOR()) }

// --- AsEnvelopeEncodable converts any supported type to EnvelopeEncodable ---

// AsEnvelopeEncodable wraps a value into an EnvelopeEncodable.
// Supported types: *Envelope, EnvelopeEncodable, string, int, int64, uint64,
// bool, float64, []byte, *Assertion, knownvalues.KnownValue,
// bccomponents.Digest, dcbor.CBOR, dcbor.CBORTaggedEncodable.
func AsEnvelopeEncodable(v any) EnvelopeEncodable {
	switch val := v.(type) {
	case EnvelopeEncodable:
		return val
	case *Envelope:
		return EnvelopeEncodableEnvelope{val}
	case string:
		return EnvelopeEncodableString{val}
	case int:
		return EnvelopeEncodableInt{val}
	case int64:
		return EnvelopeEncodableInt64{val}
	case uint64:
		return EnvelopeEncodableUint64{val}
	case bool:
		return EnvelopeEncodableBool{val}
	case float64:
		return EnvelopeEncodableFloat64{val}
	case []byte:
		return EnvelopeEncodableBytes{val}
	case *Assertion:
		return EnvelopeEncodableAssertion{val}
	case knownvalues.KnownValue:
		return EnvelopeEncodableKnownValue{val}
	case bccomponents.Digest:
		return EnvelopeEncodableDigest{val}
	case dcbor.CBOR:
		return EnvelopeEncodableCBOR{val}
	case dcbor.CBORTaggedEncodable:
		cbor, err := dcbor.TaggedCBOR(val)
		if err != nil {
			panic("AsEnvelopeEncodable: " + err.Error())
		}
		return EnvelopeEncodableCBOR{cbor}
	default:
		panic("AsEnvelopeEncodable: unsupported type")
	}
}

// --- Convenience constructors ---

// EnvelopeFromString creates an envelope from a string value.
func EnvelopeFromString(s string) *Envelope { return EnvelopeEncodableString{s}.Envelope() }

// EnvelopeFromInt creates an envelope from an int value.
func EnvelopeFromInt(v int) *Envelope { return EnvelopeEncodableInt{v}.Envelope() }

// EnvelopeFromUint64 creates an envelope from a uint64 value.
func EnvelopeFromUint64(v uint64) *Envelope { return EnvelopeEncodableUint64{v}.Envelope() }

// EnvelopeFromBool creates an envelope from a bool value.
func EnvelopeFromBool(b bool) *Envelope { return EnvelopeEncodableBool{b}.Envelope() }

// EnvelopeFromBytes creates an envelope from a byte slice.
func EnvelopeFromBytes(b []byte) *Envelope { return EnvelopeEncodableBytes{b}.Envelope() }

// EnvelopeFromCBOR creates an envelope from a CBOR value.
func EnvelopeFromCBOR(cbor dcbor.CBOR) *Envelope { return EnvelopeEncodableCBOR{cbor}.Envelope() }

// EnvelopeFromKnownValue creates an envelope from a known value.
func EnvelopeFromKnownValue(kv knownvalues.KnownValue) *Envelope {
	return EnvelopeEncodableKnownValue{kv}.Envelope()
}
// EnvelopeFromTaggedCBOR creates an envelope from a tagged CBOR encodable value.
func EnvelopeFromTaggedCBOR(v dcbor.CBORTaggedEncodable) *Envelope {
	cbor, err := dcbor.TaggedCBOR(v)
	if err != nil {
		panic("EnvelopeFromTaggedCBOR: " + err.Error())
	}
	return newLeaf(cbor)
}
// NewKnownValueEnvelope creates an envelope from a known value.
func NewKnownValueEnvelope(kv knownvalues.KnownValue) *Envelope {
	return newWithKnownValue(kv)
}

// --- Decodable ---

// EnvelopeFromCBORValue decodes a tagged CBOR value into an envelope.
func EnvelopeFromCBORValue(cbor dcbor.CBOR) (*Envelope, error) {
	return EnvelopeFromTaggedCBORValue(cbor)
}

// EnvelopeFromCBORData decodes binary CBOR data into an envelope.
func EnvelopeFromCBORData(data []byte) (*Envelope, error) {
	cbor, err := dcbor.TryFromData(data)
	if err != nil {
		return nil, err
	}
	return EnvelopeFromCBORValue(cbor)
}

// --- Extract helpers ---

// ExtractSubjectString extracts a string from the envelope's subject leaf.
func ExtractSubjectString(e *Envelope) (string, error) {
	cbor, err := e.Subject().TryLeaf()
	if err != nil {
		return "", err
	}
	s, err := cbor.TryIntoText()
	if err != nil {
		return "", ErrInvalidFormat
	}
	return s, nil
}

// ExtractSubjectInt extracts an int from the envelope's subject leaf.
func ExtractSubjectInt(e *Envelope) (int, error) {
	cbor, err := e.Subject().TryLeaf()
	if err != nil {
		return 0, err
	}
	v, ok := cbor.AsInt64()
	if !ok {
		return 0, ErrInvalidFormat
	}
	return int(v), nil
}

// ExtractSubjectUint64 extracts a uint64 from the envelope's subject leaf.
func ExtractSubjectUint64(e *Envelope) (uint64, error) {
	cbor, err := e.Subject().TryLeaf()
	if err != nil {
		return 0, err
	}
	v, ok := cbor.AsUnsigned()
	if !ok {
		return 0, ErrInvalidFormat
	}
	return v, nil
}

// ExtractSubjectBool extracts a bool from the envelope's subject leaf.
func ExtractSubjectBool(e *Envelope) (bool, error) {
	cbor, err := e.Subject().TryLeaf()
	if err != nil {
		return false, err
	}
	v, ok := cbor.AsBool()
	if !ok {
		return false, ErrInvalidFormat
	}
	return v, nil
}

// ExtractSubjectFloat64 extracts a float64 from the envelope's subject leaf.
func ExtractSubjectFloat64(e *Envelope) (float64, error) {
	cbor, err := e.Subject().TryLeaf()
	if err != nil {
		return 0, err
	}
	v, ok := cbor.AsFloat64()
	if !ok {
		return 0, ErrInvalidFormat
	}
	return v, nil
}

// ExtractSubjectBytes extracts a byte slice from the envelope's subject leaf.
func ExtractSubjectBytes(e *Envelope) ([]byte, error) {
	cbor, err := e.Subject().TryLeaf()
	if err != nil {
		return nil, err
	}
	b, err := cbor.TryIntoByteString()
	if err != nil {
		return nil, ErrInvalidFormat
	}
	return b, nil
}

// ExtractSubjectCBOR extracts the CBOR value from the envelope's subject leaf.
func ExtractSubjectCBOR(e *Envelope) (dcbor.CBOR, error) {
	return e.Subject().TryLeaf()
}

// ExtractSubjectEnvelope extracts the wrapped envelope from the envelope's subject.
func ExtractSubjectEnvelope(e *Envelope) (*Envelope, error) {
	switch c := e.Case().(type) {
	case *WrappedCase:
		return c.Envelope, nil
	case *NodeCase:
		return ExtractSubjectEnvelope(c.Subject)
	default:
		return nil, ErrInvalidFormat
	}
}

// ExtractSubjectAssertion extracts the assertion from the envelope's subject.
func ExtractSubjectAssertion(e *Envelope) (*Assertion, error) {
	switch c := e.Case().(type) {
	case *AssertionCase:
		return c.Assertion, nil
	case *NodeCase:
		return ExtractSubjectAssertion(c.Subject)
	default:
		return nil, ErrInvalidFormat
	}
}

// ExtractSubjectKnownValue extracts the known value from the envelope's subject.
func ExtractSubjectKnownValue(e *Envelope) (knownvalues.KnownValue, error) {
	switch c := e.Case().(type) {
	case *KnownValueCase:
		return c.Value, nil
	case *NodeCase:
		return ExtractSubjectKnownValue(c.Subject)
	default:
		return knownvalues.KnownValue{}, ErrInvalidFormat
	}
}

// ExtractSubjectDigest extracts the digest from an elided envelope's subject.
func ExtractSubjectDigest(e *Envelope) (bccomponents.Digest, error) {
	switch c := e.Case().(type) {
	case *ElidedCase:
		return *c.Digest, nil
	case *NodeCase:
		return ExtractSubjectDigest(c.Subject)
	default:
		return bccomponents.Digest{}, ErrInvalidFormat
	}
}

// ExtractObjectForPredicate extracts a typed CBOR value from an assertion's object
// using a generic decoder function.
func ExtractObjectForPredicate[T any](e *Envelope, predicate any, decode func(dcbor.CBOR) (T, error)) (T, error) {
	var zero T
	obj, err := e.ObjectForPredicate(predicate)
	if err != nil {
		return zero, err
	}
	cbor, err := obj.TryLeaf()
	if err != nil {
		return zero, err
	}
	return decode(cbor)
}

// ExtractOptionalObjectForPredicate extracts a typed CBOR value from an optional assertion's object.
func ExtractOptionalObjectForPredicate[T any](e *Envelope, predicate any, decode func(dcbor.CBOR) (T, error)) (*T, error) {
	obj, err := e.OptionalObjectForPredicate(predicate)
	if err != nil {
		return nil, err
	}
	if obj == nil {
		return nil, nil
	}
	cbor, err := obj.TryLeaf()
	if err != nil {
		return nil, err
	}
	val, err := decode(cbor)
	if err != nil {
		return nil, err
	}
	return &val, nil
}

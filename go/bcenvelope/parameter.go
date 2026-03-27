package bcenvelope

import (
	"fmt"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// Parameter is a parameter identifier used in Gordian Envelope expressions.
//
// Parameters can be identified in two ways:
//  1. By a numeric ID (for well-known parameters) - parameterKindKnown
//  2. By a string name (for application-specific parameters) - parameterKindNamed
//
// When encoded in CBOR, parameters are tagged with #6.40007 (TagParameter).
type Parameter struct {
	kind  parameterKind
	value uint64 // used for Known
	name  string // used for both Known (optional) and Named
}

type parameterKind int

const (
	parameterKindKnown parameterKind = iota
	parameterKindNamed
)

// NewKnownParameter creates a parameter with a numeric ID and a name.
func NewKnownParameter(value uint64, name string) Parameter {
	return Parameter{kind: parameterKindKnown, value: value, name: name}
}

// NewKnownParameterNoName creates a parameter with a numeric ID and no name.
func NewKnownParameterNoName(value uint64) Parameter {
	return Parameter{kind: parameterKindKnown, value: value}
}

// NewNamedParameter creates a parameter identified by a string name.
func NewNamedParameter(name string) Parameter {
	return Parameter{kind: parameterKindNamed, name: name}
}

// ParameterFromUint64 creates a known parameter from a uint64 value with no name.
func ParameterFromUint64(value uint64) Parameter {
	return NewKnownParameterNoName(value)
}

// ParameterFromString creates a named parameter from a string.
func ParameterFromString(name string) Parameter {
	return NewNamedParameter(name)
}

// IsKnown returns true if the parameter is identified by a numeric ID.
func (p Parameter) IsKnown() bool { return p.kind == parameterKindKnown }

// IsNamed returns true if the parameter is identified by a string name.
func (p Parameter) IsNamed() bool { return p.kind == parameterKindNamed }

// Value returns the numeric ID for known parameters; panics for named parameters.
func (p Parameter) Value() uint64 {
	if p.kind != parameterKindKnown {
		panic("Value() called on named parameter")
	}
	return p.value
}

// Name returns the display name of the parameter.
//
// For known parameters with a name, returns the name.
// For known parameters without a name, returns the numeric ID as a string.
// For named parameters, returns the name enclosed in quotes.
func (p Parameter) Name() string {
	switch p.kind {
	case parameterKindKnown:
		if p.name != "" {
			return p.name
		}
		return fmt.Sprintf("%d", p.value)
	case parameterKindNamed:
		return fmt.Sprintf("%q", p.name)
	default:
		return "<unknown>"
	}
}

// Equal reports whether two parameters are equal.
// Known parameters are equal if they have the same numeric ID (names ignored).
// Named parameters are equal if they have the same name.
// Known and named parameters are never equal to each other.
func (p Parameter) Equal(other Parameter) bool {
	if p.kind != other.kind {
		return false
	}
	switch p.kind {
	case parameterKindKnown:
		return p.value == other.value
	case parameterKindNamed:
		return p.name == other.name
	default:
		return false
	}
}

// String returns a display representation using no store.
func (p Parameter) String() string {
	return NameForParameter(p, nil)
}

// ParameterCBORTags returns the accepted CBOR tags for parameters.
func ParameterCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagParameter})
}

// CBORTags returns the accepted CBOR tags for parameters.
func (p Parameter) CBORTags() []dcbor.Tag {
	return ParameterCBORTags()
}

// UntaggedCBOR returns the untagged CBOR representation.
// Known parameters encode as unsigned integers; named parameters encode as text.
func (p Parameter) UntaggedCBOR() dcbor.CBOR {
	switch p.kind {
	case parameterKindKnown:
		return dcbor.NewCBORUnsigned(p.value)
	case parameterKindNamed:
		return dcbor.NewCBORText(p.name)
	default:
		panic("invalid parameter kind")
	}
}

// TaggedCBOR returns the tagged CBOR representation.
func (p Parameter) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(p)
	return cbor
}

// ToCBOR returns the tagged CBOR representation.
func (p Parameter) ToCBOR() dcbor.CBOR {
	return p.TaggedCBOR()
}

// DecodeParameter decodes an untagged CBOR value into a Parameter.
func DecodeParameter(cbor dcbor.CBOR) (Parameter, error) {
	switch cbor.Kind() {
	case dcbor.CBORKindUnsigned:
		value, ok := cbor.AsUnsigned()
		if !ok {
			return Parameter{}, fmt.Errorf("invalid parameter: expected unsigned integer")
		}
		return NewKnownParameterNoName(value), nil
	case dcbor.CBORKindText:
		name, ok := cbor.AsText()
		if !ok {
			return Parameter{}, fmt.Errorf("invalid parameter: expected text")
		}
		return NewNamedParameter(name), nil
	default:
		return Parameter{}, fmt.Errorf("invalid parameter")
	}
}

// DecodeTaggedParameter decodes a tagged CBOR value into a Parameter.
func DecodeTaggedParameter(cbor dcbor.CBOR) (Parameter, error) {
	return dcbor.DecodeTagged(cbor, ParameterCBORTags(), DecodeParameter)
}

var (
	_ dcbor.CBORTaggedEncodable = Parameter{}
)

package bcenvelope

import (
	"fmt"

	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// Function is a function identifier used in Gordian Envelope expressions.
//
// Functions can be identified in two ways:
//  1. By a numeric ID (for well-known functions) - FunctionKnown
//  2. By a string name (for application-specific functions) - FunctionNamed
//
// When encoded in CBOR, functions are tagged with #6.40006 (TagFunction).
type Function struct {
	kind  functionKind
	value uint64 // used for Known
	name  string // used for both Known (optional) and Named
}

type functionKind int

const (
	functionKindKnown functionKind = iota
	functionKindNamed
)

// NewKnownFunction creates a function with a numeric ID and an optional name.
func NewKnownFunction(value uint64, name string) Function {
	return Function{kind: functionKindKnown, value: value, name: name}
}

// NewKnownFunctionNoName creates a function with a numeric ID and no name.
func NewKnownFunctionNoName(value uint64) Function {
	return Function{kind: functionKindKnown, value: value}
}

// NewNamedFunction creates a function identified by a string name.
func NewNamedFunction(name string) Function {
	return Function{kind: functionKindNamed, name: name}
}

// FunctionFromUint64 creates a known function from a uint64 value with no name.
func FunctionFromUint64(value uint64) Function {
	return NewKnownFunctionNoName(value)
}

// FunctionFromString creates a named function from a string.
func FunctionFromString(name string) Function {
	return NewNamedFunction(name)
}

// IsKnown returns true if the function is identified by a numeric ID.
func (f Function) IsKnown() bool { return f.kind == functionKindKnown }

// IsNamed returns true if the function is identified by a string name.
func (f Function) IsNamed() bool { return f.kind == functionKindNamed }

// Value returns the numeric ID for known functions; panics for named functions.
func (f Function) Value() uint64 {
	if f.kind != functionKindKnown {
		panic("Value() called on named function")
	}
	return f.value
}

// Name returns the display name of the function.
//
// For known functions with a name, returns the name.
// For known functions without a name, returns the numeric ID as a string.
// For named functions, returns the name enclosed in quotes.
func (f Function) Name() string {
	switch f.kind {
	case functionKindKnown:
		if f.name != "" {
			return f.name
		}
		return fmt.Sprintf("%d", f.value)
	case functionKindNamed:
		return fmt.Sprintf("%q", f.name)
	default:
		return "<unknown>"
	}
}

// NamedName returns the raw string name for named functions, or empty string and false for known functions.
func (f Function) NamedName() (string, bool) {
	if f.kind == functionKindNamed {
		return f.name, true
	}
	return "", false
}

// Equal reports whether two functions are equal.
// Known functions are equal if they have the same numeric ID (names ignored).
// Named functions are equal if they have the same name.
// Known and named functions are never equal to each other.
func (f Function) Equal(other Function) bool {
	if f.kind != other.kind {
		return false
	}
	switch f.kind {
	case functionKindKnown:
		return f.value == other.value
	case functionKindNamed:
		return f.name == other.name
	default:
		return false
	}
}

// String returns a display representation using no store.
func (f Function) String() string {
	return NameForFunction(f, nil)
}

// FunctionCBORTags returns the accepted CBOR tags for functions.
func FunctionCBORTags() []dcbor.Tag {
	return dcbor.TagsForValues([]dcbor.TagValue{bctags.TagFunction})
}

// CBORTags returns the accepted CBOR tags for functions.
func (f Function) CBORTags() []dcbor.Tag {
	return FunctionCBORTags()
}

// UntaggedCBOR returns the untagged CBOR representation.
// Known functions encode as unsigned integers; named functions encode as text.
func (f Function) UntaggedCBOR() dcbor.CBOR {
	switch f.kind {
	case functionKindKnown:
		return dcbor.NewCBORUnsigned(f.value)
	case functionKindNamed:
		return dcbor.NewCBORText(f.name)
	default:
		panic("invalid function kind")
	}
}

// TaggedCBOR returns the tagged CBOR representation.
func (f Function) TaggedCBOR() dcbor.CBOR {
	cbor, _ := dcbor.TaggedCBOR(f)
	return cbor
}

// ToCBOR returns the tagged CBOR representation.
func (f Function) ToCBOR() dcbor.CBOR {
	return f.TaggedCBOR()
}

// DecodeFunction decodes an untagged CBOR value into a Function.
func DecodeFunction(cbor dcbor.CBOR) (Function, error) {
	switch cbor.Kind() {
	case dcbor.CBORKindUnsigned:
		value, ok := cbor.AsUnsigned()
		if !ok {
			return Function{}, fmt.Errorf("invalid function: expected unsigned integer")
		}
		return NewKnownFunctionNoName(value), nil
	case dcbor.CBORKindText:
		name, ok := cbor.AsText()
		if !ok {
			return Function{}, fmt.Errorf("invalid function: expected text")
		}
		return NewNamedFunction(name), nil
	default:
		return Function{}, fmt.Errorf("invalid function")
	}
}

// DecodeTaggedFunction decodes a tagged CBOR value into a Function.
func DecodeTaggedFunction(cbor dcbor.CBOR) (Function, error) {
	return dcbor.DecodeTagged(cbor, FunctionCBORTags(), DecodeFunction)
}

var (
	_ dcbor.CBORTaggedEncodable = Function{}
)

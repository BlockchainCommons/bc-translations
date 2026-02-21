package dcbor

import (
	"math"
)

// SimpleKind enumerates supported simple values.
type SimpleKind int

const (
	// SimpleFalse represents a CBOR false simple value.
	SimpleFalse SimpleKind = iota
	// SimpleTrue represents a CBOR true simple value.
	SimpleTrue
	// SimpleNull represents a CBOR null simple value.
	SimpleNull
	// SimpleFloat represents a CBOR floating-point simple value.
	SimpleFloat
)

// Simple represents CBOR major type 7 values supported by dCBOR.
type Simple struct {
	kind  SimpleKind
	float float64
}

// SimpleFalseValue returns the canonical false simple value.
func SimpleFalseValue() Simple {
	return Simple{kind: SimpleFalse}
}

// SimpleTrueValue returns the canonical true simple value.
func SimpleTrueValue() Simple {
	return Simple{kind: SimpleTrue}
}

// SimpleNullValue returns the canonical null simple value.
func SimpleNullValue() Simple {
	return Simple{kind: SimpleNull}
}

// SimpleFloatValue returns a floating-point simple value.
func SimpleFloatValue(v float64) Simple {
	return Simple{kind: SimpleFloat, float: v}
}

// Name returns the canonical diagnostic label for the value.
func (s Simple) Name() string {
	switch s.kind {
	case SimpleFalse:
		return "false"
	case SimpleTrue:
		return "true"
	case SimpleNull:
		return "null"
	default:
		return formatFloatDiagnostic(s.float)
	}
}

// String returns the canonical textual name for the simple value.
func (s Simple) String() string {
	return s.Name()
}

// IsFloat reports whether the simple value holds a float.
func (s Simple) IsFloat() bool {
	return s.kind == SimpleFloat
}

// IsNaN reports whether the simple value is a NaN float.
func (s Simple) IsNaN() bool {
	return s.kind == SimpleFloat && math.IsNaN(s.float)
}

// Kind returns the simple-value kind discriminator.
func (s Simple) Kind() SimpleKind {
	return s.kind
}

// Float64 returns the float payload when the value is float-typed.
func (s Simple) Float64() (float64, bool) {
	if s.kind != SimpleFloat {
		return 0, false
	}
	return s.float, true
}

// Equal reports value equality for simple values.
func (s Simple) Equal(other Simple) bool {
	if s.kind != other.kind {
		return false
	}
	if s.kind != SimpleFloat {
		return true
	}
	return s.float == other.float
}

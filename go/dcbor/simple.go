package dcbor

import (
	"math"
)

// SimpleKind enumerates supported simple values.
type SimpleKind int

const (
	SimpleFalse SimpleKind = iota
	SimpleTrue
	SimpleNull
	SimpleFloat
)

// Simple represents CBOR major type 7 values supported by dCBOR.
type Simple struct {
	kind  SimpleKind
	float float64
}

func SimpleFalseValue() Simple {
	return Simple{kind: SimpleFalse}
}

func SimpleTrueValue() Simple {
	return Simple{kind: SimpleTrue}
}

func SimpleNullValue() Simple {
	return Simple{kind: SimpleNull}
}

func SimpleFloatValue(v float64) Simple {
	return Simple{kind: SimpleFloat, float: v}
}

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

func (s Simple) IsFloat() bool {
	return s.kind == SimpleFloat
}

func (s Simple) IsNaN() bool {
	return s.kind == SimpleFloat && math.IsNaN(s.float)
}

func (s Simple) Kind() SimpleKind {
	return s.kind
}

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

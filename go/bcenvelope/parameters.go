package bcenvelope

// Well-known parameter constants for Gordian Envelope expressions.

// Parameter value constants.
const (
	ParameterBlankValue uint64 = 1
	ParameterLHSValue   uint64 = 2
	ParameterRHSValue   uint64 = 3
)

// Well-known parameter constants.
var (
	ParameterBlank = NewKnownParameter(ParameterBlankValue, "_")
	ParameterLHS   = NewKnownParameter(ParameterLHSValue, "lhs")
	ParameterRHS   = NewKnownParameter(ParameterRHSValue, "rhs")
)

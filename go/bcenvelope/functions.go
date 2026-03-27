package bcenvelope

// Well-known function constants for Gordian Envelope expressions.

// Function value constants.
const (
	FunctionAddValue uint64 = 1
	FunctionSubValue uint64 = 2
	FunctionMulValue uint64 = 3
	FunctionDivValue uint64 = 4
	FunctionNegValue uint64 = 5
	FunctionLTValue  uint64 = 6
	FunctionLEValue  uint64 = 7
	FunctionGTValue  uint64 = 8
	FunctionGEValue  uint64 = 9
	FunctionEQValue  uint64 = 10
	FunctionNEValue  uint64 = 11
	FunctionAndValue uint64 = 12
	FunctionOrValue  uint64 = 13
	FunctionXorValue uint64 = 14
	FunctionNotValue uint64 = 15
)

// Well-known function constants.
var (
	FunctionAdd = NewKnownFunction(FunctionAddValue, "add")
	FunctionSub = NewKnownFunction(FunctionSubValue, "sub")
	FunctionMul = NewKnownFunction(FunctionMulValue, "mul")
	FunctionDiv = NewKnownFunction(FunctionDivValue, "div")
	FunctionNeg = NewKnownFunction(FunctionNegValue, "neg")
	FunctionLT  = NewKnownFunction(FunctionLTValue, "lt")
	FunctionLE  = NewKnownFunction(FunctionLEValue, "le")
	FunctionGT  = NewKnownFunction(FunctionGTValue, "gt")
	FunctionGE  = NewKnownFunction(FunctionGEValue, "ge")
	FunctionEQ  = NewKnownFunction(FunctionEQValue, "eq")
	FunctionNE  = NewKnownFunction(FunctionNEValue, "ne")
	FunctionAnd = NewKnownFunction(FunctionAndValue, "and")
	FunctionOr  = NewKnownFunction(FunctionOrValue, "or")
	FunctionXor = NewKnownFunction(FunctionXorValue, "xor")
	FunctionNot = NewKnownFunction(FunctionNotValue, "not")
)

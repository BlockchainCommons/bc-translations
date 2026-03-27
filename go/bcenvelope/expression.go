package bcenvelope

import (
	"fmt"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
)

// Expression represents an expression in a Gordian Envelope.
//
// An expression consists of a function (the subject of the envelope) and zero
// or more parameters (as assertions on the envelope).
type Expression struct {
	function Function
	envelope *Envelope
}

// NewExpression creates a new expression with the given function.
func NewExpression(function Function) *Expression {
	return &Expression{
		function: function,
		envelope: EnvelopeFromCBOR(function.TaggedCBOR()),
	}
}

// NewExpressionFromString creates a new expression with a named function.
func NewExpressionFromString(name string) *Expression {
	return NewExpression(NewNamedFunction(name))
}

// Function returns the function identifier of the expression.
func (e *Expression) Function() Function { return e.function }

// ExpressionEnvelope returns the envelope that represents this expression.
func (e *Expression) ExpressionEnvelope() *Envelope { return e.envelope }

// WithParameter adds a parameter to the expression.
func (e *Expression) WithParameter(param Parameter, value *Envelope) *Expression {
	paramEnv := EnvelopeFromCBOR(param.TaggedCBOR())
	e.envelope = e.envelope.AddAssertion(paramEnv, value)
	return e
}

// WithParameterCBOR adds a parameter with a CBOR value to the expression.
func (e *Expression) WithParameterCBOR(param Parameter, value dcbor.CBOR) *Expression {
	return e.WithParameter(param, EnvelopeFromCBOR(value))
}

// WithOptionalParameter adds a parameter if the value is not nil.
func (e *Expression) WithOptionalParameter(param Parameter, value *Envelope) *Expression {
	if value != nil {
		return e.WithParameter(param, value)
	}
	return e
}

// ObjectForParameter returns the argument envelope for the given parameter.
func (e *Expression) ObjectForParameter(param Parameter) (*Envelope, error) {
	return e.envelope.ObjectForPredicate(EnvelopeFromCBOR(param.TaggedCBOR()))
}

// ObjectsForParameter returns all argument envelopes for the given parameter.
func (e *Expression) ObjectsForParameter(param Parameter) []*Envelope {
	return e.envelope.ObjectsForPredicate(EnvelopeFromCBOR(param.TaggedCBOR()))
}

// ExtractObjectForParameter extracts and decodes the argument for a parameter.
func ExtractObjectForParameter[T any](expr *Expression, param Parameter, decode func(dcbor.CBOR) (T, error)) (T, error) {
	obj, err := expr.ObjectForParameter(param)
	if err != nil {
		var zero T
		return zero, err
	}
	cbor, err := obj.TryLeaf()
	if err != nil {
		var zero T
		return zero, err
	}
	return decode(cbor)
}

// ExtractOptionalObjectForParameter extracts and decodes the optional argument for a parameter.
func ExtractOptionalObjectForParameter[T any](expr *Expression, param Parameter, decode func(dcbor.CBOR) (T, error)) (*T, error) {
	obj, err := expr.ObjectForParameter(param)
	if err != nil {
		// Not found is not an error for optional parameters
		return nil, nil
	}
	cbor, err := obj.TryLeaf()
	if err != nil {
		return nil, err
	}
	result, err := decode(cbor)
	if err != nil {
		return nil, err
	}
	return &result, nil
}

// ExtractObjectsForParameter extracts and decodes all arguments for a parameter.
func ExtractObjectsForParameter[T any](expr *Expression, param Parameter, decode func(dcbor.CBOR) (T, error)) ([]T, error) {
	objs := expr.ObjectsForParameter(param)
	var results []T
	for _, obj := range objs {
		cbor, err := obj.TryLeaf()
		if err != nil {
			return nil, err
		}
		result, err := decode(cbor)
		if err != nil {
			return nil, err
		}
		results = append(results, result)
	}
	return results, nil
}

// ToEnvelope converts the expression to an envelope.
func (e *Expression) ToEnvelope() *Envelope { return e.envelope }

// ExpressionFromEnvelope extracts an Expression from an Envelope.
func ExpressionFromEnvelope(envelope *Envelope) (*Expression, error) {
	subjectCBOR, err := envelope.Subject().TryLeaf()
	if err != nil {
		return nil, fmt.Errorf("expression subject is not a leaf: %w", err)
	}
	function, err := DecodeTaggedFunction(subjectCBOR)
	if err != nil {
		return nil, fmt.Errorf("expression subject is not a function: %w", err)
	}
	return &Expression{function: function, envelope: envelope}, nil
}

// ExpressionFromEnvelopeExpecting extracts an Expression from an Envelope,
// checking that it matches the expected function if provided.
func ExpressionFromEnvelopeExpecting(envelope *Envelope, expectedFunction *Function) (*Expression, error) {
	expression, err := ExpressionFromEnvelope(envelope)
	if err != nil {
		return nil, err
	}
	if expectedFunction != nil && !expression.function.Equal(*expectedFunction) {
		return nil, fmt.Errorf("expected function %v, but found %v", expectedFunction, expression.function)
	}
	return expression, nil
}

// String returns the formatted envelope representation of the expression.
func (e *Expression) String() string {
	return e.envelope.Format()
}

// Equal reports whether two expressions are equal.
func (e *Expression) Equal(other *Expression) bool {
	if e == nil || other == nil {
		return e == other
	}
	return e.function.Equal(other.function) && e.envelope.Digest() == other.envelope.Digest()
}

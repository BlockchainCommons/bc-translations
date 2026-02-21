package dcbor

import (
	"errors"
	"fmt"
)

var (
	ErrUnderrun            = errors.New("early end of CBOR data")
	ErrUnsupportedHeader   = errors.New("unsupported value in CBOR header")
	ErrNonCanonicalNumeric = errors.New("a CBOR numeric value was encoded in non-canonical form")
	ErrInvalidSimpleValue  = errors.New("an invalid CBOR simple value was encountered")
	ErrInvalidString       = errors.New("an invalidly-encoded UTF-8 string was encountered in the CBOR")
	ErrNonCanonicalString  = errors.New("a CBOR string was not encoded in Unicode Canonical Normalization Form C")
	ErrMisorderedMapKey    = errors.New("the decoded CBOR map has keys that are not in canonical order")
	ErrDuplicateMapKey     = errors.New("the decoded CBOR map has a duplicate key")
	ErrMissingMapKey       = errors.New("missing CBOR map key")
	ErrOutOfRange          = errors.New("the CBOR numeric value could not be represented in the specified numeric type")
	ErrWrongType           = errors.New("the decoded CBOR value was not the expected type")
)

// WrongTagError reports a mismatched tag.
type WrongTagError struct {
	Expected Tag
	Actual   Tag
}

// Error returns the mismatch description.
func (e WrongTagError) Error() string {
	return fmt.Sprintf("expected CBOR tag %s, but got %s", e.Expected.String(), e.Actual.String())
}

// InvalidDateError reports an invalid date string.
type InvalidDateError struct {
	Value string
}

// Error returns the invalid-date description.
func (e InvalidDateError) Error() string {
	return fmt.Sprintf("invalid ISO 8601 date string: %s", e.Value)
}

// CustomError holds free-form contextual error text.
type CustomError struct {
	Message string
}

// Error returns the wrapped custom message.
func (e CustomError) Error() string {
	return e.Message
}

// Errorf returns a CustomError with formatted text.
func Errorf(format string, args ...any) error {
	return CustomError{Message: fmt.Sprintf(format, args...)}
}

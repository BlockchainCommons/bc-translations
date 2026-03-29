package provenancemark

import (
	"fmt"
)

// ErrorCode identifies a provenance-mark failure category.
type ErrorCode string

const (
	ErrorInvalidSeedLength    ErrorCode = "InvalidSeedLength"
	ErrorDuplicateKey         ErrorCode = "DuplicateKey"
	ErrorMissingKey           ErrorCode = "MissingKey"
	ErrorInvalidKey           ErrorCode = "InvalidKey"
	ErrorExtraKeys            ErrorCode = "ExtraKeys"
	ErrorInvalidKeyLength     ErrorCode = "InvalidKeyLength"
	ErrorInvalidNextKeyLength ErrorCode = "InvalidNextKeyLength"
	ErrorInvalidChainIDLength ErrorCode = "InvalidChainIDLength"
	ErrorInvalidMessageLength ErrorCode = "InvalidMessageLength"
	ErrorInvalidInfoCBOR      ErrorCode = "InvalidInfoCbor"
	ErrorDateOutOfRange       ErrorCode = "DateOutOfRange"
	ErrorInvalidDate          ErrorCode = "InvalidDate"
	ErrorMissingURLParameter  ErrorCode = "MissingURLParameter"
	ErrorYearOutOfRange       ErrorCode = "YearOutOfRange"
	ErrorInvalidMonthOrDay    ErrorCode = "InvalidMonthOrDay"
	ErrorResolution           ErrorCode = "ResolutionError"
	ErrorBytewords            ErrorCode = "Bytewords"
	ErrorCBOR                 ErrorCode = "Cbor"
	ErrorURL                  ErrorCode = "URL"
	ErrorBase64               ErrorCode = "Base64"
	ErrorJSON                 ErrorCode = "JSON"
	ErrorEnvelope             ErrorCode = "Envelope"
	ErrorValidation           ErrorCode = "Validation"
)

// Error matches the Rust crate's public error surface while remaining idiomatic
// in Go's `(value, error)` style.
type Error struct {
	Code    ErrorCode
	Message string
	Err     error
}

// Error implements the error interface.
func (e *Error) Error() string {
	if e == nil {
		return "<nil>"
	}
	return e.Message
}

// Unwrap returns the wrapped cause, when present.
func (e *Error) Unwrap() error {
	if e == nil {
		return nil
	}
	return e.Err
}

func newError(code ErrorCode, format string, args ...any) error {
	return &Error{
		Code:    code,
		Message: fmt.Sprintf(format, args...),
	}
}

func wrapError(code ErrorCode, err error, format string, args ...any) error {
	return &Error{
		Code:    code,
		Message: fmt.Sprintf(format, args...),
		Err:     err,
	}
}

func newInvalidSeedLength(actual int) error {
	return newError(ErrorInvalidSeedLength, "invalid seed length: expected 32 bytes, got %d bytes", actual)
}

func newDuplicateKey(key string) error {
	return newError(ErrorDuplicateKey, "duplicate key: %s", key)
}

func newMissingKey(key string) error {
	return newError(ErrorMissingKey, "missing key: %s", key)
}

func newInvalidKey(key string) error {
	return newError(ErrorInvalidKey, "invalid key: %s", key)
}

func newExtraKeys(expected, actual int) error {
	return newError(ErrorExtraKeys, "wrong number of keys: expected %d, got %d", expected, actual)
}

func newInvalidKeyLength(expected, actual int) error {
	return newError(ErrorInvalidKeyLength, "invalid key length: expected %d, got %d", expected, actual)
}

func newInvalidNextKeyLength(expected, actual int) error {
	return newError(ErrorInvalidNextKeyLength, "invalid next key length: expected %d, got %d", expected, actual)
}

func newInvalidChainIDLength(expected, actual int) error {
	return newError(ErrorInvalidChainIDLength, "invalid chain ID length: expected %d, got %d", expected, actual)
}

func newInvalidMessageLength(expected, actual int) error {
	return newError(ErrorInvalidMessageLength, "invalid message length: expected at least %d, got %d", expected, actual)
}

func newInvalidInfoCBOR() error {
	return newError(ErrorInvalidInfoCBOR, "invalid CBOR data in info field")
}

func newDateOutOfRange(details string) error {
	return newError(ErrorDateOutOfRange, "date out of range: %s", details)
}

func newInvalidDate(details string) error {
	return newError(ErrorInvalidDate, "invalid date: %s", details)
}

func newMissingURLParameter(parameter string) error {
	return newError(ErrorMissingURLParameter, "missing required URL parameter: %s", parameter)
}

func newYearOutOfRange(year int) error {
	return newError(ErrorYearOutOfRange, "year out of range for 2-byte serialization: must be between 2023-2150, got %d", year)
}

func newInvalidMonthOrDay(year, month, day int) error {
	return newError(ErrorInvalidMonthOrDay, "invalid month (%d) or day (%d) for year %d", month, day, year)
}

func newResolutionError(details string) error {
	return newError(ErrorResolution, "resolution serialization error: %s", details)
}

func wrapBytewordsError(err error) error {
	return wrapError(ErrorBytewords, err, "bytewords error: %v", err)
}

func wrapCBORError(err error) error {
	return wrapError(ErrorCBOR, err, "CBOR error: %v", err)
}

func wrapURLError(err error) error {
	return wrapError(ErrorURL, err, "URL parsing error: %v", err)
}

func wrapBase64Error(err error) error {
	return wrapError(ErrorBase64, err, "base64 decoding error: %v", err)
}

func wrapJSONError(err error) error {
	return wrapError(ErrorJSON, err, "JSON error: %v", err)
}

func wrapEnvelopeError(err error) error {
	return wrapError(ErrorEnvelope, err, "envelope error: %v", err)
}

func wrapValidationIssue(issue ValidationIssue) error {
	return &Error{
		Code:    ErrorValidation,
		Message: fmt.Sprintf("validation error: %s", issue.Error()),
		Err:     issue,
	}
}

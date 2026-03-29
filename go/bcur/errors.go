// Package bcur provides Uniform Resources (UR) encoding for CBOR structures.
//
// It implements the UR specification for single-part and multipart encoding
// using bytewords and fountain codes, enabling efficient transmission of
// binary data via URI-friendly strings and QR codes.
package bcur

import (
	"errors"
	"fmt"
)

var (
	ErrInvalidScheme      = errors.New("invalid UR scheme")
	ErrTypeUnspecified    = errors.New("no UR type specified")
	ErrInvalidType        = errors.New("invalid UR type")
	ErrNotSinglePart      = errors.New("UR is not a single-part")
	ErrEmptyMessage       = errors.New("expected non-empty message")
	ErrEmptyPart          = errors.New("expected non-empty part")
	ErrInvalidFragmentLen = errors.New("expected positive maximum fragment length")
	ErrInconsistentPart   = errors.New("part is inconsistent with previous ones")
	ErrInvalidPadding     = errors.New("invalid padding")
	ErrInvalidWord        = errors.New("invalid byteword")
	ErrInvalidChecksum    = errors.New("invalid checksum")
	ErrInvalidLength      = errors.New("invalid bytewords length")
	ErrNonASCII           = errors.New("bytewords string contains non-ASCII characters")
	ErrNotMultiPart       = errors.New("can't decode single-part UR as multi-part")
	ErrInvalidIndices     = errors.New("invalid indices")
	ErrInvalidCharacters  = errors.New("type contains invalid characters")
	ErrCBORDecode         = errors.New("CBOR decode error")
	ErrCBORArrayLength    = errors.New("invalid CBOR array length")
)

// UnexpectedTypeError is returned when a UR type does not match the expected type.
type UnexpectedTypeError struct {
	Expected string
	Found    string
}

func (e *UnexpectedTypeError) Error() string {
	return fmt.Sprintf("expected UR type %s, but found %s", e.Expected, e.Found)
}

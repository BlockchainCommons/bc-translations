package bcenvelope

import (
	"errors"
	"fmt"
)

// Sentinel errors for base envelope operations.
var (
	ErrAlreadyElided      = errors.New("envelope was elided, so it cannot be compressed or encrypted")
	ErrAmbiguousPredicate = errors.New("more than one assertion matches the predicate")
	ErrInvalidDigest      = errors.New("digest did not match")
	ErrInvalidFormat      = errors.New("invalid format")
	ErrMissingDigest      = errors.New("a digest was expected but not found")
	ErrNonexistentPredicate = errors.New("no assertion matches the predicate")
	ErrNotWrapped         = errors.New("cannot unwrap an envelope that was not wrapped")
	ErrNotLeaf            = errors.New("the envelope's subject is not a leaf")
	ErrNotAssertion       = errors.New("the envelope's subject is not an assertion")
	ErrInvalidAssertion   = errors.New("assertion must be a map with exactly one element")
)

// Attachment extension errors.
var (
	ErrInvalidAttachment    = errors.New("invalid attachment")
	ErrNonexistentAttachment = errors.New("nonexistent attachment")
	ErrAmbiguousAttachment  = errors.New("ambiguous attachment")
)

// Compression extension errors.
var (
	ErrAlreadyCompressed = errors.New("envelope was already compressed")
	ErrNotCompressed     = errors.New("cannot decompress an envelope that was not compressed")
)

// Symmetric encryption extension errors.
var (
	ErrAlreadyEncrypted = errors.New("envelope was already encrypted or compressed, so it cannot be encrypted")
	ErrNotEncrypted     = errors.New("cannot decrypt an envelope that was not encrypted")
)

// Known value extension errors.
var (
	ErrNotKnownValue = errors.New("the envelope's subject is not a known value")
	ErrSubjectNotUnit = errors.New("the subject of the envelope is not the unit value")
)

// Public key encryption extension errors.
var (
	ErrUnknownRecipient = errors.New("unknown recipient")
)

// Encrypted key extension errors.
var (
	ErrUnknownSecret = errors.New("secret not found")
)

// Public key signing extension errors.
var (
	ErrUnverifiedSignature    = errors.New("could not verify a signature")
	ErrInvalidOuterSignatureType = errors.New("unexpected outer signature object type")
	ErrInvalidInnerSignatureType = errors.New("unexpected inner signature object type")
	ErrUnverifiedInnerSignature = errors.New("inner signature not made with same key as outer signature")
	ErrInvalidSignatureType     = errors.New("unexpected signature object type")
)

// SSKR extension errors.
var (
	ErrInvalidShares = errors.New("invalid SSKR shares")
)

// Types extension errors.
var (
	ErrInvalidType  = errors.New("invalid type")
	ErrAmbiguousType = errors.New("ambiguous type")
)

// Expression extension errors.
var (
	ErrUnexpectedResponseID = errors.New("unexpected response ID")
	ErrInvalidResponse      = errors.New("invalid response")
)

// Edge extension errors.
var (
	ErrEdgeMissingIsA         = errors.New("edge missing 'isA' assertion")
	ErrEdgeMissingSource      = errors.New("edge missing 'source' assertion")
	ErrEdgeMissingTarget      = errors.New("edge missing 'target' assertion")
	ErrEdgeDuplicateIsA       = errors.New("edge has duplicate 'isA' assertions")
	ErrEdgeDuplicateSource    = errors.New("edge has duplicate 'source' assertions")
	ErrEdgeDuplicateTarget    = errors.New("edge has duplicate 'target' assertions")
	ErrEdgeUnexpectedAssertion = errors.New("edge has unexpected assertion")
	ErrNonexistentEdge        = errors.New("nonexistent edge")
	ErrAmbiguousEdge          = errors.New("ambiguous edge")
)

// EnvelopeError wraps an upstream error with an envelope-specific context message.
type EnvelopeError struct {
	Message string
	Err     error
}

func (e *EnvelopeError) Error() string {
	if e.Err != nil {
		return fmt.Sprintf("%s: %v", e.Message, e.Err)
	}
	return e.Message
}

func (e *EnvelopeError) Unwrap() error {
	return e.Err
}

// WrapError wraps an upstream error with an envelope-specific message.
func WrapError(msg string, err error) error {
	return &EnvelopeError{Message: msg, Err: err}
}

// Errorf creates a new EnvelopeError with a formatted message and no wrapped error.
func Errorf(format string, args ...any) error {
	return &EnvelopeError{Message: fmt.Sprintf(format, args...)}
}

package bcenvelope

import (
	"fmt"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// Response represents a reply to a Request containing either a successful result or an error.
//
// When serialized to an envelope, responses are tagged with #6.40005 (TagResponse).
type Response struct {
	isSuccess bool
	id        *bccomponents.ARID // nil for early failure
	result    *Envelope          // non-nil for success
	errorVal  *Envelope          // non-nil for failure
}

// NewSuccessResponse creates a new successful response with the specified request ID.
// The default result is 'OK'.
func NewSuccessResponse(id bccomponents.ARID) *Response {
	return &Response{
		isSuccess: true,
		id:        &id,
		result:    OKEnvelope(),
	}
}

// NewFailureResponse creates a new failure response with the specified request ID.
// The default error is 'Unknown'.
func NewFailureResponse(id bccomponents.ARID) *Response {
	return &Response{
		isSuccess: false,
		id:        &id,
		errorVal:  UnknownEnvelope(),
	}
}

// NewEarlyFailureResponse creates a new failure response without a request ID.
// Used when the error happens before the request has been fully processed.
func NewEarlyFailureResponse() *Response {
	return &Response{
		isSuccess: false,
		id:        nil,
		errorVal:  UnknownEnvelope(),
	}
}

// UnknownEnvelope creates an envelope containing the 'Unknown' known value.
func UnknownEnvelope() *Envelope {
	return EnvelopeFromKnownValue(knownvalues.UnknownValue)
}

// OKEnvelope creates an envelope containing the 'OK' known value.
func OKEnvelope() *Envelope {
	return EnvelopeFromKnownValue(knownvalues.OKValue)
}

// IsSuccess returns true if this is a successful response.
func (r *Response) IsSuccess() bool { return r.isSuccess }

// IsError returns true if this is a failure response.
func (r *Response) IsError() bool { return !r.isSuccess }

// ID returns the ID of the request this response corresponds to, if known.
func (r *Response) ID() *bccomponents.ARID { return r.id }

// ExpectID returns the ID of the request, panicking if not known.
func (r *Response) ExpectID() bccomponents.ARID {
	if r.id == nil {
		panic("expected an ID")
	}
	return *r.id
}

// Result returns the result envelope for a successful response.
func (r *Response) Result() (*Envelope, error) {
	if !r.isSuccess {
		return nil, fmt.Errorf("cannot get result from failed response")
	}
	return r.result, nil
}

// Error returns the error envelope for a failure response.
func (r *Response) Error() (*Envelope, error) {
	if r.isSuccess {
		return nil, fmt.Errorf("cannot get error from successful response")
	}
	return r.errorVal, nil
}

// WithResult sets the result value for a successful response.
// Panics if called on a failure response.
func (r *Response) WithResult(result *Envelope) *Response {
	if !r.isSuccess {
		panic("cannot set result on a failed response")
	}
	r.result = result
	return r
}

// WithResultCBOR sets the result from a CBOR value.
func (r *Response) WithResultCBOR(value dcbor.CBOR) *Response {
	return r.WithResult(EnvelopeFromCBOR(value))
}

// WithOptionalResult sets the result if non-nil, otherwise sets null.
func (r *Response) WithOptionalResult(result *Envelope) *Response {
	if result != nil {
		return r.WithResult(result)
	}
	return r.WithResult(NullEnvelope())
}

// WithError sets the error value for a failure response.
// Panics if called on a successful response.
func (r *Response) WithError(errorVal *Envelope) *Response {
	if r.isSuccess {
		panic("cannot set error on a successful response")
	}
	r.errorVal = errorVal
	return r
}

// WithErrorCBOR sets the error from a CBOR value.
func (r *Response) WithErrorCBOR(value dcbor.CBOR) *Response {
	return r.WithError(EnvelopeFromCBOR(value))
}

// WithOptionalError sets the error if non-nil, otherwise leaves the default.
func (r *Response) WithOptionalError(errorVal *Envelope) *Response {
	if errorVal != nil {
		return r.WithError(errorVal)
	}
	return r
}

// Summary returns a human-readable summary of the response.
func (r *Response) Summary() string {
	if r.isSuccess {
		return fmt.Sprintf("id: %s, result: %s", r.id.ShortDescription(), r.result.FormatFlat())
	}
	if r.id != nil {
		return fmt.Sprintf("id: %s error: %s", r.id.ShortDescription(), r.errorVal.FormatFlat())
	}
	return fmt.Sprintf("id: 'Unknown' error: %s", r.errorVal.FormatFlat())
}

// String returns a display string for the response.
func (r *Response) String() string {
	return fmt.Sprintf("Response(%s)", r.Summary())
}

// ToEnvelope converts a Response to an Envelope.
func (r *Response) ToEnvelope() *Envelope {
	responseTag, _ := dcbor.GlobalTags.Get().TagForValue(bctags.TagResponse)
	if r.isSuccess {
		subject := EnvelopeFromCBOR(dcbor.ToTaggedValue(responseTag, r.id.TaggedCBOR()))
		return subject.AddAssertion(
			EnvelopeFromKnownValue(knownvalues.Result),
			r.result,
		)
	}

	var subject *Envelope
	if r.id != nil {
		subject = EnvelopeFromCBOR(dcbor.ToTaggedValue(responseTag, r.id.TaggedCBOR()))
	} else {
		subject = EnvelopeFromCBOR(dcbor.ToTaggedValue(responseTag, knownvalues.UnknownValue.TaggedCBOR()))
	}
	return subject.AddAssertion(
		EnvelopeFromKnownValue(knownvalues.Error),
		r.errorVal,
	)
}

// ResponseFromEnvelope extracts a Response from an Envelope.
func ResponseFromEnvelope(envelope *Envelope) (*Response, error) {
	resultEnv, resultErr := envelope.ObjectForPredicate(EnvelopeFromKnownValue(knownvalues.Result))
	errorEnv, errorErr := envelope.ObjectForPredicate(EnvelopeFromKnownValue(knownvalues.Error))

	hasResult := resultErr == nil
	hasError := errorErr == nil

	if hasResult == hasError {
		return nil, fmt.Errorf("invalid response: must have exactly one of result or error")
	}

	responseTag, _ := dcbor.GlobalTags.Get().TagForValue(bctags.TagResponse)

	if hasResult {
		subjectCBOR, err := envelope.Subject().TryLeaf()
		if err != nil {
			return nil, fmt.Errorf("response subject is not a leaf: %w", err)
		}
		untaggedValue, err := subjectCBOR.TryIntoExpectedTaggedValue(responseTag)
		if err != nil {
			return nil, fmt.Errorf("response subject is not tagged with TAG_RESPONSE: %w", err)
		}
		id, err := bccomponents.DecodeTaggedARID(untaggedValue)
		if err != nil {
			return nil, fmt.Errorf("response subject ARID decode failed: %w", err)
		}
		return &Response{
			isSuccess: true,
			id:        &id,
			result:    resultEnv,
		}, nil
	}

	// Error response
	subjectCBOR, err := envelope.Subject().TryLeaf()
	if err != nil {
		return nil, fmt.Errorf("response subject is not a leaf: %w", err)
	}
	untaggedValue, err := subjectCBOR.TryIntoExpectedTaggedValue(responseTag)
	if err != nil {
		return nil, fmt.Errorf("response subject is not tagged with TAG_RESPONSE: %w", err)
	}

	// Check if the untagged value is a tagged known value ('Unknown')
	kv, kvErr := knownvalues.DecodeTaggedKnownValue(untaggedValue)
	if kvErr == nil && kv.Equal(knownvalues.UnknownValue) {
		// Early failure: no ID
		return &Response{
			isSuccess: false,
			id:        nil,
			errorVal:  errorEnv,
		}, nil
	}

	// Regular failure with ID
	id, err := bccomponents.DecodeTaggedARID(untaggedValue)
	if err != nil {
		return nil, fmt.Errorf("response error subject ARID decode failed: %w", err)
	}
	return &Response{
		isSuccess: false,
		id:        &id,
		errorVal:  errorEnv,
	}, nil
}

// Equal reports whether two responses are equal.
func (r *Response) Equal(other *Response) bool {
	if r == nil || other == nil {
		return r == other
	}
	if r.isSuccess != other.isSuccess {
		return false
	}
	if (r.id == nil) != (other.id == nil) {
		return false
	}
	if r.id != nil && other.id != nil && *r.id != *other.id {
		return false
	}
	if r.isSuccess {
		return r.result.Digest() == other.result.Digest()
	}
	return r.errorVal.Digest() == other.errorVal.Digest()
}

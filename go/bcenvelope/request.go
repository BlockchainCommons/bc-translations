package bcenvelope

import (
	"fmt"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// Request represents a message requesting execution of a function with parameters.
//
// Each request:
//   - Contains a body (an Expression) that represents the function to be executed
//   - Has a unique identifier (ARID) for tracking and correlation
//   - May include optional metadata like a note and timestamp
//
// When serialized to an envelope, requests are tagged with #6.40004 (TagRequest).
type Request struct {
	body *Expression
	id   bccomponents.ARID
	note string
	date *dcbor.Date
}

// NewRequest creates a new request with a function and ID.
func NewRequest(function Function, id bccomponents.ARID) *Request {
	return &Request{
		body: NewExpression(function),
		id:   id,
	}
}

// NewRequestFromString creates a new request with a named function and ID.
func NewRequestFromString(name string, id bccomponents.ARID) *Request {
	return NewRequest(NewNamedFunction(name), id)
}

// NewRequestWithBody creates a new request with a pre-built expression body and ID.
func NewRequestWithBody(body *Expression, id bccomponents.ARID) *Request {
	return &Request{
		body: body,
		id:   id,
	}
}

// Body returns the body of the request (the expression to be evaluated).
func (r *Request) Body() *Expression { return r.body }

// ID returns the unique identifier (ARID) of the request.
func (r *Request) ID() bccomponents.ARID { return r.id }

// Note returns the note attached to the request, or empty string if none.
func (r *Request) Note() string { return r.note }

// Date returns the date attached to the request, if any.
func (r *Request) Date() *dcbor.Date { return r.date }

// Function returns the function of the request.
func (r *Request) Function() Function { return r.body.Function() }

// ExpressionEnvelope returns the expression envelope of the request.
func (r *Request) ExpressionEnvelope() *Envelope { return r.body.ExpressionEnvelope() }

// WithParameter adds a parameter to the request.
func (r *Request) WithParameter(param Parameter, value *Envelope) *Request {
	r.body = r.body.WithParameter(param, value)
	return r
}

// WithParameterCBOR adds a parameter with a CBOR value.
func (r *Request) WithParameterCBOR(param Parameter, value dcbor.CBOR) *Request {
	r.body = r.body.WithParameterCBOR(param, value)
	return r
}

// WithOptionalParameter adds a parameter if the value is not nil.
func (r *Request) WithOptionalParameter(param Parameter, value *Envelope) *Request {
	r.body = r.body.WithOptionalParameter(param, value)
	return r
}

// WithNote adds a note to the request.
func (r *Request) WithNote(note string) *Request {
	r.note = note
	return r
}

// WithDate adds a date to the request.
func (r *Request) WithDate(date dcbor.Date) *Request {
	r.date = &date
	return r
}

// ObjectForParameter returns the argument envelope for the given parameter.
func (r *Request) ObjectForParameter(param Parameter) (*Envelope, error) {
	return r.body.ObjectForParameter(param)
}

// ObjectsForParameter returns all argument envelopes for the given parameter.
func (r *Request) ObjectsForParameter(param Parameter) []*Envelope {
	return r.body.ObjectsForParameter(param)
}

// Summary returns a human-readable summary of the request.
func (r *Request) Summary() string {
	return fmt.Sprintf("id: %s, body: %s", r.id.ShortDescription(), r.body.ExpressionEnvelope().FormatFlat())
}

// String returns a display string for the request.
func (r *Request) String() string {
	return fmt.Sprintf("Request(%s)", r.Summary())
}

// ToEnvelope converts a Request to an Envelope.
func (r *Request) ToEnvelope() *Envelope {
	requestTag, _ := dcbor.GlobalTags.Get().TagForValue(bctags.TagRequest)
	subject := EnvelopeFromCBOR(dcbor.ToTaggedValue(requestTag, r.id.TaggedCBOR()))
	result := subject.AddAssertion(
		EnvelopeFromKnownValue(knownvalues.Body),
		r.body.ToEnvelope(),
	)
	if r.note != "" {
		result = result.AddAssertion(
			EnvelopeFromKnownValue(knownvalues.Note),
			r.note,
		)
	}
	if r.date != nil {
		result = result.AddAssertion(
			EnvelopeFromKnownValue(knownvalues.Date),
			EnvelopeFromCBOR(r.date.ToCBOR()),
		)
	}
	return result
}

// RequestFromEnvelope extracts a Request from an Envelope.
func RequestFromEnvelope(envelope *Envelope) (*Request, error) {
	return RequestFromEnvelopeExpecting(envelope, nil)
}

// RequestFromEnvelopeExpecting extracts a Request from an Envelope,
// optionally checking for an expected function.
func RequestFromEnvelopeExpecting(envelope *Envelope, expectedFunction *Function) (*Request, error) {
	bodyEnvelope, err := envelope.ObjectForPredicate(EnvelopeFromKnownValue(knownvalues.Body))
	if err != nil {
		return nil, fmt.Errorf("request missing body: %w", err)
	}
	body, err := ExpressionFromEnvelopeExpecting(bodyEnvelope, expectedFunction)
	if err != nil {
		return nil, err
	}

	// Extract ARID from subject
	subjectCBOR, err := envelope.Subject().TryLeaf()
	if err != nil {
		return nil, fmt.Errorf("request subject is not a leaf: %w", err)
	}
	requestTag, _ := dcbor.GlobalTags.Get().TagForValue(bctags.TagRequest)
	untaggedValue, err := subjectCBOR.TryIntoExpectedTaggedValue(requestTag)
	if err != nil {
		return nil, fmt.Errorf("request subject is not tagged with TAG_REQUEST: %w", err)
	}
	id, err := bccomponents.DecodeTaggedARID(untaggedValue)
	if err != nil {
		return nil, fmt.Errorf("request subject ARID decode failed: %w", err)
	}

	req := &Request{
		body: body,
		id:   id,
	}

	// Extract note (optional, default "")
	noteEnv, err := envelope.ObjectForPredicate(EnvelopeFromKnownValue(knownvalues.Note))
	if err == nil {
		noteCBOR, err2 := noteEnv.TryLeaf()
		if err2 == nil {
			if text, ok := noteCBOR.AsText(); ok {
				req.note = text
			}
		}
	}

	// Extract date (optional)
	dateEnv, err := envelope.ObjectForPredicate(EnvelopeFromKnownValue(knownvalues.Date))
	if err == nil {
		dateCBOR, err2 := dateEnv.TryLeaf()
		if err2 == nil {
			date, err3 := dcbor.DateFromTaggedCBOR(dateCBOR)
			if err3 == nil {
				req.date = &date
			}
		}
	}

	return req, nil
}

// Equal reports whether two requests are equal.
func (r *Request) Equal(other *Request) bool {
	if r == nil || other == nil {
		return r == other
	}
	if r.id != other.id {
		return false
	}
	if !r.body.Equal(other.body) {
		return false
	}
	if r.note != other.note {
		return false
	}
	if (r.date == nil) != (other.date == nil) {
		return false
	}
	if r.date != nil && other.date != nil && !r.date.Equal(*other.date) {
		return false
	}
	return true
}

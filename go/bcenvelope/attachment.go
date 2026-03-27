package bcenvelope

import (
	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// decodeStringFromCBOR decodes a string from a CBOR text value.
func decodeStringFromCBOR(cbor dcbor.CBOR) (string, error) {
	return cbor.TryIntoText()
}

// --- Assertion-level attachment methods ---

// NewAttachmentAssertion creates a new attachment assertion. An attachment
// assertion has the predicate 'attachment' and an object that is a wrapped
// envelope containing the payload with 'vendor' and optional 'conformsTo'
// assertions.
func NewAttachmentAssertion(
	payload any,
	vendor string,
	conformsTo *string,
) *Assertion {
	payloadEnvelope := NewEnvelope(payload).Wrap()
	payloadEnvelope = payloadEnvelope.AddAssertion(knownvalues.Vendor, vendor)
	if conformsTo != nil {
		payloadEnvelope = payloadEnvelope.AddAssertion(knownvalues.ConformsTo, *conformsTo)
	}
	return NewAssertion(
		NewKnownValueEnvelope(knownvalues.Attachment),
		payloadEnvelope,
	)
}

// AttachmentPayload returns the payload of an attachment assertion.
func (a *Assertion) AttachmentPayload() (*Envelope, error) {
	return a.Object().Unwrap()
}

// AttachmentVendor returns the vendor identifier of an attachment assertion.
func (a *Assertion) AttachmentVendor() (string, error) {
	return ExtractObjectForPredicate(a.Object(), knownvalues.Vendor, decodeStringFromCBOR)
}

// AttachmentConformsTo returns the optional conformsTo URI of an attachment
// assertion.
func (a *Assertion) AttachmentConformsTo() (*string, error) {
	return ExtractOptionalObjectForPredicate(a.Object(), knownvalues.ConformsTo, decodeStringFromCBOR)
}

// ValidateAttachment validates that an assertion is a proper attachment
// assertion with the required structure.
func (a *Assertion) ValidateAttachment() error {
	payload, err := a.AttachmentPayload()
	if err != nil {
		return ErrInvalidAttachment
	}
	vendor, err := a.AttachmentVendor()
	if err != nil {
		return ErrInvalidAttachment
	}
	conformsTo, err := a.AttachmentConformsTo()
	if err != nil {
		return ErrInvalidAttachment
	}
	var conformsToStr *string
	if conformsTo != nil {
		conformsToStr = conformsTo
	}
	expected := NewAttachmentAssertion(payload, vendor, conformsToStr)
	expectedEnvelope := expected.ToEnvelope()
	actualEnvelope := a.ToEnvelope()
	if !expectedEnvelope.IsEquivalentTo(actualEnvelope) {
		return ErrInvalidAttachment
	}
	return nil
}

// --- Envelope-level attachment methods ---

// NewAttachmentEnvelope creates a new envelope with an attachment as its subject.
func NewAttachmentEnvelope(
	payload any,
	vendor string,
	conformsTo *string,
) *Envelope {
	return NewAttachmentAssertion(payload, vendor, conformsTo).ToEnvelope()
}

// AddAttachment returns a new envelope with an added attachment assertion.
func (e *Envelope) AddAttachment(
	payload any,
	vendor string,
	conformsTo *string,
) *Envelope {
	assertion := NewAttachmentAssertion(payload, vendor, conformsTo)
	result, err := e.AddAssertionEnvelope(assertion.ToEnvelope())
	if err != nil {
		panic("bcenvelope: AddAttachment: " + err.Error())
	}
	return result
}

// AttachmentPayload returns the payload of an attachment envelope.
func (e *Envelope) AttachmentPayload() (*Envelope, error) {
	a, ok := e.Case().(*AssertionCase)
	if !ok {
		return nil, ErrInvalidAttachment
	}
	return a.Assertion.AttachmentPayload()
}

// AttachmentVendor returns the vendor identifier of an attachment envelope.
func (e *Envelope) AttachmentVendor() (string, error) {
	a, ok := e.Case().(*AssertionCase)
	if !ok {
		return "", ErrInvalidAttachment
	}
	return a.Assertion.AttachmentVendor()
}

// AttachmentConformsTo returns the optional conformsTo URI of an attachment
// envelope.
func (e *Envelope) AttachmentConformsTo() (*string, error) {
	a, ok := e.Case().(*AssertionCase)
	if !ok {
		return nil, ErrInvalidAttachment
	}
	return a.Assertion.AttachmentConformsTo()
}

// Attachments returns all attachment envelopes from this envelope's assertions.
func (e *Envelope) Attachments() ([]*Envelope, error) {
	return e.AttachmentsWithVendorAndConformsTo(nil, nil)
}

// AttachmentsWithVendorAndConformsTo searches the envelope's assertions for
// attachments matching the given vendor and conformsTo. If vendor is nil,
// matches any vendor. If conformsTo is nil, matches any conformsTo value.
func (e *Envelope) AttachmentsWithVendorAndConformsTo(
	vendor *string,
	conformsTo *string,
) ([]*Envelope, error) {
	assertions := e.AssertionsWithPredicate(knownvalues.Attachment)

	// Validate all attachment assertions first
	for _, assertion := range assertions {
		if err := e.validateAttachmentEnvelope(assertion); err != nil {
			return nil, err
		}
	}

	var matching []*Envelope
	for _, assertion := range assertions {
		if vendor != nil {
			v, err := assertion.AttachmentVendor()
			if err != nil || v != *vendor {
				continue
			}
		}

		if conformsTo != nil {
			c, err := assertion.AttachmentConformsTo()
			if err != nil {
				continue
			}
			if c == nil || *c != *conformsTo {
				continue
			}
		}

		matching = append(matching, assertion)
	}

	return matching, nil
}

// AttachmentWithVendorAndConformsTo finds a single attachment matching the
// given vendor and conformsTo. Returns ErrNonexistentAttachment if none
// match, or ErrAmbiguousAttachment if more than one matches.
func (e *Envelope) AttachmentWithVendorAndConformsTo(
	vendor *string,
	conformsTo *string,
) (*Envelope, error) {
	attachments, err := e.AttachmentsWithVendorAndConformsTo(vendor, conformsTo)
	if err != nil {
		return nil, err
	}
	if len(attachments) == 0 {
		return nil, ErrNonexistentAttachment
	}
	if len(attachments) > 1 {
		return nil, ErrAmbiguousAttachment
	}
	return attachments[0], nil
}

// ValidateAttachment validates that an envelope is a proper attachment
// envelope.
func (e *Envelope) ValidateAttachment() error {
	a, ok := e.Case().(*AssertionCase)
	if !ok {
		return ErrInvalidAttachment
	}
	return a.Assertion.ValidateAttachment()
}

// validateAttachmentEnvelope validates an attachment envelope (called during
// search).
func (e *Envelope) validateAttachmentEnvelope(attachment *Envelope) error {
	a, ok := attachment.Case().(*AssertionCase)
	if !ok {
		return ErrInvalidAttachment
	}
	return a.Assertion.ValidateAttachment()
}

// --- Attachments container ---

// Attachments is a container for vendor-specific metadata attachments,
// stored as envelopes keyed by their digest.
type AttachmentsContainer struct {
	envelopes map[bccomponents.Digest]*Envelope
}

// NewAttachmentsContainer creates a new empty attachments container.
func NewAttachmentsContainer() *AttachmentsContainer {
	return &AttachmentsContainer{
		envelopes: make(map[bccomponents.Digest]*Envelope),
	}
}

// Add adds a new attachment with the specified payload and metadata.
func (a *AttachmentsContainer) Add(payload any, vendor string, conformsTo *string) {
	attachment := NewAttachmentEnvelope(payload, vendor, conformsTo)
	a.envelopes[attachment.Digest()] = attachment
}

// Get retrieves an attachment by its digest.
func (a *AttachmentsContainer) Get(digest bccomponents.Digest) *Envelope {
	return a.envelopes[digest]
}

// Remove removes an attachment by its digest and returns it.
func (a *AttachmentsContainer) Remove(digest bccomponents.Digest) *Envelope {
	env := a.envelopes[digest]
	delete(a.envelopes, digest)
	return env
}

// Clear removes all attachments.
func (a *AttachmentsContainer) Clear() {
	a.envelopes = make(map[bccomponents.Digest]*Envelope)
}

// IsEmpty returns whether the container has any attachments.
func (a *AttachmentsContainer) IsEmpty() bool {
	return len(a.envelopes) == 0
}

// AddToEnvelope adds all attachments as assertion envelopes to the given
// envelope.
func (a *AttachmentsContainer) AddToEnvelope(envelope *Envelope) *Envelope {
	result := envelope
	for _, env := range a.envelopes {
		r, err := result.AddAssertionEnvelope(env)
		if err != nil {
			panic("bcenvelope: AttachmentsContainer.AddToEnvelope: " + err.Error())
		}
		result = r
	}
	return result
}

// AttachmentsContainerFromEnvelope extracts attachments from an envelope's
// 'attachment' assertions.
func AttachmentsContainerFromEnvelope(envelope *Envelope) (*AttachmentsContainer, error) {
	attachmentEnvelopes, err := envelope.Attachments()
	if err != nil {
		return nil, err
	}
	container := NewAttachmentsContainer()
	for _, attachment := range attachmentEnvelopes {
		container.envelopes[attachment.Digest()] = attachment
	}
	return container, nil
}

// Attachable is implemented by types that can have metadata attachments.
type Attachable interface {
	AttachmentsContainer() *AttachmentsContainer
}

package bcenvelope

import (
	"fmt"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// Event represents a notification or message that doesn't expect a response.
//
// Unlike Request and Response which form a pair, an Event is a standalone message
// that can be used for broadcasting information, logging, or publishing notifications.
//
// When serialized to an envelope, events are tagged with #6.40026 (TagEvent).
type Event struct {
	content *Envelope
	id      bccomponents.ARID
	note    string
	date    *dcbor.Date
}

// NewEvent creates a new event with the specified content envelope and ID.
func NewEvent(content *Envelope, id bccomponents.ARID) *Event {
	return &Event{
		content: content,
		id:      id,
	}
}

// NewEventFromCBOR creates a new event with a CBOR value as content.
func NewEventFromCBOR(value dcbor.CBOR, id bccomponents.ARID) *Event {
	return NewEvent(EnvelopeFromCBOR(value), id)
}

// NewEventFromString creates a new event with a string value as content.
func NewEventFromString(text string, id bccomponents.ARID) *Event {
	return NewEventFromCBOR(dcbor.NewCBORText(text), id)
}

// Content returns the content of the event.
func (e *Event) Content() *Envelope { return e.content }

// ID returns the unique identifier (ARID) of the event.
func (e *Event) ID() bccomponents.ARID { return e.id }

// Note returns the note attached to the event, or empty string if none.
func (e *Event) Note() string { return e.note }

// Date returns the date attached to the event, if any.
func (e *Event) Date() *dcbor.Date { return e.date }

// WithNote adds a note to the event.
func (e *Event) WithNote(note string) *Event {
	e.note = note
	return e
}

// WithDate adds a date to the event.
func (e *Event) WithDate(date dcbor.Date) *Event {
	e.date = &date
	return e
}

// Summary returns a human-readable summary of the event.
func (e *Event) Summary() string {
	return fmt.Sprintf("id: %s, content: %s", e.id.ShortDescription(), e.content.FormatFlat())
}

// String returns a display string for the event.
func (e *Event) String() string {
	return fmt.Sprintf("Event(%s)", e.Summary())
}

// ToEnvelope converts an Event to an Envelope.
func (e *Event) ToEnvelope() *Envelope {
	eventTag, _ := dcbor.GlobalTags.Get().TagForValue(bctags.TagEvent)
	subject := EnvelopeFromCBOR(dcbor.ToTaggedValue(eventTag, e.id.TaggedCBOR()))
	result := subject.AddAssertion(
		EnvelopeFromKnownValue(knownvalues.Content),
		e.content,
	)
	if e.note != "" {
		result = result.AddAssertion(
			EnvelopeFromKnownValue(knownvalues.Note),
			e.note,
		)
	}
	if e.date != nil {
		result = result.AddAssertion(
			EnvelopeFromKnownValue(knownvalues.Date),
			EnvelopeFromCBOR(e.date.ToCBOR()),
		)
	}
	return result
}

// EventFromEnvelope extracts an Event from an Envelope.
func EventFromEnvelope(envelope *Envelope) (*Event, error) {
	contentEnv, err := envelope.ObjectForPredicate(EnvelopeFromKnownValue(knownvalues.Content))
	if err != nil {
		return nil, fmt.Errorf("event missing content: %w", err)
	}

	// Extract ARID from subject
	subjectCBOR, err := envelope.Subject().TryLeaf()
	if err != nil {
		return nil, fmt.Errorf("event subject is not a leaf: %w", err)
	}
	eventTag, _ := dcbor.GlobalTags.Get().TagForValue(bctags.TagEvent)
	untaggedValue, err := subjectCBOR.TryIntoExpectedTaggedValue(eventTag)
	if err != nil {
		return nil, fmt.Errorf("event subject is not tagged with TAG_EVENT: %w", err)
	}
	id, err := bccomponents.DecodeTaggedARID(untaggedValue)
	if err != nil {
		return nil, fmt.Errorf("event subject ARID decode failed: %w", err)
	}

	event := &Event{
		content: contentEnv,
		id:      id,
	}

	// Extract note (optional)
	noteEnv, err := envelope.ObjectForPredicate(EnvelopeFromKnownValue(knownvalues.Note))
	if err == nil {
		noteCBOR, err2 := noteEnv.TryLeaf()
		if err2 == nil {
			if text, ok := noteCBOR.AsText(); ok {
				event.note = text
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
				event.date = &date
			}
		}
	}

	return event, nil
}

// Equal reports whether two events are equal.
func (e *Event) Equal(other *Event) bool {
	if e == nil || other == nil {
		return e == other
	}
	if e.id != other.id {
		return false
	}
	if e.content.Digest() != other.content.Digest() {
		return false
	}
	if e.note != other.note {
		return false
	}
	if (e.date == nil) != (other.date == nil) {
		return false
	}
	if e.date != nil && other.date != nil && !e.date.Equal(*other.date) {
		return false
	}
	return true
}

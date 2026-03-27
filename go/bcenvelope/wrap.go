package bcenvelope

// Wrap returns a new envelope that wraps the current envelope.
func (e *Envelope) Wrap() *Envelope {
	return newWrapped(e)
}

// TryUnwrap extracts the inner envelope from a wrapped envelope.
// Returns an error if this is not a wrapped envelope.
func (e *Envelope) TryUnwrap() (*Envelope, error) {
	switch c := e.Subject().envelopeCase.(type) {
	case *WrappedCase:
		return c.Envelope, nil
	default:
		return nil, ErrNotWrapped
	}
}

// Unwrap is an alias for TryUnwrap for compatibility with extension code.
func (e *Envelope) Unwrap() (*Envelope, error) {
	return e.TryUnwrap()
}

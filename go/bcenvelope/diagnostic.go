package bcenvelope

import dcbor "github.com/nickel-blockchaincommons/dcbor-go"

// DiagnosticAnnotated returns the CBOR diagnostic notation for this envelope,
// with tag name annotations.
func (e *Envelope) DiagnosticAnnotated() string {
	return WithFormatContext(func(context *FormatContext) string {
		return e.TaggedCBOR().DiagnosticOpt(
			dcbor.DiagFormatOpts{}.Annotate(true).Tags(dcbor.TagsCustom(context.Tags())),
		)
	})
}

// Diagnostic returns the CBOR diagnostic notation for this envelope.
func (e *Envelope) Diagnostic() string {
	return e.TaggedCBOR().Diagnostic()
}

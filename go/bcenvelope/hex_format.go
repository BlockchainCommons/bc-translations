package bcenvelope

import dcbor "github.com/nickel-blockchaincommons/dcbor-go"

// HexFormatOpts controls annotated hex formatting for envelopes.
type HexFormatOpts struct {
	Annotate bool
	Context  FormatContextOpt
}

// DefaultHexFormatOpts returns default hex format options (annotated, global context).
func DefaultHexFormatOpts() HexFormatOpts {
	return HexFormatOpts{
		Annotate: true,
		Context:  FormatOptGlobal(),
	}
}

func (opts HexFormatOpts) toDcborHexOpts() dcbor.HexFormatOpts {
	return dcbor.HexFormatOpts{}.Annotate(opts.Annotate).Context(opts.Context.ToTagsStoreOpt())
}

// HexOpt returns the CBOR hex dump with the given options.
func (e *Envelope) HexOpt(opts HexFormatOpts) string {
	cbor := e.TaggedCBOR()
	return cbor.HexOpt(opts.toDcborHexOpts())
}

// Hex returns the annotated CBOR hex dump of this envelope.
func (e *Envelope) Hex() string {
	return e.HexOpt(DefaultHexFormatOpts())
}

package bcenvelope

import (
	"fmt"
	"math"
	"strings"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// Summary returns a short summary of the envelope's content with a maximum length.
func (e *Envelope) Summary(maxLength int, context *FormatContext) string {
	switch c := e.Case().(type) {
	case *NodeCase:
		return "NODE"
	case *LeafCase:
		result, err := envelopeSummaryCBOR(*c.CBOR, maxLength, FormatOptCustom(context))
		if err != nil {
			return "<error>"
		}
		return result
	case *WrappedCase:
		return "WRAPPED"
	case *AssertionCase:
		return "ASSERTION"
	case *ElidedCase:
		return "ELIDED"
	case *KnownValueCase:
		kv := knownvalues.KnownValueForRawValue(c.Value.Value(), context.KnownValues())
		return "'" + kv.String() + "'"
	case *EncryptedCase:
		return "ENCRYPTED"
	case *CompressedCase:
		return "COMPRESSED"
	default:
		return "<unknown>"
	}
}

// envelopeSummaryCBOR returns a summary string for a CBOR value.
func envelopeSummaryCBOR(cbor dcbor.CBOR, maxLength int, contextOpt FormatContextOpt) (string, error) {
	switch cbor.Kind() {
	case dcbor.CBORKindUnsigned:
		v, _ := cbor.AsUnsigned()
		return fmt.Sprintf("%d", v), nil
	case dcbor.CBORKindNegative:
		v, _ := cbor.AsInt64()
		return fmt.Sprintf("%d", v), nil
	case dcbor.CBORKindByteString:
		data, _ := cbor.AsByteString()
		return fmt.Sprintf("Bytes(%d)", len(data)), nil
	case dcbor.CBORKindText:
		text, _ := cbor.AsText()
		if maxLength < math.MaxInt && len(text) > maxLength {
			runes := []rune(text)
			if len(runes) > maxLength {
				text = string(runes[:maxLength]) + "\u2026"
			}
		}
		text = strings.ReplaceAll(text, "\n", "\\n")
		return fmt.Sprintf("%q", text), nil
	case dcbor.CBORKindSimple:
		return cbor.DiagnosticOpt(dcbor.DiagFormatOpts{}), nil
	case dcbor.CBORKindArray, dcbor.CBORKindMap, dcbor.CBORKindTagged:
		return summarizeWithContext(cbor, contextOpt), nil
	default:
		return "<unknown>", nil
	}
}

func summarizeWithContext(cbor dcbor.CBOR, contextOpt FormatContextOpt) string {
	switch contextOpt.Mode {
	case FormatContextNone:
		return cbor.DiagnosticOpt(dcbor.DiagFormatOpts{}.Summarize(true).Tags(dcbor.TagsNone()))
	case FormatContextGlobal:
		return WithFormatContext(func(ctx *FormatContext) string {
			return cbor.DiagnosticOpt(dcbor.DiagFormatOpts{}.Summarize(true).Tags(dcbor.TagsCustom(ctx.Tags())))
		})
	case FormatContextCustom:
		if contextOpt.Context != nil {
			return cbor.DiagnosticOpt(dcbor.DiagFormatOpts{}.Summarize(true).Tags(dcbor.TagsCustom(contextOpt.Context.Tags())))
		}
		return cbor.DiagnosticOpt(dcbor.DiagFormatOpts{}.Summarize(true).Tags(dcbor.TagsNone()))
	default:
		return cbor.DiagnosticOpt(dcbor.DiagFormatOpts{}.Summarize(true).Tags(dcbor.TagsNone()))
	}
}

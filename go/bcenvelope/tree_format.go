package bcenvelope

import (
	"strings"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	bcur "github.com/nickel-blockchaincommons/bcur-go"
)

// DigestDisplayFormat controls how digests are shown in tree output.
type DigestDisplayFormat int

const (
	// DigestDisplayShort shows the first 8 hex characters of the digest.
	DigestDisplayShort DigestDisplayFormat = iota
	// DigestDisplayFull shows the full hex digest.
	DigestDisplayFull
	// DigestDisplayUR shows a ur:digest UR string.
	DigestDisplayUR
)

// TreeFormatOpts configures tree formatting.
type TreeFormatOpts struct {
	HideNodes          bool
	HighlightingTarget map[bccomponents.Digest]bool
	Context            FormatContextOpt
	DigestDisplay      DigestDisplayFormat
}

// DefaultTreeFormatOpts returns default tree format options.
func DefaultTreeFormatOpts() TreeFormatOpts {
	return TreeFormatOpts{
		HighlightingTarget: make(map[bccomponents.Digest]bool),
		Context:            FormatOptGlobal(),
	}
}

// TreeFormat returns a tree-formatted string representation with default options.
func (e *Envelope) TreeFormat() string {
	opts := DefaultTreeFormatOpts()
	return e.TreeFormatOpt(opts)
}

// TreeFormatOpt returns a tree-formatted string representation with the specified options.
func (e *Envelope) TreeFormatOpt(opts TreeFormatOpts) string {
	var elements []treeElement
	visitor := func(env *Envelope, level int, incomingEdge EdgeType, _ any) (any, bool) {
		elem := treeElement{
			level:         level,
			envelope:      env,
			incomingEdge:  incomingEdge,
			showID:        !opts.HideNodes,
			isHighlighted: opts.HighlightingTarget[env.Digest()],
		}
		elements = append(elements, elem)
		return nil, false
	}
	e.Walk(opts.HideNodes, nil, visitor)

	formatElements := func(elems []treeElement, ctx *FormatContext) string {
		var lines []string
		for _, elem := range elems {
			lines = append(lines, elem.formatString(ctx, opts.DigestDisplay))
		}
		return strings.Join(lines, "\n")
	}

	switch opts.Context.Mode {
	case FormatContextNone:
		ctx := DefaultFormatContext()
		return formatElements(elements, ctx)
	case FormatContextGlobal:
		return WithFormatContext(func(ctx *FormatContext) string {
			return formatElements(elements, ctx)
		})
	case FormatContextCustom:
		return formatElements(elements, opts.Context.Context)
	default:
		ctx := DefaultFormatContext()
		return formatElements(elements, ctx)
	}
}

// ShortID returns a text representation of the envelope's digest.
func (e *Envelope) ShortID(opt DigestDisplayFormat) string {
	d := e.Digest()
	switch opt {
	case DigestDisplayShort:
		return d.ShortDescription()
	case DigestDisplayFull:
		return d.Hex()
	case DigestDisplayUR:
		return bcur.ToURString(d)
	default:
		return d.ShortDescription()
	}
}

// treeElement represents an element in the tree representation of an envelope.
type treeElement struct {
	level         int
	envelope      *Envelope
	incomingEdge  EdgeType
	showID        bool
	isHighlighted bool
}

func (te treeElement) formatString(context *FormatContext, digestDisplay DigestDisplayFormat) string {
	var parts []string
	if te.isHighlighted {
		parts = append(parts, "*")
	}
	if te.showID {
		parts = append(parts, te.envelope.ShortID(digestDisplay))
	}
	if label := te.incomingEdge.Label(); label != "" {
		parts = append(parts, label)
	}
	parts = append(parts, te.envelope.Summary(40, context))
	line := strings.Join(parts, " ")
	indent := strings.Repeat(" ", te.level*4)
	return indent + line
}

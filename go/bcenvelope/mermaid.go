package bcenvelope

import (
	"fmt"
	"strings"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
)

// MermaidOrientation is the flow direction for Mermaid diagrams.
type MermaidOrientation int

const (
	MermaidLeftToRight MermaidOrientation = iota
	MermaidTopToBottom
	MermaidRightToLeft
	MermaidBottomToTop
)

func (o MermaidOrientation) String() string {
	switch o {
	case MermaidLeftToRight:
		return "LR"
	case MermaidTopToBottom:
		return "TB"
	case MermaidRightToLeft:
		return "RL"
	case MermaidBottomToTop:
		return "BT"
	default:
		return "LR"
	}
}

// MermaidTheme is the color theme for Mermaid diagrams.
type MermaidTheme int

const (
	MermaidThemeDefault MermaidTheme = iota
	MermaidThemeNeutral
	MermaidThemeDark
	MermaidThemeForest
	MermaidThemeBase
)

func (t MermaidTheme) String() string {
	switch t {
	case MermaidThemeDefault:
		return "default"
	case MermaidThemeNeutral:
		return "neutral"
	case MermaidThemeDark:
		return "dark"
	case MermaidThemeForest:
		return "forest"
	case MermaidThemeBase:
		return "base"
	default:
		return "default"
	}
}

// MermaidFormatOpts configures Mermaid diagram formatting.
type MermaidFormatOpts struct {
	HideNodes          bool
	Monochrome         bool
	Theme              MermaidTheme
	Orientation        MermaidOrientation
	HighlightingTarget map[bccomponents.Digest]bool
	Context            FormatContextOpt
}

// DefaultMermaidFormatOpts returns default Mermaid format options.
func DefaultMermaidFormatOpts() MermaidFormatOpts {
	return MermaidFormatOpts{
		HighlightingTarget: make(map[bccomponents.Digest]bool),
		Context:            FormatOptGlobal(),
	}
}

// MermaidFormat returns a Mermaid diagram string with default options.
func (e *Envelope) MermaidFormat() string {
	return e.MermaidFormatOpt(DefaultMermaidFormatOpts())
}

// MermaidFormatOpt returns a Mermaid diagram string with the specified options.
func (e *Envelope) MermaidFormatOpt(opts MermaidFormatOpts) string {
	var elements []*mermaidElement
	nextID := 0
	visitor := func(env *Envelope, level int, incomingEdge EdgeType, parent any) (any, bool) {
		id := nextID
		nextID++
		var parentElem *mermaidElement
		if parent != nil {
			parentElem = parent.(*mermaidElement)
		}
		elem := &mermaidElement{
			id:            id,
			level:         level,
			envelope:      env,
			incomingEdge:  incomingEdge,
			showID:        !opts.HideNodes,
			isHighlighted: opts.HighlightingTarget[env.Digest()],
			parent:        parentElem,
		}
		elements = append(elements, elem)
		return elem, false
	}
	e.Walk(opts.HideNodes, nil, visitor)

	elementIDs := make(map[int]bool)
	for _, elem := range elements {
		elementIDs[elem.id] = true
	}

	lines := []string{
		fmt.Sprintf("%%%%{ init: { 'theme': '%s', 'flowchart': { 'curve': 'basis' } } }%%%%", opts.Theme),
		fmt.Sprintf("graph %s", opts.Orientation),
	}

	var nodeStyles []string
	var linkStyles []string
	linkIndex := 0

	for _, elem := range elements {
		indent := strings.Repeat("    ", elem.level)
		var content string
		if elem.parent != nil {
			var thisLinkStyles []string
			if !opts.Monochrome {
				if color := elem.incomingEdge.LinkStrokeColor(); color != "" {
					thisLinkStyles = append(thisLinkStyles, "stroke:"+color)
				}
			}
			if elem.isHighlighted && elem.parent.isHighlighted {
				thisLinkStyles = append(thisLinkStyles, "stroke-width:4px")
			} else {
				thisLinkStyles = append(thisLinkStyles, "stroke-width:2px")
			}
			if len(thisLinkStyles) > 0 {
				linkStyles = append(linkStyles, fmt.Sprintf("linkStyle %d %s", linkIndex, strings.Join(thisLinkStyles, ",")))
			}
			linkIndex++
			content = elem.formatEdge(elementIDs)
		} else {
			content = elem.formatNode(elementIDs)
		}

		var thisNodeStyles []string
		if !opts.Monochrome {
			strokeColor := elem.envelope.nodeColor()
			thisNodeStyles = append(thisNodeStyles, "stroke:"+strokeColor)
		}
		if elem.isHighlighted {
			thisNodeStyles = append(thisNodeStyles, "stroke-width:6px")
		} else {
			thisNodeStyles = append(thisNodeStyles, "stroke-width:4px")
		}
		if len(thisNodeStyles) > 0 {
			nodeStyles = append(nodeStyles, fmt.Sprintf("style %d %s", elem.id, strings.Join(thisNodeStyles, ",")))
		}
		lines = append(lines, indent+content)
	}

	lines = append(lines, nodeStyles...)
	lines = append(lines, linkStyles...)

	return strings.Join(lines, "\n")
}

type mermaidElement struct {
	id            int
	level         int
	envelope      *Envelope
	incomingEdge  EdgeType
	showID        bool
	isHighlighted bool
	parent        *mermaidElement
}

func (me *mermaidElement) formatNode(elementIDs map[int]bool) string {
	if elementIDs[me.id] {
		delete(elementIDs, me.id)
		var lines []string
		summary := WithFormatContext(func(ctx *FormatContext) string {
			return strings.ReplaceAll(me.envelope.Summary(20, ctx), "\"", "&quot;")
		})
		lines = append(lines, summary)
		if me.showID {
			id := me.envelope.Digest().ShortDescription()
			lines = append(lines, id)
		}
		content := strings.Join(lines, "<br>")
		frameL, frameR := me.envelope.mermaidFrame()
		return fmt.Sprintf("%d%s\"%s\"%s", me.id, frameL, content, frameR)
	}
	return fmt.Sprintf("%d", me.id)
}

func (me *mermaidElement) formatEdge(elementIDs map[int]bool) string {
	parent := me.parent
	var arrow string
	if label := me.incomingEdge.Label(); label != "" {
		arrow = fmt.Sprintf("-- %s -->", label)
	} else {
		arrow = "-->"
	}
	return fmt.Sprintf("%s %s %s", parent.formatNode(elementIDs), arrow, me.formatNode(elementIDs))
}

func (e *Envelope) mermaidFrame() (string, string) {
	switch e.Case().(type) {
	case *NodeCase:
		return "((", "))"
	case *LeafCase:
		return "[", "]"
	case *WrappedCase:
		return "[/", "\\]"
	case *AssertionCase:
		return "([", "])"
	case *ElidedCase:
		return "{{", "}}"
	case *KnownValueCase:
		return "[/", "/]"
	case *EncryptedCase:
		return ">", "]"
	case *CompressedCase:
		return "[[", "]]"
	default:
		return "[", "]"
	}
}

func (e *Envelope) nodeColor() string {
	switch e.Case().(type) {
	case *NodeCase:
		return "red"
	case *LeafCase:
		return "teal"
	case *WrappedCase:
		return "blue"
	case *AssertionCase:
		return "green"
	case *ElidedCase:
		return "gray"
	case *KnownValueCase:
		return "goldenrod"
	case *EncryptedCase:
		return "coral"
	case *CompressedCase:
		return "purple"
	default:
		return "gray"
	}
}

// LinkStrokeColor returns the stroke color for this edge type in Mermaid diagrams.
func (et EdgeType) LinkStrokeColor() string {
	switch et {
	case EdgeSubject:
		return "red"
	case EdgeContent:
		return "blue"
	case EdgePredicate:
		return "cyan"
	case EdgeObject:
		return "magenta"
	default:
		return ""
	}
}

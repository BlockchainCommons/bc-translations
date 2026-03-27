package bcenvelope

import (
	"fmt"
	"math"
	"slices"
	"strings"

	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// envelopeFormatOpts controls envelope notation formatting.
type envelopeFormatOpts struct {
	flat    bool
	context FormatContextOpt
}

func defaultEnvelopeFormatOpts() envelopeFormatOpts {
	return envelopeFormatOpts{context: FormatOptGlobal()}
}

// envelopeFormatItem represents a formatted element in envelope notation.
// This is an internal type used by the notation formatting system.
type envelopeFormatItem struct {
	kind  formatItemKind
	str   string
	items []envelopeFormatItem
}

type formatItemKind int

const (
	formatItemBegin formatItemKind = iota
	formatItemEnd
	formatItemItem
	formatItemSeparator
	formatItemList
)

func formatBegin(s string) envelopeFormatItem {
	return envelopeFormatItem{kind: formatItemBegin, str: s}
}

func formatEnd(s string) envelopeFormatItem {
	return envelopeFormatItem{kind: formatItemEnd, str: s}
}

func formatItem(s string) envelopeFormatItem {
	return envelopeFormatItem{kind: formatItemItem, str: s}
}

func formatSeparator() envelopeFormatItem {
	return envelopeFormatItem{kind: formatItemSeparator}
}

func formatList(items []envelopeFormatItem) envelopeFormatItem {
	return envelopeFormatItem{kind: formatItemList, items: items}
}

func (item envelopeFormatItem) flatten() []envelopeFormatItem {
	if item.kind == formatItemList {
		var result []envelopeFormatItem
		for _, child := range item.items {
			result = append(result, child.flatten()...)
		}
		return result
	}
	return []envelopeFormatItem{item}
}

func nicenItems(items []envelopeFormatItem) []envelopeFormatItem {
	input := make([]envelopeFormatItem, len(items))
	copy(input, items)
	var result []envelopeFormatItem

	for len(input) > 0 {
		current := input[0]
		input = input[1:]
		if len(input) == 0 {
			result = append(result, current)
			break
		}
		if current.kind == formatItemEnd {
			if input[0].kind == formatItemBegin {
				result = append(result, formatEnd(current.str+" "+input[0].str))
				result = append(result, formatBegin(""))
				input = input[1:]
			} else {
				result = append(result, current)
			}
		} else {
			result = append(result, current)
		}
	}

	return result
}

func formatIndent(level int) string {
	return strings.Repeat(" ", level*4)
}

func addSpaceAtEndIfNeeded(s string) string {
	if s == "" {
		return " "
	}
	if strings.HasSuffix(s, " ") {
		return s
	}
	return s + " "
}

func (item envelopeFormatItem) format(opts envelopeFormatOpts) string {
	if opts.flat {
		return item.formatFlat()
	}
	return item.formatHierarchical()
}

func (item envelopeFormatItem) formatFlat() string {
	var line string
	items := item.flatten()
	for _, it := range items {
		switch it.kind {
		case formatItemBegin:
			if !strings.HasSuffix(line, " ") {
				line += " "
			}
			line += it.str
			line += " "
		case formatItemEnd:
			if !strings.HasSuffix(line, " ") {
				line += " "
			}
			line += it.str
			line += " "
		case formatItemItem:
			line += it.str
		case formatItemSeparator:
			line = strings.TrimRight(line, " ") + ", "
		case formatItemList:
			for _, child := range it.items {
				line += child.formatFlat()
			}
		}
	}
	return line
}

func (item envelopeFormatItem) formatHierarchical() string {
	var lines []string
	level := 0
	currentLine := ""
	items := nicenItems(item.flatten())
	for _, it := range items {
		switch it.kind {
		case formatItemBegin:
			if it.str != "" {
				c := it.str
				if currentLine != "" {
					c = addSpaceAtEndIfNeeded(currentLine) + it.str
				}
				lines = append(lines, formatIndent(level)+c+"\n")
			}
			level++
			currentLine = ""
		case formatItemEnd:
			if currentLine != "" {
				lines = append(lines, formatIndent(level)+currentLine+"\n")
				currentLine = ""
			}
			level--
			lines = append(lines, formatIndent(level)+it.str+"\n")
		case formatItemItem:
			currentLine += it.str
		case formatItemSeparator:
			if currentLine != "" {
				lines = append(lines, formatIndent(level)+currentLine+"\n")
				currentLine = ""
			}
		case formatItemList:
			lines = append(lines, "<list>")
		}
	}
	if currentLine != "" {
		lines = append(lines, currentLine)
	}
	return strings.Join(lines, "")
}

// formatItemIndex returns a sort index for the item kind.
func (item envelopeFormatItem) formatItemIndex() int {
	switch item.kind {
	case formatItemBegin:
		return 1
	case formatItemEnd:
		return 2
	case formatItemItem:
		return 3
	case formatItemSeparator:
		return 4
	case formatItemList:
		return 5
	default:
		return 0
	}
}

// compareFormatItems compares two format items for sorting.
func compareFormatItems(a, b envelopeFormatItem) int {
	ai := a.formatItemIndex()
	bi := b.formatItemIndex()
	if ai < bi {
		return -1
	}
	if ai > bi {
		return 1
	}
	if a.str < b.str {
		return -1
	}
	if a.str > b.str {
		return 1
	}
	// For list items, compare child items recursively
	if a.kind == formatItemList && b.kind == formatItemList {
		minLen := len(a.items)
		if len(b.items) < minLen {
			minLen = len(b.items)
		}
		for i := 0; i < minLen; i++ {
			cmp := compareFormatItems(a.items[i], b.items[i])
			if cmp != 0 {
				return cmp
			}
		}
		if len(a.items) < len(b.items) {
			return -1
		}
		if len(a.items) > len(b.items) {
			return 1
		}
	}
	return 0
}

// Envelope formatting methods.

// Format returns the envelope notation for this envelope using the global format context.
func (e *Envelope) Format() string {
	return e.FormatOpt(defaultEnvelopeFormatOpts())
}

// FormatFlat returns the envelope notation in flat (single-line) format.
func (e *Envelope) FormatFlat() string {
	opts := defaultEnvelopeFormatOpts()
	opts.flat = true
	return e.FormatOpt(opts)
}

// FormatOpt returns the envelope notation with the given options.
func (e *Envelope) FormatOpt(opts envelopeFormatOpts) string {
	item := e.formatItem(opts)
	return strings.TrimSpace(item.format(opts))
}

// formatItem returns an envelopeFormatItem for this envelope's notation.
func (e *Envelope) formatItem(opts envelopeFormatOpts) envelopeFormatItem {
	switch c := e.Case().(type) {
	case *LeafCase:
		return formatCBOR(*c.CBOR, opts)
	case *WrappedCase:
		return formatList([]envelopeFormatItem{
			formatBegin("{"),
			c.Envelope.formatItem(opts),
			formatEnd("}"),
		})
	case *AssertionCase:
		return c.Assertion.formatItem(opts)
	case *KnownValueCase:
		return formatKnownValue(c.Value, opts)
	case *EncryptedCase:
		return formatItem("ENCRYPTED")
	case *CompressedCase:
		return formatItem("COMPRESSED")
	case *NodeCase:
		return formatNode(c, opts)
	case *ElidedCase:
		return formatItem("ELIDED")
	default:
		return formatItem("<error>")
	}
}

func formatNode(nc *NodeCase, opts envelopeFormatOpts) envelopeFormatItem {
	var items []envelopeFormatItem

	subjectItem := nc.Subject.formatItem(opts)
	var elidedCount int
	var encryptedCount int
	var compressedCount int
	var typeAssertionItems [][]envelopeFormatItem
	var assertionItems [][]envelopeFormatItem

	for _, assertion := range nc.Assertions {
		switch assertion.Case().(type) {
		case *ElidedCase:
			elidedCount++
		case *EncryptedCase:
			encryptedCount++
		case *CompressedCase:
			compressedCount++
		default:
			item := []envelopeFormatItem{assertion.formatItem(opts)}
			// Check if this is a type assertion (isA predicate)
			isTypeAssertion := false
			if pred := assertion.AsPredicate(); pred != nil {
				if kv := pred.Subject().AsKnownValue(); kv != nil {
					if kv.Equal(knownvalues.IsA) {
						isTypeAssertion = true
					}
				}
			}
			if isTypeAssertion {
				typeAssertionItems = append(typeAssertionItems, item)
			} else {
				assertionItems = append(assertionItems, item)
			}
		}
	}

	// Sort type assertion items
	sortFormatItemGroups(typeAssertionItems)
	// Sort assertion items
	sortFormatItemGroups(assertionItems)
	// Prepend type assertions
	assertionItems = append(typeAssertionItems, assertionItems...)

	// Add compressed markers
	if compressedCount > 1 {
		assertionItems = append(assertionItems, []envelopeFormatItem{
			formatItem(fmt.Sprintf("COMPRESSED (%d)", compressedCount)),
		})
	} else if compressedCount > 0 {
		assertionItems = append(assertionItems, []envelopeFormatItem{
			formatItem("COMPRESSED"),
		})
	}

	// Add elided markers
	if elidedCount > 1 {
		assertionItems = append(assertionItems, []envelopeFormatItem{
			formatItem(fmt.Sprintf("ELIDED (%d)", elidedCount)),
		})
	} else if elidedCount > 0 {
		assertionItems = append(assertionItems, []envelopeFormatItem{
			formatItem("ELIDED"),
		})
	}

	// Add encrypted markers
	if encryptedCount > 1 {
		assertionItems = append(assertionItems, []envelopeFormatItem{
			formatItem(fmt.Sprintf("ENCRYPTED (%d)", encryptedCount)),
		})
	} else if encryptedCount > 0 {
		assertionItems = append(assertionItems, []envelopeFormatItem{
			formatItem("ENCRYPTED"),
		})
	}

	// Intersperse separators
	var joinedItems []envelopeFormatItem
	for i, group := range assertionItems {
		if i > 0 {
			joinedItems = append(joinedItems, formatSeparator())
		}
		joinedItems = append(joinedItems, group...)
	}

	needsBraces := nc.Subject.IsSubjectAssertion()

	if needsBraces {
		items = append(items, formatBegin("{"))
	}
	items = append(items, subjectItem)
	if needsBraces {
		items = append(items, formatEnd("}"))
	}
	items = append(items, formatBegin("["))
	items = append(items, joinedItems...)
	items = append(items, formatEnd("]"))
	return formatList(items)
}

func sortFormatItemGroups(groups [][]envelopeFormatItem) {
	slices.SortStableFunc(groups, func(a, b []envelopeFormatItem) int {
		minLen := len(a)
		if len(b) < minLen {
			minLen = len(b)
		}
		for k := 0; k < minLen; k++ {
			cmp := compareFormatItems(a[k], b[k])
			if cmp != 0 {
				return cmp
			}
		}
		if len(a) < len(b) {
			return -1
		}
		if len(a) > len(b) {
			return 1
		}
		return 0
	})
}

// formatCBOR formats a CBOR value for envelope notation.
func formatCBOR(cbor dcbor.CBOR, opts envelopeFormatOpts) envelopeFormatItem {
	// Check if this is a tagged envelope
	if tag, inner, ok := cbor.AsTaggedValue(); ok {
		envelopeTags := EnvelopeCBORTags()
		if len(envelopeTags) > 0 && tag.Value() == envelopeTags[0].Value() {
			env, err := EnvelopeFromUntaggedCBOR(inner)
			if err == nil {
				return env.formatItem(opts)
			}
			return formatItem("<error>")
		}
	}
	summary, err := envelopeSummaryCBOR(cbor, math.MaxInt, opts.context)
	if err != nil {
		return formatItem("<error>")
	}
	return formatItem(summary)
}

// formatKnownValue formats a KnownValue for envelope notation.
func formatKnownValue(kv knownvalues.KnownValue, opts envelopeFormatOpts) envelopeFormatItem {
	var name string
	switch opts.context.Mode {
	case FormatContextNone:
		name = kv.Name()
	case FormatContextGlobal:
		name = WithFormatContext(func(ctx *FormatContext) string {
			if assigned, ok := ctx.KnownValues().AssignedName(kv); ok {
				return assigned
			}
			return kv.Name()
		})
	case FormatContextCustom:
		if opts.context.Context != nil {
			if assigned, ok := opts.context.Context.KnownValues().AssignedName(kv); ok {
				name = assigned
			} else {
				name = kv.Name()
			}
		} else {
			name = kv.Name()
		}
	}
	return formatItem("'" + name + "'")
}

// Assertion formatting.
func (a *Assertion) formatItem(opts envelopeFormatOpts) envelopeFormatItem {
	return formatList([]envelopeFormatItem{
		a.Predicate().formatItem(opts),
		formatItem(": "),
		a.Object().formatItem(opts),
	})
}

// Envelope String returns a debug description.
func (e *Envelope) String() string {
	opts := defaultEnvelopeFormatOpts()
	switch c := e.Case().(type) {
	case *NodeCase:
		var assertionStrs []string
		for _, a := range c.Assertions {
			assertionStrs = append(assertionStrs, a.String())
		}
		assertions := "[" + strings.Join(assertionStrs, ", ") + "]"
		return fmt.Sprintf(".node(%s, %s)", c.Subject, assertions)
	case *LeafCase:
		return fmt.Sprintf(".cbor(%s)", formatCBOR(*c.CBOR, opts).str)
	case *WrappedCase:
		return fmt.Sprintf(".wrapped(%s)", c.Envelope)
	case *AssertionCase:
		return fmt.Sprintf(".assertion(%s, %s)", c.Assertion.Predicate(), c.Assertion.Object())
	case *ElidedCase:
		return ".elided"
	case *KnownValueCase:
		return fmt.Sprintf(".knownValue(%s)", c.Value)
	case *EncryptedCase:
		return ".encrypted"
	case *CompressedCase:
		return ".compressed"
	default:
		return "<unknown>"
	}
}

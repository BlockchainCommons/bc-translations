package dcbor

import (
	"encoding/hex"
	"fmt"
	"strings"
	"unicode/utf8"
)

// DiagFormatOpts controls diagnostic formatting behavior.
type DiagFormatOpts struct {
	annotate  bool
	summarize bool
	flat      bool
	tags      TagsStoreOpt
	tagsSet   bool
}

// Annotate toggles tag-name annotations in diagnostic output.
func (o DiagFormatOpts) Annotate(v bool) DiagFormatOpts {
	o.annotate = v
	return o
}

// Summarize toggles tag summarization in diagnostic output.
func (o DiagFormatOpts) Summarize(v bool) DiagFormatOpts {
	o.summarize = v
	if v {
		o.flat = true
	}
	return o
}

// Flat toggles single-line formatting for nested values.
func (o DiagFormatOpts) Flat(v bool) DiagFormatOpts {
	o.flat = v
	return o
}

// Tags sets the tag context used for name lookup and summarizers.
func (o DiagFormatOpts) Tags(v TagsStoreOpt) DiagFormatOpts {
	o.tags = v
	o.tagsSet = true
	return o
}

// DiagnosticOpt formats CBOR using the supplied diagnostic options.
func (c CBOR) DiagnosticOpt(opts DiagFormatOpts) string {
	opts = normalizeDiagOpts(opts)
	return c.diagnosticAtLevel(0, opts)
}

func normalizeDiagOpts(opts DiagFormatOpts) DiagFormatOpts {
	if !opts.tagsSet {
		opts.tags = TagsGlobal()
	}
	return opts
}

func (c CBOR) diagnosticAtLevel(level int, opts DiagFormatOpts) string {
	if opts.summarize && c.kind == CBORKindTagged {
		tagged := c.value.(TaggedValue)
		if summarizer, ok := lookupSummarizer(opts.tags, tagged.Tag.Value()); ok {
			if summary, err := summarizer(tagged.Value.Clone(), opts.flat); err == nil {
				return summary
			}
		}
	}

	switch c.kind {
	case CBORKindUnsigned:
		return fmt.Sprintf("%d", c.value.(uint64))
	case CBORKindNegative:
		return formatNegativeDisplay(c.value.(uint64))
	case CBORKindByteString:
		return fmt.Sprintf("h'%x'", c.value.(ByteString).Data())
	case CBORKindText:
		return fmt.Sprintf("%q", c.value.(string))
	case CBORKindSimple:
		return c.value.(Simple).Name()
	case CBORKindTagged:
		tagged := c.value.(TaggedValue)
		tagPrefix := fmt.Sprintf("%d", tagged.Tag.Value())
		if opts.annotate {
			if name, ok := lookupTagName(opts.tags, tagged.Tag); ok {
				return fmt.Sprintf("%d(%s)   / %s /", tagged.Tag.Value(), tagged.Value.diagnosticAtLevel(level+1, opts), name)
			}
		}
		return fmt.Sprintf("%s(%s)", tagPrefix, tagged.Value.diagnosticAtLevel(level+1, opts))
	case CBORKindArray:
		items := c.value.([]CBOR)
		if len(items) == 0 {
			return "[]"
		}
		if opts.flat || !containsNestedCollection(items) {
			parts := make([]string, 0, len(items))
			for _, item := range items {
				parts = append(parts, item.diagnosticAtLevel(level+1, opts))
			}
			return fmt.Sprintf("[%s]", strings.Join(parts, ", "))
		}
		indent := strings.Repeat(" ", (level+1)*4)
		parts := make([]string, 0, len(items))
		for _, item := range items {
			parts = append(parts, indent+item.diagnosticAtLevel(level+1, opts))
		}
		return fmt.Sprintf("[\n%s\n%s]", strings.Join(parts, ",\n"), strings.Repeat(" ", level*4))
	case CBORKindMap:
		m := c.value.(Map)
		entries := m.AsEntries()
		if len(entries) == 0 {
			return "{}"
		}
		if opts.flat || !mapContainsNestedCollection(entries) {
			parts := make([]string, 0, len(entries))
			for _, entry := range entries {
				parts = append(parts, fmt.Sprintf("%s: %s", entry.Key.diagnosticAtLevel(level+1, opts), entry.Value.diagnosticAtLevel(level+1, opts)))
			}
			return fmt.Sprintf("{%s}", strings.Join(parts, ", "))
		}
		indent := strings.Repeat(" ", (level+1)*4)
		parts := make([]string, 0, len(entries))
		for _, entry := range entries {
			parts = append(parts, fmt.Sprintf("%s%s: %s", indent, entry.Key.diagnosticAtLevel(level+1, opts), entry.Value.diagnosticAtLevel(level+1, opts)))
		}
		return fmt.Sprintf("{\n%s\n%s}", strings.Join(parts, ",\n"), strings.Repeat(" ", level*4))
	default:
		return "<unknown>"
	}
}

func containsNestedCollection(items []CBOR) bool {
	for _, item := range items {
		if isNestedDiagnosticItem(item) {
			return true
		}
	}
	return false
}

func mapContainsNestedCollection(entries []MapEntry) bool {
	for _, entry := range entries {
		if isNestedDiagnosticItem(entry.Key) || isNestedDiagnosticItem(entry.Value) {
			return true
		}
	}
	return false
}

func isNestedDiagnosticItem(cbor CBOR) bool {
	return cbor.kind == CBORKindArray || cbor.kind == CBORKindMap || cbor.kind == CBORKindTagged
}

func lookupSummarizer(opt TagsStoreOpt, value TagValue) (CBORSummarizer, bool) {
	switch opt.Mode {
	case TagsStoreModeCustom:
		if opt.Store == nil {
			return nil, false
		}
		return opt.Store.Summarizer(value)
	case TagsStoreModeGlobal:
		store := GlobalTags.Get()
		return store.Summarizer(value)
	default:
		return nil, false
	}
}

func lookupTagName(opt TagsStoreOpt, tag Tag) (string, bool) {
	switch opt.Mode {
	case TagsStoreModeCustom:
		if opt.Store == nil {
			return "", false
		}
		return opt.Store.AssignedNameForTag(tag)
	case TagsStoreModeGlobal:
		store := GlobalTags.Get()
		return store.AssignedNameForTag(tag)
	default:
		return "", false
	}
}

// HexFormatOpts controls annotated hex formatting.
type HexFormatOpts struct {
	annotate bool
	tags     TagsStoreOpt
	tagsSet  bool
}

// Annotate toggles structured annotated-hex output.
func (o HexFormatOpts) Annotate(v bool) HexFormatOpts {
	o.annotate = v
	return o
}

// Context sets the tag context used for annotated comments.
func (o HexFormatOpts) Context(tags TagsStoreOpt) HexFormatOpts {
	o.tags = tags
	o.tagsSet = true
	return o
}

func normalizeHexOpts(opts HexFormatOpts) HexFormatOpts {
	if !opts.tagsSet {
		opts.tags = TagsGlobal()
	}
	return opts
}

// HexOpt formats CBOR as hex with optional annotation comments.
func (c CBOR) HexOpt(opts HexFormatOpts) string {
	opts = normalizeHexOpts(opts)
	hexOnly := c.Hex()
	if !opts.annotate {
		return hexOnly
	}
	lines := c.annotatedHexLines(0, opts)
	return renderAnnotatedHex(lines)
}

type annotatedHexLine struct {
	indent  int
	hexText string
	comment string
}

func (c CBOR) annotatedHexLines(level int, opts HexFormatOpts) []annotatedHexLine {
	switch c.kind {
	case CBORKindUnsigned:
		return []annotatedHexLine{{
			indent:  level,
			hexText: hex.EncodeToString(encodeHead(majorUnsigned, c.value.(uint64))),
			comment: fmt.Sprintf("unsigned(%d)", c.value.(uint64)),
		}}
	case CBORKindNegative:
		encodedMagnitude := c.value.(uint64)
		return []annotatedHexLine{{
			indent:  level,
			hexText: hex.EncodeToString(encodeHead(majorNegative, encodedMagnitude)),
			comment: fmt.Sprintf("negative(%s)", formatNegativeDisplay(encodedMagnitude)),
		}}
	case CBORKindByteString:
		payload := c.value.(ByteString).Data()
		lines := []annotatedHexLine{{
			indent:  level,
			hexText: hex.EncodeToString(encodeHead(majorBytes, uint64(len(payload)))),
			comment: fmt.Sprintf("bytes(%d)", len(payload)),
		}}
		if len(payload) > 0 {
			payloadComment := ""
			if utf8.Valid(payload) {
				payloadComment = fmt.Sprintf("%q", string(payload))
			}
			lines = append(lines, annotatedHexLine{
				indent:  level + 1,
				hexText: hex.EncodeToString(payload),
				comment: payloadComment,
			})
		}
		return lines
	case CBORKindText:
		text := c.value.(string)
		payload := []byte(text)
		headerData := encodeHead(majorText, uint64(len(payload)))
		lines := []annotatedHexLine{{
			indent:  level,
			hexText: splitHeaderHex(headerData),
			comment: fmt.Sprintf("text(%d)", len(payload)),
		}}
		if len(payload) > 0 {
			lines = append(lines, annotatedHexLine{
				indent:  level + 1,
				hexText: hex.EncodeToString(payload),
				comment: fmt.Sprintf("%q", text),
			})
		}
		return lines
	case CBORKindArray:
		items := c.value.([]CBOR)
		headerData := encodeHead(majorArray, uint64(len(items)))
		lines := []annotatedHexLine{{
			indent:  level,
			hexText: splitHeaderHex(headerData),
			comment: fmt.Sprintf("array(%d)", len(items)),
		}}
		for _, item := range items {
			lines = append(lines, item.annotatedHexLines(level+1, opts)...)
		}
		return lines
	case CBORKindMap:
		m := c.value.(Map)
		entries := m.AsEntries()
		headerData := encodeHead(majorMap, uint64(len(entries)))
		lines := []annotatedHexLine{{
			indent:  level,
			hexText: splitHeaderHex(headerData),
			comment: fmt.Sprintf("map(%d)", len(entries)),
		}}
		for _, entry := range entries {
			lines = append(lines, entry.Key.annotatedHexLines(level+1, opts)...)
			lines = append(lines, entry.Value.annotatedHexLines(level+1, opts)...)
		}
		return lines
	case CBORKindTagged:
		tagged := c.value.(TaggedValue)
		tagComment := fmt.Sprintf("tag(%d)", tagged.Tag.Value())
		if name, ok := lookupTagName(opts.tags, tagged.Tag); ok {
			tagComment = fmt.Sprintf("tag(%d) %s", tagged.Tag.Value(), name)
		}
		headerData := encodeHead(majorTagged, tagged.Tag.Value())
		lines := []annotatedHexLine{{
			indent:  level,
			hexText: splitHeaderHex(headerData),
			comment: tagComment,
		}}
		lines = append(lines, tagged.Value.annotatedHexLines(level+1, opts)...)
		return lines
	case CBORKindSimple:
		s := c.value.(Simple)
		simpleData, err := encodeSimple(s)
		if err != nil {
			return []annotatedHexLine{{
				indent:  level,
				hexText: "<invalid>",
				comment: err.Error(),
			}}
		}
		return []annotatedHexLine{{
			indent:  level,
			hexText: hex.EncodeToString(simpleData),
			comment: simpleHexComment(s),
		}}
	default:
		return []annotatedHexLine{{
			indent:  level,
			hexText: "<unknown>",
			comment: "unsupported CBOR kind",
		}}
	}
}

func simpleHexComment(simple Simple) string {
	switch simple.Kind() {
	case SimpleFalse:
		return "false"
	case SimpleTrue:
		return "true"
	case SimpleNull:
		return "null"
	default:
		value, _ := simple.Float64()
		return formatFloatDiagnostic(value)
	}
}

func splitHeaderHex(data []byte) string {
	if len(data) == 0 {
		return ""
	}
	if len(data) == 1 {
		return fmt.Sprintf("%02x", data[0])
	}
	return fmt.Sprintf("%02x %s", data[0], hex.EncodeToString(data[1:]))
}

func renderAnnotatedHex(lines []annotatedHexLine) string {
	if len(lines) == 0 {
		return ""
	}

	noteColumn := 0
	for _, line := range lines {
		column1Len := len(strings.Repeat("    ", line.indent)) + len(line.hexText)
		if column1Len > noteColumn {
			noteColumn = column1Len
		}
	}
	// Match Rust layout behavior: round up to nearest multiple of 4, then minus 1.
	noteColumn = ((noteColumn + 4) &^ 3) - 1

	var out strings.Builder
	for i, line := range lines {
		if i > 0 {
			out.WriteByte('\n')
		}
		column1 := strings.Repeat("    ", line.indent) + line.hexText
		out.WriteString(column1)
		if line.comment != "" {
			targetColumn := noteColumn
			if targetColumn > 39 {
				targetColumn = 39
			}
			padding := targetColumn - len(column1) + 1
			if padding < 1 {
				padding = 1
			}
			out.WriteString(strings.Repeat(" ", padding))
			out.WriteString("# ")
			out.WriteString(line.comment)
		}
	}

	return out.String()
}

package dcbor

import (
	"fmt"
	"strings"
)

// DiagFormatOpts controls diagnostic formatting behavior.
type DiagFormatOpts struct {
	annotate  bool
	summarize bool
	flat      bool
	tags      TagsStoreOpt
	tagsSet   bool
}

func (o DiagFormatOpts) Annotate(v bool) DiagFormatOpts {
	o.annotate = v
	return o
}

func (o DiagFormatOpts) Summarize(v bool) DiagFormatOpts {
	o.summarize = v
	if v {
		o.flat = true
	}
	return o
}

func (o DiagFormatOpts) Flat(v bool) DiagFormatOpts {
	o.flat = v
	return o
}

func (o DiagFormatOpts) Tags(v TagsStoreOpt) DiagFormatOpts {
	o.tags = v
	o.tagsSet = true
	return o
}

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
		return fmt.Sprintf("h'%x'", c.value.(ByteString).AsRef())
	case CBORKindText:
		return fmt.Sprintf("%q", c.value.(string))
	case CBORKindSimple:
		return c.value.(Simple).Name()
	case CBORKindTagged:
		tagged := c.value.(TaggedValue)
		tagPrefix := fmt.Sprintf("%d", tagged.Tag.Value())
		if opts.annotate {
			if name, ok := lookupTagName(opts.tags, tagged.Tag); ok {
				tagPrefix = fmt.Sprintf("%d(%s)", tagged.Tag.Value(), name)
				return fmt.Sprintf("%s(%s)", tagPrefix, tagged.Value.diagnosticAtLevel(level+1, opts))
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
		if item.kind == CBORKindArray || item.kind == CBORKindMap {
			return true
		}
	}
	return false
}

func mapContainsNestedCollection(entries []MapEntry) bool {
	for _, entry := range entries {
		if entry.Value.kind == CBORKindArray || entry.Value.kind == CBORKindMap {
			return true
		}
	}
	return false
}

func lookupSummarizer(opt TagsStoreOpt, value TagValue) (CBORSummarizer, bool) {
	switch opt.Mode {
	case TagsStoreModeCustom:
		if opt.Store == nil {
			return nil, false
		}
		return opt.Store.Summarizer(value)
	case TagsStoreModeGlobal:
		store := GLOBAL_TAGS.Get()
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
		store := GLOBAL_TAGS.Get()
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

func (o HexFormatOpts) Annotate(v bool) HexFormatOpts {
	o.annotate = v
	return o
}

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

func (c CBOR) HexOpt(opts HexFormatOpts) string {
	opts = normalizeHexOpts(opts)
	hexOnly := c.Hex()
	if !opts.annotate {
		return hexOnly
	}
	return fmt.Sprintf("%s  # %s", hexOnly, c.DiagnosticFlat())
}

package bcenvelope

import (
	"sync"

	bccomponents "github.com/nickel-blockchaincommons/bccomponents-go"
	bctags "github.com/nickel-blockchaincommons/bctags-go"
	dcbor "github.com/nickel-blockchaincommons/dcbor-go"
	knownvalues "github.com/nickel-blockchaincommons/knownvalues-go"
)

// FormatContextMode determines which format context to use.
type FormatContextMode int

const (
	// FormatContextNone uses no format context.
	FormatContextNone FormatContextMode = iota
	// FormatContextGlobal uses the process-global format context.
	FormatContextGlobal
	// FormatContextCustom uses a caller-provided format context.
	FormatContextCustom
)

// FormatContextOpt configures which format context source to use when formatting.
type FormatContextOpt struct {
	Mode    FormatContextMode
	Context *FormatContext
}

// FormatOptNone selects no format context.
func FormatOptNone() FormatContextOpt {
	return FormatContextOpt{Mode: FormatContextNone}
}

// FormatOptGlobal selects the process-global format context.
func FormatOptGlobal() FormatContextOpt {
	return FormatContextOpt{Mode: FormatContextGlobal}
}

// FormatOptCustom selects a caller-provided format context.
func FormatOptCustom(ctx *FormatContext) FormatContextOpt {
	return FormatContextOpt{Mode: FormatContextCustom, Context: ctx}
}

// ToTagsStoreOpt converts a FormatContextOpt to a dcbor.TagsStoreOpt.
func (o FormatContextOpt) ToTagsStoreOpt() dcbor.TagsStoreOpt {
	switch o.Mode {
	case FormatContextNone:
		return dcbor.TagsNone()
	case FormatContextGlobal:
		return dcbor.TagsGlobal()
	case FormatContextCustom:
		if o.Context != nil {
			return dcbor.TagsCustom(o.Context.Tags())
		}
		return dcbor.TagsNone()
	default:
		return dcbor.TagsNone()
	}
}

// FormatContext provides information about CBOR tags, known values, functions,
// and parameters that are used to annotate the output of envelope formatting functions.
type FormatContext struct {
	tags        *dcbor.TagsStore
	knownValues *knownvalues.KnownValuesStore
	functions   *FunctionsStore
	parameters  *ParametersStore
}

// NewFormatContext creates a new format context with the specified components.
// Any nil component is replaced with its default.
func NewFormatContext(
	tags *dcbor.TagsStore,
	kv *knownvalues.KnownValuesStore,
	functions *FunctionsStore,
	parameters *ParametersStore,
) *FormatContext {
	if tags == nil {
		tags = dcbor.NewTagsStore(nil)
	}
	if kv == nil {
		kv = knownvalues.NewKnownValuesStore()
	}
	if functions == nil {
		functions = NewFunctionsStore()
	}
	if parameters == nil {
		parameters = NewParametersStore()
	}
	return &FormatContext{
		tags:        tags,
		knownValues: kv,
		functions:   functions,
		parameters:  parameters,
	}
}

// DefaultFormatContext creates a format context with default (empty) components.
func DefaultFormatContext() *FormatContext {
	return NewFormatContext(nil, nil, nil, nil)
}

// Tags returns the CBOR tags registry.
func (c *FormatContext) Tags() *dcbor.TagsStore { return c.tags }

// KnownValues returns the known values registry.
func (c *FormatContext) KnownValues() *knownvalues.KnownValuesStore { return c.knownValues }

// Functions returns the functions registry.
func (c *FormatContext) Functions() *FunctionsStore { return c.functions }

// Parameters returns the parameters registry.
func (c *FormatContext) Parameters() *ParametersStore { return c.parameters }

// Clone returns an independent copy of the format context.
func (c *FormatContext) Clone() *FormatContext {
	if c == nil {
		return DefaultFormatContext()
	}
	// Tags store is shared (not deep cloned) since TagsStore has no Clone
	// in the Go dcbor package; callers can register additional tags.
	return &FormatContext{
		tags:        c.tags,
		knownValues: c.knownValues.Clone(),
		functions:   c.functions.Clone(),
		parameters:  c.parameters.Clone(),
	}
}

// TagsResolver interface delegation methods.

// AssignedNameForTag returns the registered name for the tag value, if present.
func (c *FormatContext) AssignedNameForTag(tag dcbor.Tag) (string, bool) {
	return c.tags.AssignedNameForTag(tag)
}

// NameForTag returns the assigned name for a tag, or numeric fallback text.
func (c *FormatContext) NameForTag(tag dcbor.Tag) string {
	return c.tags.NameForTag(tag)
}

// TagForValue looks up a tag definition by numeric value.
func (c *FormatContext) TagForValue(value dcbor.TagValue) (dcbor.Tag, bool) {
	return c.tags.TagForValue(value)
}

// TagForName looks up a tag definition by assigned name.
func (c *FormatContext) TagForName(name string) (dcbor.Tag, bool) {
	return c.tags.TagForName(name)
}

// NameForValue returns the assigned name for a value, or numeric fallback text.
func (c *FormatContext) NameForValue(value dcbor.TagValue) string {
	return c.tags.NameForValue(value)
}

// Summarizer looks up the registered summarizer for a tag value.
func (c *FormatContext) Summarizer(tag dcbor.TagValue) (dcbor.CBORSummarizer, bool) {
	return c.tags.Summarizer(tag)
}

// Verify FormatContext implements dcbor.TagsResolver.
var _ dcbor.TagsResolver = (*FormatContext)(nil)

// Global format context singleton.
var (
	globalFormatContext     *FormatContext
	globalFormatContextOnce sync.Once
	globalFormatContextMu   sync.Mutex
)

func initGlobalFormatContext() {
	globalFormatContextOnce.Do(func() {
		bccomponents.RegisterTags()
		tagsStore := dcbor.GlobalTags.Get()
		kvStore := knownvalues.KnownValues.Get()
		functionsStore := GlobalFunctions()
		parametersStore := GlobalParameters()
		globalFormatContext = NewFormatContext(tagsStore, kvStore, functionsStore, parametersStore)
	})
}

// WithFormatContext provides read access to the global format context under lock.
func WithFormatContext[T any](action func(*FormatContext) T) T {
	initGlobalFormatContext()
	globalFormatContextMu.Lock()
	defer globalFormatContextMu.Unlock()
	return action(globalFormatContext)
}

// UpdateFormatContext provides read-write access to the global format context under lock.
func UpdateFormatContext(action func(*FormatContext)) {
	initGlobalFormatContext()
	globalFormatContextMu.Lock()
	defer globalFormatContextMu.Unlock()
	action(globalFormatContext)
}

// RegisterTagsIn registers standard tags and summarizers in a format context.
func RegisterTagsIn(context *FormatContext) {
	bccomponents.RegisterTagsIn(context.Tags())

	// Known value summarizer
	kvStore := context.KnownValues().Clone()
	context.Tags().SetSummarizer(bctags.TagKnownValue, func(untaggedCBOR dcbor.CBOR, _ bool) (string, error) {
		kv, err := knownvalues.DecodeKnownValue(untaggedCBOR)
		if err != nil {
			return "", err
		}
		return "'" + kvStore.Name(kv) + "'", nil
	})

	// Function summarizer
	fStore := context.Functions().Clone()
	context.Tags().SetSummarizer(bctags.TagFunction, func(untaggedCBOR dcbor.CBOR, _ bool) (string, error) {
		f, err := DecodeFunction(untaggedCBOR)
		if err != nil {
			return "", err
		}
		return "\u00ab" + NameForFunction(f, fStore) + "\u00bb", nil
	})

	// Parameter summarizer
	pStore := context.Parameters().Clone()
	context.Tags().SetSummarizer(bctags.TagParameter, func(untaggedCBOR dcbor.CBOR, _ bool) (string, error) {
		p, err := DecodeParameter(untaggedCBOR)
		if err != nil {
			return "", err
		}
		return "\u2770" + NameForParameter(p, pStore) + "\u2771", nil
	})

	// Request summarizer
	clonedCtxReq := context.Clone()
	context.Tags().SetSummarizer(bctags.TagRequest, func(untaggedCBOR dcbor.CBOR, flat bool) (string, error) {
		e := EnvelopeFromCBOR(untaggedCBOR)
		opts := defaultEnvelopeFormatOpts()
		opts.flat = flat
		opts.context = FormatOptCustom(clonedCtxReq)
		return "request(" + e.FormatOpt(opts) + ")", nil
	})

	// Response summarizer
	clonedCtxResp := context.Clone()
	context.Tags().SetSummarizer(bctags.TagResponse, func(untaggedCBOR dcbor.CBOR, flat bool) (string, error) {
		e := EnvelopeFromCBOR(untaggedCBOR)
		opts := defaultEnvelopeFormatOpts()
		opts.flat = flat
		opts.context = FormatOptCustom(clonedCtxResp)
		return "response(" + e.FormatOpt(opts) + ")", nil
	})

	// Event summarizer
	clonedCtxEvent := context.Clone()
	context.Tags().SetSummarizer(bctags.TagEvent, func(untaggedCBOR dcbor.CBOR, flat bool) (string, error) {
		e := EnvelopeFromCBOR(untaggedCBOR)
		opts := defaultEnvelopeFormatOpts()
		opts.flat = flat
		opts.context = FormatOptCustom(clonedCtxEvent)
		return "event(" + e.FormatOpt(opts) + ")", nil
	})
}

// RegisterTags registers standard tags in the global format context.
func RegisterTags() {
	UpdateFormatContext(func(context *FormatContext) {
		RegisterTagsIn(context)
	})
}

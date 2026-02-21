package dcbor

import (
	"fmt"
	"sync"
)

// CBORSummarizer summarizes tagged CBOR content for diagnostic/summary output.
type CBORSummarizer func(cbor CBOR, flat bool) (string, error)

// TagsResolver defines the interface for tag, name, and summarizer lookups.
type TagsResolver interface {
	AssignedNameForTag(tag Tag) (string, bool)
	NameForTag(tag Tag) string
	TagForValue(value TagValue) (Tag, bool)
	TagForName(name string) (Tag, bool)
	NameForValue(value TagValue) string
	Summarizer(tag TagValue) (CBORSummarizer, bool)
}

// TagsStoreMode determines which tag context to use.
type TagsStoreMode int

const (
	TagsStoreModeNone TagsStoreMode = iota
	TagsStoreModeGlobal
	TagsStoreModeCustom
)

// TagsStoreOpt configures which tag source to use when formatting.
type TagsStoreOpt struct {
	Mode  TagsStoreMode
	Store TagsResolver
}

// TagsNone selects no tag store context.
func TagsNone() TagsStoreOpt {
	return TagsStoreOpt{Mode: TagsStoreModeNone}
}

// TagsGlobal selects the process-global tag store context.
func TagsGlobal() TagsStoreOpt {
	return TagsStoreOpt{Mode: TagsStoreModeGlobal}
}

// TagsCustom selects a caller-provided tag store context.
func TagsCustom(store TagsResolver) TagsStoreOpt {
	return TagsStoreOpt{Mode: TagsStoreModeCustom, Store: store}
}

// TagsStore keeps tag registry state.
type TagsStore struct {
	tagsByValue map[TagValue]Tag
	tagsByName  map[string]Tag
	summarizers map[TagValue]CBORSummarizer
}

// NewTagsStore constructs a tag store initialized with the provided tags.
func NewTagsStore(tags []Tag) *TagsStore {
	ts := &TagsStore{
		tagsByValue: make(map[TagValue]Tag),
		tagsByName:  make(map[string]Tag),
		summarizers: make(map[TagValue]CBORSummarizer),
	}
	ts.InsertAll(tags)
	return ts
}

// Insert adds a tag mapping, panicking on conflicting redefinition.
func (t *TagsStore) Insert(tag Tag) {
	if existing, ok := t.tagsByValue[tag.Value()]; ok {
		en, eok := existing.Name()
		tn, tok := tag.Name()
		if eok != tok || (eok && en != tn) {
			panic(fmt.Sprintf("tag %d already has name %q", tag.Value(), en))
		}
	}
	t.tagsByValue[tag.Value()] = tag.Clone()
	if name, ok := tag.Name(); ok {
		t.tagsByName[name] = tag.Clone()
	}
}

// InsertAll inserts each tag from the provided slice.
func (t *TagsStore) InsertAll(tags []Tag) {
	for _, tag := range tags {
		t.Insert(tag)
	}
}

// SetSummarizer registers a summarizer for a numeric tag value.
func (t *TagsStore) SetSummarizer(tag TagValue, summarizer CBORSummarizer) {
	t.summarizers[tag] = summarizer
}

// AssignedNameForTag returns the registered name for the tag value, if present.
func (t *TagsStore) AssignedNameForTag(tag Tag) (string, bool) {
	stored, ok := t.tagsByValue[tag.Value()]
	if !ok {
		return "", false
	}
	return stored.Name()
}

// NameForTag returns the assigned name for a tag, or numeric fallback text.
func (t *TagsStore) NameForTag(tag Tag) string {
	if name, ok := t.AssignedNameForTag(tag); ok {
		return name
	}
	return fmt.Sprintf("%d", tag.Value())
}

// TagForValue looks up a tag definition by numeric value.
func (t *TagsStore) TagForValue(value TagValue) (Tag, bool) {
	tag, ok := t.tagsByValue[value]
	if !ok {
		return Tag{}, false
	}
	return tag.Clone(), true
}

// TagForName looks up a tag definition by assigned name.
func (t *TagsStore) TagForName(name string) (Tag, bool) {
	tag, ok := t.tagsByName[name]
	if !ok {
		return Tag{}, false
	}
	return tag.Clone(), true
}

// NameForValue returns the assigned name for a value, or numeric fallback text.
func (t *TagsStore) NameForValue(value TagValue) string {
	tag, ok := t.tagsByValue[value]
	if !ok {
		return fmt.Sprintf("%d", value)
	}
	name, hasName := tag.Name()
	if !hasName {
		return fmt.Sprintf("%d", value)
	}
	return name
}

// Summarizer looks up the registered summarizer for a tag value.
func (t *TagsStore) Summarizer(tag TagValue) (CBORSummarizer, bool) {
	s, ok := t.summarizers[tag]
	return s, ok
}

// LazyTagsStore lazily initializes a process-wide tag store.
type LazyTagsStore struct {
	once sync.Once
	mu   sync.Mutex
	data *TagsStore
}

func (l *LazyTagsStore) init() {
	l.once.Do(func() {
		l.mu.Lock()
		defer l.mu.Unlock()
		if l.data == nil {
			l.data = NewTagsStore(nil)
		}
	})
}

// Get returns the lazily initialized global tag store instance.
func (l *LazyTagsStore) Get() *TagsStore {
	l.init()
	l.mu.Lock()
	defer l.mu.Unlock()
	return l.data
}

// GlobalTags is the process-wide lazily initialized tag registry.
var GlobalTags = &LazyTagsStore{}

func withLockedGlobalTags[T any](action func(*TagsStore) T) T {
	GlobalTags.init()
	GlobalTags.mu.Lock()
	defer GlobalTags.mu.Unlock()
	return action(GlobalTags.data)
}

// WithTags provides read-style access to the global tags store under lock.
func WithTags[T any](action func(*TagsStore) T) T {
	return withLockedGlobalTags(action)
}

const (
	TagDate               uint64 = 1
	TagNameDate                  = "date"
	TagPositiveBignum     uint64 = 2
	TagNamePositiveBignum        = "positive-bignum"
	TagNegativeBignum     uint64 = 3
	TagNameNegativeBignum        = "negative-bignum"
)

// RegisterTagsIn populates a tag store with the default dCBOR tag set and summarizers.
func RegisterTagsIn(tagsStore *TagsStore) {
	tagsStore.InsertAll([]Tag{
		NewTag(TagDate, TagNameDate),
		NewTag(TagPositiveBignum, TagNamePositiveBignum),
		NewTag(TagNegativeBignum, TagNameNegativeBignum),
	})
	tagsStore.SetSummarizer(TagDate, func(untagged CBOR, _ bool) (string, error) {
		date, err := DateFromUntaggedCBOR(untagged)
		if err != nil {
			return "", err
		}
		return date.String(), nil
	})
	tagsStore.SetSummarizer(TagPositiveBignum, func(untagged CBOR, _ bool) (string, error) {
		value, err := decodePositiveBigNumUntagged(untagged)
		if err != nil {
			return "", err
		}
		return fmt.Sprintf("bignum(%s)", value.String()), nil
	})
	tagsStore.SetSummarizer(TagNegativeBignum, func(untagged CBOR, _ bool) (string, error) {
		value, err := decodeNegativeBigNumUntagged(untagged)
		if err != nil {
			return "", err
		}
		return fmt.Sprintf("bignum(%s)", value.String()), nil
	})
}

// RegisterTags registers the default dCBOR tags in the global tag store.
func RegisterTags() {
	WithTags(func(tagsStore *TagsStore) struct{} {
		RegisterTagsIn(tagsStore)
		return struct{}{}
	})
}

// TagsForValues resolves numeric tag values to registered tags when available.
func TagsForValues(values []TagValue) []Tag {
	return WithTags(func(tagsStore *TagsStore) []Tag {
		result := make([]Tag, 0, len(values))
		for _, value := range values {
			if tag, ok := tagsStore.TagForValue(value); ok {
				result = append(result, tag)
			} else {
				result = append(result, TagWithValue(value))
			}
		}
		return result
	})
}

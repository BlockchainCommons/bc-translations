package dcbor

import (
	"fmt"
	"sync"
)

// CBORSummarizer summarizes tagged CBOR content for diagnostic/summary output.
type CBORSummarizer func(cbor CBOR, flat bool) (string, error)

// TagsStoreTrait defines tag/name/summarizer lookup operations.
type TagsStoreTrait interface {
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
	Store TagsStoreTrait
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
func TagsCustom(store TagsStoreTrait) TagsStoreOpt {
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
	t.tagsByValue[tag.Value()] = tag.clone()
	if name, ok := tag.Name(); ok {
		t.tagsByName[name] = tag.clone()
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
	return tag.clone(), true
}

// TagForName looks up a tag definition by assigned name.
func (t *TagsStore) TagForName(name string) (Tag, bool) {
	tag, ok := t.tagsByName[name]
	if !ok {
		return Tag{}, false
	}
	return tag.clone(), true
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

// GLOBAL_TAGS is the process-wide lazily initialized tag registry.
var GLOBAL_TAGS = &LazyTagsStore{}

func withLockedGlobalTags[T any](action func(*TagsStore) T) T {
	GLOBAL_TAGS.init()
	GLOBAL_TAGS.mu.Lock()
	defer GLOBAL_TAGS.mu.Unlock()
	return action(GLOBAL_TAGS.data)
}

// WithTags provides read-style access to the global tags store under lock.
func WithTags[T any](action func(*TagsStore) T) T {
	return withLockedGlobalTags(action)
}

// WithTagsMut provides write-style access to the global tags store under lock.
func WithTagsMut[T any](action func(*TagsStore) T) T {
	return withLockedGlobalTags(action)
}

const (
	TAG_DATE                 uint64 = 1
	TAG_NAME_DATE                   = "date"
	TAG_POSITIVE_BIGNUM      uint64 = 2
	TAG_NAME_POSITIVE_BIGNUM        = "positive-bignum"
	TAG_NEGATIVE_BIGNUM      uint64 = 3
	TAG_NAME_NEGATIVE_BIGNUM        = "negative-bignum"
)

// RegisterTagsIn populates a tag store with the default dCBOR tag set and summarizers.
func RegisterTagsIn(tagsStore *TagsStore) {
	tagsStore.InsertAll([]Tag{
		NewTag(TAG_DATE, TAG_NAME_DATE),
		NewTag(TAG_POSITIVE_BIGNUM, TAG_NAME_POSITIVE_BIGNUM),
		NewTag(TAG_NEGATIVE_BIGNUM, TAG_NAME_NEGATIVE_BIGNUM),
	})
	tagsStore.SetSummarizer(TAG_DATE, func(untagged CBOR, _ bool) (string, error) {
		date, err := DateFromUntaggedCBOR(untagged)
		if err != nil {
			return "", err
		}
		return date.String(), nil
	})
	tagsStore.SetSummarizer(TAG_POSITIVE_BIGNUM, func(untagged CBOR, _ bool) (string, error) {
		value, err := decodePositiveBigNumUntagged(untagged)
		if err != nil {
			return "", err
		}
		return fmt.Sprintf("bignum(%s)", value.String()), nil
	})
	tagsStore.SetSummarizer(TAG_NEGATIVE_BIGNUM, func(untagged CBOR, _ bool) (string, error) {
		value, err := decodeNegativeBigNumUntagged(untagged)
		if err != nil {
			return "", err
		}
		return fmt.Sprintf("bignum(%s)", value.String()), nil
	})
}

// RegisterTags registers the default dCBOR tags in the global tag store.
func RegisterTags() {
	WithTagsMut(func(tagsStore *TagsStore) struct{} {
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

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

func TagsNone() TagsStoreOpt {
	return TagsStoreOpt{Mode: TagsStoreModeNone}
}

func TagsGlobal() TagsStoreOpt {
	return TagsStoreOpt{Mode: TagsStoreModeGlobal}
}

func TagsCustom(store TagsStoreTrait) TagsStoreOpt {
	return TagsStoreOpt{Mode: TagsStoreModeCustom, Store: store}
}

// TagsStore keeps tag registry state.
type TagsStore struct {
	tagsByValue map[TagValue]Tag
	tagsByName  map[string]Tag
	summarizers map[TagValue]CBORSummarizer
}

func NewTagsStore(tags []Tag) *TagsStore {
	ts := &TagsStore{
		tagsByValue: make(map[TagValue]Tag),
		tagsByName:  make(map[string]Tag),
		summarizers: make(map[TagValue]CBORSummarizer),
	}
	ts.InsertAll(tags)
	return ts
}

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

func (t *TagsStore) InsertAll(tags []Tag) {
	for _, tag := range tags {
		t.Insert(tag)
	}
}

func (t *TagsStore) SetSummarizer(tag TagValue, summarizer CBORSummarizer) {
	t.summarizers[tag] = summarizer
}

func (t *TagsStore) AssignedNameForTag(tag Tag) (string, bool) {
	stored, ok := t.tagsByValue[tag.Value()]
	if !ok {
		return "", false
	}
	return stored.Name()
}

func (t *TagsStore) NameForTag(tag Tag) string {
	if name, ok := t.AssignedNameForTag(tag); ok {
		return name
	}
	return fmt.Sprintf("%d", tag.Value())
}

func (t *TagsStore) TagForValue(value TagValue) (Tag, bool) {
	tag, ok := t.tagsByValue[value]
	if !ok {
		return Tag{}, false
	}
	return tag.clone(), true
}

func (t *TagsStore) TagForName(name string) (Tag, bool) {
	tag, ok := t.tagsByName[name]
	if !ok {
		return Tag{}, false
	}
	return tag.clone(), true
}

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

func (l *LazyTagsStore) Get() *TagsStore {
	l.init()
	l.mu.Lock()
	defer l.mu.Unlock()
	return l.data
}

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

func RegisterTags() {
	WithTagsMut(func(tagsStore *TagsStore) struct{} {
		RegisterTagsIn(tagsStore)
		return struct{}{}
	})
}

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

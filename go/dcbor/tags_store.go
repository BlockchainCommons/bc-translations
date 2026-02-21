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

func (l *LazyTagsStore) Get() *TagsStore {
	l.once.Do(func() {
		l.mu.Lock()
		defer l.mu.Unlock()
		l.data = NewTagsStore(nil)
	})
	l.mu.Lock()
	defer l.mu.Unlock()
	return l.data
}

var GLOBAL_TAGS = &LazyTagsStore{}

const (
	TAG_DATE      uint64 = 1
	TAG_NAME_DATE        = "date"
)

func RegisterTagsIn(tagsStore *TagsStore) {
	tagsStore.InsertAll([]Tag{NewTag(TAG_DATE, TAG_NAME_DATE)})
	tagsStore.SetSummarizer(TAG_DATE, func(untagged CBOR, _ bool) (string, error) {
		date, err := DateFromUntaggedCBOR(untagged)
		if err != nil {
			return "", err
		}
		return date.String(), nil
	})
}

func RegisterTags() {
	tagsStore := GLOBAL_TAGS.Get()
	RegisterTagsIn(tagsStore)
}

func TagsForValues(values []TagValue) []Tag {
	tagsStore := GLOBAL_TAGS.Get()
	result := make([]Tag, 0, len(values))
	for _, value := range values {
		if tag, ok := tagsStore.TagForValue(value); ok {
			result = append(result, tag)
		} else {
			result = append(result, TagWithValue(value))
		}
	}
	return result
}

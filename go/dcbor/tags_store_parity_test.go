package dcbor

import "testing"

func TestTagsStoreCoreParity(t *testing.T) {
	store := NewTagsStore([]Tag{NewTag(100, "alpha"), NewTag(200, "beta")})

	if got, want := store.NameForValue(100), "alpha"; got != want {
		t.Fatalf("NameForValue mismatch: got %q want %q", got, want)
	}
	if got, want := store.NameForTag(TagWithValue(200)), "beta"; got != want {
		t.Fatalf("NameForTag mismatch: got %q want %q", got, want)
	}
	if got, ok := store.AssignedNameForTag(TagWithValue(100)); !ok || got != "alpha" {
		t.Fatalf("AssignedNameForTag mismatch: got=%q ok=%v", got, ok)
	}
	if _, ok := store.AssignedNameForTag(TagWithValue(999)); ok {
		t.Fatalf("unexpected assigned name for unknown tag")
	}

	tagByValue, ok := store.TagForValue(100)
	if !ok {
		t.Fatalf("TagForValue failed")
	}
	if got, want := tagByValue.String(), "alpha"; got != want {
		t.Fatalf("TagForValue mismatch: got %q want %q", got, want)
	}

	tagByName, ok := store.TagForName("beta")
	if !ok {
		t.Fatalf("TagForName failed")
	}
	if got, want := tagByName.Value(), TagValue(200); got != want {
		t.Fatalf("TagForName value mismatch: got %d want %d", got, want)
	}
	if _, ok := store.TagForName("missing"); ok {
		t.Fatalf("expected missing TagForName lookup to fail")
	}
}

func TestTagsStoreInsertConflictParity(t *testing.T) {
	store := NewTagsStore(nil)
	store.Insert(NewTag(7, "seven"))
	defer func() {
		if r := recover(); r == nil {
			t.Fatalf("expected panic for conflicting tag name assignment")
		}
	}()
	store.Insert(NewTag(7, "siete"))
}

func TestTagRegistrationAndSummarizerParity(t *testing.T) {
	store := NewTagsStore(nil)
	RegisterTagsIn(store)

	tag, ok := store.TagForValue(TagDate)
	if !ok {
		t.Fatalf("expected TagDate registration")
	}
	if got, want := tag.String(), TagNameDate; got != want {
		t.Fatalf("registered tag name mismatch: got %q want %q", got, want)
	}

	summarizer, ok := store.Summarizer(TagDate)
	if !ok {
		t.Fatalf("expected date summarizer registration")
	}
	out, err := summarizer(MustFromAny(0), true)
	if err != nil {
		t.Fatalf("date summarizer failed: %v", err)
	}
	if got, want := out, "1970-01-01"; got != want {
		t.Fatalf("date summarizer output mismatch: got %q want %q", got, want)
	}

	positiveBigNumTag, ok := store.TagForValue(TagPositiveBignum)
	if !ok {
		t.Fatalf("expected TagPositiveBignum registration")
	}
	if got, want := positiveBigNumTag.String(), TagNamePositiveBignum; got != want {
		t.Fatalf("positive bignum tag name mismatch: got %q want %q", got, want)
	}

	negativeBigNumTag, ok := store.TagForValue(TagNegativeBignum)
	if !ok {
		t.Fatalf("expected TagNegativeBignum registration")
	}
	if got, want := negativeBigNumTag.String(), TagNameNegativeBignum; got != want {
		t.Fatalf("negative bignum tag name mismatch: got %q want %q", got, want)
	}

	positiveSummarizer, ok := store.Summarizer(TagPositiveBignum)
	if !ok {
		t.Fatalf("expected positive bignum summarizer registration")
	}
	positiveOut, err := positiveSummarizer(ToByteString([]byte{0x01, 0x00}), true)
	if err != nil {
		t.Fatalf("positive bignum summarizer failed: %v", err)
	}
	if got, want := positiveOut, "bignum(256)"; got != want {
		t.Fatalf("positive bignum summarizer output mismatch: got %q want %q", got, want)
	}

	negativeSummarizer, ok := store.Summarizer(TagNegativeBignum)
	if !ok {
		t.Fatalf("expected negative bignum summarizer registration")
	}
	negativeOut, err := negativeSummarizer(ToByteString([]byte{0x00}), true)
	if err != nil {
		t.Fatalf("negative bignum summarizer failed: %v", err)
	}
	if got, want := negativeOut, "bignum(-1)"; got != want {
		t.Fatalf("negative bignum summarizer output mismatch: got %q want %q", got, want)
	}
}

func TestTagsStoreOptHelpersParity(t *testing.T) {
	store := NewTagsStore(nil)

	if got := TagsNone(); got.Mode != TagsStoreModeNone {
		t.Fatalf("TagsNone mode mismatch: got %v", got.Mode)
	}
	if got := TagsGlobal(); got.Mode != TagsStoreModeGlobal {
		t.Fatalf("TagsGlobal mode mismatch: got %v", got.Mode)
	}
	custom := TagsCustom(store)
	if custom.Mode != TagsStoreModeCustom || custom.Store != store {
		t.Fatalf("TagsCustom mismatch: mode=%v store=%v", custom.Mode, custom.Store)
	}
}

func TestWithTagsHelperParity(t *testing.T) {
	const testTagValue TagValue = 90001
	const testTagName = "with-tags-parity"

	WithTags(func(store *TagsStore) struct{} {
		store.Insert(NewTag(testTagValue, testTagName))
		return struct{}{}
	})

	gotName := WithTags(func(store *TagsStore) string {
		return store.NameForValue(testTagValue)
	})
	if gotName != testTagName {
		t.Fatalf("WithTags name mismatch: got %q want %q", gotName, testTagName)
	}

	type tagLookup struct {
		tag Tag
		ok  bool
	}
	lookup := WithTags(func(store *TagsStore) tagLookup {
		tag, ok := store.TagForName(testTagName)
		return tagLookup{tag: tag, ok: ok}
	})
	if !lookup.ok {
		t.Fatalf("WithTags TagForName lookup failed")
	}
	if lookup.tag.Value() != testTagValue {
		t.Fatalf("WithTags tag value mismatch: got %d want %d", lookup.tag.Value(), testTagValue)
	}
}

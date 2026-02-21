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

	tag, ok := store.TagForValue(TAG_DATE)
	if !ok {
		t.Fatalf("expected TAG_DATE registration")
	}
	if got, want := tag.String(), TAG_NAME_DATE; got != want {
		t.Fatalf("registered tag name mismatch: got %q want %q", got, want)
	}

	summarizer, ok := store.Summarizer(TAG_DATE)
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

	positiveBigNumTag, ok := store.TagForValue(TAG_POSITIVE_BIGNUM)
	if !ok {
		t.Fatalf("expected TAG_POSITIVE_BIGNUM registration")
	}
	if got, want := positiveBigNumTag.String(), TAG_NAME_POSITIVE_BIGNUM; got != want {
		t.Fatalf("positive bignum tag name mismatch: got %q want %q", got, want)
	}

	negativeBigNumTag, ok := store.TagForValue(TAG_NEGATIVE_BIGNUM)
	if !ok {
		t.Fatalf("expected TAG_NEGATIVE_BIGNUM registration")
	}
	if got, want := negativeBigNumTag.String(), TAG_NAME_NEGATIVE_BIGNUM; got != want {
		t.Fatalf("negative bignum tag name mismatch: got %q want %q", got, want)
	}

	positiveSummarizer, ok := store.Summarizer(TAG_POSITIVE_BIGNUM)
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

	negativeSummarizer, ok := store.Summarizer(TAG_NEGATIVE_BIGNUM)
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

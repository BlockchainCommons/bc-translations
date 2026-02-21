package dcbor

import "testing"

func TestTagDisplayParity(t *testing.T) {
	tag := NewTag(1, "A")
	if got, want := tag.String(), "A"; got != want {
		t.Fatalf("named tag display mismatch: got %q want %q", got, want)
	}
	if got, ok := tag.Name(); !ok || got != "A" {
		t.Fatalf("named tag Name mismatch: got %q ok=%v", got, ok)
	}

	unnamed := TagWithValue(2)
	if got, want := unnamed.String(), "2"; got != want {
		t.Fatalf("unnamed tag display mismatch: got %q want %q", got, want)
	}
	if _, ok := unnamed.Name(); ok {
		t.Fatalf("expected unnamed tag to have no name")
	}
}

func TestExpectedTaggedValueParity(t *testing.T) {
	value := ToTaggedValue(TagWithValue(1), MustFromAny("Hello"))

	content, err := value.TryIntoExpectedTaggedValue(TagWithValue(1))
	if err != nil {
		t.Fatalf("TryIntoExpectedTaggedValue failed: %v", err)
	}
	if got, want := content.String(), `"Hello"`; got != want {
		t.Fatalf("unexpected tagged content: got %q want %q", got, want)
	}

	_, err = value.TryIntoExpectedTaggedValue(TagWithValue(2))
	if err == nil {
		t.Fatalf("expected wrong-tag error")
	}
	if _, ok := err.(WrongTagError); !ok {
		t.Fatalf("expected WrongTagError, got %T", err)
	}
}

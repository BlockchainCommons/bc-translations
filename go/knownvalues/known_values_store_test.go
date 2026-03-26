package knownvalues

import "testing"

func TestNewAndLookupHelpersMatchRustSemantics(t *testing.T) {
	store := NewKnownValuesStore(IsA, Note, Signed)

	if got, ok := store.AssignedName(IsA); !ok || got != "isA" {
		t.Fatalf("AssignedName mismatch: got %q ok=%t", got, ok)
	}
	if got, want := store.Name(Signed), "signed"; got != want {
		t.Fatalf("Name mismatch: got %q want %q", got, want)
	}
	if got, ok := store.KnownValueNamed("isA"); !ok || got.Value() != 1 {
		t.Fatalf("KnownValueNamed mismatch: got %v ok=%t", got, ok)
	}

	fromRaw := KnownValueForRawValue(1, store)
	if got, want := fromRaw.Name(), "isA"; got != want {
		t.Fatalf("KnownValueForRawValue mismatch: got %q want %q", got, want)
	}

	unknown := KnownValueForRawValue(999, store)
	if got, want := unknown.Name(), "999"; got != want {
		t.Fatalf("unknown KnownValueForRawValue mismatch: got %q want %q", got, want)
	}

	fromName, ok := KnownValueForName("note", store)
	if !ok || fromName.Value() != 4 {
		t.Fatalf("KnownValueForName mismatch: got %v ok=%t", fromName, ok)
	}
	if _, ok := KnownValueForName("unknown", store); ok {
		t.Fatalf("unexpected lookup hit for unknown name")
	}
	if got, want := NameForKnownValue(Signed, store), "signed"; got != want {
		t.Fatalf("NameForKnownValue mismatch: got %q want %q", got, want)
	}
	if got, want := NameForKnownValue(NewKnownValue(999), store), "999"; got != want {
		t.Fatalf("unknown NameForKnownValue mismatch: got %q want %q", got, want)
	}
}

func TestInsertRemovesStaleNameWhenCodepointIsOverridden(t *testing.T) {
	store := NewKnownValuesStore(IsA)
	store.Insert(NewKnownValueWithName(1, "overriddenIsA"))

	if _, ok := store.KnownValueNamed("isA"); ok {
		t.Fatalf("old name should have been removed after override")
	}
	if got, ok := store.KnownValueNamed("overriddenIsA"); !ok || got.Value() != 1 {
		t.Fatalf("override lookup mismatch: got %v ok=%t", got, ok)
	}
}

func TestCloneCreatesIndependentStoreCopy(t *testing.T) {
	original := NewKnownValuesStore(IsA)
	clone := original.Clone()
	clone.Insert(NewKnownValueWithName(100, "customValue"))

	if _, ok := original.KnownValueNamed("customValue"); ok {
		t.Fatalf("clone should not mutate original store")
	}
	if got, ok := clone.KnownValueNamed("customValue"); !ok || got.Value() != 100 {
		t.Fatalf("clone lookup mismatch: got %v ok=%t", got, ok)
	}
}

func TestZeroValueStoreIsUsable(t *testing.T) {
	var store KnownValuesStore
	if _, ok := store.KnownValueNamed("isA"); ok {
		t.Fatalf("zero-value store should be empty")
	}
	if got, want := store.Name(NewKnownValue(1)), "1"; got != want {
		t.Fatalf("zero-value store name mismatch: got %q want %q", got, want)
	}
}

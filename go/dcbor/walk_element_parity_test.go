package dcbor

import "testing"

func TestWalkElementHelpersParity(t *testing.T) {
	singleElement := walkSingle(MustFromAny("hello"))
	if single, ok := singleElement.AsSingle(); !ok || single.DiagnosticFlat() != `"hello"` {
		t.Fatalf("AsSingle mismatch: ok=%v value=%s", ok, single.DiagnosticFlat())
	}
	if _, _, ok := singleElement.AsKeyValue(); ok {
		t.Fatalf("expected AsKeyValue to fail for single element")
	}
	if got, want := singleElement.DiagnosticFlat(), `"hello"`; got != want {
		t.Fatalf("single DiagnosticFlat mismatch: got %q want %q", got, want)
	}

	keyValueElement := walkKeyValue(MustFromAny("k"), MustFromAny(42))
	if _, ok := keyValueElement.AsSingle(); ok {
		t.Fatalf("expected AsSingle to fail for key/value element")
	}
	key, value, ok := keyValueElement.AsKeyValue()
	if !ok || !key.Equal(MustFromAny("k")) || !value.Equal(MustFromAny(42)) {
		t.Fatalf("AsKeyValue mismatch: key=%s value=%s ok=%v", key.DiagnosticFlat(), value.DiagnosticFlat(), ok)
	}
	if got, want := keyValueElement.DiagnosticFlat(), `"k": 42`; got != want {
		t.Fatalf("key/value DiagnosticFlat mismatch: got %q want %q", got, want)
	}
}

func TestEdgeLabelParityMatrix(t *testing.T) {
	cases := []struct {
		edge   EdgeType
		label  string
		hasLbl bool
	}{
		{edge: EdgeNone(), label: "", hasLbl: false},
		{edge: EdgeArrayElement(5), label: "arr[5]", hasLbl: true},
		{edge: EdgeMapKeyValue(), label: "kv", hasLbl: true},
		{edge: EdgeMapKey(), label: "key", hasLbl: true},
		{edge: EdgeMapValue(), label: "val", hasLbl: true},
		{edge: EdgeTaggedContent(), label: "content", hasLbl: true},
	}

	for _, tc := range cases {
		got, ok := tc.edge.Label()
		if ok != tc.hasLbl {
			t.Fatalf("edge %+v label presence mismatch: got %v want %v", tc.edge, ok, tc.hasLbl)
		}
		if got != tc.label {
			t.Fatalf("edge %+v label mismatch: got %q want %q", tc.edge, got, tc.label)
		}
	}
}

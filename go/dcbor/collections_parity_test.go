package dcbor

import (
	"errors"
	"testing"
)

func TestMapAPIParity(t *testing.T) {
	m := NewMap()
	if !m.IsEmpty() || m.Len() != 0 {
		t.Fatalf("expected empty map initially")
	}

	m.MustInsertAny("b", 2)
	m.MustInsertAny("a", 1)
	m.Insert(MustFromAny(10), MustFromAny(3))

	if m.IsEmpty() || m.Len() != 3 {
		t.Fatalf("expected 3 map entries, got %d", m.Len())
	}
	if !m.ContainsKey(MustFromAny("a")) || m.ContainsKey(MustFromAny("missing")) {
		t.Fatalf("ContainsKey parity mismatch")
	}

	if got, ok := m.GetAny("a"); !ok || !got.Equal(MustFromAny(1)) {
		t.Fatalf("GetAny mismatch for key 'a'")
	}
	if got, err := m.ExtractAny(10); err != nil || !got.Equal(MustFromAny(3)) {
		t.Fatalf("ExtractAny mismatch for key 10: value=%v err=%v", got, err)
	}
	if got, ok, err := DecodeMapValue(m, "a", DecodeInt64); err != nil || !ok || got != 1 {
		t.Fatalf("GetDecoded mismatch for key 'a': value=%d ok=%v err=%v", got, ok, err)
	}
	if _, ok, err := DecodeMapValue(m, "missing", DecodeInt64); err != nil || ok {
		t.Fatalf("GetDecoded expected missing key: ok=%v err=%v", ok, err)
	}
	if _, ok, err := DecodeMapValue(m, "a", DecodeText); !ok || !errors.Is(err, ErrWrongType) {
		t.Fatalf("GetDecoded expected decode error on key 'a': ok=%v err=%v", ok, err)
	}
	if got, err := ExtractMapValue(m, 10, DecodeInt64); err != nil || got != 3 {
		t.Fatalf("ExtractDecoded mismatch for key 10: value=%d err=%v", got, err)
	}
	if _, err := ExtractMapValue(m, "missing", DecodeInt64); !errors.Is(err, ErrMissingMapKey) {
		t.Fatalf("ExtractDecoded expected ErrMissingMapKey, got %v", err)
	}
	if got := MustExtractMapValue(m, 10, DecodeInt64); got != 3 {
		t.Fatalf("MustExtractDecoded mismatch for key 10: got %d", got)
	}

	iter := m.Iter()
	k1, _, ok := iter.Next()
	if !ok {
		t.Fatalf("expected first iterator element")
	}
	if got, _ := k1.AsInt64(); got != 10 {
		t.Fatalf("unexpected first key ordering: got %s", k1.DiagnosticFlat())
	}

	clone := m.Clone()
	m.MustInsertAny("c", 4)
	if clone.Len() != 3 || m.Len() != 4 {
		t.Fatalf("clone/original independence mismatch: clone=%d original=%d", clone.Len(), m.Len())
	}

	entries := clone.AsEntries()
	if len(entries) != 3 {
		t.Fatalf("AsEntries size mismatch: got %d want 3", len(entries))
	}
}

func TestSetAPIParity(t *testing.T) {
	s := NewSet()
	if !s.IsEmpty() || s.Len() != 0 {
		t.Fatalf("expected empty set initially")
	}

	s.Insert(MustFromAny(2))
	s.Insert(MustFromAny(1))
	s.Insert(MustFromAny(2)) // duplicate insert should be idempotent
	if s.IsEmpty() || s.Len() != 2 {
		t.Fatalf("expected set size 2, got %d", s.Len())
	}
	if !s.Contains(MustFromAny(1)) || s.Contains(MustFromAny(3)) {
		t.Fatalf("Contains parity mismatch")
	}

	values := s.AsVec()
	if len(values) != 2 {
		t.Fatalf("AsVec size mismatch: got %d", len(values))
	}
	if got, _ := values[0].AsInt64(); got != 1 {
		t.Fatalf("set ordering mismatch: first=%s", values[0].DiagnosticFlat())
	}
	if got, _ := values[1].AsInt64(); got != 2 {
		t.Fatalf("set ordering mismatch: second=%s", values[1].DiagnosticFlat())
	}

	iter := s.Iter()
	first, ok := iter.Next()
	if !ok {
		t.Fatalf("expected first set iterator element")
	}
	if got, _ := first.AsInt64(); got != 1 {
		t.Fatalf("set iterator ordering mismatch: got %s", first.DiagnosticFlat())
	}

	clone := s.Clone()
	s.Insert(MustFromAny(3))
	if clone.Len() != 2 || s.Len() != 3 {
		t.Fatalf("set clone/original independence mismatch: clone=%d original=%d", clone.Len(), s.Len())
	}
}

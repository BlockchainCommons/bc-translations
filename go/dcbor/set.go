package dcbor

// Set is a deterministic CBOR set backed by a deterministic map (value->value).
type Set struct {
	m Map
}

// NewSet creates an empty deterministic set.
func NewSet() Set {
	return Set{m: NewMap()}
}

// Len returns the number of set elements.
func (s *Set) Len() int {
	return s.m.Len()
}

// IsEmpty reports whether the set has no elements.
func (s *Set) IsEmpty() bool {
	return s.m.IsEmpty()
}

// Iter returns an iterator over values in deterministic encoded order.
func (s *Set) Iter() SetIter {
	return SetIter{inner: s.m.Iter()}
}

// Insert adds a value to the set.
func (s *Set) Insert(value CBOR) {
	s.m.Insert(value, value)
}

func (s *Set) insertNext(value CBOR) error {
	return s.m.insertNext(value, value)
}

// Contains reports whether a value exists in the set.
func (s *Set) Contains(value CBOR) bool {
	return s.m.ContainsKey(value)
}

// AsVec returns set values in deterministic encoded order.
func (s *Set) AsVec() []CBOR {
	items := make([]CBOR, 0, s.m.Len())
	iter := s.m.Iter()
	for {
		_, value, ok := iter.Next()
		if !ok {
			break
		}
		items = append(items, value)
	}
	return items
}

// SetFromVec builds a set from values using standard insertion semantics.
func SetFromVec(items []CBOR) Set {
	set := NewSet()
	for _, item := range items {
		set.Insert(item)
	}
	return set
}

// TrySetFromVec builds a set while requiring canonical order and no duplicates.
func TrySetFromVec(items []CBOR) (Set, error) {
	set := NewSet()
	for _, item := range items {
		if err := set.insertNext(item); err != nil {
			return Set{}, err
		}
	}
	return set, nil
}

// CBORData returns deterministic binary encoding for set-as-array representation.
func (s *Set) CBORData() []byte {
	items := s.AsVec()
	array := NewCBORArray(items)
	return array.ToCBORData()
}

// Clone returns an independent copy of the set.
func (s *Set) Clone() Set {
	return Set{m: s.m.Clone()}
}

// Equal reports deterministic set equality by encoded item ordering/content.
func (s Set) Equal(other Set) bool {
	return s.m.Equal(other.m)
}

// SetIter iterates set elements.
type SetIter struct {
	inner MapIter
}

// Next yields the next set element in deterministic order.
func (it *SetIter) Next() (CBOR, bool) {
	_, value, ok := it.inner.Next()
	if !ok {
		return CBOR{}, false
	}
	return value, true
}

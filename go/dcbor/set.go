package dcbor

// Set is a deterministic CBOR set backed by a deterministic map (value->value).
type Set struct {
	m Map
}

func NewSet() Set {
	return Set{m: NewMap()}
}

func (s *Set) Len() int {
	return s.m.Len()
}

func (s *Set) IsEmpty() bool {
	return s.m.IsEmpty()
}

func (s *Set) Iter() SetIter {
	return SetIter{inner: s.m.Iter()}
}

func (s *Set) Insert(value CBOR) {
	s.m.Insert(value, value)
}

func (s *Set) insertNext(value CBOR) error {
	return s.m.insertNext(value, value)
}

func (s *Set) Contains(value CBOR) bool {
	return s.m.ContainsKey(value)
}

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

func SetFromVec(items []CBOR) Set {
	set := NewSet()
	for _, item := range items {
		set.Insert(item)
	}
	return set
}

func TrySetFromVec(items []CBOR) (Set, error) {
	set := NewSet()
	for _, item := range items {
		if err := set.insertNext(item); err != nil {
			return Set{}, err
		}
	}
	return set, nil
}

func (s *Set) CBORData() []byte {
	items := s.AsVec()
	array := NewCBORArray(items)
	return array.ToCBORData()
}

func (s *Set) Clone() Set {
	return Set{m: s.m.Clone()}
}

// SetIter iterates set elements.
type SetIter struct {
	inner MapIter
}

func (it *SetIter) Next() (CBOR, bool) {
	_, value, ok := it.inner.Next()
	if !ok {
		return CBOR{}, false
	}
	return value, true
}

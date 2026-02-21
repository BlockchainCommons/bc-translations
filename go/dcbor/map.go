package dcbor

import (
	"bytes"
	"encoding/hex"
)

type mapEntry struct {
	key     CBOR
	value   CBOR
	keyData []byte
}

// Map is a deterministic CBOR map sorted by key CBOR encoding bytes.
type Map struct {
	entries []mapEntry
}

func NewMap() Map {
	return Map{entries: nil}
}

func (m Map) Len() int {
	return len(m.entries)
}

func (m Map) IsEmpty() bool {
	return len(m.entries) == 0
}

func (m Map) Iter() MapIter {
	copied := make([]mapEntry, len(m.entries))
	copy(copied, m.entries)
	return MapIter{entries: copied}
}

func (m *Map) Insert(key CBOR, value CBOR) {
	keyData := key.ToCBORData()
	idx := m.indexForKeyData(keyData)
	if idx >= 0 {
		m.entries[idx] = mapEntry{key: key.Clone(), value: value.Clone(), keyData: keyData}
		return
	}
	entry := mapEntry{key: key.Clone(), value: value.Clone(), keyData: keyData}
	insertAt := m.firstGreaterIndex(keyData)
	m.entries = append(m.entries, mapEntry{})
	copy(m.entries[insertAt+1:], m.entries[insertAt:])
	m.entries[insertAt] = entry
}

func (m *Map) InsertAny(key any, value any) error {
	keyCBOR, err := FromAny(key)
	if err != nil {
		return err
	}
	valueCBOR, err := FromAny(value)
	if err != nil {
		return err
	}
	m.Insert(keyCBOR, valueCBOR)
	return nil
}

func (m *Map) MustInsertAny(key any, value any) {
	if err := m.InsertAny(key, value); err != nil {
		panic(err)
	}
}

func (m *Map) insertNext(key CBOR, value CBOR) error {
	keyData := key.ToCBORData()
	if len(m.entries) == 0 {
		m.entries = append(m.entries, mapEntry{key: key.Clone(), value: value.Clone(), keyData: keyData})
		return nil
	}
	last := m.entries[len(m.entries)-1]
	cmp := bytes.Compare(last.keyData, keyData)
	if cmp == 0 {
		return ErrDuplicateMapKey
	}
	if cmp > 0 {
		return ErrMisorderedMapKey
	}
	m.entries = append(m.entries, mapEntry{key: key.Clone(), value: value.Clone(), keyData: keyData})
	return nil
}

func (m Map) Get(key CBOR) (CBOR, bool) {
	keyData := key.ToCBORData()
	idx := m.indexForKeyData(keyData)
	if idx < 0 {
		return CBOR{}, false
	}
	return m.entries[idx].value.Clone(), true
}

func (m Map) GetAny(key any) (CBOR, bool) {
	keyCBOR, err := FromAny(key)
	if err != nil {
		return CBOR{}, false
	}
	return m.Get(keyCBOR)
}

func (m Map) ContainsKey(key CBOR) bool {
	_, ok := m.Get(key)
	return ok
}

func (m Map) Extract(key CBOR) (CBOR, error) {
	value, ok := m.Get(key)
	if !ok {
		return CBOR{}, ErrMissingMapKey
	}
	return value, nil
}

func (m Map) ExtractAny(key any) (CBOR, error) {
	keyCBOR, err := FromAny(key)
	if err != nil {
		return CBOR{}, err
	}
	return m.Extract(keyCBOR)
}

// DecodeMapValue looks up a key and decodes the mapped CBOR value into a typed value.
// The boolean return indicates whether the key exists.
func DecodeMapValue[T any](m Map, key any, decode CBORDecodeFunc[T]) (T, bool, error) {
	var zero T
	value, ok := m.GetAny(key)
	if !ok {
		return zero, false, nil
	}
	decoded, err := decode(value)
	if err != nil {
		return zero, true, err
	}
	return decoded, true, nil
}

// ExtractMapValue extracts and decodes a required map value.
func ExtractMapValue[T any](m Map, key any, decode CBORDecodeFunc[T]) (T, error) {
	var zero T
	value, err := m.ExtractAny(key)
	if err != nil {
		return zero, err
	}
	decoded, err := decode(value)
	if err != nil {
		return zero, err
	}
	return decoded, nil
}

// MustExtractMapValue extracts and decodes a required map value and panics on failure.
func MustExtractMapValue[T any](m Map, key any, decode CBORDecodeFunc[T]) T {
	value, err := ExtractMapValue(m, key, decode)
	if err != nil {
		panic(err)
	}
	return value
}

func (m Map) keyDataHexes() []string {
	keys := make([]string, 0, len(m.entries))
	for _, entry := range m.entries {
		keys = append(keys, hex.EncodeToString(entry.keyData))
	}
	return keys
}

func (m Map) CBORData() []byte {
	result := encodeHead(majorMap, uint64(len(m.entries)))
	for _, entry := range m.entries {
		result = append(result, entry.keyData...)
		result = append(result, entry.value.ToCBORData()...)
	}
	return result
}

func (m Map) AsEntries() []MapEntry {
	out := make([]MapEntry, 0, len(m.entries))
	for _, entry := range m.entries {
		out = append(out, MapEntry{Key: entry.key.Clone(), Value: entry.value.Clone()})
	}
	return out
}

func (m Map) Clone() Map {
	cloned := make([]mapEntry, len(m.entries))
	for i, entry := range m.entries {
		keyDataCopy := make([]byte, len(entry.keyData))
		copy(keyDataCopy, entry.keyData)
		cloned[i] = mapEntry{
			key:     entry.key.Clone(),
			value:   entry.value.Clone(),
			keyData: keyDataCopy,
		}
	}
	return Map{entries: cloned}
}

// Equal reports deterministic equality by key encoding and value content.
func (m Map) Equal(other Map) bool {
	if len(m.entries) != len(other.entries) {
		return false
	}
	for i := range m.entries {
		left := m.entries[i]
		right := other.entries[i]
		if !bytes.Equal(left.keyData, right.keyData) {
			return false
		}
		if !left.value.Equal(right.value) {
			return false
		}
	}
	return true
}

func (m Map) firstGreaterIndex(keyData []byte) int {
	for i, entry := range m.entries {
		if bytes.Compare(entry.keyData, keyData) > 0 {
			return i
		}
	}
	return len(m.entries)
}

func (m Map) indexForKeyData(keyData []byte) int {
	for i, entry := range m.entries {
		if bytes.Equal(entry.keyData, keyData) {
			return i
		}
	}
	return -1
}

// MapEntry is a public key/value pair view for deterministic map iteration.
type MapEntry struct {
	Key   CBOR
	Value CBOR
}

// MapIter iterates deterministic map entries.
type MapIter struct {
	entries []mapEntry
	index   int
}

func (it *MapIter) Next() (CBOR, CBOR, bool) {
	if it.index >= len(it.entries) {
		return CBOR{}, CBOR{}, false
	}
	entry := it.entries[it.index]
	it.index++
	return entry.key.Clone(), entry.value.Clone(), true
}

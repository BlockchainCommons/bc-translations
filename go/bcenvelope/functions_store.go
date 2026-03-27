package bcenvelope

import "sync"

// FunctionsStore maps functions to their assigned names.
type FunctionsStore struct {
	dict map[uint64]functionEntry   // known functions keyed by value
	named map[string]functionEntry  // named functions keyed by name string
}

type functionEntry struct {
	function Function
	name     string
}

// NewFunctionsStore creates a new store populated with the provided functions.
// Only known (numeric) functions can be inserted.
func NewFunctionsStore(functions ...Function) *FunctionsStore {
	store := &FunctionsStore{
		dict:  make(map[uint64]functionEntry),
		named: make(map[string]functionEntry),
	}
	for _, f := range functions {
		store.Insert(f)
	}
	return store
}

// Insert adds a known function into the store.
// Panics if a named function is inserted.
func (s *FunctionsStore) Insert(function Function) {
	if function.kind != functionKindKnown {
		panic("only known functions can be inserted into FunctionsStore")
	}
	name := function.Name()
	s.dict[function.value] = functionEntry{function: function, name: name}
}

// AssignedName returns the assigned name for a function if it exists in the store.
func (s *FunctionsStore) AssignedName(function Function) (string, bool) {
	if s == nil {
		return "", false
	}
	if function.kind == functionKindKnown {
		entry, ok := s.dict[function.value]
		if !ok {
			return "", false
		}
		return entry.name, true
	}
	return "", false
}

// Name returns the name for a function from the store, falling back to the function's own name.
func (s *FunctionsStore) Name(function Function) string {
	if name, ok := s.AssignedName(function); ok {
		return name
	}
	return function.Name()
}

// NameForFunction returns the name of a function using an optional store.
func NameForFunction(function Function, store *FunctionsStore) string {
	if store != nil {
		if name, ok := store.AssignedName(function); ok {
			return name
		}
	}
	return function.Name()
}

// Clone returns an independent copy of the store.
func (s *FunctionsStore) Clone() *FunctionsStore {
	if s == nil {
		return NewFunctionsStore()
	}
	clone := NewFunctionsStore()
	for _, entry := range s.dict {
		clone.Insert(entry.function)
	}
	return clone
}

// Global functions store singleton.
var (
	globalFunctions     *FunctionsStore
	globalFunctionsOnce sync.Once
)

// GlobalFunctions returns the lazily initialized global functions store.
func GlobalFunctions() *FunctionsStore {
	globalFunctionsOnce.Do(func() {
		globalFunctions = NewFunctionsStore(
			FunctionAdd, FunctionSub, FunctionMul, FunctionDiv,
		)
	})
	return globalFunctions
}

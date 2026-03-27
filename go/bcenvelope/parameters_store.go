package bcenvelope

import "sync"

// ParametersStore maps parameters to their assigned names.
type ParametersStore struct {
	dict map[uint64]parameterEntry // known parameters keyed by value
}

type parameterEntry struct {
	parameter Parameter
	name      string
}

// NewParametersStore creates a new store populated with the provided parameters.
// Only known (numeric) parameters can be inserted.
func NewParametersStore(parameters ...Parameter) *ParametersStore {
	store := &ParametersStore{
		dict: make(map[uint64]parameterEntry),
	}
	for _, p := range parameters {
		store.Insert(p)
	}
	return store
}

// Insert adds a known parameter into the store.
// Panics if a named parameter is inserted.
func (s *ParametersStore) Insert(parameter Parameter) {
	if parameter.kind != parameterKindKnown {
		panic("only known parameters can be inserted into ParametersStore")
	}
	name := parameter.Name()
	s.dict[parameter.value] = parameterEntry{parameter: parameter, name: name}
}

// AssignedName returns the assigned name for a parameter if it exists in the store.
func (s *ParametersStore) AssignedName(parameter Parameter) (string, bool) {
	if s == nil {
		return "", false
	}
	if parameter.kind == parameterKindKnown {
		entry, ok := s.dict[parameter.value]
		if !ok {
			return "", false
		}
		return entry.name, true
	}
	return "", false
}

// Name returns the name for a parameter from the store, falling back to the parameter's own name.
func (s *ParametersStore) Name(parameter Parameter) string {
	if name, ok := s.AssignedName(parameter); ok {
		return name
	}
	return parameter.Name()
}

// NameForParameter returns the name of a parameter using an optional store.
func NameForParameter(parameter Parameter, store *ParametersStore) string {
	if store != nil {
		if name, ok := store.AssignedName(parameter); ok {
			return name
		}
	}
	return parameter.Name()
}

// Clone returns an independent copy of the store.
func (s *ParametersStore) Clone() *ParametersStore {
	if s == nil {
		return NewParametersStore()
	}
	clone := NewParametersStore()
	for _, entry := range s.dict {
		clone.Insert(entry.parameter)
	}
	return clone
}

// Global parameters store singleton.
var (
	globalParameters     *ParametersStore
	globalParametersOnce sync.Once
)

// GlobalParameters returns the lazily initialized global parameters store.
func GlobalParameters() *ParametersStore {
	globalParametersOnce.Do(func() {
		globalParameters = NewParametersStore(
			ParameterBlank, ParameterLHS, ParameterRHS,
		)
	})
	return globalParameters
}

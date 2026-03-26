package knownvalues

// KnownValuesStore maps between known values and assigned names.
type KnownValuesStore struct {
	knownValuesByRawValue     map[uint64]KnownValue
	knownValuesByAssignedName map[string]KnownValue
}

// NewKnownValuesStore creates a new store populated with the provided values.
func NewKnownValuesStore(knownValues ...KnownValue) *KnownValuesStore {
	store := &KnownValuesStore{}
	store.ensureInitialized()
	for _, knownValue := range knownValues {
		insertKnownValue(
			knownValue,
			store.knownValuesByRawValue,
			store.knownValuesByAssignedName,
		)
	}
	return store
}

func (s *KnownValuesStore) ensureInitialized() {
	if s.knownValuesByRawValue == nil {
		s.knownValuesByRawValue = make(map[uint64]KnownValue)
	}
	if s.knownValuesByAssignedName == nil {
		s.knownValuesByAssignedName = make(map[string]KnownValue)
	}
}

// Clone returns an independent copy of the store.
func (s *KnownValuesStore) Clone() *KnownValuesStore {
	if s == nil {
		return NewKnownValuesStore()
	}
	clone := NewKnownValuesStore()
	for _, knownValue := range s.knownValuesByRawValue {
		insertKnownValue(
			knownValue,
			clone.knownValuesByRawValue,
			clone.knownValuesByAssignedName,
		)
	}
	return clone
}

// Insert adds or replaces a known value in the store.
func (s *KnownValuesStore) Insert(knownValue KnownValue) {
	s.ensureInitialized()
	insertKnownValue(
		knownValue,
		s.knownValuesByRawValue,
		s.knownValuesByAssignedName,
	)
}

// AssignedName returns the store-backed assigned name for the given value.
func (s *KnownValuesStore) AssignedName(knownValue KnownValue) (string, bool) {
	if s == nil {
		return "", false
	}
	stored, ok := s.knownValuesByRawValue[knownValue.Value()]
	if !ok {
		return "", false
	}
	return stored.AssignedName()
}

// Name returns the display name for the given known value.
func (s *KnownValuesStore) Name(knownValue KnownValue) string {
	if assignedName, ok := s.AssignedName(knownValue); ok {
		return assignedName
	}
	return knownValue.Name()
}

// KnownValueNamed looks up a known value by assigned name.
func (s *KnownValuesStore) KnownValueNamed(assignedName string) (KnownValue, bool) {
	if s == nil {
		return KnownValue{}, false
	}
	knownValue, ok := s.knownValuesByAssignedName[assignedName]
	return knownValue, ok
}

// KnownValueForRawValue looks up a raw value in an optional store and falls
// back to creating an unnamed known value.
func KnownValueForRawValue(rawValue uint64, knownValues *KnownValuesStore) KnownValue {
	if knownValues != nil {
		if knownValue, ok := knownValues.knownValuesByRawValue[rawValue]; ok {
			return knownValue
		}
	}
	return NewKnownValue(rawValue)
}

// KnownValueForName looks up a known value by assigned name in an optional
// store.
func KnownValueForName(name string, knownValues *KnownValuesStore) (KnownValue, bool) {
	if knownValues == nil {
		return KnownValue{}, false
	}
	return knownValues.KnownValueNamed(name)
}

// NameForKnownValue returns the display name for a known value using an
// optional store override.
func NameForKnownValue(knownValue KnownValue, knownValues *KnownValuesStore) string {
	if knownValues != nil {
		if assignedName, ok := knownValues.AssignedName(knownValue); ok {
			return assignedName
		}
	}
	return knownValue.Name()
}

// LoadFromDirectory loads known values from JSON files in a single directory.
func (s *KnownValuesStore) LoadFromDirectory(path string) (int, error) {
	values, err := LoadFromDirectory(path)
	if err != nil {
		return 0, err
	}
	for _, value := range values {
		s.Insert(value)
	}
	return len(values), nil
}

// LoadFromConfig loads known values from all configured directories.
func (s *KnownValuesStore) LoadFromConfig(config DirectoryConfig) LoadResult {
	result := LoadFromConfig(config)
	for _, value := range result.Values() {
		s.Insert(value)
	}
	return result
}

func insertKnownValue(
	knownValue KnownValue,
	knownValuesByRawValue map[uint64]KnownValue,
	knownValuesByAssignedName map[string]KnownValue,
) {
	if oldValue, ok := knownValuesByRawValue[knownValue.Value()]; ok {
		if oldName, ok := oldValue.AssignedName(); ok {
			delete(knownValuesByAssignedName, oldName)
		}
	}

	knownValuesByRawValue[knownValue.Value()] = knownValue
	if assignedName, ok := knownValue.AssignedName(); ok {
		knownValuesByAssignedName[assignedName] = knownValue
	}
}

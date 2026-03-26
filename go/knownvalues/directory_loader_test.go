package knownvalues

import (
	"encoding/json"
	"errors"
	"os"
	"path/filepath"
	"testing"
)

func resetGlobalStateForTesting() {
	customConfigMu.Lock()
	customConfig = nil
	customConfigMu.Unlock()
	configLocked.Store(false)
	KnownValues = newLazyKnownValues(newDefaultKnownValuesStore)
}

func mustSetEmptyGlobalConfig(t *testing.T) {
	t.Helper()
	if err := SetDirectoryConfig(NewDirectoryConfig()); err != nil {
		t.Fatalf("SetDirectoryConfig failed: %v", err)
	}
}

func writeJSONFile(t *testing.T, directory, fileName, contents string) string {
	t.Helper()
	path := filepath.Join(directory, fileName)
	if err := os.WriteFile(path, []byte(contents), 0o600); err != nil {
		t.Fatalf("os.WriteFile(%q) failed: %v", path, err)
	}
	return path
}

func TestParseRegistryJSON(t *testing.T) {
	jsonText := `{
		"ontology": {"name": "test"},
		"entries": [
			{"codepoint": 9999, "name": "testValue", "type": "property"}
		],
		"statistics": {}
	}`

	var registry RegistryFile
	if err := json.Unmarshal([]byte(jsonText), &registry); err != nil {
		t.Fatalf("json.Unmarshal failed: %v", err)
	}
	if got, want := len(registry.Entries), 1; got != want {
		t.Fatalf("entry count mismatch: got %d want %d", got, want)
	}
	if got, want := registry.Entries[0].Codepoint, uint64(9999); got != want {
		t.Fatalf("Codepoint mismatch: got %d want %d", got, want)
	}
	if got, want := registry.Entries[0].Name, "testValue"; got != want {
		t.Fatalf("Name mismatch: got %q want %q", got, want)
	}
}

func TestParseMinimalRegistry(t *testing.T) {
	jsonText := `{"entries": [{"codepoint": 1, "name": "minimal"}]}`

	var registry RegistryFile
	if err := json.Unmarshal([]byte(jsonText), &registry); err != nil {
		t.Fatalf("json.Unmarshal failed: %v", err)
	}
	if got, want := len(registry.Entries), 1; got != want {
		t.Fatalf("entry count mismatch: got %d want %d", got, want)
	}
	if got, want := registry.Entries[0].Codepoint, uint64(1); got != want {
		t.Fatalf("Codepoint mismatch: got %d want %d", got, want)
	}
}

func TestParseFullEntry(t *testing.T) {
	jsonText := `{
		"entries": [{
			"codepoint": 100,
			"name": "fullEntry",
			"type": "class",
			"uri": "https://example.com/vocab#fullEntry",
			"description": "A complete entry with all fields"
		}]
	}`

	var registry RegistryFile
	if err := json.Unmarshal([]byte(jsonText), &registry); err != nil {
		t.Fatalf("json.Unmarshal failed: %v", err)
	}
	entry := registry.Entries[0]
	if got, want := entry.Codepoint, uint64(100); got != want {
		t.Fatalf("Codepoint mismatch: got %d want %d", got, want)
	}
	if got, want := entry.Name, "fullEntry"; got != want {
		t.Fatalf("Name mismatch: got %q want %q", got, want)
	}
	if entry.EntryType == nil || *entry.EntryType != "class" {
		t.Fatalf("EntryType mismatch: got %v", entry.EntryType)
	}
	if entry.URI == nil || *entry.URI != "https://example.com/vocab#fullEntry" {
		t.Fatalf("URI mismatch: got %v", entry.URI)
	}
	if entry.Description == nil || *entry.Description != "A complete entry with all fields" {
		t.Fatalf("Description mismatch: got %v", entry.Description)
	}
}

func TestDirectoryConfigDefault(t *testing.T) {
	config := DefaultOnlyDirectoryConfig()
	paths := config.Paths()
	if got, want := len(paths), 1; got != want {
		t.Fatalf("path count mismatch: got %d want %d", got, want)
	}
	if got := filepath.Base(paths[0]); got != ".known-values" {
		t.Fatalf("default path mismatch: got %q", paths[0])
	}
}

func TestDirectoryConfigCustomPaths(t *testing.T) {
	config := DirectoryConfigWithPaths([]string{"/a", "/b"})
	paths := config.Paths()
	if got, want := len(paths), 2; got != want {
		t.Fatalf("path count mismatch: got %d want %d", got, want)
	}
	if got, want := paths[0], filepath.Clean("/a"); got != want {
		t.Fatalf("first path mismatch: got %q want %q", got, want)
	}
	if got, want := paths[1], filepath.Clean("/b"); got != want {
		t.Fatalf("second path mismatch: got %q want %q", got, want)
	}

	clone := config.Clone()
	if got, want := len(clone.Paths()), 2; got != want {
		t.Fatalf("clone path count mismatch: got %d want %d", got, want)
	}
}

func TestDirectoryConfigWithDefault(t *testing.T) {
	config := DirectoryConfigWithPathsAndDefault([]string{"/custom"})
	paths := config.Paths()
	if got, want := len(paths), 2; got != want {
		t.Fatalf("path count mismatch: got %d want %d", got, want)
	}
	if got, want := paths[0], filepath.Clean("/custom"); got != want {
		t.Fatalf("first path mismatch: got %q want %q", got, want)
	}
	if got := filepath.Base(paths[1]); got != ".known-values" {
		t.Fatalf("default path mismatch: got %q", paths[1])
	}
}

func TestLoadFromNonexistentDirectory(t *testing.T) {
	values, err := LoadFromDirectory("/nonexistent/path/12345")
	if err != nil {
		t.Fatalf("LoadFromDirectory failed: %v", err)
	}
	if got, want := len(values), 0; got != want {
		t.Fatalf("value count mismatch: got %d want %d", got, want)
	}
}

func TestLoadResultMethods(t *testing.T) {
	result := NewLoadResult()
	if got, want := result.ValuesCount(), 0; got != want {
		t.Fatalf("ValuesCount mismatch: got %d want %d", got, want)
	}
	if result.HasErrors() {
		t.Fatalf("HasErrors should be false for empty result")
	}

	result.Values[1] = NewKnownValueWithName(1, "test")
	if got, want := result.ValuesCount(), 1; got != want {
		t.Fatalf("ValuesCount mismatch after insert: got %d want %d", got, want)
	}
}

func TestGlobalRegistryStillWorks(t *testing.T) {
	resetGlobalStateForTesting()
	t.Cleanup(resetGlobalStateForTesting)
	mustSetEmptyGlobalConfig(t)

	store := KnownValues.Get()
	isA, ok := store.KnownValueNamed("isA")
	if !ok || isA.Value() != 1 {
		t.Fatalf("global registry lookup mismatch: got %v ok=%t", isA, ok)
	}
}

func TestLoadFromTempDirectory(t *testing.T) {
	tempDir := t.TempDir()
	writeJSONFile(t, tempDir, "test_registry.json", `{
		"entries": [
			{"codepoint": 99999, "name": "integrationTestValue"}
		]
	}`)

	store := NewKnownValuesStore(IsA, Note)
	count, err := store.LoadFromDirectory(tempDir)
	if err != nil {
		t.Fatalf("LoadFromDirectory failed: %v", err)
	}
	if got, want := count, 1; got != want {
		t.Fatalf("LoadFromDirectory count mismatch: got %d want %d", got, want)
	}
	if loaded, ok := store.KnownValueNamed("integrationTestValue"); !ok || loaded.Value() != 99999 {
		t.Fatalf("loaded value mismatch: got %v ok=%t", loaded, ok)
	}
	if _, ok := store.KnownValueNamed("isA"); !ok {
		t.Fatalf("expected original isA value to remain present")
	}
	if _, ok := store.KnownValueNamed("note"); !ok {
		t.Fatalf("expected original note value to remain present")
	}
}

func TestOverrideHardcodedValue(t *testing.T) {
	tempDir := t.TempDir()
	writeJSONFile(t, tempDir, "override.json", `{
		"entries": [
			{"codepoint": 1, "name": "overriddenIsA"}
		]
	}`)

	store := NewKnownValuesStore(IsA)
	if _, err := store.LoadFromDirectory(tempDir); err != nil {
		t.Fatalf("LoadFromDirectory failed: %v", err)
	}
	if _, ok := store.KnownValueNamed("isA"); ok {
		t.Fatalf("old assigned name should be removed after override")
	}
	if overridden, ok := store.KnownValueNamed("overriddenIsA"); !ok || overridden.Value() != 1 {
		t.Fatalf("override mismatch: got %v ok=%t", overridden, ok)
	}
}

func TestMultipleFilesInDirectory(t *testing.T) {
	tempDir := t.TempDir()
	writeJSONFile(t, tempDir, "registry1.json", `{"entries": [{"codepoint": 10001, "name": "valueOne"}]}`)
	writeJSONFile(t, tempDir, "registry2.json", `{"entries": [{"codepoint": 10002, "name": "valueTwo"}]}`)

	store := NewKnownValuesStore()
	count, err := store.LoadFromDirectory(tempDir)
	if err != nil {
		t.Fatalf("LoadFromDirectory failed: %v", err)
	}
	if got, want := count, 2; got != want {
		t.Fatalf("LoadFromDirectory count mismatch: got %d want %d", got, want)
	}
	if _, ok := store.KnownValueNamed("valueOne"); !ok {
		t.Fatalf("expected valueOne to be present")
	}
	if _, ok := store.KnownValueNamed("valueTwo"); !ok {
		t.Fatalf("expected valueTwo to be present")
	}
}

func TestDirectoryConfigCustomPathsLoadsValuesFromAllDirectories(t *testing.T) {
	tempDir1 := t.TempDir()
	tempDir2 := t.TempDir()

	writeJSONFile(t, tempDir1, "a.json", `{"entries": [{"codepoint": 20001, "name": "fromDirOne"}]}`)
	writeJSONFile(t, tempDir2, "b.json", `{"entries": [{"codepoint": 20002, "name": "fromDirTwo"}]}`)

	config := DirectoryConfigWithPaths([]string{tempDir1, tempDir2})
	store := NewKnownValuesStore()
	result := store.LoadFromConfig(config)

	if got, want := result.ValuesCount(), 2; got != want {
		t.Fatalf("ValuesCount mismatch: got %d want %d", got, want)
	}
	if _, ok := store.KnownValueNamed("fromDirOne"); !ok {
		t.Fatalf("expected fromDirOne to be present")
	}
	if _, ok := store.KnownValueNamed("fromDirTwo"); !ok {
		t.Fatalf("expected fromDirTwo to be present")
	}
}

func TestLaterDirectoryOverridesEarlier(t *testing.T) {
	tempDir1 := t.TempDir()
	tempDir2 := t.TempDir()

	writeJSONFile(t, tempDir1, "first.json", `{"entries": [{"codepoint": 30000, "name": "firstVersion"}]}`)
	writeJSONFile(t, tempDir2, "second.json", `{"entries": [{"codepoint": 30000, "name": "secondVersion"}]}`)

	config := DirectoryConfigWithPaths([]string{tempDir1, tempDir2})
	store := NewKnownValuesStore()
	store.LoadFromConfig(config)

	if value, ok := store.KnownValueNamed("secondVersion"); !ok || value.Value() != 30000 {
		t.Fatalf("secondVersion mismatch: got %v ok=%t", value, ok)
	}
	if _, ok := store.KnownValueNamed("firstVersion"); ok {
		t.Fatalf("firstVersion should have been overridden by later directory")
	}
}

func TestNonexistentDirectoryIsOK(t *testing.T) {
	store := NewKnownValuesStore()
	count, err := store.LoadFromDirectory("/nonexistent/path/12345")
	if err != nil {
		t.Fatalf("LoadFromDirectory failed: %v", err)
	}
	if got, want := count, 0; got != want {
		t.Fatalf("LoadFromDirectory count mismatch: got %d want %d", got, want)
	}
}

func TestInvalidJSONIsError(t *testing.T) {
	tempDir := t.TempDir()
	writeJSONFile(t, tempDir, "invalid.json", "{ this is not valid json }")

	store := NewKnownValuesStore()
	_, err := store.LoadFromDirectory(tempDir)
	if err == nil {
		t.Fatalf("expected invalid JSON to produce an error")
	}
	var loadErr LoadError
	if !errors.As(err, &loadErr) || !loadErr.JSON {
		t.Fatalf("expected JSON LoadError, got %T (%v)", err, err)
	}
}

func TestTolerantLoadingContinuesOnError(t *testing.T) {
	tempDir := t.TempDir()
	writeJSONFile(t, tempDir, "valid.json", `{"entries": [{"codepoint": 40001, "name": "validValue"}]}`)
	writeJSONFile(t, tempDir, "invalid.json", "{ invalid json }")

	config := DirectoryConfigWithPaths([]string{tempDir})
	result := LoadFromConfig(config)

	if _, ok := result.Values[40001]; !ok {
		t.Fatalf("expected validValue to be present in tolerant result")
	}
	if !result.HasErrors() {
		t.Fatalf("expected tolerant loading to record errors")
	}
}

func TestFullRegistryFormat(t *testing.T) {
	tempDir := t.TempDir()
	writeJSONFile(t, tempDir, "full_format.json", `{
		"ontology": {
			"name": "test_registry",
			"source_url": "https://example.com",
			"start_code_point": 50000,
			"processing_strategy": "test"
		},
		"generated": {
			"tool": "test"
		},
		"entries": [
			{
				"codepoint": 50001,
				"name": "fullFormatValue",
				"type": "property",
				"uri": "https://example.com/vocab#fullFormatValue",
				"description": "A value in full format"
			},
			{
				"codepoint": 50002,
				"name": "anotherValue",
				"type": "class"
			}
		],
		"statistics": {
			"total_entries": 2
		}
	}`)

	store := NewKnownValuesStore()
	count, err := store.LoadFromDirectory(tempDir)
	if err != nil {
		t.Fatalf("LoadFromDirectory failed: %v", err)
	}
	if got, want := count, 2; got != want {
		t.Fatalf("LoadFromDirectory count mismatch: got %d want %d", got, want)
	}
	if _, ok := store.KnownValueNamed("fullFormatValue"); !ok {
		t.Fatalf("expected fullFormatValue to be present")
	}
	if _, ok := store.KnownValueNamed("anotherValue"); !ok {
		t.Fatalf("expected anotherValue to be present")
	}
}

func TestLoadResultMethodsOnConfiguredLoad(t *testing.T) {
	tempDir := t.TempDir()
	writeJSONFile(t, tempDir, "test.json", `{
		"entries": [
			{"codepoint": 60001, "name": "resultTest1"},
			{"codepoint": 60002, "name": "resultTest2"}
		]
	}`)

	config := DirectoryConfigWithPaths([]string{tempDir})
	result := LoadFromConfig(config)

	if got, want := result.ValuesCount(), 2; got != want {
		t.Fatalf("ValuesCount mismatch: got %d want %d", got, want)
	}
	if result.HasErrors() {
		t.Fatalf("HasErrors should be false for successful configured load")
	}
	if got, want := len(result.FilesProcessed), 1; got != want {
		t.Fatalf("FilesProcessed length mismatch: got %d want %d", got, want)
	}
	if got, want := len(result.ValuesIter()), 2; got != want {
		t.Fatalf("ValuesIter length mismatch: got %d want %d", got, want)
	}
}

func TestEmptyEntriesArray(t *testing.T) {
	tempDir := t.TempDir()
	writeJSONFile(t, tempDir, "empty.json", `{"entries": []}`)

	store := NewKnownValuesStore()
	count, err := store.LoadFromDirectory(tempDir)
	if err != nil {
		t.Fatalf("LoadFromDirectory failed: %v", err)
	}
	if got, want := count, 0; got != want {
		t.Fatalf("LoadFromDirectory count mismatch: got %d want %d", got, want)
	}
}

func TestNonJSONFilesIgnored(t *testing.T) {
	tempDir := t.TempDir()
	writeJSONFile(t, tempDir, "valid.json", `{"entries": [{"codepoint": 70001, "name": "jsonValue"}]}`)
	if err := os.WriteFile(filepath.Join(tempDir, "readme.txt"), []byte("Some text"), 0o600); err != nil {
		t.Fatalf("os.WriteFile(readme.txt) failed: %v", err)
	}
	if err := os.WriteFile(filepath.Join(tempDir, "data.xml"), []byte("<xml/>"), 0o600); err != nil {
		t.Fatalf("os.WriteFile(data.xml) failed: %v", err)
	}

	store := NewKnownValuesStore()
	count, err := store.LoadFromDirectory(tempDir)
	if err != nil {
		t.Fatalf("LoadFromDirectory failed: %v", err)
	}
	if got, want := count, 1; got != want {
		t.Fatalf("LoadFromDirectory count mismatch: got %d want %d", got, want)
	}
	if _, ok := store.KnownValueNamed("jsonValue"); !ok {
		t.Fatalf("expected jsonValue to be present")
	}
}

func TestSetDirectoryConfigBeforeGlobalAccessLoadsCustomValues(t *testing.T) {
	resetGlobalStateForTesting()
	t.Cleanup(resetGlobalStateForTesting)

	tempDir := t.TempDir()
	writeJSONFile(t, tempDir, "global.json", `{"entries": [{"codepoint": 80001, "name": "globalCustomValue"}]}`)

	if err := SetDirectoryConfig(DirectoryConfigWithPaths([]string{tempDir})); err != nil {
		t.Fatalf("SetDirectoryConfig failed: %v", err)
	}

	store := KnownValues.Get()
	if value, ok := store.KnownValueNamed("globalCustomValue"); !ok || value.Value() != 80001 {
		t.Fatalf("global custom value mismatch: got %v ok=%t", value, ok)
	}
}

func TestSetDirectoryConfigFailsAfterGlobalRegistryInitialization(t *testing.T) {
	resetGlobalStateForTesting()
	t.Cleanup(resetGlobalStateForTesting)
	mustSetEmptyGlobalConfig(t)
	KnownValues.Get()

	err := SetDirectoryConfig(DirectoryConfigWithPaths([]string{"/custom/path"}))
	if err == nil {
		t.Fatalf("expected SetDirectoryConfig to fail after global initialization")
	}
	var configErr ConfigError
	if !errors.As(err, &configErr) {
		t.Fatalf("expected ConfigError, got %T (%v)", err, err)
	}
}

func TestAddSearchPathsUsesDefaultDirectoryWhenNoConfigurationExists(t *testing.T) {
	resetGlobalStateForTesting()
	t.Cleanup(resetGlobalStateForTesting)

	if err := AddSearchPaths([]string{"/custom/path"}); err != nil {
		t.Fatalf("AddSearchPaths failed: %v", err)
	}

	config := getAndLockConfig()
	paths := config.Paths()
	if got, want := len(paths), 2; got != want {
		t.Fatalf("path count mismatch: got %d want %d", got, want)
	}
	if got := filepath.Base(paths[0]); got != ".known-values" {
		t.Fatalf("default path mismatch: got %q", paths[0])
	}
	if got, want := paths[1], filepath.Clean("/custom/path"); got != want {
		t.Fatalf("custom path mismatch: got %q want %q", got, want)
	}
}

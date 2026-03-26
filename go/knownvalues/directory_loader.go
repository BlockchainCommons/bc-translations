package knownvalues

import (
	"encoding/json"
	"errors"
	"fmt"
	"os"
	"path/filepath"
	"slices"
	"sync"
	"sync/atomic"
)

// RegistryEntry is a single entry in a known values JSON registry file.
type RegistryEntry struct {
	Codepoint   uint64  `json:"codepoint"`
	Name        string  `json:"name"`
	EntryType   *string `json:"type,omitempty"`
	URI         *string `json:"uri,omitempty"`
	Description *string `json:"description,omitempty"`
}

// OntologyInfo describes optional registry metadata.
type OntologyInfo struct {
	Name               *string `json:"name,omitempty"`
	SourceURL          *string `json:"source_url,omitempty"`
	StartCodePoint     *uint64 `json:"start_code_point,omitempty"`
	ProcessingStrategy *string `json:"processing_strategy,omitempty"`
}

// RegistryFile is the top-level JSON registry document.
type RegistryFile struct {
	Ontology   *OntologyInfo   `json:"ontology,omitempty"`
	Generated  *GeneratedInfo  `json:"generated,omitempty"`
	Entries    []RegistryEntry `json:"entries"`
	Statistics json.RawMessage `json:"statistics,omitempty"`
}

// GeneratedInfo describes optional generator metadata for a registry file.
type GeneratedInfo struct {
	Tool *string `json:"tool,omitempty"`
}

// LoadError reports either an I/O or JSON parsing failure.
type LoadError struct {
	Path string
	Err  error
	JSON bool
}

// Error returns the user-facing load error message.
func (e LoadError) Error() string {
	if e.JSON {
		return fmt.Sprintf("JSON parse error in %s: %v", e.Path, e.Err)
	}
	return fmt.Sprintf("IO error: %v", e.Err)
}

// Unwrap returns the wrapped underlying error.
func (e LoadError) Unwrap() error {
	return e.Err
}

// LoadResult is the tolerant multi-directory loading result.
type LoadResult struct {
	ValuesMap      map[uint64]KnownValue
	FilesProcessed []string
	Errors         []LoadError
}

// NewLoadResult creates an empty load result with initialized collections.
func NewLoadResult() LoadResult {
	return LoadResult{
		ValuesMap:      make(map[uint64]KnownValue),
		FilesProcessed: make([]string, 0),
		Errors:         make([]LoadError, 0),
	}
}

// ValuesCount returns the number of unique loaded values.
func (r LoadResult) ValuesCount() int {
	return len(r.ValuesMap)
}

// Values returns the loaded values in ascending raw-value order.
func (r LoadResult) Values() []KnownValue {
	return knownValuesFromMap(r.ValuesMap)
}

// HasErrors reports whether any non-fatal errors were recorded.
func (r LoadResult) HasErrors() bool {
	return len(r.Errors) != 0
}

// DirectoryConfig configures directory loading.
type DirectoryConfig struct {
	paths []string
}

// NewDirectoryConfig creates an empty configuration with no search paths.
func NewDirectoryConfig() DirectoryConfig {
	return DirectoryConfig{paths: make([]string, 0)}
}

// DefaultOnlyDirectoryConfig creates a configuration with only the default
// `~/.known-values` path.
func DefaultOnlyDirectoryConfig() DirectoryConfig {
	return DirectoryConfig{paths: []string{DefaultDirectory()}}
}

// DirectoryConfigWithPaths creates a configuration with only the provided
// custom paths.
func DirectoryConfigWithPaths(paths []string) DirectoryConfig {
	config := NewDirectoryConfig()
	for _, path := range paths {
		config.AddPath(path)
	}
	return config
}

// DirectoryConfigWithPathsAndDefault creates a configuration with the
// provided custom paths followed by the default directory.
func DirectoryConfigWithPathsAndDefault(paths []string) DirectoryConfig {
	config := DirectoryConfigWithPaths(paths)
	config.AddPath(DefaultDirectory())
	return config
}

// DefaultDirectory returns the default directory used for JSON registries.
func DefaultDirectory() string {
	home, err := os.UserHomeDir()
	if err != nil || home == "" {
		home = "."
	}
	return filepath.Join(home, ".known-values")
}

// Clone returns a copy of the directory configuration.
func (c DirectoryConfig) Clone() DirectoryConfig {
	return DirectoryConfig{paths: c.Paths()}
}

// Paths returns the configured search paths.
func (c DirectoryConfig) Paths() []string {
	return append([]string(nil), c.paths...)
}

// AddPath appends a path to the configuration.
func (c *DirectoryConfig) AddPath(path string) {
	c.paths = append(c.paths, path)
}

// LoadFromDirectory strictly loads all JSON files in a single directory.
func LoadFromDirectory(path string) ([]KnownValue, error) {
	info, err := os.Stat(path)
	if err != nil {
		if os.IsNotExist(err) {
			return []KnownValue{}, nil
		}
		return nil, LoadError{Err: err}
	}
	if !info.IsDir() {
		return []KnownValue{}, nil
	}

	entries, err := os.ReadDir(path)
	if err != nil {
		return nil, LoadError{Err: err}
	}

	values := make([]KnownValue, 0)
	for _, entry := range entries {
		if entry.IsDir() || filepath.Ext(entry.Name()) != ".json" {
			continue
		}
		filePath := filepath.Join(path, entry.Name())
		fileValues, err := loadSingleFile(filePath)
		if err != nil {
			return nil, err
		}
		values = append(values, fileValues...)
	}

	return values, nil
}

// LoadFromConfig loads known values from all configured directories, keeping
// later-directory overrides and recording non-fatal per-file failures.
func LoadFromConfig(config DirectoryConfig) LoadResult {
	result := NewLoadResult()

	for _, dirPath := range config.Paths() {
		values, errors, err := loadFromDirectoryTolerant(dirPath)
		if err != nil {
			result.Errors = append(result.Errors, ensureLoadError(dirPath, err))
			continue
		}

		for _, value := range values {
			result.ValuesMap[value.Value()] = value
		}
		result.Errors = append(result.Errors, errors...)
		result.FilesProcessed = append(result.FilesProcessed, dirPath)
	}

	return result
}

func loadFromDirectoryTolerant(path string) ([]KnownValue, []LoadError, error) {
	info, err := os.Stat(path)
	if err != nil {
		if os.IsNotExist(err) {
			return []KnownValue{}, nil, nil
		}
		return nil, nil, LoadError{Err: err}
	}
	if !info.IsDir() {
		return []KnownValue{}, nil, nil
	}

	entries, err := os.ReadDir(path)
	if err != nil {
		return nil, nil, LoadError{Err: err}
	}

	values := make([]KnownValue, 0)
	var loadErrors []LoadError
	for _, entry := range entries {
		if entry.IsDir() || filepath.Ext(entry.Name()) != ".json" {
			continue
		}
		filePath := filepath.Join(path, entry.Name())
		fileValues, err := loadSingleFile(filePath)
		if err != nil {
			loadErrors = append(loadErrors, ensureLoadError(filePath, err))
			continue
		}
		values = append(values, fileValues...)
	}

	return values, loadErrors, nil
}

func loadSingleFile(path string) ([]KnownValue, error) {
	content, err := os.ReadFile(path)
	if err != nil {
		return nil, LoadError{Err: err}
	}

	var registry RegistryFile
	if err := json.Unmarshal(content, &registry); err != nil {
		return nil, LoadError{Path: path, Err: err, JSON: true}
	}

	values := make([]KnownValue, 0, len(registry.Entries))
	for _, entry := range registry.Entries {
		values = append(values, NewKnownValueWithName(entry.Codepoint, entry.Name))
	}
	return values, nil
}

func knownValuesFromMap(values map[uint64]KnownValue) []KnownValue {
	if len(values) == 0 {
		return []KnownValue{}
	}

	keys := make([]uint64, 0, len(values))
	for key := range values {
		keys = append(keys, key)
	}
	slices.SortFunc(keys, func(a, b uint64) int {
		if a < b {
			return -1
		}
		if a > b {
			return 1
		}
		return 0
	})

	ordered := make([]KnownValue, 0, len(keys))
	for _, key := range keys {
		ordered = append(ordered, values[key])
	}
	return ordered
}

func ensureLoadError(path string, err error) LoadError {
	var loadErr LoadError
	if errors.As(err, &loadErr) {
		return loadErr
	}
	return LoadError{Path: path, Err: err}
}

var (
	customConfigMu sync.Mutex
	customConfig   *DirectoryConfig
	configLocked   atomic.Bool
)

// ConfigError reports an attempt to mutate global directory configuration
// after the lazy global registry has already been accessed.
type ConfigError struct{}

// Error returns the configuration error message.
func (ConfigError) Error() string {
	return "Cannot modify directory configuration after KNOWN_VALUES has been accessed"
}

// SetDirectoryConfig replaces the global directory-loading configuration.
func SetDirectoryConfig(config DirectoryConfig) error {
	if configLocked.Load() {
		return ConfigError{}
	}

	customConfigMu.Lock()
	defer customConfigMu.Unlock()

	if configLocked.Load() {
		return ConfigError{}
	}

	copied := config.Clone()
	customConfig = &copied
	return nil
}

// AddSearchPaths appends search paths to the global configuration, creating a
// default-only configuration first if none exists yet.
func AddSearchPaths(paths []string) error {
	if configLocked.Load() {
		return ConfigError{}
	}

	customConfigMu.Lock()
	defer customConfigMu.Unlock()

	if configLocked.Load() {
		return ConfigError{}
	}

	if customConfig == nil {
		config := DefaultOnlyDirectoryConfig()
		customConfig = &config
	}
	for _, path := range paths {
		customConfig.AddPath(path)
	}

	return nil
}

func getAndLockConfig() DirectoryConfig {
	configLocked.Store(true)

	customConfigMu.Lock()
	defer customConfigMu.Unlock()

	if customConfig == nil {
		return DefaultOnlyDirectoryConfig()
	}

	config := customConfig.Clone()
	customConfig = nil
	return config
}

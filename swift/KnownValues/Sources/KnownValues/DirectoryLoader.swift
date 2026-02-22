import Foundation

// MARK: - Registry JSON Types (internal)

/// A single entry in a known values JSON registry file.
struct RegistryEntry: Sendable, Decodable {
    let codepoint: UInt64
    let name: String
    let entryType: String?
    let uri: String?
    let description: String?

    private enum CodingKeys: String, CodingKey {
        case codepoint
        case name
        case entryType = "type"
        case uri
        case description
    }
}

/// Metadata about the ontology or registry source.
struct OntologyInfo: Sendable, Decodable {
    let name: String?
    let sourceUrl: String?
    let startCodePoint: UInt64?
    let processingStrategy: String?

    private enum CodingKeys: String, CodingKey {
        case name
        case sourceUrl = "source_url"
        case startCodePoint = "start_code_point"
        case processingStrategy = "processing_strategy"
    }
}

/// Information about how a registry file was generated.
struct GeneratedInfo: Sendable, Decodable {
    let tool: String?
}

/// Root structure of a known values JSON registry file.
struct RegistryFile: Sendable, Decodable {
    let ontology: OntologyInfo?
    let generated: GeneratedInfo?
    let entries: [RegistryEntry]

    private enum CodingKeys: String, CodingKey {
        case ontology
        case generated
        case entries
    }
}

// MARK: - Errors

/// Errors that can occur when loading known values from directories.
public enum LoadError: Error, Sendable {
    /// An I/O error occurred while reading files.
    case io(URL, any Error)
    /// A JSON parsing error occurred.
    case json(file: URL, underlying: any Error)
}

extension LoadError: LocalizedError {
    public var errorDescription: String? {
        switch self {
        case .io(let url, let error):
            return "IO error at \(url.path): \(error.localizedDescription)"
        case .json(let file, let error):
            return "JSON parse error in \(file.path): \(error.localizedDescription)"
        }
    }
}

/// Error returned when configuration cannot be modified.
public enum ConfigError: Error, Sendable, Equatable {
    /// Configuration was attempted after the global registry was initialized.
    case alreadyInitialized
}

extension ConfigError: LocalizedError {
    public var errorDescription: String? {
        switch self {
        case .alreadyInitialized:
            return "Cannot modify directory configuration after KnownValuesStore.shared has been accessed"
        }
    }
}

// MARK: - File Error

/// A non-fatal error encountered while loading a registry file.
public struct FileError: Sendable {
    /// The file that caused the error.
    public let file: URL
    /// The error encountered.
    public let error: LoadError
}

// MARK: - Load Result

/// Result of a directory loading operation.
public struct LoadResult: Sendable {
    /// Known values loaded, keyed by codepoint.
    public fileprivate(set) var values: [UInt64: KnownValue]
    /// Files that were successfully processed.
    public fileprivate(set) var filesProcessed: [URL]
    /// Non-fatal errors encountered during loading.
    public fileprivate(set) var errors: [FileError]

    /// Creates an empty LoadResult.
    init() {
        self.values = [:]
        self.filesProcessed = []
        self.errors = []
    }

    /// Returns the number of unique values loaded.
    public var valuesCount: Int { values.count }

    /// Returns true if any errors occurred during loading.
    public var hasErrors: Bool { !errors.isEmpty }
}

// MARK: - Directory Configuration

/// Configuration for loading known values from directories.
///
/// Specifies which directories to search for JSON registry files.
/// Directories are processed in order, with values from later directories
/// overriding values from earlier directories when codepoints collide.
public struct DirectoryConfig: Sendable {
    /// Search paths in priority order (later paths override earlier).
    public private(set) var paths: [URL]

    /// Creates a new empty configuration with no search paths.
    public init() {
        self.paths = []
    }

    /// Creates configuration with only the default directory
    /// (`~/.known-values/`).
    public static func defaultOnly() -> DirectoryConfig {
        DirectoryConfig(paths: [defaultDirectory()])
    }

    /// Creates configuration with custom paths (processed in order).
    ///
    /// Later paths in the list take precedence over earlier paths when
    /// values have the same codepoint.
    public init(paths: [URL]) {
        self.paths = paths
    }

    /// Creates configuration with custom paths followed by the default
    /// directory.
    ///
    /// The default directory (`~/.known-values/`) is appended to the list,
    /// so its values will override values from the custom paths.
    public static func withPathsAndDefault(_ paths: [URL]) -> DirectoryConfig {
        var allPaths = paths
        allPaths.append(defaultDirectory())
        return DirectoryConfig(paths: allPaths)
    }

    /// Returns the default directory: `~/.known-values/`
    ///
    /// Falls back to `./.known-values/` if the home directory cannot be
    /// determined.
    public static func defaultDirectory() -> URL {
        FileManager.default.homeDirectoryForCurrentUser
            .appendingPathComponent(".known-values")
    }

    /// Adds a path to the configuration.
    ///
    /// The new path will be processed after existing paths, so its values
    /// will override values from earlier paths.
    public mutating func addPath(_ path: URL) {
        paths.append(path)
    }

    /// Sets custom directory configuration for known values loading.
    ///
    /// Must be called **before** the first access to
    /// ``KnownValuesStore/shared``. Once `shared` is accessed, the
    /// configuration is locked and cannot be changed.
    public static func setDirectoryConfig(
        _ config: DirectoryConfig
    ) throws {
        try ConfigState.shared.setConfig(config)
    }

    /// Adds additional search paths to the directory configuration.
    ///
    /// Must be called **before** the first access to
    /// ``KnownValuesStore/shared``. Paths are added after any existing paths,
    /// so they will take precedence.
    ///
    /// If no configuration has been set, this creates a new configuration
    /// with the default directory and appends the new paths.
    public static func addSearchPaths(_ paths: [URL]) throws {
        try ConfigState.shared.addPaths(paths)
    }
}

// MARK: - Config State Manager

/// Thread-safe manager for directory configuration global state.
final class ConfigState: @unchecked Sendable {
    private let lock = NSLock()
    private var config: DirectoryConfig?
    private var locked = false

    static let shared = ConfigState()

    func setConfig(_ config: DirectoryConfig) throws {
        lock.lock()
        defer { lock.unlock() }
        guard !locked else { throw ConfigError.alreadyInitialized }
        self.config = config
    }

    func addPaths(_ paths: [URL]) throws {
        lock.lock()
        defer { lock.unlock() }
        guard !locked else { throw ConfigError.alreadyInitialized }
        if config == nil { config = .defaultOnly() }
        for path in paths {
            config!.addPath(path)
        }
    }

    func getAndLock() -> DirectoryConfig {
        lock.lock()
        defer { lock.unlock() }
        locked = true
        return config ?? .defaultOnly()
    }
}

// MARK: - Directory Loader

/// Internal loader implementation for known values from JSON registry files.
enum DirectoryLoader {
    /// Loads all JSON registry files from a single directory.
    ///
    /// Returns an empty array if the directory doesn't exist.
    static func loadFromDirectory(at path: URL) throws -> [KnownValue] {
        let fm = FileManager.default
        var isDir: ObjCBool = false

        guard fm.fileExists(atPath: path.path, isDirectory: &isDir),
              isDir.boolValue else {
            return []
        }

        let contents: [URL]
        do {
            contents = try fm.contentsOfDirectory(
                at: path,
                includingPropertiesForKeys: nil
            )
        } catch {
            throw LoadError.io(path, error)
        }

        var values = [KnownValue]()
        for fileURL in contents {
            guard fileURL.pathExtension == "json" else { continue }
            let data: Data
            do {
                data = try Data(contentsOf: fileURL)
            } catch {
                throw LoadError.io(fileURL, error)
            }
            let registry: RegistryFile
            do {
                registry = try JSONDecoder().decode(
                    RegistryFile.self,
                    from: data
                )
            } catch {
                throw LoadError.json(file: fileURL, underlying: error)
            }
            for entry in registry.entries {
                values.append(
                    KnownValue(value: entry.codepoint, name: entry.name)
                )
            }
        }
        return values
    }

    /// Loads known values from all directories in the given configuration.
    ///
    /// Fault-tolerant: continues processing even if some files fail to parse.
    static func loadFromConfig(_ config: DirectoryConfig) -> LoadResult {
        var result = LoadResult()

        for dirPath in config.paths {
            let outcome = loadFromDirectoryTolerant(at: dirPath)
            switch outcome {
            case .success(let (values, errors)):
                for value in values {
                    result.values[value.value] = value
                }
                result.errors.append(contentsOf: errors)
                result.filesProcessed.append(dirPath)
            case .failure(let error):
                result.errors.append(FileError(file: dirPath, error: error))
            }
        }
        return result
    }

    /// Loads from a directory with tolerance for individual file failures.
    private static func loadFromDirectoryTolerant(
        at path: URL
    ) -> Result<([KnownValue], [FileError]), LoadError> {
        let fm = FileManager.default
        var isDir: ObjCBool = false

        guard fm.fileExists(atPath: path.path, isDirectory: &isDir),
              isDir.boolValue else {
            return .success(([], []))
        }

        let contents: [URL]
        do {
            contents = try fm.contentsOfDirectory(
                at: path,
                includingPropertiesForKeys: nil
            )
        } catch {
            return .failure(.io(path, error))
        }

        var values = [KnownValue]()
        var errors = [FileError]()

        for fileURL in contents {
            guard fileURL.pathExtension == "json" else { continue }
            switch loadSingleFile(at: fileURL) {
            case .success(let fileValues):
                values.append(contentsOf: fileValues)
            case .failure(let error):
                errors.append(FileError(file: fileURL, error: error))
            }
        }
        return .success((values, errors))
    }

    /// Loads known values from a single JSON file.
    private static func loadSingleFile(
        at path: URL
    ) -> Result<[KnownValue], LoadError> {
        let data: Data
        do {
            data = try Data(contentsOf: path)
        } catch {
            return .failure(.io(path, error))
        }
        let registry: RegistryFile
        do {
            registry = try JSONDecoder().decode(
                RegistryFile.self,
                from: data
            )
        } catch {
            return .failure(.json(file: path, underlying: error))
        }
        let values = registry.entries.map {
            KnownValue(value: $0.codepoint, name: $0.name)
        }
        return .success(values)
    }
}

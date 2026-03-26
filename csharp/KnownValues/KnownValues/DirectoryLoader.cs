using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlockchainCommons.KnownValues;

/// <summary>
/// A single entry in a known values JSON registry file.
/// </summary>
public sealed class RegistryEntry
{
    /// <summary>
    /// The unique numeric identifier for this known value.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("codepoint")]
    public ulong Codepoint { get; init; }

    /// <summary>
    /// The canonical string name for this known value.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The type of entry, such as <c>property</c>, <c>class</c>, or
    /// <c>value</c>.
    /// </summary>
    [JsonPropertyName("type")]
    public string? EntryType { get; init; }

    /// <summary>
    /// An optional URI reference for this known value.
    /// </summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; init; }

    /// <summary>
    /// An optional human-readable description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

/// <summary>
/// Metadata about the ontology or registry source.
/// </summary>
public sealed class OntologyInfo
{
    /// <summary>
    /// The name of this registry or ontology.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// The source URL for this registry.
    /// </summary>
    [JsonPropertyName("source_url")]
    public string? SourceUrl { get; init; }

    /// <summary>
    /// The starting codepoint for entries in this registry.
    /// </summary>
    [JsonPropertyName("start_code_point")]
    public ulong? StartCodePoint { get; init; }

    /// <summary>
    /// The processing strategy used to generate this registry.
    /// </summary>
    [JsonPropertyName("processing_strategy")]
    public string? ProcessingStrategy { get; init; }
}

/// <summary>
/// Root structure of a known values JSON registry file.
/// </summary>
public sealed class RegistryFile
{
    /// <summary>
    /// Metadata about this registry.
    /// </summary>
    [JsonPropertyName("ontology")]
    public OntologyInfo? Ontology { get; init; }

    /// <summary>
    /// Information about how this file was generated.
    /// </summary>
    [JsonPropertyName("generated")]
    public GeneratedInfo? Generated { get; init; }

    /// <summary>
    /// The known value entries in this registry.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("entries")]
    public List<RegistryEntry> Entries { get; init; } = [];

    /// <summary>
    /// Statistics about this registry.
    /// </summary>
    [JsonPropertyName("statistics")]
    public JsonElement? Statistics { get; init; }
}

/// <summary>
/// Information about how a registry file was generated.
/// </summary>
public sealed class GeneratedInfo
{
    /// <summary>
    /// The tool used to generate this registry.
    /// </summary>
    [JsonPropertyName("tool")]
    public string? Tool { get; init; }
}

/// <summary>
/// Errors that can occur when loading known values from directories.
/// </summary>
public sealed class LoadError : Exception
{
    private LoadError(
        string message,
        Exception innerException,
        string? filePath = null)
        : base(message, innerException)
    {
        FilePath = filePath;
    }

    /// <summary>
    /// The file path associated with a JSON parse error, if present.
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// Creates an I/O-flavored load error.
    /// </summary>
    public static LoadError FromIo(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new LoadError($"IO error: {exception.Message}", exception);
    }

    /// <summary>
    /// Creates a JSON parse load error for the given file.
    /// </summary>
    public static LoadError FromJson(string filePath, JsonException exception)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(exception);

        return new LoadError(
            $"JSON parse error in {filePath}: {exception.Message}",
            exception,
            filePath);
    }
}

/// <summary>
/// A non-fatal error encountered during directory loading, associating a file
/// or directory path with its <see cref="LoadError"/>.
/// </summary>
public sealed class LoadErrorEntry
{
    /// <summary>
    /// Creates a new <see cref="LoadErrorEntry"/>.
    /// </summary>
    public LoadErrorEntry(string path, LoadError error)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(error);

        Path = path;
        Error = error;
    }

    /// <summary>
    /// The file or directory path that caused the error.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// The error that occurred.
    /// </summary>
    public LoadError Error { get; }
}

/// <summary>
/// Result of a directory-loading operation.
/// </summary>
public sealed class LoadResult
{
    internal Dictionary<ulong, KnownValue> ValuesByCodepoint { get; } = new();
    internal List<string> ProcessedFiles { get; } = [];
    internal List<LoadErrorEntry> ErrorEntries { get; } = [];

    /// <summary>
    /// Known values loaded, keyed by codepoint.
    /// </summary>
    public IReadOnlyDictionary<ulong, KnownValue> Values => ValuesByCodepoint;

    /// <summary>
    /// Directories that were successfully processed.
    /// </summary>
    public IReadOnlyList<string> FilesProcessed => ProcessedFiles;

    /// <summary>
    /// Non-fatal errors encountered during loading.
    /// </summary>
    public IReadOnlyList<LoadErrorEntry> Errors => ErrorEntries;

    /// <summary>
    /// The number of unique values loaded.
    /// </summary>
    public int Count => ValuesByCodepoint.Count;

    /// <summary>
    /// Returns the loaded known values.
    /// </summary>
    public IEnumerable<KnownValue> GetValues() => ValuesByCodepoint.Values;

    /// <summary>
    /// <c>true</c> if any errors occurred during loading.
    /// </summary>
    public bool HasErrors => ErrorEntries.Count != 0;
}

/// <summary>
/// Configuration for loading known values from directories.
/// </summary>
public sealed class DirectoryConfig : IEquatable<DirectoryConfig>
{
    private readonly List<string> _paths;

    /// <summary>
    /// Creates a new empty configuration.
    /// </summary>
    public DirectoryConfig()
    {
        _paths = [];
    }

    private DirectoryConfig(IEnumerable<string> paths)
    {
        _paths = paths.Select(Path.GetFullPath).ToList();
    }

    /// <summary>
    /// Creates configuration with only the default directory.
    /// </summary>
    public static DirectoryConfig DefaultOnly() =>
        new([DefaultDirectory()]);

    /// <summary>
    /// Creates configuration with only the given custom paths.
    /// </summary>
    public static DirectoryConfig WithPaths(IEnumerable<string> paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        return new DirectoryConfig(paths);
    }

    /// <summary>
    /// Creates configuration with the given custom paths followed by the
    /// default directory.
    /// </summary>
    public static DirectoryConfig WithPathsAndDefault(IEnumerable<string> paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var allPaths = paths.ToList();
        allPaths.Add(DefaultDirectory());
        return new DirectoryConfig(allPaths);
    }

    /// <summary>
    /// Returns the default directory: <c>~/.known-values/</c>.
    /// </summary>
    public static string DefaultDirectory()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(home))
        {
            home = ".";
        }

        return Path.GetFullPath(Path.Combine(home, ".known-values"));
    }

    /// <summary>
    /// The configured search paths.
    /// </summary>
    public IReadOnlyList<string> Paths => _paths;

    /// <summary>
    /// Adds a path to the configuration.
    /// </summary>
    public void AddPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        _paths.Add(Path.GetFullPath(path));
    }

    /// <inheritdoc />
    public bool Equals(DirectoryConfig? other)
    {
        return other is not null
            && _paths.SequenceEqual(other._paths, StringComparer.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is DirectoryConfig other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var path in _paths)
        {
            hash.Add(path, StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Creates a copy of this <see cref="DirectoryConfig"/>.
    /// </summary>
    public DirectoryConfig Clone() => new DirectoryConfig(_paths);
}

/// <summary>
/// Error returned when the global directory configuration cannot be modified.
/// </summary>
public sealed class ConfigError : Exception, IEquatable<ConfigError>
{
    private const string AlreadyInitializedMessage =
        "Cannot modify directory configuration after KNOWN_VALUES has been accessed";

    /// <summary>
    /// Creates a new <see cref="ConfigError"/> representing an attempt to
    /// modify the configuration after the global registry was initialized.
    /// </summary>
    public ConfigError()
        : base(AlreadyInitializedMessage)
    {
    }

    /// <inheritdoc />
    public bool Equals(ConfigError? other) =>
        other is not null
        && string.Equals(Message, other.Message, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is ConfigError other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(Message);
}

/// <summary>
/// Directory-based loading of known values from JSON registry files.
/// </summary>
public static class DirectoryLoader
{
    private static readonly object ConfigSync = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
    };

    private static DirectoryConfig? _customConfig;
    private static bool _configLocked;

    /// <summary>
    /// Loads all JSON registry files from a single directory.
    /// </summary>
    public static List<KnownValue> LoadFromDirectory(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var fullPath = Path.GetFullPath(path);
        var values = new List<KnownValue>();

        if (!Directory.Exists(fullPath))
        {
            return values;
        }

        try
        {
            foreach (var filePath in EnumerateJsonFiles(fullPath))
            {
                var content = ReadAllText(filePath);
                var registry = DeserializeRegistry(content, filePath);

                foreach (var entry in registry.Entries)
                {
                    values.Add(KnownValue.NewWithName(entry.Codepoint, entry.Name));
                }
            }
        }
        catch (IOException exception)
        {
            throw LoadError.FromIo(exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw LoadError.FromIo(exception);
        }

        return values;
    }

    /// <summary>
    /// Loads known values from all directories in the given configuration.
    /// </summary>
    public static LoadResult LoadFromConfig(DirectoryConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var result = new LoadResult();

        foreach (var dirPath in config.Paths)
        {
            var normalizedPath = Path.GetFullPath(dirPath);

            try
            {
                var (values, errors) = LoadFromDirectoryTolerant(normalizedPath);

                foreach (var value in values)
                {
                    result.ValuesByCodepoint[value.Value] = value;
                }

                if (errors.Count != 0)
                {
                    result.ErrorEntries.AddRange(errors);
                }

                result.ProcessedFiles.Add(normalizedPath);
            }
            catch (IOException exception)
            {
                result.ErrorEntries.Add(new LoadErrorEntry(normalizedPath, LoadError.FromIo(exception)));
            }
            catch (UnauthorizedAccessException exception)
            {
                result.ErrorEntries.Add(new LoadErrorEntry(normalizedPath, LoadError.FromIo(exception)));
            }
        }

        return result;
    }

    /// <summary>
    /// Sets custom directory configuration for known values loading.
    /// </summary>
    public static void SetDirectoryConfig(DirectoryConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        lock (ConfigSync)
        {
            if (_configLocked)
            {
                throw new ConfigError();
            }

            _customConfig = config.Clone();
        }
    }

    /// <summary>
    /// Adds additional search paths to the directory configuration.
    /// </summary>
    public static void AddSearchPaths(IEnumerable<string> paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        lock (ConfigSync)
        {
            if (_configLocked)
            {
                throw new ConfigError();
            }

            _customConfig ??= DirectoryConfig.DefaultOnly();

            foreach (var path in paths)
            {
                _customConfig.AddPath(path);
            }
        }
    }

    private static (List<KnownValue> Values, List<LoadErrorEntry> Errors)
        LoadFromDirectoryTolerant(string path)
    {
        var values = new List<KnownValue>();
        var errors = new List<LoadErrorEntry>();

        if (!Directory.Exists(path))
        {
            return (values, errors);
        }

        foreach (var filePath in EnumerateJsonFiles(path))
        {
            try
            {
                values.AddRange(LoadSingleFile(filePath));
            }
            catch (LoadError exception)
            {
                errors.Add(new LoadErrorEntry(filePath, exception));
            }
        }

        return (values, errors);
    }

    private static List<KnownValue> LoadSingleFile(string path)
    {
        var content = ReadAllText(path);
        var registry = DeserializeRegistry(content, path);

        return registry.Entries
            .Select(entry => KnownValue.NewWithName(entry.Codepoint, entry.Name))
            .ToList();
    }

    private static IEnumerable<string> EnumerateJsonFiles(string path)
    {
        return Directory.EnumerateFiles(path)
            .Where(filePath => string.Equals(
                Path.GetExtension(filePath),
                ".json",
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(filePath => filePath, StringComparer.Ordinal);
    }

    private static string ReadAllText(string filePath)
    {
        try
        {
            return File.ReadAllText(filePath);
        }
        catch (IOException exception)
        {
            throw LoadError.FromIo(exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw LoadError.FromIo(exception);
        }
    }

    private static RegistryFile DeserializeRegistry(string content, string filePath)
    {
        try
        {
            return JsonSerializer.Deserialize<RegistryFile>(content, JsonOptions)
                ?? throw new JsonException("Registry file deserialized to null.");
        }
        catch (JsonException exception)
        {
            throw LoadError.FromJson(filePath, exception);
        }
    }

    internal static DirectoryConfig GetAndLockConfig()
    {
        lock (ConfigSync)
        {
            _configLocked = true;
            return (_customConfig ?? DirectoryConfig.DefaultOnly()).Clone();
        }
    }

    internal static void ResetForTesting()
    {
        lock (ConfigSync)
        {
            _customConfig = null;
            _configLocked = false;
        }
    }
}

internal static class KnownValuesTestHooks
{
    internal static void ResetGlobalState()
    {
        DirectoryLoader.ResetForTesting();
        KnownValuesRegistry.ResetForTesting();
    }
}

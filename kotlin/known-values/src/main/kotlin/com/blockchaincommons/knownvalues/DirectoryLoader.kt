package com.blockchaincommons.knownvalues

import com.fasterxml.jackson.annotation.JsonIgnoreProperties
import com.fasterxml.jackson.annotation.JsonProperty
import com.fasterxml.jackson.module.kotlin.jacksonObjectMapper
import com.fasterxml.jackson.module.kotlin.readValue
import java.io.IOException
import java.math.BigInteger
import java.nio.file.Files
import java.nio.file.Path
import java.util.concurrent.atomic.AtomicBoolean

/** A single entry in a known-values registry JSON file. */
data class RegistryEntry(
    val codepoint: ULong,
    val name: String,
    val entryType: String? = null,
    val uri: String? = null,
    val description: String? = null,
)

/** Metadata about the source ontology/registry. */
data class OntologyInfo(
    val name: String? = null,
    val sourceUrl: String? = null,
    val startCodePoint: ULong? = null,
    val processingStrategy: String? = null,
)

/** Metadata about the registry generation process. */
data class GeneratedInfo(
    val tool: String? = null,
)

/** Root structure of a known-values registry JSON file. */
data class RegistryFile(
    val ontology: OntologyInfo? = null,
    val generated: GeneratedInfo? = null,
    val entries: List<RegistryEntry>,
    val statistics: Any? = null,
) {
    companion object {
        /** Parses a registry from JSON text. */
        fun fromJson(json: String): RegistryFile = parseRegistryFile(json)
    }
}

/** Errors that can occur while loading known values from files/directories. */
sealed class LoadError(message: String, cause: Throwable? = null) :
    Exception(message, cause) {
    class Io(val ioError: IOException) :
        LoadError("IO error: $ioError", ioError)

    class Json(val file: Path, val error: Exception) :
        LoadError(
            "JSON parse error in ${file.toAbsolutePath()}: ${error.message}",
            error,
        )
}

/** Result of tolerant directory-loading operation. */
class LoadResult(
    val values: MutableMap<ULong, KnownValue> = linkedMapOf(),
    val filesProcessed: MutableList<Path> = mutableListOf(),
    val errors: MutableList<Pair<Path, LoadError>> = mutableListOf(),
) {
    fun valuesCount(): Int = values.size

    fun valuesIter(): Iterable<KnownValue> = values.values

    fun intoValues(): Iterable<KnownValue> = values.values.toList()

    fun hasErrors(): Boolean = errors.isNotEmpty()
}

/** Configuration for searching directories that contain registry JSON files. */
class DirectoryConfig private constructor(
    private val configuredPaths: MutableList<Path>,
) {
    companion object {
        /** Creates an empty configuration. */
        fun new(): DirectoryConfig = DirectoryConfig(mutableListOf())

        /** Creates a configuration containing only the default directory. */
        fun defaultOnly(): DirectoryConfig =
            DirectoryConfig(mutableListOf(defaultDirectory()))

        /** Creates a configuration with custom paths in processing order. */
        fun withPaths(paths: List<Path>): DirectoryConfig =
            DirectoryConfig(paths.toMutableList())

        /**
         * Creates a configuration with custom paths followed by the default
         * directory.
         */
        fun withPathsAndDefault(paths: List<Path>): DirectoryConfig =
            DirectoryConfig(paths.toMutableList().apply { add(defaultDirectory()) })

        /** Returns the default search directory (`~/.known-values`). */
        fun defaultDirectory(): Path {
            val home = System.getProperty("user.home")?.takeIf { it.isNotBlank() }
                ?: "."
            return Path.of(home).resolve(".known-values")
        }
    }

    /** Returns configured paths in processing order. */
    fun paths(): List<Path> = configuredPaths.toList()

    /** Appends a path so it takes precedence over earlier paths. */
    fun addPath(path: Path) {
        configuredPaths.add(path)
    }

    internal fun copyConfig(): DirectoryConfig =
        DirectoryConfig(configuredPaths.toMutableList())
}

/** Errors thrown when mutating global directory configuration is disallowed. */
sealed class ConfigError(message: String) : Exception(message) {
    class AlreadyInitialized :
        ConfigError(
            "Cannot modify directory configuration after KNOWN_VALUES has been accessed",
        )
}

/**
 * Loads known values from all `.json` files in a single directory.
 *
 * Returns an empty list when the directory does not exist.
 */
@Throws(LoadError::class)
fun loadFromDirectory(path: Path): List<KnownValue> {
    val values = mutableListOf<KnownValue>()

    if (!Files.exists(path) || !Files.isDirectory(path)) {
        return values
    }

    try {
        Files.newDirectoryStream(path).use { entries ->
            for (entry in entries) {
                if (isJsonFile(entry)) {
                    val content = Files.readString(entry)
                    val registry = try {
                        parseRegistryFile(content)
                    } catch (e: Exception) {
                        throw LoadError.Json(entry, e)
                    }
                    for (registryEntry in registry.entries) {
                        values.add(
                            KnownValue.newWithName(
                                registryEntry.codepoint,
                                registryEntry.name,
                            ),
                        )
                    }
                }
            }
        }
    } catch (error: LoadError) {
        throw error
    } catch (error: IOException) {
        throw LoadError.Io(error)
    }

    return values
}

/**
 * Loads known values from configured directories, collecting file-level parse
 * errors while continuing to process other files.
 */
fun loadFromConfig(config: DirectoryConfig): LoadResult {
    val result = LoadResult()

    for (dirPath in config.paths()) {
        try {
            val (values, errors) = loadFromDirectoryTolerant(dirPath)
            for (value in values) {
                result.values[value.value()] = value
            }
            if (errors.isNotEmpty()) {
                result.errors.addAll(errors)
            }
            result.filesProcessed.add(dirPath)
        } catch (error: LoadError) {
            result.errors.add(dirPath to error)
        }
    }

    return result
}

/** Sets a custom global directory configuration before `KNOWN_VALUES` access. */
@Throws(ConfigError.AlreadyInitialized::class)
fun setDirectoryConfig(config: DirectoryConfig) {
    if (configLocked.get()) {
        throw ConfigError.AlreadyInitialized()
    }
    synchronized(customConfigLock) {
        customConfig = config.copyConfig()
    }
}

/**
 * Adds search paths to the global directory configuration before
 * `KNOWN_VALUES` access.
 */
@Throws(ConfigError.AlreadyInitialized::class)
fun addSearchPaths(paths: List<Path>) {
    if (configLocked.get()) {
        throw ConfigError.AlreadyInitialized()
    }

    synchronized(customConfigLock) {
        val config = customConfig ?: DirectoryConfig.defaultOnly().also {
            customConfig = it
        }
        for (path in paths) {
            config.addPath(path)
        }
    }
}

internal fun getAndLockConfig(): DirectoryConfig {
    configLocked.set(true)
    synchronized(customConfigLock) {
        val config = customConfig?.copyConfig()
        customConfig = null
        return config ?: DirectoryConfig.defaultOnly()
    }
}

private fun loadFromDirectoryTolerant(
    path: Path,
): Pair<List<KnownValue>, List<Pair<Path, LoadError>>> {
    val values = mutableListOf<KnownValue>()
    val errors = mutableListOf<Pair<Path, LoadError>>()

    if (!Files.exists(path) || !Files.isDirectory(path)) {
        return values to errors
    }

    try {
        Files.newDirectoryStream(path).use { entries ->
            for (entry in entries) {
                if (isJsonFile(entry)) {
                    try {
                        values.addAll(loadSingleFile(entry))
                    } catch (error: LoadError) {
                        errors.add(entry to error)
                    }
                }
            }
        }
    } catch (error: IOException) {
        throw LoadError.Io(error)
    }

    return values to errors
}

private fun loadSingleFile(path: Path): List<KnownValue> {
    val content = try {
        Files.readString(path)
    } catch (error: IOException) {
        throw LoadError.Io(error)
    }

    val registry = try {
        parseRegistryFile(content)
    } catch (error: Exception) {
        throw LoadError.Json(path, error)
    }

    return registry.entries.map { entry ->
        KnownValue.newWithName(entry.codepoint, entry.name)
    }
}

private fun isJsonFile(path: Path): Boolean =
    path.fileName.toString().endsWith(".json")

private fun parseRegistryFile(json: String): RegistryFile {
    val parsed: RegistryFileJson = objectMapper.readValue(json)
    return RegistryFile(
        ontology = parsed.ontology?.toPublic(),
        generated = parsed.generated?.toPublic(),
        entries = parsed.entries.map { it.toPublic() },
        statistics = parsed.statistics,
    )
}

private fun EntryJson.toPublic(): RegistryEntry {
    return RegistryEntry(
        codepoint = codepoint.toULongChecked("codepoint"),
        name = name,
        entryType = entryType,
        uri = uri,
        description = description,
    )
}

private fun OntologyInfoJson.toPublic(): OntologyInfo = OntologyInfo(
    name = name,
    sourceUrl = sourceUrl,
    startCodePoint = startCodePoint?.toULongChecked("start_code_point"),
    processingStrategy = processingStrategy,
)

private fun GeneratedInfoJson.toPublic(): GeneratedInfo = GeneratedInfo(
    tool = tool,
)

private fun BigInteger.toULongChecked(fieldName: String): ULong {
    require(this >= BigInteger.ZERO) { "$fieldName must be non-negative" }
    require(this <= MAX_ULONG_BIG_INTEGER) { "$fieldName must fit in u64" }
    return toString().toULong()
}

private val MAX_ULONG_BIG_INTEGER = BigInteger("18446744073709551615")

private val objectMapper = jacksonObjectMapper()

private val customConfigLock = Any()
private var customConfig: DirectoryConfig? = null
private val configLocked: AtomicBoolean = AtomicBoolean(false)

@JsonIgnoreProperties(ignoreUnknown = true)
private data class EntryJson(
    val codepoint: BigInteger,
    val name: String,
    @param:JsonProperty("type") val entryType: String? = null,
    val uri: String? = null,
    val description: String? = null,
)

@JsonIgnoreProperties(ignoreUnknown = true)
private data class OntologyInfoJson(
    val name: String? = null,
    @param:JsonProperty("source_url") val sourceUrl: String? = null,
    @param:JsonProperty("start_code_point")
    val startCodePoint: BigInteger? = null,
    @param:JsonProperty("processing_strategy")
    val processingStrategy: String? = null,
)

@JsonIgnoreProperties(ignoreUnknown = true)
private data class GeneratedInfoJson(
    val tool: String? = null,
)

@JsonIgnoreProperties(ignoreUnknown = true)
private data class RegistryFileJson(
    val ontology: OntologyInfoJson? = null,
    val generated: GeneratedInfoJson? = null,
    val entries: List<EntryJson>,
    val statistics: Any? = null,
)

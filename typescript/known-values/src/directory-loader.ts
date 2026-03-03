/**
 * Directory-based loading of known values from JSON registry files.
 *
 * Supports loading known values from JSON files stored in configurable
 * directories. Values loaded from JSON files can override hardcoded values
 * when they share the same codepoint.
 */

import * as fs from 'node:fs';
import * as path from 'node:path';
import * as os from 'node:os';

import { KnownValue } from './known-value.js';
import { isConfigLocked } from './config-state.js';

// ---------------------------------------------------------------------------
// Registry JSON types
// ---------------------------------------------------------------------------

/** A single entry in a known values JSON registry file. */
export interface RegistryEntry {
    codepoint: number;
    name: string;
    type?: string;
    uri?: string;
    description?: string;
}

/** Metadata about the ontology or registry source. */
export interface OntologyInfo {
    name?: string;
    source_url?: string;
    start_code_point?: number;
    processing_strategy?: string;
}

/** Information about how a registry file was generated. */
export interface GeneratedInfo {
    tool?: string;
}

/** Root structure of a known values JSON registry file. */
export interface RegistryFile {
    ontology?: OntologyInfo;
    generated?: GeneratedInfo;
    entries: RegistryEntry[];
    statistics?: unknown;
}

// ---------------------------------------------------------------------------
// Error types
// ---------------------------------------------------------------------------

/** Errors that can occur when loading known values from directories. */
export class LoadError extends Error {
    readonly kind: 'io' | 'json';
    readonly filePath?: string;

    constructor(kind: 'io' | 'json', message: string, filePath?: string, cause?: Error) {
        super(message, cause ? { cause } : undefined);
        this.kind = kind;
        this.filePath = filePath;
        this.name = 'LoadError';
    }

    static io(message: string): LoadError {
        return new LoadError('io', `IO error: ${message}`);
    }

    static json(filePath: string, cause: Error): LoadError {
        return new LoadError('json', `JSON parse error in ${filePath}: ${cause.message}`, filePath, cause);
    }
}

/** Error thrown when attempting to modify directory configuration after KNOWN_VALUES has been accessed. */
export class ConfigError extends Error {
    constructor() {
        super('Cannot modify directory configuration after KNOWN_VALUES has been accessed');
        this.name = 'ConfigError';
    }
}

// ---------------------------------------------------------------------------
// LoadResult
// ---------------------------------------------------------------------------

/** Result of a directory loading operation. */
export class LoadResult {
    readonly values: Map<bigint, KnownValue>;
    readonly filesProcessed: string[];
    readonly errors: Array<[string, LoadError]>;

    constructor(
        values: Map<bigint, KnownValue> = new Map(),
        filesProcessed: string[] = [],
        errors: Array<[string, LoadError]> = [],
    ) {
        this.values = values;
        this.filesProcessed = filesProcessed;
        this.errors = errors;
    }

    /** The number of unique values loaded. */
    get valuesCount(): number {
        return this.values.size;
    }

    /** Whether any errors occurred during loading. */
    get hasErrors(): boolean {
        return this.errors.length > 0;
    }
}

// ---------------------------------------------------------------------------
// DirectoryConfig
// ---------------------------------------------------------------------------

/** Configuration for loading known values from directories. */
export class DirectoryConfig {
    readonly #paths: string[];

    private constructor(paths: string[]) {
        this.#paths = paths;
    }

    /** Creates a new empty configuration with no search paths. */
    static create(): DirectoryConfig {
        return new DirectoryConfig([]);
    }

    /** Creates configuration with only the default directory (~/.known-values/). */
    static defaultOnly(): DirectoryConfig {
        return new DirectoryConfig([DirectoryConfig.defaultDirectory()]);
    }

    /** Creates configuration with custom paths (processed in order). */
    static withPaths(paths: string[]): DirectoryConfig {
        return new DirectoryConfig([...paths]);
    }

    /** Creates configuration with custom paths followed by the default directory. */
    static withPathsAndDefault(paths: string[]): DirectoryConfig {
        return new DirectoryConfig([...paths, DirectoryConfig.defaultDirectory()]);
    }

    /** Returns the default directory: ~/.known-values/ */
    static defaultDirectory(): string {
        const home = os.homedir();
        return path.join(home, '.known-values');
    }

    /** Returns the configured search paths. */
    get paths(): readonly string[] {
        return this.#paths;
    }

    /** Adds a path to the configuration. */
    addPath(dirPath: string): void {
        this.#paths.push(dirPath);
    }
}

// ---------------------------------------------------------------------------
// Loading functions
// ---------------------------------------------------------------------------

/**
 * Loads all JSON registry files from a single directory.
 *
 * Returns an array of loaded KnownValue instances, or an empty array if the
 * directory doesn't exist. Throws LoadError on I/O or JSON parse errors.
 */
export function loadFromDirectory(dirPath: string): KnownValue[] {
    const values: KnownValue[] = [];

    if (!fs.existsSync(dirPath) || !fs.statSync(dirPath).isDirectory()) {
        return values;
    }

    const entries = fs.readdirSync(dirPath);
    for (const entry of entries) {
        const filePath = path.join(dirPath, entry);
        if (path.extname(filePath) !== '.json') continue;

        const content = fs.readFileSync(filePath, 'utf-8');
        let registry: RegistryFile;
        try {
            registry = JSON.parse(content) as RegistryFile;
        } catch (e) {
            throw LoadError.json(filePath, e as Error);
        }

        for (const regEntry of registry.entries) {
            values.push(KnownValue.withName(regEntry.codepoint, regEntry.name));
        }
    }

    return values;
}

/**
 * Loads from a directory with tolerance for individual file failures.
 */
function loadFromDirectoryTolerant(
    dirPath: string,
): { values: KnownValue[]; errors: Array<[string, LoadError]> } {
    const values: KnownValue[] = [];
    const errors: Array<[string, LoadError]> = [];

    if (!fs.existsSync(dirPath) || !fs.statSync(dirPath).isDirectory()) {
        return { values, errors };
    }

    const dirEntries = fs.readdirSync(dirPath);
    for (const entry of dirEntries) {
        const filePath = path.join(dirPath, entry);
        if (path.extname(filePath) !== '.json') continue;

        try {
            const content = fs.readFileSync(filePath, 'utf-8');
            const registry = JSON.parse(content) as RegistryFile;
            for (const regEntry of registry.entries) {
                values.push(KnownValue.withName(regEntry.codepoint, regEntry.name));
            }
        } catch (e) {
            errors.push([filePath, LoadError.json(filePath, e as Error)]);
        }
    }

    return { values, errors };
}

/**
 * Loads known values from all directories in the given configuration.
 *
 * Directories are processed in order. Values from later directories override
 * values from earlier directories when codepoints collide.
 */
export function loadFromConfig(config: DirectoryConfig): LoadResult {
    const result = new LoadResult();

    for (const dirPath of config.paths) {
        try {
            const { values, errors } = loadFromDirectoryTolerant(dirPath);
            for (const value of values) {
                result.values.set(value.value, value);
            }
            for (const [errorPath, error] of errors) {
                result.errors.push([errorPath, error]);
            }
            result.filesProcessed.push(dirPath);
        } catch (e) {
            result.errors.push([dirPath, LoadError.io((e as Error).message)]);
        }
    }

    return result;
}

// ---------------------------------------------------------------------------
// Global configuration state
// ---------------------------------------------------------------------------

let _customConfig: DirectoryConfig | undefined;

/**
 * Sets custom directory configuration for known values loading.
 *
 * Must be called before the first access to KNOWN_VALUES.
 */
export function setDirectoryConfig(config: DirectoryConfig): void {
    if (isConfigLocked()) {
        throw new ConfigError();
    }
    _customConfig = config;
}

/**
 * Adds additional search paths to the directory configuration.
 *
 * Must be called before the first access to KNOWN_VALUES.
 */
export function addSearchPaths(paths: string[]): void {
    if (isConfigLocked()) {
        throw new ConfigError();
    }
    if (_customConfig === undefined) {
        _customConfig = DirectoryConfig.defaultOnly();
    }
    for (const p of paths) {
        _customConfig.addPath(p);
    }
}

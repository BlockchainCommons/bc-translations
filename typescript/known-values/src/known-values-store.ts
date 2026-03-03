/**
 * A store that maps between Known Values and their assigned names.
 *
 * Provides bidirectional mapping between numeric values (bigint) and
 * KnownValue instances, and between string names and KnownValue instances.
 */

import { KnownValue } from './known-value.js';
import {
    type DirectoryConfig,
    type LoadResult,
    loadFromDirectory as doLoadFromDirectory,
    loadFromConfig as doLoadFromConfig,
} from './directory-loader.js';

export class KnownValuesStore {
    #byRawValue: Map<bigint, KnownValue> = new Map();
    #byAssignedName: Map<string, KnownValue> = new Map();

    /**
     * Creates a new KnownValuesStore with the provided Known Values.
     */
    constructor(knownValues: Iterable<KnownValue> = []) {
        for (const kv of knownValues) {
            this.#doInsert(kv);
        }
    }

    /**
     * Inserts a KnownValue into the store.
     *
     * If a KnownValue with the same raw value already exists, its old name
     * is removed from the name index before adding the new one.
     */
    insert(knownValue: KnownValue): void {
        this.#doInsert(knownValue);
    }

    /**
     * Returns the assigned name for a KnownValue, if present in the store.
     */
    assignedName(knownValue: KnownValue): string | undefined {
        return this.#byRawValue.get(knownValue.value)?.assignedName;
    }

    /**
     * Returns a human-readable name for a KnownValue.
     *
     * If the KnownValue has an assigned name in the store, that name is
     * returned. Otherwise, the KnownValue's default name is returned.
     */
    name(knownValue: KnownValue): string {
        return this.assignedName(knownValue) ?? knownValue.name;
    }

    /**
     * Looks up a KnownValue by its assigned name.
     */
    knownValueNamed(assignedName: string): KnownValue | undefined {
        return this.#byAssignedName.get(assignedName);
    }

    /**
     * Looks up a KnownValue by its raw numeric value.
     */
    getByRawValue(rawValue: bigint): KnownValue | undefined {
        return this.#byRawValue.get(rawValue);
    }

    /**
     * Retrieves a KnownValue for a raw value, using a store if provided.
     *
     * If a store is provided and contains a mapping, that KnownValue is
     * returned. Otherwise, a new KnownValue with no assigned name is created.
     */
    static knownValueForRawValue(
        rawValue: bigint,
        knownValues?: KnownValuesStore,
    ): KnownValue {
        if (knownValues) {
            const found = knownValues.getByRawValue(rawValue);
            if (found !== undefined) return found;
        }
        return new KnownValue(rawValue);
    }

    /**
     * Attempts to find a KnownValue by its name, using a store if provided.
     */
    static knownValueForName(
        name: string,
        knownValues?: KnownValuesStore,
    ): KnownValue | undefined {
        return knownValues?.knownValueNamed(name);
    }

    /**
     * Returns a human-readable name for a KnownValue, using a store if provided.
     */
    static nameForKnownValue(
        knownValue: KnownValue,
        knownValues?: KnownValuesStore,
    ): string {
        if (knownValues) {
            const assigned = knownValues.assignedName(knownValue);
            if (assigned !== undefined) {
                return assigned;
            }
        }
        return knownValue.name;
    }

    /**
     * Loads and inserts known values from a directory containing JSON registry
     * files.
     *
     * Returns the number of values loaded.
     */
    loadFromDirectory(dirPath: string): number {
        const values = doLoadFromDirectory(dirPath);
        for (const value of values) {
            this.insert(value);
        }
        return values.length;
    }

    /**
     * Loads known values from multiple directories using the provided
     * configuration.
     *
     * Directories are processed in order. Later directories override earlier
     * ones.
     */
    loadFromConfig(config: DirectoryConfig): LoadResult {
        const result = doLoadFromConfig(config);
        for (const value of result.values.values()) {
            this.insert(value);
        }
        return result;
    }

    #doInsert(knownValue: KnownValue): void {
        // If there's an existing value with the same codepoint, remove its
        // old name from the name index to avoid stale entries.
        const existing = this.#byRawValue.get(knownValue.value);
        if (existing?.assignedName !== undefined) {
            this.#byAssignedName.delete(existing.assignedName);
        }

        this.#byRawValue.set(knownValue.value, knownValue);
        if (knownValue.assignedName !== undefined) {
            this.#byAssignedName.set(knownValue.assignedName, knownValue);
        }
    }
}

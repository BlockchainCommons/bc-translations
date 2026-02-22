package com.blockchaincommons.knownvalues

import java.nio.file.Path

/**
 * A bidirectional store for known values by numeric codepoint and assigned
 * name.
 */
class KnownValuesStore(
    knownValues: Iterable<KnownValue> = emptyList(),
) {
    private val knownValuesByRawValue: MutableMap<ULong, KnownValue> =
        linkedMapOf()
    private val knownValuesByAssignedName: MutableMap<String, KnownValue> =
        linkedMapOf()

    init {
        for (knownValue in knownValues) {
            insertInternal(
                knownValue,
                knownValuesByRawValue,
                knownValuesByAssignedName,
            )
        }
    }

    /** Inserts or replaces a known value by codepoint and name. */
    fun insert(knownValue: KnownValue) {
        insertInternal(
            knownValue,
            knownValuesByRawValue,
            knownValuesByAssignedName,
        )
    }

    /** Returns the store-assigned name for a known value, if present. */
    fun assignedName(knownValue: KnownValue): String? {
        return knownValuesByRawValue[knownValue.value()]?.assignedName()
    }

    /** Returns the store-assigned name or falls back to value.name(). */
    fun name(knownValue: KnownValue): String {
        return assignedName(knownValue) ?: knownValue.name()
    }

    /** Looks up a known value by assigned name. */
    fun knownValueNamed(assignedName: String): KnownValue? {
        return knownValuesByAssignedName[assignedName]
    }

    /**
     * Loads known values from JSON registry files in a directory.
     *
     * @return number of entries loaded from files in this directory.
     * @throws LoadError on I/O or JSON parse failure.
     */
    @Throws(LoadError::class)
    fun loadFromDirectory(path: Path): Int {
        val values = com.blockchaincommons.knownvalues.loadFromDirectory(path)
        val count = values.size
        for (value in values) {
            insert(value)
        }
        return count
    }

    /**
     * Loads known values from configured directories with tolerant error
     * collection.
     */
    fun loadFromConfig(config: DirectoryConfig): LoadResult {
        val result = com.blockchaincommons.knownvalues.loadFromConfig(config)
        for (value in result.values.values) {
            insert(value)
        }
        return result
    }

    companion object {
        /** Static-style constructor equivalent to Rust `KnownValuesStore::new`. */
        fun new(knownValues: Iterable<KnownValue>): KnownValuesStore {
            return KnownValuesStore(knownValues)
        }

        /** Returns a known value from store lookup, or a raw fallback value. */
        fun knownValueForRawValue(
            rawValue: ULong,
            knownValues: KnownValuesStore?,
        ): KnownValue {
            return knownValues?.knownValuesByRawValue?.get(rawValue)
                ?: KnownValue.new(rawValue)
        }

        /** Returns a known value by name, if found in the optional store. */
        fun knownValueForName(
            name: String,
            knownValues: KnownValuesStore?,
        ): KnownValue? {
            return knownValues?.knownValueNamed(name)
        }

        /** Returns a display name for a known value using an optional store. */
        fun nameForKnownValue(
            knownValue: KnownValue,
            knownValues: KnownValuesStore?,
        ): String {
            return knownValues?.assignedName(knownValue) ?: knownValue.name()
        }

        private fun insertInternal(
            knownValue: KnownValue,
            byRawValue: MutableMap<ULong, KnownValue>,
            byAssignedName: MutableMap<String, KnownValue>,
        ) {
            val oldValue = byRawValue[knownValue.value()]
            val oldName = oldValue?.assignedName()
            if (oldName != null) {
                byAssignedName.remove(oldName)
            }

            byRawValue[knownValue.value()] = knownValue
            val newName = knownValue.assignedName()
            if (newName != null) {
                byAssignedName[newName] = knownValue
            }
        }
    }
}

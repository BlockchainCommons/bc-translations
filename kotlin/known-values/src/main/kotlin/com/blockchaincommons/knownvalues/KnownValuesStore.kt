package com.blockchaincommons.knownvalues

import java.nio.file.Path

/**
 * A bidirectional store for known values, indexed by both numeric codepoint
 * and assigned name.
 */
class KnownValuesStore(
    knownValues: Iterable<KnownValue> = emptyList(),
) {
    private val byValue: MutableMap<ULong, KnownValue> = linkedMapOf()
    private val byName: MutableMap<String, KnownValue> = linkedMapOf()

    init {
        for (knownValue in knownValues) {
            insertValue(knownValue)
        }
    }

    /** Inserts or replaces a known value by codepoint and name. */
    fun insert(knownValue: KnownValue) {
        insertValue(knownValue)
    }

    /** Returns the store-assigned name for a known value, if present. */
    fun assignedName(knownValue: KnownValue): String? =
        byValue[knownValue.value]?.assignedName

    /** Returns the store-assigned name or falls back to the value's own name. */
    fun name(knownValue: KnownValue): String =
        assignedName(knownValue) ?: knownValue.name

    /** Looks up a known value by assigned name. */
    fun knownValueNamed(assignedName: String): KnownValue? = byName[assignedName]

    /**
     * Loads known values from JSON registry files in a directory.
     *
     * @return number of entries loaded from files in this directory.
     * @throws LoadError on I/O or JSON parse failure.
     */
    @Throws(LoadError::class)
    fun loadFromDirectory(path: Path): Int {
        val values = com.blockchaincommons.knownvalues.loadFromDirectory(path)
        for (value in values) {
            insert(value)
        }
        return values.size
    }

    /**
     * Loads known values from configured directories with tolerant error
     * collection.
     */
    fun loadFromConfig(config: DirectoryConfig): LoadResult {
        val result = com.blockchaincommons.knownvalues.loadFromConfig(config)
        for (value in result.values()) {
            insert(value)
        }
        return result
    }

    private fun insertValue(knownValue: KnownValue) {
        val oldValue = byValue[knownValue.value]
        val oldName = oldValue?.assignedName
        if (oldName != null) {
            byName.remove(oldName)
        }

        byValue[knownValue.value] = knownValue
        val newName = knownValue.assignedName
        if (newName != null) {
            byName[newName] = knownValue
        }
    }

    companion object {
        /** Returns a known value from store lookup, or a raw fallback value. */
        fun knownValueForRawValue(
            rawValue: ULong,
            knownValues: KnownValuesStore?,
        ): KnownValue {
            return knownValues?.byValue?.get(rawValue)
                ?: KnownValue(rawValue)
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
            return knownValues?.assignedName(knownValue) ?: knownValue.name
        }
    }
}

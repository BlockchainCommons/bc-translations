package com.blockchaincommons.dcbor

typealias CborSummarizer = (Cbor, Boolean) -> String

/**
 * Interface for types that map between CBOR tags and their human-readable names.
 */
interface TagsStoreTrait {
    fun assignedNameForTag(tag: Tag): String?
    fun nameForTag(tag: Tag): String = assignedNameForTag(tag) ?: tag.value.toString()
    fun tagForValue(value: TagValue): Tag?
    fun tagForName(name: String): Tag?
    fun nameForValue(value: TagValue): String {
        return tagForValue(value)?.name ?: value.toString()
    }
    fun summarizer(tag: TagValue): CborSummarizer?
}

/**
 * Specifies which tag store to use for formatting operations.
 */
sealed class TagsStoreOpt {
    data object None : TagsStoreOpt()
    data object Global : TagsStoreOpt()
    class Custom(val store: TagsStoreTrait) : TagsStoreOpt()
}

/**
 * Registry that maintains mappings between CBOR tags, their names, and optional summarizers.
 */
class TagsStore : TagsStoreTrait {
    private val tagsByValue = mutableMapOf<TagValue, Tag>()
    private val tagsByName = mutableMapOf<String, Tag>()
    private val summarizers = mutableMapOf<TagValue, CborSummarizer>()

    constructor()

    constructor(tags: Iterable<Tag>) {
        for (tag in tags) insert(tag)
    }

    fun insert(tag: Tag) {
        val name = requireNotNull(tag.name) { "Tag must have a name to be inserted into a TagsStore" }
        require(name.isNotEmpty()) { "Tag name must not be empty" }
        val existing = tagsByValue[tag.value]
        if (existing != null) {
            val existingName = existing.name!!
            require(existingName == name) {
                "Attempt to register tag: ${tag.value} '$existingName' with different name: '$name'"
            }
        }
        tagsByValue[tag.value] = tag
        tagsByName[name] = tag
    }

    fun insertAll(tags: List<Tag>) {
        for (tag in tags) insert(tag)
    }

    fun setSummarizer(tag: TagValue, summarizer: CborSummarizer) {
        summarizers[tag] = summarizer
    }

    override fun assignedNameForTag(tag: Tag): String? {
        return tagForValue(tag.value)?.name
    }

    override fun tagForValue(value: TagValue): Tag? = tagsByValue[value]

    override fun tagForName(name: String): Tag? = tagsByName[name]

    override fun summarizer(tag: TagValue): CborSummarizer? = summarizers[tag]
}

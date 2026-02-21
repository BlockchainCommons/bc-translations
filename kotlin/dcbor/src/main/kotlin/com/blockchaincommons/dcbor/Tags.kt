package com.blockchaincommons.dcbor

/** CBOR tag value for date/time (tag 1, RFC 8949). */
const val TAG_DATE: ULong = 1uL
const val TAG_NAME_DATE: String = "date"

/**
 * Global tag store singleton.
 *
 * Thread-safe via synchronized access. Call [registerTags] to populate
 * with standard CBOR tags before using diagnostic formatting.
 */
object GlobalTags {
    private val store = TagsStore()
    private val lock = Any()

    fun <T> withTags(action: (TagsStore) -> T): T = synchronized(lock) { action(store) }
    fun withTagsMut(action: (TagsStore) -> Unit) = synchronized(lock) { action(store) }
}

/**
 * Register standard CBOR tags (date) in the given tags store.
 */
fun registerTagsIn(tagsStore: TagsStore) {
    tagsStore.insert(Tag(TAG_DATE, TAG_NAME_DATE))
    tagsStore.setSummarizer(TAG_DATE) { untaggedCbor, _ ->
        val date = CborDate.fromUntaggedCbor(untaggedCbor)
        date.toString()
    }
}

/**
 * Register standard CBOR tags in the global tags store.
 */
fun registerTags() {
    GlobalTags.withTagsMut { registerTagsIn(it) }
}

/**
 * Convert tag values to Tag objects, looking up names in the global store.
 */
fun tagsForValues(values: List<ULong>): List<Tag> {
    return GlobalTags.withTags { store ->
        values.map { value ->
            store.tagForValue(value) ?: Tag.withValue(value)
        }
    }
}

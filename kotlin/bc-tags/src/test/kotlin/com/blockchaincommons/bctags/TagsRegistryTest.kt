package com.blockchaincommons.bctags

import com.blockchaincommons.dcbor.GlobalTags
import com.blockchaincommons.dcbor.TAG_DATE
import com.blockchaincommons.dcbor.TAG_NAME_DATE
import com.blockchaincommons.dcbor.TagsStore
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNotNull

class TagsRegistryTest {
    @Test
    fun registerTagsInRegistersDcborAndBcTags() {
        val store = TagsStore()

        registerTagsIn(store)

        assertEquals(TAG_NAME_DATE, store.nameForValue(TAG_DATE))
        assertEquals(75, BC_TAGS.size)

        BC_TAGS.forEach { tag ->
            val found = store.tagForValue(tag.value)
            assertNotNull(found)
            assertEquals(tag.name, found.name)
        }
    }

    @Test
    fun registerTagsRegistersGlobalStore() {
        registerTags()

        GlobalTags.withTags { store ->
            assertEquals(TAG_NAME_DATE, store.nameForValue(TAG_DATE))
            assertEquals(TAG_NAME_ENVELOPE, store.nameForValue(TAG_ENVELOPE))
            assertEquals(TAG_NAME_PROVENANCE_MARK, store.nameForValue(TAG_PROVENANCE_MARK))
        }
    }

    @Test
    fun registrationOrderMatchesRustList() {
        assertEquals(TAG_URI, BC_TAGS.first().value)
        assertEquals(TAG_PROVENANCE_MARK, BC_TAGS.last().value)
        assertEquals(TAG_NAME_URI, BC_TAGS.first().name)
        assertEquals(TAG_NAME_PROVENANCE_MARK, BC_TAGS.last().name)
    }
}

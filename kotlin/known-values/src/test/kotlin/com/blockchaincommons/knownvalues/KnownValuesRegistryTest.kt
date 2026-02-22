package com.blockchaincommons.knownvalues

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNotNull

class KnownValuesRegistryTest {
    @Test
    fun testKnownValueLookupByName() {
        assertEquals(1uL, IS_A.value)
        assertEquals("isA", IS_A.name)

        val knownValues = KNOWN_VALUES
        val isA = knownValues.knownValueNamed("isA")
        assertNotNull(isA)
        assertEquals(1uL, isA.value)
    }
}

package com.blockchaincommons.bcrand

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse

class SecureRandomTest {

    @Test
    fun testRandomData() {
        val data1 = randomData(32)
        val data2 = randomData(32)
        val data3 = randomData(32)
        assertEquals(32, data1.size)
        assertFalse(data1.contentEquals(data2))
        assertFalse(data1.contentEquals(data3))
    }
}

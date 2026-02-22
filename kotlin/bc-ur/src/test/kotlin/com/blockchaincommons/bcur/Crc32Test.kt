package com.blockchaincommons.bcur

import kotlin.test.Test
import kotlin.test.assertEquals

class Crc32Test {
    @Test
    fun testCrc32() {
        assertEquals(0xEBE6C6E6u, Crc32.checksum("Hello, world!".toByteArray()))
        assertEquals(0x598C84DCu, Crc32.checksum("Wolf".toByteArray()))
    }
}

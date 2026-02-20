package com.blockchaincommons.bccrypto

import kotlin.test.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertEquals
import kotlin.test.assertFalse

class ScryptTest {

    @Test
    fun testScryptBasic() {
        val pass = "password".toByteArray()
        val salt = "salt".toByteArray()
        val output = scrypt(pass, salt, 32)
        assertEquals(32, output.size)
        val output2 = scrypt(pass, salt, 32)
        assertContentEquals(output, output2)
    }

    @Test
    fun testScryptDifferentSalt() {
        val pass = "password".toByteArray()
        val salt1 = "salt1".toByteArray()
        val salt2 = "salt2".toByteArray()
        val out1 = scrypt(pass, salt1, 32)
        val out2 = scrypt(pass, salt2, 32)
        assertFalse(out1.contentEquals(out2))
    }

    @Test
    fun testScryptOptBasic() {
        val pass = "password".toByteArray()
        val salt = "salt".toByteArray()
        val output = scrypt(pass, salt, 32, logN = 15, r = 8, p = 1)
        assertEquals(32, output.size)
    }

    @Test
    fun testScryptOutputLength() {
        val pass = "password".toByteArray()
        val salt = "salt".toByteArray()
        for (len in listOf(16, 24, 32, 64)) {
            val output = scrypt(pass, salt, len)
            assertEquals(len, output.size)
        }
    }
}

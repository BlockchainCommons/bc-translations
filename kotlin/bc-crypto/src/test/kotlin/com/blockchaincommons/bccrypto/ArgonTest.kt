package com.blockchaincommons.bccrypto

import kotlin.test.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertEquals
import kotlin.test.assertFalse

class ArgonTest {

    @Test
    fun testArgon2idBasic() {
        val pass = "password".toByteArray()
        val salt = "example salt".toByteArray()
        val output = argon2id(pass, salt, 32)
        assertEquals(32, output.size)
        val output2 = argon2id(pass, salt, 32)
        assertContentEquals(output, output2)
    }

    @Test
    fun testArgon2idDifferentSalt() {
        val pass = "password".toByteArray()
        val salt1 = "example salt".toByteArray()
        val salt2 = "example salt2".toByteArray()
        val out1 = argon2id(pass, salt1, 32)
        val out2 = argon2id(pass, salt2, 32)
        assertFalse(out1.contentEquals(out2))
    }
}

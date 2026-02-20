package com.blockchaincommons.bccrypto

import kotlin.test.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertEquals

class HashTest {

    @Test
    fun testCrc32() {
        val input = "Hello, world!".toByteArray()
        assertEquals(0xebe6c6e6u, crc32(input))
        assertContentEquals("ebe6c6e6".hexToByteArray(), crc32Data(input))
        assertContentEquals("e6c6e6eb".hexToByteArray(), crc32Data(input, littleEndian = true))
    }

    @Test
    fun testSha256() {
        val input = "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"
        val expected = "248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1".hexToByteArray()
        assertContentEquals(expected, sha256(input.toByteArray()))
    }

    @Test
    fun testSha512() {
        val input = "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"
        val expected = "204a8fc6dda82f0a0ced7beb8e08a41657c16ef468b228a8279be331a703c33596fd15c13b1b07f9aa1d3bea57789ca031ad85c7a71dd70354ec631238ca3445".hexToByteArray()
        assertContentEquals(expected, sha512(input.toByteArray()))
    }

    @Test
    fun testHmacSha() {
        val key = "0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b".hexToByteArray()
        val message = "Hi There".toByteArray()
        assertContentEquals(
            "b0344c61d8db38535ca8afceaf0bf12b881dc200c9833da726e9376c2e32cff7".hexToByteArray(),
            hmacSha256(key, message),
        )
        assertContentEquals(
            "87aa7cdea5ef619d4ff0b4241a1d6cb02379f4e2ce4ec2787ad0b30545e17cdedaa833b7d6b8a702038b274eaea3f4e4be9d914eeb61f1702e696c203a126854".hexToByteArray(),
            hmacSha512(key, message),
        )
    }

    @Test
    fun testPbkdf2HmacSha256() {
        assertContentEquals(
            "120fb6cffcf8b32c43e7225256c4f837a86548c92ccc35480805987cb70be17b".hexToByteArray(),
            pbkdf2HmacSha256("password".toByteArray(), "salt".toByteArray(), 1, 32),
        )
    }

    @Test
    fun testHkdfHmacSha256() {
        val keyMaterial = "hello".toByteArray()
        val salt = "8e94ef805b93e683ff18".hexToByteArray()
        assertContentEquals(
            "13485067e21af17c0900f70d885f02593c0e61e46f86450e4a0201a54c14db76".hexToByteArray(),
            hkdfHmacSha256(keyMaterial, salt, 32),
        )
    }
}

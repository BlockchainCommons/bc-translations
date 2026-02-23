package com.blockchaincommons.provenancemark

import kotlin.test.Test
import kotlin.test.assertContentEquals

class CryptoUtilsTest {
    @Test
    fun testSha256() {
        val data = "Hello World".encodeToByteArray()
        assertContentEquals(
            hex("a591a6d40bf420404a011733cfb7b190d62c65bf0bcda32b57b277d9ad9f146e"),
            CryptoUtils.sha256(data),
        )
    }

    @Test
    fun testExtendKey() {
        val data = "Hello World".encodeToByteArray()
        assertContentEquals(
            hex("813085a508d5fec645abe5a1fb9a23c2a6ac6bef0a99650017b3ef50538dba39"),
            CryptoUtils.extendKey(data),
        )
    }

    @Test
    fun testObfuscate() {
        val key = "Hello".encodeToByteArray()
        val message = "World".encodeToByteArray()

        val obfuscated = CryptoUtils.obfuscate(key, message)
        assertContentEquals(hex("c43889aafa"), obfuscated)

        val deobfuscated = CryptoUtils.obfuscate(key, obfuscated)
        assertContentEquals(message, deobfuscated)
    }
}

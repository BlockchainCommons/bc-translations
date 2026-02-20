package com.blockchaincommons.bccrypto

import com.blockchaincommons.bcrand.randomData
import kotlin.test.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertEquals

class SymmetricEncryptionTest {

    private val plaintext = "Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it.".toByteArray()
    private val aad = "50515253c0c1c2c3c4c5c6c7".hexToByteArray()
    private val key = "808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9f".hexToByteArray()
    private val nonce = "070000004041424344454647".hexToByteArray()
    private val expectedCiphertext = "d31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2b5a736ee62d63dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6a5b67ecd3b3692ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585808b4831d7bc3ff4def08e4b7a9de576d26586cec64b6116".hexToByteArray()
    private val expectedAuth = "1ae10b594f09e26a7e902ecbd0600691".hexToByteArray()

    @Test
    fun testRfcTestVector() {
        val (ciphertext, auth) = aeadChaCha20Poly1305Encrypt(plaintext, key, nonce, aad)
        assertContentEquals(expectedCiphertext, ciphertext)
        assertContentEquals(expectedAuth, auth)

        val decrypted = aeadChaCha20Poly1305Decrypt(ciphertext, key, nonce, auth, aad)
        assertContentEquals(plaintext, decrypted)
    }

    @Test
    fun testRandomKeyAndNonce() {
        val randomKey = randomData(32)
        val randomNonce = randomData(12)
        val (ciphertext, auth) = aeadChaCha20Poly1305Encrypt(plaintext, randomKey, randomNonce, aad)
        val decrypted = aeadChaCha20Poly1305Decrypt(ciphertext, randomKey, randomNonce, auth, aad)
        assertContentEquals(plaintext, decrypted)
    }

    @Test
    fun testEmptyData() {
        val randomKey = randomData(32)
        val randomNonce = randomData(12)
        val (ciphertext, auth) = aeadChaCha20Poly1305Encrypt(ByteArray(0), randomKey, randomNonce)
        assertEquals(0, ciphertext.size)
        val decrypted = aeadChaCha20Poly1305Decrypt(ciphertext, randomKey, randomNonce, auth)
        assertEquals(0, decrypted.size)
    }
}

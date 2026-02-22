package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertEquals

/**
 * Tests for [SymmetricKey], [AuthenticationTag], and [EncryptedMessage].
 *
 * Includes the RFC-8439 ChaCha20-Poly1305 test vector and CBOR/UR roundtrip
 * tests from the Rust `symmetric/mod.rs` inline tests.
 */
class SymmetricKeyTest {

    // RFC-8439 Section 2.8.2 test vectors
    private val plaintext =
        "Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it."
            .toByteArray()
    private val aad = "50515253c0c1c2c3c4c5c6c7".hexToByteArray()
    private val key = SymmetricKey.fromHex(
        "808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9f"
    )
    private val nonce = Nonce.fromHex("070000004041424344454647")
    private val expectedCiphertext =
        "d31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2b5a736ee62d63dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6a5b67ecd3b3692ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585808b4831d7bc3ff4def08e4b7a9de576d26586cec64b6116"
            .hexToByteArray()
    private val expectedAuth = AuthenticationTag.fromData(
        "1ae10b594f09e26a7e902ecbd0600691".hexToByteArray()
    )

    private fun encryptedMessage(): EncryptedMessage =
        key.encrypt(plaintext, aad, nonce)

    @Test
    fun testRfcTestVector() {
        val encrypted = encryptedMessage()
        assertContentEquals(expectedCiphertext, encrypted.ciphertext())
        assertContentEquals(aad, encrypted.aad())
        assertEquals(nonce, encrypted.nonce())
        assertEquals(expectedAuth, encrypted.authenticationTag())

        val decrypted = key.decrypt(encrypted)
        assertContentEquals(plaintext, decrypted)
    }

    @Test
    fun testRandomKeyAndNonce() {
        val randomKey = SymmetricKey.create()
        val randomNonce = Nonce.create()
        val encrypted = randomKey.encrypt(plaintext, aad, randomNonce)
        val decrypted = randomKey.decrypt(encrypted)
        assertContentEquals(plaintext, decrypted)
    }

    @Test
    fun testEmptyData() {
        val randomKey = SymmetricKey.create()
        val encrypted = randomKey.encrypt(ByteArray(0))
        val decrypted = randomKey.decrypt(encrypted)
        assertContentEquals(ByteArray(0), decrypted)
    }

    @Test
    fun testCborData() {
        registerTags()
        val encrypted = encryptedMessage()
        val cbor = encrypted.taggedCbor()
        val data = cbor.toCborData()

        val expectedHex =
            "d99c42845872d31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2b5a736ee62d63dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6a5b67ecd3b3692ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585808b4831d7bc3ff4def08e4b7a9de576d26586cec64b61164c070000004041424344454647501ae10b594f09e26a7e902ecbd06006914c50515253c0c1c2c3c4c5c6c7"
        assertEquals(expectedHex, data.toHexString())
    }

    @Test
    fun testCborRoundtrip() {
        val encrypted = encryptedMessage()
        val cbor = encrypted.taggedCbor()
        val decoded = EncryptedMessage.fromTaggedCbor(cbor)
        assertEquals(encrypted, decoded)
    }

    @Test
    fun testUrRoundtrip() {
        registerTags()
        val encrypted = encryptedMessage()
        val ur = encrypted.ur()
        val expectedUr =
            "ur:encrypted/lrhdjptecylgeeiemnhnuykglnperfguwskbsaoxpmwegydtjtayzeptvoreosenwyidtbfsrnoxhylkptiobglfzszointnmojplucyjsuebknnambddtahtbonrpkbsnfrenmoutrylbdpktlulkmkaxplvldeascwhdzsqddkvezstbkpmwgolplalufdehtsrffhwkuewtmngrknntvwkotdihlntoswgrhscmgsataeaeaefzfpfwfxfyfefgflgdcyvybdhkgwasvoimkbmhdmsbtihnammegsgdgygmgurtsesasrssskswstcfnbpdct"
        assertEquals(expectedUr, ur.toString())

        val decoded = EncryptedMessage.fromUr(ur)
        assertEquals(encrypted, decoded)
    }
}

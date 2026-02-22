package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertTrue

/**
 * Tests for [EncryptedKey].
 *
 * Based on Rust `encrypted_key/encrypted_key_impl.rs` inline tests:
 * test_encrypted_key_hkdf_roundtrip, test_encrypted_key_pbkdf2_roundtrip,
 * test_encrypted_key_scrypt_roundtrip, test_encrypted_key_argon2id_roundtrip,
 * and test_encrypted_key_wrong_secret_fails.
 */
class EncryptedKeyTest {

    private val testSecret = "correct horse battery staple".toByteArray()

    private fun testContentKey(): SymmetricKey = SymmetricKey.create()

    @Test
    fun testEncryptedKeyHkdfRoundtrip() {
        registerTags()
        val contentKey = testContentKey()

        val encrypted = EncryptedKey.lock(
            KeyDerivationMethod.HKDF, testSecret, contentKey,
        )
        assertTrue(encrypted.toString().contains("HKDF"))
        assertTrue(encrypted.toString().contains("SHA256"))

        val cbor = encrypted.taggedCbor()
        val encrypted2 = EncryptedKey.fromTaggedCbor(cbor)
        val decrypted = encrypted2.unlock(testSecret)

        assertEquals(contentKey, decrypted)
    }

    @Test
    fun testEncryptedKeyPbkdf2Roundtrip() {
        registerTags()
        val contentKey = testContentKey()

        val encrypted = EncryptedKey.lock(
            KeyDerivationMethod.PBKDF2, testSecret, contentKey,
        )
        assertTrue(encrypted.toString().contains("PBKDF2"))
        assertTrue(encrypted.toString().contains("SHA256"))

        val cbor = encrypted.taggedCbor()
        val encrypted2 = EncryptedKey.fromTaggedCbor(cbor)
        val decrypted = encrypted2.unlock(testSecret)

        assertEquals(contentKey, decrypted)
    }

    @Test
    fun testEncryptedKeyScryptRoundtrip() {
        registerTags()
        val contentKey = testContentKey()

        val encrypted = EncryptedKey.lock(
            KeyDerivationMethod.Scrypt, testSecret, contentKey,
        )
        assertTrue(encrypted.toString().contains("Scrypt"))

        val cbor = encrypted.taggedCbor()
        val encrypted2 = EncryptedKey.fromTaggedCbor(cbor)
        val decrypted = encrypted2.unlock(testSecret)

        assertEquals(contentKey, decrypted)
    }

    @Test
    fun testEncryptedKeyArgon2idRoundtrip() {
        registerTags()
        val contentKey = testContentKey()

        val encrypted = EncryptedKey.lock(
            KeyDerivationMethod.Argon2id, testSecret, contentKey,
        )
        assertTrue(encrypted.toString().contains("Argon2id"))

        val cbor = encrypted.taggedCbor()
        val encrypted2 = EncryptedKey.fromTaggedCbor(cbor)
        val decrypted = encrypted2.unlock(testSecret)

        assertEquals(contentKey, decrypted)
    }

    @Test
    fun testEncryptedKeyWrongSecretFailsHkdf() {
        val contentKey = testContentKey()
        val wrongSecret = "wrong secret".toByteArray()

        val encrypted = EncryptedKey.lock(
            KeyDerivationMethod.HKDF, testSecret, contentKey,
        )
        assertFailsWith<BcComponentsException> {
            encrypted.unlock(wrongSecret)
        }
    }

    @Test
    fun testEncryptedKeyWrongSecretFailsPbkdf2() {
        val contentKey = testContentKey()
        val wrongSecret = "wrong secret".toByteArray()

        val encrypted = EncryptedKey.lock(
            KeyDerivationMethod.PBKDF2, testSecret, contentKey,
        )
        assertFailsWith<BcComponentsException> {
            encrypted.unlock(wrongSecret)
        }
    }

    @Test
    fun testEncryptedKeyWrongSecretFailsScrypt() {
        val contentKey = testContentKey()
        val wrongSecret = "wrong secret".toByteArray()

        val encrypted = EncryptedKey.lock(
            KeyDerivationMethod.Scrypt, testSecret, contentKey,
        )
        assertFailsWith<BcComponentsException> {
            encrypted.unlock(wrongSecret)
        }
    }

    @Test
    fun testEncryptedKeyWrongSecretFailsArgon2id() {
        val contentKey = testContentKey()
        val wrongSecret = "wrong secret".toByteArray()

        val encrypted = EncryptedKey.lock(
            KeyDerivationMethod.Argon2id, testSecret, contentKey,
        )
        assertFailsWith<BcComponentsException> {
            encrypted.unlock(wrongSecret)
        }
    }
}

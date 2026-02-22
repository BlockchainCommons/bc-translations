package com.blockchaincommons.bccomponents

import com.blockchaincommons.bcrand.fakeRandomNumberGenerator
import kotlin.test.Test
import kotlin.test.assertEquals

/**
 * Tests for [X25519PrivateKey] and [X25519PublicKey].
 *
 * Covers key generation, public key derivation, key agreement, deterministic
 * derivation, and CBOR/UR roundtrip tests from the Rust `lib.rs` integration
 * tests (test_x25519_keys, test_agreement).
 */
class X25519Test {

    @Test
    fun testX25519Keys() {
        registerTags()
        val rng = fakeRandomNumberGenerator()

        val privateKey = X25519PrivateKey.createUsing(rng)
        val privateKeyUr = privateKey.urString()
        assertEquals(
            "ur:agreement-private-key/hdcxkbrehkrkrsjztodseytknecfgewmgdmwfsvdvysbpmghuozsprknfwkpnehydlweynwkrtct",
            privateKeyUr,
        )
        assertEquals(privateKey, X25519PrivateKey.fromUrString(privateKeyUr))

        val publicKey = privateKey.publicKey()
        val publicKeyUr = publicKey.urString()
        assertEquals(
            "ur:agreement-public-key/hdcxwnryknkbbymnoxhswmptgydsotwswsghfmrkksfxntbzjyrnuornkildchgswtdahehpwkrl",
            publicKeyUr,
        )
        assertEquals(publicKey, X25519PublicKey.fromUrString(publicKeyUr))

        val derivedPrivateKey =
            X25519PrivateKey.deriveFromKeyMaterial("password".toByteArray())
        assertEquals(
            "ur:agreement-private-key/hdcxkgcfkomeeyiemywkftvabnrdolmttlrnfhjnguvaiehlrldmdpemgyjlatdthsnecytdoxat",
            derivedPrivateKey.urString(),
        )
    }

    @Test
    fun testAgreement() {
        val rng = fakeRandomNumberGenerator()

        val alicePrivateKey = X25519PrivateKey.createUsing(rng)
        val alicePublicKey = alicePrivateKey.publicKey()

        val bobPrivateKey = X25519PrivateKey.createUsing(rng)
        val bobPublicKey = bobPrivateKey.publicKey()

        val aliceSharedKey = alicePrivateKey.sharedKeyWith(bobPublicKey)
        val bobSharedKey = bobPrivateKey.sharedKeyWith(alicePublicKey)

        assertEquals(aliceSharedKey, bobSharedKey)
    }

    @Test
    fun testPrivateKeyCborRoundtrip() {
        registerTags()
        val rng = fakeRandomNumberGenerator()
        val privateKey = X25519PrivateKey.createUsing(rng)
        val cbor = privateKey.taggedCbor()
        val decoded = X25519PrivateKey.fromTaggedCbor(cbor)
        assertEquals(privateKey, decoded)
    }

    @Test
    fun testPublicKeyCborRoundtrip() {
        registerTags()
        val rng = fakeRandomNumberGenerator()
        val privateKey = X25519PrivateKey.createUsing(rng)
        val publicKey = privateKey.publicKey()
        val cbor = publicKey.taggedCbor()
        val decoded = X25519PublicKey.fromTaggedCbor(cbor)
        assertEquals(publicKey, decoded)
    }

    @Test
    fun testKeypairUsing() {
        val rng = fakeRandomNumberGenerator()
        val (privateKey, publicKey) = X25519PrivateKey.keypairUsing(rng)
        assertEquals(publicKey, privateKey.publicKey())
    }
}

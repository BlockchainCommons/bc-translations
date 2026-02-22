package com.blockchaincommons.bccomponents

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

/**
 * Tests for the signing framework: [SigningPrivateKey], [SigningPublicKey],
 * [Signature], and [SignatureScheme].
 *
 * Based on Rust `signing/mod.rs` tests.
 */
class SigningTest {

    private val ecdsaPrivateKeyHex = "322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36"
    private val message = "Wolf McNally".toByteArray()

    @Test
    fun testSchnorrSigning() {
        val ecPrivKey = ECPrivateKey.fromHex(ecdsaPrivateKeyHex)
        val signingPrivKey = SigningPrivateKey.SchnorrKey(ecPrivKey)
        val sig = signingPrivKey.sign(message)
        assertTrue(signingPrivKey.verify(sig, message))

        val signingPubKey = signingPrivKey.publicKey()
        assertTrue(signingPubKey.verify(sig, message))
    }

    @Test
    fun testEcdsaSigning() {
        val ecPrivKey = ECPrivateKey.fromHex(ecdsaPrivateKeyHex)
        val signingPrivKey = SigningPrivateKey.ECDSAKey(ecPrivKey)
        val sig = signingPrivKey.sign(message)
        assertTrue(signingPrivKey.verify(sig, message))

        val signingPubKey = signingPrivKey.publicKey()
        assertTrue(signingPubKey.verify(sig, message))
    }

    @Test
    fun testEd25519Signing() {
        val (privKey, pubKey) = SignatureScheme.Ed25519.keypair()
        val sig = privKey.sign(message)
        assertTrue(privKey.verify(sig, message))
        assertTrue(pubKey.verify(sig, message))
    }

    @Test
    fun testSchnorrKeypair() {
        val (privKey, pubKey) = SignatureScheme.Schnorr.keypair()
        val sig = privKey.sign(message)
        assertTrue(pubKey.verify(sig, message))
    }

    @Test
    fun testEcdsaKeypair() {
        val (privKey, pubKey) = SignatureScheme.ECDSA.keypair()
        val sig = privKey.sign(message)
        assertTrue(pubKey.verify(sig, message))
    }

    @Test
    fun testEd25519Keypair() {
        val (privKey, pubKey) = SignatureScheme.Ed25519.keypair()
        val sig = privKey.sign(message)
        assertTrue(pubKey.verify(sig, message))
    }

    @Test
    fun testSignatureCborRoundtrip() {
        registerTags()
        val ecPrivKey = ECPrivateKey.fromHex(ecdsaPrivateKeyHex)

        // Schnorr
        val schnorrPriv = SigningPrivateKey.SchnorrKey(ecPrivKey)
        val schnorrSig = schnorrPriv.sign(message)
        val schnorrCbor = schnorrSig.taggedCbor()
        val schnorrDecoded = Signature.fromTaggedCbor(schnorrCbor)
        assertEquals(schnorrSig, schnorrDecoded)

        // ECDSA
        val ecdsaPriv = SigningPrivateKey.ECDSAKey(ecPrivKey)
        val ecdsaSig = ecdsaPriv.sign(message)
        val ecdsaCbor = ecdsaSig.taggedCbor()
        val ecdsaDecoded = Signature.fromTaggedCbor(ecdsaCbor)
        assertEquals(ecdsaSig, ecdsaDecoded)
    }

    @Test
    fun testSigningPrivateKeyCborRoundtrip() {
        registerTags()
        val ecPrivKey = ECPrivateKey.fromHex(ecdsaPrivateKeyHex)

        // Schnorr
        val schnorrKey = SigningPrivateKey.SchnorrKey(ecPrivKey)
        val schnorrDecoded = SigningPrivateKey.fromTaggedCbor(schnorrKey.taggedCbor())
        assertEquals(schnorrKey, schnorrDecoded)

        // ECDSA
        val ecdsaKey = SigningPrivateKey.ECDSAKey(ecPrivKey)
        val ecdsaDecoded = SigningPrivateKey.fromTaggedCbor(ecdsaKey.taggedCbor())
        assertEquals(ecdsaKey, ecdsaDecoded)
    }

    @Test
    fun testSigningPublicKeyCborRoundtrip() {
        registerTags()
        val ecPrivKey = ECPrivateKey.fromHex(ecdsaPrivateKeyHex)

        val schnorrPub = SigningPublicKey.SchnorrKey(ecPrivKey.schnorrPublicKey())
        val schnorrDecoded = SigningPublicKey.fromTaggedCbor(schnorrPub.taggedCbor())
        assertEquals(schnorrPub, schnorrDecoded)

        val ecdsaPub = SigningPublicKey.ECDSAKey(ecPrivKey.publicKey())
        val ecdsaDecoded = SigningPublicKey.fromTaggedCbor(ecdsaPub.taggedCbor())
        assertEquals(ecdsaPub, ecdsaDecoded)
    }
}

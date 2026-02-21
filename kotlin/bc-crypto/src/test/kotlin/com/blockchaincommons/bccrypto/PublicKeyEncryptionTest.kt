package com.blockchaincommons.bccrypto

import com.blockchaincommons.bcrand.fakeRandomNumberGenerator
import kotlin.test.Test
import kotlin.test.assertContentEquals

class PublicKeyEncryptionTest {

    @Test
    fun testX25519Keys() {
        val rng = fakeRandomNumberGenerator()
        val privateKey = x25519NewPrivateKeyUsing(rng)
        assertContentEquals(
            "7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed".hexToByteArray(),
            privateKey,
        )
        val publicKey = x25519PublicKeyFromPrivateKey(privateKey)
        assertContentEquals(
            "f1bd7a7e118ea461eba95126a3efef543ebb78439d1574bedcbe7d89174cf025".hexToByteArray(),
            publicKey,
        )

        val derivedAgreementKey = deriveAgreementPrivateKey("password".toByteArray())
        assertContentEquals(
            "7b19769132648ff43ae60cbaa696d5be3f6d53e6645db72e2d37516f0729619f".hexToByteArray(),
            derivedAgreementKey,
        )

        val derivedSigningKey = deriveSigningPrivateKey("password".toByteArray())
        assertContentEquals(
            "05cc550daa75058e613e606d9898fedf029e395911c43273a208b7e0e88e271b".hexToByteArray(),
            derivedSigningKey,
        )
    }

    @Test
    fun testKeyAgreement() {
        val rng = fakeRandomNumberGenerator()
        val alicePrivateKey = x25519NewPrivateKeyUsing(rng)
        val alicePublicKey = x25519PublicKeyFromPrivateKey(alicePrivateKey)
        val bobPrivateKey = x25519NewPrivateKeyUsing(rng)
        val bobPublicKey = x25519PublicKeyFromPrivateKey(bobPrivateKey)
        val aliceSharedKey = x25519SharedKey(alicePrivateKey, bobPublicKey)
        val bobSharedKey = x25519SharedKey(bobPrivateKey, alicePublicKey)
        assertContentEquals(aliceSharedKey, bobSharedKey)
        assertContentEquals(
            "1e9040d1ff45df4bfca7ef2b4dd2b11101b40d91bf5bf83f8c83d53f0fbb6c23".hexToByteArray(),
            aliceSharedKey,
        )
    }
}

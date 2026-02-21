package com.blockchaincommons.bccrypto

import com.blockchaincommons.bcrand.fakeRandomNumberGenerator
import kotlin.test.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertTrue

class EcdsaSigningTest {

    private val message = "Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it.".toByteArray()

    @Test
    fun testEcdsaSigning() {
        val rng = fakeRandomNumberGenerator()
        val privateKey = ecdsaNewPrivateKeyUsing(rng)
        val publicKey = ecdsaPublicKeyFromPrivateKey(privateKey)
        val signature = ecdsaSign(privateKey, message)
        assertContentEquals(
            "e75702ed8f645ce7fe510507b2403029e461ef4570d12aa440e4f81385546a13740b7d16878ff0b46b1cbe08bc218ccb0b00937b61c4707de2ca6148508e51fb".hexToByteArray(),
            signature,
        )
        assertTrue(ecdsaVerify(publicKey, signature, message))
    }
}

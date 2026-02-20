package com.blockchaincommons.bccrypto

import com.blockchaincommons.bcrand.makeFakeRandomNumberGenerator
import kotlin.test.Test
import kotlin.test.assertContentEquals

class EcdsaKeysTest {

    @Test
    fun testEcdsaKeys() {
        val rng = makeFakeRandomNumberGenerator()
        val privateKey = ecdsaNewPrivateKeyUsing(rng)
        assertContentEquals(
            "7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed".hexToByteArray(),
            privateKey,
        )
        val publicKey = ecdsaPublicKeyFromPrivateKey(privateKey)
        assertContentEquals(
            "0271b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b".hexToByteArray(),
            publicKey,
        )
        val decompressed = ecdsaDecompressPublicKey(publicKey)
        assertContentEquals(
            "0471b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b72325f1f3bb69a44d3f1cb6d1fd488220dd502f49c0b1a46cb91ce3718d8334a".hexToByteArray(),
            decompressed,
        )
        val compressed = ecdsaCompressPublicKey(decompressed)
        assertContentEquals(publicKey, compressed)
        val xOnlyPublicKey = schnorrPublicKeyFromPrivateKey(privateKey)
        assertContentEquals(
            "71b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b".hexToByteArray(),
            xOnlyPublicKey,
        )

        val derivedPrivateKey = ecdsaDerivePrivateKey("password".toByteArray())
        assertContentEquals(
            "05cc550daa75058e613e606d9898fedf029e395911c43273a208b7e0e88e271b".hexToByteArray(),
            derivedPrivateKey,
        )
    }
}

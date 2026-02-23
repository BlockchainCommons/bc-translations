package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.SignatureScheme
import com.blockchaincommons.bccomponents.SigningOptions
// registerTags() from bc-envelope package initializes GlobalFormatContext
import kotlin.test.Test

class KeypairSigningTest {

    private fun testScheme(scheme: SignatureScheme, options: SigningOptions? = null) {
        val (privateKey, publicKey) = scheme.keypair()
        val envelope = helloEnvelope()
            .signOpt(privateKey, options)
            .checkEncoding()
        envelope.verify(publicKey)
    }

    @Test
    fun testKeypairSigning() {
        registerTags()

        testScheme(SignatureScheme.Schnorr)
        testScheme(SignatureScheme.ECDSA)
        testScheme(SignatureScheme.Ed25519)
        testScheme(SignatureScheme.MLDSA44)
        testScheme(SignatureScheme.MLDSA65)
        testScheme(SignatureScheme.MLDSA87)
    }
}

package com.blockchaincommons.bcenvelope

import kotlin.test.Test
import kotlin.test.assertEquals

class CompressionTest {

    private val source = "Lorem ipsum dolor sit amet consectetur adipiscing elit mi nibh ornare proin blandit diam ridiculus, faucibus mus dui eu vehicula nam donec dictumst sed vivamus bibendum aliquet efficitur. Felis imperdiet sodales dictum morbi vivamus augue dis duis aliquet velit ullamcorper porttitor, lobortis dapibus hac purus aliquam natoque iaculis blandit montes nunc pretium."

    @Test
    fun testCompress() {
        val original = Envelope.from(source)
        assertEquals(371, original.taggedCbor().toCborData().size)
        val compressed = original.compress().checkEncoding()
        assertEquals(283, compressed.taggedCbor().toCborData().size)

        assertEquals(original.digest(), compressed.digest())
        val decompressed = compressed.decompress().checkEncoding()
        assertEquals(decompressed.digest(), original.digest())
        assertEquals(decompressed.structuralDigest(), original.structuralDigest())
    }

    @Test
    fun testCompressSubject() {
        val original = Envelope.from("Alice")
            .addAssertion(com.blockchaincommons.knownvalues.NOTE, source)
            .wrap()
            .addSignature(alicePrivateKey())
        val compressed = original.compressSubject().checkEncoding()
        val decompressed = compressed.decompressSubject().checkEncoding()
        assertEquals(decompressed.digest(), original.digest())
        assertEquals(decompressed.structuralDigest(), original.structuralDigest())
    }
}

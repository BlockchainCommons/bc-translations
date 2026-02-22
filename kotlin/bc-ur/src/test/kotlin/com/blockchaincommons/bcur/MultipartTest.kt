package com.blockchaincommons.bcur

import com.blockchaincommons.dcbor.Cbor
import kotlin.test.Test
import kotlin.test.assertEquals

class MultipartTest {

    @Test
    fun testMultipartUr() {
        val urObj = UR.create("bytes", Cbor.fromByteString(Xoshiro256.makeMessage("Wolf", 32767)))
        val encoder = MultipartEncoder(urObj, 1000)
        val decoder = MultipartDecoder()

        while (!decoder.isComplete) {
            assertEquals(null, decoder.message())
            decoder.receive(encoder.nextPart())
        }

        val decoded = decoder.message()!!
        assertEquals(urObj, decoded)
    }

    @Test
    fun testFountain() {
        assertEquals(5, runFountainTest(1))
        assertEquals(61, runFountainTest(51))
        assertEquals(110, runFountainTest(101))
        assertEquals(507, runFountainTest(501))
    }

    private fun runFountainTest(startPart: Int): Int {
        val message = "The only thing we have to fear is fear itself."
        val cbor = Cbor.fromByteString(message.toByteArray())
        val ur = UR.create("bytes", cbor)

        val encoder = MultipartEncoder(ur, 10)
        val decoder = MultipartDecoder()

        for (i in 1..1000) {
            val part = encoder.nextPart()
            if (encoder.currentIndex >= startPart) {
                decoder.receive(part)
            }
            if (decoder.isComplete) break
        }

        val receivedUr = decoder.message()!!
        assertEquals(ur, receivedUr)
        return encoder.currentIndex
    }
}

@file:OptIn(ExperimentalStdlibApi::class)

package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bcrand.fakeRandomBytes
import com.blockchaincommons.bcrand.fakeRandomNumberGenerator
import com.blockchaincommons.bcrand.nextInClosedRange
import com.blockchaincommons.dcbor.CborDate
import com.blockchaincommons.knownvalues.KnownValue
import com.blockchaincommons.knownvalues.SIGNED
import kotlin.test.Test
import kotlin.test.assertEquals

class TypeTest {

    @Test
    fun testKnownValue() {
        val envelope = SIGNED.toEnvelope().checkEncoding()
        assertEquals("'signed'", envelope.format())
        assertEquals(
            "Digest(d0e39e788c0d8f0343af4588db21d3d51381db454bdf710a9a1891aaa537693c)",
            envelope.digest().toString()
        )
    }

    @Test
    fun testDate() {
        val date = CborDate.fromString("2018-01-07")
        val envelope = Envelope.from(date).checkEncoding()
        assertEquals("2018-01-07", envelope.format())
    }

    @Test
    fun testFakeRandomData() {
        assertEquals(
            "7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed518684c556472008a67932f7c682125b50cb72e8216f6906358fdaf28d3545532daee0c5bb5023f50cd8e71ec14901ac746c576c481b893be6656b80622b3a564e59b4e2",
            fakeRandomBytes(100).toHexString()
        )
    }

    @Test
    fun testFakeNumbers() {
        val rng = fakeRandomNumberGenerator()
        val array = (0 until 100).map { rng.nextInClosedRange(-50L, 50L, bits = 32).toInt() }
        assertEquals(
            "[-43, -6, 43, -34, -34, 17, -9, 24, 17, -29, -32, -44, 12, -15, -46, 20, 50, -31, -50, 36, -28, -23, 6, -27, -31, -45, -27, 26, 31, -23, 24, 19, -32, 43, -18, -17, 6, -13, -1, -27, 4, -48, -4, -44, -6, 17, -15, 22, 15, 20, -25, -35, -33, -27, -17, -44, -27, 15, -14, -38, -29, -12, 8, 43, 49, -42, -11, -1, -42, -26, -25, 22, -13, 14, 42, -29, -38, 17, 2, 5, 5, -31, 27, -3, 39, -12, 42, 46, -17, -25, -46, -19, 16, 2, -45, 41, 12, -22, 43, -11]",
            array.toString()
        )
    }
}

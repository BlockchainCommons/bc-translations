package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.Digest
import com.blockchaincommons.dcbor.Cbor
import kotlin.test.Test
import kotlin.test.assertEquals

class CoreEncodingTest {

    @Test
    fun testDigest() {
        Envelope.from(Digest.fromImage("Hello.".toByteArray()))
            .checkEncoding()
    }

    @Test
    fun test1() {
        val e = Envelope.from("Hello.")

        assertEquals(
            """
            200(   / envelope /
                201("Hello.")   / leaf /
            )
            """.trimIndent(),
            e.diagnosticAnnotated()
        )
    }

    @Test
    fun test2() {
        val array = listOf(Cbor.fromUnsigned(1uL), Cbor.fromUnsigned(2uL), Cbor.fromUnsigned(3uL))
        val e = Envelope.from(Cbor.fromArray(array))

        assertEquals(
            """
            200(   / envelope /
                201(   / leaf /
                    [1, 2, 3]
                )
            )
            """.trimIndent(),
            e.diagnosticAnnotated()
        )
    }

    @Test
    fun test3() {
        val e1 = Envelope.newAssertion("A", "B").checkEncoding()
        val e2 = Envelope.newAssertion("C", "D").checkEncoding()
        val e3 = Envelope.newAssertion("E", "F").checkEncoding()

        val e4 = e2.addAssertionEnvelope(e3)
        assertEquals(
            """
            {
                "C": "D"
            } [
                "E": "F"
            ]
            """.trimIndent(),
            e4.format()
        )

        assertEquals(
            """
            200(   / envelope /
                [
                    {
                        201("C"):   / leaf /
                        201("D")   / leaf /
                    },
                    {
                        201("E"):   / leaf /
                        201("F")   / leaf /
                    }
                ]
            )
            """.trimIndent(),
            e4.diagnosticAnnotated()
        )

        e4.checkEncoding()

        val e5 = e1.addAssertionEnvelope(e4).checkEncoding()

        assertEquals(
            """
            {
                "A": "B"
            } [
                {
                    "C": "D"
                } [
                    "E": "F"
                ]
            ]
            """.trimIndent(),
            e5.format()
        )

        assertEquals(
            """
            200(   / envelope /
                [
                    {
                        201("A"):   / leaf /
                        201("B")   / leaf /
                    },
                    [
                        {
                            201("C"):   / leaf /
                            201("D")   / leaf /
                        },
                        {
                            201("E"):   / leaf /
                            201("F")   / leaf /
                        }
                    ]
                ]
            )
            """.trimIndent(),
            e5.diagnosticAnnotated()
        )
    }
}

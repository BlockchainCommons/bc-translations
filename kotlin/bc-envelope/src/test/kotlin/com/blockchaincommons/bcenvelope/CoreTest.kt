@file:OptIn(ExperimentalStdlibApi::class)

package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.Digest
import com.blockchaincommons.knownvalues.KnownValue
import com.blockchaincommons.knownvalues.NOTE
import com.blockchaincommons.knownvalues.UNIT
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class CoreTest {

    @Test
    fun testReadLegacyLeaf() {
        val legacyEnvelope = Envelope.fromTaggedCborData(
            "d8c8d818182a".hexToByteArray()
        )
        val e = Envelope.from(42)
        assertTrue(legacyEnvelope.isIdenticalTo(e))
        assertTrue(legacyEnvelope.isEquivalentTo(e))
    }

    @Test
    fun testIntSubject() {
        val e = Envelope.from(42).checkEncoding()

        assertEquals(
            """
            200(   / envelope /
                201(42)   / leaf /
            )
            """.trimIndent(),
            e.diagnosticAnnotated()
        )

        assertEquals(
            "Digest(7f83f7bda2d63959d34767689f06d47576683d378d9eb8d09386c9a020395c53)",
            e.digest().toString()
        )

        assertEquals("42", e.format())
        assertEquals(42, e.extractSubject<Int>())
    }

    @Test
    fun testNegativeIntSubject() {
        val e = Envelope.from(-42).checkEncoding()

        assertEquals(
            """
            200(   / envelope /
                201(-42)   / leaf /
            )
            """.trimIndent(),
            e.diagnosticAnnotated()
        )

        assertEquals(
            "Digest(9e0ad272780de7aa1dbdfbc99058bb81152f623d3b95b5dfb0a036badfcc9055)",
            e.digest().toString()
        )

        assertEquals("-42", e.format())
        assertEquals(-42, e.extractSubject<Int>())
    }

    @Test
    fun testCborEncodableSubject() {
        val e = helloEnvelope().checkEncoding()

        assertEquals(
            """
            200(   / envelope /
                201("Hello.")   / leaf /
            )
            """.trimIndent(),
            e.diagnosticAnnotated()
        )

        assertEquals(
            "Digest(8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59)",
            e.digest().toString()
        )

        assertEquals("\"Hello.\"", e.format())
        assertEquals(PLAINTEXT_HELLO, e.extractSubject<String>())
    }

    @Test
    fun testKnownValueSubject() {
        val e = knownValueEnvelope().checkEncoding()

        assertEquals(
            """
            200(4)   / envelope /
            """.trimIndent(),
            e.diagnosticAnnotated()
        )

        assertEquals(
            "Digest(0fcd6a39d6ed37f2e2efa6a96214596f1b28a5cd42a5a27afc32162aaf821191)",
            e.digest().toString()
        )

        assertEquals("'note'", e.format())
        assertEquals(NOTE, e.extractSubject<KnownValue>())
    }

    @Test
    fun testAssertionSubject() {
        val e = assertionEnvelope().checkEncoding()

        assertEquals(
            "Digest(db7dd21c5169b4848d2a1bcb0a651c9617cdd90bae29156baaefbb2a8abef5ba)",
            e.asPredicate()!!.digest().toString()
        )
        assertEquals(
            "Digest(13b741949c37b8e09cc3daa3194c58e4fd6b2f14d4b1d0f035a46d6d5a1d3f11)",
            e.asObject()!!.digest().toString()
        )
        assertEquals(
            "Digest(78d666eb8f4c0977a0425ab6aa21ea16934a6bc97c6f0c3abaefac951c1714a2)",
            e.subject().digest().toString()
        )
        assertEquals(
            "Digest(78d666eb8f4c0977a0425ab6aa21ea16934a6bc97c6f0c3abaefac951c1714a2)",
            e.digest().toString()
        )

        assertEquals(
            """
            200(   / envelope /
                {
                    201("knows"):   / leaf /
                    201("Bob")   / leaf /
                }
            )
            """.trimIndent(),
            e.diagnosticAnnotated()
        )

        assertEquals("\"knows\": \"Bob\"", e.format())
        assertEquals(e.digest(), Envelope.newAssertion("knows", "Bob").digest())
    }

    @Test
    fun testSubjectWithAssertion() {
        val e = singleAssertionEnvelope().checkEncoding()

        assertEquals(
            """
            200(   / envelope /
                [
                    201("Alice"),   / leaf /
                    {
                        201("knows"):   / leaf /
                        201("Bob")   / leaf /
                    }
                ]
            )
            """.trimIndent(),
            e.diagnosticAnnotated()
        )

        assertEquals(
            "Digest(8955db5e016affb133df56c11fe6c5c82fa3036263d651286d134c7e56c0e9f2)",
            e.digest().toString()
        )

        assertEquals(
            """
            "Alice" [
                "knows": "Bob"
            ]
            """.trimIndent(),
            e.format()
        )

        assertEquals("Alice", e.extractSubject<String>())
    }

    @Test
    fun testSubjectWithTwoAssertions() {
        val e = doubleAssertionEnvelope().checkEncoding()

        assertEquals(
            """
            200(   / envelope /
                [
                    201("Alice"),   / leaf /
                    {
                        201("knows"):   / leaf /
                        201("Carol")   / leaf /
                    },
                    {
                        201("knows"):   / leaf /
                        201("Bob")   / leaf /
                    }
                ]
            )
            """.trimIndent(),
            e.diagnosticAnnotated()
        )

        assertEquals(
            "Digest(b8d857f6e06a836fbc68ca0ce43e55ceb98eefd949119dab344e11c4ba5a0471)",
            e.digest().toString()
        )

        assertEquals(
            """
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
            ]
            """.trimIndent(),
            e.format()
        )

        assertEquals("Alice", e.extractSubject<String>())
    }

    @Test
    fun testWrapped() {
        val e = wrappedEnvelope().checkEncoding()

        assertEquals(
            """
            200(   / envelope /
                200(   / envelope /
                    201("Hello.")   / leaf /
                )
            )
            """.trimIndent(),
            e.diagnosticAnnotated()
        )

        assertEquals(
            "Digest(172a5e51431062e7b13525cbceb8ad8475977444cf28423e21c0d1dcbdfcaf47)",
            e.digest().toString()
        )

        assertEquals(
            """
            {
                "Hello."
            }
            """.trimIndent(),
            e.format()
        )
    }

    @Test
    fun testDoubleWrapped() {
        val e = doubleWrappedEnvelope().checkEncoding()

        assertEquals(
            """
            200(   / envelope /
                200(   / envelope /
                    200(   / envelope /
                        201("Hello.")   / leaf /
                    )
                )
            )
            """.trimIndent(),
            e.diagnosticAnnotated()
        )

        assertEquals(
            "Digest(8b14f3bcd7c05aac8f2162e7047d7ef5d5eab7d82ee3f9dc4846c70bae4d200b)",
            e.digest().toString()
        )

        assertEquals(
            """
            {
                {
                    "Hello."
                }
            }
            """.trimIndent(),
            e.format()
        )
    }

    @Test
    fun testAssertionWithAssertions() {
        val a = Envelope.newAssertion(1, 2)
            .addAssertion(3, 4)
            .addAssertion(5, 6)
        val e = Envelope.from(7).addAssertionEnvelope(a)

        assertEquals(
            """
            7 [
                {
                    1: 2
                } [
                    3: 4
                    5: 6
                ]
            ]
            """.trimIndent(),
            e.format()
        )
    }

    @Test
    fun testDigestLeaf() {
        val digest = helloEnvelope().digest()
        val e = Envelope.from(digest).checkEncoding()

        assertEquals("Digest(8cc96cdb)", e.format())

        assertEquals(
            "Digest(07b518af92a6196bc153752aabefedb34ff8e1a7d820c01ef978dfc3e7e52e05)",
            e.digest().toString()
        )

        assertEquals(
            """
            200(   / envelope /
                201(   / leaf /
                    40001(   / digest /
                        h'8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59'
                    )
                )
            )
            """.trimIndent(),
            e.diagnosticAnnotated()
        )
    }

    @Test
    fun testTrue() {
        registerTags()
        val e = Envelope.from(true).checkEncoding()
        assertTrue(e.isBool())
        assertTrue(e.isTrue())
        assertFalse(e.isFalse())
        assertEquals(e, Envelope.true_())
        assertEquals("true", e.format())
    }

    @Test
    fun testFalse() {
        registerTags()
        val e = Envelope.from(false).checkEncoding()
        assertTrue(e.isBool())
        assertFalse(e.isTrue())
        assertTrue(e.isFalse())
        assertEquals(e, Envelope.false_())
        assertEquals("false", e.format())
    }

    @Test
    fun testUnit() {
        registerTags()
        var e = Envelope.unit().checkEncoding()
        assertTrue(e.isSubjectUnit())
        assertEquals("''", e.format())

        e = e.addAssertion("foo", "bar")
        assertTrue(e.isSubjectUnit())
        assertEquals(
            """
            '' [
                "foo": "bar"
            ]
            """.trimIndent(),
            e.format()
        )

        val subject = e.extractSubject<KnownValue>()
        assertEquals(UNIT, subject)
    }

    @Test
    fun testUnknownLeaf() {
        registerTags()

        val unknownUr =
            "ur:envelope/tpsotaaodnoyadgdjlssmkcklgoskseodnyteofwwfylkiftaydpdsjz"
        val e = Envelope.fromUrString(unknownUr)
        val expected = "555({1: h'6fc4981e8da778332bf93342f3f77d3a'})"
        assertEquals(expected, e.format())
    }

    @Test
    fun testPosition() {
        registerTags()

        var e = Envelope.from("Hello")
        assertTrue(
            try { e.position(); false } catch (_: Exception) { true }
        )

        e = e.setPosition(42)
        assertEquals(42, e.position())
        assertEquals(
            """
            "Hello" [
                'position': 42
            ]
            """.trimIndent(),
            e.format()
        )

        e = e.setPosition(0)
        assertEquals(0, e.position())
        assertEquals(
            """
            "Hello" [
                'position': 0
            ]
            """.trimIndent(),
            e.format()
        )

        e = e.removePosition()
        assertTrue(
            try { e.position(); false } catch (_: Exception) { true }
        )
        assertEquals("\"Hello\"", e.format())
    }
}

package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bcrand.fakeRandomNumberGenerator
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class NonCorrelationTest {

    @Test
    fun testEnvelopeNonCorrelation() {
        val e1 = Envelope.from("Hello.")
        assertTrue(e1.isEquivalentTo(e1.elide()))

        val rng = fakeRandomNumberGenerator()
        val e2 = e1.addSaltUsing(rng).checkEncoding()

        assertEquals(
            """
            "Hello." [
                'salt': Salt
            ]
            """.trimIndent(),
            e2.format()
        )

        assertFalse(e1.isEquivalentTo(e2))
        assertFalse(e1.isEquivalentTo(e2.elide()))
    }

    @Test
    fun testPredicateCorrelation() {
        val e1 = Envelope.from("Foo")
            .addAssertion("note", "Bar")
            .checkEncoding()
        val e2 = Envelope.from("Baz")
            .addAssertion("note", "Quux")
            .checkEncoding()

        assertEquals(
            """
            "Foo" [
                "note": "Bar"
            ]
            """.trimIndent(),
            e1.format()
        )

        // e1 and e2 have the same predicate
        assertTrue(
            e1.assertions().first().asPredicate()!!
                .isEquivalentTo(
                    e2.assertions().first().asPredicate()!!
                )
        )

        val e1Elided = e1.elideRevealingTarget(e1).checkEncoding()
        assertEquals(
            """
            ELIDED [
                ELIDED
            ]
            """.trimIndent(),
            e1Elided.format()
        )
    }

    @Test
    fun testAddSalt() {
        val source = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
        val e1 = Envelope.from("Alpha")
            .addSalt()
            .checkEncoding()
            .wrap()
            .checkEncoding()
            .addAssertion(
                Envelope.from(com.blockchaincommons.knownvalues.NOTE)
                    .addSalt()
                    .checkEncoding(),
                Envelope.from(source)
                    .addSalt()
                    .checkEncoding(),
            )
            .checkEncoding()

        val e1Elided = e1.elideRevealingTarget(e1).checkEncoding()
        assertEquals(
            """
            ELIDED [
                ELIDED
            ]
            """.trimIndent(),
            e1Elided.format()
        )
    }
}

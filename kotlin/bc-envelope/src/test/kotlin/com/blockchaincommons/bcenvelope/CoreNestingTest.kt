package com.blockchaincommons.bcenvelope

import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

class CoreNestingTest {

    @Test
    fun testPredicateEnclosures() {
        val alice = Envelope.from("Alice")
        val knows = Envelope.from("knows")
        val bob = Envelope.from("Bob")

        val a = Envelope.from("A")
        val b = Envelope.from("B")

        val knowsBob = Envelope.newAssertion(knows, bob)
        assertEquals("\"knows\": \"Bob\"", knowsBob.format())

        val ab = Envelope.newAssertion(a, b)
        assertEquals("\"A\": \"B\"", ab.format())

        val knowsAbBob = Envelope.newAssertion(
            knows.addAssertionEnvelope(ab),
            bob,
        ).checkEncoding()
        assertEquals(
            """
            "knows" [
                "A": "B"
            ]
            : "Bob"
            """.trimIndent(),
            knowsAbBob.format()
        )

        val knowsBobAb = Envelope.newAssertion(
            knows,
            bob.addAssertionEnvelope(ab),
        ).checkEncoding()
        assertEquals(
            """
            "knows": "Bob" [
                "A": "B"
            ]
            """.trimIndent(),
            knowsBobAb.format()
        )

        val knowsBobEncloseAb = knowsBob
            .addAssertionEnvelope(ab)
            .checkEncoding()
        assertEquals(
            """
            {
                "knows": "Bob"
            } [
                "A": "B"
            ]
            """.trimIndent(),
            knowsBobEncloseAb.format()
        )

        val aliceKnowsBob = alice
            .addAssertionEnvelope(knowsBob)
            .checkEncoding()
        assertEquals(
            """
            "Alice" [
                "knows": "Bob"
            ]
            """.trimIndent(),
            aliceKnowsBob.format()
        )

        val aliceAbKnowsBob = aliceKnowsBob
            .addAssertionEnvelope(ab)
            .checkEncoding()
        assertEquals(
            """
            "Alice" [
                "A": "B"
                "knows": "Bob"
            ]
            """.trimIndent(),
            aliceAbKnowsBob.format()
        )

        val aliceKnowsAbBob = alice
            .addAssertionEnvelope(
                Envelope.newAssertion(
                    knows.addAssertionEnvelope(ab),
                    bob,
                )
            )
            .checkEncoding()
        assertEquals(
            """
            "Alice" [
                "knows" [
                    "A": "B"
                ]
                : "Bob"
            ]
            """.trimIndent(),
            aliceKnowsAbBob.format()
        )

        val aliceKnowsBobAb = alice
            .addAssertionEnvelope(
                Envelope.newAssertion(
                    knows,
                    bob.addAssertionEnvelope(ab),
                )
            )
            .checkEncoding()
        assertEquals(
            """
            "Alice" [
                "knows": "Bob" [
                    "A": "B"
                ]
            ]
            """.trimIndent(),
            aliceKnowsBobAb.format()
        )

        val aliceKnowsAbBobAb = alice
            .addAssertionEnvelope(
                Envelope.newAssertion(
                    knows.addAssertionEnvelope(ab),
                    bob.addAssertionEnvelope(ab),
                )
            )
            .checkEncoding()
        assertEquals(
            """
            "Alice" [
                "knows" [
                    "A": "B"
                ]
                : "Bob" [
                    "A": "B"
                ]
            ]
            """.trimIndent(),
            aliceKnowsAbBobAb.format()
        )
    }

    @Test
    fun testNestingPlaintext() {
        val envelope = Envelope.from("Hello.")
        assertEquals("\"Hello.\"", envelope.format())

        val elidedEnvelope = envelope.elide()
        assertTrue(elidedEnvelope.isEquivalentTo(envelope))
        assertEquals("ELIDED", elidedEnvelope.format())
    }

    @Test
    fun testNestingOnce() {
        val envelope = Envelope.from("Hello.").wrap().checkEncoding()
        assertEquals(
            """
            {
                "Hello."
            }
            """.trimIndent(),
            envelope.format()
        )

        val elidedEnvelope = Envelope.from("Hello.")
            .elide()
            .wrap()
            .checkEncoding()
        assertTrue(elidedEnvelope.isEquivalentTo(envelope))
        assertEquals(
            """
            {
                ELIDED
            }
            """.trimIndent(),
            elidedEnvelope.format()
        )
    }

    @Test
    fun testNestingTwice() {
        val envelope = Envelope.from("Hello.")
            .wrap()
            .wrap()
            .checkEncoding()
        assertEquals(
            """
            {
                {
                    "Hello."
                }
            }
            """.trimIndent(),
            envelope.format()
        )

        val target = envelope.unwrap().unwrap()
        val elidedEnvelope = envelope.elideRemovingTarget(target)
        assertEquals(
            """
            {
                {
                    ELIDED
                }
            }
            """.trimIndent(),
            elidedEnvelope.format()
        )
        assertTrue(envelope.isEquivalentTo(elidedEnvelope))
    }

    @Test
    fun testAssertionsOnAllPartsOfEnvelope() {
        val predicate = Envelope.from("predicate")
            .addAssertion("predicate-predicate", "predicate-object")
        val objectEnv = Envelope.from("object")
            .addAssertion("object-predicate", "object-object")
        val envelope = Envelope.from("subject")
            .addAssertion(predicate, objectEnv)
            .checkEncoding()

        assertEquals(
            """
            "subject" [
                "predicate" [
                    "predicate-predicate": "predicate-object"
                ]
                : "object" [
                    "object-predicate": "object-object"
                ]
            ]
            """.trimIndent(),
            envelope.format()
        )
    }

    @Test
    fun testAssertionOnBareAssertion() {
        val envelope = Envelope.newAssertion("predicate", "object")
            .addAssertion("assertion-predicate", "assertion-object")

        assertEquals(
            """
            {
                "predicate": "object"
            } [
                "assertion-predicate": "assertion-object"
            ]
            """.trimIndent(),
            envelope.format()
        )
    }
}

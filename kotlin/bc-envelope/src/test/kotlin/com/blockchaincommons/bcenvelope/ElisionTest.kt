package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.Digest
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class ElisionTest {

    private fun basicEnvelope(): Envelope = Envelope.from("Hello.")

    private fun assertionEnv(): Envelope = Envelope.newAssertion("knows", "Bob")

    private fun singleAssertionEnv(): Envelope =
        Envelope.from("Alice").addAssertion("knows", "Bob")

    private fun doubleAssertionEnv(): Envelope =
        Envelope.from("Alice")
            .addAssertion("knows", "Bob")
            .addAssertion("knows", "Carol")

    @Test
    fun testEnvelopeElision() {
        val e1 = basicEnvelope()
        val e2 = e1.elide()
        assertTrue(e1.isEquivalentTo(e2))
        assertFalse(e1.isIdenticalTo(e2))
        assertEquals("ELIDED", e2.format())
        assertEquals(
            """
            200(   / envelope /
                h'8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59'
            )
            """.trimIndent(),
            e2.diagnosticAnnotated()
        )
        val e3 = e2.unelide(e1)
        assertTrue(e3.isEquivalentTo(e1))
        assertEquals("\"Hello.\"", e3.format())
    }

    @Test
    fun testSingleAssertionRemoveElision() {
        val e1 = singleAssertionEnv()
        assertEquals(
            """
            "Alice" [
                "knows": "Bob"
            ]
            """.trimIndent(),
            e1.format()
        )

        // Elide the entire envelope
        val e2 = e1.elideRemovingTarget(e1).checkEncoding()
        assertEquals("ELIDED", e2.format())

        // Elide just subject
        val e3 = e1.elideRemovingTarget("Alice".toEnvelope()).checkEncoding()
        assertEquals(
            """
            ELIDED [
                "knows": "Bob"
            ]
            """.trimIndent(),
            e3.format()
        )

        // Elide just predicate
        val e4 = e1.elideRemovingTarget("knows".toEnvelope()).checkEncoding()
        assertEquals(
            """
            "Alice" [
                ELIDED: "Bob"
            ]
            """.trimIndent(),
            e4.format()
        )

        // Elide just object
        val e5 = e1.elideRemovingTarget("Bob".toEnvelope()).checkEncoding()
        assertEquals(
            """
            "Alice" [
                "knows": ELIDED
            ]
            """.trimIndent(),
            e5.format()
        )

        // Elide the entire assertion
        val e6 = e1.elideRemovingTarget(assertionEnv()).checkEncoding()
        assertEquals(
            """
            "Alice" [
                ELIDED
            ]
            """.trimIndent(),
            e6.format()
        )
    }

    @Test
    fun testDoubleAssertionRemoveElision() {
        val e1 = doubleAssertionEnv()
        assertEquals(
            """
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
            ]
            """.trimIndent(),
            e1.format()
        )

        val e2 = e1.elideRemovingTarget(e1).checkEncoding()
        assertEquals("ELIDED", e2.format())

        val e3 = e1.elideRemovingTarget("Alice".toEnvelope()).checkEncoding()
        assertEquals(
            """
            ELIDED [
                "knows": "Bob"
                "knows": "Carol"
            ]
            """.trimIndent(),
            e3.format()
        )

        val e4 = e1.elideRemovingTarget("knows".toEnvelope()).checkEncoding()
        assertEquals(
            """
            "Alice" [
                ELIDED: "Bob"
                ELIDED: "Carol"
            ]
            """.trimIndent(),
            e4.format()
        )

        val e5 = e1.elideRemovingTarget("Bob".toEnvelope()).checkEncoding()
        assertEquals(
            """
            "Alice" [
                "knows": "Carol"
                "knows": ELIDED
            ]
            """.trimIndent(),
            e5.format()
        )

        val e6 = e1.elideRemovingTarget(assertionEnv()).checkEncoding()
        assertEquals(
            """
            "Alice" [
                "knows": "Carol"
                ELIDED
            ]
            """.trimIndent(),
            e6.format()
        )
    }

    @Test
    fun testSingleAssertionRevealElision() {
        val e1 = singleAssertionEnv()

        val e2 = e1.elideRevealingArray(emptyList()).checkEncoding()
        assertEquals("ELIDED", e2.format())

        val e3 = e1.elideRevealingArray(listOf(e1)).checkEncoding()
        assertEquals(
            """
            ELIDED [
                ELIDED
            ]
            """.trimIndent(),
            e3.format()
        )

        val e4 = e1.elideRevealingArray(listOf(e1, "Alice".toEnvelope())).checkEncoding()
        assertEquals(
            """
            "Alice" [
                ELIDED
            ]
            """.trimIndent(),
            e4.format()
        )

        val e5 = e1.elideRevealingArray(listOf(e1, assertionEnv())).checkEncoding()
        assertEquals(
            """
            ELIDED [
                ELIDED: ELIDED
            ]
            """.trimIndent(),
            e5.format()
        )

        val e6 = e1.elideRevealingArray(
            listOf(e1, assertionEnv(), "knows".toEnvelope())
        ).checkEncoding()
        assertEquals(
            """
            ELIDED [
                "knows": ELIDED
            ]
            """.trimIndent(),
            e6.format()
        )

        val e7 = e1.elideRevealingArray(
            listOf(e1, assertionEnv(), "Bob".toEnvelope())
        ).checkEncoding()
        assertEquals(
            """
            ELIDED [
                ELIDED: "Bob"
            ]
            """.trimIndent(),
            e7.format()
        )
    }

    @Test
    fun testDigests() {
        val e1 = doubleAssertionEnv()

        val e2 = e1.elideRevealingSet(e1.digests(0)).checkEncoding()
        assertEquals("ELIDED", e2.format())

        val e3 = e1.elideRevealingSet(e1.digests(1)).checkEncoding()
        assertEquals(
            """
            "Alice" [
                ELIDED (2)
            ]
            """.trimIndent(),
            e3.format()
        )

        val e4 = e1.elideRevealingSet(e1.digests(2)).checkEncoding()
        assertEquals(
            """
            "Alice" [
                ELIDED: ELIDED
                ELIDED: ELIDED
            ]
            """.trimIndent(),
            e4.format()
        )

        val e5 = e1.elideRevealingSet(e1.digests(3)).checkEncoding()
        assertEquals(
            """
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
            ]
            """.trimIndent(),
            e5.format()
        )
    }

    @Test
    fun testTargetReveal() {
        val e1 = doubleAssertionEnv().addAssertion("livesAt", "123 Main St.")

        val target = mutableSetOf<Digest>()
        target.addAll(e1.digests(1))
        target.addAll(e1.subject().deepDigests())
        target.addAll(assertionEnv().deepDigests())
        target.addAll(e1.assertionWithPredicate("livesAt").deepDigests())
        val e2 = e1.elideRevealingSet(target).checkEncoding()
        assertEquals(
            """
            "Alice" [
                "knows": "Bob"
                "livesAt": "123 Main St."
                ELIDED
            ]
            """.trimIndent(),
            e2.format()
        )
    }

    @Test
    fun testTargetedRemove() {
        val e1 = doubleAssertionEnv().addAssertion("livesAt", "123 Main St.")

        val target2 = assertionEnv().digests(1).toMutableSet()
        val e2 = e1.elideRemovingSet(target2).checkEncoding()
        assertEquals(
            """
            "Alice" [
                "knows": "Carol"
                "livesAt": "123 Main St."
                ELIDED
            ]
            """.trimIndent(),
            e2.format()
        )

        val target3 = e1.assertionWithPredicate("livesAt").deepDigests().toMutableSet()
        val e3 = e1.elideRemovingSet(target3).checkEncoding()
        assertEquals(
            """
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
                ELIDED
            ]
            """.trimIndent(),
            e3.format()
        )

        assertTrue(e1.isEquivalentTo(e3))
        assertFalse(e1.isIdenticalTo(e3))
    }

    @Test
    fun testDoubleAssertionRevealElision() {
        val e1 = doubleAssertionEnv()
        assertEquals(
            """
            "Alice" [
                "knows": "Bob"
                "knows": "Carol"
            ]
            """.trimIndent(),
            e1.format()
        )

        // Elide revealing nothing
        val e2 = e1.elideRevealingArray(emptyList()).checkEncoding()
        assertEquals("ELIDED", e2.format())

        // Reveal just the envelope's structure
        val e3 = e1.elideRevealingArray(listOf(e1)).checkEncoding()
        assertEquals(
            """
            ELIDED [
                ELIDED (2)
            ]
            """.trimIndent(),
            e3.format()
        )

        // Reveal just the envelope's subject
        val e4 = e1.elideRevealingArray(listOf(e1, "Alice".toEnvelope())).checkEncoding()
        assertEquals(
            """
            "Alice" [
                ELIDED (2)
            ]
            """.trimIndent(),
            e4.format()
        )

        // Reveal just the assertion's structure
        val e5 = e1.elideRevealingArray(listOf(e1, assertionEnv())).checkEncoding()
        assertEquals(
            """
            ELIDED [
                ELIDED: ELIDED
                ELIDED
            ]
            """.trimIndent(),
            e5.format()
        )

        // Reveal just the assertion's predicate
        val e6 = e1.elideRevealingArray(
            listOf(e1, assertionEnv(), "knows".toEnvelope())
        ).checkEncoding()
        assertEquals(
            """
            ELIDED [
                "knows": ELIDED
                ELIDED
            ]
            """.trimIndent(),
            e6.format()
        )

        // Reveal just the assertion's object
        val e7 = e1.elideRevealingArray(
            listOf(e1, assertionEnv(), "Bob".toEnvelope())
        ).checkEncoding()
        assertEquals(
            """
            ELIDED [
                ELIDED: "Bob"
                ELIDED
            ]
            """.trimIndent(),
            e7.format()
        )
    }

    @Test
    fun testWalkReplaceSubject() {
        val alice = Envelope.from("Alice")
        val bob = Envelope.from("Bob")
        val carol = Envelope.from("Carol")

        val envelope = alice.addAssertion("knows", bob)

        assertEquals(
            """
            "Alice" [
                "knows": "Bob"
            ]
            """.trimIndent(),
            envelope.format()
        )

        // Replace the subject (Alice) with Carol
        val target = setOf(alice.digest())
        val modified = envelope.walkReplace(target, carol)

        assertEquals(
            """
            "Carol" [
                "knows": "Bob"
            ]
            """.trimIndent(),
            modified.format()
        )
    }

    @Test
    fun testWalkReplaceWrapped() {
        val alice = Envelope.from("Alice")
        val bob = Envelope.from("Bob")
        val charlie = Envelope.from("Charlie")

        // Create a wrapped envelope containing Bob
        val wrapped = bob.wrap()
        val envelope = alice.addAssertion("data", wrapped)

        assertEquals(
            """
            "Alice" [
                "data": {
                    "Bob"
                }
            ]
            """.trimIndent(),
            envelope.format()
        )

        // Replace Bob with Charlie
        val target = setOf(bob.digest())
        val modified = envelope.walkReplace(target, charlie)

        assertEquals(
            """
            "Alice" [
                "data": {
                    "Charlie"
                }
            ]
            """.trimIndent(),
            modified.format()
        )
    }

    @Test
    fun testWalkReplaceMultipleTargets() {
        val alice = Envelope.from("Alice")
        val bob = Envelope.from("Bob")
        val carol = Envelope.from("Carol")
        val replacement = Envelope.from("REDACTED")

        val envelope = alice
            .addAssertion("knows", bob)
            .addAssertion("likes", carol)

        assertEquals(
            """
            "Alice" [
                "knows": "Bob"
                "likes": "Carol"
            ]
            """.trimIndent(),
            envelope.format()
        )

        // Replace both Bob and Carol with REDACTED
        val target = setOf(bob.digest(), carol.digest())
        val modified = envelope.walkReplace(target, replacement)

        assertEquals(
            """
            "Alice" [
                "knows": "REDACTED"
                "likes": "REDACTED"
            ]
            """.trimIndent(),
            modified.format()
        )
    }

    @Test
    fun testWalkReplaceAssertionWithNonAssertionFails() {
        val alice = Envelope.from("Alice")
        val bob = Envelope.from("Bob")
        val charlie = Envelope.from("Charlie")

        val envelope = alice.addAssertion("knows", bob)

        // Get the assertion's digest
        val knowsAssertion = envelope.assertionWithPredicate("knows")
        val assertionDigest = knowsAssertion.digest()

        // Try to replace the entire assertion with Charlie (a non-assertion)
        val target = setOf(assertionDigest)
        val result = runCatching { envelope.walkReplace(target, charlie) }

        // This should fail because we're replacing an assertion with a non-assertion
        assertTrue(result.isFailure)
    }

    @Test
    fun testWalkReplaceBasic() {
        val bob = Envelope.from("Bob")
        val charlie = Envelope.from("Charlie")

        val envelope = Envelope.from("Alice")
            .addAssertion("knows", bob)
            .addAssertion("likes", bob)

        assertEquals(
            """
            "Alice" [
                "knows": "Bob"
                "likes": "Bob"
            ]
            """.trimIndent(),
            envelope.format()
        )

        val target = setOf(bob.digest())
        val modified = envelope.walkReplace(target, charlie)

        assertEquals(
            """
            "Alice" [
                "knows": "Charlie"
                "likes": "Charlie"
            ]
            """.trimIndent(),
            modified.format()
        )

        assertFalse(modified.isEquivalentTo(envelope))
    }

    @Test
    fun testWalkReplaceNested() {
        val bob = Envelope.from("Bob")
        val charlie = Envelope.from("Charlie")

        val inner = bob.addAssertion("friend", bob)
        val envelope = Envelope.from("Alice").addAssertion("knows", inner)

        assertEquals(
            """
            "Alice" [
                "knows": "Bob" [
                    "friend": "Bob"
                ]
            ]
            """.trimIndent(),
            envelope.format()
        )

        val target = setOf(bob.digest())
        val modified = envelope.walkReplace(target, charlie)

        assertEquals(
            """
            "Alice" [
                "knows": "Charlie" [
                    "friend": "Charlie"
                ]
            ]
            """.trimIndent(),
            modified.format()
        )
    }

    @Test
    fun testWalkReplaceNoMatch() {
        val bob = Envelope.from("Bob")
        val charlie = Envelope.from("Charlie")
        val dave = Envelope.from("Dave")

        val envelope = Envelope.from("Alice").addAssertion("knows", bob)

        val target = setOf(dave.digest())
        val modified = envelope.walkReplace(target, charlie)

        assertTrue(modified.isIdenticalTo(envelope))
    }

    @Test
    fun testWalkReplaceElided() {
        val bob = Envelope.from("Bob")
        val charlie = Envelope.from("Charlie")

        val envelope = Envelope.from("Alice")
            .addAssertion("knows", bob)
            .addAssertion("likes", bob)

        val elided = envelope.elideRemovingTarget(bob)
        assertEquals(
            """
            "Alice" [
                "knows": ELIDED
                "likes": ELIDED
            ]
            """.trimIndent(),
            elided.format()
        )

        val target = setOf(bob.digest())
        val modified = elided.walkReplace(target, charlie)

        assertEquals(
            """
            "Alice" [
                "knows": "Charlie"
                "likes": "Charlie"
            ]
            """.trimIndent(),
            modified.format()
        )
    }
}

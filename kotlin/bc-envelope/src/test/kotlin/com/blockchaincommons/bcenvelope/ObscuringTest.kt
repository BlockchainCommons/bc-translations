package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.Digest
import com.blockchaincommons.bccomponents.SymmetricKey
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

class ObscuringTest {

    @Test
    fun testObscuring() {
        val key = SymmetricKey.create()
        val envelope = Envelope.from(PLAINTEXT_HELLO)
        assertFalse(envelope.isObscured())

        val encrypted = envelope.encryptSubject(key)
        assertTrue(encrypted.isObscured())

        val elided = envelope.elide()
        assertTrue(elided.isObscured())

        val compressed = envelope.compress()
        assertTrue(compressed.isObscured())

        // Cannot encrypt already encrypted
        assertTrue(
            try { encrypted.encryptSubject(key); false }
            catch (_: Exception) { true }
        )

        // Cannot encrypt elided
        assertTrue(
            try { elided.encryptSubject(key); false }
            catch (_: Exception) { true }
        )

        // OK to encrypt compressed
        val encryptedCompressed = compressed.encryptSubject(key)
        assertTrue(encryptedCompressed.isEncrypted())

        // OK to elide encrypted
        val elidedEncrypted = encrypted.elide()
        assertTrue(elidedEncrypted.isElided())

        // Eliding elided is idempotent
        val elidedElided = elided.elide()
        assertTrue(elidedElided.isElided())

        // OK to elide compressed
        val elidedCompressed = compressed.elide()
        assertTrue(elidedCompressed.isElided())

        // Cannot compress encrypted
        assertTrue(
            try { encrypted.compress(); false }
            catch (_: Exception) { true }
        )

        // Cannot compress elided
        assertTrue(
            try { elided.compress(); false }
            catch (_: Exception) { true }
        )

        // Compressing compressed is idempotent
        val compressedCompressed = compressed.compress()
        assertTrue(compressedCompressed.isCompressed())
    }

    @Test
    fun testNodesMatching() {
        val envelope = Envelope.from("Alice")
            .addAssertion("knows", "Bob")
            .addAssertion("age", 30)
            .addAssertion("city", "Boston")

        val knowsAssertion = envelope.assertionWithPredicate("knows")
        val knowsDigest = knowsAssertion.digest()

        val ageAssertion = envelope.assertionWithPredicate("age")
        val ageDigest = ageAssertion.digest()

        val elideTarget = setOf(knowsDigest)
        val compressTarget = setOf(ageDigest)

        var obscured = envelope.elideRemovingSet(elideTarget)
        obscured = obscured.elideRemovingSetWithAction(
            compressTarget,
            ObscureAction.Compress,
        )

        assertEquals(
            """
            "Alice" [
                "city": "Boston"
                COMPRESSED
                ELIDED
            ]
            """.trimIndent(),
            obscured.format()
        )

        // Test finding elided nodes
        val elidedNodes = obscured.nodesMatching(null, listOf(ObscureType.Elided))
        assertTrue(elidedNodes.contains(knowsDigest))

        // Test finding compressed nodes
        val compressedNodes = obscured.nodesMatching(null, listOf(ObscureType.Compressed))
        assertTrue(compressedNodes.contains(ageDigest))

        // Test with target filter
        val targetFilter = setOf(knowsDigest)
        val filtered = obscured.nodesMatching(targetFilter, listOf(ObscureType.Elided))
        assertEquals(1, filtered.size)
        assertTrue(filtered.contains(knowsDigest))
    }

    @Test
    fun testWalkUnelide() {
        val alice = Envelope.from("Alice")
        val bob = Envelope.from("Bob")
        val carol = Envelope.from("Carol")

        val envelope = Envelope.from("Alice")
            .addAssertion("knows", "Bob")
            .addAssertion("friend", "Carol")

        val elided = envelope
            .elideRemovingTarget(alice)
            .elideRemovingTarget(bob)

        assertEquals(
            """
            ELIDED [
                "friend": "Carol"
                "knows": ELIDED
            ]
            """.trimIndent(),
            elided.format()
        )

        val restored = elided.walkUnelide(listOf(alice, bob, carol))
        assertEquals(
            """
            "Alice" [
                "friend": "Carol"
                "knows": "Bob"
            ]
            """.trimIndent(),
            restored.format()
        )

        val partial = elided.walkUnelide(listOf(alice))
        assertEquals(
            """
            "Alice" [
                "friend": "Carol"
                "knows": ELIDED
            ]
            """.trimIndent(),
            partial.format()
        )

        val unchanged = elided.walkUnelide(emptyList())
        assertTrue(unchanged.isIdenticalTo(elided))
    }

    @Test
    fun testWalkDecrypt() {
        val key1 = SymmetricKey.create()
        val key2 = SymmetricKey.create()
        val key3 = SymmetricKey.create()

        val envelope = Envelope.from("Alice")
            .addAssertion("knows", "Bob")
            .addAssertion("age", 30)
            .addAssertion("city", "Boston")

        val knowsAssertion = envelope.assertionWithPredicate("knows")
        val ageAssertion = envelope.assertionWithPredicate("age")

        val encrypt1Target = setOf(knowsAssertion.digest())
        val encrypt2Target = setOf(ageAssertion.digest())

        val encrypted = envelope
            .elideRemovingSetWithAction(encrypt1Target, ObscureAction.Encrypt(key1))
            .elideRemovingSetWithAction(encrypt2Target, ObscureAction.Encrypt(key2))

        assertEquals(
            """
            "Alice" [
                "city": "Boston"
                ENCRYPTED (2)
            ]
            """.trimIndent(),
            encrypted.format()
        )

        // Decrypt with all keys
        val decrypted = encrypted.walkDecrypt(listOf(key1, key2))
        assertEquals(
            """
            "Alice" [
                "age": 30
                "city": "Boston"
                "knows": "Bob"
            ]
            """.trimIndent(),
            decrypted.format()
        )

        // Decrypt with wrong key unchanged
        val unchanged = encrypted.walkDecrypt(listOf(key3))
        assertTrue(unchanged.isIdenticalTo(encrypted))
    }

    @Test
    fun testWalkDecompress() {
        val envelope = Envelope.from("Alice")
            .addAssertion("knows", "Bob")
            .addAssertion("bio", "A".repeat(1000))
            .addAssertion("description", "B".repeat(1000))

        val bioAssertion = envelope.assertionWithPredicate("bio")
        val descAssertion = envelope.assertionWithPredicate("description")

        val bioDigest = bioAssertion.digest()
        val descDigest = descAssertion.digest()

        val compressTarget = setOf(bioDigest, descDigest)
        val compressed = envelope.elideRemovingSetWithAction(
            compressTarget,
            ObscureAction.Compress,
        )

        assertEquals(
            """
            "Alice" [
                "knows": "Bob"
                COMPRESSED (2)
            ]
            """.trimIndent(),
            compressed.format()
        )

        // Decompress all
        val decompressed = compressed.walkDecompress(null)
        assertTrue(decompressed.isEquivalentTo(envelope))

        // Decompress with target filter
        val target = setOf(bioDigest)
        val partial = compressed.walkDecompress(target)
        assertFalse(partial.isIdenticalTo(compressed))
        assertTrue(partial.isEquivalentTo(envelope))

        // Decompress with no match
        val noMatch = setOf(Digest.fromImage("nonexistent".toByteArray()))
        val unchangedResult = compressed.walkDecompress(noMatch)
        assertTrue(unchangedResult.isIdenticalTo(compressed))
    }

    @Test
    fun testMixedObscurationOperations() {
        val key = SymmetricKey.create()

        val envelope = Envelope.from("Alice")
            .addAssertion("knows", "Bob")
            .addAssertion("age", 30)
            .addAssertion("bio", "A".repeat(1000))

        val knowsAssertion = envelope.assertionWithPredicate("knows")
        val ageAssertion = envelope.assertionWithPredicate("age")
        val bioAssertion = envelope.assertionWithPredicate("bio")

        val knowsDigest = knowsAssertion.digest()
        val ageDigest = ageAssertion.digest()
        val bioDigest = bioAssertion.digest()

        val elideTarget = setOf(knowsDigest)
        val encryptTarget = setOf(ageDigest)
        val compressTarget = setOf(bioDigest)

        val obscured = envelope
            .elideRemovingSet(elideTarget)
            .elideRemovingSetWithAction(encryptTarget, ObscureAction.Encrypt(key))
            .elideRemovingSetWithAction(compressTarget, ObscureAction.Compress)

        val elidedNodes = obscured.nodesMatching(null, listOf(ObscureType.Elided))
        val encryptedNodes = obscured.nodesMatching(null, listOf(ObscureType.Encrypted))
        val compressedNodes = obscured.nodesMatching(null, listOf(ObscureType.Compressed))

        assertTrue(elidedNodes.contains(knowsDigest))
        assertTrue(encryptedNodes.contains(ageDigest))
        assertTrue(compressedNodes.contains(bioDigest))

        // Restore everything
        val restored = obscured
            .walkUnelide(listOf(knowsAssertion))
            .walkDecrypt(listOf(key))
            .walkDecompress(null)
        assertTrue(restored.isEquivalentTo(envelope))
    }
}

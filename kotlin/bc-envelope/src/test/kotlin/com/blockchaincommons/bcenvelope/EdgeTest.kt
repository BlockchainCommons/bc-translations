package com.blockchaincommons.bcenvelope

import com.blockchaincommons.bccomponents.URI
import com.blockchaincommons.knownvalues.DEREFERENCE_VIA
import com.blockchaincommons.knownvalues.IS_A
import com.blockchaincommons.knownvalues.SOURCE
import com.blockchaincommons.knownvalues.TARGET
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertNotNull
import kotlin.test.assertNull
import kotlin.test.assertTrue

class EdgeTest {

    private fun makeEdge(
        subject: String,
        isA: String,
        source: Envelope,
        target: Envelope,
    ): Envelope =
        Envelope.from(subject)
            .addAssertion(IS_A, isA)
            .addAssertion(SOURCE, source)
            .addAssertion(TARGET, target)

    private fun xidLike(name: String): Envelope = Envelope.from(name)

    // -------------------------------------------------------------------
    // Edge construction and format
    // -------------------------------------------------------------------

    @Test
    fun testEdgeBasicFormat() {
        registerTags()
        val alice = xidLike("Alice")
        val edge = makeEdge("credential-1", "foaf:Person", alice, alice)

        val expected = """
            "credential-1" [
                'isA': "foaf:Person"
                'source': "Alice"
                'target': "Alice"
            ]
        """.trimIndent()
        assertEquals(expected, edge.format())
    }

    @Test
    fun testEdgeRelationshipFormat() {
        registerTags()
        val alice = xidLike("Alice")
        val bob = xidLike("Bob")
        val edge = makeEdge("knows-bob", "schema:colleague", alice, bob)

        val expected = """
            "knows-bob" [
                'isA': "schema:colleague"
                'source': "Alice"
                'target': "Bob"
            ]
        """.trimIndent()
        assertEquals(expected, edge.format())
    }

    // -------------------------------------------------------------------
    // Edge validation
    // -------------------------------------------------------------------

    @Test
    fun testValidateEdgeValid() {
        val alice = xidLike("Alice")
        val edge = makeEdge("cred-1", "foaf:Person", alice, alice)
        edge.validateEdge() // should not throw
    }

    @Test
    fun testValidateEdgeMissingIsA() {
        val alice = xidLike("Alice")
        val edge = Envelope.from("cred-1")
            .addAssertion(SOURCE, alice)
            .addAssertion(TARGET, alice)
        assertFailsWith<EnvelopeException.EdgeMissingIsA> {
            edge.validateEdge()
        }
    }

    @Test
    fun testValidateEdgeMissingSource() {
        val alice = xidLike("Alice")
        val edge = Envelope.from("cred-1")
            .addAssertion(IS_A, "foaf:Person")
            .addAssertion(TARGET, alice)
        assertFailsWith<EnvelopeException.EdgeMissingSource> {
            edge.validateEdge()
        }
    }

    @Test
    fun testValidateEdgeMissingTarget() {
        val alice = xidLike("Alice")
        val edge = Envelope.from("cred-1")
            .addAssertion(IS_A, "foaf:Person")
            .addAssertion(SOURCE, alice)
        assertFailsWith<EnvelopeException.EdgeMissingTarget> {
            edge.validateEdge()
        }
    }

    @Test
    fun testValidateEdgeNoAssertions() {
        val edge = Envelope.from("cred-1")
        assertFailsWith<EnvelopeException.EdgeMissingIsA> {
            edge.validateEdge()
        }
    }

    @Test
    fun testValidateEdgeDuplicateIsA() {
        val alice = xidLike("Alice")
        val edge = Envelope.from("cred-1")
            .addAssertion(IS_A, "foaf:Person")
            .addAssertion(IS_A, "schema:Thing")
            .addAssertion(SOURCE, alice)
            .addAssertion(TARGET, alice)
        assertFailsWith<EnvelopeException.EdgeDuplicateIsA> {
            edge.validateEdge()
        }
    }

    @Test
    fun testValidateEdgeDuplicateSource() {
        val alice = xidLike("Alice")
        val bob = xidLike("Bob")
        val edge = Envelope.from("cred-1")
            .addAssertion(IS_A, "foaf:Person")
            .addAssertion(SOURCE, alice)
            .addAssertion(SOURCE, bob)
            .addAssertion(TARGET, alice)
        assertFailsWith<EnvelopeException.EdgeDuplicateSource> {
            edge.validateEdge()
        }
    }

    @Test
    fun testValidateEdgeDuplicateTarget() {
        val alice = xidLike("Alice")
        val bob = xidLike("Bob")
        val edge = Envelope.from("cred-1")
            .addAssertion(IS_A, "foaf:Person")
            .addAssertion(SOURCE, alice)
            .addAssertion(TARGET, alice)
            .addAssertion(TARGET, bob)
        assertFailsWith<EnvelopeException.EdgeDuplicateTarget> {
            edge.validateEdge()
        }
    }

    @Test
    fun testValidateEdgeWrappedSigned() {
        registerTags()
        val alice = xidLike("Alice")
        val edge = makeEdge("cred-1", "foaf:Person", alice, alice)

        val signedEdge = edge.wrap().addSignature(alicePrivateKey())
        signedEdge.validateEdge() // should not throw
    }

    // -------------------------------------------------------------------
    // Edge accessor methods
    // -------------------------------------------------------------------

    @Test
    fun testEdgeIsA() {
        registerTags()
        val alice = xidLike("Alice")
        val edge = makeEdge("cred-1", "foaf:Person", alice, alice)

        val isA = edge.edgeIsA()
        assertEquals("\"foaf:Person\"", isA.format())
    }

    @Test
    fun testEdgeSource() {
        registerTags()
        val alice = xidLike("Alice")
        val edge = makeEdge("cred-1", "foaf:Person", alice, alice)

        val source = edge.edgeSource()
        assertEquals("\"Alice\"", source.format())
    }

    @Test
    fun testEdgeTarget() {
        registerTags()
        val alice = xidLike("Alice")
        val bob = xidLike("Bob")
        val edge = makeEdge("knows-bob", "schema:colleague", alice, bob)

        val target = edge.edgeTarget()
        assertEquals("\"Bob\"", target.format())
    }

    @Test
    fun testEdgeSubject() {
        registerTags()
        val alice = xidLike("Alice")
        val edge = makeEdge("my-credential", "foaf:Person", alice, alice)

        val subject = edge.edgeSubject()
        assertEquals("\"my-credential\"", subject.format())
    }

    @Test
    fun testEdgeAccessorsOnSignedEdge() {
        registerTags()
        val alice = xidLike("Alice")
        val bob = xidLike("Bob")
        val edge = makeEdge("cred-1", "foaf:Person", alice, bob)

        val signedEdge = edge.wrap().addSignature(alicePrivateKey())

        assertEquals("\"foaf:Person\"", signedEdge.edgeIsA().format())
        assertEquals("\"Alice\"", signedEdge.edgeSource().format())
        assertEquals("\"Bob\"", signedEdge.edgeTarget().format())
        assertEquals("\"cred-1\"", signedEdge.edgeSubject().format())
    }

    // -------------------------------------------------------------------
    // Adding edges to envelopes
    // -------------------------------------------------------------------

    @Test
    fun testAddEdgeEnvelope() {
        registerTags()
        val alice = xidLike("Alice")
        val edge = makeEdge("cred-1", "foaf:Person", alice, alice)

        val doc = Envelope.from("Alice").addEdgeEnvelope(edge)

        val expected = """
            "Alice" [
                'edge': "cred-1" [
                    'isA': "foaf:Person"
                    'source': "Alice"
                    'target': "Alice"
                ]
            ]
        """.trimIndent()
        assertEquals(expected, doc.format())
    }

    @Test
    fun testAddMultipleEdges() {
        registerTags()
        val alice = xidLike("Alice")
        val bob = xidLike("Bob")
        val edge1 = makeEdge("self-desc", "foaf:Person", alice, alice)
        val edge2 = makeEdge("knows-bob", "schema:colleague", alice, bob)

        val doc = Envelope.from("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)

        val edges = doc.edges()
        assertEquals(2, edges.size)

        val formatted = doc.format()
        assertTrue(formatted.contains("'edge'"))
        assertTrue(formatted.contains("\"self-desc\""))
        assertTrue(formatted.contains("\"knows-bob\""))
    }

    // -------------------------------------------------------------------
    // Edges retrieval via envelope
    // -------------------------------------------------------------------

    @Test
    fun testEdgesEmpty() {
        val doc = Envelope.from("Alice")
        val edges = doc.edges()
        assertEquals(0, edges.size)
    }

    @Test
    fun testEdgesRetrieval() {
        val alice = xidLike("Alice")
        val edge1 = makeEdge("cred-1", "foaf:Person", alice, alice)
        val edge2 = makeEdge("cred-2", "schema:Thing", alice, alice)

        val doc = Envelope.from("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)

        val edges = doc.edges()
        assertEquals(2, edges.size)

        for (edge in edges) {
            edge.validateEdge()
        }
    }

    // -------------------------------------------------------------------
    // Edges container (add / get / remove / clear / len)
    // -------------------------------------------------------------------

    @Test
    fun testEdgesContainerNewIsEmpty() {
        val edges = Edges()
        assertTrue(edges.isEmpty())
        assertEquals(0, edges.size)
    }

    @Test
    fun testEdgesContainerAddAndGet() {
        val alice = xidLike("Alice")
        val edge = makeEdge("cred-1", "foaf:Person", alice, alice)
        val digest = edge.digest()

        val edges = Edges()
        edges.add(edge)

        assertTrue(!edges.isEmpty())
        assertEquals(1, edges.size)
        assertNotNull(edges.get(digest))
        assertTrue(edges.get(digest)!!.isEquivalentTo(edge))
    }

    @Test
    fun testEdgesContainerRemove() {
        val alice = xidLike("Alice")
        val edge = makeEdge("cred-1", "foaf:Person", alice, alice)
        val digest = edge.digest()

        val edges = Edges()
        edges.add(edge)

        val removed = edges.remove(digest)
        assertNotNull(removed)
        assertTrue(edges.isEmpty())
    }

    @Test
    fun testEdgesContainerRemoveNonexistent() {
        val alice = xidLike("Alice")
        val edge = makeEdge("cred-1", "foaf:Person", alice, alice)

        val edges = Edges()
        val removed = edges.remove(edge.digest())
        assertNull(removed)
    }

    @Test
    fun testEdgesContainerClear() {
        val alice = xidLike("Alice")
        val edge1 = makeEdge("cred-1", "foaf:Person", alice, alice)
        val edge2 = makeEdge("cred-2", "schema:Thing", alice, alice)

        val edges = Edges()
        edges.add(edge1)
        edges.add(edge2)
        assertEquals(2, edges.size)

        edges.clear()
        assertTrue(edges.isEmpty())
        assertEquals(0, edges.size)
    }

    @Test
    fun testEdgesContainerIter() {
        val alice = xidLike("Alice")
        val edge1 = makeEdge("cred-1", "foaf:Person", alice, alice)
        val edge2 = makeEdge("cred-2", "schema:Thing", alice, alice)

        val edges = Edges()
        edges.add(edge1)
        edges.add(edge2)

        assertEquals(2, edges.entries.size)
    }

    // -------------------------------------------------------------------
    // Edges container round-trip: addToEnvelope / fromEnvelope
    // -------------------------------------------------------------------

    @Test
    fun testEdgesContainerRoundtrip() {
        val alice = xidLike("Alice")
        val edge1 = makeEdge("cred-1", "foaf:Person", alice, alice)
        val edge2 = makeEdge("cred-2", "schema:Thing", alice, alice)

        val edges = Edges()
        edges.add(edge1)
        edges.add(edge2)

        val doc = Envelope.from("Alice")
        val docWithEdges = edges.addToEnvelope(doc)

        val recovered = Edges.fromEnvelope(docWithEdges)
        assertEquals(2, recovered.size)
        assertNotNull(recovered.get(edge1.digest()))
        assertNotNull(recovered.get(edge2.digest()))
    }

    @Test
    fun testEdgesContainerRoundtripEmpty() {
        val edges = Edges()
        val doc = Envelope.from("Alice")
        val docWithEdges = edges.addToEnvelope(doc)

        val recovered = Edges.fromEnvelope(docWithEdges)
        assertTrue(recovered.isEmpty())
    }

    @Test
    fun testEdgesContainerRoundtripPreservesFormat() {
        registerTags()
        val alice = xidLike("Alice")
        val bob = xidLike("Bob")
        val edge = makeEdge("knows-bob", "schema:colleague", alice, bob)

        val edges = Edges()
        edges.add(edge)

        val doc = edges.addToEnvelope(Envelope.from("Alice"))

        val expected = """
            "Alice" [
                'edge': "knows-bob" [
                    'isA': "schema:colleague"
                    'source': "Alice"
                    'target': "Bob"
                ]
            ]
        """.trimIndent()
        assertEquals(expected, doc.format())

        val recovered = Edges.fromEnvelope(doc)
        assertEquals(1, recovered.size)
    }

    // -------------------------------------------------------------------
    // Edgeable trait
    // -------------------------------------------------------------------

    @Test
    fun testEdgeableDefaultMethods() {
        val alice = xidLike("Alice")
        val edge = makeEdge("cred-1", "foaf:Person", alice, alice)
        val digest = edge.digest()

        val edges = Edges()
        edges.add(edge)

        assertTrue(!edges.isEmpty())
        assertEquals(1, edges.size)
        assertNotNull(edges.get(digest))

        val removed = edges.remove(digest)
        assertNotNull(removed)
        assertTrue(edges.isEmpty())
    }

    // -------------------------------------------------------------------
    // edgesMatching -- filtering by criteria
    // -------------------------------------------------------------------

    @Test
    fun testEdgesMatchingNoFilters() {
        val alice = xidLike("Alice")
        val bob = xidLike("Bob")
        val edge1 = makeEdge("self-desc", "foaf:Person", alice, alice)
        val edge2 = makeEdge("knows-bob", "schema:colleague", alice, bob)

        val doc = Envelope.from("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)

        val matching = doc.edgesMatching()
        assertEquals(2, matching.size)
    }

    @Test
    fun testEdgesMatchingByIsA() {
        val alice = xidLike("Alice")
        val bob = xidLike("Bob")
        val edge1 = makeEdge("self-desc", "foaf:Person", alice, alice)
        val edge2 = makeEdge("knows-bob", "schema:colleague", alice, bob)
        val edge3 = makeEdge("self-thing", "foaf:Person", alice, alice)

        val doc = Envelope.from("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)
            .addEdgeEnvelope(edge3)

        val isAPerson = Envelope.from("foaf:Person")
        assertEquals(2, doc.edgesMatching(isA = isAPerson).size)

        val isAColleague = Envelope.from("schema:colleague")
        assertEquals(1, doc.edgesMatching(isA = isAColleague).size)

        val isANone = Envelope.from("nonexistent")
        assertEquals(0, doc.edgesMatching(isA = isANone).size)
    }

    @Test
    fun testEdgesMatchingBySource() {
        val alice = xidLike("Alice")
        val bob = xidLike("Bob")
        val edge1 = makeEdge("alice-claim", "foaf:Person", alice, alice)
        val edge2 = makeEdge("bob-claim", "foaf:Person", bob, alice)

        val doc = Envelope.from("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)

        assertEquals(1, doc.edgesMatching(source = alice).size)
        assertEquals(1, doc.edgesMatching(source = bob).size)

        val carol = xidLike("Carol")
        assertEquals(0, doc.edgesMatching(source = carol).size)
    }

    @Test
    fun testEdgesMatchingByTarget() {
        val alice = xidLike("Alice")
        val bob = xidLike("Bob")
        val edge1 = makeEdge("self-desc", "foaf:Person", alice, alice)
        val edge2 = makeEdge("knows-bob", "schema:colleague", alice, bob)

        val doc = Envelope.from("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)

        assertEquals(1, doc.edgesMatching(target = alice).size)
        assertEquals(1, doc.edgesMatching(target = bob).size)
    }

    @Test
    fun testEdgesMatchingBySubject() {
        val alice = xidLike("Alice")
        val edge1 = makeEdge("self-desc", "foaf:Person", alice, alice)
        val edge2 = makeEdge("cred-2", "schema:Thing", alice, alice)

        val doc = Envelope.from("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)

        val subjectFilter = Envelope.from("self-desc")
        assertEquals(1, doc.edgesMatching(subject = subjectFilter).size)

        val subjectFilter2 = Envelope.from("nonexistent")
        assertEquals(0, doc.edgesMatching(subject = subjectFilter2).size)
    }

    @Test
    fun testEdgesMatchingCombinedFilters() {
        val alice = xidLike("Alice")
        val bob = xidLike("Bob")
        val edge1 = makeEdge("self-desc", "foaf:Person", alice, alice)
        val edge2 = makeEdge("self-thing", "foaf:Person", alice, alice)
        val edge3 = makeEdge("knows-bob", "foaf:Person", alice, bob)

        val doc = Envelope.from("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)
            .addEdgeEnvelope(edge3)

        val isA = Envelope.from("foaf:Person")

        // All three are foaf:Person
        assertEquals(3, doc.edgesMatching(isA = isA).size)

        // foaf:Person + target Alice => 2 (self-desc, self-thing)
        assertEquals(2, doc.edgesMatching(isA = isA, target = alice).size)

        // foaf:Person + target Bob => 1 (knows-bob)
        assertEquals(1, doc.edgesMatching(isA = isA, target = bob).size)

        // foaf:Person + target Alice + subject "self-desc" => 1
        val subj = Envelope.from("self-desc")
        assertEquals(
            1,
            doc.edgesMatching(isA = isA, target = alice, subject = subj).size
        )

        // foaf:Person + source Alice + target Bob + subject "knows-bob" => 1
        val subj2 = Envelope.from("knows-bob")
        assertEquals(
            1,
            doc.edgesMatching(
                isA = isA,
                source = alice,
                target = bob,
                subject = subj2
            ).size
        )

        // All filters that match nothing
        val subj3 = Envelope.from("nonexistent")
        assertEquals(
            0,
            doc.edgesMatching(
                isA = isA,
                source = alice,
                target = alice,
                subject = subj3
            ).size
        )
    }

    // -------------------------------------------------------------------
    // Signed edges with format verification
    // -------------------------------------------------------------------

    @Test
    fun testSignedEdgeFormat() {
        registerTags()
        val alice = xidLike("Alice")
        val edge = makeEdge("cred-1", "foaf:Person", alice, alice)

        val signedEdge = edge.wrap().addSignature(alicePrivateKey())

        val expected = """
            {
                "cred-1" [
                    'isA': "foaf:Person"
                    'source': "Alice"
                    'target': "Alice"
                ]
            } [
                'signed': Signature
            ]
        """.trimIndent()
        assertEquals(expected, signedEdge.format())
    }

    @Test
    fun testSignedEdgeOnDocumentFormat() {
        registerTags()
        val alice = xidLike("Alice")
        val edge = makeEdge("cred-1", "foaf:Person", alice, alice)
        val signedEdge = edge.wrap().addSignature(alicePrivateKey())

        val doc = Envelope.from("Alice")
            .addAssertion("knows", "Bob")
            .addEdgeEnvelope(signedEdge)

        val formatted = doc.format()
        assertTrue(formatted.contains("'edge': {"))
        assertTrue(formatted.contains("'signed': Signature"))
        assertTrue(formatted.contains("'isA': \"foaf:Person\""))
    }

    // -------------------------------------------------------------------
    // Edge coexistence with attachments
    // -------------------------------------------------------------------

    @Test
    fun testEdgesCoexistWithAttachments() {
        registerTags()
        val alice = xidLike("Alice")
        val edge = makeEdge("cred-1", "foaf:Person", alice, alice)

        val doc = Envelope.from("Alice")
            .addAttachment(
                "Metadata",
                "com.example",
                "https://example.com/v1",
            )
            .addEdgeEnvelope(edge)

        assertEquals(1, doc.edges().size)
        assertEquals(1, doc.attachments().size)

        val formatted = doc.format()
        assertTrue(formatted.contains("'edge'"))
        assertTrue(formatted.contains("'attachment'"))
    }

    // -------------------------------------------------------------------
    // Edge UR round-trip
    // -------------------------------------------------------------------

    @Test
    fun testEdgeUrRoundtrip() {
        val alice = xidLike("Alice")
        val edge = makeEdge("cred-1", "foaf:Person", alice, alice)

        val doc = Envelope.from("Alice").addEdgeEnvelope(edge)

        val ur = doc.ur()
        val recovered = Envelope.fromUr(ur)
        assertTrue(recovered.isEquivalentTo(doc))

        val recoveredEdges = recovered.edges()
        assertEquals(1, recoveredEdges.size)
        assertTrue(recoveredEdges[0].isEquivalentTo(edge))
    }

    @Test
    fun testMultipleEdgesUrRoundtrip() {
        val alice = xidLike("Alice")
        val bob = xidLike("Bob")
        val edge1 = makeEdge("self-desc", "foaf:Person", alice, alice)
        val edge2 = makeEdge("knows-bob", "schema:colleague", alice, bob)
        val edge3 = makeEdge("project", "schema:CreativeWork", alice, bob)

        val doc = Envelope.from("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)
            .addEdgeEnvelope(edge3)

        val ur = doc.ur()
        val recovered = Envelope.fromUr(ur)
        assertTrue(recovered.isEquivalentTo(doc))

        val recoveredEdges = recovered.edges()
        assertEquals(3, recoveredEdges.size)
    }

    // -------------------------------------------------------------------
    // Edge with extra assertions beyond the required three
    // -------------------------------------------------------------------

    @Test
    fun testEdgeWithAdditionalAssertions() {
        val alice = xidLike("Alice")
        val bob = xidLike("Bob")

        val edge = Envelope.from("knows-bob")
            .addAssertion(IS_A, "schema:colleague")
            .addAssertion(SOURCE, alice)
            .addAssertion(TARGET, bob)
            .addAssertion("department", "Engineering")
            .addAssertion("since", "2024-01-15")

        assertFailsWith<EnvelopeException.EdgeUnexpectedAssertion> {
            edge.validateEdge()
        }
    }

    @Test
    fun testEdgeWithClaimDetailOnTarget() {
        val alice = xidLike("Alice")
        val target = xidLike("Bob")
            .addAssertion("department", "Engineering")
            .addAssertion("since", "2024-01-15")
        val edge = makeEdge("knows-bob", "schema:colleague", alice, target)
        edge.validateEdge() // should not throw
    }

    @Test
    fun testEdgeWithClaimDetailOnSource() {
        val source = xidLike("Alice").addAssertion(
            DEREFERENCE_VIA,
            URI.fromString("https://example.com/xid/"),
        )
        val target = xidLike("Bob")
        val edge = makeEdge("knows-bob", "schema:colleague", source, target)
        edge.validateEdge() // should not throw
    }
}

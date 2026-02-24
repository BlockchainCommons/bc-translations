import Testing
import BCComponents
import BCEnvelope
import Foundation

struct EdgeTests {
    private func makeEdge(
        subject: String,
        isA: String,
        source: Envelope,
        target: Envelope
    ) -> Envelope {
        Envelope(subject)
            .addAssertion(.isA, isA)
            .addAssertion(.source, source)
            .addAssertion(.target, target)
    }

    private func xidLike(_ name: String) -> Envelope {
        Envelope(name)
    }

    private func expectEnvelopeErrorType(
        _ expectedType: String,
        _ body: () throws -> Void
    ) {
        do {
            try body()
            #expect(Bool(false), "Expected EnvelopeError(\(expectedType))")
        } catch let error as EnvelopeError {
            #expect(error.type == expectedType)
        } catch {
            #expect(Bool(false), "Unexpected error type: \(String(describing: error))")
        }
    }

    // MARK: - Edge construction and format

    @Test func testEdgeBasicFormat() {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "credential-1", isA: "foaf:Person", source: alice, target: alice)

        #expect(edge.format() ==
        """
        "credential-1" [
            'isA': "foaf:Person"
            'source': "Alice"
            'target': "Alice"
        ]
        """)
    }

    @Test func testEdgeRelationshipFormat() {
        let alice = xidLike("Alice")
        let bob = xidLike("Bob")
        let edge = makeEdge(subject: "knows-bob", isA: "schema:colleague", source: alice, target: bob)

        #expect(edge.format() ==
        """
        "knows-bob" [
            'isA': "schema:colleague"
            'source': "Alice"
            'target': "Bob"
        ]
        """)
    }

    // MARK: - Edge validation

    @Test func testValidateEdgeValid() throws {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)
        try edge.validateEdge()
    }

    @Test func testValidateEdgeMissingIsA() {
        let alice = xidLike("Alice")
        let edge = Envelope("cred-1")
            .addAssertion(.source, alice)
            .addAssertion(.target, alice)
        expectEnvelopeErrorType("edgeMissingIsA") {
            try edge.validateEdge()
        }
    }

    @Test func testValidateEdgeMissingSource() {
        let alice = xidLike("Alice")
        let edge = Envelope("cred-1")
            .addAssertion(.isA, "foaf:Person")
            .addAssertion(.target, alice)
        expectEnvelopeErrorType("edgeMissingSource") {
            try edge.validateEdge()
        }
    }

    @Test func testValidateEdgeMissingTarget() {
        let alice = xidLike("Alice")
        let edge = Envelope("cred-1")
            .addAssertion(.isA, "foaf:Person")
            .addAssertion(.source, alice)
        expectEnvelopeErrorType("edgeMissingTarget") {
            try edge.validateEdge()
        }
    }

    @Test func testValidateEdgeNoAssertions() {
        let edge = Envelope("cred-1")
        expectEnvelopeErrorType("edgeMissingIsA") {
            try edge.validateEdge()
        }
    }

    @Test func testValidateEdgeDuplicateIsA() {
        let alice = xidLike("Alice")
        let edge = Envelope("cred-1")
            .addAssertion(.isA, "foaf:Person")
            .addAssertion(.isA, "schema:Thing")
            .addAssertion(.source, alice)
            .addAssertion(.target, alice)
        expectEnvelopeErrorType("edgeDuplicateIsA") {
            try edge.validateEdge()
        }
    }

    @Test func testValidateEdgeDuplicateSource() {
        let alice = xidLike("Alice")
        let bob = xidLike("Bob")
        let edge = Envelope("cred-1")
            .addAssertion(.isA, "foaf:Person")
            .addAssertion(.source, alice)
            .addAssertion(.source, bob)
            .addAssertion(.target, alice)
        expectEnvelopeErrorType("edgeDuplicateSource") {
            try edge.validateEdge()
        }
    }

    @Test func testValidateEdgeDuplicateTarget() {
        let alice = xidLike("Alice")
        let bob = xidLike("Bob")
        let edge = Envelope("cred-1")
            .addAssertion(.isA, "foaf:Person")
            .addAssertion(.source, alice)
            .addAssertion(.target, alice)
            .addAssertion(.target, bob)
        expectEnvelopeErrorType("edgeDuplicateTarget") {
            try edge.validateEdge()
        }
    }

    @Test func testValidateEdgeWrappedSigned() throws {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)
        let signedEdge = edge.wrap().addSignature(alicePrivateKeys)
        try signedEdge.validateEdge()
    }

    // MARK: - Edge accessor methods

    @Test func testEdgeIsA() throws {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)
        #expect(try edge.edgeIsA().format() == "\"foaf:Person\"")
    }

    @Test func testEdgeSource() throws {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)
        #expect(try edge.edgeSource().format() == "\"Alice\"")
    }

    @Test func testEdgeTarget() throws {
        let alice = xidLike("Alice")
        let bob = xidLike("Bob")
        let edge = makeEdge(subject: "knows-bob", isA: "schema:colleague", source: alice, target: bob)
        #expect(try edge.edgeTarget().format() == "\"Bob\"")
    }

    @Test func testEdgeSubject() throws {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "my-credential", isA: "foaf:Person", source: alice, target: alice)
        #expect(try edge.edgeSubject().format() == "\"my-credential\"")
    }

    @Test func testEdgeAccessorsOnSignedEdge() throws {
        let alice = xidLike("Alice")
        let bob = xidLike("Bob")
        let edge = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: bob)
        let signedEdge = edge.wrap().addSignature(alicePrivateKeys)

        #expect(try signedEdge.edgeIsA().format() == "\"foaf:Person\"")
        #expect(try signedEdge.edgeSource().format() == "\"Alice\"")
        #expect(try signedEdge.edgeTarget().format() == "\"Bob\"")
        #expect(try signedEdge.edgeSubject().format() == "\"cred-1\"")
    }

    // MARK: - Adding edges to envelopes

    @Test func testAddEdgeEnvelope() {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)
        let doc = Envelope("Alice").addEdgeEnvelope(edge)

        #expect(doc.format() ==
        """
        "Alice" [
            'edge': "cred-1" [
                'isA': "foaf:Person"
                'source': "Alice"
                'target': "Alice"
            ]
        ]
        """)
    }

    @Test func testAddMultipleEdges() throws {
        let alice = xidLike("Alice")
        let bob = xidLike("Bob")
        let edge1 = makeEdge(subject: "self-desc", isA: "foaf:Person", source: alice, target: alice)
        let edge2 = makeEdge(subject: "knows-bob", isA: "schema:colleague", source: alice, target: bob)

        let doc = Envelope("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)

        let edges = try doc.edges()
        #expect(edges.count == 2)

        let formatted = doc.format()
        #expect(formatted.contains("'edge'"))
        #expect(formatted.contains("\"self-desc\""))
        #expect(formatted.contains("\"knows-bob\""))
    }

    // MARK: - Edges retrieval via envelope

    @Test func testEdgesEmpty() throws {
        let doc = Envelope("Alice")
        let edges = try doc.edges()
        #expect(edges.count == 0)
    }

    @Test func testEdgesRetrieval() throws {
        let alice = xidLike("Alice")
        let edge1 = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)
        let edge2 = makeEdge(subject: "cred-2", isA: "schema:Thing", source: alice, target: alice)

        let doc = Envelope("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)

        let edges = try doc.edges()
        #expect(edges.count == 2)
        for edge in edges {
            try edge.validateEdge()
        }
    }

    // MARK: - Edges container

    @Test func testEdgesContainerNewIsEmpty() {
        let edges = Edges()
        #expect(edges.isEmpty())
        #expect(edges.len() == 0)
    }

    @Test func testEdgesContainerAddAndGet() {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)
        let digest = edge.digest

        var edges = Edges()
        edges.add(edge)

        #expect(!edges.isEmpty())
        #expect(edges.len() == 1)
        #expect(edges.get(digest) != nil)
        #expect(edges.get(digest)!.isEquivalent(to: edge))
    }

    @Test func testEdgesContainerRemove() {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)
        let digest = edge.digest

        var edges = Edges()
        edges.add(edge)

        let removed = edges.remove(digest)
        #expect(removed != nil)
        #expect(edges.isEmpty())
    }

    @Test func testEdgesContainerRemoveNonexistent() {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)

        var edges = Edges()
        let removed = edges.remove(edge.digest)
        #expect(removed == nil)
    }

    @Test func testEdgesContainerClear() {
        let alice = xidLike("Alice")
        let edge1 = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)
        let edge2 = makeEdge(subject: "cred-2", isA: "schema:Thing", source: alice, target: alice)

        var edges = Edges()
        edges.add(edge1)
        edges.add(edge2)
        #expect(edges.len() == 2)

        edges.clear()
        #expect(edges.isEmpty())
        #expect(edges.len() == 0)
    }

    @Test func testEdgesContainerIter() {
        let alice = xidLike("Alice")
        let edge1 = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)
        let edge2 = makeEdge(subject: "cred-2", isA: "schema:Thing", source: alice, target: alice)

        var edges = Edges()
        edges.add(edge1)
        edges.add(edge2)

        #expect(edges.iter().count == 2)
    }

    // MARK: - Edges container round-trip

    @Test func testEdgesContainerRoundtrip() throws {
        let alice = xidLike("Alice")
        let edge1 = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)
        let edge2 = makeEdge(subject: "cred-2", isA: "schema:Thing", source: alice, target: alice)

        var edges = Edges()
        edges.add(edge1)
        edges.add(edge2)

        let docWithEdges = edges.addToEnvelope(Envelope("Alice"))
        let recovered = try Edges.tryFromEnvelope(docWithEdges)
        #expect(recovered.len() == 2)
        #expect(recovered.get(edge1.digest) != nil)
        #expect(recovered.get(edge2.digest) != nil)
    }

    @Test func testEdgesContainerRoundtripEmpty() throws {
        let edges = Edges()
        let docWithEdges = edges.addToEnvelope(Envelope("Alice"))
        let recovered = try Edges.tryFromEnvelope(docWithEdges)
        #expect(recovered.isEmpty())
    }

    @Test func testEdgesContainerRoundtripPreservesFormat() throws {
        let alice = xidLike("Alice")
        let bob = xidLike("Bob")
        let edge = makeEdge(subject: "knows-bob", isA: "schema:colleague", source: alice, target: bob)

        var edges = Edges()
        edges.add(edge)
        let doc = edges.addToEnvelope(Envelope("Alice"))

        #expect(doc.format() ==
        """
        "Alice" [
            'edge': "knows-bob" [
                'isA': "schema:colleague"
                'source': "Alice"
                'target': "Bob"
            ]
        ]
        """)

        let recovered = try Edges.tryFromEnvelope(doc)
        #expect(recovered.len() == 1)
    }

    // MARK: - Edgeable trait

    @Test func testEdgeableDefaultMethods() {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)
        let digest = edge.digest

        var edges = Edges()
        edges.add(edge)

        #expect(!edges.isEmpty())
        #expect(edges.len() == 1)
        #expect(edges.get(digest) != nil)

        let removed = edges.remove(digest)
        #expect(removed != nil)
        #expect(edges.isEmpty())
    }

    // MARK: - edgesMatching

    @Test func testEdgesMatchingNoFilters() throws {
        let alice = xidLike("Alice")
        let bob = xidLike("Bob")
        let edge1 = makeEdge(subject: "self-desc", isA: "foaf:Person", source: alice, target: alice)
        let edge2 = makeEdge(subject: "knows-bob", isA: "schema:colleague", source: alice, target: bob)

        let doc = Envelope("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)

        let matching = try doc.edgesMatching()
        #expect(matching.count == 2)
    }

    @Test func testEdgesMatchingByIsA() throws {
        let alice = xidLike("Alice")
        let bob = xidLike("Bob")
        let edge1 = makeEdge(subject: "self-desc", isA: "foaf:Person", source: alice, target: alice)
        let edge2 = makeEdge(subject: "knows-bob", isA: "schema:colleague", source: alice, target: bob)
        let edge3 = makeEdge(subject: "self-thing", isA: "foaf:Person", source: alice, target: alice)

        let doc = Envelope("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)
            .addEdgeEnvelope(edge3)

        #expect(try doc.edgesMatching(isA: Envelope("foaf:Person")).count == 2)
        #expect(try doc.edgesMatching(isA: Envelope("schema:colleague")).count == 1)
        #expect(try doc.edgesMatching(isA: Envelope("nonexistent")).count == 0)
    }

    @Test func testEdgesMatchingBySource() throws {
        let alice = xidLike("Alice")
        let bob = xidLike("Bob")
        let edge1 = makeEdge(subject: "alice-claim", isA: "foaf:Person", source: alice, target: alice)
        let edge2 = makeEdge(subject: "bob-claim", isA: "foaf:Person", source: bob, target: alice)

        let doc = Envelope("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)

        #expect(try doc.edgesMatching(source: alice).count == 1)
        #expect(try doc.edgesMatching(source: bob).count == 1)
        #expect(try doc.edgesMatching(source: xidLike("Carol")).count == 0)
    }

    @Test func testEdgesMatchingByTarget() throws {
        let alice = xidLike("Alice")
        let bob = xidLike("Bob")
        let edge1 = makeEdge(subject: "self-desc", isA: "foaf:Person", source: alice, target: alice)
        let edge2 = makeEdge(subject: "knows-bob", isA: "schema:colleague", source: alice, target: bob)

        let doc = Envelope("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)

        #expect(try doc.edgesMatching(target: alice).count == 1)
        #expect(try doc.edgesMatching(target: bob).count == 1)
    }

    @Test func testEdgesMatchingBySubject() throws {
        let alice = xidLike("Alice")
        let edge1 = makeEdge(subject: "self-desc", isA: "foaf:Person", source: alice, target: alice)
        let edge2 = makeEdge(subject: "cred-2", isA: "schema:Thing", source: alice, target: alice)

        let doc = Envelope("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)

        #expect(try doc.edgesMatching(subject: Envelope("self-desc")).count == 1)
        #expect(try doc.edgesMatching(subject: Envelope("nonexistent")).count == 0)
    }

    @Test func testEdgesMatchingCombinedFilters() throws {
        let alice = xidLike("Alice")
        let bob = xidLike("Bob")
        let edge1 = makeEdge(subject: "self-desc", isA: "foaf:Person", source: alice, target: alice)
        let edge2 = makeEdge(subject: "self-thing", isA: "foaf:Person", source: alice, target: alice)
        let edge3 = makeEdge(subject: "knows-bob", isA: "foaf:Person", source: alice, target: bob)

        let doc = Envelope("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)
            .addEdgeEnvelope(edge3)

        let isA = Envelope("foaf:Person")
        #expect(try doc.edgesMatching(isA: isA).count == 3)
        #expect(try doc.edgesMatching(isA: isA, target: alice).count == 2)
        #expect(try doc.edgesMatching(isA: isA, target: bob).count == 1)
        #expect(try doc.edgesMatching(isA: isA, target: alice, subject: Envelope("self-desc")).count == 1)
        #expect(try doc.edgesMatching(isA: isA, source: alice, target: bob, subject: Envelope("knows-bob")).count == 1)
        #expect(try doc.edgesMatching(isA: isA, source: alice, target: alice, subject: Envelope("nonexistent")).count == 0)
    }

    // MARK: - Signed edges with format verification

    @Test func testSignedEdgeFormat() {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)
        let signedEdge = edge.wrap().addSignature(alicePrivateKeys)

        #expect(signedEdge.format() ==
        """
        {
            "cred-1" [
                'isA': "foaf:Person"
                'source': "Alice"
                'target': "Alice"
            ]
        } [
            'signed': Signature
        ]
        """)
    }

    @Test func testSignedEdgeOnDocumentFormat() {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)
        let signedEdge = edge.wrap().addSignature(alicePrivateKeys)

        let doc = Envelope("Alice")
            .addAssertion("knows", "Bob")
            .addEdgeEnvelope(signedEdge)

        let formatted = doc.format()
        #expect(formatted.contains("'edge': {"))
        #expect(formatted.contains("'signed': Signature"))
        #expect(formatted.contains("'isA': \"foaf:Person\""))
    }

    // MARK: - Edge coexistence with attachments

    @Test func testEdgesCoexistWithAttachments() throws {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)

        let doc = Envelope("Alice")
            .addAttachment(Envelope("Metadata"), vendor: "com.example", conformsTo: "https://example.com/v1")
            .addEdgeEnvelope(edge)

        #expect(try doc.edges().count == 1)
        #expect(try doc.attachments(withVendor: nil, conformingTo: nil).count == 1)

        let formatted = doc.format()
        #expect(formatted.contains("'edge'"))
        #expect(formatted.contains("'attachment'"))
    }

    // MARK: - Edge UR round-trip

    @Test func testEdgeURRoundtrip() throws {
        let alice = xidLike("Alice")
        let edge = makeEdge(subject: "cred-1", isA: "foaf:Person", source: alice, target: alice)

        let doc = Envelope("Alice").addEdgeEnvelope(edge)
        let recovered = try Envelope(ur: doc.ur)

        #expect(recovered.isEquivalent(to: doc))
        let recoveredEdges = try recovered.edges()
        #expect(recoveredEdges.count == 1)
        #expect(recoveredEdges[0].isEquivalent(to: edge))
    }

    @Test func testMultipleEdgesURRoundtrip() throws {
        let alice = xidLike("Alice")
        let bob = xidLike("Bob")
        let edge1 = makeEdge(subject: "self-desc", isA: "foaf:Person", source: alice, target: alice)
        let edge2 = makeEdge(subject: "knows-bob", isA: "schema:colleague", source: alice, target: bob)
        let edge3 = makeEdge(subject: "project", isA: "schema:CreativeWork", source: alice, target: bob)

        let doc = Envelope("Alice")
            .addEdgeEnvelope(edge1)
            .addEdgeEnvelope(edge2)
            .addEdgeEnvelope(edge3)

        let recovered = try Envelope(ur: doc.ur)
        #expect(recovered.isEquivalent(to: doc))
        #expect(try recovered.edges().count == 3)
    }

    // MARK: - Additional edge validation scenarios

    @Test func testEdgeWithAdditionalAssertions() {
        let alice = xidLike("Alice")
        let bob = xidLike("Bob")
        let edge = Envelope("knows-bob")
            .addAssertion(.isA, "schema:colleague")
            .addAssertion(.source, alice)
            .addAssertion(.target, bob)
            .addAssertion("department", "Engineering")
            .addAssertion("since", "2024-01-15")

        expectEnvelopeErrorType("edgeUnexpectedAssertion") {
            try edge.validateEdge()
        }
    }

    @Test func testEdgeWithClaimDetailOnTarget() throws {
        let alice = xidLike("Alice")
        let target = xidLike("Bob")
            .addAssertion("department", "Engineering")
            .addAssertion("since", "2024-01-15")
        let edge = makeEdge(subject: "knows-bob", isA: "schema:colleague", source: alice, target: target)
        try edge.validateEdge()
    }

    @Test func testEdgeWithClaimDetailOnSource() throws {
        let source = xidLike("Alice")
            .addAssertion(.dereferenceVia, try URI("https://example.com/xid/"))
        let target = xidLike("Bob")
        let edge = makeEdge(subject: "knows-bob", isA: "schema:colleague", source: source, target: target)
        try edge.validateEdge()
    }
}

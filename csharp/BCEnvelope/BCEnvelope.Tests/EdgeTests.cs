using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class EdgeTests
{
    // Helper to create a basic edge envelope with the three required assertions.
    private static Envelope MakeEdge(
        string subject, string isA, Envelope source, Envelope target)
    {
        return Envelope.Create(subject)
            .AddAssertion(KnownValuesRegistry.IsA, isA)
            .AddAssertion(KnownValuesRegistry.Source, source)
            .AddAssertion(KnownValuesRegistry.Target, target);
    }

    // Helper to create an XID-like identifier envelope.
    private static Envelope XidLike(string name) => Envelope.Create(name);

    // -------------------------------------------------------------------
    // Edge construction and format
    // -------------------------------------------------------------------

    [Fact]
    public void TestEdgeBasicFormat()
    {
        var alice = XidLike("Alice");
        var edge = MakeEdge("credential-1", "foaf:Person", alice, alice);

        var expected =
            "\"credential-1\" [\n" +
            "    'isA': \"foaf:Person\"\n" +
            "    'source': \"Alice\"\n" +
            "    'target': \"Alice\"\n" +
            "]";
        Assert.Equal(expected, edge.Format());
    }

    [Fact]
    public void TestEdgeRelationshipFormat()
    {
        var alice = XidLike("Alice");
        var bob = XidLike("Bob");
        var edge = MakeEdge("knows-bob", "schema:colleague", alice, bob);

        var expected =
            "\"knows-bob\" [\n" +
            "    'isA': \"schema:colleague\"\n" +
            "    'source': \"Alice\"\n" +
            "    'target': \"Bob\"\n" +
            "]";
        Assert.Equal(expected, edge.Format());
    }

    // -------------------------------------------------------------------
    // Edge validation
    // -------------------------------------------------------------------

    [Fact]
    public void TestValidateEdgeValid()
    {
        var alice = XidLike("Alice");
        var edge = MakeEdge("cred-1", "foaf:Person", alice, alice);
        edge.ValidateEdge(); // Should not throw
    }

    [Fact]
    public void TestValidateEdgeMissingIsA()
    {
        var alice = XidLike("Alice");
        var edge = Envelope.Create("cred-1")
            .AddAssertion(KnownValuesRegistry.Source, alice)
            .AddAssertion(KnownValuesRegistry.Target, alice);
        var ex = Assert.Throws<EnvelopeException>(() => edge.ValidateEdge());
        Assert.Contains("isA", ex.Message);
    }

    [Fact]
    public void TestValidateEdgeMissingSource()
    {
        var alice = XidLike("Alice");
        var edge = Envelope.Create("cred-1")
            .AddAssertion(KnownValuesRegistry.IsA, "foaf:Person")
            .AddAssertion(KnownValuesRegistry.Target, alice);
        var ex = Assert.Throws<EnvelopeException>(() => edge.ValidateEdge());
        Assert.Contains("source", ex.Message);
    }

    [Fact]
    public void TestValidateEdgeMissingTarget()
    {
        var alice = XidLike("Alice");
        var edge = Envelope.Create("cred-1")
            .AddAssertion(KnownValuesRegistry.IsA, "foaf:Person")
            .AddAssertion(KnownValuesRegistry.Source, alice);
        var ex = Assert.Throws<EnvelopeException>(() => edge.ValidateEdge());
        Assert.Contains("target", ex.Message);
    }

    [Fact]
    public void TestValidateEdgeNoAssertions()
    {
        var edge = Envelope.Create("cred-1");
        var ex = Assert.Throws<EnvelopeException>(() => edge.ValidateEdge());
        Assert.Contains("isA", ex.Message);
    }

    [Fact]
    public void TestValidateEdgeDuplicateIsA()
    {
        var alice = XidLike("Alice");
        var edge = Envelope.Create("cred-1")
            .AddAssertion(KnownValuesRegistry.IsA, "foaf:Person")
            .AddAssertion(KnownValuesRegistry.IsA, "schema:Thing")
            .AddAssertion(KnownValuesRegistry.Source, alice)
            .AddAssertion(KnownValuesRegistry.Target, alice);
        var ex = Assert.Throws<EnvelopeException>(() => edge.ValidateEdge());
        Assert.Contains("duplicate", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("isA", ex.Message);
    }

    [Fact]
    public void TestValidateEdgeDuplicateSource()
    {
        var alice = XidLike("Alice");
        var bob = XidLike("Bob");
        var edge = Envelope.Create("cred-1")
            .AddAssertion(KnownValuesRegistry.IsA, "foaf:Person")
            .AddAssertion(KnownValuesRegistry.Source, alice)
            .AddAssertion(KnownValuesRegistry.Source, bob)
            .AddAssertion(KnownValuesRegistry.Target, alice);
        var ex = Assert.Throws<EnvelopeException>(() => edge.ValidateEdge());
        Assert.Contains("duplicate", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("source", ex.Message);
    }

    [Fact]
    public void TestValidateEdgeDuplicateTarget()
    {
        var alice = XidLike("Alice");
        var bob = XidLike("Bob");
        var edge = Envelope.Create("cred-1")
            .AddAssertion(KnownValuesRegistry.IsA, "foaf:Person")
            .AddAssertion(KnownValuesRegistry.Source, alice)
            .AddAssertion(KnownValuesRegistry.Target, alice)
            .AddAssertion(KnownValuesRegistry.Target, bob);
        var ex = Assert.Throws<EnvelopeException>(() => edge.ValidateEdge());
        Assert.Contains("duplicate", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("target", ex.Message);
    }

    [Fact]
    public void TestValidateEdgeWrappedSigned()
    {
        var alice = XidLike("Alice");
        var edge = MakeEdge("cred-1", "foaf:Person", alice, alice);

        // Wrap and sign the edge
        var signedEdge = edge.Wrap().AddSignature(TestData.AlicePrivateKey());

        // Signed (wrapped) edge should still validate
        signedEdge.ValidateEdge(); // Should not throw
    }

    // -------------------------------------------------------------------
    // Edge accessor methods
    // -------------------------------------------------------------------

    [Fact]
    public void TestEdgeIsA()
    {
        var alice = XidLike("Alice");
        var edge = MakeEdge("cred-1", "foaf:Person", alice, alice);

        var isA = edge.EdgeIsA();
        Assert.Equal("\"foaf:Person\"", isA.Format());
    }

    [Fact]
    public void TestEdgeSource()
    {
        var alice = XidLike("Alice");
        var edge = MakeEdge("cred-1", "foaf:Person", alice, alice);

        var source = edge.EdgeSource();
        Assert.Equal("\"Alice\"", source.Format());
    }

    [Fact]
    public void TestEdgeTarget()
    {
        var alice = XidLike("Alice");
        var bob = XidLike("Bob");
        var edge = MakeEdge("knows-bob", "schema:colleague", alice, bob);

        var target = edge.EdgeTarget();
        Assert.Equal("\"Bob\"", target.Format());
    }

    [Fact]
    public void TestEdgeSubject()
    {
        var alice = XidLike("Alice");
        var edge = MakeEdge("my-credential", "foaf:Person", alice, alice);

        var subject = edge.EdgeSubject();
        Assert.Equal("\"my-credential\"", subject.Format());
    }

    [Fact]
    public void TestEdgeAccessorsOnSignedEdge()
    {
        var alice = XidLike("Alice");
        var bob = XidLike("Bob");
        var edge = MakeEdge("cred-1", "foaf:Person", alice, bob);

        var signedEdge = edge.Wrap().AddSignature(TestData.AlicePrivateKey());

        // Accessors should work through the wrapped/signed layer
        var isA = signedEdge.EdgeIsA();
        Assert.Equal("\"foaf:Person\"", isA.Format());

        var source = signedEdge.EdgeSource();
        Assert.Equal("\"Alice\"", source.Format());

        var target = signedEdge.EdgeTarget();
        Assert.Equal("\"Bob\"", target.Format());

        var subject = signedEdge.EdgeSubject();
        Assert.Equal("\"cred-1\"", subject.Format());
    }

    // -------------------------------------------------------------------
    // Adding edges to envelopes
    // -------------------------------------------------------------------

    [Fact]
    public void TestAddEdgeEnvelope()
    {
        var alice = XidLike("Alice");
        var edge = MakeEdge("cred-1", "foaf:Person", alice, alice);

        var doc = Envelope.Create("Alice").AddEdgeEnvelope(edge);

        var expected =
            "\"Alice\" [\n" +
            "    'edge': \"cred-1\" [\n" +
            "        'isA': \"foaf:Person\"\n" +
            "        'source': \"Alice\"\n" +
            "        'target': \"Alice\"\n" +
            "    ]\n" +
            "]";
        Assert.Equal(expected, doc.Format());
    }

    [Fact]
    public void TestAddMultipleEdges()
    {
        var alice = XidLike("Alice");
        var bob = XidLike("Bob");
        var edge1 = MakeEdge("self-desc", "foaf:Person", alice, alice);
        var edge2 = MakeEdge("knows-bob", "schema:colleague", alice, bob);

        var doc = Envelope.Create("Alice")
            .AddEdgeEnvelope(edge1)
            .AddEdgeEnvelope(edge2);

        var edges = doc.Edges();
        Assert.Equal(2, edges.Count);

        var formatted = doc.Format();
        Assert.Contains("'edge'", formatted);
        Assert.Contains("\"self-desc\"", formatted);
        Assert.Contains("\"knows-bob\"", formatted);
    }

    // -------------------------------------------------------------------
    // Edges retrieval via envelope
    // -------------------------------------------------------------------

    [Fact]
    public void TestEdgesEmpty()
    {
        var doc = Envelope.Create("Alice");
        var edges = doc.Edges();
        Assert.Empty(edges);
    }

    [Fact]
    public void TestEdgesRetrieval()
    {
        var alice = XidLike("Alice");
        var edge1 = MakeEdge("cred-1", "foaf:Person", alice, alice);
        var edge2 = MakeEdge("cred-2", "schema:Thing", alice, alice);

        var doc = Envelope.Create("Alice")
            .AddEdgeEnvelope(edge1)
            .AddEdgeEnvelope(edge2);

        var edges = doc.Edges();
        Assert.Equal(2, edges.Count);

        // Each retrieved edge should be a valid edge
        foreach (var edge in edges)
        {
            edge.ValidateEdge();
        }
    }

    // -------------------------------------------------------------------
    // Edges container (add / get / remove / clear / len)
    // -------------------------------------------------------------------

    [Fact]
    public void TestEdgesContainerNewIsEmpty()
    {
        var edges = new Edges();
        Assert.True(edges.IsEmpty);
        Assert.Equal(0, edges.Count);
    }

    [Fact]
    public void TestEdgesContainerAddAndGet()
    {
        var alice = XidLike("Alice");
        var edge = MakeEdge("cred-1", "foaf:Person", alice, alice);
        var digest = edge.GetDigest();

        var edges = new Edges();
        edges.Add(edge);

        Assert.False(edges.IsEmpty);
        Assert.Equal(1, edges.Count);
        Assert.NotNull(edges.Get(digest));
        Assert.True(edges.Get(digest)!.IsEquivalentTo(edge));
    }

    [Fact]
    public void TestEdgesContainerRemove()
    {
        var alice = XidLike("Alice");
        var edge = MakeEdge("cred-1", "foaf:Person", alice, alice);
        var digest = edge.GetDigest();

        var edges = new Edges();
        edges.Add(edge);

        var removed = edges.Remove(digest);
        Assert.NotNull(removed);
        Assert.True(edges.IsEmpty);
    }

    [Fact]
    public void TestEdgesContainerRemoveNonexistent()
    {
        var alice = XidLike("Alice");
        var edge = MakeEdge("cred-1", "foaf:Person", alice, alice);

        var edges = new Edges();
        var removed = edges.Remove(edge.GetDigest());
        Assert.Null(removed);
    }

    [Fact]
    public void TestEdgesContainerClear()
    {
        var alice = XidLike("Alice");
        var edge1 = MakeEdge("cred-1", "foaf:Person", alice, alice);
        var edge2 = MakeEdge("cred-2", "schema:Thing", alice, alice);

        var edges = new Edges();
        edges.Add(edge1);
        edges.Add(edge2);
        Assert.Equal(2, edges.Count);

        edges.Clear();
        Assert.True(edges.IsEmpty);
        Assert.Equal(0, edges.Count);
    }

    [Fact]
    public void TestEdgesContainerIter()
    {
        var alice = XidLike("Alice");
        var edge1 = MakeEdge("cred-1", "foaf:Person", alice, alice);
        var edge2 = MakeEdge("cred-2", "schema:Thing", alice, alice);

        var edges = new Edges();
        edges.Add(edge1);
        edges.Add(edge2);

        var count = edges.Entries.Count();
        Assert.Equal(2, count);
    }

    // -------------------------------------------------------------------
    // Edges container round-trip: AddToEnvelope / FromEnvelope
    // -------------------------------------------------------------------

    [Fact]
    public void TestEdgesContainerRoundtrip()
    {
        var alice = XidLike("Alice");
        var edge1 = MakeEdge("cred-1", "foaf:Person", alice, alice);
        var edge2 = MakeEdge("cred-2", "schema:Thing", alice, alice);

        var edges = new Edges();
        edges.Add(edge1);
        edges.Add(edge2);

        // Serialize to envelope
        var doc = Envelope.Create("Alice");
        var docWithEdges = edges.AddToEnvelope(doc);

        // Deserialize back
        var recovered = Edges.FromEnvelope(docWithEdges);
        Assert.Equal(2, recovered.Count);
        Assert.NotNull(recovered.Get(edge1.GetDigest()));
        Assert.NotNull(recovered.Get(edge2.GetDigest()));
    }

    [Fact]
    public void TestEdgesContainerRoundtripEmpty()
    {
        var edges = new Edges();
        var doc = Envelope.Create("Alice");
        var docWithEdges = edges.AddToEnvelope(doc);

        var recovered = Edges.FromEnvelope(docWithEdges);
        Assert.True(recovered.IsEmpty);
    }

    [Fact]
    public void TestEdgesContainerRoundtripPreservesFormat()
    {
        var alice = XidLike("Alice");
        var bob = XidLike("Bob");
        var edge = MakeEdge("knows-bob", "schema:colleague", alice, bob);

        var edges = new Edges();
        edges.Add(edge);

        var doc = edges.AddToEnvelope(Envelope.Create("Alice"));

        var expected =
            "\"Alice\" [\n" +
            "    'edge': \"knows-bob\" [\n" +
            "        'isA': \"schema:colleague\"\n" +
            "        'source': \"Alice\"\n" +
            "        'target': \"Bob\"\n" +
            "    ]\n" +
            "]";
        Assert.Equal(expected, doc.Format());

        var recovered = Edges.FromEnvelope(doc);
        Assert.Equal(1, recovered.Count);
    }

    // -------------------------------------------------------------------
    // Edgeable trait
    // -------------------------------------------------------------------

    [Fact]
    public void TestEdgeableDefaultMethods()
    {
        var alice = XidLike("Alice");
        var edge = MakeEdge("cred-1", "foaf:Person", alice, alice);
        var digest = edge.GetDigest();

        var edges = new Edges();
        edges.Add(edge);

        Assert.False(edges.IsEmpty);
        Assert.Equal(1, edges.Count);
        Assert.NotNull(edges.Get(digest));

        var removed = edges.Remove(digest);
        Assert.NotNull(removed);
        Assert.True(edges.IsEmpty);
    }

    // -------------------------------------------------------------------
    // edges_matching -- filtering by criteria
    // -------------------------------------------------------------------

    [Fact]
    public void TestEdgesMatchingNoFilters()
    {
        var alice = XidLike("Alice");
        var bob = XidLike("Bob");
        var edge1 = MakeEdge("self-desc", "foaf:Person", alice, alice);
        var edge2 = MakeEdge("knows-bob", "schema:colleague", alice, bob);

        var doc = Envelope.Create("Alice")
            .AddEdgeEnvelope(edge1)
            .AddEdgeEnvelope(edge2);

        // No filters => all edges
        var matching = doc.EdgesMatching();
        Assert.Equal(2, matching.Count);
    }

    [Fact]
    public void TestEdgesMatchingByIsA()
    {
        var alice = XidLike("Alice");
        var bob = XidLike("Bob");
        var edge1 = MakeEdge("self-desc", "foaf:Person", alice, alice);
        var edge2 = MakeEdge("knows-bob", "schema:colleague", alice, bob);
        var edge3 = MakeEdge("self-thing", "foaf:Person", alice, alice);

        var doc = Envelope.Create("Alice")
            .AddEdgeEnvelope(edge1)
            .AddEdgeEnvelope(edge2)
            .AddEdgeEnvelope(edge3);

        var isAPerson = Envelope.Create("foaf:Person");
        var matching = doc.EdgesMatching(isA: isAPerson);
        Assert.Equal(2, matching.Count);

        var isAColleague = Envelope.Create("schema:colleague");
        matching = doc.EdgesMatching(isA: isAColleague);
        Assert.Single(matching);

        var isANone = Envelope.Create("nonexistent");
        matching = doc.EdgesMatching(isA: isANone);
        Assert.Empty(matching);
    }

    [Fact]
    public void TestEdgesMatchingBySource()
    {
        var alice = XidLike("Alice");
        var bob = XidLike("Bob");
        var edge1 = MakeEdge("alice-claim", "foaf:Person", alice, alice);
        var edge2 = MakeEdge("bob-claim", "foaf:Person", bob, alice);

        var doc = Envelope.Create("Alice")
            .AddEdgeEnvelope(edge1)
            .AddEdgeEnvelope(edge2);

        var matching = doc.EdgesMatching(source: alice);
        Assert.Single(matching);

        matching = doc.EdgesMatching(source: bob);
        Assert.Single(matching);

        var carol = XidLike("Carol");
        matching = doc.EdgesMatching(source: carol);
        Assert.Empty(matching);
    }

    [Fact]
    public void TestEdgesMatchingByTarget()
    {
        var alice = XidLike("Alice");
        var bob = XidLike("Bob");
        var edge1 = MakeEdge("self-desc", "foaf:Person", alice, alice);
        var edge2 = MakeEdge("knows-bob", "schema:colleague", alice, bob);

        var doc = Envelope.Create("Alice")
            .AddEdgeEnvelope(edge1)
            .AddEdgeEnvelope(edge2);

        var matching = doc.EdgesMatching(target: alice);
        Assert.Single(matching);

        matching = doc.EdgesMatching(target: bob);
        Assert.Single(matching);
    }

    [Fact]
    public void TestEdgesMatchingBySubject()
    {
        var alice = XidLike("Alice");
        var edge1 = MakeEdge("self-desc", "foaf:Person", alice, alice);
        var edge2 = MakeEdge("cred-2", "schema:Thing", alice, alice);

        var doc = Envelope.Create("Alice")
            .AddEdgeEnvelope(edge1)
            .AddEdgeEnvelope(edge2);

        var subjectFilter = Envelope.Create("self-desc");
        var matching = doc.EdgesMatching(subject: subjectFilter);
        Assert.Single(matching);

        subjectFilter = Envelope.Create("nonexistent");
        matching = doc.EdgesMatching(subject: subjectFilter);
        Assert.Empty(matching);
    }

    [Fact]
    public void TestEdgesMatchingCombinedFilters()
    {
        var alice = XidLike("Alice");
        var bob = XidLike("Bob");
        var edge1 = MakeEdge("self-desc", "foaf:Person", alice, alice);
        var edge2 = MakeEdge("self-thing", "foaf:Person", alice, alice);
        var edge3 = MakeEdge("knows-bob", "foaf:Person", alice, bob);

        var doc = Envelope.Create("Alice")
            .AddEdgeEnvelope(edge1)
            .AddEdgeEnvelope(edge2)
            .AddEdgeEnvelope(edge3);

        // All three are foaf:Person
        var isA = Envelope.Create("foaf:Person");
        var matching = doc.EdgesMatching(isA: isA);
        Assert.Equal(3, matching.Count);

        // foaf:Person + target Alice => 2 (self-desc, self-thing)
        matching = doc.EdgesMatching(isA: isA, target: alice);
        Assert.Equal(2, matching.Count);

        // foaf:Person + target Bob => 1 (knows-bob)
        matching = doc.EdgesMatching(isA: isA, target: bob);
        Assert.Single(matching);

        // foaf:Person + target Alice + subject "self-desc" => 1
        var subj = Envelope.Create("self-desc");
        matching = doc.EdgesMatching(isA: isA, target: alice, subject: subj);
        Assert.Single(matching);

        // foaf:Person + source Alice + target Bob + subject "knows-bob" => 1
        subj = Envelope.Create("knows-bob");
        matching = doc.EdgesMatching(isA: isA, source: alice, target: bob, subject: subj);
        Assert.Single(matching);

        // All filters that match nothing
        subj = Envelope.Create("nonexistent");
        matching = doc.EdgesMatching(isA: isA, source: alice, target: alice, subject: subj);
        Assert.Empty(matching);
    }

    // -------------------------------------------------------------------
    // Signed edges with format verification
    // -------------------------------------------------------------------

    [Fact]
    public void TestSignedEdgeFormat()
    {
        var alice = XidLike("Alice");
        var edge = MakeEdge("cred-1", "foaf:Person", alice, alice);

        var signedEdge = edge.Wrap().AddSignature(TestData.AlicePrivateKey());

        var expected =
            "{\n" +
            "    \"cred-1\" [\n" +
            "        'isA': \"foaf:Person\"\n" +
            "        'source': \"Alice\"\n" +
            "        'target': \"Alice\"\n" +
            "    ]\n" +
            "} [\n" +
            "    'signed': Signature\n" +
            "]";
        Assert.Equal(expected, signedEdge.Format());
    }

    [Fact]
    public void TestSignedEdgeOnDocumentFormat()
    {
        var alice = XidLike("Alice");
        var edge = MakeEdge("cred-1", "foaf:Person", alice, alice);
        var signedEdge = edge.Wrap().AddSignature(TestData.AlicePrivateKey());

        var doc = Envelope.Create("Alice")
            .AddAssertion("knows", "Bob")
            .AddEdgeEnvelope(signedEdge);

        var formatted = doc.Format();
        Assert.Contains("'edge': {", formatted);
        Assert.Contains("'signed': Signature", formatted);
        Assert.Contains("'isA': \"foaf:Person\"", formatted);
    }

    // -------------------------------------------------------------------
    // Edge coexistence with attachments
    // -------------------------------------------------------------------

    [Fact]
    public void TestEdgesCoexistWithAttachments()
    {
        var alice = XidLike("Alice");
        var edge = MakeEdge("cred-1", "foaf:Person", alice, alice);

        var doc = Envelope.Create("Alice")
            .AddAttachment(
                "Metadata",
                "com.example",
                "https://example.com/v1")
            .AddEdgeEnvelope(edge);

        // Both should be present
        Assert.Single(doc.Edges());
        Assert.Single(doc.Attachments());

        var formatted = doc.Format();
        Assert.Contains("'edge'", formatted);
        Assert.Contains("'attachment'", formatted);
    }

    // -------------------------------------------------------------------
    // Edge UR round-trip
    // -------------------------------------------------------------------

    [Fact]
    public void TestEdgeUrRoundtrip()
    {
        TagsRegistry.RegisterTags();

        var alice = XidLike("Alice");
        var edge = MakeEdge("cred-1", "foaf:Person", alice, alice);

        var doc = Envelope.Create("Alice").AddEdgeEnvelope(edge);

        // Round-trip through UR
        var ur = BCUR.UR.Create("envelope", doc.TaggedCbor());
        var urString = ur.ToUrString();
        var receivedUr = BCUR.UR.FromUrString(urString);
        var recovered = Envelope.FromTaggedCbor(receivedUr.Cbor);
        Assert.True(recovered.IsEquivalentTo(doc));

        var recoveredEdges = recovered.Edges();
        Assert.Single(recoveredEdges);
        Assert.True(recoveredEdges[0].IsEquivalentTo(edge));
    }

    [Fact]
    public void TestMultipleEdgesUrRoundtrip()
    {
        TagsRegistry.RegisterTags();

        var alice = XidLike("Alice");
        var bob = XidLike("Bob");
        var edge1 = MakeEdge("self-desc", "foaf:Person", alice, alice);
        var edge2 = MakeEdge("knows-bob", "schema:colleague", alice, bob);
        var edge3 = MakeEdge("project", "schema:CreativeWork", alice, bob);

        var doc = Envelope.Create("Alice")
            .AddEdgeEnvelope(edge1)
            .AddEdgeEnvelope(edge2)
            .AddEdgeEnvelope(edge3);

        var ur = BCUR.UR.Create("envelope", doc.TaggedCbor());
        var urString = ur.ToUrString();
        var receivedUr = BCUR.UR.FromUrString(urString);
        var recovered = Envelope.FromTaggedCbor(receivedUr.Cbor);
        Assert.True(recovered.IsEquivalentTo(doc));

        var recoveredEdges = recovered.Edges();
        Assert.Equal(3, recoveredEdges.Count);
    }

    // -------------------------------------------------------------------
    // Edge with extra assertions beyond the required three
    // -------------------------------------------------------------------

    [Fact]
    public void TestEdgeWithAdditionalAssertions()
    {
        var alice = XidLike("Alice");
        var bob = XidLike("Bob");

        var edge = Envelope.Create("knows-bob")
            .AddAssertion(KnownValuesRegistry.IsA, "schema:colleague")
            .AddAssertion(KnownValuesRegistry.Source, alice)
            .AddAssertion(KnownValuesRegistry.Target, bob)
            .AddAssertion("department", "Engineering")
            .AddAssertion("since", "2024-01-15");

        Assert.Throws<EnvelopeException>(() => edge.ValidateEdge());
    }

    [Fact]
    public void TestEdgeWithClaimDetailOnTarget()
    {
        var alice = XidLike("Alice");
        var target = XidLike("Bob")
            .AddAssertion("department", "Engineering")
            .AddAssertion("since", "2024-01-15");
        var edge = MakeEdge("knows-bob", "schema:colleague", alice, target);
        edge.ValidateEdge(); // Should not throw
    }

    [Fact]
    public void TestEdgeWithClaimDetailOnSource()
    {
        var source = XidLike("Alice").AddAssertion(
            KnownValuesRegistry.DereferenceVia,
            BCComponents.URI.FromString("https://example.com/xid/"));
        var target = XidLike("Bob");
        var edge = MakeEdge("knows-bob", "schema:colleague", source, target);
        edge.ValidateEdge(); // Should not throw
    }
}

using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class ElisionTests
{
    private static Envelope BasicEnvelope() => Envelope.Create("Hello.");

    private static Envelope AssertionEnvelope() => Envelope.CreateAssertion("knows", "Bob");

    private static Envelope SingleAssertionEnvelope() =>
        Envelope.Create("Alice").AddAssertion("knows", "Bob");

    private static Envelope DoubleAssertionEnvelope() =>
        Envelope.Create("Alice")
            .AddAssertion("knows", "Bob")
            .AddAssertion("knows", "Carol");

    [Fact]
    public void TestEnvelopeElision()
    {
        var e1 = BasicEnvelope();

        var e2 = e1.Elide();
        Assert.True(e1.IsEquivalentTo(e2));
        Assert.False(e1.IsIdenticalTo(e2));

        Assert.Equal("ELIDED", e2.Format());

        Assert.Equal(
            "200(   / envelope /\n" +
            "    h'8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59'\n" +
            ")",
            e2.DiagnosticAnnotated());

        var e3 = e2.Unelide(e1);
        Assert.True(e3.IsEquivalentTo(e1));
        Assert.Equal("\"Hello.\"", e3.Format());
    }

    [Fact]
    public void TestSingleAssertionRemoveElision()
    {
        // The original Envelope
        var e1 = SingleAssertionEnvelope();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "]",
            e1.Format());

        // Elide the entire envelope
        var e2 = e1.ElideRemovingTarget(e1).CheckEncoding();
        Assert.Equal("ELIDED", e2.Format());

        // Elide just the envelope's subject
        var e3 = e1.ElideRemovingTarget(Envelope.Create("Alice")).CheckEncoding();
        Assert.Equal(
            "ELIDED [\n" +
            "    \"knows\": \"Bob\"\n" +
            "]",
            e3.Format());

        // Elide just the assertion's predicate
        var e4 = e1.ElideRemovingTarget(Envelope.Create("knows")).CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    ELIDED: \"Bob\"\n" +
            "]",
            e4.Format());

        // Elide just the assertion's object
        var e5 = e1.ElideRemovingTarget(Envelope.Create("Bob")).CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": ELIDED\n" +
            "]",
            e5.Format());

        // Elide the entire assertion
        var e6 = e1.ElideRemovingTarget(AssertionEnvelope()).CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    ELIDED\n" +
            "]",
            e6.Format());
    }

    [Fact]
    public void TestDoubleAssertionRemoveElision()
    {
        // The original Envelope
        var e1 = DoubleAssertionEnvelope();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    \"knows\": \"Carol\"\n" +
            "]",
            e1.Format());

        // Elide the entire envelope
        var e2 = e1.ElideRemovingTarget(e1).CheckEncoding();
        Assert.Equal("ELIDED", e2.Format());

        // Elide just the envelope's subject
        var e3 = e1.ElideRemovingTarget(Envelope.Create("Alice")).CheckEncoding();
        Assert.Equal(
            "ELIDED [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    \"knows\": \"Carol\"\n" +
            "]",
            e3.Format());

        // Elide just the assertion's predicate
        var e4 = e1.ElideRemovingTarget(Envelope.Create("knows")).CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    ELIDED: \"Bob\"\n" +
            "    ELIDED: \"Carol\"\n" +
            "]",
            e4.Format());

        // Elide just the assertion's object
        var e5 = e1.ElideRemovingTarget(Envelope.Create("Bob")).CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Carol\"\n" +
            "    \"knows\": ELIDED\n" +
            "]",
            e5.Format());

        // Elide the entire assertion
        var e6 = e1.ElideRemovingTarget(AssertionEnvelope()).CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Carol\"\n" +
            "    ELIDED\n" +
            "]",
            e6.Format());
    }

    [Fact]
    public void TestSingleAssertionRevealElision()
    {
        // The original Envelope
        var e1 = SingleAssertionEnvelope();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "]",
            e1.Format());

        // Elide revealing nothing
        var e2 = e1.ElideRevealingArray(Array.Empty<Envelope>()).CheckEncoding();
        Assert.Equal("ELIDED", e2.Format());

        // Reveal just the envelope's structure
        var e3 = e1.ElideRevealingArray(new[] { e1 }).CheckEncoding();
        Assert.Equal(
            "ELIDED [\n" +
            "    ELIDED\n" +
            "]",
            e3.Format());

        // Reveal just the envelope's subject
        var e4 = e1.ElideRevealingArray(new[] { e1, Envelope.Create("Alice") }).CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    ELIDED\n" +
            "]",
            e4.Format());

        // Reveal just the assertion's structure.
        var e5 = e1.ElideRevealingArray(new[] { e1, AssertionEnvelope() }).CheckEncoding();
        Assert.Equal(
            "ELIDED [\n" +
            "    ELIDED: ELIDED\n" +
            "]",
            e5.Format());

        // Reveal just the assertion's predicate
        var e6 = e1.ElideRevealingArray(new[] {
            e1,
            AssertionEnvelope(),
            Envelope.Create("knows"),
        }).CheckEncoding();
        Assert.Equal(
            "ELIDED [\n" +
            "    \"knows\": ELIDED\n" +
            "]",
            e6.Format());

        // Reveal just the assertion's object
        var e7 = e1.ElideRevealingArray(new[] {
            e1,
            AssertionEnvelope(),
            Envelope.Create("Bob"),
        }).CheckEncoding();
        Assert.Equal(
            "ELIDED [\n" +
            "    ELIDED: \"Bob\"\n" +
            "]",
            e7.Format());
    }

    [Fact]
    public void TestDoubleAssertionRevealElision()
    {
        // The original Envelope
        var e1 = DoubleAssertionEnvelope();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    \"knows\": \"Carol\"\n" +
            "]",
            e1.Format());

        // Elide revealing nothing
        var e2 = e1.ElideRevealingArray(Array.Empty<Envelope>()).CheckEncoding();
        Assert.Equal("ELIDED", e2.Format());

        // Reveal just the envelope's structure
        var e3 = e1.ElideRevealingArray(new[] { e1 }).CheckEncoding();
        Assert.Equal(
            "ELIDED [\n" +
            "    ELIDED (2)\n" +
            "]",
            e3.Format());

        // Reveal just the envelope's subject
        var e4 = e1.ElideRevealingArray(new[] { e1, Envelope.Create("Alice") }).CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    ELIDED (2)\n" +
            "]",
            e4.Format());

        // Reveal just the assertion's structure.
        var e5 = e1.ElideRevealingArray(new[] { e1, AssertionEnvelope() }).CheckEncoding();
        Assert.Equal(
            "ELIDED [\n" +
            "    ELIDED: ELIDED\n" +
            "    ELIDED\n" +
            "]",
            e5.Format());

        // Reveal just the assertion's predicate
        var e6 = e1.ElideRevealingArray(new[] {
            e1,
            AssertionEnvelope(),
            Envelope.Create("knows"),
        }).CheckEncoding();
        Assert.Equal(
            "ELIDED [\n" +
            "    \"knows\": ELIDED\n" +
            "    ELIDED\n" +
            "]",
            e6.Format());

        // Reveal just the assertion's object
        var e7 = e1.ElideRevealingArray(new[] {
            e1,
            AssertionEnvelope(),
            Envelope.Create("Bob"),
        }).CheckEncoding();
        Assert.Equal(
            "ELIDED [\n" +
            "    ELIDED: \"Bob\"\n" +
            "    ELIDED\n" +
            "]",
            e7.Format());
    }

    [Fact]
    public void TestDigests()
    {
        var e1 = DoubleAssertionEnvelope();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    \"knows\": \"Carol\"\n" +
            "]",
            e1.Format());

        var e2 = e1.ElideRevealingSet(e1.Digests(0)).CheckEncoding();
        Assert.Equal("ELIDED", e2.Format());

        var e3 = e1.ElideRevealingSet(e1.Digests(1)).CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    ELIDED (2)\n" +
            "]",
            e3.Format());

        var e4 = e1.ElideRevealingSet(e1.Digests(2)).CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    ELIDED: ELIDED\n" +
            "    ELIDED: ELIDED\n" +
            "]",
            e4.Format());

        var e5 = e1.ElideRevealingSet(e1.Digests(3)).CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    \"knows\": \"Carol\"\n" +
            "]",
            e5.Format());
    }

    [Fact]
    public void TestTargetReveal()
    {
        var e1 = DoubleAssertionEnvelope().AddAssertion("livesAt", "123 Main St.");
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    \"knows\": \"Carol\"\n" +
            "    \"livesAt\": \"123 Main St.\"\n" +
            "]",
            e1.Format());

        var target = new HashSet<Digest>();
        // Reveal the Envelope structure
        target.UnionWith(e1.Digests(1));
        // Reveal everything about the subject
        target.UnionWith(e1.Subject.DeepDigests());
        // Reveal everything about one of the assertions
        target.UnionWith(AssertionEnvelope().DeepDigests());
        // Reveal the specific `livesAt` assertion
        target.UnionWith(e1.AssertionWithPredicate("livesAt").DeepDigests());
        var e2 = e1.ElideRevealingSet(target).CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    \"livesAt\": \"123 Main St.\"\n" +
            "    ELIDED\n" +
            "]",
            e2.Format());
    }

    [Fact]
    public void TestTargetedRemove()
    {
        var e1 = DoubleAssertionEnvelope().AddAssertion("livesAt", "123 Main St.");
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    \"knows\": \"Carol\"\n" +
            "    \"livesAt\": \"123 Main St.\"\n" +
            "]",
            e1.Format());

        var target2 = new HashSet<Digest>();
        // Hide one of the assertions
        target2.UnionWith(AssertionEnvelope().Digests(1));
        var e2 = e1.ElideRemovingSet(target2).CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Carol\"\n" +
            "    \"livesAt\": \"123 Main St.\"\n" +
            "    ELIDED\n" +
            "]",
            e2.Format());

        var target3 = new HashSet<Digest>();
        // Hide one of the assertions by finding its predicate
        target3.UnionWith(e1.AssertionWithPredicate("livesAt").DeepDigests());
        var e3 = e1.ElideRemovingSet(target3).CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    \"knows\": \"Carol\"\n" +
            "    ELIDED\n" +
            "]",
            e3.Format());

        // Semantically equivalent
        Assert.True(e1.IsEquivalentTo(e3));

        // Structurally different
        Assert.False(e1.IsIdenticalTo(e3));
    }

    [Fact]
    public void TestWalkReplaceBasic()
    {
        // Create envelopes
        var alice = Envelope.Create("Alice");
        var bob = Envelope.Create("Bob");
        var charlie = Envelope.Create("Charlie");

        // Create an envelope with Bob referenced multiple times
        var envelope = alice
            .AddAssertion("knows", bob)
            .AddAssertion("likes", bob);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    \"likes\": \"Bob\"\n" +
            "]",
            envelope.Format());

        // Replace all instances of Bob with Charlie
        var target = new HashSet<Digest> { bob.GetDigest() };

        var modified = envelope.WalkReplace(target, charlie);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Charlie\"\n" +
            "    \"likes\": \"Charlie\"\n" +
            "]",
            modified.Format());

        // The structure is different (different content)
        Assert.False(modified.IsEquivalentTo(envelope));
    }

    [Fact]
    public void TestWalkReplaceSubject()
    {
        var alice = Envelope.Create("Alice");
        var bob = Envelope.Create("Bob");
        var carol = Envelope.Create("Carol");

        var envelope = alice.AddAssertion("knows", bob);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "]",
            envelope.Format());

        // Replace the subject (Alice) with Carol
        var target = new HashSet<Digest> { alice.GetDigest() };

        var modified = envelope.WalkReplace(target, carol);

        Assert.Equal(
            "\"Carol\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "]",
            modified.Format());
    }

    [Fact]
    public void TestWalkReplaceNested()
    {
        var alice = Envelope.Create("Alice");
        var bob = Envelope.Create("Bob");
        var charlie = Envelope.Create("Charlie");

        // Create a nested structure with Bob appearing at multiple levels
        var inner = bob.AddAssertion("friend", bob);
        var envelope = alice.AddAssertion("knows", inner);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\" [\n" +
            "        \"friend\": \"Bob\"\n" +
            "    ]\n" +
            "]",
            envelope.Format());

        // Replace all instances of Bob with Charlie
        var target = new HashSet<Digest> { bob.GetDigest() };

        var modified = envelope.WalkReplace(target, charlie);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Charlie\" [\n" +
            "        \"friend\": \"Charlie\"\n" +
            "    ]\n" +
            "]",
            modified.Format());
    }

    [Fact]
    public void TestWalkReplaceWrapped()
    {
        var alice = Envelope.Create("Alice");
        var bob = Envelope.Create("Bob");
        var charlie = Envelope.Create("Charlie");

        // Create a wrapped envelope containing Bob
        var wrapped = bob.Wrap();
        var envelope = alice.AddAssertion("data", wrapped);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"data\": {\n" +
            "        \"Bob\"\n" +
            "    }\n" +
            "]",
            envelope.Format());

        // Replace Bob with Charlie
        var target = new HashSet<Digest> { bob.GetDigest() };

        var modified = envelope.WalkReplace(target, charlie);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"data\": {\n" +
            "        \"Charlie\"\n" +
            "    }\n" +
            "]",
            modified.Format());
    }

    [Fact]
    public void TestWalkReplaceNoMatch()
    {
        var alice = Envelope.Create("Alice");
        var bob = Envelope.Create("Bob");
        var charlie = Envelope.Create("Charlie");
        var dave = Envelope.Create("Dave");

        var envelope = alice.AddAssertion("knows", bob);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "]",
            envelope.Format());

        // Try to replace Dave (who doesn't exist in the envelope)
        var target = new HashSet<Digest> { dave.GetDigest() };

        var modified = envelope.WalkReplace(target, charlie);

        // Should be identical since nothing matched
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "]",
            modified.Format());

        Assert.True(modified.IsIdenticalTo(envelope));
    }

    [Fact]
    public void TestWalkReplaceMultipleTargets()
    {
        var alice = Envelope.Create("Alice");
        var bob = Envelope.Create("Bob");
        var carol = Envelope.Create("Carol");
        var replacement = Envelope.Create("REDACTED");

        var envelope = alice
            .AddAssertion("knows", bob)
            .AddAssertion("likes", carol);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    \"likes\": \"Carol\"\n" +
            "]",
            envelope.Format());

        // Replace both Bob and Carol with REDACTED
        var target = new HashSet<Digest> { bob.GetDigest(), carol.GetDigest() };

        var modified = envelope.WalkReplace(target, replacement);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"REDACTED\"\n" +
            "    \"likes\": \"REDACTED\"\n" +
            "]",
            modified.Format());
    }

    [Fact]
    public void TestWalkReplaceElided()
    {
        var alice = Envelope.Create("Alice");
        var bob = Envelope.Create("Bob");
        var charlie = Envelope.Create("Charlie");

        // Create an envelope with Bob, then elide Bob
        var envelope = alice
            .AddAssertion("knows", bob)
            .AddAssertion("likes", bob);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    \"likes\": \"Bob\"\n" +
            "]",
            envelope.Format());

        // Elide Bob
        var elided = envelope.ElideRemovingTarget(bob);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": ELIDED\n" +
            "    \"likes\": ELIDED\n" +
            "]",
            elided.Format());

        // Replace the elided Bob with Charlie
        // This works because the elided node has Bob's digest
        var target = new HashSet<Digest> { bob.GetDigest() };

        var modified = elided.WalkReplace(target, charlie);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Charlie\"\n" +
            "    \"likes\": \"Charlie\"\n" +
            "]",
            modified.Format());

        // Verify that the elided nodes were replaced
        Assert.False(modified.IsEquivalentTo(envelope));
        Assert.False(modified.IsEquivalentTo(elided));
    }

    [Fact]
    public void TestWalkReplaceAssertionWithNonAssertionFails()
    {
        var alice = Envelope.Create("Alice");
        var bob = Envelope.Create("Bob");
        var charlie = Envelope.Create("Charlie");

        var envelope = alice.AddAssertion("knows", bob);

        // Get the assertion's digest
        var knowsAssertion = envelope.AssertionWithPredicate("knows");
        var assertionDigest = knowsAssertion.GetDigest();

        // Try to replace the entire assertion with Charlie (a non-assertion)
        var target = new HashSet<Digest> { assertionDigest };

        var ex = Assert.Throws<EnvelopeException>(() => envelope.WalkReplace(target, charlie));
        Assert.Equal("invalid format", ex.Message);
    }
}

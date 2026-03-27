using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.DCbor;
using Xunit;

namespace BlockchainCommons.BCEnvelope.Tests;

/// <summary>
/// Tests for nested envelope structures and complex subject-assertion hierarchies.
/// </summary>
public class CoreNestingTests
{
    [Fact]
    public void TestPredicateEnclosures()
    {
        var alice = Envelope.Create("Alice");
        var knows = Envelope.Create("knows");
        var bob = Envelope.Create("Bob");

        var a = Envelope.Create("A");
        var b = Envelope.Create("B");

        var knowsBob = Envelope.CreateAssertion(knows, bob);
        Assert.Equal("\"knows\": \"Bob\"", knowsBob.Format());

        var ab = Envelope.CreateAssertion(a, b);
        Assert.Equal("\"A\": \"B\"", ab.Format());

        var knowsAbBob = Envelope.CreateAssertion(
            knows.AddAssertionEnvelope(ab),
            bob
        ).CheckEncoding();
        Assert.Equal(
            "\"knows\" [\n" +
            "    \"A\": \"B\"\n" +
            "]\n" +
            ": \"Bob\"",
            knowsAbBob.Format());

        var knowsBobAb = Envelope.CreateAssertion(
            knows,
            bob.AddAssertionEnvelope(ab)
        ).CheckEncoding();
        Assert.Equal(
            "\"knows\": \"Bob\" [\n" +
            "    \"A\": \"B\"\n" +
            "]",
            knowsBobAb.Format());

        var knowsBobEncloseAb = knowsBob
            .AddAssertionEnvelope(ab)
            .CheckEncoding();
        Assert.Equal(
            "{\n" +
            "    \"knows\": \"Bob\"\n" +
            "} [\n" +
            "    \"A\": \"B\"\n" +
            "]",
            knowsBobEncloseAb.Format());

        var aliceKnowsBob = alice
            .AddAssertionEnvelope(knowsBob)
            .CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "]",
            aliceKnowsBob.Format());

        var aliceAbKnowsBob = aliceKnowsBob
            .AddAssertionEnvelope(ab)
            .CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"A\": \"B\"\n" +
            "    \"knows\": \"Bob\"\n" +
            "]",
            aliceAbKnowsBob.Format());

        var aliceKnowsAbBob = alice
            .AddAssertionEnvelope(Envelope.CreateAssertion(
                knows.AddAssertionEnvelope(ab),
                bob))
            .CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\" [\n" +
            "        \"A\": \"B\"\n" +
            "    ]\n" +
            "    : \"Bob\"\n" +
            "]",
            aliceKnowsAbBob.Format());

        var aliceKnowsBobAb = alice
            .AddAssertionEnvelope(Envelope.CreateAssertion(
                knows,
                bob.AddAssertionEnvelope(ab)))
            .CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\" [\n" +
            "        \"A\": \"B\"\n" +
            "    ]\n" +
            "]",
            aliceKnowsBobAb.Format());

        var aliceKnowsAbBobAb = alice
            .AddAssertionEnvelope(Envelope.CreateAssertion(
                knows.AddAssertionEnvelope(ab),
                bob.AddAssertionEnvelope(ab)))
            .CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\" [\n" +
            "        \"A\": \"B\"\n" +
            "    ]\n" +
            "    : \"Bob\" [\n" +
            "        \"A\": \"B\"\n" +
            "    ]\n" +
            "]",
            aliceKnowsAbBobAb.Format());

        var aliceAbKnowsAbBobAb = alice
            .AddAssertionEnvelope(ab)
            .AddAssertionEnvelope(Envelope.CreateAssertion(
                knows.AddAssertionEnvelope(ab),
                bob.AddAssertionEnvelope(ab)))
            .CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"A\": \"B\"\n" +
            "    \"knows\" [\n" +
            "        \"A\": \"B\"\n" +
            "    ]\n" +
            "    : \"Bob\" [\n" +
            "        \"A\": \"B\"\n" +
            "    ]\n" +
            "]",
            aliceAbKnowsAbBobAb.Format());

        var aliceAbKnowsAbBobAbEncloseAb = alice
            .AddAssertionEnvelope(ab)
            .AddAssertionEnvelope(
                Envelope.CreateAssertion(
                    knows.AddAssertionEnvelope(ab),
                    bob.AddAssertionEnvelope(ab))
                .AddAssertionEnvelope(ab))
            .CheckEncoding();
        Assert.Equal(
            "\"Alice\" [\n" +
            "    {\n" +
            "        \"knows\" [\n" +
            "            \"A\": \"B\"\n" +
            "        ]\n" +
            "        : \"Bob\" [\n" +
            "            \"A\": \"B\"\n" +
            "        ]\n" +
            "    } [\n" +
            "        \"A\": \"B\"\n" +
            "    ]\n" +
            "    \"A\": \"B\"\n" +
            "]",
            aliceAbKnowsAbBobAbEncloseAb.Format());
    }

    [Fact]
    public void TestNestingPlaintext()
    {
        var envelope = Envelope.Create("Hello.");
        Assert.Equal("\"Hello.\"", envelope.Format());

        var elidedEnvelope = envelope.Elide();
        Assert.True(elidedEnvelope.IsEquivalentTo(envelope));
        Assert.Equal("ELIDED", elidedEnvelope.Format());
    }

    [Fact]
    public void TestNestingOnce()
    {
        var envelope = Envelope.Create("Hello.").Wrap().CheckEncoding();
        Assert.Equal(
            "{\n" +
            "    \"Hello.\"\n" +
            "}",
            envelope.Format());

        var elidedEnvelope = Envelope.Create("Hello.")
            .Elide()
            .Wrap()
            .CheckEncoding();
        Assert.True(elidedEnvelope.IsEquivalentTo(envelope));
        Assert.Equal(
            "{\n" +
            "    ELIDED\n" +
            "}",
            elidedEnvelope.Format());
    }

    [Fact]
    public void TestNestingTwice()
    {
        var envelope = Envelope.Create("Hello.")
            .Wrap()
            .Wrap()
            .CheckEncoding();

        Assert.Equal(
            "{\n" +
            "    {\n" +
            "        \"Hello.\"\n" +
            "    }\n" +
            "}",
            envelope.Format());

        var target = envelope.TryUnwrap().TryUnwrap();
        var elidedEnvelope = envelope.ElideRemovingTarget(target);

        Assert.Equal(
            "{\n" +
            "    {\n" +
            "        ELIDED\n" +
            "    }\n" +
            "}",
            elidedEnvelope.Format());
        Assert.True(envelope.IsEquivalentTo(elidedEnvelope));
        Assert.True(envelope.IsEquivalentTo(elidedEnvelope));
    }

    [Fact]
    public void TestAssertionsOnAllPartsOfEnvelope()
    {
        var predicate = Envelope.Create("predicate")
            .AddAssertion("predicate-predicate", "predicate-object");
        var @object = Envelope.Create("object")
            .AddAssertion("object-predicate", "object-object");
        var envelope = Envelope.Create("subject")
            .AddAssertion(predicate, @object)
            .CheckEncoding();

        Assert.Equal(
            "\"subject\" [\n" +
            "    \"predicate\" [\n" +
            "        \"predicate-predicate\": \"predicate-object\"\n" +
            "    ]\n" +
            "    : \"object\" [\n" +
            "        \"object-predicate\": \"object-object\"\n" +
            "    ]\n" +
            "]",
            envelope.Format());
    }

    [Fact]
    public void TestAssertionOnBareAssertion()
    {
        var envelope = Envelope.CreateAssertion("predicate", "object")
            .AddAssertion("assertion-predicate", "assertion-object");

        Assert.Equal(
            "{\n" +
            "    \"predicate\": \"object\"\n" +
            "} [\n" +
            "    \"assertion-predicate\": \"assertion-object\"\n" +
            "]",
            envelope.Format());
    }
}

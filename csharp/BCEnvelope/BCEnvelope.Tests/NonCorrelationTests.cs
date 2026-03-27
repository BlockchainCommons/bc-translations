using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.BCRand;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class NonCorrelationTests
{
    [Fact]
    public void TestEnvelopeNonCorrelation()
    {
        var e1 = Envelope.Create("Hello.");

        // e1 correlates with its elision
        Assert.True(e1.IsEquivalentTo(e1.Elide()));

        // e2 is the same message, but with random salt
        var rng = SeededRandomNumberGenerator.CreateFake();
        var e2 = e1.AddSaltUsing(rng).CheckEncoding();

        Assert.Equal(
            "\"Hello.\" [\n" +
            "    'salt': Salt\n" +
            "]",
            e2.Format());

        Assert.Equal(
            "200(   / envelope /\n" +
            "    [\n" +
            "        201(\"Hello.\"),   / leaf /\n" +
            "        {\n" +
            "            15:\n" +
            "            201(   / leaf /\n" +
            "                40018(h'b559bbbf6cce2632')   / salt /\n" +
            "            )\n" +
            "        }\n" +
            "    ]\n" +
            ")",
            e2.DiagnosticAnnotated());

        Assert.Equal(
            "4f0f2d55 NODE\n" +
            "    8cc96cdb subj \"Hello.\"\n" +
            "    dd412f1d ASSERTION\n" +
            "        618975ce pred 'salt'\n" +
            "        7915f200 obj Salt",
            e2.TreeFormat());

        // So even though its content is the same, it doesn't correlate.
        Assert.False(e1.IsEquivalentTo(e2));

        // And of course, neither does its elision.
        Assert.False(e1.IsEquivalentTo(e2.Elide()));
    }

    [Fact]
    public void TestPredicateCorrelation()
    {
        var e1 = Envelope.Create("Foo")
            .AddAssertion("note", "Bar")
            .CheckEncoding();
        var e2 = Envelope.Create("Baz")
            .AddAssertion("note", "Quux")
            .CheckEncoding();

        Assert.Equal(
            "\"Foo\" [\n" +
            "    \"note\": \"Bar\"\n" +
            "]",
            e1.Format());

        // e1 and e2 have the same predicate
        Assert.True(
            e1.Assertions[0].AsPredicate()!
                .IsEquivalentTo(
                    e2.Assertions[0].AsPredicate()!));

        // Redact the entire contents of e1 without
        // redacting the envelope itself.
        var e1Elided = e1.ElideRevealingTarget(e1).CheckEncoding();

        Assert.Equal(
            "ELIDED [\n" +
            "    ELIDED\n" +
            "]",
            e1Elided.Format());
    }

    [Fact]
    public void TestAddSalt()
    {
        var source = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        var e1 = Envelope.Create("Alpha")
            .AddSalt()
            .CheckEncoding()
            .Wrap()
            .CheckEncoding()
            .AddAssertion(
                Envelope.Create(KnownValuesRegistry.Note)
                    .AddSalt()
                    .CheckEncoding(),
                Envelope.Create(source).AddSalt().CheckEncoding())
            .CheckEncoding();

        Assert.Equal(
            "{\n" +
            "    \"Alpha\" [\n" +
            "        'salt': Salt\n" +
            "    ]\n" +
            "} [\n" +
            "    'note' [\n" +
            "        'salt': Salt\n" +
            "    ]\n" +
            "    : \"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\" [\n" +
            "        'salt': Salt\n" +
            "    ]\n" +
            "]",
            e1.Format());

        var e1Elided = e1.ElideRevealingTarget(e1).CheckEncoding();

        Assert.Equal(
            "ELIDED [\n" +
            "    ELIDED\n" +
            "]",
            e1Elided.Format());
    }
}

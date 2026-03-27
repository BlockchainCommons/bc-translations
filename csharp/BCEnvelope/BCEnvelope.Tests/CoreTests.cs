using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.BCUR;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;
using Xunit;

namespace BlockchainCommons.BCEnvelope.Tests;

/// <summary>
/// Tests for core Gordian Envelope construction and content extraction.
/// </summary>
public class CoreTests
{
    [Fact]
    public void TestReadLegacyLeaf()
    {
        // d8c8d818182a is the legacy encoding using tag #6.24 for leaf
        var legacyEnvelope = Envelope.FromTaggedCbor(
            Cbor.TryFromData(Convert.FromHexString("d8c8d818182a")));
        var e = Envelope.Create(42);
        Assert.True(legacyEnvelope.IsIdenticalTo(e));
        Assert.True(legacyEnvelope.IsEquivalentTo(e));
    }

    [Fact]
    public void TestIntSubject()
    {
        var e = Envelope.Create(42).CheckEncoding();

        Assert.Equal(
            "200(   / envelope /\n" +
            "    201(42)   / leaf /\n" +
            ")",
            e.DiagnosticAnnotated());

        Assert.Equal(
            "Digest(7f83f7bda2d63959d34767689f06d47576683d378d9eb8d09386c9a020395c53)",
            e.GetDigest().ToString());

        Assert.Equal("42", e.Format());

        Assert.Equal(42, e.ExtractSubject<int>());
    }

    [Fact]
    public void TestNegativeIntSubject()
    {
        var e = Envelope.Create(-42).CheckEncoding();

        Assert.Equal(
            "200(   / envelope /\n" +
            "    201(-42)   / leaf /\n" +
            ")",
            e.DiagnosticAnnotated());

        Assert.Equal(
            "Digest(9e0ad272780de7aa1dbdfbc99058bb81152f623d3b95b5dfb0a036badfcc9055)",
            e.GetDigest().ToString());

        Assert.Equal("-42", e.Format());

        Assert.Equal(-42, e.ExtractSubject<int>());
    }

    [Fact]
    public void TestCborEncodableSubject()
    {
        var e = TestData.HelloEnvelope().CheckEncoding();

        Assert.Equal(
            "200(   / envelope /\n" +
            "    201(\"Hello.\")   / leaf /\n" +
            ")",
            e.DiagnosticAnnotated());

        Assert.Equal(
            "Digest(8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59)",
            e.GetDigest().ToString());

        Assert.Equal("\"Hello.\"", e.Format());

        Assert.Equal(TestData.PlaintextHello, e.ExtractSubject<string>());
    }

    [Fact]
    public void TestKnownValueSubject()
    {
        var e = TestData.KnownValueEnvelope().CheckEncoding();

        Assert.Equal(
            "200(4)   / envelope /",
            e.DiagnosticAnnotated());

        Assert.Equal(
            "Digest(0fcd6a39d6ed37f2e2efa6a96214596f1b28a5cd42a5a27afc32162aaf821191)",
            e.GetDigest().ToString());

        Assert.Equal("'note'", e.Format());

        Assert.Equal(KnownValuesRegistry.Note, e.ExtractSubject<KnownValue>());
    }

    [Fact]
    public void TestAssertionSubject()
    {
        var e = TestData.AssertionEnvelope().CheckEncoding();

        Assert.Equal(
            "Digest(db7dd21c5169b4848d2a1bcb0a651c9617cdd90bae29156baaefbb2a8abef5ba)",
            e.AsPredicate()!.GetDigest().ToString());
        Assert.Equal(
            "Digest(13b741949c37b8e09cc3daa3194c58e4fd6b2f14d4b1d0f035a46d6d5a1d3f11)",
            e.AsObject()!.GetDigest().ToString());
        Assert.Equal(
            "Digest(78d666eb8f4c0977a0425ab6aa21ea16934a6bc97c6f0c3abaefac951c1714a2)",
            e.Subject.GetDigest().ToString());
        Assert.Equal(
            "Digest(78d666eb8f4c0977a0425ab6aa21ea16934a6bc97c6f0c3abaefac951c1714a2)",
            e.GetDigest().ToString());

        Assert.Equal(
            "200(   / envelope /\n" +
            "    {\n" +
            "        201(\"knows\"):   / leaf /\n" +
            "        201(\"Bob\")   / leaf /\n" +
            "    }\n" +
            ")",
            e.DiagnosticAnnotated());

        Assert.Equal("\"knows\": \"Bob\"", e.Format());

        Assert.Equal(e.GetDigest(), Envelope.CreateAssertion("knows", "Bob").GetDigest());
    }

    [Fact]
    public void TestSubjectWithAssertion()
    {
        var e = TestData.SingleAssertionEnvelope().CheckEncoding();

        Assert.Equal(
            "200(   / envelope /\n" +
            "    [\n" +
            "        201(\"Alice\"),   / leaf /\n" +
            "        {\n" +
            "            201(\"knows\"):   / leaf /\n" +
            "            201(\"Bob\")   / leaf /\n" +
            "        }\n" +
            "    ]\n" +
            ")",
            e.DiagnosticAnnotated());

        Assert.Equal(
            "Digest(8955db5e016affb133df56c11fe6c5c82fa3036263d651286d134c7e56c0e9f2)",
            e.GetDigest().ToString());

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "]",
            e.Format());

        Assert.Equal("Alice", e.ExtractSubject<string>());
    }

    [Fact]
    public void TestSubjectWithTwoAssertions()
    {
        var e = TestData.DoubleAssertionEnvelope().CheckEncoding();

        Assert.Equal(
            "200(   / envelope /\n" +
            "    [\n" +
            "        201(\"Alice\"),   / leaf /\n" +
            "        {\n" +
            "            201(\"knows\"):   / leaf /\n" +
            "            201(\"Carol\")   / leaf /\n" +
            "        },\n" +
            "        {\n" +
            "            201(\"knows\"):   / leaf /\n" +
            "            201(\"Bob\")   / leaf /\n" +
            "        }\n" +
            "    ]\n" +
            ")",
            e.DiagnosticAnnotated());

        Assert.Equal(
            "Digest(b8d857f6e06a836fbc68ca0ce43e55ceb98eefd949119dab344e11c4ba5a0471)",
            e.GetDigest().ToString());

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    \"knows\": \"Carol\"\n" +
            "]",
            e.Format());

        Assert.Equal("Alice", e.ExtractSubject<string>());
    }

    [Fact]
    public void TestWrapped()
    {
        var e = TestData.WrappedEnvelope().CheckEncoding();

        Assert.Equal(
            "200(   / envelope /\n" +
            "    200(   / envelope /\n" +
            "        201(\"Hello.\")   / leaf /\n" +
            "    )\n" +
            ")",
            e.DiagnosticAnnotated());

        Assert.Equal(
            "Digest(172a5e51431062e7b13525cbceb8ad8475977444cf28423e21c0d1dcbdfcaf47)",
            e.GetDigest().ToString());

        Assert.Equal(
            "{\n" +
            "    \"Hello.\"\n" +
            "}",
            e.Format());
    }

    [Fact]
    public void TestDoubleWrapped()
    {
        var e = TestData.DoubleWrappedEnvelope().CheckEncoding();

        Assert.Equal(
            "200(   / envelope /\n" +
            "    200(   / envelope /\n" +
            "        200(   / envelope /\n" +
            "            201(\"Hello.\")   / leaf /\n" +
            "        )\n" +
            "    )\n" +
            ")",
            e.DiagnosticAnnotated());

        Assert.Equal(
            "Digest(8b14f3bcd7c05aac8f2162e7047d7ef5d5eab7d82ee3f9dc4846c70bae4d200b)",
            e.GetDigest().ToString());

        Assert.Equal(
            "{\n" +
            "    {\n" +
            "        \"Hello.\"\n" +
            "    }\n" +
            "}",
            e.Format());
    }

    [Fact]
    public void TestAssertionWithAssertions()
    {
        var a = Envelope.CreateAssertion(1, 2)
            .AddAssertion(3, 4)
            .AddAssertion(5, 6);
        var e = Envelope.Create(7).AddAssertionEnvelope(a);

        Assert.Equal(
            "7 [\n" +
            "    {\n" +
            "        1: 2\n" +
            "    } [\n" +
            "        3: 4\n" +
            "        5: 6\n" +
            "    ]\n" +
            "]",
            e.Format());
    }

    [Fact]
    public void TestDigestLeaf()
    {
        var digest = TestData.HelloEnvelope().GetDigest();
        var e = Envelope.Create(digest).CheckEncoding();

        Assert.Equal("Digest(8cc96cdb)", e.Format());

        Assert.Equal(
            "Digest(07b518af92a6196bc153752aabefedb34ff8e1a7d820c01ef978dfc3e7e52e05)",
            e.GetDigest().ToString());

        Assert.Equal(
            "200(   / envelope /\n" +
            "    201(   / leaf /\n" +
            "        40001(   / digest /\n" +
            "            h'8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59'\n" +
            "        )\n" +
            "    )\n" +
            ")",
            e.DiagnosticAnnotated());
    }

    [Fact]
    public void TestUnknownLeaf()
    {
        GlobalFormatContext.RegisterTags();

        var unknownUr = "ur:envelope/tpsotaaodnoyadgdjlssmkcklgoskseodnyteofwwfylkiftaydpdsjz";
        var ur = UR.FromUrString(unknownUr);
        var e = Envelope.FromUntaggedCbor(ur.Cbor);
        var expected = "555({1: h'6fc4981e8da778332bf93342f3f77d3a'})";
        Assert.Equal(expected, e.Format());
    }

    [Fact]
    public void TestTrue()
    {
        GlobalFormatContext.RegisterTags();
        var e = Envelope.Create(true).CheckEncoding();
        Assert.True(e.IsBool);
        Assert.True(e.IsTrue);
        Assert.False(e.IsFalse);
        Assert.Equal(Envelope.True(), e);
        Assert.Equal("true", e.Format());
    }

    [Fact]
    public void TestFalse()
    {
        GlobalFormatContext.RegisterTags();
        var e = Envelope.Create(false).CheckEncoding();
        Assert.True(e.IsBool);
        Assert.False(e.IsTrue);
        Assert.True(e.IsFalse);
        Assert.Equal(Envelope.False(), e);
        Assert.Equal("false", e.Format());
    }

    [Fact]
    public void TestUnit()
    {
        GlobalFormatContext.RegisterTags();
        var e = Envelope.Unit().CheckEncoding();
        Assert.True(e.IsSubjectUnit);
        Assert.Equal("''", e.Format());

        e = e.AddAssertion("foo", "bar");
        Assert.True(e.IsSubjectUnit);
        Assert.Equal(
            "'' [\n" +
            "    \"foo\": \"bar\"\n" +
            "]",
            e.Format());

        var subject = e.ExtractSubject<KnownValue>();
        Assert.Equal(KnownValuesRegistry.Unit, subject);
    }

    [Fact]
    public void TestPosition()
    {
        GlobalFormatContext.RegisterTags();

        var e = Envelope.Create("Hello");
        Assert.Throws<EnvelopeException>(() => e.GetPosition());

        e = e.SetPosition(42);
        Assert.Equal(42, e.GetPosition());
        Assert.Equal(
            "\"Hello\" [\n" +
            "    'position': 42\n" +
            "]",
            e.Format());

        e = e.SetPosition(0);
        Assert.Equal(0, e.GetPosition());
        Assert.Equal(
            "\"Hello\" [\n" +
            "    'position': 0\n" +
            "]",
            e.Format());

        e = e.RemovePosition();
        Assert.Throws<EnvelopeException>(() => e.GetPosition());
        Assert.Equal("\"Hello\"", e.Format());
    }
}

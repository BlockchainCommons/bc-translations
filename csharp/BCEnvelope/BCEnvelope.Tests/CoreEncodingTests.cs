using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.DCbor;
using Xunit;

namespace BlockchainCommons.BCEnvelope.Tests;

/// <summary>
/// Tests for CBOR encoding round-trip verification of core envelope types.
/// </summary>
public class CoreEncodingTests
{
    [Fact]
    public void TestDigest()
    {
        Envelope.Create(
            Digest.FromImage(System.Text.Encoding.UTF8.GetBytes("Hello."))
        ).CheckEncoding();
    }

    [Fact]
    public void Test1()
    {
        var e = Envelope.Create("Hello.");

        Assert.Equal(
            "200(   / envelope /\n" +
            "    201(\"Hello.\")   / leaf /\n" +
            ")",
            e.DiagnosticAnnotated());
    }

    [Fact]
    public void Test2()
    {
        var array = new Cbor(CborCase.Array(new List<Cbor>
        {
            Cbor.FromUInt(1),
            Cbor.FromUInt(2),
            Cbor.FromUInt(3),
        }));
        var e = Envelope.Create(array);

        Assert.Equal(
            "200(   / envelope /\n" +
            "    201(   / leaf /\n" +
            "        [1, 2, 3]\n" +
            "    )\n" +
            ")",
            e.DiagnosticAnnotated());
    }

    [Fact]
    public void Test3()
    {
        var e1 = Envelope.CreateAssertion("A", "B").CheckEncoding();
        var e2 = Envelope.CreateAssertion("C", "D").CheckEncoding();
        var e3 = Envelope.CreateAssertion("E", "F").CheckEncoding();

        var e4 = e2.AddAssertionEnvelope(e3);
        Assert.Equal(
            "{\n" +
            "    \"C\": \"D\"\n" +
            "} [\n" +
            "    \"E\": \"F\"\n" +
            "]",
            e4.Format());

        Assert.Equal(
            "200(   / envelope /\n" +
            "    [\n" +
            "        {\n" +
            "            201(\"C\"):   / leaf /\n" +
            "            201(\"D\")   / leaf /\n" +
            "        },\n" +
            "        {\n" +
            "            201(\"E\"):   / leaf /\n" +
            "            201(\"F\")   / leaf /\n" +
            "        }\n" +
            "    ]\n" +
            ")",
            e4.DiagnosticAnnotated());

        e4.CheckEncoding();

        var e5 = e1.AddAssertionEnvelope(e4).CheckEncoding();

        Assert.Equal(
            "{\n" +
            "    \"A\": \"B\"\n" +
            "} [\n" +
            "    {\n" +
            "        \"C\": \"D\"\n" +
            "    } [\n" +
            "        \"E\": \"F\"\n" +
            "    ]\n" +
            "]",
            e5.Format());

        Assert.Equal(
            "200(   / envelope /\n" +
            "    [\n" +
            "        {\n" +
            "            201(\"A\"):   / leaf /\n" +
            "            201(\"B\")   / leaf /\n" +
            "        },\n" +
            "        [\n" +
            "            {\n" +
            "                201(\"C\"):   / leaf /\n" +
            "                201(\"D\")   / leaf /\n" +
            "            },\n" +
            "            {\n" +
            "                201(\"E\"):   / leaf /\n" +
            "                201(\"F\")   / leaf /\n" +
            "            }\n" +
            "        ]\n" +
            "    ]\n" +
            ")",
            e5.DiagnosticAnnotated());
    }
}

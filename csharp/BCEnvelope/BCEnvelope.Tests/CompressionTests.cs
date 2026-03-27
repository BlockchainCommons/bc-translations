using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.BCRand;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class CompressionTests
{
    private const string Source = "Lorem ipsum dolor sit amet consectetur adipiscing elit mi nibh ornare proin blandit diam ridiculus, faucibus mus dui eu vehicula nam donec dictumst sed vivamus bibendum aliquet efficitur. Felis imperdiet sodales dictum morbi vivamus augue dis duis aliquet velit ullamcorper porttitor, lobortis dapibus hac purus aliquam natoque iaculis blandit montes nunc pretium.";

    [Fact]
    public void TestCompress()
    {
        var original = Envelope.Create(Source);
        Assert.Equal(371, original.TaggedCbor().ToCborData().Length);
        var compressed = original.Compress().CheckEncoding();
        Assert.Equal(283, compressed.TaggedCbor().ToCborData().Length);

        Assert.Equal(original.GetDigest(), compressed.GetDigest());
        var decompressed = compressed.Decompress().CheckEncoding();
        Assert.Equal(decompressed.GetDigest(), original.GetDigest());
        Assert.Equal(decompressed.StructuralDigest(), original.StructuralDigest());
    }

    [Fact]
    public void TestCompressSubject()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();
        var options = new SigningOptions.SchnorrOptions(rng);

        var alicePrivateKey = PrivateKeyBase.FromData(
            Convert.FromHexString("82f32c855d3d542256180810797e0073"));

        var original = Envelope.Create("Alice")
            .AddAssertion(KnownValuesRegistry.Note, Source)
            .Wrap()
            .AddSignatureOpt(alicePrivateKey, options, null);
        Assert.Equal(458, original.TaggedCbor().ToCborData().Length);

        var s = original.TreeFormat();
        Assert.Equal(
            "ec608f27 NODE\n" +
            "    d7183f04 subj WRAPPED\n" +
            "        7f35e345 cont NODE\n" +
            "            13941b48 subj \"Alice\"\n" +
            "            9fb69539 ASSERTION\n" +
            "                0fcd6a39 pred 'note'\n" +
            "                e343c9b4 obj \"Lorem ipsum dolor sit amet consectetur a\u2026\"\n" +
            "    0db2ee20 ASSERTION\n" +
            "        d0e39e78 pred 'signed'\n" +
            "        f0d3ce4c obj Signature",
            s);

        var compressedEnvelope = original
            .CompressSubject()
            .CheckEncoding();
        Assert.Equal(374, compressedEnvelope.TaggedCbor().ToCborData().Length);

        var s2 = compressedEnvelope.TreeFormat();
        Assert.Equal(
            "ec608f27 NODE\n" +
            "    d7183f04 subj COMPRESSED\n" +
            "    0db2ee20 ASSERTION\n" +
            "        d0e39e78 pred 'signed'\n" +
            "        f0d3ce4c obj Signature",
            s2);

        var s3 = compressedEnvelope.MermaidFormat();
        Assert.Equal(
            "%%{ init: { 'theme': 'default', 'flowchart': { 'curve': 'basis' } } }%%\n" +
            "graph LR\n" +
            "0((\"NODE<br>ec608f27\"))\n" +
            "    0 -- subj --> 1[[\"COMPRESSED<br>d7183f04\"]]\n" +
            "    0 --> 2([\"ASSERTION<br>0db2ee20\"])\n" +
            "        2 -- pred --> 3[/\"'signed'<br>d0e39e78\"/]\n" +
            "        2 -- obj --> 4[\"Signature<br>f0d3ce4c\"]\n" +
            "style 0 stroke:red,stroke-width:4px\n" +
            "style 1 stroke:purple,stroke-width:4px\n" +
            "style 2 stroke:green,stroke-width:4px\n" +
            "style 3 stroke:goldenrod,stroke-width:4px\n" +
            "style 4 stroke:teal,stroke-width:4px\n" +
            "linkStyle 0 stroke:red,stroke-width:2px\n" +
            "linkStyle 1 stroke-width:2px\n" +
            "linkStyle 2 stroke:cyan,stroke-width:2px\n" +
            "linkStyle 3 stroke:magenta,stroke-width:2px",
            s3);

        var decompressed = compressedEnvelope
            .DecompressSubject()
            .CheckEncoding();
        Assert.Equal(decompressed.GetDigest(), original.GetDigest());
        Assert.Equal(decompressed.StructuralDigest(), original.StructuralDigest());
    }
}

using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.BCRand;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;
using Xunit;

namespace BlockchainCommons.BCEnvelope.Tests;

/// <summary>
/// Tests for envelope notation and tree format rendering.
/// </summary>
public class FormatTests
{
    [Fact]
    public void TestPlaintext()
    {
        var envelope = Envelope.Create(TestData.PlaintextHello);

        Assert.Equal("\"Hello.\"", envelope.Format());

        Assert.Equal("\"Hello.\"", envelope.FormatFlat());

        Assert.Equal(
            "8cc96cdb \"Hello.\"",
            envelope.TreeFormat());

        Assert.Equal(
            "8cc96cdb \"Hello.\"",
            envelope.TreeFormatOpt(new TreeFormatOpts(context: FormatContextOpt.None)));

        Assert.Equal(
            "8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59 \"Hello.\"",
            envelope.TreeFormatOpt(new TreeFormatOpts(digestDisplay: DigestDisplayFormat.Full)));

        Assert.Equal(
            "ur:digest/hdcxlksojzuyktbykovsecbygebsldeninbdfptkwebtwzdpadglwetbgltnwdmwhlhksbbthtpy \"Hello.\"",
            envelope.TreeFormatOpt(new TreeFormatOpts(digestDisplay: DigestDisplayFormat.UR)));

        Assert.Equal(
            "\"Hello.\"",
            envelope.TreeFormatOpt(new TreeFormatOpts(hideNodes: true)));

        Assert.Equal(
            envelope.ElementsCount,
            envelope.TreeFormat().Split('\n').Length);
    }

    [Fact]
    public void TestSignedPlaintext()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();
        var options = new SigningOptions.SchnorrOptions(rng);
        var envelope = Envelope.Create(TestData.PlaintextHello)
            .AddSignatureOpt(TestData.AlicePrivateKey(), options, null);

        Assert.Equal(
            "\"Hello.\" [\n" +
            "    'signed': Signature\n" +
            "]",
            envelope.Format());

        Assert.Equal(
            "\"Hello.\" [ 'signed': Signature ]",
            envelope.FormatFlat());

        Assert.Equal(
            "949a991e NODE\n" +
            "    8cc96cdb subj \"Hello.\"\n" +
            "    fcb4e2be ASSERTION\n" +
            "        d0e39e78 pred 'signed'\n" +
            "        b8bb043f obj Signature",
            envelope.TreeFormat());

        Assert.Equal(
            "949a991e NODE\n" +
            "    8cc96cdb subj \"Hello.\"\n" +
            "    fcb4e2be ASSERTION\n" +
            "        d0e39e78 pred '3'\n" +
            "        b8bb043f obj 40020(h'd0f6b2577edb3f4b0f533e21577bc12a58aaca2604bc71e84bd4e2c81421900bca361a1a8de3b7dbfe1cb5c16e34cb8c9a78fe6f7a387e959bbb15f6f3d898d3')",
            envelope.TreeFormatOpt(new TreeFormatOpts(context: FormatContextOpt.None)));

        Assert.Equal(
            "subj \"Hello.\"\n" +
            "    ASSERTION\n" +
            "        pred 'signed'\n" +
            "        obj Signature",
            envelope.TreeFormatOpt(new TreeFormatOpts(hideNodes: true)));

        Assert.Equal(
            "subj \"Hello.\"\n" +
            "    ASSERTION\n" +
            "        pred '3'\n" +
            "        obj 40020(h'd0f6b2577edb3f4b0f533e21577bc12a58aaca2604bc71e84bd4e2c81421900bca361a1a8de3b7dbfe1cb5c16e34cb8c9a78fe6f7a387e959bbb15f6f3d898d3')",
            envelope.TreeFormatOpt(new TreeFormatOpts(
                hideNodes: true,
                context: FormatContextOpt.None)));

        Assert.Equal(
            envelope.ElementsCount,
            envelope.TreeFormat().Split('\n').Length);
    }

    [Fact]
    public void TestEncryptSubject()
    {
        var envelope = Envelope.Create("Alice")
            .AddAssertion("knows", "Bob")
            .EncryptSubject(SymmetricKey.New());

        Assert.Equal(
            "ENCRYPTED [\n" +
            "    \"knows\": \"Bob\"\n" +
            "]",
            envelope.Format());

        Assert.Equal(
            "ENCRYPTED [ \"knows\": \"Bob\" ]",
            envelope.FormatFlat());

        Assert.Equal(
            "8955db5e NODE\n" +
            "    13941b48 subj ENCRYPTED\n" +
            "    78d666eb ASSERTION\n" +
            "        db7dd21c pred \"knows\"\n" +
            "        13b74194 obj \"Bob\"",
            envelope.TreeFormat());

        var mermaid = envelope.MermaidFormat();
        Assert.Equal(
            "%%{ init: { 'theme': 'default', 'flowchart': { 'curve': 'basis' } } }%%\n" +
            "graph LR\n" +
            "0((\"NODE<br>8955db5e\"))\n" +
            "    0 -- subj --> 1>\"ENCRYPTED<br>13941b48\"]\n" +
            "    0 --> 2([\"ASSERTION<br>78d666eb\"])\n" +
            "        2 -- pred --> 3[\"&quot;knows&quot;<br>db7dd21c\"]\n" +
            "        2 -- obj --> 4[\"&quot;Bob&quot;<br>13b74194\"]\n" +
            "style 0 stroke:red,stroke-width:4px\n" +
            "style 1 stroke:coral,stroke-width:4px\n" +
            "style 2 stroke:green,stroke-width:4px\n" +
            "style 3 stroke:teal,stroke-width:4px\n" +
            "style 4 stroke:teal,stroke-width:4px\n" +
            "linkStyle 0 stroke:red,stroke-width:2px\n" +
            "linkStyle 1 stroke-width:2px\n" +
            "linkStyle 2 stroke:cyan,stroke-width:2px\n" +
            "linkStyle 3 stroke:magenta,stroke-width:2px",
            mermaid);

        Assert.Equal(
            "subj ENCRYPTED\n" +
            "    ASSERTION\n" +
            "        pred \"knows\"\n" +
            "        obj \"Bob\"",
            envelope.TreeFormatOpt(new TreeFormatOpts(hideNodes: true)));

        Assert.Equal(
            envelope.ElementsCount,
            envelope.TreeFormat().Split('\n').Length);
    }

    [Fact]
    public void TestTopLevelAssertion()
    {
        var envelope = Envelope.CreateAssertion("knows", "Bob");

        Assert.Equal("\"knows\": \"Bob\"", envelope.Format());

        Assert.Equal("\"knows\": \"Bob\"", envelope.FormatFlat());

        Assert.Equal(
            "78d666eb ASSERTION\n" +
            "    db7dd21c pred \"knows\"\n" +
            "    13b74194 obj \"Bob\"",
            envelope.TreeFormat());

        Assert.Equal(
            "ASSERTION\n" +
            "    pred \"knows\"\n" +
            "    obj \"Bob\"",
            envelope.TreeFormatOpt(new TreeFormatOpts(hideNodes: true)));

        Assert.Equal(
            envelope.ElementsCount,
            envelope.TreeFormat().Split('\n').Length);
    }

    [Fact]
    public void TestElidedObject()
    {
        var envelope = Envelope.Create("Alice").AddAssertion("knows", "Bob");
        var elided = envelope.ElideRemovingTarget(Envelope.Create("Bob"));

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": ELIDED\n" +
            "]",
            elided.Format());

        Assert.Equal(
            "\"Alice\" [ \"knows\": ELIDED ]",
            elided.FormatFlat());

        Assert.Equal(
            "8955db5e NODE\n" +
            "    13941b48 subj \"Alice\"\n" +
            "    78d666eb ASSERTION\n" +
            "        db7dd21c pred \"knows\"\n" +
            "        13b74194 obj ELIDED",
            elided.TreeFormat());

        Assert.Equal(
            "subj \"Alice\"\n" +
            "    ASSERTION\n" +
            "        pred \"knows\"\n" +
            "        obj ELIDED",
            elided.TreeFormatOpt(new TreeFormatOpts(hideNodes: true)));

        Assert.Equal(
            elided.ElementsCount,
            elided.TreeFormat().Split('\n').Length);
    }

    [Fact]
    public void TestSignedSubject()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();
        var options = new SigningOptions.SchnorrOptions(rng);
        var envelope = Envelope.Create("Alice")
            .AddAssertion("knows", "Bob")
            .AddAssertion("knows", "Carol")
            .AddSignatureOpt(TestData.AlicePrivateKey(), options, null);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    \"knows\": \"Carol\"\n" +
            "    'signed': Signature\n" +
            "]",
            envelope.Format());

        Assert.Equal(
            "\"Alice\" [ \"knows\": \"Bob\", \"knows\": \"Carol\", 'signed': Signature ]",
            envelope.FormatFlat());

        Assert.Equal(
            "d595106e NODE\n" +
            "    13941b48 subj \"Alice\"\n" +
            "    399c974c ASSERTION\n" +
            "        d0e39e78 pred 'signed'\n" +
            "        ff10427c obj Signature\n" +
            "    4012caf2 ASSERTION\n" +
            "        db7dd21c pred \"knows\"\n" +
            "        afb8122e obj \"Carol\"\n" +
            "    78d666eb ASSERTION\n" +
            "        db7dd21c pred \"knows\"\n" +
            "        13b74194 obj \"Bob\"",
            envelope.TreeFormat());

        Assert.Equal(
            "subj \"Alice\"\n" +
            "    ASSERTION\n" +
            "        pred 'signed'\n" +
            "        obj Signature\n" +
            "    ASSERTION\n" +
            "        pred \"knows\"\n" +
            "        obj \"Carol\"\n" +
            "    ASSERTION\n" +
            "        pred \"knows\"\n" +
            "        obj \"Bob\"",
            envelope.TreeFormatOpt(new TreeFormatOpts(hideNodes: true)));

        Assert.Equal(
            envelope.ElementsCount,
            envelope.TreeFormat().Split('\n').Length);

        // Elided assertions
        var target = new HashSet<Digest>();
        target.Add(envelope.GetDigest());
        target.Add(envelope.Subject.GetDigest());
        var elided = envelope.ElideRevealingSet(target);

        Assert.Equal(
            "\"Alice\" [\n" +
            "    ELIDED (3)\n" +
            "]",
            elided.Format());

        Assert.Equal(
            "\"Alice\" [ ELIDED (3) ]",
            elided.FormatFlat());

        Assert.Equal(
            "d595106e NODE\n" +
            "    13941b48 subj \"Alice\"\n" +
            "    399c974c ELIDED\n" +
            "    4012caf2 ELIDED\n" +
            "    78d666eb ELIDED",
            elided.TreeFormat());

        Assert.Equal(
            "subj \"Alice\"\n" +
            "    ELIDED\n" +
            "    ELIDED\n" +
            "    ELIDED",
            elided.TreeFormatOpt(new TreeFormatOpts(hideNodes: true)));

        Assert.Equal(
            elided.ElementsCount,
            elided.TreeFormat().Split('\n').Length);
    }

    [Fact]
    public void TestWrapThenSigned()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();
        var options = new SigningOptions.SchnorrOptions(rng);
        var envelope = Envelope.Create("Alice")
            .AddAssertion("knows", "Bob")
            .AddAssertion("knows", "Carol")
            .Wrap()
            .AddSignatureOpt(TestData.AlicePrivateKey(), options, null);

        Assert.Equal(
            "{\n" +
            "    \"Alice\" [\n" +
            "        \"knows\": \"Bob\"\n" +
            "        \"knows\": \"Carol\"\n" +
            "    ]\n" +
            "} [\n" +
            "    'signed': Signature\n" +
            "]",
            envelope.Format());

        Assert.Equal(
            "{ \"Alice\" [ \"knows\": \"Bob\", \"knows\": \"Carol\" ] } [ 'signed': Signature ]",
            envelope.FormatFlat());

        Assert.Equal(
            "66c9d594 NODE\n" +
            "    9e3b0673 subj WRAPPED\n" +
            "        b8d857f6 cont NODE\n" +
            "            13941b48 subj \"Alice\"\n" +
            "            4012caf2 ASSERTION\n" +
            "                db7dd21c pred \"knows\"\n" +
            "                afb8122e obj \"Carol\"\n" +
            "            78d666eb ASSERTION\n" +
            "                db7dd21c pred \"knows\"\n" +
            "                13b74194 obj \"Bob\"\n" +
            "    f13623da ASSERTION\n" +
            "        d0e39e78 pred 'signed'\n" +
            "        e30a727c obj Signature",
            envelope.TreeFormat());

        Assert.Equal(
            "subj WRAPPED\n" +
            "    subj \"Alice\"\n" +
            "        ASSERTION\n" +
            "            pred \"knows\"\n" +
            "            obj \"Carol\"\n" +
            "        ASSERTION\n" +
            "            pred \"knows\"\n" +
            "            obj \"Bob\"\n" +
            "    ASSERTION\n" +
            "        pred 'signed'\n" +
            "        obj Signature",
            envelope.TreeFormatOpt(new TreeFormatOpts(hideNodes: true)));

        Assert.Equal(
            "66c9d5944eaedc418d8acf52df7842f50c2aa2d0da2857ad1048412cd070c3e8 NODE\n" +
            "    9e3b06737407b10cac0b9353dd978c4a68537709554dabdd66a8b68b8bd36cf6 subj WRAPPED\n" +
            "        b8d857f6e06a836fbc68ca0ce43e55ceb98eefd949119dab344e11c4ba5a0471 cont NODE\n" +
            "            13941b487c1ddebce827b6ec3f46d982938acdc7e3b6a140db36062d9519dd2f subj \"Alice\"\n" +
            "            4012caf2d96bf3962514bcfdcf8dd70c351735dec72c856ec5cdcf2ee35d6a91 ASSERTION\n" +
            "                db7dd21c5169b4848d2a1bcb0a651c9617cdd90bae29156baaefbb2a8abef5ba pred \"knows\"\n" +
            "                afb8122e3227657b415f9f1c930d4891fb040b3e23c1f7770f185e2d0396c737 obj \"Carol\"\n" +
            "            78d666eb8f4c0977a0425ab6aa21ea16934a6bc97c6f0c3abaefac951c1714a2 ASSERTION\n" +
            "                db7dd21c5169b4848d2a1bcb0a651c9617cdd90bae29156baaefbb2a8abef5ba pred \"knows\"\n" +
            "                13b741949c37b8e09cc3daa3194c58e4fd6b2f14d4b1d0f035a46d6d5a1d3f11 obj \"Bob\"\n" +
            "    f13623dac926c57e2ac128868dfaa22fb8e434a7e4a552029992d6f6283da533 ASSERTION\n" +
            "        d0e39e788c0d8f0343af4588db21d3d51381db454bdf710a9a1891aaa537693c pred 'signed'\n" +
            "        e30a727cc1f43fbe3c9fd228447c34faaf6b54101bf7bcd766e280f8449ceade obj Signature",
            envelope.TreeFormatOpt(new TreeFormatOpts(digestDisplay: DigestDisplayFormat.Full)));

        Assert.Equal(
            "ur:digest/hdcxiysotlmwglpluofplgletkgmurksfwykbndroetitndehgpmbefdfpdwtijosrvsbsdlsndm NODE\n" +
            "    ur:digest/hdcxnnframjkjyatpabnpsbdmuguutmslkgeisguktasgogtpyutiypdrplulutejzynmygrnlly subj WRAPPED\n" +
            "        ur:digest/hdcxrotphgynvtimlsjlrfissgbnvefmgotorhmnwstagabyntpyeeglbyssrdhtaajsaetafrbw cont NODE\n" +
            "            ur:digest/hdcxbwmwcwfdkecauerfvsdirpwpfhfgtalfmulesnstvlrpoyfzuyenamdpmdcfutdlstyaqzrk subj \"Alice\"\n" +
            "            ur:digest/hdcxfzbgsgwztajewfmtdabbrfzctklgtsbnecchecuestdwlpjtsksntkdmvlhlimmetlcpiyms ASSERTION\n" +
            "                ur:digest/hdcxuykitdcegyinqzlrlgdrcwsbbkihcemtchsntabdpldtbzjepkwsrkdrlernykrddpjtgdfh pred \"knows\"\n" +
            "                ur:digest/hdcxperobgdmeydiihkgfphenecemubtfdmezoaabdfmcnseylktbscshydpaxmtstemtarhmngd obj \"Carol\"\n" +
            "            ur:digest/hdcxkstbiywmmygsasktnbfwhtrppkclwdcmmugejesokejlbnftrdwspsmdcechbboerhzebtws ASSERTION\n" +
            "                ur:digest/hdcxuykitdcegyinqzlrlgdrcwsbbkihcemtchsntabdpldtbzjepkwsrkdrlernykrddpjtgdfh pred \"knows\"\n" +
            "                ur:digest/hdcxbwrlfpmwnsemrovtnssrtnotcfgshdvezcjedlbbtypatiwtecoxjnjnhtcafhbysptsnsnl obj \"Bob\"\n" +
            "    ur:digest/hdcxwnencntnsodsskkbdrsedelnlgzsoedlroveeeosveongmaonlmotbyndefsoneorfutayas ASSERTION\n" +
            "        ur:digest/hdcxtivlnnkslkbtmyaxfxpefelouycltetlbwlyuyfegrurjsbknycsmepkoneminfnrpjpssla pred 'signed'\n" +
            "        ur:digest/hdcxvlbkjpkesewkfhrnfnnetddefykeeezspejeghbecwylrftsiyvolayafynswduefytsgaos obj Signature",
            envelope.TreeFormatOpt(new TreeFormatOpts(digestDisplay: DigestDisplayFormat.UR)));

        Assert.Equal(
            envelope.ElementsCount,
            envelope.TreeFormat().Split('\n').Length);
    }

    [Fact]
    public void TestEncryptToRecipients()
    {
        var bobPublicKeys = ((IPublicKeysProvider)TestData.BobPrivateKey()).PublicKeys();
        var carolPublicKeys = ((IPublicKeysProvider)TestData.CarolPrivateKey()).PublicKeys();

        var envelope = Envelope.Create(TestData.PlaintextHello)
            .EncryptSubject(TestData.FakeContentKey(), TestData.FakeNonce())
            .CheckEncoding()
            .AddRecipientOpt(bobPublicKeys, TestData.FakeContentKey(), TestData.FakeNonce())
            .CheckEncoding()
            .AddRecipientOpt(carolPublicKeys, TestData.FakeContentKey(), TestData.FakeNonce())
            .CheckEncoding();

        Assert.Equal(
            "ENCRYPTED [\n" +
            "    'hasRecipient': SealedMessage\n" +
            "    'hasRecipient': SealedMessage\n" +
            "]",
            envelope.Format());

        Assert.Equal(
            "ENCRYPTED [ 'hasRecipient': SealedMessage, 'hasRecipient': SealedMessage ]",
            envelope.FormatFlat());

        Assert.Equal(
            "subj ENCRYPTED\n" +
            "    ASSERTION\n" +
            "        pred 'hasRecipient'\n" +
            "        obj SealedMessage\n" +
            "    ASSERTION\n" +
            "        pred 'hasRecipient'\n" +
            "        obj SealedMessage",
            envelope.TreeFormatOpt(new TreeFormatOpts(hideNodes: true)));

        Assert.Equal(
            envelope.ElementsCount,
            envelope.TreeFormat().Split('\n').Length);
    }

    [Fact]
    public void TestAssertionPositions()
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

        Assert.Equal(
            "\"subject\" [ \"predicate\" [ \"predicate-predicate\": \"predicate-object\" ] : \"object\" [ \"object-predicate\": \"object-object\" ] ]",
            envelope.FormatFlat());

        Assert.Equal(
            "e06d7003 NODE\n" +
            "    8e4e62eb subj \"subject\"\n" +
            "    91a436e0 ASSERTION\n" +
            "        cece8b2c pred NODE\n" +
            "            d21efb76 subj \"predicate\"\n" +
            "            66a0c92b ASSERTION\n" +
            "                ab829e9f pred \"predicate-predicate\"\n" +
            "                f1098628 obj \"predicate-object\"\n" +
            "        03a99a27 obj NODE\n" +
            "            fda63155 subj \"object\"\n" +
            "            d1878aea ASSERTION\n" +
            "                88bb262f pred \"object-predicate\"\n" +
            "                0bdb89a6 obj \"object-object\"",
            envelope.TreeFormat());

        Assert.Equal(
            "subj \"subject\"\n" +
            "    ASSERTION\n" +
            "        subj \"predicate\"\n" +
            "            ASSERTION\n" +
            "                pred \"predicate-predicate\"\n" +
            "                obj \"predicate-object\"\n" +
            "        subj \"object\"\n" +
            "            ASSERTION\n" +
            "                pred \"object-predicate\"\n" +
            "                obj \"object-object\"",
            envelope.TreeFormatOpt(new TreeFormatOpts(hideNodes: true)));

        Assert.Equal(
            envelope.ElementsCount,
            envelope.TreeFormat().Split('\n').Length);
    }

    [Fact]
    public void TestComplexMetadata()
    {
        var author = Envelope.Create(ARID.FromData(Convert.FromHexString(
                "9c747ace78a4c826392510dd6285551e7df4e5164729a1b36198e56e017666c8")))
            .AddAssertion(KnownValuesRegistry.DereferenceVia, "LibraryOfCongress")
            .AddAssertion(KnownValuesRegistry.Name, "Ayn Rand")
            .CheckEncoding();

        var nameEn = Envelope.Create("Atlas Shrugged")
            .AddAssertion(KnownValuesRegistry.Language, "en");

        var nameEs = Envelope.Create("La rebeli\u00f3n de Atlas")
            .AddAssertion(KnownValuesRegistry.Language, "es");

        var work = Envelope.Create(ARID.FromData(Convert.FromHexString(
                "7fb90a9d96c07f39f75ea6acf392d79f241fac4ec0be2120f7c82489711e3e80")))
            .AddAssertion(KnownValuesRegistry.IsA, "novel")
            .AddAssertion("isbn", "9780451191144")
            .AddAssertion("author", author)
            .AddAssertion(KnownValuesRegistry.DereferenceVia, "LibraryOfCongress")
            .AddAssertion(KnownValuesRegistry.Name, nameEn)
            .AddAssertion(KnownValuesRegistry.Name, nameEs)
            .CheckEncoding();

        var bookData = "This is the entire book \u201cAtlas Shrugged\u201d in EPUB format.";
        var bookMetadata = Envelope.Create(
                Digest.FromImage(System.Text.Encoding.UTF8.GetBytes(bookData)))
            .AddAssertion("work", work)
            .AddAssertion("format", "EPUB")
            .AddAssertion(KnownValuesRegistry.DereferenceVia, "IPFS")
            .CheckEncoding();

        Assert.Equal(
            "Digest(26d05af5) [\n" +
            "    \"format\": \"EPUB\"\n" +
            "    \"work\": ARID(7fb90a9d) [\n" +
            "        'isA': \"novel\"\n" +
            "        \"author\": ARID(9c747ace) [\n" +
            "            'dereferenceVia': \"LibraryOfCongress\"\n" +
            "            'name': \"Ayn Rand\"\n" +
            "        ]\n" +
            "        \"isbn\": \"9780451191144\"\n" +
            "        'dereferenceVia': \"LibraryOfCongress\"\n" +
            "        'name': \"Atlas Shrugged\" [\n" +
            "            'language': \"en\"\n" +
            "        ]\n" +
            "        'name': \"La rebeli\u00f3n de Atlas\" [\n" +
            "            'language': \"es\"\n" +
            "        ]\n" +
            "    ]\n" +
            "    'dereferenceVia': \"IPFS\"\n" +
            "]",
            bookMetadata.Format());

        Assert.Equal(
            "Digest(26d05af5) [ \"format\": \"EPUB\", \"work\": ARID(7fb90a9d) [ 'isA': \"novel\", \"author\": ARID(9c747ace) [ 'dereferenceVia': \"LibraryOfCongress\", 'name': \"Ayn Rand\" ], \"isbn\": \"9780451191144\", 'dereferenceVia': \"LibraryOfCongress\", 'name': \"Atlas Shrugged\" [ 'language': \"en\" ], 'name': \"La rebeli\u00f3n de Atlas\" [ 'language': \"es\" ] ], 'dereferenceVia': \"IPFS\" ]",
            bookMetadata.FormatFlat());

        Assert.Equal(
            "c93370e7 NODE\n" +
            "    0c1e45b9 subj Digest(26d05af5)\n" +
            "    83b00bef ASSERTION\n" +
            "        cdb6a696 pred 'dereferenceVia'\n" +
            "        15eac58f obj \"IPFS\"\n" +
            "    953cdab2 ASSERTION\n" +
            "        a9a86b03 pred \"format\"\n" +
            "        9536cfe0 obj \"EPUB\"\n" +
            "    eec25a61 ASSERTION\n" +
            "        2ddb0b05 pred \"work\"\n" +
            "        26681136 obj NODE\n" +
            "            0c69be6e subj ARID(7fb90a9d)\n" +
            "            1786d8b5 ASSERTION\n" +
            "                4019420b pred \"isbn\"\n" +
            "                69ff76b1 obj \"9780451191144\"\n" +
            "            5355d973 ASSERTION\n" +
            "                2be2d79b pred 'isA'\n" +
            "                6d7c7189 obj \"novel\"\n" +
            "            63cd143a ASSERTION\n" +
            "                14ff9eac pred 'name'\n" +
            "                29fa40b1 obj NODE\n" +
            "                    5e825721 subj \"La rebeli\u00f3n de Atlas\"\n" +
            "                    c8db157b ASSERTION\n" +
            "                        60dfb783 pred 'language'\n" +
            "                        b33e79c2 obj \"es\"\n" +
            "            7d6d5c1d ASSERTION\n" +
            "                29c09059 pred \"author\"\n" +
            "                1ba13788 obj NODE\n" +
            "                    3c47e105 subj ARID(9c747ace)\n" +
            "                    9c10d60f ASSERTION\n" +
            "                        cdb6a696 pred 'dereferenceVia'\n" +
            "                        34a04547 obj \"LibraryOfCongress\"\n" +
            "                    bff8435a ASSERTION\n" +
            "                        14ff9eac pred 'name'\n" +
            "                        98985bd5 obj \"Ayn Rand\"\n" +
            "            9c10d60f ASSERTION\n" +
            "                cdb6a696 pred 'dereferenceVia'\n" +
            "                34a04547 obj \"LibraryOfCongress\"\n" +
            "            b722c07c ASSERTION\n" +
            "                14ff9eac pred 'name'\n" +
            "                0cfacc06 obj NODE\n" +
            "                    e84c3091 subj \"Atlas Shrugged\"\n" +
            "                    b80d3b05 ASSERTION\n" +
            "                        60dfb783 pred 'language'\n" +
            "                        6700869c obj \"en\"",
            bookMetadata.TreeFormat());

        Assert.Equal(
            "subj Digest(26d05af5)\n" +
            "    ASSERTION\n" +
            "        pred 'dereferenceVia'\n" +
            "        obj \"IPFS\"\n" +
            "    ASSERTION\n" +
            "        pred \"format\"\n" +
            "        obj \"EPUB\"\n" +
            "    ASSERTION\n" +
            "        pred \"work\"\n" +
            "        subj ARID(7fb90a9d)\n" +
            "            ASSERTION\n" +
            "                pred \"isbn\"\n" +
            "                obj \"9780451191144\"\n" +
            "            ASSERTION\n" +
            "                pred 'isA'\n" +
            "                obj \"novel\"\n" +
            "            ASSERTION\n" +
            "                pred 'name'\n" +
            "                subj \"La rebeli\u00f3n de Atlas\"\n" +
            "                    ASSERTION\n" +
            "                        pred 'language'\n" +
            "                        obj \"es\"\n" +
            "            ASSERTION\n" +
            "                pred \"author\"\n" +
            "                subj ARID(9c747ace)\n" +
            "                    ASSERTION\n" +
            "                        pred 'dereferenceVia'\n" +
            "                        obj \"LibraryOfCongress\"\n" +
            "                    ASSERTION\n" +
            "                        pred 'name'\n" +
            "                        obj \"Ayn Rand\"\n" +
            "            ASSERTION\n" +
            "                pred 'dereferenceVia'\n" +
            "                obj \"LibraryOfCongress\"\n" +
            "            ASSERTION\n" +
            "                pred 'name'\n" +
            "                subj \"Atlas Shrugged\"\n" +
            "                    ASSERTION\n" +
            "                        pred 'language'\n" +
            "                        obj \"en\"",
            bookMetadata.TreeFormatOpt(new TreeFormatOpts(hideNodes: true)));

        Assert.Equal(
            bookMetadata.ElementsCount,
            bookMetadata.TreeFormat().Split('\n').Length);
    }

    [Fact]
    public void TestCredential()
    {
        var credential = TestData.Credential();

        Assert.Equal(
            "{\n" +
            "    ARID(4676635a) [\n" +
            "        'isA': \"Certificate of Completion\"\n" +
            "        \"certificateNumber\": \"123-456-789\"\n" +
            "        \"continuingEducationUnits\": 1\n" +
            "        \"expirationDate\": 2028-01-01\n" +
            "        \"firstName\": \"James\"\n" +
            "        \"issueDate\": 2020-01-01\n" +
            "        \"lastName\": \"Maxwell\"\n" +
            "        \"photo\": \"This is James Maxwell's photo.\"\n" +
            "        \"professionalDevelopmentHours\": 15\n" +
            "        \"subject\": \"RF and Microwave Engineering\"\n" +
            "        \"topics\": [\"Subject 1\", \"Subject 2\"]\n" +
            "        'controller': \"Example Electrical Engineering Board\"\n" +
            "        'issuer': \"Example Electrical Engineering Board\"\n" +
            "    ]\n" +
            "} [\n" +
            "    'note': \"Signed by Example Electrical Engineering Board\"\n" +
            "    'signed': Signature\n" +
            "]",
            credential.Format());

        Assert.Equal(
            "{ ARID(4676635a) [ 'isA': \"Certificate of Completion\", \"certificateNumber\": \"123-456-789\", \"continuingEducationUnits\": 1, \"expirationDate\": 2028-01-01, \"firstName\": \"James\", \"issueDate\": 2020-01-01, \"lastName\": \"Maxwell\", \"photo\": \"This is James Maxwell's photo.\", \"professionalDevelopmentHours\": 15, \"subject\": \"RF and Microwave Engineering\", \"topics\": [\"Subject 1\", \"Subject 2\"], 'controller': \"Example Electrical Engineering Board\", 'issuer': \"Example Electrical Engineering Board\" ] } [ 'note': \"Signed by Example Electrical Engineering Board\", 'signed': Signature ]",
            credential.FormatFlat());

        Assert.Equal(
            "0b721f78 NODE\n" +
            "    397a2d4c subj WRAPPED\n" +
            "        8122ffa9 cont NODE\n" +
            "            10d3de01 subj ARID(4676635a)\n" +
            "            1f9ff098 ASSERTION\n" +
            "                9e3bff3a pred \"certificateNumber\"\n" +
            "                21c21808 obj \"123-456-789\"\n" +
            "            36c254d0 ASSERTION\n" +
            "                6e5d379f pred \"expirationDate\"\n" +
            "                639ae9bf obj 2028-01-01\n" +
            "            3c114201 ASSERTION\n" +
            "                5f82a16a pred \"lastName\"\n" +
            "                fe4d5230 obj \"Maxwell\"\n" +
            "            4a9b2e4d ASSERTION\n" +
            "                222afe69 pred \"issueDate\"\n" +
            "                cb67f31d obj 2020-01-01\n" +
            "            4d67bba0 ASSERTION\n" +
            "                2be2d79b pred 'isA'\n" +
            "                051beee6 obj \"Certificate of Completion\"\n" +
            "            5171cbaf ASSERTION\n" +
            "                3976ef74 pred \"photo\"\n" +
            "                231b8527 obj \"This is James Maxwell's photo.\"\n" +
            "            54b3e1e7 ASSERTION\n" +
            "                f13aa855 pred \"professionalDevelopmentHours\"\n" +
            "                dc0e9c36 obj 15\n" +
            "            5dc6d4e3 ASSERTION\n" +
            "                4395643b pred \"firstName\"\n" +
            "                d6d0b768 obj \"James\"\n" +
            "            68895d8e ASSERTION\n" +
            "                e6bf4dd3 pred \"topics\"\n" +
            "                543fcc09 obj [\"Subject 1\", \"Subject 2\"]\n" +
            "            8ec5e912 ASSERTION\n" +
            "                2b191589 pred \"continuingEducationUnits\"\n" +
            "                4bf5122f obj 1\n" +
            "            9b3d4785 ASSERTION\n" +
            "                af10ee92 pred 'controller'\n" +
            "                f8489ac1 obj \"Example Electrical Engineering Board\"\n" +
            "            caf5ced3 ASSERTION\n" +
            "                8e4e62eb pred \"subject\"\n" +
            "                202c10ef obj \"RF and Microwave Engineering\"\n" +
            "            d3e0cc15 ASSERTION\n" +
            "                6dd16ba3 pred 'issuer'\n" +
            "                f8489ac1 obj \"Example Electrical Engineering Board\"\n" +
            "    46a02aaf ASSERTION\n" +
            "        d0e39e78 pred 'signed'\n" +
            "        34c14941 obj Signature\n" +
            "    e6d7fca0 ASSERTION\n" +
            "        0fcd6a39 pred 'note'\n" +
            "        f106bad1 obj \"Signed by Example Electrical Engineering\u2026\"",
            credential.TreeFormat());

        Assert.Equal(
            "subj WRAPPED\n" +
            "    subj ARID(4676635a)\n" +
            "        ASSERTION\n" +
            "            pred \"certificateNumber\"\n" +
            "            obj \"123-456-789\"\n" +
            "        ASSERTION\n" +
            "            pred \"expirationDate\"\n" +
            "            obj 2028-01-01\n" +
            "        ASSERTION\n" +
            "            pred \"lastName\"\n" +
            "            obj \"Maxwell\"\n" +
            "        ASSERTION\n" +
            "            pred \"issueDate\"\n" +
            "            obj 2020-01-01\n" +
            "        ASSERTION\n" +
            "            pred 'isA'\n" +
            "            obj \"Certificate of Completion\"\n" +
            "        ASSERTION\n" +
            "            pred \"photo\"\n" +
            "            obj \"This is James Maxwell's photo.\"\n" +
            "        ASSERTION\n" +
            "            pred \"professionalDevelopmentHours\"\n" +
            "            obj 15\n" +
            "        ASSERTION\n" +
            "            pred \"firstName\"\n" +
            "            obj \"James\"\n" +
            "        ASSERTION\n" +
            "            pred \"topics\"\n" +
            "            obj [\"Subject 1\", \"Subject 2\"]\n" +
            "        ASSERTION\n" +
            "            pred \"continuingEducationUnits\"\n" +
            "            obj 1\n" +
            "        ASSERTION\n" +
            "            pred 'controller'\n" +
            "            obj \"Example Electrical Engineering Board\"\n" +
            "        ASSERTION\n" +
            "            pred \"subject\"\n" +
            "            obj \"RF and Microwave Engineering\"\n" +
            "        ASSERTION\n" +
            "            pred 'issuer'\n" +
            "            obj \"Example Electrical Engineering Board\"\n" +
            "    ASSERTION\n" +
            "        pred 'signed'\n" +
            "        obj Signature\n" +
            "    ASSERTION\n" +
            "        pred 'note'\n" +
            "        obj \"Signed by Example Electrical Engineering\u2026\"",
            credential.TreeFormatOpt(new TreeFormatOpts(hideNodes: true)));

        Assert.Equal(
            credential.ElementsCount,
            credential.TreeFormat().Split('\n').Length);
    }

    [Fact]
    public void TestRedactedCredential()
    {
        var redactedCredential = TestData.RedactedCredential();
        var rng = SeededRandomNumberGenerator.CreateFake();
        var options = new SigningOptions.SchnorrOptions(rng);
        var warranty = redactedCredential
            .Wrap()
            .AddAssertion("employeeHiredDate", CborDate.FromYmd(2022, 1, 1))
            .AddAssertion("employeeStatus", "active")
            .Wrap()
            .AddAssertion(KnownValuesRegistry.Note, "Signed by Employer Corp.")
            .AddSignatureOpt(TestData.BobPrivateKey(), options, null)
            .CheckEncoding();

        Assert.Equal(
            "{\n" +
            "    {\n" +
            "        {\n" +
            "            ARID(4676635a) [\n" +
            "                'isA': \"Certificate of Completion\"\n" +
            "                \"expirationDate\": 2028-01-01\n" +
            "                \"firstName\": \"James\"\n" +
            "                \"lastName\": \"Maxwell\"\n" +
            "                \"subject\": \"RF and Microwave Engineering\"\n" +
            "                'issuer': \"Example Electrical Engineering Board\"\n" +
            "                ELIDED (7)\n" +
            "            ]\n" +
            "        } [\n" +
            "            'note': \"Signed by Example Electrical Engineering Board\"\n" +
            "            'signed': Signature\n" +
            "        ]\n" +
            "    } [\n" +
            "        \"employeeHiredDate\": 2022-01-01\n" +
            "        \"employeeStatus\": \"active\"\n" +
            "    ]\n" +
            "} [\n" +
            "    'note': \"Signed by Employer Corp.\"\n" +
            "    'signed': Signature\n" +
            "]",
            warranty.Format());

        Assert.Equal(
            "{ { { ARID(4676635a) [ 'isA': \"Certificate of Completion\", \"expirationDate\": 2028-01-01, \"firstName\": \"James\", \"lastName\": \"Maxwell\", \"subject\": \"RF and Microwave Engineering\", 'issuer': \"Example Electrical Engineering Board\", ELIDED (7) ] } [ 'note': \"Signed by Example Electrical Engineering Board\", 'signed': Signature ] } [ \"employeeHiredDate\": 2022-01-01, \"employeeStatus\": \"active\" ] } [ 'note': \"Signed by Employer Corp.\", 'signed': Signature ]",
            warranty.FormatFlat());

        Assert.Equal(
            "7ab3e6b1 NODE\n" +
            "    3907ee6f subj WRAPPED\n" +
            "        719d5955 cont NODE\n" +
            "            10fb2e18 subj WRAPPED\n" +
            "                0b721f78 cont NODE\n" +
            "                    397a2d4c subj WRAPPED\n" +
            "                        8122ffa9 cont NODE\n" +
            "                            10d3de01 subj ARID(4676635a)\n" +
            "                            1f9ff098 ELIDED\n" +
            "                            36c254d0 ASSERTION\n" +
            "                                6e5d379f pred \"expirationDate\"\n" +
            "                                639ae9bf obj 2028-01-01\n" +
            "                            3c114201 ASSERTION\n" +
            "                                5f82a16a pred \"lastName\"\n" +
            "                                fe4d5230 obj \"Maxwell\"\n" +
            "                            4a9b2e4d ELIDED\n" +
            "                            4d67bba0 ASSERTION\n" +
            "                                2be2d79b pred 'isA'\n" +
            "                                051beee6 obj \"Certificate of Completion\"\n" +
            "                            5171cbaf ELIDED\n" +
            "                            54b3e1e7 ELIDED\n" +
            "                            5dc6d4e3 ASSERTION\n" +
            "                                4395643b pred \"firstName\"\n" +
            "                                d6d0b768 obj \"James\"\n" +
            "                            68895d8e ELIDED\n" +
            "                            8ec5e912 ELIDED\n" +
            "                            9b3d4785 ELIDED\n" +
            "                            caf5ced3 ASSERTION\n" +
            "                                8e4e62eb pred \"subject\"\n" +
            "                                202c10ef obj \"RF and Microwave Engineering\"\n" +
            "                            d3e0cc15 ASSERTION\n" +
            "                                6dd16ba3 pred 'issuer'\n" +
            "                                f8489ac1 obj \"Example Electrical Engineering Board\"\n" +
            "                    46a02aaf ASSERTION\n" +
            "                        d0e39e78 pred 'signed'\n" +
            "                        34c14941 obj Signature\n" +
            "                    e6d7fca0 ASSERTION\n" +
            "                        0fcd6a39 pred 'note'\n" +
            "                        f106bad1 obj \"Signed by Example Electrical Engineering\u2026\"\n" +
            "            4c159c16 ASSERTION\n" +
            "                e1ae011e pred \"employeeHiredDate\"\n" +
            "                13b5a817 obj 2022-01-01\n" +
            "            e071508b ASSERTION\n" +
            "                d03e7352 pred \"employeeStatus\"\n" +
            "                1d7a790d obj \"active\"\n" +
            "    874aa7e1 ASSERTION\n" +
            "        0fcd6a39 pred 'note'\n" +
            "        f59806d2 obj \"Signed by Employer Corp.\"\n" +
            "    d21d2033 ASSERTION\n" +
            "        d0e39e78 pred 'signed'\n" +
            "        5ba600c9 obj Signature",
            warranty.TreeFormat());

        Assert.Equal(
            "subj WRAPPED\n" +
            "    subj WRAPPED\n" +
            "        subj WRAPPED\n" +
            "            subj ARID(4676635a)\n" +
            "                ELIDED\n" +
            "                ASSERTION\n" +
            "                    pred \"expirationDate\"\n" +
            "                    obj 2028-01-01\n" +
            "                ASSERTION\n" +
            "                    pred \"lastName\"\n" +
            "                    obj \"Maxwell\"\n" +
            "                ELIDED\n" +
            "                ASSERTION\n" +
            "                    pred 'isA'\n" +
            "                    obj \"Certificate of Completion\"\n" +
            "                ELIDED\n" +
            "                ELIDED\n" +
            "                ASSERTION\n" +
            "                    pred \"firstName\"\n" +
            "                    obj \"James\"\n" +
            "                ELIDED\n" +
            "                ELIDED\n" +
            "                ELIDED\n" +
            "                ASSERTION\n" +
            "                    pred \"subject\"\n" +
            "                    obj \"RF and Microwave Engineering\"\n" +
            "                ASSERTION\n" +
            "                    pred 'issuer'\n" +
            "                    obj \"Example Electrical Engineering Board\"\n" +
            "            ASSERTION\n" +
            "                pred 'signed'\n" +
            "                obj Signature\n" +
            "            ASSERTION\n" +
            "                pred 'note'\n" +
            "                obj \"Signed by Example Electrical Engineering\u2026\"\n" +
            "        ASSERTION\n" +
            "            pred \"employeeHiredDate\"\n" +
            "            obj 2022-01-01\n" +
            "        ASSERTION\n" +
            "            pred \"employeeStatus\"\n" +
            "            obj \"active\"\n" +
            "    ASSERTION\n" +
            "        pred 'note'\n" +
            "        obj \"Signed by Employer Corp.\"\n" +
            "    ASSERTION\n" +
            "        pred 'signed'\n" +
            "        obj Signature",
            warranty.TreeFormatOpt(new TreeFormatOpts(hideNodes: true)));

        Assert.Equal(
            warranty.ElementsCount,
            warranty.TreeFormat().Split('\n').Length);

        // Mermaid format with dark theme
        var mermaidDark = warranty.MermaidFormatOpt(new MermaidFormatOpts(
            theme: MermaidTheme.Dark));
        Assert.Equal(
            "%%{ init: { 'theme': 'dark', 'flowchart': { 'curve': 'basis' } } }%%\n" +
            "graph LR\n" +
            "0((\"NODE<br>7ab3e6b1\"))\n" +
            "    0 -- subj --> 1[/\"WRAPPED<br>3907ee6f\"\\]\n" +
            "        1 -- cont --> 2((\"NODE<br>719d5955\"))\n" +
            "            2 -- subj --> 3[/\"WRAPPED<br>10fb2e18\"\\]\n" +
            "                3 -- cont --> 4((\"NODE<br>0b721f78\"))\n" +
            "                    4 -- subj --> 5[/\"WRAPPED<br>397a2d4c\"\\]\n" +
            "                        5 -- cont --> 6((\"NODE<br>8122ffa9\"))\n" +
            "                            6 -- subj --> 7[\"ARID(4676635a)<br>10d3de01\"]\n" +
            "                            6 --> 8{{\"ELIDED<br>1f9ff098\"}}\n" +
            "                            6 --> 9([\"ASSERTION<br>36c254d0\"])\n" +
            "                                9 -- pred --> 10[\"&quot;expirationDate&quot;<br>6e5d379f\"]\n" +
            "                                9 -- obj --> 11[\"2028-01-01<br>639ae9bf\"]\n" +
            "                            6 --> 12([\"ASSERTION<br>3c114201\"])\n" +
            "                                12 -- pred --> 13[\"&quot;lastName&quot;<br>5f82a16a\"]\n" +
            "                                12 -- obj --> 14[\"&quot;Maxwell&quot;<br>fe4d5230\"]\n" +
            "                            6 --> 15{{\"ELIDED<br>4a9b2e4d\"}}\n" +
            "                            6 --> 16([\"ASSERTION<br>4d67bba0\"])\n" +
            "                                16 -- pred --> 17[/\"'isA'<br>2be2d79b\"/]\n" +
            "                                16 -- obj --> 18[\"&quot;Certificate of Compl\u2026&quot;<br>051beee6\"]\n" +
            "                            6 --> 19{{\"ELIDED<br>5171cbaf\"}}\n" +
            "                            6 --> 20{{\"ELIDED<br>54b3e1e7\"}}\n" +
            "                            6 --> 21([\"ASSERTION<br>5dc6d4e3\"])\n" +
            "                                21 -- pred --> 22[\"&quot;firstName&quot;<br>4395643b\"]\n" +
            "                                21 -- obj --> 23[\"&quot;James&quot;<br>d6d0b768\"]\n" +
            "                            6 --> 24{{\"ELIDED<br>68895d8e\"}}\n" +
            "                            6 --> 25{{\"ELIDED<br>8ec5e912\"}}\n" +
            "                            6 --> 26{{\"ELIDED<br>9b3d4785\"}}\n" +
            "                            6 --> 27([\"ASSERTION<br>caf5ced3\"])\n" +
            "                                27 -- pred --> 28[\"&quot;subject&quot;<br>8e4e62eb\"]\n" +
            "                                27 -- obj --> 29[\"&quot;RF and Microwave Eng\u2026&quot;<br>202c10ef\"]\n" +
            "                            6 --> 30([\"ASSERTION<br>d3e0cc15\"])\n" +
            "                                30 -- pred --> 31[/\"'issuer'<br>6dd16ba3\"/]\n" +
            "                                30 -- obj --> 32[\"&quot;Example Electrical E\u2026&quot;<br>f8489ac1\"]\n" +
            "                    4 --> 33([\"ASSERTION<br>46a02aaf\"])\n" +
            "                        33 -- pred --> 34[/\"'signed'<br>d0e39e78\"/]\n" +
            "                        33 -- obj --> 35[\"Signature<br>34c14941\"]\n" +
            "                    4 --> 36([\"ASSERTION<br>e6d7fca0\"])\n" +
            "                        36 -- pred --> 37[/\"'note'<br>0fcd6a39\"/]\n" +
            "                        36 -- obj --> 38[\"&quot;Signed by Example El\u2026&quot;<br>f106bad1\"]\n" +
            "            2 --> 39([\"ASSERTION<br>4c159c16\"])\n" +
            "                39 -- pred --> 40[\"&quot;employeeHiredDate&quot;<br>e1ae011e\"]\n" +
            "                39 -- obj --> 41[\"2022-01-01<br>13b5a817\"]\n" +
            "            2 --> 42([\"ASSERTION<br>e071508b\"])\n" +
            "                42 -- pred --> 43[\"&quot;employeeStatus&quot;<br>d03e7352\"]\n" +
            "                42 -- obj --> 44[\"&quot;active&quot;<br>1d7a790d\"]\n" +
            "    0 --> 45([\"ASSERTION<br>874aa7e1\"])\n" +
            "        45 -- pred --> 46[/\"'note'<br>0fcd6a39\"/]\n" +
            "        45 -- obj --> 47[\"&quot;Signed by Employer C\u2026&quot;<br>f59806d2\"]\n" +
            "    0 --> 48([\"ASSERTION<br>d21d2033\"])\n" +
            "        48 -- pred --> 49[/\"'signed'<br>d0e39e78\"/]\n" +
            "        48 -- obj --> 50[\"Signature<br>5ba600c9\"]\n" +
            "style 0 stroke:red,stroke-width:4px\n" +
            "style 1 stroke:blue,stroke-width:4px\n" +
            "style 2 stroke:red,stroke-width:4px\n" +
            "style 3 stroke:blue,stroke-width:4px\n" +
            "style 4 stroke:red,stroke-width:4px\n" +
            "style 5 stroke:blue,stroke-width:4px\n" +
            "style 6 stroke:red,stroke-width:4px\n" +
            "style 7 stroke:teal,stroke-width:4px\n" +
            "style 8 stroke:gray,stroke-width:4px\n" +
            "style 9 stroke:green,stroke-width:4px\n" +
            "style 10 stroke:teal,stroke-width:4px\n" +
            "style 11 stroke:teal,stroke-width:4px\n" +
            "style 12 stroke:green,stroke-width:4px\n" +
            "style 13 stroke:teal,stroke-width:4px\n" +
            "style 14 stroke:teal,stroke-width:4px\n" +
            "style 15 stroke:gray,stroke-width:4px\n" +
            "style 16 stroke:green,stroke-width:4px\n" +
            "style 17 stroke:goldenrod,stroke-width:4px\n" +
            "style 18 stroke:teal,stroke-width:4px\n" +
            "style 19 stroke:gray,stroke-width:4px\n" +
            "style 20 stroke:gray,stroke-width:4px\n" +
            "style 21 stroke:green,stroke-width:4px\n" +
            "style 22 stroke:teal,stroke-width:4px\n" +
            "style 23 stroke:teal,stroke-width:4px\n" +
            "style 24 stroke:gray,stroke-width:4px\n" +
            "style 25 stroke:gray,stroke-width:4px\n" +
            "style 26 stroke:gray,stroke-width:4px\n" +
            "style 27 stroke:green,stroke-width:4px\n" +
            "style 28 stroke:teal,stroke-width:4px\n" +
            "style 29 stroke:teal,stroke-width:4px\n" +
            "style 30 stroke:green,stroke-width:4px\n" +
            "style 31 stroke:goldenrod,stroke-width:4px\n" +
            "style 32 stroke:teal,stroke-width:4px\n" +
            "style 33 stroke:green,stroke-width:4px\n" +
            "style 34 stroke:goldenrod,stroke-width:4px\n" +
            "style 35 stroke:teal,stroke-width:4px\n" +
            "style 36 stroke:green,stroke-width:4px\n" +
            "style 37 stroke:goldenrod,stroke-width:4px\n" +
            "style 38 stroke:teal,stroke-width:4px\n" +
            "style 39 stroke:green,stroke-width:4px\n" +
            "style 40 stroke:teal,stroke-width:4px\n" +
            "style 41 stroke:teal,stroke-width:4px\n" +
            "style 42 stroke:green,stroke-width:4px\n" +
            "style 43 stroke:teal,stroke-width:4px\n" +
            "style 44 stroke:teal,stroke-width:4px\n" +
            "style 45 stroke:green,stroke-width:4px\n" +
            "style 46 stroke:goldenrod,stroke-width:4px\n" +
            "style 47 stroke:teal,stroke-width:4px\n" +
            "style 48 stroke:green,stroke-width:4px\n" +
            "style 49 stroke:goldenrod,stroke-width:4px\n" +
            "style 50 stroke:teal,stroke-width:4px\n" +
            "linkStyle 0 stroke:red,stroke-width:2px\n" +
            "linkStyle 1 stroke:blue,stroke-width:2px\n" +
            "linkStyle 2 stroke:red,stroke-width:2px\n" +
            "linkStyle 3 stroke:blue,stroke-width:2px\n" +
            "linkStyle 4 stroke:red,stroke-width:2px\n" +
            "linkStyle 5 stroke:blue,stroke-width:2px\n" +
            "linkStyle 6 stroke:red,stroke-width:2px\n" +
            "linkStyle 7 stroke-width:2px\n" +
            "linkStyle 8 stroke-width:2px\n" +
            "linkStyle 9 stroke:cyan,stroke-width:2px\n" +
            "linkStyle 10 stroke:magenta,stroke-width:2px\n" +
            "linkStyle 11 stroke-width:2px\n" +
            "linkStyle 12 stroke:cyan,stroke-width:2px\n" +
            "linkStyle 13 stroke:magenta,stroke-width:2px\n" +
            "linkStyle 14 stroke-width:2px\n" +
            "linkStyle 15 stroke-width:2px\n" +
            "linkStyle 16 stroke:cyan,stroke-width:2px\n" +
            "linkStyle 17 stroke:magenta,stroke-width:2px\n" +
            "linkStyle 18 stroke-width:2px\n" +
            "linkStyle 19 stroke-width:2px\n" +
            "linkStyle 20 stroke-width:2px\n" +
            "linkStyle 21 stroke:cyan,stroke-width:2px\n" +
            "linkStyle 22 stroke:magenta,stroke-width:2px\n" +
            "linkStyle 23 stroke-width:2px\n" +
            "linkStyle 24 stroke-width:2px\n" +
            "linkStyle 25 stroke-width:2px\n" +
            "linkStyle 26 stroke-width:2px\n" +
            "linkStyle 27 stroke:cyan,stroke-width:2px\n" +
            "linkStyle 28 stroke:magenta,stroke-width:2px\n" +
            "linkStyle 29 stroke-width:2px\n" +
            "linkStyle 30 stroke:cyan,stroke-width:2px\n" +
            "linkStyle 31 stroke:magenta,stroke-width:2px\n" +
            "linkStyle 32 stroke-width:2px\n" +
            "linkStyle 33 stroke:cyan,stroke-width:2px\n" +
            "linkStyle 34 stroke:magenta,stroke-width:2px\n" +
            "linkStyle 35 stroke-width:2px\n" +
            "linkStyle 36 stroke:cyan,stroke-width:2px\n" +
            "linkStyle 37 stroke:magenta,stroke-width:2px\n" +
            "linkStyle 38 stroke-width:2px\n" +
            "linkStyle 39 stroke:cyan,stroke-width:2px\n" +
            "linkStyle 40 stroke:magenta,stroke-width:2px\n" +
            "linkStyle 41 stroke-width:2px\n" +
            "linkStyle 42 stroke:cyan,stroke-width:2px\n" +
            "linkStyle 43 stroke:magenta,stroke-width:2px\n" +
            "linkStyle 44 stroke-width:2px\n" +
            "linkStyle 45 stroke:cyan,stroke-width:2px\n" +
            "linkStyle 46 stroke:magenta,stroke-width:2px\n" +
            "linkStyle 47 stroke-width:2px\n" +
            "linkStyle 48 stroke:cyan,stroke-width:2px\n" +
            "linkStyle 49 stroke:magenta,stroke-width:2px",
            mermaidDark);

        // Mermaid format with forest theme, monochrome, top-to-bottom, hide nodes
        var mermaidForest = warranty.MermaidFormatOpt(new MermaidFormatOpts(
            monochrome: true,
            theme: MermaidTheme.Forest,
            orientation: MermaidOrientation.TopToBottom,
            hideNodes: true));
        Assert.Equal(
            "%%{ init: { 'theme': 'forest', 'flowchart': { 'curve': 'basis' } } }%%\n" +
            "graph TB\n" +
            "0[/\"WRAPPED\"\\]\n" +
            "    0 -- subj --> 1[/\"WRAPPED\"\\]\n" +
            "        1 -- subj --> 2[/\"WRAPPED\"\\]\n" +
            "            2 -- subj --> 3[\"ARID(4676635a)\"]\n" +
            "                3 --> 4{{\"ELIDED\"}}\n" +
            "                3 --> 5([\"ASSERTION\"])\n" +
            "                    5 -- pred --> 6[\"&quot;expirationDate&quot;\"]\n" +
            "                    5 -- obj --> 7[\"2028-01-01\"]\n" +
            "                3 --> 8([\"ASSERTION\"])\n" +
            "                    8 -- pred --> 9[\"&quot;lastName&quot;\"]\n" +
            "                    8 -- obj --> 10[\"&quot;Maxwell&quot;\"]\n" +
            "                3 --> 11{{\"ELIDED\"}}\n" +
            "                3 --> 12([\"ASSERTION\"])\n" +
            "                    12 -- pred --> 13[/\"'isA'\"/]\n" +
            "                    12 -- obj --> 14[\"&quot;Certificate of Compl\u2026&quot;\"]\n" +
            "                3 --> 15{{\"ELIDED\"}}\n" +
            "                3 --> 16{{\"ELIDED\"}}\n" +
            "                3 --> 17([\"ASSERTION\"])\n" +
            "                    17 -- pred --> 18[\"&quot;firstName&quot;\"]\n" +
            "                    17 -- obj --> 19[\"&quot;James&quot;\"]\n" +
            "                3 --> 20{{\"ELIDED\"}}\n" +
            "                3 --> 21{{\"ELIDED\"}}\n" +
            "                3 --> 22{{\"ELIDED\"}}\n" +
            "                3 --> 23([\"ASSERTION\"])\n" +
            "                    23 -- pred --> 24[\"&quot;subject&quot;\"]\n" +
            "                    23 -- obj --> 25[\"&quot;RF and Microwave Eng\u2026&quot;\"]\n" +
            "                3 --> 26([\"ASSERTION\"])\n" +
            "                    26 -- pred --> 27[/\"'issuer'\"/]\n" +
            "                    26 -- obj --> 28[\"&quot;Example Electrical E\u2026&quot;\"]\n" +
            "            2 --> 29([\"ASSERTION\"])\n" +
            "                29 -- pred --> 30[/\"'signed'\"/]\n" +
            "                29 -- obj --> 31[\"Signature\"]\n" +
            "            2 --> 32([\"ASSERTION\"])\n" +
            "                32 -- pred --> 33[/\"'note'\"/]\n" +
            "                32 -- obj --> 34[\"&quot;Signed by Example El\u2026&quot;\"]\n" +
            "        1 --> 35([\"ASSERTION\"])\n" +
            "            35 -- pred --> 36[\"&quot;employeeHiredDate&quot;\"]\n" +
            "            35 -- obj --> 37[\"2022-01-01\"]\n" +
            "        1 --> 38([\"ASSERTION\"])\n" +
            "            38 -- pred --> 39[\"&quot;employeeStatus&quot;\"]\n" +
            "            38 -- obj --> 40[\"&quot;active&quot;\"]\n" +
            "    0 --> 41([\"ASSERTION\"])\n" +
            "        41 -- pred --> 42[/\"'note'\"/]\n" +
            "        41 -- obj --> 43[\"&quot;Signed by Employer C\u2026&quot;\"]\n" +
            "    0 --> 44([\"ASSERTION\"])\n" +
            "        44 -- pred --> 45[/\"'signed'\"/]\n" +
            "        44 -- obj --> 46[\"Signature\"]\n" +
            "style 0 stroke-width:4px\n" +
            "style 1 stroke-width:4px\n" +
            "style 2 stroke-width:4px\n" +
            "style 3 stroke-width:4px\n" +
            "style 4 stroke-width:4px\n" +
            "style 5 stroke-width:4px\n" +
            "style 6 stroke-width:4px\n" +
            "style 7 stroke-width:4px\n" +
            "style 8 stroke-width:4px\n" +
            "style 9 stroke-width:4px\n" +
            "style 10 stroke-width:4px\n" +
            "style 11 stroke-width:4px\n" +
            "style 12 stroke-width:4px\n" +
            "style 13 stroke-width:4px\n" +
            "style 14 stroke-width:4px\n" +
            "style 15 stroke-width:4px\n" +
            "style 16 stroke-width:4px\n" +
            "style 17 stroke-width:4px\n" +
            "style 18 stroke-width:4px\n" +
            "style 19 stroke-width:4px\n" +
            "style 20 stroke-width:4px\n" +
            "style 21 stroke-width:4px\n" +
            "style 22 stroke-width:4px\n" +
            "style 23 stroke-width:4px\n" +
            "style 24 stroke-width:4px\n" +
            "style 25 stroke-width:4px\n" +
            "style 26 stroke-width:4px\n" +
            "style 27 stroke-width:4px\n" +
            "style 28 stroke-width:4px\n" +
            "style 29 stroke-width:4px\n" +
            "style 30 stroke-width:4px\n" +
            "style 31 stroke-width:4px\n" +
            "style 32 stroke-width:4px\n" +
            "style 33 stroke-width:4px\n" +
            "style 34 stroke-width:4px\n" +
            "style 35 stroke-width:4px\n" +
            "style 36 stroke-width:4px\n" +
            "style 37 stroke-width:4px\n" +
            "style 38 stroke-width:4px\n" +
            "style 39 stroke-width:4px\n" +
            "style 40 stroke-width:4px\n" +
            "style 41 stroke-width:4px\n" +
            "style 42 stroke-width:4px\n" +
            "style 43 stroke-width:4px\n" +
            "style 44 stroke-width:4px\n" +
            "style 45 stroke-width:4px\n" +
            "style 46 stroke-width:4px\n" +
            "linkStyle 0 stroke-width:2px\n" +
            "linkStyle 1 stroke-width:2px\n" +
            "linkStyle 2 stroke-width:2px\n" +
            "linkStyle 3 stroke-width:2px\n" +
            "linkStyle 4 stroke-width:2px\n" +
            "linkStyle 5 stroke-width:2px\n" +
            "linkStyle 6 stroke-width:2px\n" +
            "linkStyle 7 stroke-width:2px\n" +
            "linkStyle 8 stroke-width:2px\n" +
            "linkStyle 9 stroke-width:2px\n" +
            "linkStyle 10 stroke-width:2px\n" +
            "linkStyle 11 stroke-width:2px\n" +
            "linkStyle 12 stroke-width:2px\n" +
            "linkStyle 13 stroke-width:2px\n" +
            "linkStyle 14 stroke-width:2px\n" +
            "linkStyle 15 stroke-width:2px\n" +
            "linkStyle 16 stroke-width:2px\n" +
            "linkStyle 17 stroke-width:2px\n" +
            "linkStyle 18 stroke-width:2px\n" +
            "linkStyle 19 stroke-width:2px\n" +
            "linkStyle 20 stroke-width:2px\n" +
            "linkStyle 21 stroke-width:2px\n" +
            "linkStyle 22 stroke-width:2px\n" +
            "linkStyle 23 stroke-width:2px\n" +
            "linkStyle 24 stroke-width:2px\n" +
            "linkStyle 25 stroke-width:2px\n" +
            "linkStyle 26 stroke-width:2px\n" +
            "linkStyle 27 stroke-width:2px\n" +
            "linkStyle 28 stroke-width:2px\n" +
            "linkStyle 29 stroke-width:2px\n" +
            "linkStyle 30 stroke-width:2px\n" +
            "linkStyle 31 stroke-width:2px\n" +
            "linkStyle 32 stroke-width:2px\n" +
            "linkStyle 33 stroke-width:2px\n" +
            "linkStyle 34 stroke-width:2px\n" +
            "linkStyle 35 stroke-width:2px\n" +
            "linkStyle 36 stroke-width:2px\n" +
            "linkStyle 37 stroke-width:2px\n" +
            "linkStyle 38 stroke-width:2px\n" +
            "linkStyle 39 stroke-width:2px\n" +
            "linkStyle 40 stroke-width:2px\n" +
            "linkStyle 41 stroke-width:2px\n" +
            "linkStyle 42 stroke-width:2px\n" +
            "linkStyle 43 stroke-width:2px\n" +
            "linkStyle 44 stroke-width:2px\n" +
            "linkStyle 45 stroke-width:2px",
            mermaidForest);
    }
}

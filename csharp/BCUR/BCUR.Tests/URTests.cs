using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCUR.Tests;

public class URTests
{
    [Fact]
    public void SinglePartUr()
    {
        // Encode CBOR message as UR using the ur::ur module's test
        var message = TestHelpers.MakeMessage("Wolf", 50);
        // Wrap in minicbor ByteVec encoding (CBOR byte string tag)
        var cbor = Cbor.ToByteString(message);
        var cborData = cbor.ToCborData();

        var encoded = UREncoding.Encode(cborData, "bytes");
        var expected = "ur:bytes/hdeymejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtgwdpfnsboxgwlbaawzuefywkdplrsrjynbvygabwjldapfcsdwkbrkch";
        Assert.Equal(expected, encoded);

        var (kind, decoded) = UREncoding.Decode(encoded);
        Assert.Equal(URKind.SinglePart, kind);
        Assert.Equal(cborData, decoded);
    }

    [Fact]
    public void UrRoundTrip()
    {
        var cbor = Cbor.FromIntList([1, 2, 3]);
        var ur = UR.Create("test", cbor);
        var urString = ur.ToUrString();
        Assert.Equal("ur:test/lsadaoaxjygonesw", urString);

        var ur2 = UR.FromUrString(urString);
        Assert.Equal("test", ur2.UrTypeStr);
        Assert.Equal(cbor, ur2.Cbor);

        // Case insensitive
        var capsUrString = "UR:TEST/LSADAOAXJYGONESW";
        var ur3 = UR.FromUrString(capsUrString);
        Assert.Equal("test", ur3.UrTypeStr);
        Assert.Equal(cbor, ur3.Cbor);
    }

    [Fact]
    public void UrEncoder20Parts()
    {
        // Encode using fountain codes, matching the ur::ur::tests::test_ur_encoder test
        var message = TestHelpers.MakeMessage("Wolf", 256);
        var cbor = Cbor.ToByteString(message);
        var cborData = cbor.ToCborData();

        var encoder = new FountainEncoder(cborData, 30);

        string[] expected =
        [
            "ur:bytes/1-9/lpadascfadaxcywenbpljkhdcahkadaemejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtdkgslpgh",
            "ur:bytes/2-9/lpaoascfadaxcywenbpljkhdcagwdpfnsboxgwlbaawzuefywkdplrsrjynbvygabwjldapfcsgmghhkhstlrdcxaefz",
            "ur:bytes/3-9/lpaxascfadaxcywenbpljkhdcahelbknlkuejnbadmssfhfrdpsbiegecpasvssovlgeykssjykklronvsjksopdzmol",
            "ur:bytes/4-9/lpaaascfadaxcywenbpljkhdcasotkhemthydawydtaxneurlkosgwcekonertkbrlwmplssjtammdplolsbrdzcrtas",
            "ur:bytes/5-9/lpahascfadaxcywenbpljkhdcatbbdfmssrkzmcwnezelennjpfzbgmuktrhtejscktelgfpdlrkfyfwdajldejokbwf",
            "ur:bytes/6-9/lpamascfadaxcywenbpljkhdcackjlhkhybssklbwefectpfnbbectrljectpavyrolkzczcpkmwidmwoxkilghdsowp",
            "ur:bytes/7-9/lpatascfadaxcywenbpljkhdcavszmwnjkwtclrtvaynhpahrtoxmwvwatmedibkaegdosftvandiodagdhthtrlnnhy",
            "ur:bytes/8-9/lpayascfadaxcywenbpljkhdcadmsponkkbbhgsoltjntegepmttmoonftnbuoiyrehfrtsabzsttorodklubbuyaetk",
            "ur:bytes/9-9/lpasascfadaxcywenbpljkhdcajskecpmdckihdyhphfotjojtfmlnwmadspaxrkytbztpbauotbgtgtaeaevtgavtny",
            "ur:bytes/10-9/lpbkascfadaxcywenbpljkhdcahkadaemejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtwdkiplzs",
            "ur:bytes/11-9/lpbdascfadaxcywenbpljkhdcahelbknlkuejnbadmssfhfrdpsbiegecpasvssovlgeykssjykklronvsjkvetiiapk",
            "ur:bytes/12-9/lpbnascfadaxcywenbpljkhdcarllaluzmdmgstospeyiefmwejlwtpedamktksrvlcygmzemovovllarodtmtbnptrs",
            "ur:bytes/13-9/lpbtascfadaxcywenbpljkhdcamtkgtpknghchchyketwsvwgwfdhpgmgtylctotzopdrpayoschcmhplffziachrfgd",
            "ur:bytes/14-9/lpbaascfadaxcywenbpljkhdcapazewnvonnvdnsbyleynwtnsjkjndeoldydkbkdslgjkbbkortbelomueekgvstegt",
            "ur:bytes/15-9/lpbsascfadaxcywenbpljkhdcaynmhpddpzmversbdqdfyrehnqzlugmjzmnmtwmrouohtstgsbsahpawkditkckynwt",
            "ur:bytes/16-9/lpbeascfadaxcywenbpljkhdcawygekobamwtlihsnpalnsghenskkiynthdzotsimtojetprsttmukirlrsbtamjtpd",
            "ur:bytes/17-9/lpbyascfadaxcywenbpljkhdcamklgftaxykpewyrtqzhydntpnytyisincxmhtbceaykolduortotiaiaiafhiaoyce",
            "ur:bytes/18-9/lpbgascfadaxcywenbpljkhdcahkadaemejtswhhylkepmykhhtsytsnoyoyaxaedsuttydmmhhpktpmsrjtntwkbkwy",
            "ur:bytes/19-9/lpbwascfadaxcywenbpljkhdcadekicpaajootjzpsdrbalpeywllbdsnbinaerkurspbncxgslgftvtsrjtksplcpeo",
            "ur:bytes/20-9/lpbbascfadaxcywenbpljkhdcayapmrleeleaxpasfrtrdkncffwjyjzgyetdmlewtkpktgllepfrltataztksmhkbot"
        ];

        Assert.Equal(9, encoder.FragmentCount);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(i, encoder.CurrentSequence);
            var body = Bytewords.Encode(encoder.NextPart().ToCbor(), BytewordsStyle.Minimal);
            var part = $"ur:bytes/{i + 1}-{encoder.FragmentCount}/{body}";
            Assert.Equal(expected[i], part);
        }
    }

    [Fact]
    public void DecoderErrorCases()
    {
        // Invalid scheme
        Assert.Throws<URDecoderException>(() => UREncoding.Decode("uhr:bytes/aeadaolazmjendeoti"));

        // No type
        Assert.Throws<URDecoderException>(() => UREncoding.Decode("ur:aeadaolazmjendeoti"));

        // Invalid characters
        Assert.Throws<URDecoderException>(() => UREncoding.Decode("ur:bytes#4/aeadaolazmjendeoti"));

        // Invalid indices
        Assert.Throws<URDecoderException>(() => UREncoding.Decode("ur:bytes/1-1a/aeadaolazmjendeoti"));
        Assert.Throws<URDecoderException>(() => UREncoding.Decode("ur:bytes/1-1/toomuch/aeadaolazmjendeoti"));

        // Valid decodes
        UREncoding.Decode("ur:bytes/aeadaolazmjendeoti");
        UREncoding.Decode("ur:whatever-12/aeadaolazmjendeoti");
    }

    [Fact]
    public void CustomEncoder()
    {
        var data = System.Text.Encoding.UTF8.GetBytes("Ten chars!");
        var encoder = new FountainEncoder(data, 5);
        var part = encoder.NextPart();
        var body = Bytewords.Encode(part.ToCbor(), BytewordsStyle.Minimal);
        var result = $"ur:my-scheme/{part.SequenceId}/{body}";
        Assert.Equal("ur:my-scheme/1-2/lpadaobkcywkwmhfwnfeghihjtcxiansvomopr", result);
    }

    [Fact]
    public void MultipartUr()
    {
        var message = TestHelpers.MakeMessage("Wolf", 32767);
        var cbor = Cbor.ToByteString(message);
        var cborData = cbor.ToCborData();

        var encoder = new FountainEncoder(cborData, 1000);
        var decoder = new FountainDecoder();

        while (!decoder.IsComplete)
        {
            var part = encoder.NextPart();
            var body = Bytewords.Encode(part.ToCbor(), BytewordsStyle.Minimal);
            var urString = $"ur:bytes/{part.SequenceId}/{body}";

            // Decode and feed to fountain decoder
            var (kind, data) = UREncoding.Decode(urString);
            Assert.Equal(URKind.MultiPart, kind);
            var decodedPart = FountainPart.FromCbor(data);
            decoder.Receive(decodedPart);
        }

        Assert.Equal(cborData, decoder.Message());
    }
}

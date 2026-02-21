namespace BlockchainCommons.BCUR.Tests;

public class BytewordsTests
{
    [Fact]
    public void BytewordsEncodeDecode()
    {
        var input = new byte[] { 0, 1, 2, 128, 255 };

        Assert.Equal(
            "able acid also lava zoom jade need echo taxi",
            Bytewords.Encode(input, BytewordsStyle.Standard));
        Assert.Equal(
            "able-acid-also-lava-zoom-jade-need-echo-taxi",
            Bytewords.Encode(input, BytewordsStyle.Uri));
        Assert.Equal(
            "aeadaolazmjendeoti",
            Bytewords.Encode(input, BytewordsStyle.Minimal));

        Assert.Equal(input,
            Bytewords.Decode("able acid also lava zoom jade need echo taxi", BytewordsStyle.Standard));
        Assert.Equal(input,
            Bytewords.Decode("able-acid-also-lava-zoom-jade-need-echo-taxi", BytewordsStyle.Uri));
        Assert.Equal(input,
            Bytewords.Decode("aeadaolazmjendeoti", BytewordsStyle.Minimal));

        // Empty payload is allowed
        var emptyEncoded = Bytewords.Encode(Array.Empty<byte>(), BytewordsStyle.Minimal);
        Bytewords.Decode(emptyEncoded, BytewordsStyle.Minimal);

        // Bad checksum
        Assert.Throws<BytewordsException>(() =>
            Bytewords.Decode("able acid also lava zero jade need echo wolf", BytewordsStyle.Standard));
        Assert.Throws<BytewordsException>(() =>
            Bytewords.Decode("able-acid-also-lava-zero-jade-need-echo-wolf", BytewordsStyle.Uri));
        Assert.Throws<BytewordsException>(() =>
            Bytewords.Decode("aeadaolazojendeowf", BytewordsStyle.Minimal));

        // Too short
        Assert.Throws<BytewordsException>(() =>
            Bytewords.Decode("wolf", BytewordsStyle.Standard));
        Assert.Throws<BytewordsException>(() =>
            Bytewords.Decode("", BytewordsStyle.Standard));

        // Invalid length (minimal)
        Assert.Throws<BytewordsException>(() =>
            Bytewords.Decode("aea", BytewordsStyle.Minimal));

        // Non-ASCII
        Assert.Throws<BytewordsException>(() =>
            Bytewords.Decode("\u20bf", BytewordsStyle.Standard)); // ₿
        Assert.Throws<BytewordsException>(() =>
            Bytewords.Decode("\u20bf", BytewordsStyle.Uri));
        Assert.Throws<BytewordsException>(() =>
            Bytewords.Decode("\u20bf", BytewordsStyle.Minimal));
    }

    [Fact]
    public void BytewordsEncoding100Bytes()
    {
        byte[] input =
        [
            245, 215, 20, 198, 241, 235, 69, 59, 209, 205, 165, 18, 150, 158, 116, 135, 229, 212,
            19, 159, 17, 37, 239, 240, 253, 11, 109, 191, 37, 242, 38, 120, 223, 41, 156, 189, 242,
            254, 147, 204, 66, 163, 216, 175, 191, 72, 169, 54, 32, 60, 144, 230, 210, 137, 184,
            197, 33, 113, 88, 14, 157, 31, 177, 46, 1, 115, 205, 69, 225, 150, 65, 235, 58, 144,
            65, 240, 133, 69, 113, 247, 63, 53, 242, 165, 160, 144, 26, 13, 79, 237, 133, 71, 82,
            69, 254, 165, 138, 41, 85, 24
        ];

        var encoded = "yank toys bulb skew when warm free fair tent swan " +
                      "open brag mint noon jury list view tiny brew note " +
                      "body data webs what zinc bald join runs data whiz " +
                      "days keys user diet news ruby whiz zone menu surf " +
                      "flew omit trip pose runs fund part even crux fern " +
                      "math visa tied loud redo silk curl jugs hard beta " +
                      "next cost puma drum acid junk swan free very mint " +
                      "flap warm fact math flap what limp free jugs yell " +
                      "fish epic whiz open numb math city belt glow wave " +
                      "limp fuel grim free zone open love diet gyro cats " +
                      "fizz holy city puff";

        var encodedMinimal = "yktsbbswwnwmfefrttsnonbgmtnnjyltvwtybwne" +
                             "bydawswtzcbdjnrsdawzdsksurdtnsrywzzemusf" +
                             "fwottppersfdptencxfnmhvatdldroskcljshdba" +
                             "ntctpadmadjksnfevymtfpwmftmhfpwtlpfejsyl" +
                             "fhecwzonnbmhcybtgwwelpflgmfezeonledtgocs" +
                             "fzhycypf";

        Assert.Equal(input, Bytewords.Decode(encoded, BytewordsStyle.Standard));
        Assert.Equal(input, Bytewords.Decode(encodedMinimal, BytewordsStyle.Minimal));
        Assert.Equal(encoded, Bytewords.Encode(input, BytewordsStyle.Standard));
        Assert.Equal(encodedMinimal, Bytewords.Encode(input, BytewordsStyle.Minimal));
    }

    [Fact]
    public void BytemojiUniqueness()
    {
        var seen = new HashSet<string>();
        var duplicates = new List<string>();
        foreach (var emoji in Bytewords.Bytemojis)
        {
            if (!seen.Add(emoji))
            {
                duplicates.Add(emoji);
            }
        }
        Assert.Empty(duplicates);
    }

    [Fact]
    public void BytemojiLengths()
    {
        var overLength = Bytewords.Bytemojis.Where(e => System.Text.Encoding.UTF8.GetByteCount(e) > 4).ToList();
        Assert.Empty(overLength);
    }

    [Fact]
    public void BytewordsTablesAreReadOnly()
    {
        var words = Assert.IsAssignableFrom<IList<string>>(Bytewords.Words);
        var bytemojis = Assert.IsAssignableFrom<IList<string>>(Bytewords.Bytemojis);

        Assert.True(words.IsReadOnly);
        Assert.True(bytemojis.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => words[0] = "test");
        Assert.Throws<NotSupportedException>(() => bytemojis[0] = "test");
    }
}

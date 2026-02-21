using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCUR.Tests;

public class ExampleTests
{
    [Fact]
    public void ExampleEncode()
    {
        var cbor = Cbor.FromIntList([1, 2, 3]);
        var ur = UR.Create("test", cbor);
        var urString = ur.ToUrString();
        Assert.Equal("ur:test/lsadaoaxjygonesw", urString);
    }

    [Fact]
    public void ExampleDecode()
    {
        var urString = "ur:test/lsadaoaxjygonesw";
        var ur = UR.FromUrString(urString);
        Assert.Equal("test", ur.UrTypeStr);
        var arrayCbor = Cbor.FromIntList([1, 2, 3]);
        Assert.Equal(arrayCbor, ur.Cbor);
    }

    [Fact]
    public void ExampleFountain()
    {
        int RunFountainTest(int startPart)
        {
            var message = "The only thing we have to fear is fear itself.";
            var cbor = Cbor.ToByteString(System.Text.Encoding.UTF8.GetBytes(message));
            var ur = UR.Create("bytes", cbor);

            var encoder = new MultipartEncoder(ur, 10);
            var decoder = new MultipartDecoder();
            for (int i = 0; i < 1000; i++)
            {
                var part = encoder.NextPart();
                if (encoder.CurrentIndex >= startPart)
                {
                    decoder.Receive(part);
                }
                if (decoder.IsComplete)
                {
                    break;
                }
            }
            var receivedUr = decoder.Message()!;
            Assert.Equal(ur, receivedUr);
            return encoder.CurrentIndex;
        }

        Assert.Equal(5, RunFountainTest(1));
        Assert.Equal(61, RunFountainTest(51));
        Assert.Equal(110, RunFountainTest(101));
        Assert.Equal(507, RunFountainTest(501));
    }
}

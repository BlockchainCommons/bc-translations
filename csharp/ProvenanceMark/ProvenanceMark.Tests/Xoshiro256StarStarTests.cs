using System.Text;

namespace BlockchainCommons.ProvenanceMark.Tests;

public sealed class Xoshiro256StarStarTests
{
    [Fact]
    public void TestRng()
    {
        var digest = CryptoUtils.Sha256(Encoding.UTF8.GetBytes("Hello World"));
        var rng = Xoshiro256StarStar.FromData(digest);
        var key = rng.NextBytes(32);
        Assert.Equal("b18b446df414ec00714f19cb0f03e45cd3c3d5d071d2e7483ba8627c65b9926a", Util.ToHex(key));
    }

    [Fact]
    public void TestSaveRngState()
    {
        ulong[] state =
        [
            17295166580085024720UL,
            422929670265678780UL,
            5577237070365765850UL,
            7953171132032326923UL
        ];
        var data = Xoshiro256StarStar.FromState(state).ToData();
        Assert.Equal("d0e72cf15ec604f0bcab28594b8cde05dab04ae79053664d0b9dadc201575f6e", Util.ToHex(data));

        var state2 = Xoshiro256StarStar.FromData(data).ToState();
        var data2 = Xoshiro256StarStar.FromState(state2).ToData();
        Assert.Equal(data, data2);
    }
}

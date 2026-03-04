using System.Text;
using BlockchainCommons.BCComponents;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class HKDFRngTests
{
    [Fact]
    public void TestHkdfRngNextBytes()
    {
        using var rng = new HKDFRng(Encoding.UTF8.GetBytes("key_material"), "salt");

        var page1 = rng.RandomData(16);
        Assert.Equal("1032ac8ffea232a27c79fe381d7eb7e4",
            Convert.ToHexString(page1).ToLowerInvariant());

        var page2 = rng.RandomData(16);
        Assert.Equal("aeaaf727d35b6f338218391f9f8fa1f3",
            Convert.ToHexString(page2).ToLowerInvariant());

        var page3 = rng.RandomData(16);
        Assert.Equal("4348a59427711deb1e7d8a6959c6adb4",
            Convert.ToHexString(page3).ToLowerInvariant());

        var page4 = rng.RandomData(16);
        Assert.Equal("5d937a42cb5fb090fe1a1ec88f56e32b",
            Convert.ToHexString(page4).ToLowerInvariant());
    }

    [Fact]
    public void TestHkdfRngNextU32()
    {
        using var rng = new HKDFRng(Encoding.UTF8.GetBytes("key_material"), "salt");
        var v = rng.NextUInt32();
        Assert.Equal(2410426896u, v);
    }

    [Fact]
    public void TestHkdfRngNextU64()
    {
        using var rng = new HKDFRng(Encoding.UTF8.GetBytes("key_material"), "salt");
        var v = rng.NextUInt64();
        Assert.Equal(11687583197195678224UL, v);
    }

    [Fact]
    public void TestHkdfRngFillBytes()
    {
        using var rng = new HKDFRng(Encoding.UTF8.GetBytes("key_material"), "salt");
        var dest = new byte[16];
        rng.FillRandomData(dest);
        Assert.Equal("1032ac8ffea232a27c79fe381d7eb7e4",
            Convert.ToHexString(dest).ToLowerInvariant());
    }
}

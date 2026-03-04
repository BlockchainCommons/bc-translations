using System.Text;
using BlockchainCommons.BCComponents;
using BlockchainCommons.BCUR;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class DigestTests
{
    [Fact]
    public void TestDigest()
    {
        var data = Encoding.UTF8.GetBytes("hello world");
        var digest = Digest.FromImage(data);
        Assert.Equal(Digest.Size, digest.Data.Length);
        Assert.Equal(
            "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9",
            digest.Hex);
    }

    [Fact]
    public void TestDigestFromHex()
    {
        var hex = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9";
        var digest = Digest.FromHex(hex);
        Assert.Equal(Digest.Size, digest.Data.Length);
        Assert.Equal(hex, digest.Hex);
        Assert.Equal(Digest.FromImage(Encoding.UTF8.GetBytes("hello world")), digest);
    }

    [Fact]
    public void TestUrRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var data = Encoding.UTF8.GetBytes("hello world");
        var digest = Digest.FromImage(data);
        var urString = digest.ToURString();
        var expected = "ur:digest/hdcxrhgtdirhmugtfmayondmgmtstnkipyzssslrwsvlkngulawymhloylpsvowssnwlamnlatrs";
        Assert.Equal(expected, urString);
        var digest2 = URDecodableExtensions.FromURString<Digest>(urString, Digest.FromUntaggedCbor);
        Assert.Equal(digest, digest2);
    }

    [Fact]
    public void TestDigestEquality()
    {
        var hex = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9";
        var digest1 = Digest.FromHex(hex);
        var digest2 = Digest.FromHex(hex);
        Assert.Equal(digest1, digest2);
    }

    [Fact]
    public void TestDigestInequality()
    {
        var digest1 = Digest.FromHex(
            "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9");
        var digest2 = Digest.FromHex(
            "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855");
        Assert.NotEqual(digest1, digest2);
    }

    [Fact]
    public void TestInvalidHexString()
    {
        Assert.ThrowsAny<Exception>(() => Digest.FromHex("invalid_hex_string"));
    }

    [Fact]
    public void TestValidate()
    {
        var data = Encoding.UTF8.GetBytes("hello world");
        var digest = Digest.FromImage(data);
        Assert.True(digest.Validate(data));
    }

    [Fact]
    public void TestShortDescription()
    {
        var digest = Digest.FromImage(Encoding.UTF8.GetBytes("hello world"));
        Assert.Equal("b94d27b9", digest.ShortDescription());
    }
}

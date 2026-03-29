using System.Text;

namespace BlockchainCommons.ProvenanceMark.Tests;

public sealed class CryptoUtilsTests
{
    [Fact]
    public void TestSha256()
    {
        var digest = CryptoUtils.Sha256(Encoding.UTF8.GetBytes("Hello World"));
        Assert.Equal("a591a6d40bf420404a011733cfb7b190d62c65bf0bcda32b57b277d9ad9f146e", Util.ToHex(digest));
    }

    [Fact]
    public void TestExtendKey()
    {
        var key = CryptoUtils.ExtendKey(Encoding.UTF8.GetBytes("Hello World"));
        Assert.Equal("813085a508d5fec645abe5a1fb9a23c2a6ac6bef0a99650017b3ef50538dba39", Util.ToHex(key));
    }

    [Fact]
    public void TestObfuscate()
    {
        var key = Encoding.UTF8.GetBytes("Hello");
        var message = Encoding.UTF8.GetBytes("World");
        var obfuscated = CryptoUtils.Obfuscate(key, message);
        Assert.Equal("c43889aafa", Util.ToHex(obfuscated));

        var deobfuscated = CryptoUtils.Obfuscate(key, obfuscated);
        Assert.Equal(message, deobfuscated);
    }
}

using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class NonceTests
{
    [Fact]
    public void TestNonceRaw()
    {
        var nonceRaw = new byte[Nonce.Size];
        var nonce = Nonce.FromData(nonceRaw);
        Assert.Equal(nonceRaw, nonce.AsBytes().ToArray());
    }

    [Fact]
    public void TestNonceFromDataChecked()
    {
        var rawData = new byte[Nonce.Size];
        var nonce = Nonce.FromData(rawData);
        Assert.Equal(rawData, nonce.AsBytes().ToArray());
    }

    [Fact]
    public void TestNonceInvalidSize()
    {
        var rawData = new byte[Nonce.Size + 1];
        Assert.Throws<BCComponentsException>(() => Nonce.FromData(rawData));
    }

    [Fact]
    public void TestNonceNew()
    {
        var nonce1 = Nonce.New();
        var nonce2 = Nonce.New();
        Assert.NotEqual(nonce1, nonce2);
    }

    [Fact]
    public void TestNonceHexRoundtrip()
    {
        var nonce = Nonce.New();
        var hexString = nonce.Hex;
        var nonceFromHex = Nonce.FromHex(hexString);
        Assert.Equal(nonce, nonceFromHex);
    }

    [Fact]
    public void TestNonceCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var nonce = Nonce.New();
        var cbor = nonce.TaggedCbor();
        var decoded = Nonce.FromTaggedCbor(cbor);
        Assert.Equal(nonce, decoded);
    }
}

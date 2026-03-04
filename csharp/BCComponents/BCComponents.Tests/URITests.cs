using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class URITests
{
    [Fact]
    public void TestCreation()
    {
        var uri = URI.FromString("https://example.com");
        Assert.Equal("https://example.com", uri.Value);
        Assert.Equal("https://example.com", uri.ToString());
    }

    [Fact]
    public void TestInvalidUri()
    {
        Assert.Throws<BCComponentsException>(() => URI.FromString("not a valid uri"));
    }

    [Fact]
    public void TestNoScheme()
    {
        Assert.Throws<BCComponentsException>(() => URI.FromString("example.com"));
    }

    [Fact]
    public void TestCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var uri = URI.FromString("https://example.com/path?query=value");
        var cbor = uri.TaggedCbor();
        var decoded = URI.FromTaggedCbor(cbor);
        Assert.Equal(uri, decoded);
    }

    [Fact]
    public void TestEquality()
    {
        var uri1 = URI.FromString("https://example.com");
        var uri2 = URI.FromString("https://example.com");
        Assert.Equal(uri1, uri2);
    }

    [Fact]
    public void TestVariousSchemes()
    {
        var httpUri = URI.FromString("http://example.com");
        Assert.Equal("http://example.com", httpUri.ToString());

        var ftpUri = URI.FromString("ftp://files.example.com");
        Assert.Equal("ftp://files.example.com", ftpUri.ToString());

        var mailtoUri = URI.FromString("mailto:user@example.com");
        Assert.Equal("mailto:user@example.com", mailtoUri.ToString());
    }
}

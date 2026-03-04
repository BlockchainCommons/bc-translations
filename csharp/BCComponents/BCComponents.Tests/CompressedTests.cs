using System.Text;
using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class CompressedTests
{
    [Fact]
    public void TestCompressLargeData()
    {
        var source = Encoding.UTF8.GetBytes(
            "Lorem ipsum dolor sit amet consectetur adipiscing elit mi nibh ornare proin blandit diam ridiculus, faucibus mus dui eu vehicula nam donec dictumst sed vivamus bibendum aliquet efficitur. Felis imperdiet sodales dictum morbi vivamus augue dis duis aliquet velit ullamcorper porttitor, lobortis dapibus hac purus aliquam natoque iaculis blandit montes nunc pretium.");
        var compressed = Compressed.FromDecompressedData(source);
        Assert.Equal(364, compressed.DecompressedSize);
        Assert.True(compressed.CompressedSize < compressed.DecompressedSize);
        Assert.Equal(source, compressed.Decompress());
    }

    [Fact]
    public void TestCompressMediumData()
    {
        var source = Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet consectetur adipiscing");
        var compressed = Compressed.FromDecompressedData(source);
        Assert.Equal(49, compressed.DecompressedSize);
        Assert.True(compressed.CompressedSize <= compressed.DecompressedSize);
        Assert.Equal(source, compressed.Decompress());
    }

    [Fact]
    public void TestCompressSmallData()
    {
        var source = Encoding.UTF8.GetBytes("Lorem");
        var compressed = Compressed.FromDecompressedData(source);
        Assert.Equal(5, compressed.DecompressedSize);
        Assert.Equal(5, compressed.CompressedSize);
        Assert.Equal(1.0, compressed.CompressionRatio, 2);
        Assert.Equal(source, compressed.Decompress());
    }

    [Fact]
    public void TestCompressEmptyData()
    {
        var source = Array.Empty<byte>();
        var compressed = Compressed.FromDecompressedData(source);
        Assert.Equal(0, compressed.DecompressedSize);
        Assert.Equal(0, compressed.CompressedSize);
        Assert.True(double.IsNaN(compressed.CompressionRatio));
        Assert.Equal(source, compressed.Decompress());
    }

    [Fact]
    public void TestCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var source = Encoding.UTF8.GetBytes(
            "Lorem ipsum dolor sit amet consectetur adipiscing elit mi nibh ornare proin blandit diam ridiculus, faucibus mus dui eu vehicula nam donec dictumst sed vivamus bibendum aliquet efficitur.");
        var compressed = Compressed.FromDecompressedData(source);
        var cbor = compressed.TaggedCbor();
        var decoded = Compressed.FromTaggedCbor(cbor);
        Assert.Equal(compressed, decoded);
        Assert.Equal(source, decoded.Decompress());
    }

    [Fact]
    public void TestDigest()
    {
        var source = Encoding.UTF8.GetBytes("Hello world!");
        var digest = Digest.FromImage(source);
        var compressed = Compressed.FromDecompressedData(source, digest);
        Assert.True(compressed.HasDigest);
        Assert.Equal(digest, compressed.Digest);
    }

    [Fact]
    public void TestNoDigest()
    {
        var source = Encoding.UTF8.GetBytes("Hello");
        var compressed = Compressed.FromDecompressedData(source);
        Assert.False(compressed.HasDigest);
        Assert.Null(compressed.Digest);
    }
}

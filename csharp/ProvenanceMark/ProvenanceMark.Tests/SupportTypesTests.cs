using System.Text;

namespace BlockchainCommons.ProvenanceMark.Tests;

public sealed class SupportTypesTests
{
    [Fact]
    public void ProvenanceSeedRoundTrips()
    {
        var seed = ProvenanceSeed.CreateWithPassphrase("test");

        Assert.Equal(seed, ProvenanceSeed.FromBase64(seed.ToBase64()));
        Assert.Equal(seed, ProvenanceSeed.FromJson(seed.ToJson()));
        Assert.Equal(seed, ProvenanceSeed.FromCbor(seed.ToCbor()));
        Assert.Equal(64, seed.Hex.Length);
    }

    [Fact]
    public void RngStateRoundTrips()
    {
        var bytes = CryptoUtils.Sha256(Encoding.UTF8.GetBytes("Hello World"));
        var state = RngState.FromBytes(bytes);

        Assert.Equal(state, RngState.FromBase64(state.ToBase64()));
        Assert.Equal(state, RngState.FromJson(state.ToJson()));
        Assert.Equal(state, RngState.FromCbor(state.ToCbor()));
        Assert.Equal(64, state.Hex.Length);
    }

    [Fact]
    public void GeneratorJsonRoundTrips()
    {
        var generator = ProvenanceMarkGenerator.CreateWithPassphrase(ProvenanceMarkResolution.High, "test");
        Assert.Equal(generator, ProvenanceMarkGenerator.FromJson(generator.ToJson()));
    }

    [Fact]
    public void ProvenanceMarkInfoRoundTripsAndRendersMarkdown()
    {
        ProvenanceMark.RegisterTags();

        var generator = ProvenanceMarkGenerator.CreateWithPassphrase(ProvenanceMarkResolution.Low, "test");
        var mark = generator.Next(TestSupport.BaseDate(0), "Info field content");
        var info = ProvenanceMarkInfo.Create(mark, "Genesis mark.");

        var markdown = info.MarkdownSummary();
        Assert.Contains(mark.Date.ToString(), markdown, StringComparison.Ordinal);
        Assert.Contains(info.Ur.ToString(), markdown, StringComparison.Ordinal);
        Assert.Contains(info.Bytewords, markdown, StringComparison.Ordinal);
        Assert.Contains(info.Bytemoji, markdown, StringComparison.Ordinal);
        Assert.Contains("Genesis mark.", markdown, StringComparison.Ordinal);

        var decoded = ProvenanceMarkInfo.FromJson(info.ToJson());
        Assert.Equal(info.Mark, decoded.Mark);
        Assert.Equal(info.Ur, decoded.Ur);
        Assert.Equal(info.Bytewords, decoded.Bytewords);
        Assert.Equal(info.Bytemoji, decoded.Bytemoji);
        Assert.Equal(info.Comment, decoded.Comment);
    }

    [Fact]
    public void HashMismatchIssueUsesValueEquality()
    {
        var left = new HashMismatchIssue(TestSupport.Hex("00112233"), TestSupport.Hex("44556677"));
        var right = new HashMismatchIssue(TestSupport.Hex("00112233"), TestSupport.Hex("44556677"));

        Assert.Equal(left, right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }
}

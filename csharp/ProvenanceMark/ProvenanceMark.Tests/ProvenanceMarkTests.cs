using System.Text.Json.Nodes;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.ProvenanceMark.Tests;

public sealed class ProvenanceMarkTests
{
    private readonly JsonNode _vectors = TestSupport.LoadJsonResource("mark_vectors.json");

    [Fact]
    public void TestLow() => RunVector("test_low");

    [Fact]
    public void TestLowWithInfo() => RunVector("test_low_with_info");

    [Fact]
    public void TestMedium() => RunVector("test_medium");

    [Fact]
    public void TestMediumWithInfo() => RunVector("test_medium_with_info");

    [Fact]
    public void TestQuartile() => RunVector("test_quartile");

    [Fact]
    public void TestQuartileWithInfo() => RunVector("test_quartile_with_info");

    [Fact]
    public void TestHigh() => RunVector("test_high");

    [Fact]
    public void TestHighWithInfo() => RunVector("test_high_with_info");

    [Fact]
    public void TestReadmeDeps()
    {
        var manifestPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../MANIFEST.md"));
        var manifest = File.ReadAllText(manifestPath);
        Assert.Contains("0.24.0", manifest, StringComparison.Ordinal);
        Assert.Contains("bc-rand ^0.5.0", manifest, StringComparison.Ordinal);
        Assert.Contains("bc-envelope ^0.43.0", manifest, StringComparison.Ordinal);
    }

    [Fact]
    public void TestHtmlRootUrl()
    {
        var sourcePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../ProvenanceMark/ProvenanceMark.cs"));
        var source = File.ReadAllText(sourcePath);
        Assert.Contains("namespace BlockchainCommons.ProvenanceMark;", source, StringComparison.Ordinal);
    }

    [Fact]
    public void TestEnvelope()
    {
        ProvenanceMark.RegisterTags();

        var seed = ProvenanceSeed.CreateWithPassphrase("test");
        var date = CborDate.FromString("2025-10-26");
        var generator = ProvenanceMarkGenerator.CreateWithSeed(ProvenanceMarkResolution.High, seed);
        var mark = generator.Next(date, "Info field content");

        var generatorEnvelope = generator.ToEnvelope();
        var decodedGenerator = ProvenanceMarkGenerator.FromEnvelope(generatorEnvelope);
        Assert.Equal(generator, decodedGenerator);

        var markEnvelope = mark.ToEnvelope();
        Assert.Equal(mark.ToString(), markEnvelope.Format());
        Assert.Equal(mark, ProvenanceMark.FromEnvelope(markEnvelope));
        Assert.Contains("Info field content", mark.DebugString(), StringComparison.Ordinal);
    }

    private void RunVector(string name)
    {
        var vector = _vectors[name] ?? throw new InvalidOperationException($"missing vector: {name}");
        var resolution = TestSupport.ResolutionFromString(vector["resolution"]!.GetValue<string>());
        var includeInfo = vector["include_info"]!.GetValue<bool>();
        RunTest(
            resolution,
            includeInfo,
            StringList(vector, "expected_debug"),
            StringList(vector, "expected_bytewords"),
            StringList(vector, "expected_id_words"),
            StringList(vector, "expected_bytemoji_ids"),
            StringList(vector, "expected_urs"),
            StringList(vector, "expected_urls"));
    }

    private static void RunTest(
        ProvenanceMarkResolution resolution,
        bool includeInfo,
        IReadOnlyList<string> expectedDebug,
        IReadOnlyList<string> expectedBytewords,
        IReadOnlyList<string> expectedIdWords,
        IReadOnlyList<string> expectedBytemojiIds,
        IReadOnlyList<string> expectedUrs,
        IReadOnlyList<string> expectedUrls)
    {
        ProvenanceMark.RegisterTags();

        const int count = 10;
        var encodedGenerator = ProvenanceMarkGenerator.CreateWithPassphrase(resolution, "Wolf").ToJson();
        var marks = new List<ProvenanceMark>(count);

        for (var index = 0; index < count; index++)
        {
            var generator = ProvenanceMarkGenerator.FromJson(encodedGenerator);
            var info = includeInfo ? "Lorem ipsum sit dolor amet." : null;
            var mark = generator.Next(TestSupport.BaseDate(index), info);
            encodedGenerator = generator.ToJson();
            marks.Add(mark);
        }

        Assert.True(ProvenanceMark.IsSequenceValid(marks));
        Assert.False(marks[1].Precedes(marks[0]));
        Assert.All(marks, mark => Assert.StartsWith("ProvenanceMark(", mark.ToString(), StringComparison.Ordinal));

        Assert.Equal(expectedDebug, marks.Select(mark => mark.DebugString()).ToList());

        var bytewords = marks.Select(mark => mark.ToBytewords()).ToList();
        Assert.Equal(expectedBytewords, bytewords);
        Assert.Equal(marks, bytewords.Select(value => ProvenanceMark.FromBytewords(resolution, value)).ToList());

        var idWords = marks.Select(mark => mark.IdBytewords(4, false)).ToList();
        Assert.Equal(expectedIdWords, idWords);

        var bytemojiIds = marks.Select(mark => mark.IdBytemoji(4, false)).ToList();
        Assert.Equal(expectedBytemojiIds, bytemojiIds);

        var urs = marks.Select(mark => mark.ToUrString()).ToList();
        Assert.Equal(expectedUrs, urs);
        Assert.Equal(marks, urs.Select(ProvenanceMark.FromUrString).ToList());

        const string baseUrl = "https://example.com/validate";
        var urls = marks.Select(mark => mark.ToUrl(baseUrl).ToString()).ToList();
        Assert.Equal(expectedUrls, urls);
        Assert.Equal(marks, urls.Select(ProvenanceMark.FromUrl).ToList());

        foreach (var mark in marks)
        {
            Assert.Equal(mark, ProvenanceMark.FromJson(mark.ToJson()));
        }
    }

    private static IReadOnlyList<string> StringList(JsonNode node, string field)
    {
        return node[field]!.AsArray().Select(item => item!.GetValue<string>()).ToList();
    }
}

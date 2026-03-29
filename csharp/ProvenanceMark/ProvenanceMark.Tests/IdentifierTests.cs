namespace BlockchainCommons.ProvenanceMark.Tests;

public sealed class IdentifierTests
{
    [Fact]
    public void TestIdReturns32Bytes()
    {
        foreach (var resolution in ProvenanceMarkResolution.All)
        {
            var marks = TestSupport.CreateSerializedGeneratorMarks(3, resolution, "Wolf");
            foreach (var mark in marks)
            {
                Assert.Equal(32, mark.Id().Length);
            }
        }
    }

    [Fact]
    public void TestIdPreservesHashPrefix()
    {
        foreach (var resolution in ProvenanceMarkResolution.All)
        {
            var marks = TestSupport.CreateSerializedGeneratorMarks(3, resolution, "Wolf");
            foreach (var mark in marks)
            {
                var id = mark.Id();
                var hash = mark.Hash;
                Assert.Equal(hash, id[..hash.Length]);
            }
        }
    }

    [Fact]
    public void TestIdHexIs64Chars()
    {
        var marks = TestSupport.CreateSerializedGeneratorMarks(5, ProvenanceMarkResolution.Low, "Wolf");
        foreach (var mark in marks)
        {
            Assert.Equal(64, mark.IdHex().Length);
        }
    }

    [Fact]
    public void TestIdHexEncodesFullId()
    {
        var mark = TestSupport.CreateSerializedGeneratorMarks(1, ProvenanceMarkResolution.Low, "Wolf")[0];
        Assert.Equal(Util.ToHex(mark.Id()), mark.IdHex());
    }

    [Fact]
    public void TestIdBytewordsWordCount()
    {
        var mark = TestSupport.CreateSerializedGeneratorMarks(3, ProvenanceMarkResolution.Low, "Wolf")[0];
        for (var count = 4; count <= 32; count++)
        {
            Assert.Equal(count, mark.IdBytewords(count, false).Split(' ').Length);
        }
    }

    [Fact]
    public void TestIdBytewordsPrefixExtendsShorter()
    {
        var mark = TestSupport.CreateSerializedGeneratorMarks(1, ProvenanceMarkResolution.Low, "Wolf")[0];
        var shortId = mark.IdBytewords(4, false);
        var longId = mark.IdBytewords(8, false);
        Assert.StartsWith(shortId, longId);
    }

    [Fact]
    public void TestIdBytewordsWithPrefixFlag()
    {
        var mark = TestSupport.CreateSerializedGeneratorMarks(1, ProvenanceMarkResolution.Low, "Wolf")[0];
        var withoutPrefix = mark.IdBytewords(4, false);
        var withPrefix = mark.IdBytewords(4, true);
        Assert.StartsWith("🅟 ", withPrefix);
        Assert.Equal(withoutPrefix, withPrefix[3..]);
    }

    [Fact]
    public void TestIdBytemojiWordCount()
    {
        var mark = TestSupport.CreateSerializedGeneratorMarks(1, ProvenanceMarkResolution.Low, "Wolf")[0];
        for (var count = 4; count <= 32; count++)
        {
            Assert.Equal(count, mark.IdBytemoji(count, false).Split(' ').Length);
        }
    }

    [Fact]
    public void TestIdBytewordsMinimalLength()
    {
        var mark = TestSupport.CreateSerializedGeneratorMarks(1, ProvenanceMarkResolution.Low, "Wolf")[0];
        for (var count = 4; count <= 32; count++)
        {
            Assert.Equal(count * 2, mark.IdBytewordsMinimal(count, false).Length);
        }
    }

    [Fact]
    public void TestIdBytewordsMinimalIsUppercase()
    {
        var mark = TestSupport.CreateSerializedGeneratorMarks(1, ProvenanceMarkResolution.Low, "Wolf")[0];
        var minimal = mark.IdBytewordsMinimal(4, false);
        Assert.Equal(minimal.ToUpperInvariant(), minimal);
    }

    [Fact]
    public void TestIdBytewordsMinimalExtendsShorter()
    {
        var mark = TestSupport.CreateSerializedGeneratorMarks(1, ProvenanceMarkResolution.Low, "Wolf")[0];
        var shortId = mark.IdBytewordsMinimal(4, false);
        var longId = mark.IdBytewordsMinimal(8, false);
        Assert.StartsWith(shortId, longId);
    }

    [Fact]
    public void TestIdBytewordsThrowsBelow4()
    {
        var mark = TestSupport.CreateSerializedGeneratorMarks(1, ProvenanceMarkResolution.Low, "Wolf")[0];
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => mark.IdBytewords(3, false));
        Assert.Contains("word_count must be 4..=32", error.Message);
    }

    [Fact]
    public void TestIdBytewordsThrowsAbove32()
    {
        var mark = TestSupport.CreateSerializedGeneratorMarks(1, ProvenanceMarkResolution.Low, "Wolf")[0];
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => mark.IdBytewords(33, false));
        Assert.Contains("word_count must be 4..=32", error.Message);
    }

    [Fact]
    public void TestIdBytemojiThrowsAbove32()
    {
        var mark = TestSupport.CreateSerializedGeneratorMarks(1, ProvenanceMarkResolution.Low, "Wolf")[0];
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => mark.IdBytemoji(33, false));
        Assert.Contains("word_count must be 4..=32", error.Message);
    }

    [Fact]
    public void TestIdBytewordsMinimalThrowsBelow4()
    {
        var mark = TestSupport.CreateSerializedGeneratorMarks(1, ProvenanceMarkResolution.Low, "Wolf")[0];
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => mark.IdBytewordsMinimal(3, false));
        Assert.Contains("word_count must be 4..=32", error.Message);
    }

    [Fact]
    public void TestDisambiguatedNoCollisions()
    {
        var marks = TestSupport.CreateSerializedGeneratorMarks(5, ProvenanceMarkResolution.Low, "Wolf");
        var ids = ProvenanceMark.DisambiguatedIdBytewords(marks, false);
        Assert.Equal(5, ids.Count);
        foreach (var id in ids)
        {
            Assert.Equal(4, id.Split(' ').Length);
        }
    }

    [Fact]
    public void TestDisambiguatedEmpty()
    {
        Assert.Empty(ProvenanceMark.DisambiguatedIdBytewords(Array.Empty<ProvenanceMark>(), false));
    }

    [Fact]
    public void TestDisambiguatedSingleMark()
    {
        var ids = ProvenanceMark.DisambiguatedIdBytewords(
            TestSupport.CreateSerializedGeneratorMarks(1, ProvenanceMarkResolution.Low, "Wolf"),
            false);
        Assert.Single(ids);
        Assert.Equal(4, ids[0].Split(' ').Length);
    }

    [Fact]
    public void TestDisambiguatedSelectiveExtension()
    {
        var marks = TestSupport.CreateSerializedGeneratorMarks(5, ProvenanceMarkResolution.Low, "Wolf");
        var ids = ProvenanceMark.DisambiguatedIdBytewords(marks, false);
        Assert.All(ids, id => Assert.Equal(4, id.Split(' ').Length));

        ProvenanceMark[] withDuplicate =
        [
            marks[0],
            marks[1],
            marks[2],
            marks[0]
        ];
        ids = ProvenanceMark.DisambiguatedIdBytewords(withDuplicate, false).ToList();

        Assert.Equal(4, ids.Count);
        Assert.Equal(4, ids[1].Split(' ').Length);
        Assert.Equal(4, ids[2].Split(' ').Length);
        Assert.Equal(32, ids[0].Split(' ').Length);
        Assert.Equal(32, ids[3].Split(' ').Length);
        Assert.Equal(ids[0], ids[3]);
    }

    [Fact]
    public void TestDisambiguatedAllResultsUniqueExceptIdentical()
    {
        var marks = TestSupport.CreateSerializedGeneratorMarks(10, ProvenanceMarkResolution.Low, "Wolf");
        var ids = ProvenanceMark.DisambiguatedIdBytewords(marks, false);
        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void TestDisambiguatedBytemojiSamePrefixLengths()
    {
        var marks = TestSupport.CreateSerializedGeneratorMarks(3, ProvenanceMarkResolution.Low, "Wolf");
        ProvenanceMark[] withDuplicate = [marks[0], marks[1], marks[0]];
        var wordIds = ProvenanceMark.DisambiguatedIdBytewords(withDuplicate, false);
        var emojiIds = ProvenanceMark.DisambiguatedIdBytemoji(withDuplicate, false);
        Assert.Equal(wordIds.Count, emojiIds.Count);
        for (var index = 0; index < wordIds.Count; index++)
        {
            Assert.Equal(wordIds[index].Split(' ').Length, emojiIds[index].Split(' ').Length);
        }
    }

    [Fact]
    public void TestDisambiguatedWithPrefix()
    {
        var marks = TestSupport.CreateSerializedGeneratorMarks(3, ProvenanceMarkResolution.Low, "Wolf");
        var idsWithoutPrefix = ProvenanceMark.DisambiguatedIdBytewords(marks, false);
        var idsWithPrefix = ProvenanceMark.DisambiguatedIdBytewords(marks, true);

        for (var index = 0; index < idsWithoutPrefix.Count; index++)
        {
            Assert.StartsWith("🅟 ", idsWithPrefix[index]);
            Assert.Equal(idsWithoutPrefix[index], idsWithPrefix[index][3..]);
        }
    }
}

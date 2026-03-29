using System.Text.Json.Nodes;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.ProvenanceMark.Tests;

public sealed class ValidateTests
{
    private readonly JsonNode _expected = TestSupport.LoadJsonResource("validate_expected.json");

    [Fact]
    public void TestValidateEmpty()
    {
        AssertExpectedFormats("test_validate_empty", ProvenanceMark.Validate(Array.Empty<ProvenanceMark>()));
    }

    [Fact]
    public void TestValidateSingleMark()
    {
        AssertExpectedFormats("test_validate_single_mark", ProvenanceMark.Validate(TestSupport.CreateTestMarks(1, ProvenanceMarkResolution.Low, "test")));
    }

    [Fact]
    public void TestValidateValidSequence()
    {
        AssertExpectedFormats("test_validate_valid_sequence", ProvenanceMark.Validate(TestSupport.CreateTestMarks(5, ProvenanceMarkResolution.Low, "test")));
    }

    [Fact]
    public void TestValidateDeduplication()
    {
        var marks = TestSupport.CreateTestMarks(3, ProvenanceMarkResolution.Low, "test");
        var withDuplicates = new[] { marks[0], marks[1], marks[2], marks[0], marks[1], marks[0] };
        AssertExpectedFormats("test_validate_deduplication", ProvenanceMark.Validate(withDuplicates));
    }

    [Fact]
    public void TestValidateMultipleChains()
    {
        var marks1 = TestSupport.CreateTestMarks(3, ProvenanceMarkResolution.Low, "alice");
        var marks2 = TestSupport.CreateTestMarks(3, ProvenanceMarkResolution.Low, "bob");
        AssertExpectedFormats("test_validate_multiple_chains", ProvenanceMark.Validate(marks1.Concat(marks2)));
    }

    [Fact]
    public void TestValidateMissingGenesis()
    {
        var marks = TestSupport.CreateTestMarks(5, ProvenanceMarkResolution.Low, "test");
        AssertExpectedFormats("test_validate_missing_genesis", ProvenanceMark.Validate(marks.Skip(1)));
    }

    [Fact]
    public void TestValidateSequenceGap()
    {
        var marks = TestSupport.CreateTestMarks(5, ProvenanceMarkResolution.Low, "test");
        AssertExpectedFormats("test_validate_sequence_gap", ProvenanceMark.Validate([marks[0], marks[1], marks[3], marks[4]]));
    }

    [Fact]
    public void TestValidateOutOfOrder()
    {
        var marks = TestSupport.CreateTestMarks(5, ProvenanceMarkResolution.Low, "test");
        AssertExpectedFormats("test_validate_out_of_order", ProvenanceMark.Validate([marks[0], marks[1], marks[3], marks[2], marks[4]]));
    }

    [Fact]
    public void TestValidateHashMismatch()
    {
        ProvenanceMark.RegisterTags();

        var marks = TestSupport.CreateTestMarks(3, ProvenanceMarkResolution.Low, "test");
        var mark0 = marks[0];
        var mark1 = marks[1];
        var badMark = ProvenanceMark.Create(
            mark1.Resolution,
            mark1.Key,
            mark0.Hash,
            mark1.ChainId,
            2,
            CborDate.FromYmdHms(2023, 6, 22, 12, 0, 0));

        AssertExpectedFormats("test_validate_hash_mismatch", ProvenanceMark.Validate([mark0, mark1, badMark]));
    }

    [Fact]
    public void TestValidateDateOrderingViolation()
    {
        var marks = TestSupport.CreateTestMarks(3, ProvenanceMarkResolution.Low, "test");
        AssertExpectedFormats("test_validate_date_ordering_violation", ProvenanceMark.Validate(marks));
    }

    [Fact]
    public void TestValidateMultipleSequencesInChain()
    {
        var marks = TestSupport.CreateTestMarks(7, ProvenanceMarkResolution.Low, "test");
        AssertExpectedFormats("test_validate_multiple_sequences_in_chain", ProvenanceMark.Validate([marks[0], marks[1], marks[3], marks[4], marks[6]]));
    }

    [Fact]
    public void TestValidatePrecedesOpt()
    {
        var marks = TestSupport.CreateTestMarks(3, ProvenanceMarkResolution.Low, "test");
        marks[0].PrecedesOrThrow(marks[1]);
        marks[1].PrecedesOrThrow(marks[2]);

        Assert.Throws<ProvenanceMarkValidationException>(() => marks[1].PrecedesOrThrow(marks[0]));
        Assert.Throws<ProvenanceMarkValidationException>(() => marks[0].PrecedesOrThrow(marks[2]));
    }

    [Fact]
    public void TestValidateChainIdHex()
    {
        var marks = TestSupport.CreateTestMarks(2, ProvenanceMarkResolution.Low, "test");
        var report = ProvenanceMark.Validate(marks);
        var chain = report.Chains[0];
        var chainIdHex = chain.ChainIdHex();
        Assert.Matches("^[0-9a-f]+$", chainIdHex);
        Assert.Equal(Util.ToHex(marks[0].ChainId), chainIdHex);
    }

    [Fact]
    public void TestValidateWithInfo()
    {
        ProvenanceMark.RegisterTags();

        var generator = ProvenanceMarkGenerator.CreateWithPassphrase(ProvenanceMarkResolution.Low, "test");
        var marks = new List<ProvenanceMark>();
        for (var index = 0; index < 3; index++)
        {
            marks.Add(generator.Next(TestSupport.BaseDate(index), "Test info"));
        }

        AssertExpectedFormats("test_validate_with_info", ProvenanceMark.Validate(marks));
    }

    [Fact]
    public void TestValidateSortedChains()
    {
        var marks1 = TestSupport.CreateTestMarks(2, ProvenanceMarkResolution.Low, "zebra");
        var marks2 = TestSupport.CreateTestMarks(2, ProvenanceMarkResolution.Low, "apple");
        var marks3 = TestSupport.CreateTestMarks(2, ProvenanceMarkResolution.Low, "middle");
        AssertExpectedFormats("test_validate_sorted_chains", ProvenanceMark.Validate(marks1.Concat(marks2).Concat(marks3)));
    }

    [Fact]
    public void TestValidateGenesisCheck()
    {
        var marks = TestSupport.CreateTestMarks(3, ProvenanceMarkResolution.Low, "test");
        AssertExpectedFormats("test_validate_date_ordering_violation", ProvenanceMark.Validate(marks));
        AssertExpectedFormats("test_validate_genesis_check", ProvenanceMark.Validate(marks.Skip(1)));
    }

    [Fact]
    public void TestValidateDateOrderingViolationConstructed()
    {
        ProvenanceMark.RegisterTags();

        var marks = TestSupport.CreateTestMarks(2, ProvenanceMarkResolution.Low, "test");
        var mark0 = marks[0];
        var generator = ProvenanceMarkGenerator.CreateWithPassphrase(ProvenanceMarkResolution.Low, "test");
        _ = generator.Next(mark0.Date);
        var badMark = generator.Next(CborDate.FromYmdHms(2023, 6, 19, 12, 0, 0));
        AssertExpectedFormats("test_validate_date_ordering_violation_constructed", ProvenanceMark.Validate([mark0, badMark]));
    }

    [Fact]
    public void TestValidateNonGenesisAtSeqZero()
    {
        ProvenanceMark.RegisterTags();

        var marks = TestSupport.CreateTestMarks(2, ProvenanceMarkResolution.Low, "test");
        var badMark = ProvenanceMark.Create(
            marks[1].Resolution,
            marks[1].Key,
            marks[1].Hash,
            marks[1].ChainId,
            0,
            CborDate.FromYmdHms(2023, 6, 21, 12, 0, 0));

        AssertExpectedFormats("test_validate_non_genesis_at_seq_zero", ProvenanceMark.Validate([marks[0], badMark]));
    }

    [Fact]
    public void TestValidateInvalidGenesisKeyConstructed()
    {
        ProvenanceMark.RegisterTags();

        var marks = TestSupport.CreateTestMarks(2, ProvenanceMarkResolution.Low, "test");
        var badMark = ProvenanceMark.Create(
            marks[1].Resolution,
            marks[1].ChainId,
            marks[1].Hash,
            marks[1].ChainId,
            1,
            CborDate.FromYmdHms(2023, 6, 21, 12, 0, 0));

        AssertExpectedFormats("test_validate_invalid_genesis_key_constructed", ProvenanceMark.Validate([marks[0], badMark]));
    }

    private void AssertExpectedFormats(string name, ValidationReport report)
    {
        var testCase = _expected[name] ?? throw new InvalidOperationException($"missing expected case: {name}");

        if (testCase["json_pretty"] is JsonNode prettyNode)
        {
            var expectedPretty = TestSupport.NormalizeExpectedPrettyJson(prettyNode.GetValue<string>());
            var actualPretty = report.Format(ValidationReportFormat.JsonPretty);
            TestSupport.AssertActualExpected(actualPretty, expectedPretty);
        }

        if (testCase["json_compact"] is JsonNode compactNode)
        {
            var expectedCompact = TestSupport.NormalizeBlock(compactNode.GetValue<string>());
            var actualCompact = TestSupport.NormalizeBlock(report.Format(ValidationReportFormat.JsonCompact));
            TestSupport.AssertActualExpected(actualCompact, expectedCompact);
        }

        if (testCase["text"] is JsonNode textNode)
        {
            var expectedText = NormalizeText(textNode.GetValue<string>());
            var actualText = NormalizeText(report.Format(ValidationReportFormat.Text));
            TestSupport.AssertActualExpected(actualText, expectedText);
        }
    }

    private static string NormalizeText(string value)
    {
        return string.Join(
                '\n',
                TestSupport.NormalizeBlock(value)
                    .Split('\n', StringSplitOptions.None)
                    .Select(line => line.TrimEnd()))
            .Trim();
    }
}

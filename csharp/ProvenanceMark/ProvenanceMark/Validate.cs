using BlockchainCommons.DCbor;

namespace BlockchainCommons.ProvenanceMark;

/// <summary>
/// Output formats for validation reports.
/// </summary>
public enum ValidationReportFormat
{
    Text,
    JsonCompact,
    JsonPretty
}

/// <summary>
/// Base type for validation issues.
/// </summary>
public abstract record ValidationIssue
{
    internal abstract object ToJsonModel();
}

/// <summary>
/// Hash mismatch between consecutive marks.
/// </summary>
public sealed record HashMismatchIssue : ValidationIssue
{
    private readonly byte[] _expected;
    private readonly byte[] _actual;

    public HashMismatchIssue(byte[] expected, byte[] actual)
    {
        _expected = (byte[])expected.Clone();
        _actual = (byte[])actual.Clone();
    }

    public byte[] Expected => (byte[])_expected.Clone();

    public byte[] Actual => (byte[])_actual.Clone();

    public bool Equals(HashMismatchIssue? other)
    {
        return other is not null &&
            _expected.AsSpan().SequenceEqual(other._expected) &&
            _actual.AsSpan().SequenceEqual(other._actual);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var value in _expected)
        {
            hash.Add(value);
        }
        foreach (var value in _actual)
        {
            hash.Add(value);
        }
        return hash.ToHashCode();
    }

    public override string ToString() =>
        $"hash mismatch: expected {Util.ToHex(_expected)}, got {Util.ToHex(_actual)}";

    internal override object ToJsonModel() =>
        new Dictionary<string, object?>
        {
            ["type"] = "HashMismatch",
            ["data"] = new Dictionary<string, object?>
            {
                ["expected"] = Util.ToHex(_expected),
                ["actual"] = Util.ToHex(_actual)
            }
        };
}

/// <summary>
/// Key mismatch between marks.
/// </summary>
public sealed record KeyMismatchIssue : ValidationIssue
{
    public override string ToString() =>
        "key mismatch: current hash was not generated from next key";

    internal override object ToJsonModel() =>
        new Dictionary<string, object?>
        {
            ["type"] = "KeyMismatch"
        };
}

/// <summary>
/// Sequence-number gap between marks.
/// </summary>
public sealed record SequenceGapIssue(uint Expected, uint Actual) : ValidationIssue
{
    public override string ToString() =>
        $"sequence number gap: expected {Expected}, got {Actual}";

    internal override object ToJsonModel() =>
        new Dictionary<string, object?>
        {
            ["type"] = "SequenceGap",
            ["data"] = new Dictionary<string, object?>
            {
                ["expected"] = Expected,
                ["actual"] = Actual
            }
        };
}

/// <summary>
/// Date ordering violation between marks.
/// </summary>
public sealed record DateOrderingIssue(CborDate Previous, CborDate Next) : ValidationIssue
{
    public override string ToString() =>
        $"date must be equal or later: previous is {Previous}, next is {Next}";

    internal override object ToJsonModel() =>
        new Dictionary<string, object?>
        {
            ["type"] = "DateOrdering",
            ["data"] = new Dictionary<string, object?>
            {
                ["previous"] = Util.DateToIso8601(Previous),
                ["next"] = Util.DateToIso8601(Next)
            }
        };
}

/// <summary>
/// Non-genesis mark appears at sequence zero.
/// </summary>
public sealed record NonGenesisAtZeroIssue : ValidationIssue
{
    public override string ToString() => "non-genesis mark at sequence 0";

    internal override object ToJsonModel() =>
        new Dictionary<string, object?>
        {
            ["type"] = "NonGenesisAtZero"
        };
}

/// <summary>
/// Invalid genesis mark key.
/// </summary>
public sealed record InvalidGenesisKeyIssue : ValidationIssue
{
    public override string ToString() => "genesis mark must have key equal to chain_id";

    internal override object ToJsonModel() =>
        new Dictionary<string, object?>
        {
            ["type"] = "InvalidGenesisKey"
        };
}

/// <summary>
/// Mark plus any issues flagged during validation.
/// </summary>
public sealed class FlaggedMark
{
    private FlaggedMark(ProvenanceMark mark, IReadOnlyList<ValidationIssue> issues)
    {
        Mark = mark;
        Issues = issues.ToList().AsReadOnly();
    }

    public ProvenanceMark Mark { get; }

    public IReadOnlyList<ValidationIssue> Issues { get; }

    internal static FlaggedMark Create(ProvenanceMark mark) =>
        new(mark, Array.Empty<ValidationIssue>());

    internal static FlaggedMark WithIssue(ProvenanceMark mark, ValidationIssue issue) =>
        new(mark, [issue]);
}

/// <summary>
/// Report for one contiguous sequence within a chain.
/// </summary>
public sealed class SequenceReport
{
    internal SequenceReport(uint startSeq, uint endSeq, IReadOnlyList<FlaggedMark> marks)
    {
        StartSeq = startSeq;
        EndSeq = endSeq;
        Marks = marks.ToList().AsReadOnly();
    }

    public uint StartSeq { get; }

    public uint EndSeq { get; }

    public IReadOnlyList<FlaggedMark> Marks { get; }
}

/// <summary>
/// Validation report for a single chain id.
/// </summary>
public sealed class ChainReport
{
    internal ChainReport(byte[] chainId, bool hasGenesis, IReadOnlyList<ProvenanceMark> marks, IReadOnlyList<SequenceReport> sequences)
    {
        _chainId = (byte[])chainId.Clone();
        HasGenesis = hasGenesis;
        Marks = marks.ToList().AsReadOnly();
        Sequences = sequences.ToList().AsReadOnly();
    }

    private readonly byte[] _chainId;

    public byte[] ChainId => (byte[])_chainId.Clone();

    public bool HasGenesis { get; }

    public IReadOnlyList<ProvenanceMark> Marks { get; }

    public IReadOnlyList<SequenceReport> Sequences { get; }

    public string ChainIdHex() => Util.ToHex(_chainId);
}

/// <summary>
/// Complete validation report for a set of marks.
/// </summary>
public sealed class ValidationReport
{
    private ValidationReport(IReadOnlyList<ProvenanceMark> marks, IReadOnlyList<ChainReport> chains)
    {
        Marks = marks.ToList().AsReadOnly();
        Chains = chains.ToList().AsReadOnly();
    }

    public IReadOnlyList<ProvenanceMark> Marks { get; }

    public IReadOnlyList<ChainReport> Chains { get; }

    public string Format(ValidationReportFormat format)
    {
        return format switch
        {
            ValidationReportFormat.Text => FormatText(),
            ValidationReportFormat.JsonCompact => Util.SerializeJson(ToJsonModel()),
            ValidationReportFormat.JsonPretty => Util.SerializeJsonIndented(ToJsonModel()),
            _ => string.Empty
        };
    }

    public bool HasIssues()
    {
        foreach (var chain in Chains)
        {
            if (!chain.HasGenesis)
            {
                return true;
            }
        }

        foreach (var chain in Chains)
        {
            foreach (var sequence in chain.Sequences)
            {
                foreach (var mark in sequence.Marks)
                {
                    if (mark.Issues.Count > 0)
                    {
                        return true;
                    }
                }
            }
        }

        if (Chains.Count > 1)
        {
            return true;
        }

        return Chains.Count == 1 && Chains[0].Sequences.Count > 1;
    }

    public static ValidationReport Validate(IEnumerable<ProvenanceMark> marks)
    {
        var seen = new HashSet<ProvenanceMark>();
        var deduplicated = new List<ProvenanceMark>();
        foreach (var mark in marks)
        {
            if (seen.Add(mark))
            {
                deduplicated.Add(mark);
            }
        }

        var chainBins = new Dictionary<string, List<ProvenanceMark>>(StringComparer.Ordinal);
        foreach (var mark in deduplicated)
        {
            var key = Util.ToHex(mark.ChainId);
            if (!chainBins.TryGetValue(key, out var list))
            {
                list = [];
                chainBins[key] = list;
            }
            list.Add(mark);
        }

        var chains = new List<ChainReport>();
        foreach (var entry in chainBins.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            var chainMarks = entry.Value.OrderBy(mark => mark.Sequence).ToList();
            var hasGenesis = chainMarks.FirstOrDefault() is { } first &&
                first.Sequence == 0 &&
                first.IsGenesis();
            var sequences = BuildSequenceBins(chainMarks);
            chains.Add(new ChainReport(chainMarks[0].ChainId, hasGenesis, chainMarks, sequences));
        }

        return new ValidationReport(deduplicated, chains);
    }

    private string FormatText()
    {
        if (!IsInteresting())
        {
            return string.Empty;
        }

        var lines = new List<string>
        {
            $"Total marks: {Marks.Count}",
            $"Chains: {Chains.Count}",
            string.Empty
        };

        for (var chainIndex = 0; chainIndex < Chains.Count; chainIndex++)
        {
            var chain = Chains[chainIndex];
            var chainIdHex = chain.ChainIdHex();
            var shortChainId = chainIdHex.Length > 8 ? chainIdHex[..8] : chainIdHex;

            lines.Add($"Chain {chainIndex + 1}: {shortChainId}");

            if (!chain.HasGenesis)
            {
                lines.Add("  Warning: No genesis mark found");
            }

            foreach (var sequence in chain.Sequences)
            {
                foreach (var flaggedMark in sequence.Marks)
                {
                    var mark = flaggedMark.Mark;
                    var shortId = mark.IdHex()[..8];
                    var annotations = new List<string>();

                    if (mark.IsGenesis())
                    {
                        annotations.Add("genesis mark");
                    }

                    foreach (var issue in flaggedMark.Issues)
                    {
                        var issueText = issue switch
                        {
                            SequenceGapIssue gap => $"gap: {gap.Expected} missing",
                            DateOrderingIssue ordering => $"date {ordering.Previous} < {ordering.Next}",
                            HashMismatchIssue => "hash mismatch",
                            KeyMismatchIssue => "key mismatch",
                            NonGenesisAtZeroIssue => "non-genesis at seq 0",
                            InvalidGenesisKeyIssue => "invalid genesis key",
                            _ => issue.ToString()
                        };
                        annotations.Add(issueText);
                    }

                    if (annotations.Count == 0)
                    {
                        lines.Add($"  {mark.Sequence}: {shortId}");
                    }
                    else
                    {
                        lines.Add($"  {mark.Sequence}: {shortId} ({string.Join(", ", annotations)})");
                    }
                }
            }

            lines.Add(string.Empty);
        }

        return string.Join('\n', lines).TrimEnd();
    }

    private bool IsInteresting()
    {
        if (Chains.Count == 0)
        {
            return false;
        }

        foreach (var chain in Chains)
        {
            if (!chain.HasGenesis)
            {
                return true;
            }
        }

        if (Chains.Count == 1 && Chains[0].Sequences.Count == 1)
        {
            var sequence = Chains[0].Sequences[0];
            if (sequence.Marks.All(mark => mark.Issues.Count == 0))
            {
                return false;
            }
        }

        return true;
    }

    private object ToJsonModel()
    {
        return new Dictionary<string, object?>
        {
            ["marks"] = Marks.Select(mark => mark.ToUrString()).ToList(),
            ["chains"] = Chains.Select(chain => new Dictionary<string, object?>
            {
                ["chain_id"] = chain.ChainIdHex(),
                ["has_genesis"] = chain.HasGenesis,
                ["marks"] = chain.Marks.Select(mark => mark.ToUrString()).ToList(),
                ["sequences"] = chain.Sequences.Select(sequence => new Dictionary<string, object?>
                {
                    ["start_seq"] = sequence.StartSeq,
                    ["end_seq"] = sequence.EndSeq,
                    ["marks"] = sequence.Marks.Select(flagged => new Dictionary<string, object?>
                    {
                        ["mark"] = flagged.Mark.ToUrString(),
                        ["issues"] = flagged.Issues.Select(issue => issue.ToJsonModel()).ToList()
                    }).ToList()
                }).ToList()
            }).ToList()
        };
    }

    private static IReadOnlyList<SequenceReport> BuildSequenceBins(IReadOnlyList<ProvenanceMark> marks)
    {
        var sequences = new List<SequenceReport>();
        var currentSequence = new List<FlaggedMark>();

        for (var index = 0; index < marks.Count; index++)
        {
            var mark = marks[index];
            if (index == 0)
            {
                currentSequence.Add(FlaggedMark.Create(mark));
                continue;
            }

            var previous = marks[index - 1];
            try
            {
                previous.PrecedesOrThrow(mark);
                currentSequence.Add(FlaggedMark.Create(mark));
            }
            catch (Exception ex)
            {
                if (currentSequence.Count > 0)
                {
                    sequences.Add(CreateSequenceReport(currentSequence));
                }

                var issue = ex is ProvenanceMarkValidationException validation
                    ? validation.Issue
                    : new KeyMismatchIssue();
                currentSequence = [FlaggedMark.WithIssue(mark, issue)];
            }
        }

        if (currentSequence.Count > 0)
        {
            sequences.Add(CreateSequenceReport(currentSequence));
        }

        return sequences;
    }

    private static SequenceReport CreateSequenceReport(IReadOnlyList<FlaggedMark> marks)
    {
        var startSeq = marks.FirstOrDefault()?.Mark.Sequence ?? 0;
        var endSeq = marks.LastOrDefault()?.Mark.Sequence ?? 0;
        return new SequenceReport(startSeq, endSeq, marks);
    }
}

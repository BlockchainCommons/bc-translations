using System.Text.Json;
using System.Text.Json.Nodes;
using BlockchainCommons.DCbor;
using Xunit.Sdk;

namespace BlockchainCommons.ProvenanceMark.Tests;

internal static class TestSupport
{
    internal static JsonNode LoadJsonResource(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", fileName);
        return JsonNode.Parse(File.ReadAllText(path))
            ?? throw new InvalidOperationException($"Missing or invalid JSON resource: {fileName}");
    }

    internal static string NormalizeExpectedPrettyJson(string expected)
    {
        var normalized = NormalizeBlock(expected);
        if (string.IsNullOrEmpty(normalized))
        {
            return string.Empty;
        }

        var node = JsonNode.Parse(normalized)
            ?? throw new InvalidOperationException("Expected pretty JSON block could not be parsed.");
        return node.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    internal static string NormalizeBlock(string value)
    {
        var normalized = value.Replace("\r\n", "\n", StringComparison.Ordinal);
        var lines = normalized.Split('\n');
        var nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
        if (nonEmptyLines.Count == 0)
        {
            return string.Empty;
        }

        var firstIndent = nonEmptyLines[0].TakeWhile(ch => ch == ' ').Count();
        int baselineIndent;
        if (firstIndent == 0 && nonEmptyLines.Count > 1)
        {
            baselineIndent = nonEmptyLines
                .Skip(1)
                .Select(line => line.TakeWhile(ch => ch == ' ').Count())
                .DefaultIfEmpty(0)
                .Min();
        }
        else
        {
            baselineIndent = nonEmptyLines
                .Select(line => line.TakeWhile(ch => ch == ' ').Count())
                .DefaultIfEmpty(0)
                .Min();
        }

        var firstNonEmptySeen = false;
        var adjusted = lines.Select(line =>
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return line;
            }

            if (!firstNonEmptySeen)
            {
                firstNonEmptySeen = true;
                return line;
            }

            return line.Length >= baselineIndent ? line[baselineIndent..] : line;
        });

        return string.Join('\n', adjusted).Trim();
    }

    internal static void AssertActualExpected(string actual, string expected)
    {
        if (actual == expected)
        {
            return;
        }

        throw new XunitException(
            $"actual and expected differ{Environment.NewLine}--- actual ---{Environment.NewLine}{actual}{Environment.NewLine}--- expected ---{Environment.NewLine}{expected}");
    }

    internal static byte[] Hex(string value)
    {
        return Convert.FromHexString(value);
    }

    internal static ProvenanceMarkResolution ResolutionFromString(string value)
    {
        return value switch
        {
            "low" => ProvenanceMarkResolution.Low,
            "medium" => ProvenanceMarkResolution.Medium,
            "quartile" => ProvenanceMarkResolution.Quartile,
            "high" => ProvenanceMarkResolution.High,
            _ => throw new InvalidOperationException($"unsupported resolution value: {value}")
        };
    }

    internal static CborDate BaseDate(int dayOffset)
    {
        return CborDate.FromDateTime(
            new DateTimeOffset(2023, 6, 20, 12, 0, 0, TimeSpan.Zero).AddDays(dayOffset));
    }

    internal static List<ProvenanceMark> CreateTestMarks(int count, ProvenanceMarkResolution resolution, string passphrase)
    {
        ProvenanceMark.RegisterTags();
        var generator = ProvenanceMarkGenerator.CreateWithPassphrase(resolution, passphrase);
        var marks = new List<ProvenanceMark>(count);
        for (var index = 0; index < count; index++)
        {
            marks.Add(generator.Next(BaseDate(index)));
        }
        return marks;
    }

    internal static List<ProvenanceMark> CreateSerializedGeneratorMarks(int count, ProvenanceMarkResolution resolution, string passphrase)
    {
        ProvenanceMark.RegisterTags();
        var encodedGenerator = ProvenanceMarkGenerator.CreateWithPassphrase(resolution, passphrase).ToJson();
        var marks = new List<ProvenanceMark>(count);
        for (var index = 0; index < count; index++)
        {
            var generator = ProvenanceMarkGenerator.FromJson(encodedGenerator);
            var mark = generator.Next(BaseDate(index));
            encodedGenerator = generator.ToJson();
            marks.Add(mark);
        }
        return marks;
    }
}

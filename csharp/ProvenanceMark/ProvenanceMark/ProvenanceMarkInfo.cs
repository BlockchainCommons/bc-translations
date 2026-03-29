using System.Text.Json;
using BlockchainCommons.BCUR;

namespace BlockchainCommons.ProvenanceMark;

/// <summary>
/// Convenience summary wrapper around a provenance mark and its public identifiers.
/// </summary>
public sealed class ProvenanceMarkInfo
{
    private ProvenanceMarkInfo(UR ur, string bytewords, string bytemoji, string comment, ProvenanceMark mark)
    {
        Ur = ur;
        Bytewords = bytewords;
        Bytemoji = bytemoji;
        Comment = comment;
        Mark = mark;
    }

    public ProvenanceMark Mark { get; }

    public UR Ur { get; }

    public string Bytewords { get; }

    public string Bytemoji { get; }

    public string Comment { get; }

    public string MarkdownSummary()
    {
        var lines = new List<string>
        {
            "---",
            string.Empty,
            Mark.Date.ToString(),
            string.Empty,
            $"#### {Ur}",
            string.Empty,
            $"#### `{Bytewords}`",
            string.Empty,
            Bytemoji,
            string.Empty
        };

        if (!string.IsNullOrEmpty(Comment))
        {
            lines.Add(Comment);
            lines.Add(string.Empty);
        }

        return string.Join('\n', lines);
    }

    public string ToJson()
    {
        var markJson = JsonDocument.Parse(Mark.ToJson()).RootElement.Clone();
        var fields = new Dictionary<string, object?>
        {
            ["ur"] = Ur.ToString(),
            ["bytewords"] = Bytewords,
            ["bytemoji"] = Bytemoji,
            ["mark"] = markJson
        };
        if (!string.IsNullOrEmpty(Comment))
        {
            fields["comment"] = Comment;
        }
        return Util.SerializeJson(fields);
    }

    public static ProvenanceMarkInfo Create(ProvenanceMark mark, string? comment = null)
    {
        ArgumentNullException.ThrowIfNull(mark);
        var ur = mark.ToUr();
        return new ProvenanceMarkInfo(
            ur,
            mark.IdBytewords(4, true),
            mark.IdBytemoji(4, true),
            comment ?? string.Empty,
            mark);
    }

    public static ProvenanceMarkInfo FromJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var ur = UR.FromUrString(root.GetProperty("ur").GetString()!);
            var bytewords = root.GetProperty("bytewords").GetString()!;
            var bytemoji = root.GetProperty("bytemoji").GetString()!;
            var comment = root.TryGetProperty("comment", out var commentProperty)
                ? commentProperty.GetString() ?? string.Empty
                : string.Empty;
            var mark = ProvenanceMark.FromUr(ur);
            return new ProvenanceMarkInfo(ur, bytewords, bytemoji, comment, mark);
        }
        catch (Exception ex)
        {
            throw ProvenanceMarkException.Json(ex.Message, ex);
        }
    }
}

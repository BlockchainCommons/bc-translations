namespace BlockchainCommons.DCbor;

/// <summary>
/// Options controlling diagnostic notation formatting.
/// </summary>
public class DiagFormatOptions
{
    public bool Annotate { get; set; }
    public bool Summarize { get; set; }
    public bool Flat { get; set; }
    public TagsStoreOption Tags { get; set; } = TagsStoreOption.DefaultGlobal;

    public DiagFormatOptions WithAnnotate(bool v) { Annotate = v; return this; }
    public DiagFormatOptions WithSummarize(bool v) { Summarize = v; if (v) Flat = true; return this; }
    public DiagFormatOptions WithFlat(bool v) { Flat = v; return this; }
    public DiagFormatOptions WithTags(TagsStoreOption v) { Tags = v; return this; }
}

/// <summary>
/// Diagnostic notation rendering for CBOR values.
/// </summary>
public static class CborDiag
{
    /// <summary>Returns diagnostic notation for this CBOR value.</summary>
    public static string Diagnostic(this Cbor cbor)
    {
        return cbor.DiagnosticOpt(new DiagFormatOptions());
    }

    /// <summary>Returns diagnostic notation with annotations.</summary>
    public static string DiagnosticAnnotated(this Cbor cbor)
    {
        return cbor.DiagnosticOpt(new DiagFormatOptions { Annotate = true });
    }

    /// <summary>Returns flat (single-line) diagnostic notation.</summary>
    public static string DiagnosticFlat(this Cbor cbor)
    {
        return cbor.DiagnosticOpt(new DiagFormatOptions { Flat = true });
    }

    /// <summary>Returns a summarized representation using tag summarizers.</summary>
    public static string Summary(this Cbor cbor)
    {
        return cbor.DiagnosticOpt(new DiagFormatOptions().WithSummarize(true));
    }

    /// <summary>Returns diagnostic notation with custom options.</summary>
    public static string DiagnosticOpt(this Cbor cbor, DiagFormatOptions opts)
    {
        var item = BuildDiagItem(cbor, opts);
        return item.Format(opts);
    }

    private static DiagItem BuildDiagItem(Cbor cbor, DiagFormatOptions opts)
    {
        switch (cbor.Case)
        {
            case CborCase.UnsignedCase:
            case CborCase.NegativeCase:
            case CborCase.ByteStringCase:
            case CborCase.TextCase:
            case CborCase.SimpleCase:
                return new DiagItem.Leaf(cbor.ToString());

            case CborCase.ArrayCase a:
            {
                var items = a.Value.Select(x => BuildDiagItem(x, opts)).ToList();
                return new DiagItem.Group("[", "]", items, false, null);
            }

            case CborCase.MapCase m:
            {
                var items = new List<DiagItem>();
                foreach (var (key, value) in m.Value)
                {
                    items.Add(BuildDiagItem(key, opts));
                    items.Add(BuildDiagItem(value, opts));
                }
                return new DiagItem.Group("{", "}", items, true, null);
            }

            case CborCase.TaggedCase tg:
            {
                if (opts.Summarize)
                {
                    CborSummarizer? summarizer = null;
                    switch (opts.Tags)
                    {
                        case TagsStoreOption.Custom custom:
                            summarizer = custom.Store.GetSummarizer(tg.Tag.Value);
                            break;
                        case TagsStoreOption.Global:
                            summarizer = GlobalTags.WithTags(store => store.GetSummarizer(tg.Tag.Value));
                            break;
                    }

                    if (summarizer != null)
                    {
                        try
                        {
                            string summary = summarizer(tg.Item, opts.Flat);
                            return new DiagItem.Leaf(summary);
                        }
                        catch (Exception ex)
                        {
                            return new DiagItem.Leaf($"<error: {ex.Message}>");
                        }
                    }
                }

                string? comment = null;
                if (opts.Annotate)
                {
                    comment = opts.Tags switch
                    {
                        TagsStoreOption.Custom custom =>
                            custom.Store.AssignedNameForTag(tg.Tag),
                        TagsStoreOption.Global =>
                            GlobalTags.WithTags(store => store.AssignedNameForTag(tg.Tag)),
                        _ => null,
                    };
                }

                var child = BuildDiagItem(tg.Item, opts);
                string begin = tg.Tag.Value.ToString() + "(";
                return new DiagItem.Group(begin, ")", new List<DiagItem> { child }, false, comment);
            }

            default:
                throw new InvalidOperationException();
        }
    }
}

/// <summary>
/// Internal tree representation for diagnostic formatting.
/// </summary>
internal abstract class DiagItem
{
    internal sealed class Leaf : DiagItem
    {
        internal string Text { get; }
        internal Leaf(string text) { Text = text; }
    }

    internal sealed class Group : DiagItem
    {
        internal string Begin { get; }
        internal string End { get; }
        internal List<DiagItem> Items { get; }
        internal bool IsPairs { get; }
        internal string? Comment { get; }

        internal Group(string begin, string end, List<DiagItem> items, bool isPairs, string? comment)
        {
            Begin = begin;
            End = end;
            Items = items;
            IsPairs = isPairs;
            Comment = comment;
        }
    }

    internal string Format(DiagFormatOptions opts) => FormatOpt(0, "", opts);

    private string FormatOpt(int level, string separator, DiagFormatOptions opts)
    {
        switch (this)
        {
            case Leaf leaf:
                return FormatLine(level, opts, leaf.Text, separator, null);

            case Group g:
                if (!opts.Flat
                    && (ContainsGroup()
                        || TotalStringsLen() > 20
                        || GreatestStringsLen() > 20))
                {
                    return MultilineComposition(level, separator, opts);
                }
                return SingleLineComposition(level, separator, opts);

            default:
                throw new InvalidOperationException();
        }
    }

    private static string FormatLine(int level, DiagFormatOptions opts, string text, string separator, string? comment)
    {
        string indent = opts.Flat ? "" : new string(' ', level * 4);
        string result = $"{indent}{text}{separator}";
        if (comment != null)
            return $"{result}   / {comment} /";
        return result;
    }

    private string SingleLineComposition(int level, string separator, DiagFormatOptions opts)
    {
        switch (this)
        {
            case Leaf leaf:
                return FormatLine(level, opts, leaf.Text, separator, null);

            case Group g:
            {
                var components = g.Items.Select(item => item switch
                {
                    Leaf l => l.Text,
                    Group => item.SingleLineComposition(level + 1, separator, opts),
                    _ => throw new InvalidOperationException(),
                }).ToList();

                string pairSeparator = g.IsPairs ? ": " : ", ";
                string joined = JoinItems(components, ", ", pairSeparator);
                string text = g.Begin + joined + g.End;
                string? comment = g.Comment;
                return FormatLine(level, opts, text, separator, comment);
            }
            default:
                throw new InvalidOperationException();
        }
    }

    private string MultilineComposition(int level, string separator, DiagFormatOptions opts)
    {
        switch (this)
        {
            case Leaf leaf:
                return leaf.Text;

            case Group g:
            {
                var lines = new List<string>();
                // Use a non-flat version for the opening line
                var nonFlatOpts = new DiagFormatOptions
                {
                    Annotate = opts.Annotate,
                    Summarize = opts.Summarize,
                    Flat = false,
                    Tags = opts.Tags,
                };
                lines.Add(FormatLine(level, nonFlatOpts, g.Begin, "", g.Comment));

                for (int i = 0; i < g.Items.Count; i++)
                {
                    string sep;
                    if (i == g.Items.Count - 1)
                        sep = "";
                    else if (g.IsPairs && (i & 1) == 0)
                        sep = ":";
                    else
                        sep = ",";
                    lines.Add(g.Items[i].FormatOpt(level + 1, sep, opts));
                }

                lines.Add(FormatLine(level, opts, g.End, separator, null));
                return string.Join("\n", lines);
            }
            default:
                throw new InvalidOperationException();
        }
    }

    private int TotalStringsLen()
    {
        return this switch
        {
            Leaf leaf => leaf.Text.Length,
            Group g => g.Items.Sum(item => item.TotalStringsLen()),
            _ => 0,
        };
    }

    private int GreatestStringsLen()
    {
        return this switch
        {
            Leaf leaf => leaf.Text.Length,
            Group g => g.Items.Count > 0 ? g.Items.Max(item => item.TotalStringsLen()) : 0,
            _ => 0,
        };
    }

    private bool IsGroup() => this is Group;

    private bool ContainsGroup()
    {
        return this switch
        {
            Leaf => false,
            Group g => g.Items.Any(x => x.IsGroup()),
            _ => false,
        };
    }

    private static string JoinItems(List<string> elements, string itemSeparator, string? pairSeparator)
    {
        pairSeparator ??= itemSeparator;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < elements.Count; i++)
        {
            sb.Append(elements[i]);
            if (i != elements.Count - 1)
            {
                sb.Append((i & 1) != 0 ? itemSeparator : pairSeparator);
            }
        }
        return sb.ToString();
    }
}

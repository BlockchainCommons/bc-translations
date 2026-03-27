using System.Text;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Options for formatting envelopes in envelope notation.
/// </summary>
public sealed class EnvelopeFormatOpts
{
    /// <summary>Whether to format in flat (single-line) mode.</summary>
    public bool Flat { get; }

    /// <summary>The format context option.</summary>
    public FormatContextOpt Context { get; }

    /// <summary>
    /// Creates new envelope format options.
    /// </summary>
    /// <param name="flat">Whether to format in flat mode.</param>
    /// <param name="context">The format context option.</param>
    public EnvelopeFormatOpts(bool flat = false, FormatContextOpt? context = null)
    {
        Flat = flat;
        Context = context ?? FormatContextOpt.Global;
    }
}

/// <summary>
/// The item types produced during envelope notation formatting.
/// </summary>
/// <remarks>
/// An <see cref="EnvelopeFormatItem"/> is a recursive tree that can be flattened
/// and rendered to text in either flat or hierarchical mode.
/// </remarks>
public abstract class EnvelopeFormatItem : IComparable<EnvelopeFormatItem>
{
    private EnvelopeFormatItem() { }

    public sealed class Begin : EnvelopeFormatItem
    {
        public string Delimiter { get; }
        public Begin(string delimiter) { Delimiter = delimiter; }
    }

    public sealed class End : EnvelopeFormatItem
    {
        public string Delimiter { get; }
        public End(string delimiter) { Delimiter = delimiter; }
    }

    public sealed class Item : EnvelopeFormatItem
    {
        public string Text { get; }
        public Item(string text) { Text = text; }
    }

    public sealed class Separator : EnvelopeFormatItem
    {
        internal static readonly Separator Instance = new();
    }

    public sealed class ListItems : EnvelopeFormatItem
    {
        public IReadOnlyList<EnvelopeFormatItem> Items { get; }
        public ListItems(IReadOnlyList<EnvelopeFormatItem> items) { Items = items; }
    }

    private int Index() => this switch
    {
        Begin => 1,
        End => 2,
        Item => 3,
        Separator => 4,
        ListItems => 5,
        _ => 0,
    };

    /// <summary>Flattens the item tree into a list.</summary>
    public List<EnvelopeFormatItem> Flatten()
    {
        if (this is ListItems list)
        {
            var result = new List<EnvelopeFormatItem>();
            foreach (var item in list.Items)
                result.AddRange(item.Flatten());
            return result;
        }
        return [this];
    }

    /// <summary>Formats this item tree as text.</summary>
    public string Format(EnvelopeFormatOpts opts)
    {
        return opts.Flat ? FormatFlat() : FormatHierarchical();
    }

    /// <summary>Formats in flat (single-line) mode.</summary>
    public string FormatFlat()
    {
        var sb = new StringBuilder();
        var items = Flatten();
        foreach (var item in items)
        {
            switch (item)
            {
                case Begin b:
                    if (sb.Length > 0 && sb[^1] != ' ') sb.Append(' ');
                    sb.Append(b.Delimiter);
                    sb.Append(' ');
                    break;
                case End e:
                    if (sb.Length > 0 && sb[^1] != ' ') sb.Append(' ');
                    sb.Append(e.Delimiter);
                    sb.Append(' ');
                    break;
                case Item i:
                    sb.Append(i.Text);
                    break;
                case Separator:
                {
                    var trimmed = sb.ToString().TrimEnd();
                    sb.Clear();
                    sb.Append(trimmed);
                    sb.Append(", ");
                    break;
                }
                case ListItems list:
                    foreach (var child in list.Items)
                        sb.Append(child.FormatFlat());
                    break;
            }
        }
        return sb.ToString();
    }

    /// <summary>Formats in hierarchical (indented) mode.</summary>
    public string FormatHierarchical()
    {
        var lines = new List<string>();
        int level = 0;
        var currentLine = new StringBuilder();
        var items = Nicen(Flatten());
        foreach (var item in items)
        {
            switch (item)
            {
                case Begin b:
                    if (b.Delimiter.Length > 0)
                    {
                        string c;
                        if (currentLine.Length == 0)
                        {
                            c = b.Delimiter;
                        }
                        else
                        {
                            c = AddSpaceAtEndIfNeeded(currentLine.ToString()) + b.Delimiter;
                        }
                        lines.Add(Indent(level) + c + "\n");
                    }
                    level++;
                    currentLine.Clear();
                    break;
                case End e:
                    if (currentLine.Length > 0)
                    {
                        lines.Add(Indent(level) + currentLine + "\n");
                        currentLine.Clear();
                    }
                    level--;
                    lines.Add(Indent(level) + e.Delimiter + "\n");
                    break;
                case Item i:
                    currentLine.Append(i.Text);
                    break;
                case Separator:
                    if (currentLine.Length > 0)
                    {
                        lines.Add(Indent(level) + currentLine + "\n");
                        currentLine.Clear();
                    }
                    break;
                case ListItems:
                    lines.Add("<list>");
                    break;
            }
        }
        if (currentLine.Length > 0)
            lines.Add(currentLine.ToString());

        return string.Join("", lines);
    }

    public int CompareTo(EnvelopeFormatItem? other)
    {
        if (other is null) return 1;
        int indexCmp = Index().CompareTo(other.Index());
        if (indexCmp != 0) return indexCmp;
        return (this, other) switch
        {
            (Begin a, Begin b) => string.Compare(a.Delimiter, b.Delimiter, StringComparison.Ordinal),
            (End a, End b) => string.Compare(a.Delimiter, b.Delimiter, StringComparison.Ordinal),
            (Item a, Item b) => string.Compare(a.Text, b.Text, StringComparison.Ordinal),
            (Separator, Separator) => 0,
            (ListItems a, ListItems b) => CompareItemLists(a.Items, b.Items),
            _ => 0,
        };
    }

    public override string ToString() => this switch
    {
        Begin b => $".begin({b.Delimiter})",
        End e => $".end({e.Delimiter})",
        Item i => $".item({i.Text})",
        Separator => ".separator",
        ListItems l => $".list({l.Items})",
        _ => base.ToString()!,
    };

    // --- Static helpers ---

    private static string Indent(int level) => new(' ', level * 4);

    private static string AddSpaceAtEndIfNeeded(string s)
    {
        if (s.Length == 0) return " ";
        if (s.EndsWith(' ')) return s;
        return s + " ";
    }

    internal static List<EnvelopeFormatItem> Nicen(List<EnvelopeFormatItem> items)
    {
        var input = new List<EnvelopeFormatItem>(items);
        var result = new List<EnvelopeFormatItem>();
        while (input.Count > 0)
        {
            var current = input[0];
            input.RemoveAt(0);
            if (input.Count == 0)
            {
                result.Add(current);
                break;
            }
            if (current is End endItem && input[0] is Begin beginItem)
            {
                result.Add(new End($"{endItem.Delimiter} {beginItem.Delimiter}"));
                result.Add(new Begin(""));
                input.RemoveAt(0);
            }
            else
            {
                result.Add(current);
            }
        }
        return result;
    }

    private static int CompareItemLists(IReadOnlyList<EnvelopeFormatItem> a, IReadOnlyList<EnvelopeFormatItem> b)
    {
        int minSize = Math.Min(a.Count, b.Count);
        for (int i = 0; i < minSize; i++)
        {
            int cmp = a[i].CompareTo(b[i]);
            if (cmp != 0) return cmp;
        }
        return a.Count.CompareTo(b.Count);
    }
}

/// <summary>
/// Envelope notation format methods (partial class on Envelope).
/// </summary>
public partial class Envelope
{
    /// <summary>
    /// Returns the envelope notation for this envelope with the given options.
    /// </summary>
    public string FormatOpt(EnvelopeFormatOpts opts)
    {
        return EnvelopeFormatting.FormatItem(this, opts).Format(opts).Trim();
    }

    /// <summary>
    /// Returns the envelope notation for this envelope.
    /// </summary>
    public string Format()
    {
        return FormatOpt(new EnvelopeFormatOpts());
    }

    /// <summary>
    /// Returns the envelope notation in flat (single-line) format.
    /// </summary>
    public string FormatFlat()
    {
        return FormatOpt(new EnvelopeFormatOpts(flat: true));
    }
}

/// <summary>
/// Internal formatting logic for building envelope notation item trees.
/// </summary>
internal static class EnvelopeFormatting
{
    /// <summary>
    /// Builds an <see cref="EnvelopeFormatItem"/> tree for a CBOR value.
    /// </summary>
    public static EnvelopeFormatItem FormatItemForCbor(Cbor cbor, EnvelopeFormatOpts opts)
    {
        if (cbor.Case is CborCase.TaggedCase tg && tg.Tag.Value == BcTags.TagEnvelope)
        {
            try
            {
                var envelope = Envelope.FromUntaggedCbor(tg.Item);
                return FormatItem(envelope, opts);
            }
            catch
            {
                return new EnvelopeFormatItem.Item("<error>");
            }
        }

        try
        {
            var summary = cbor.EnvelopeSummary(int.MaxValue, opts.Context);
            return new EnvelopeFormatItem.Item(summary);
        }
        catch
        {
            return new EnvelopeFormatItem.Item("<error>");
        }
    }

    /// <summary>
    /// Builds an <see cref="EnvelopeFormatItem"/> tree for an Envelope.
    /// </summary>
    public static EnvelopeFormatItem FormatItem(Envelope envelope, EnvelopeFormatOpts opts)
    {
        switch (envelope.Case)
        {
            case EnvelopeCase.LeafCase l:
                return FormatItemForCbor(l.Cbor, opts);

            case EnvelopeCase.WrappedCase w:
                return new EnvelopeFormatItem.ListItems(new List<EnvelopeFormatItem>
                {
                    new EnvelopeFormatItem.Begin("{"),
                    FormatItem(w.Envelope, opts),
                    new EnvelopeFormatItem.End("}"),
                });

            case EnvelopeCase.AssertionCase a:
                return FormatItemForAssertion(a.Assertion, opts);

            case EnvelopeCase.KnownValueCase kv:
                return FormatItemForKnownValue(kv.Value, opts);

            case EnvelopeCase.EncryptedCase:
                return new EnvelopeFormatItem.Item("ENCRYPTED");

            case EnvelopeCase.CompressedCase:
                return new EnvelopeFormatItem.Item("COMPRESSED");

            case EnvelopeCase.NodeCase n:
                return FormatItemForNode(n, envelope, opts);

            case EnvelopeCase.ElidedCase:
                return new EnvelopeFormatItem.Item("ELIDED");

            default:
                throw new InvalidOperationException();
        }
    }

    private static EnvelopeFormatItem FormatItemForAssertion(Assertion assertion, EnvelopeFormatOpts opts)
    {
        return new EnvelopeFormatItem.ListItems(new List<EnvelopeFormatItem>
        {
            FormatItem(assertion.Predicate, opts),
            new EnvelopeFormatItem.Item(": "),
            FormatItem(assertion.Object, opts),
        });
    }

    private static EnvelopeFormatItem FormatItemForKnownValue(KnownValue value, EnvelopeFormatOpts opts)
    {
        string name;
        switch (opts.Context)
        {
            case FormatContextOpt.NoneOpt:
                name = value.Name.FlankedBy("'", "'");
                break;

            case FormatContextOpt.GlobalOpt:
                name = GlobalFormatContext.WithFormatContext(ctx =>
                {
                    var assigned = ctx.KnownValues.AssignedName(value);
                    return (assigned ?? value.Name).FlankedBy("'", "'");
                });
                break;

            case FormatContextOpt.CustomOpt custom:
            {
                var assigned = custom.Context.KnownValues.AssignedName(value);
                name = (assigned ?? value.Name).FlankedBy("'", "'");
                break;
            }

            default:
                name = value.Name.FlankedBy("'", "'");
                break;
        }
        return new EnvelopeFormatItem.Item(name);
    }

    private static EnvelopeFormatItem FormatItemForNode(
        EnvelopeCase.NodeCase node,
        Envelope envelope,
        EnvelopeFormatOpts opts)
    {
        var items = new List<EnvelopeFormatItem>();

        var subjectItem = FormatItem(node.Subject, opts);
        int elidedCount = 0;
        int encryptedCount = 0;
        int compressedCount = 0;
        var typeAssertionItems = new List<List<EnvelopeFormatItem>>();
        var assertionItems = new List<List<EnvelopeFormatItem>>();

        foreach (var assertion in node.Assertions)
        {
            switch (assertion.Case)
            {
                case EnvelopeCase.ElidedCase:
                    elidedCount++;
                    break;
                case EnvelopeCase.EncryptedCase:
                    encryptedCount++;
                    break;
                case EnvelopeCase.CompressedCase:
                    compressedCount++;
                    break;
                default:
                {
                    var item = new List<EnvelopeFormatItem> { FormatItem(assertion, opts) };
                    bool isTypeAssertion = false;
                    var predicate = assertion.AsPredicate();
                    if (predicate is not null)
                    {
                        var knownValue = predicate.Subject.AsKnownValue();
                        if (knownValue is not null && knownValue == KnownValuesRegistry.IsA)
                        {
                            isTypeAssertion = true;
                        }
                    }

                    if (isTypeAssertion)
                        typeAssertionItems.Add(item);
                    else
                        assertionItems.Add(item);
                    break;
                }
            }
        }

        typeAssertionItems.Sort((a, b) => a[0].CompareTo(b[0]));
        assertionItems.Sort((a, b) => a[0].CompareTo(b[0]));
        assertionItems.InsertRange(0, typeAssertionItems);

        if (compressedCount > 1)
            assertionItems.Add(new List<EnvelopeFormatItem>
                { new EnvelopeFormatItem.Item($"COMPRESSED ({compressedCount})") });
        else if (compressedCount > 0)
            assertionItems.Add(new List<EnvelopeFormatItem>
                { new EnvelopeFormatItem.Item("COMPRESSED") });

        if (elidedCount > 1)
            assertionItems.Add(new List<EnvelopeFormatItem>
                { new EnvelopeFormatItem.Item($"ELIDED ({elidedCount})") });
        else if (elidedCount > 0)
            assertionItems.Add(new List<EnvelopeFormatItem>
                { new EnvelopeFormatItem.Item("ELIDED") });

        if (encryptedCount > 1)
            assertionItems.Add(new List<EnvelopeFormatItem>
                { new EnvelopeFormatItem.Item($"ENCRYPTED ({encryptedCount})") });
        else if (encryptedCount > 0)
            assertionItems.Add(new List<EnvelopeFormatItem>
                { new EnvelopeFormatItem.Item("ENCRYPTED") });

        // Intersperse with separators
        var joinedItems = new List<EnvelopeFormatItem>();
        for (int i = 0; i < assertionItems.Count; i++)
        {
            joinedItems.AddRange(assertionItems[i]);
            if (i < assertionItems.Count - 1)
                joinedItems.Add(EnvelopeFormatItem.Separator.Instance);
        }

        bool needsBraces = node.Subject.IsSubjectAssertion;

        if (needsBraces)
            items.Add(new EnvelopeFormatItem.Begin("{"));
        items.Add(subjectItem);
        if (needsBraces)
            items.Add(new EnvelopeFormatItem.End("}"));
        items.Add(new EnvelopeFormatItem.Begin("["));
        items.AddRange(joinedItems);
        items.Add(new EnvelopeFormatItem.End("]"));

        return new EnvelopeFormatItem.ListItems(items);
    }
}

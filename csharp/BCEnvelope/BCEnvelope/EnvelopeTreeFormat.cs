using System.Text;
using BlockchainCommons.BCComponents;
using BlockchainCommons.BCUR;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Tree format methods (partial class on Envelope).
/// </summary>
public partial class Envelope
{
    /// <summary>
    /// Returns a tree-formatted string representation of the envelope with
    /// default options.
    /// </summary>
    public string TreeFormat()
    {
        return TreeFormatOpt(new TreeFormatOpts());
    }

    /// <summary>
    /// Returns a tree-formatted string representation of the envelope with the
    /// specified options.
    /// </summary>
    public string TreeFormatOpt(TreeFormatOpts opts)
    {
        var elements = new List<TreeElement>();
        Walk(opts.HideNodes, default(object?), (envelope, level, incomingEdge, _) =>
        {
            var elem = new TreeElement(
                level,
                envelope,
                incomingEdge,
                showId: !opts.HideNodes,
                isHighlighted: opts.HighlightingTarget.Contains(envelope.GetDigest()));
            elements.Add(elem);
            return ((object?)null, false);
        });

        string FormatElements(IReadOnlyList<TreeElement> elems, FormatContext ctx)
        {
            return string.Join("\n", elems.Select(e => e.ToFormattedString(ctx, opts.DigestDisplay)));
        }

        return opts.Context switch
        {
            FormatContextOpt.NoneOpt =>
                FormatElements(elements, new FormatContext()),
            FormatContextOpt.GlobalOpt =>
                GlobalFormatContext.WithFormatContext(ctx => FormatElements(elements, ctx)),
            FormatContextOpt.CustomOpt custom =>
                FormatElements(elements, custom.Context),
            _ => throw new InvalidOperationException(),
        };
    }

    /// <summary>
    /// Returns a tree-formatted string for this envelope with the given target
    /// digests highlighted.
    /// </summary>
    public string TreeFormatWithTarget(HashSet<Digest> target)
    {
        return TreeFormatOpt(new TreeFormatOpts(highlightingTarget: target));
    }

    /// <summary>
    /// Returns a text representation of the envelope's digest.
    /// </summary>
    public string ShortId(DigestDisplayFormat format = DigestDisplayFormat.Short)
    {
        return format switch
        {
            DigestDisplayFormat.Short => GetDigest().ShortDescription(),
            DigestDisplayFormat.Full => GetDigest().Hex,
            DigestDisplayFormat.UR => GetDigest().ToURString(),
            _ => GetDigest().ShortDescription(),
        };
    }
}

/// <summary>
/// An element in the tree representation of an envelope.
/// </summary>
internal sealed class TreeElement
{
    public int Level { get; }
    public Envelope Envelope { get; }
    public EdgeType IncomingEdge { get; }
    public bool ShowId { get; }
    public bool IsHighlighted { get; }

    public TreeElement(
        int level,
        Envelope envelope,
        EdgeType incomingEdge,
        bool showId,
        bool isHighlighted)
    {
        Level = level;
        Envelope = envelope;
        IncomingEdge = incomingEdge;
        ShowId = showId;
        IsHighlighted = isHighlighted;
    }

    public string ToFormattedString(FormatContext context, DigestDisplayFormat digestDisplay)
    {
        var parts = new List<string>();
        if (IsHighlighted)
            parts.Add("*");
        if (ShowId)
            parts.Add(Envelope.ShortId(digestDisplay));
        var label = IncomingEdge.Label();
        if (label is not null)
            parts.Add(label);
        parts.Add(Envelope.Summary(40, context));
        var line = string.Join(" ", parts);
        var indent = new string(' ', Level * 4);
        return indent + line;
    }
}

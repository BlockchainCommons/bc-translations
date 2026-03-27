using System.Text;
using BlockchainCommons.BCComponents;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Mermaid diagram format methods (partial class on Envelope).
/// </summary>
public partial class Envelope
{
    /// <summary>
    /// Returns a Mermaid flowchart for this envelope with default options.
    /// </summary>
    public string MermaidFormat()
    {
        return MermaidFormatOpt(new MermaidFormatOpts());
    }

    /// <summary>
    /// Returns a Mermaid flowchart for this envelope with the given options.
    /// </summary>
    public string MermaidFormatOpt(MermaidFormatOpts opts)
    {
        var elements = new List<MermaidElement>();
        int nextId = 0;
        Walk(opts.HideNodes, default(MermaidElement?), (envelope, level, incomingEdge, parent) =>
        {
            int id = nextId++;
            var elem = new MermaidElement(
                id, level, envelope, incomingEdge,
                showId: !opts.HideNodes,
                isHighlighted: opts.HighlightingTarget.Contains(envelope.GetDigest()),
                parent: parent);
            elements.Add(elem);
            return (elem, false);
        });

        var elementIds = new HashSet<int>(elements.Select(e => e.Id));

        var lines = new List<string>
        {
            $"%%{{ init: {{ 'theme': '{opts.Theme.ToMermaidName()}', 'flowchart': {{ 'curve': 'basis' }} }} }}%%",
            $"graph {opts.Orientation.ToMermaidCode()}",
        };

        var nodeStyles = new List<string>();
        var linkStyles = new List<string>();
        int linkIndex = 0;

        foreach (var element in elements)
        {
            var indent = new string(' ', element.Level * 4);
            string content;

            if (element.Parent is not null)
            {
                var thisLinkStyles = new List<string>();
                if (!opts.Monochrome)
                {
                    var color = element.IncomingEdge.LinkStrokeColor();
                    if (color is not null)
                        thisLinkStyles.Add($"stroke:{color}");
                }
                if (element.IsHighlighted && element.Parent.IsHighlighted)
                    thisLinkStyles.Add("stroke-width:4px");
                else
                    thisLinkStyles.Add("stroke-width:2px");

                if (thisLinkStyles.Count > 0)
                    linkStyles.Add($"linkStyle {linkIndex} {string.Join(",", thisLinkStyles)}");

                linkIndex++;
                content = element.FormatEdge(elementIds);
            }
            else
            {
                content = element.FormatNode(elementIds);
            }

            var thisNodeStyles = new List<string>();
            if (!opts.Monochrome)
                thisNodeStyles.Add($"stroke:{element.Envelope.NodeColor()}");
            if (element.IsHighlighted)
                thisNodeStyles.Add("stroke-width:6px");
            else
                thisNodeStyles.Add("stroke-width:4px");

            if (thisNodeStyles.Count > 0)
                nodeStyles.Add($"style {element.Id} {string.Join(",", thisNodeStyles)}");

            lines.Add($"{indent}{content}");
        }

        lines.AddRange(nodeStyles);
        lines.AddRange(linkStyles);

        return string.Join("\n", lines);
    }

    /// <summary>Returns the Mermaid node frame delimiters for this envelope's case.</summary>
    internal (string Left, string Right) MermaidFrame()
    {
        return Case switch
        {
            EnvelopeCase.NodeCase => ("((", "))"),
            EnvelopeCase.LeafCase => ("[", "]"),
            EnvelopeCase.WrappedCase => ("[/", "\\]"),
            EnvelopeCase.AssertionCase => ("([", "])"),
            EnvelopeCase.ElidedCase => ("{{", "}}"),
            EnvelopeCase.KnownValueCase => ("[/", "/]"),
            EnvelopeCase.EncryptedCase => (">", "]"),
            EnvelopeCase.CompressedCase => ("[[", "]]"),
            _ => ("[", "]"),
        };
    }

    /// <summary>Returns the Mermaid node stroke color for this envelope's case.</summary>
    internal string NodeColor()
    {
        return Case switch
        {
            EnvelopeCase.NodeCase => "red",
            EnvelopeCase.LeafCase => "teal",
            EnvelopeCase.WrappedCase => "blue",
            EnvelopeCase.AssertionCase => "green",
            EnvelopeCase.ElidedCase => "gray",
            EnvelopeCase.KnownValueCase => "goldenrod",
            EnvelopeCase.EncryptedCase => "coral",
            EnvelopeCase.CompressedCase => "purple",
            _ => "teal",
        };
    }
}

/// <summary>
/// An element in the Mermaid flowchart representation.
/// </summary>
internal sealed class MermaidElement
{
    public int Id { get; }
    public int Level { get; }
    public Envelope Envelope { get; }
    public EdgeType IncomingEdge { get; }
    public bool ShowId { get; }
    public bool IsHighlighted { get; }
    public MermaidElement? Parent { get; }

    public MermaidElement(
        int id,
        int level,
        Envelope envelope,
        EdgeType incomingEdge,
        bool showId,
        bool isHighlighted,
        MermaidElement? parent)
    {
        Id = id;
        Level = level;
        Envelope = envelope;
        IncomingEdge = incomingEdge;
        ShowId = showId;
        IsHighlighted = isHighlighted;
        Parent = parent;
    }

    public string FormatNode(HashSet<int> elementIds)
    {
        if (elementIds.Contains(Id))
        {
            elementIds.Remove(Id);
            var lines = new List<string>();
            var summary = GlobalFormatContext.WithFormatContext(ctx =>
                Envelope.Summary(20, ctx).Replace("\"", "&quot;"));
            lines.Add(summary);
            if (ShowId)
                lines.Add(Envelope.GetDigest().ShortDescription());
            var joinedLines = string.Join("<br>", lines);
            var (frameL, frameR) = Envelope.MermaidFrame();
            return $"{Id}{frameL}\"{joinedLines}\"{frameR}";
        }
        return Id.ToString();
    }

    public string FormatEdge(HashSet<int> elementIds)
    {
        var parentElement = Parent!;
        var label = IncomingEdge.Label();
        var arrow = label is not null ? $"-- {label} -->" : "-->";
        return $"{parentElement.FormatNode(elementIds)} {arrow} {FormatNode(elementIds)}";
    }
}

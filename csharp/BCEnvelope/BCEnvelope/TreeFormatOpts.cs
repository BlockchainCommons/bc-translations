using BlockchainCommons.BCComponents;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Options for tree-formatted output of an envelope.
/// </summary>
public sealed class TreeFormatOpts
{
    /// <summary>Whether to hide NODE identifiers in the tree representation.</summary>
    public bool HideNodes { get; }

    /// <summary>The set of digests to highlight in the tree representation.</summary>
    public HashSet<Digest> HighlightingTarget { get; }

    /// <summary>The formatting context.</summary>
    public FormatContextOpt Context { get; }

    /// <summary>The digest display format.</summary>
    public DigestDisplayFormat DigestDisplay { get; }

    /// <summary>
    /// Creates new tree format options.
    /// </summary>
    public TreeFormatOpts(
        bool hideNodes = false,
        HashSet<Digest>? highlightingTarget = null,
        FormatContextOpt? context = null,
        DigestDisplayFormat digestDisplay = DigestDisplayFormat.Short)
    {
        HideNodes = hideNodes;
        HighlightingTarget = highlightingTarget ?? [];
        Context = context ?? FormatContextOpt.Global;
        DigestDisplay = digestDisplay;
    }
}

using BlockchainCommons.BCComponents;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Orientation for Mermaid diagram layout.
/// </summary>
public enum MermaidOrientation
{
    /// <summary>Left to right layout.</summary>
    LeftToRight,

    /// <summary>Top to bottom layout.</summary>
    TopToBottom,

    /// <summary>Right to left layout.</summary>
    RightToLeft,

    /// <summary>Bottom to top layout.</summary>
    BottomToTop,
}

/// <summary>
/// Extension methods for <see cref="MermaidOrientation"/>.
/// </summary>
public static class MermaidOrientationExtensions
{
    /// <summary>Returns the Mermaid flowchart direction code.</summary>
    public static string ToMermaidCode(this MermaidOrientation orientation) => orientation switch
    {
        MermaidOrientation.LeftToRight => "LR",
        MermaidOrientation.TopToBottom => "TB",
        MermaidOrientation.RightToLeft => "RL",
        MermaidOrientation.BottomToTop => "BT",
        _ => "LR",
    };
}

/// <summary>
/// Theme for Mermaid diagrams.
/// </summary>
public enum MermaidTheme
{
    /// <summary>Default theme.</summary>
    Default,

    /// <summary>Neutral theme.</summary>
    Neutral,

    /// <summary>Dark theme.</summary>
    Dark,

    /// <summary>Forest theme.</summary>
    Forest,

    /// <summary>Base theme.</summary>
    Base,
}

/// <summary>
/// Extension methods for <see cref="MermaidTheme"/>.
/// </summary>
public static class MermaidThemeExtensions
{
    /// <summary>Returns the Mermaid theme name.</summary>
    public static string ToMermaidName(this MermaidTheme theme) => theme switch
    {
        MermaidTheme.Default => "default",
        MermaidTheme.Neutral => "neutral",
        MermaidTheme.Dark => "dark",
        MermaidTheme.Forest => "forest",
        MermaidTheme.Base => "base",
        _ => "default",
    };
}

/// <summary>
/// Options for Mermaid diagram output.
/// </summary>
public sealed class MermaidFormatOpts
{
    /// <summary>Whether to hide NODE identifiers.</summary>
    public bool HideNodes { get; }

    /// <summary>When true, uses a monochrome color scheme.</summary>
    public bool Monochrome { get; }

    /// <summary>The Mermaid theme.</summary>
    public MermaidTheme Theme { get; }

    /// <summary>The diagram orientation.</summary>
    public MermaidOrientation Orientation { get; }

    /// <summary>The set of digests to highlight.</summary>
    public HashSet<Digest> HighlightingTarget { get; }

    /// <summary>The formatting context.</summary>
    public FormatContextOpt Context { get; }

    /// <summary>
    /// Creates new Mermaid format options.
    /// </summary>
    public MermaidFormatOpts(
        bool hideNodes = false,
        bool monochrome = false,
        MermaidTheme theme = MermaidTheme.Default,
        MermaidOrientation orientation = MermaidOrientation.LeftToRight,
        HashSet<Digest>? highlightingTarget = null,
        FormatContextOpt? context = null)
    {
        HideNodes = hideNodes;
        Monochrome = monochrome;
        Theme = theme;
        Orientation = orientation;
        HighlightingTarget = highlightingTarget ?? [];
        Context = context ?? FormatContextOpt.Global;
    }
}

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// The type of incoming edge provided to the visitor during envelope traversal.
/// </summary>
/// <remarks>
/// Each edge type represents a specific relationship within the envelope
/// structure during a walk operation. This identifies how an envelope element
/// is connected to its parent in the hierarchy.
/// </remarks>
public enum EdgeType
{
    /// <summary>No incoming edge (root).</summary>
    None,

    /// <summary>Element is the subject of a node.</summary>
    Subject,

    /// <summary>Element is an assertion on a node.</summary>
    Assertion,

    /// <summary>Element is the predicate of an assertion.</summary>
    Predicate,

    /// <summary>Element is the object of an assertion.</summary>
    Object,

    /// <summary>Element is the content wrapped by another envelope.</summary>
    Content,
}

/// <summary>
/// Extension methods for <see cref="EdgeType"/>.
/// </summary>
public static class EdgeTypeExtensions
{
    /// <summary>
    /// Returns a short text label for the edge type used in tree formatting,
    /// or <c>null</c> if no label is needed.
    /// </summary>
    /// <param name="edgeType">The edge type.</param>
    /// <returns>A short label string, or <c>null</c>.</returns>
    public static string? Label(this EdgeType edgeType) => edgeType switch
    {
        EdgeType.Subject => "subj",
        EdgeType.Content => "cont",
        EdgeType.Predicate => "pred",
        EdgeType.Object => "obj",
        _ => null,
    };

    /// <summary>
    /// Returns the stroke color for this edge type in Mermaid diagrams,
    /// or <c>null</c> if no specific color is assigned.
    /// </summary>
    public static string? LinkStrokeColor(this EdgeType edgeType) => edgeType switch
    {
        EdgeType.Subject => "red",
        EdgeType.Content => "blue",
        EdgeType.Predicate => "cyan",
        EdgeType.Object => "magenta",
        _ => null,
    };
}

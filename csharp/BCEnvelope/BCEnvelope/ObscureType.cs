namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// The type of obscuration applied to an envelope element.
/// </summary>
/// <remarks>
/// Used by <see cref="Envelope.NodesMatching"/> to filter for elements
/// obscured in a particular way.
/// </remarks>
public enum ObscureType
{
    /// <summary>The element has been elided (replaced by its digest).</summary>
    Elided,

    /// <summary>The element has been encrypted.</summary>
    Encrypted,

    /// <summary>The element has been compressed.</summary>
    Compressed,
}

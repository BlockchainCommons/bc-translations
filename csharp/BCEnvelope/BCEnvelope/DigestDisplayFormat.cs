namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Format for displaying digests in the tree representation.
/// </summary>
public enum DigestDisplayFormat
{
    /// <summary>Display a shortened version of the digest (first 8 characters).</summary>
    Short,

    /// <summary>Display the full digest for each element in the tree.</summary>
    Full,

    /// <summary>Display a ur:digest UR for each element in the tree.</summary>
    UR,
}

namespace BlockchainCommons.DCbor;

/// <summary>
/// Defines the CBOR tags associated with a type.
/// The first tag in the list is the "preferred" tag used for encoding;
/// all tags are accepted during decoding (for backward compatibility).
/// </summary>
public interface ICborTagged
{
    /// <summary>
    /// Returns the CBOR tags associated with this type.
    /// The first tag is used for encoding; all are accepted for decoding.
    /// </summary>
    static abstract IReadOnlyList<Tag> CborTags { get; }
}

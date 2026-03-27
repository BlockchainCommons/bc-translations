using BlockchainCommons.BCComponents;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// A container for vendor-specific metadata attachments.
/// </summary>
/// <remarks>
/// <see cref="Attachments"/> provides a flexible mechanism for attaching arbitrary
/// metadata to envelopes without modifying their core structure.
/// </remarks>
public sealed class Attachments : IEquatable<Attachments>
{
    private readonly Dictionary<Digest, Envelope> _envelopes = new();

    /// <summary>
    /// Creates a new empty attachments container.
    /// </summary>
    public Attachments() { }

    /// <summary>
    /// Adds a new attachment with the specified payload and metadata.
    /// </summary>
    /// <param name="payload">The data to attach.</param>
    /// <param name="vendor">A string identifying the vendor.</param>
    /// <param name="conformsTo">An optional URI identifying the format.</param>
    public void Add(object payload, string vendor, string? conformsTo = null)
    {
        var attachment = Envelope.NewAttachment(payload, vendor, conformsTo);
        _envelopes[attachment.GetDigest()] = attachment;
    }

    /// <summary>
    /// Retrieves an attachment by its digest.
    /// </summary>
    /// <param name="digest">The digest of the attachment to retrieve.</param>
    /// <returns>The envelope if found, or <c>null</c>.</returns>
    public Envelope? Get(Digest digest)
    {
        return _envelopes.TryGetValue(digest, out var envelope) ? envelope : null;
    }

    /// <summary>
    /// Removes an attachment by its digest.
    /// </summary>
    /// <param name="digest">The digest of the attachment to remove.</param>
    /// <returns>The removed envelope if found, or <c>null</c>.</returns>
    public Envelope? Remove(Digest digest)
    {
        if (_envelopes.TryGetValue(digest, out var envelope))
        {
            _envelopes.Remove(digest);
            return envelope;
        }
        return null;
    }

    /// <summary>
    /// Removes all attachments from the container.
    /// </summary>
    public void Clear() => _envelopes.Clear();

    /// <summary>
    /// Returns <c>true</c> if there are no attachments.
    /// </summary>
    public bool IsEmpty => _envelopes.Count == 0;

    /// <summary>
    /// Adds all attachments as assertions to the given envelope.
    /// </summary>
    /// <param name="envelope">The envelope to add attachments to.</param>
    /// <returns>A new envelope with all attachments added as assertions.</returns>
    public Envelope AddToEnvelope(Envelope envelope)
    {
        var result = envelope;
        foreach (var (_, attachment) in _envelopes)
        {
            result = result.AddAssertionEnvelope(attachment);
        }
        return result;
    }

    /// <summary>
    /// Extracts attachments from an envelope's attachment assertions.
    /// </summary>
    /// <param name="envelope">The envelope to extract attachments from.</param>
    /// <returns>An <see cref="Attachments"/> container with the extracted attachments.</returns>
    public static Attachments FromEnvelope(Envelope envelope)
    {
        var attachments = new Attachments();
        foreach (var attachment in envelope.Attachments())
        {
            attachments._envelopes[attachment.GetDigest()] = attachment;
        }
        return attachments;
    }

    /// <inheritdoc/>
    public bool Equals(Attachments? other)
    {
        if (other is null) return false;
        if (_envelopes.Count != other._envelopes.Count) return false;
        foreach (var (key, value) in _envelopes)
        {
            if (!other._envelopes.TryGetValue(key, out var otherValue))
                return false;
            if (!value.IsIdenticalTo(otherValue))
                return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Attachments a && Equals(a);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var key in _envelopes.Keys)
            hash.Add(key);
        return hash.ToHashCode();
    }
}

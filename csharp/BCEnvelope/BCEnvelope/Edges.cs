using BlockchainCommons.BCComponents;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// A container for edge envelopes on a document.
/// </summary>
/// <remarks>
/// <see cref="Edges"/> stores pre-constructed edge envelopes keyed by their digest,
/// mirroring the <see cref="Attachments"/> container but for edges as defined in
/// BCR-2026-003.
/// </remarks>
public sealed class Edges : IEquatable<Edges>
{
    private readonly Dictionary<Digest, Envelope> _envelopes = new();

    /// <summary>
    /// Creates a new empty edges container.
    /// </summary>
    public Edges() { }

    /// <summary>
    /// Adds a pre-constructed edge envelope.
    /// </summary>
    /// <param name="edgeEnvelope">The edge envelope to add.</param>
    public void Add(Envelope edgeEnvelope)
    {
        _envelopes[edgeEnvelope.GetDigest()] = edgeEnvelope;
    }

    /// <summary>
    /// Retrieves an edge by its digest.
    /// </summary>
    /// <param name="digest">The digest of the edge to retrieve.</param>
    /// <returns>The envelope if found, or <c>null</c>.</returns>
    public Envelope? Get(Digest digest)
    {
        return _envelopes.TryGetValue(digest, out var envelope) ? envelope : null;
    }

    /// <summary>
    /// Removes an edge by its digest.
    /// </summary>
    /// <param name="digest">The digest of the edge to remove.</param>
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
    /// Removes all edges from the container.
    /// </summary>
    public void Clear() => _envelopes.Clear();

    /// <summary>
    /// Returns <c>true</c> if there are no edges.
    /// </summary>
    public bool IsEmpty => _envelopes.Count == 0;

    /// <summary>
    /// Returns the number of edges in the container.
    /// </summary>
    public int Count => _envelopes.Count;

    /// <summary>
    /// Returns an enumerator over all edge envelopes.
    /// </summary>
    public IEnumerable<KeyValuePair<Digest, Envelope>> Entries => _envelopes;

    /// <summary>
    /// Adds all edges as <c>'edge'</c> assertion envelopes to the given envelope.
    /// </summary>
    /// <param name="envelope">The envelope to add edges to.</param>
    /// <returns>A new envelope with all edges added as assertions.</returns>
    public Envelope AddToEnvelope(Envelope envelope)
    {
        var result = envelope;
        foreach (var (_, edgeEnvelope) in _envelopes)
        {
            result = result.AddAssertion(KnownValuesRegistry.Edge, edgeEnvelope);
        }
        return result;
    }

    /// <summary>
    /// Extracts edges from an envelope's <c>'edge'</c> assertions.
    /// </summary>
    /// <param name="envelope">The envelope to extract edges from.</param>
    /// <returns>An <see cref="Edges"/> container with the extracted edges.</returns>
    public static Edges FromEnvelope(Envelope envelope)
    {
        var edges = new Edges();
        foreach (var edge in envelope.Edges())
        {
            edges._envelopes[edge.GetDigest()] = edge;
        }
        return edges;
    }

    /// <inheritdoc/>
    public bool Equals(Edges? other)
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
    public override bool Equals(object? obj) => obj is Edges e && Equals(e);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var key in _envelopes.Keys)
            hash.Add(key);
        return hash.ToHashCode();
    }
}

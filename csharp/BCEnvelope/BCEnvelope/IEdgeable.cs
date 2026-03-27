using BlockchainCommons.BCComponents;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Interface for types that can have edges.
/// </summary>
/// <remarks>
/// <see cref="IEdgeable"/> provides a consistent interface for working with edges.
/// Types implementing this interface can store and retrieve edge envelopes
/// representing verifiable claims as defined in BCR-2026-003.
/// </remarks>
public interface IEdgeable
{
    /// <summary>
    /// Returns a reference to the edges container.
    /// </summary>
    Edges EdgesContainer { get; }

    /// <summary>
    /// Adds a pre-constructed edge envelope.
    /// </summary>
    /// <param name="edgeEnvelope">The edge envelope to add.</param>
    void AddEdge(Envelope edgeEnvelope)
    {
        EdgesContainer.Add(edgeEnvelope);
    }

    /// <summary>
    /// Retrieves an edge by its digest.
    /// </summary>
    /// <param name="digest">The digest of the edge to retrieve.</param>
    /// <returns>The envelope if found, or <c>null</c>.</returns>
    Envelope? GetEdge(Digest digest)
    {
        return EdgesContainer.Get(digest);
    }

    /// <summary>
    /// Removes an edge by its digest.
    /// </summary>
    /// <param name="digest">The digest of the edge to remove.</param>
    /// <returns>The removed envelope if found, or <c>null</c>.</returns>
    Envelope? RemoveEdge(Digest digest)
    {
        return EdgesContainer.Remove(digest);
    }

    /// <summary>
    /// Removes all edges.
    /// </summary>
    void ClearEdges()
    {
        EdgesContainer.Clear();
    }

    /// <summary>
    /// Returns <c>true</c> if the object has any edges.
    /// </summary>
    bool HasEdges => !EdgesContainer.IsEmpty;
}

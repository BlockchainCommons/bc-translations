using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Edge support for Gordian Envelopes.
/// </summary>
/// <remarks>
/// Edges represent verifiable claims in an envelope graph structure
/// as defined in BCR-2026-003. An edge envelope must have exactly three
/// assertion predicates: <c>'isA'</c>, <c>'source'</c>, and <c>'target'</c>.
/// </remarks>
public partial class Envelope
{
    /// <summary>
    /// Returns a new envelope with an added <c>'edge': &lt;edge&gt;</c> assertion.
    /// </summary>
    /// <param name="edge">The edge envelope to add.</param>
    /// <returns>A new envelope with the edge assertion added.</returns>
    public Envelope AddEdgeEnvelope(Envelope edge)
    {
        return AddAssertion(KnownValuesRegistry.Edge, edge);
    }

    /// <summary>
    /// Returns all edge object envelopes (assertions with predicate <c>'edge'</c>).
    /// </summary>
    /// <returns>A list of edge envelopes.</returns>
    public List<Envelope> Edges()
    {
        return ObjectsForPredicate(KnownValuesRegistry.Edge);
    }

    /// <summary>
    /// Validates an edge envelope's structure per BCR-2026-003.
    /// </summary>
    /// <remarks>
    /// An edge may be wrapped (signed) or unwrapped. The inner envelope
    /// must have exactly three assertion predicates: <c>'isA'</c>, <c>'source'</c>,
    /// and <c>'target'</c>. No other assertions are permitted.
    /// </remarks>
    /// <exception cref="EnvelopeException">
    /// Thrown if the edge structure is invalid (missing, duplicate, or unexpected assertions).
    /// </exception>
    public void ValidateEdge()
    {
        var inner = Subject.IsWrapped ? Subject.TryUnwrap() : this;

        var seenIsA = false;
        var seenSource = false;
        var seenTarget = false;

        foreach (var assertion in inner.Assertions)
        {
            ulong predicate;
            try
            {
                predicate = assertion.TryPredicate().TryKnownValue().Value;
            }
            catch
            {
                throw EnvelopeException.EdgeUnexpectedAssertion();
            }

            if (predicate == KnownValuesRegistry.IsARaw)
            {
                if (seenIsA) throw EnvelopeException.EdgeDuplicateIsA();
                seenIsA = true;
            }
            else if (predicate == KnownValuesRegistry.SourceRaw)
            {
                if (seenSource) throw EnvelopeException.EdgeDuplicateSource();
                seenSource = true;
            }
            else if (predicate == KnownValuesRegistry.TargetRaw)
            {
                if (seenTarget) throw EnvelopeException.EdgeDuplicateTarget();
                seenTarget = true;
            }
            else
            {
                throw EnvelopeException.EdgeUnexpectedAssertion();
            }
        }

        if (!seenIsA) throw EnvelopeException.EdgeMissingIsA();
        if (!seenSource) throw EnvelopeException.EdgeMissingSource();
        if (!seenTarget) throw EnvelopeException.EdgeMissingTarget();
    }

    /// <summary>
    /// Extracts the <c>'isA'</c> assertion object from an edge envelope.
    /// </summary>
    /// <returns>The type envelope.</returns>
    public Envelope EdgeIsA()
    {
        var inner = Subject.IsWrapped ? Subject.TryUnwrap() : this;
        return inner.ObjectForPredicate(KnownValuesRegistry.IsA);
    }

    /// <summary>
    /// Extracts the <c>'source'</c> assertion object from an edge envelope.
    /// </summary>
    /// <returns>The source envelope.</returns>
    public Envelope EdgeSource()
    {
        var inner = Subject.IsWrapped ? Subject.TryUnwrap() : this;
        return inner.ObjectForPredicate(KnownValuesRegistry.Source);
    }

    /// <summary>
    /// Extracts the <c>'target'</c> assertion object from an edge envelope.
    /// </summary>
    /// <returns>The target envelope.</returns>
    public Envelope EdgeTarget()
    {
        var inner = Subject.IsWrapped ? Subject.TryUnwrap() : this;
        return inner.ObjectForPredicate(KnownValuesRegistry.Target);
    }

    /// <summary>
    /// Extracts the edge's subject identifier (the inner envelope's subject).
    /// </summary>
    /// <returns>The edge subject envelope.</returns>
    public Envelope EdgeSubject()
    {
        var inner = Subject.IsWrapped ? Subject.TryUnwrap() : this;
        return inner.Subject;
    }

    /// <summary>
    /// Filters edges by optional criteria.
    /// </summary>
    /// <remarks>
    /// Each parameter is optional. When provided, only edges matching
    /// all specified criteria are returned.
    /// </remarks>
    /// <param name="isA">Optional type filter.</param>
    /// <param name="source">Optional source filter.</param>
    /// <param name="target">Optional target filter.</param>
    /// <param name="subject">Optional subject filter.</param>
    /// <returns>A list of matching edge envelopes.</returns>
    public List<Envelope> EdgesMatching(
        Envelope? isA = null,
        Envelope? source = null,
        Envelope? target = null,
        Envelope? subject = null)
    {
        var allEdges = Edges();
        var matching = new List<Envelope>();

        foreach (var edge in allEdges)
        {
            if (isA != null)
            {
                try
                {
                    var edgeIsA = edge.EdgeIsA();
                    if (!edgeIsA.IsEquivalentTo(isA))
                        continue;
                }
                catch
                {
                    continue;
                }
            }

            if (source != null)
            {
                try
                {
                    var edgeSource = edge.EdgeSource();
                    if (!edgeSource.IsEquivalentTo(source))
                        continue;
                }
                catch
                {
                    continue;
                }
            }

            if (target != null)
            {
                try
                {
                    var edgeTarget = edge.EdgeTarget();
                    if (!edgeTarget.IsEquivalentTo(target))
                        continue;
                }
                catch
                {
                    continue;
                }
            }

            if (subject != null)
            {
                try
                {
                    var edgeSubject = edge.EdgeSubject();
                    if (!edgeSubject.IsEquivalentTo(subject))
                        continue;
                }
                catch
                {
                    continue;
                }
            }

            matching.Add(edge);
        }

        return matching;
    }
}

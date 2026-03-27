using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Type system extensions for Gordian Envelopes.
/// </summary>
/// <remarks>
/// Types are represented as assertions with the <c>'isA'</c> predicate (known value 1).
/// This provides semantic classification, type verification, and conversion
/// between domain objects and envelopes.
/// </remarks>
public partial class Envelope
{
    /// <summary>
    /// Adds a type assertion using the <c>'isA'</c> predicate.
    /// </summary>
    /// <param name="type">The type to assign to this envelope.</param>
    /// <returns>A new envelope with the type assertion added.</returns>
    public Envelope AddType(object type)
    {
        return AddAssertion(KnownValuesRegistry.IsA, type);
    }

    /// <summary>
    /// Returns all type objects from <c>'isA'</c> assertions.
    /// </summary>
    /// <returns>A list of envelopes, each representing a type assigned to this envelope.</returns>
    public List<Envelope> Types()
    {
        return ObjectsForPredicate(KnownValuesRegistry.IsA);
    }

    /// <summary>
    /// Returns the single type object, or throws if zero or multiple types exist.
    /// </summary>
    /// <returns>The single type envelope.</returns>
    /// <exception cref="EnvelopeException">Thrown if multiple types exist or no type is set.</exception>
    public Envelope GetEnvelopeType()
    {
        var t = Types();
        if (t.Count == 1)
            return t[0];
        throw EnvelopeException.AmbiguousType();
    }

    /// <summary>
    /// Returns <c>true</c> if the envelope has the given type.
    /// </summary>
    /// <param name="type">The type to check for.</param>
    /// <returns><c>true</c> if the envelope has the specified type.</returns>
    public bool HasType(object type)
    {
        var e = Envelope.Create(type);
        return Types().Any(x => x.GetDigest() == e.GetDigest());
    }

    /// <summary>
    /// Returns <c>true</c> if the envelope has the given <see cref="KnownValue"/> type.
    /// </summary>
    /// <param name="type">The known value type to check for.</param>
    /// <returns><c>true</c> if the envelope has the specified known value type.</returns>
    public bool HasTypeValue(KnownValue type)
    {
        var typeEnvelope = Envelope.Create(type);
        return Types().Any(x => x.GetDigest() == typeEnvelope.GetDigest());
    }

    /// <summary>
    /// Verifies that the envelope has the given <see cref="KnownValue"/> type.
    /// </summary>
    /// <param name="type">The known value type to verify.</param>
    /// <exception cref="EnvelopeException">Thrown if the envelope does not have the specified type.</exception>
    public void CheckTypeValue(KnownValue type)
    {
        if (!HasTypeValue(type))
            throw EnvelopeException.InvalidType();
    }

    /// <summary>
    /// Verifies that the envelope has the given type.
    /// </summary>
    /// <param name="type">The type to verify.</param>
    /// <exception cref="EnvelopeException">Thrown if the envelope does not have the specified type.</exception>
    public void CheckType(object type)
    {
        if (!HasType(type))
            throw EnvelopeException.InvalidType();
    }
}

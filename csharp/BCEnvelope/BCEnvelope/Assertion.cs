using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// A predicate-object relationship representing an assertion about a subject.
/// </summary>
/// <remarks>
/// <para>
/// In Gordian Envelope, assertions are the basic building blocks for attaching
/// information to a subject. An assertion consists of a predicate (what is being
/// asserted) and an object (the value of the assertion).
/// </para>
/// <para>
/// Assertions are equivalent to RDF triples, where the envelope's subject is
/// the subject of the triple, the assertion's predicate is the predicate of the
/// triple, and the assertion's object is the object of the triple.
/// </para>
/// <para>
/// Generally you do not create instances directly, but instead use
/// <see cref="Envelope.CreateAssertion"/> or the assertion management methods
/// on <see cref="Envelope"/>.
/// </para>
/// </remarks>
public sealed partial class Assertion : IEquatable<Assertion>, IDigestProvider
{
    private readonly Envelope _predicate;
    private readonly Envelope _object;
    private readonly Digest _digest;

    /// <summary>
    /// Creates a new assertion with the given predicate and object.
    /// </summary>
    /// <param name="predicate">The predicate envelope.</param>
    /// <param name="object">The object envelope.</param>
    public Assertion(Envelope predicate, Envelope @object)
    {
        _predicate = predicate;
        _object = @object;
        _digest = Digest.FromDigests([predicate.GetDigest(), @object.GetDigest()]);
    }

    /// <summary>
    /// Creates a new assertion with the given predicate and object, converting
    /// each to envelopes.
    /// </summary>
    /// <param name="predicate">The predicate value.</param>
    /// <param name="object">The object value.</param>
    public Assertion(Envelope predicate, object @object)
        : this(predicate, EnvelopeExtensions.ToEnvelope(@object))
    {
    }

    /// <summary>Gets the predicate of the assertion.</summary>
    public Envelope Predicate => _predicate;

    /// <summary>Gets the object of the assertion.</summary>
    public Envelope Object => _object;

    /// <summary>Returns the assertion's digest.</summary>
    public Digest GetDigest() => _digest;

    /// <summary>
    /// Converts this assertion to its CBOR representation: a single-entry map.
    /// </summary>
    public Cbor ToCbor(Func<Envelope, Cbor> untaggedCborFunc)
    {
        var map = new CborMap();
        map.Insert(untaggedCborFunc(_predicate), untaggedCborFunc(_object));
        return new Cbor(CborCase.Map(map));
    }

    /// <summary>
    /// Attempts to decode an assertion from a CBOR map.
    /// </summary>
    /// <param name="map">A CBOR map with exactly one entry.</param>
    /// <param name="fromUntaggedCbor">A function to decode an envelope from untagged CBOR.</param>
    /// <returns>A new assertion.</returns>
    /// <exception cref="EnvelopeException">Thrown if the map does not have exactly one entry.</exception>
    public static Assertion FromCborMap(CborMap map, Func<Cbor, Envelope> fromUntaggedCbor)
    {
        if (map.Count != 1)
            throw EnvelopeException.InvalidAssertion();

        Cbor? firstKey = null;
        Cbor? firstValue = null;
        foreach (var (key, value) in map)
        {
            firstKey = key;
            firstValue = value;
            break;
        }

        var predicate = fromUntaggedCbor(firstKey!);
        var @object = fromUntaggedCbor(firstValue!);
        return new Assertion(predicate, @object);
    }

    /// <summary>
    /// Attempts to decode an assertion from a CBOR value.
    /// </summary>
    /// <param name="cbor">A CBOR value that must be a single-entry map.</param>
    /// <param name="fromUntaggedCbor">A function to decode an envelope from untagged CBOR.</param>
    /// <returns>A new assertion.</returns>
    /// <exception cref="EnvelopeException">Thrown if the CBOR is not a valid assertion map.</exception>
    public static Assertion FromCbor(Cbor cbor, Func<Cbor, Envelope> fromUntaggedCbor)
    {
        if (cbor.Case is CborCase.MapCase mapCase)
            return FromCborMap(mapCase.Value, fromUntaggedCbor);
        throw EnvelopeException.InvalidAssertion();
    }

    // --- IEquatable<Assertion> ---

    /// <summary>
    /// Two assertions are equal if they have the same digest.
    /// </summary>
    public bool Equals(Assertion? other)
    {
        if (other is null) return false;
        return _digest == other._digest;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Assertion a && Equals(a);

    /// <inheritdoc />
    public override int GetHashCode() => _digest.GetHashCode();

    /// <summary>Tests equality of two assertions.</summary>
    public static bool operator ==(Assertion? left, Assertion? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>Tests inequality of two assertions.</summary>
    public static bool operator !=(Assertion? left, Assertion? right) => !(left == right);
}

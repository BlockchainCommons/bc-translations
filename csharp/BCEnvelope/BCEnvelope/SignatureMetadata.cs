namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Metadata associated with a signature in a Gordian Envelope.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SignatureMetadata"/> provides a way to attach additional information
/// to signatures, such as the signer's identity, the signing date, or the purpose
/// of the signature. When used with the signature extension, this metadata is
/// included in a structured way that is also signed, ensuring the metadata
/// cannot be tampered with without invalidating the signature.
/// </para>
/// <para>
/// The metadata is represented as a collection of assertions that are attached
/// to the signature envelope and then themselves signed with the same key.
/// </para>
/// </remarks>
public sealed class SignatureMetadata
{
    private readonly List<Assertion> _assertions;

    /// <summary>
    /// Creates a new, empty <see cref="SignatureMetadata"/> instance.
    /// </summary>
    public SignatureMetadata()
    {
        _assertions = new List<Assertion>();
    }

    /// <summary>
    /// Creates a new <see cref="SignatureMetadata"/> with the specified assertions.
    /// </summary>
    /// <param name="assertions">The assertions to include.</param>
    public SignatureMetadata(IEnumerable<Assertion> assertions)
    {
        _assertions = new List<Assertion>(assertions);
    }

    /// <summary>Gets the assertions contained in this metadata.</summary>
    public IReadOnlyList<Assertion> Assertions => _assertions;

    /// <summary>Gets whether this metadata contains any assertions.</summary>
    public bool HasAssertions => _assertions.Count > 0;

    /// <summary>
    /// Adds an assertion to this metadata. Returns this instance for chaining.
    /// </summary>
    /// <param name="assertion">The assertion to add.</param>
    /// <returns>This <see cref="SignatureMetadata"/> instance.</returns>
    public SignatureMetadata AddAssertion(Assertion assertion)
    {
        _assertions.Add(assertion);
        return this;
    }

    /// <summary>
    /// Adds a new assertion with the provided predicate and object.
    /// Returns this instance for chaining.
    /// </summary>
    /// <param name="predicate">The predicate for the assertion.</param>
    /// <param name="object">The object for the assertion.</param>
    /// <returns>This <see cref="SignatureMetadata"/> instance.</returns>
    public SignatureMetadata WithAssertion(object predicate, object @object)
    {
        var predicateEnvelope = EnvelopeExtensions.ToEnvelope(predicate);
        var objectEnvelope = EnvelopeExtensions.ToEnvelope(@object);
        _assertions.Add(new Assertion(predicateEnvelope, objectEnvelope));
        return this;
    }
}

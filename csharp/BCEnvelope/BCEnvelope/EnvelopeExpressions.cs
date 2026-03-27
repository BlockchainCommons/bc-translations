using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Expression-related extension methods for Gordian Envelope.
/// </summary>
public sealed partial class Envelope
{
    /// <summary>
    /// Adds an assertion only if the given condition is true.
    /// </summary>
    /// <param name="condition">Whether to add the assertion.</param>
    /// <param name="predicate">The assertion predicate.</param>
    /// <param name="object">The assertion object.</param>
    /// <returns>This envelope with the assertion added if the condition is true, or unchanged otherwise.</returns>
    public Envelope AddAssertionIf(bool condition, object predicate, object @object)
    {
        return condition ? AddAssertion(predicate, @object) : this;
    }

    /// <summary>
    /// Creates an envelope containing the 'OK' known value.
    /// </summary>
    /// <remarks>
    /// Used when a response doesn't need to return any specific value,
    /// just an acknowledgment that the request was successful.
    /// </remarks>
    public static Envelope Ok() => KnownValuesRegistry.OkValue.ToEnvelope();

    /// <summary>
    /// Creates an envelope containing the 'Unknown' known value.
    /// </summary>
    /// <remarks>
    /// Used when representing an unknown error or value.
    /// </remarks>
    public static Envelope Unknown() => KnownValuesRegistry.UnknownValue.ToEnvelope();
}

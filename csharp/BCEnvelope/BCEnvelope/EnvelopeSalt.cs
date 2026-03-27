using BlockchainCommons.BCComponents;
using BlockchainCommons.BCRand;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Salt (decorrelation) operations for Gordian Envelopes.
/// </summary>
/// <remarks>
/// Salt is added as an assertion with the known value predicate 'salt' and
/// a random value. When an envelope is elided, this salt ensures that the
/// digest of the elided envelope cannot be correlated with other elided
/// envelopes containing the same information.
/// </remarks>
public partial class Envelope
{
    /// <summary>
    /// Adds a proportionally-sized salt assertion to decorrelate the envelope.
    /// </summary>
    /// <remarks>
    /// The salt size is proportional to the envelope's serialized size:
    /// roughly 5-25% of the envelope size, with a minimum of 8 bytes.
    /// </remarks>
    /// <returns>A new envelope with the salt assertion added.</returns>
    public Envelope AddSalt()
        => AddSaltUsing(SecureRandomNumberGenerator.Shared);

    /// <summary>
    /// Adds a proportionally-sized salt assertion using the given RNG.
    /// </summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A new envelope with the salt assertion added.</returns>
    public Envelope AddSaltUsing(IRandomNumberGenerator rng)
    {
        var size = TaggedCbor().ToCborData().Length;
        var salt = Salt.CreateForSizeUsing(size, rng);
        return AddSaltInstance(salt);
    }

    /// <summary>
    /// Adds the given <see cref="Salt"/> as an assertion to the envelope.
    /// </summary>
    /// <param name="salt">The salt to add.</param>
    /// <returns>A new envelope with the salt assertion added.</returns>
    public Envelope AddSaltInstance(Salt salt)
        => AddAssertion(KnownValuesRegistry.Salt, salt);

    /// <summary>
    /// Adds salt of a specific byte length to the envelope.
    /// </summary>
    /// <param name="count">The number of salt bytes. Must be at least 8.</param>
    /// <returns>A new envelope with the salt assertion added.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if <paramref name="count"/> is less than 8.
    /// </exception>
    public Envelope AddSaltWithLength(int count)
        => AddSaltWithLengthUsing(count, SecureRandomNumberGenerator.Shared);

    /// <summary>
    /// Adds salt of a specific byte length using the given RNG.
    /// </summary>
    /// <param name="count">The number of salt bytes. Must be at least 8.</param>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A new envelope with the salt assertion added.</returns>
    public Envelope AddSaltWithLengthUsing(int count, IRandomNumberGenerator rng)
    {
        var salt = Salt.CreateWithLengthUsing(count, rng);
        return AddSaltInstance(salt);
    }

    /// <summary>
    /// Adds salt with a byte length randomly chosen from the given range.
    /// </summary>
    /// <param name="min">The minimum salt length (inclusive). Must be at least 8.</param>
    /// <param name="max">The maximum salt length (inclusive).</param>
    /// <returns>A new envelope with the salt assertion added.</returns>
    /// <exception cref="BCComponentsException">
    /// Thrown if <paramref name="min"/> is less than 8.
    /// </exception>
    public Envelope AddSaltInRange(int min, int max)
        => AddSaltInRangeUsing(min, max, SecureRandomNumberGenerator.Shared);

    /// <summary>
    /// Adds salt with a byte length randomly chosen from the given range using the given RNG.
    /// </summary>
    /// <param name="min">The minimum salt length (inclusive). Must be at least 8.</param>
    /// <param name="max">The maximum salt length (inclusive).</param>
    /// <param name="rng">The random number generator to use.</param>
    /// <returns>A new envelope with the salt assertion added.</returns>
    public Envelope AddSaltInRangeUsing(int min, int max, IRandomNumberGenerator rng)
    {
        var salt = Salt.CreateInRangeUsing(min, max, rng);
        return AddSaltInstance(salt);
    }
}

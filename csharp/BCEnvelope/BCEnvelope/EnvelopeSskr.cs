using BlockchainCommons.BCComponents;
using BlockchainCommons.BCRand;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// SSKR extension for Gordian Envelopes.
/// </summary>
/// <remarks>
/// Provides methods for splitting and combining envelopes using Sharded
/// Secret Key Reconstruction (SSKR), which is an implementation of
/// Shamir's Secret Sharing. SSKR allows splitting a symmetric encryption
/// key into multiple shares, with a threshold required for reconstruction.
/// </remarks>
public partial class Envelope
{
    /// <summary>
    /// Adds an <c>sskrShare: SSKRShare</c> assertion to the envelope.
    /// </summary>
    private Envelope AddSskrShare(SSKRShare share)
    {
        return AddAssertion(KnownValuesRegistry.SSKRShare, share);
    }

    /// <summary>
    /// Splits the envelope into SSKR shares grouped by SSKR groups.
    /// </summary>
    /// <remarks>
    /// The returned structure preserves the group structure of the SSKR shares.
    /// Each outer list represents a group, and each inner list contains the
    /// share envelopes for that group.
    /// </remarks>
    /// <param name="spec">The SSKR specification defining groups and thresholds.</param>
    /// <param name="contentKey">The symmetric key used to encrypt the envelope.</param>
    /// <returns>A nested list of envelopes organized by groups.</returns>
    public List<List<Envelope>> SskrSplit(
        BlockchainCommons.SSKR.Spec spec,
        SymmetricKey contentKey)
    {
        var rng = SecureRandomNumberGenerator.Shared;
        return SskrSplitUsing(spec, contentKey, rng);
    }

    /// <summary>
    /// Splits the envelope into a flattened list of SSKR shares.
    /// </summary>
    /// <param name="spec">The SSKR specification defining groups and thresholds.</param>
    /// <param name="contentKey">The symmetric key used to encrypt the envelope.</param>
    /// <returns>A flat list of all share envelopes.</returns>
    public List<Envelope> SskrSplitFlattened(
        BlockchainCommons.SSKR.Spec spec,
        SymmetricKey contentKey)
    {
        return SskrSplit(spec, contentKey)
            .SelectMany(group => group)
            .ToList();
    }

    /// <summary>
    /// Splits the envelope into SSKR shares using a provided random number generator.
    /// </summary>
    /// <remarks>
    /// This method is primarily used for testing to ensure deterministic SSKR shares.
    /// </remarks>
    /// <param name="spec">The SSKR specification defining groups and thresholds.</param>
    /// <param name="contentKey">The symmetric key used to encrypt the envelope.</param>
    /// <param name="testRng">The random number generator to use.</param>
    /// <returns>A nested list of envelopes organized by groups.</returns>
    internal List<List<Envelope>> SskrSplitUsing(
        BlockchainCommons.SSKR.Spec spec,
        SymmetricKey contentKey,
        IRandomNumberGenerator testRng)
    {
        var masterSecret = BlockchainCommons.SSKR.Secret.Create(contentKey.Data);
        var shares = SSKRShare.SskrGenerateUsing(spec, masterSecret, testRng);
        var result = new List<List<Envelope>>();
        foreach (var group in shares)
        {
            var groupResult = new List<Envelope>();
            foreach (var share in group)
            {
                groupResult.Add(AddSskrShare(share));
            }
            result.Add(groupResult);
        }
        return result;
    }

    /// <summary>
    /// Reconstructs the original envelope from a set of SSKR share envelopes.
    /// </summary>
    /// <remarks>
    /// Given envelopes with SSKR share assertions, this method combines the shares
    /// to reconstruct the original symmetric key, then uses it to decrypt the
    /// envelope and return the original subject.
    /// </remarks>
    /// <param name="envelopes">The envelopes containing SSKR shares.</param>
    /// <returns>The original envelope if reconstruction is successful.</returns>
    /// <exception cref="EnvelopeException">
    /// Thrown if not enough valid shares are provided or decryption fails.
    /// </exception>
    public static Envelope SskrJoin(IReadOnlyList<Envelope> envelopes)
    {
        if (envelopes.Count == 0)
            throw EnvelopeException.InvalidShares();

        var grouped = SskrSharesIn(envelopes);
        foreach (var shares in grouped.Values)
        {
            try
            {
                var secret = SSKRShare.SskrCombine(shares);
                var contentKey = SymmetricKey.FromData(secret.ToArray());
                var envelope = envelopes[0].DecryptSubject(contentKey);
                return envelope.Subject;
            }
            catch
            {
                // Try next group
            }
        }
        throw EnvelopeException.InvalidShares();
    }

    /// <summary>
    /// Extracts and groups SSKR shares from envelopes by identifier.
    /// </summary>
    private static Dictionary<int, List<SSKRShare>> SskrSharesIn(IReadOnlyList<Envelope> envelopes)
    {
        var result = new Dictionary<int, List<SSKRShare>>();
        foreach (var envelope in envelopes)
        {
            foreach (var assertion in envelope.AssertionsWithPredicate(KnownValuesRegistry.SSKRShare))
            {
                var share = assertion.AsObject()!.ExtractSubject<SSKRShare>();
                var identifier = share.Identifier();
                if (!result.ContainsKey(identifier))
                    result[identifier] = new List<SSKRShare>();
                result[identifier].Add(share);
            }
        }
        return result;
    }
}

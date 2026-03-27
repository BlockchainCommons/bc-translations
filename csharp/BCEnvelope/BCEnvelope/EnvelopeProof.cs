using BlockchainCommons.BCComponents;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Inclusion proof extension for Gordian Envelopes.
/// </summary>
/// <remarks>
/// <para>
/// Inclusion proofs allow proving that specific elements exist within an
/// envelope without revealing the entire contents. This leverages the
/// Merkle-like digest tree structure of envelopes.
/// </para>
/// <para>
/// The holder creates a minimal structure containing only the digests
/// necessary to validate the proof. A verifier with a trusted root digest
/// can confirm that specific elements exist in the original envelope.
/// All other content remains elided, preserving privacy.
/// </para>
/// </remarks>
public partial class Envelope
{
    /// <summary>
    /// Creates a proof that this envelope includes every element in the target set.
    /// </summary>
    /// <param name="target">The set of digests representing elements to prove.</param>
    /// <returns>
    /// A proof envelope if all targets can be proven to exist, or <c>null</c>
    /// if the proof cannot be constructed.
    /// </returns>
    public Envelope? ProofContainsSet(HashSet<Digest> target)
    {
        var revealSet = RevealSetOfSet(target);
        if (!target.IsSubsetOf(revealSet))
            return null;
        return ElideRevealingSet(revealSet).ElideRemovingSet(target);
    }

    /// <summary>
    /// Creates a proof that this envelope includes the single target element.
    /// </summary>
    /// <param name="target">The element to prove exists in the envelope.</param>
    /// <returns>
    /// A proof envelope if the target can be proven to exist, or <c>null</c> otherwise.
    /// </returns>
    public Envelope? ProofContainsTarget(IDigestProvider target)
    {
        return ProofContainsSet(new HashSet<Digest> { target.GetDigest() });
    }

    /// <summary>
    /// Verifies that all target elements exist using the given inclusion proof.
    /// </summary>
    /// <param name="target">The set of digests representing elements to verify.</param>
    /// <param name="proof">The inclusion proof envelope.</param>
    /// <returns>
    /// <c>true</c> if all target elements are proven to exist; <c>false</c> otherwise.
    /// </returns>
    public bool ConfirmContainsSet(HashSet<Digest> target, Envelope proof)
    {
        return GetDigest() == proof.GetDigest() && proof.ContainsAll(target);
    }

    /// <summary>
    /// Verifies that the target element exists using the given inclusion proof.
    /// </summary>
    /// <param name="target">The element to verify exists.</param>
    /// <param name="proof">The inclusion proof envelope.</param>
    /// <returns>
    /// <c>true</c> if the target element is proven to exist; <c>false</c> otherwise.
    /// </returns>
    public bool ConfirmContainsTarget(IDigestProvider target, Envelope proof)
    {
        return ConfirmContainsSet(new HashSet<Digest> { target.GetDigest() }, proof);
    }

    // --- Internal implementation ---

    /// <summary>
    /// Builds a set of all digests needed to reveal the target set.
    /// </summary>
    private HashSet<Digest> RevealSetOfSet(HashSet<Digest> target)
    {
        var result = new HashSet<Digest>();
        RevealSets(target, new HashSet<Digest>(), result);
        return result;
    }

    /// <summary>
    /// Checks if this envelope contains all elements in the target set.
    /// </summary>
    private bool ContainsAll(HashSet<Digest> target)
    {
        var remaining = new HashSet<Digest>(target);
        RemoveAllFound(remaining);
        return remaining.Count == 0;
    }

    /// <summary>
    /// Recursively collects all digests forming the path from root to each
    /// target element.
    /// </summary>
    private void RevealSets(
        HashSet<Digest> target,
        HashSet<Digest> current,
        HashSet<Digest> result)
    {
        var currentWithSelf = new HashSet<Digest>(current);
        currentWithSelf.Add(GetDigest());

        if (target.Contains(GetDigest()))
        {
            result.UnionWith(currentWithSelf);
        }

        switch (Case)
        {
            case EnvelopeCase.NodeCase node:
                node.Subject.RevealSets(target, currentWithSelf, result);
                foreach (var assertion in node.Assertions)
                {
                    assertion.RevealSets(target, currentWithSelf, result);
                }
                break;
            case EnvelopeCase.WrappedCase wrapped:
                wrapped.Envelope.RevealSets(target, currentWithSelf, result);
                break;
            case EnvelopeCase.AssertionCase assertionCase:
                assertionCase.Assertion.Predicate.RevealSets(target, currentWithSelf, result);
                assertionCase.Assertion.Object.RevealSets(target, currentWithSelf, result);
                break;
        }
    }

    /// <summary>
    /// Recursively traverses the envelope and removes found target elements
    /// from the set.
    /// </summary>
    private void RemoveAllFound(HashSet<Digest> target)
    {
        target.Remove(GetDigest());
        if (target.Count == 0)
            return;

        switch (Case)
        {
            case EnvelopeCase.NodeCase node:
                node.Subject.RemoveAllFound(target);
                foreach (var assertion in node.Assertions)
                {
                    assertion.RemoveAllFound(target);
                }
                break;
            case EnvelopeCase.WrappedCase wrapped:
                wrapped.Envelope.RemoveAllFound(target);
                break;
            case EnvelopeCase.AssertionCase assertionCase:
                assertionCase.Assertion.Predicate.RemoveAllFound(target);
                assertionCase.Assertion.Object.RemoveAllFound(target);
                break;
        }
    }
}

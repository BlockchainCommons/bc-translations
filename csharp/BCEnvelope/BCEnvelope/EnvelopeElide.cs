using System.Diagnostics;
using BlockchainCommons.BCComponents;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Elision, obscuration, and selective disclosure operations for Gordian Envelopes.
/// </summary>
public partial class Envelope
{
    // ---- Basic Elision ----

    /// <summary>
    /// Returns the elided variant of this envelope, preserving only its digest.
    /// </summary>
    /// <returns>An elided envelope with the same digest.</returns>
    public Envelope Elide()
    {
        if (Case is EnvelopeCase.ElidedCase) return this;
        return CreateElided(GetDigest());
    }

    /// <summary>
    /// Returns a version of this envelope with elements in the target set obscured.
    /// </summary>
    /// <param name="target">The set of digests identifying elements to obscure.</param>
    /// <param name="isRevealing">
    /// If <c>true</c>, the target set identifies elements to <em>reveal</em>
    /// (all others are obscured). If <c>false</c>, the target set identifies
    /// elements to <em>obscure</em> (all others are revealed).
    /// </param>
    /// <param name="action">The obscuration action to apply.</param>
    /// <returns>The resulting envelope.</returns>
    public Envelope ElideSetWithAction(
        HashSet<Digest> target,
        bool isRevealing,
        ObscureAction action)
    {
        var selfDigest = GetDigest();
        if (target.Contains(selfDigest) != isRevealing)
        {
            return action switch
            {
                ObscureAction.ElideAction => Elide(),
                ObscureAction.EncryptAction encrypt =>
                    CreateWithEncrypted(encrypt.Key.EncryptWithDigest(
                        TaggedCbor().ToCborData(), selfDigest)),
                ObscureAction.CompressAction => Compress(),
                _ => throw new InvalidOperationException("Unknown obscure action"),
            };
        }

        switch (Case)
        {
            case EnvelopeCase.AssertionCase assertionCase:
            {
                var predicate = assertionCase.Assertion.Predicate
                    .ElideSetWithAction(target, isRevealing, action);
                var obj = assertionCase.Assertion.Object
                    .ElideSetWithAction(target, isRevealing, action);
                var elidedAssertion = new Assertion(predicate, obj);
                Debug.Assert(elidedAssertion == assertionCase.Assertion);
                return CreateWithAssertion(elidedAssertion);
            }

            case EnvelopeCase.NodeCase node:
            {
                var elidedSubject = node.Subject
                    .ElideSetWithAction(target, isRevealing, action);
                Debug.Assert(elidedSubject.GetDigest() == node.Subject.GetDigest());
                var elidedAssertions = new List<Envelope>(node.Assertions.Count);
                foreach (var assertion in node.Assertions)
                {
                    var elided = assertion.ElideSetWithAction(target, isRevealing, action);
                    Debug.Assert(elided.GetDigest() == assertion.GetDigest());
                    elidedAssertions.Add(elided);
                }
                return CreateWithUncheckedAssertions(elidedSubject, elidedAssertions);
            }

            case EnvelopeCase.WrappedCase wrapped:
            {
                var elidedEnvelope = wrapped.Envelope
                    .ElideSetWithAction(target, isRevealing, action);
                Debug.Assert(elidedEnvelope.GetDigest() == wrapped.Envelope.GetDigest());
                return CreateWrapped(elidedEnvelope);
            }

            default:
                return this;
        }
    }

    /// <summary>
    /// Returns a version with elements in the target set elided (simple elision).
    /// </summary>
    public Envelope ElideSet(HashSet<Digest> target, bool isRevealing)
        => ElideSetWithAction(target, isRevealing, ObscureAction.Elide);

    // ---- Removing (target = elements to obscure) ----

    /// <summary>Elides elements whose digests are in the target set.</summary>
    public Envelope ElideRemovingSet(HashSet<Digest> target)
        => ElideSet(target, false);

    /// <summary>Obscures elements whose digests are in the target set with the given action.</summary>
    public Envelope ElideRemovingSetWithAction(HashSet<Digest> target, ObscureAction action)
        => ElideSetWithAction(target, false, action);

    /// <summary>Elides elements whose digests are in the target array.</summary>
    public Envelope ElideRemovingArray(IEnumerable<IDigestProvider> target)
        => ElideRemovingSet(new HashSet<Digest>(target.Select(p => p.GetDigest())));

    /// <summary>Obscures elements whose digests are in the target array with the given action.</summary>
    public Envelope ElideRemovingArrayWithAction(IEnumerable<IDigestProvider> target, ObscureAction action)
        => ElideRemovingSetWithAction(new HashSet<Digest>(target.Select(p => p.GetDigest())), action);

    /// <summary>Elides a single target element.</summary>
    public Envelope ElideRemovingTarget(IDigestProvider target)
        => ElideRemovingSet(new HashSet<Digest> { target.GetDigest() });

    /// <summary>Obscures a single target element with the given action.</summary>
    public Envelope ElideRemovingTargetWithAction(IDigestProvider target, ObscureAction action)
        => ElideRemovingSetWithAction(new HashSet<Digest> { target.GetDigest() }, action);

    // ---- Revealing (target = elements to keep visible) ----

    /// <summary>Reveals only elements whose digests are in the target set, eliding all others.</summary>
    public Envelope ElideRevealingSet(HashSet<Digest> target)
        => ElideSet(target, true);

    /// <summary>Reveals only elements in the target set, obscuring all others with the given action.</summary>
    public Envelope ElideRevealingSetWithAction(HashSet<Digest> target, ObscureAction action)
        => ElideSetWithAction(target, true, action);

    /// <summary>Reveals only elements in the target array, eliding all others.</summary>
    public Envelope ElideRevealingArray(IEnumerable<IDigestProvider> target)
        => ElideRevealingSet(new HashSet<Digest>(target.Select(p => p.GetDigest())));

    /// <summary>Reveals only elements in the target array, obscuring all others with the given action.</summary>
    public Envelope ElideRevealingArrayWithAction(IEnumerable<IDigestProvider> target, ObscureAction action)
        => ElideRevealingSetWithAction(new HashSet<Digest>(target.Select(p => p.GetDigest())), action);

    /// <summary>Reveals only a single target element, eliding all others.</summary>
    public Envelope ElideRevealingTarget(IDigestProvider target)
        => ElideRevealingSet(new HashSet<Digest> { target.GetDigest() });

    /// <summary>Reveals only a single target element, obscuring all others with the given action.</summary>
    public Envelope ElideRevealingTargetWithAction(IDigestProvider target, ObscureAction action)
        => ElideRevealingSetWithAction(new HashSet<Digest> { target.GetDigest() }, action);

    // ---- Unelide ----

    /// <summary>
    /// Restores an elided envelope if the provided envelope's digest matches.
    /// </summary>
    /// <param name="envelope">The original envelope to restore from.</param>
    /// <returns>The restored envelope.</returns>
    /// <exception cref="EnvelopeException">Thrown if the digests do not match.</exception>
    public Envelope Unelide(Envelope envelope)
    {
        if (GetDigest() == envelope.GetDigest())
            return envelope;
        throw EnvelopeException.InvalidDigest();
    }

    // ---- Walk-based operations ----

    /// <summary>
    /// Recursively restores elided nodes from the provided envelopes.
    /// </summary>
    /// <param name="envelopes">Envelopes that may match elided nodes.</param>
    /// <returns>A new envelope with elided nodes restored where possible.</returns>
    public Envelope WalkUnelide(IEnumerable<Envelope> envelopes)
    {
        var map = new Dictionary<Digest, Envelope>();
        foreach (var env in envelopes)
        {
            map[env.GetDigest()] = env;
        }
        return WalkUnelideWithMap(map);
    }

    private Envelope WalkUnelideWithMap(Dictionary<Digest, Envelope> map)
    {
        switch (Case)
        {
            case EnvelopeCase.ElidedCase:
                return map.TryGetValue(GetDigest(), out var replacement) ? replacement : this;

            case EnvelopeCase.NodeCase node:
            {
                var newSubject = node.Subject.WalkUnelideWithMap(map);
                var newAssertions = node.Assertions.Select(a => a.WalkUnelideWithMap(map)).ToList();
                if (newSubject.IsIdenticalTo(node.Subject)
                    && newAssertions.Zip(node.Assertions).All(pair => pair.First.IsIdenticalTo(pair.Second)))
                    return this;
                return CreateWithUncheckedAssertions(newSubject, newAssertions);
            }

            case EnvelopeCase.WrappedCase wrapped:
            {
                var newEnv = wrapped.Envelope.WalkUnelideWithMap(map);
                return newEnv.IsIdenticalTo(wrapped.Envelope) ? this : CreateWrapped(newEnv);
            }

            case EnvelopeCase.AssertionCase assertionCase:
            {
                var newPred = assertionCase.Assertion.Predicate.WalkUnelideWithMap(map);
                var newObj = assertionCase.Assertion.Object.WalkUnelideWithMap(map);
                if (newPred.IsIdenticalTo(assertionCase.Assertion.Predicate)
                    && newObj.IsIdenticalTo(assertionCase.Assertion.Object))
                    return this;
                return CreateAssertion(newPred, newObj);
            }

            default:
                return this;
        }
    }

    /// <summary>
    /// Recursively replaces nodes whose digests match the target set.
    /// </summary>
    /// <param name="target">Set of digests identifying nodes to replace.</param>
    /// <param name="replacement">The replacement envelope.</param>
    /// <returns>A new envelope with matching nodes replaced.</returns>
    /// <exception cref="EnvelopeException">
    /// Thrown if attempting to replace an assertion with a non-assertion that
    /// is also not obscured.
    /// </exception>
    public Envelope WalkReplace(HashSet<Digest> target, Envelope replacement)
    {
        if (target.Contains(GetDigest()))
            return replacement;

        switch (Case)
        {
            case EnvelopeCase.NodeCase node:
            {
                var newSubject = node.Subject.WalkReplace(target, replacement);
                var newAssertions = node.Assertions.Select(a => a.WalkReplace(target, replacement)).ToList();
                if (newSubject.IsIdenticalTo(node.Subject)
                    && newAssertions.Zip(node.Assertions).All(pair => pair.First.IsIdenticalTo(pair.Second)))
                    return this;
                return CreateWithAssertions(newSubject, newAssertions);
            }

            case EnvelopeCase.WrappedCase wrapped:
            {
                var newEnv = wrapped.Envelope.WalkReplace(target, replacement);
                return newEnv.IsIdenticalTo(wrapped.Envelope) ? this : CreateWrapped(newEnv);
            }

            case EnvelopeCase.AssertionCase assertionCase:
            {
                var newPred = assertionCase.Assertion.Predicate.WalkReplace(target, replacement);
                var newObj = assertionCase.Assertion.Object.WalkReplace(target, replacement);
                if (newPred.IsIdenticalTo(assertionCase.Assertion.Predicate)
                    && newObj.IsIdenticalTo(assertionCase.Assertion.Object))
                    return this;
                return CreateAssertion(newPred, newObj);
            }

            default:
                return this;
        }
    }

    /// <summary>
    /// Recursively decrypts encrypted nodes using the provided keys.
    /// </summary>
    /// <param name="keys">Symmetric keys to try for decryption.</param>
    /// <returns>A new envelope with encrypted nodes decrypted where possible.</returns>
    public Envelope WalkDecrypt(IEnumerable<SymmetricKey> keys)
    {
        var keyList = keys as IList<SymmetricKey> ?? keys.ToList();

        switch (Case)
        {
            case EnvelopeCase.EncryptedCase:
            {
                foreach (var key in keyList)
                {
                    try
                    {
                        return DecryptSubject(key).WalkDecrypt(keyList);
                    }
                    catch { /* try next key */ }
                }
                return this;
            }

            case EnvelopeCase.NodeCase node:
            {
                var newSubject = node.Subject.WalkDecrypt(keyList);
                var newAssertions = node.Assertions.Select(a => a.WalkDecrypt(keyList)).ToList();
                if (newSubject.IsIdenticalTo(node.Subject)
                    && newAssertions.Zip(node.Assertions).All(pair => pair.First.IsIdenticalTo(pair.Second)))
                    return this;
                return CreateWithUncheckedAssertions(newSubject, newAssertions);
            }

            case EnvelopeCase.WrappedCase wrapped:
            {
                var newEnv = wrapped.Envelope.WalkDecrypt(keyList);
                return newEnv.IsIdenticalTo(wrapped.Envelope) ? this : CreateWrapped(newEnv);
            }

            case EnvelopeCase.AssertionCase assertionCase:
            {
                var newPred = assertionCase.Assertion.Predicate.WalkDecrypt(keyList);
                var newObj = assertionCase.Assertion.Object.WalkDecrypt(keyList);
                if (newPred.IsIdenticalTo(assertionCase.Assertion.Predicate)
                    && newObj.IsIdenticalTo(assertionCase.Assertion.Object))
                    return this;
                return CreateAssertion(newPred, newObj);
            }

            default:
                return this;
        }
    }

    /// <summary>
    /// Recursively decompresses compressed nodes.
    /// </summary>
    /// <param name="targetDigests">
    /// Optional set of digests to filter by. If <c>null</c>,
    /// all compressed nodes will be decompressed.
    /// </param>
    /// <returns>A new envelope with matching compressed nodes decompressed.</returns>
    public Envelope WalkDecompress(HashSet<Digest>? targetDigests = null)
    {
        switch (Case)
        {
            case EnvelopeCase.CompressedCase:
            {
                var matches = targetDigests?.Contains(GetDigest()) ?? true;
                if (matches)
                {
                    try { return Decompress().WalkDecompress(targetDigests); }
                    catch { return this; }
                }
                return this;
            }

            case EnvelopeCase.NodeCase node:
            {
                var newSubject = node.Subject.WalkDecompress(targetDigests);
                var newAssertions = node.Assertions.Select(a => a.WalkDecompress(targetDigests)).ToList();
                if (newSubject.IsIdenticalTo(node.Subject)
                    && newAssertions.Zip(node.Assertions).All(pair => pair.First.IsIdenticalTo(pair.Second)))
                    return this;
                return CreateWithUncheckedAssertions(newSubject, newAssertions);
            }

            case EnvelopeCase.WrappedCase wrapped:
            {
                var newEnv = wrapped.Envelope.WalkDecompress(targetDigests);
                return newEnv.IsIdenticalTo(wrapped.Envelope) ? this : CreateWrapped(newEnv);
            }

            case EnvelopeCase.AssertionCase assertionCase:
            {
                var newPred = assertionCase.Assertion.Predicate.WalkDecompress(targetDigests);
                var newObj = assertionCase.Assertion.Object.WalkDecompress(targetDigests);
                if (newPred.IsIdenticalTo(assertionCase.Assertion.Predicate)
                    && newObj.IsIdenticalTo(assertionCase.Assertion.Object))
                    return this;
                return CreateAssertion(newPred, newObj);
            }

            default:
                return this;
        }
    }

    /// <summary>
    /// Returns the set of digests of nodes matching the specified criteria.
    /// </summary>
    /// <param name="targetDigests">
    /// Optional set of digests to filter by. If <c>null</c>, all nodes are considered.
    /// </param>
    /// <param name="obscureTypes">
    /// Types of obscuration to match against. If empty, all nodes in the target set are returned.
    /// </param>
    /// <returns>A set of digests for matching nodes.</returns>
    public HashSet<Digest> NodesMatching(
        HashSet<Digest>? targetDigests = null,
        params ObscureType[] obscureTypes)
    {
        var result = new HashSet<Digest>();

        Walk(false, 0, (envelope, _, _, state) =>
        {
            var digestMatches = targetDigests?.Contains(envelope.GetDigest()) ?? true;
            if (!digestMatches) return (state, false);

            if (obscureTypes.Length == 0)
            {
                result.Add(envelope.GetDigest());
                return (state, false);
            }

            var typeMatches = obscureTypes.Any(type => type switch
            {
                ObscureType.Elided => envelope.IsElided,
                ObscureType.Encrypted => envelope.IsEncrypted,
                ObscureType.Compressed => envelope.IsCompressed,
                _ => false,
            });

            if (typeMatches)
                result.Add(envelope.GetDigest());

            return (state, false);
        });

        return result;
    }
}

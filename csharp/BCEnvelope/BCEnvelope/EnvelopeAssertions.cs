using BlockchainCommons.BCComponents;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Assertion management methods for <see cref="Envelope"/>.
/// </summary>
public sealed partial class Envelope
{
    /// <summary>
    /// Returns a new envelope with the given assertion added.
    /// </summary>
    /// <param name="predicate">The predicate value.</param>
    /// <param name="object">The object value.</param>
    /// <returns>A new envelope with the assertion added.</returns>
    public Envelope AddAssertion(object predicate, object @object)
    {
        var assertion = CreateAssertion(predicate, @object);
        return AddOptionalAssertionEnvelope(assertion);
    }

    /// <summary>
    /// Returns a new envelope with the given assertion envelope added.
    /// </summary>
    /// <param name="assertionEnvelope">A valid assertion envelope to add.</param>
    /// <returns>A new envelope with the assertion added.</returns>
    /// <exception cref="EnvelopeException">Thrown if the envelope is not a valid assertion.</exception>
    public Envelope AddAssertionEnvelope(Envelope assertionEnvelope)
    {
        return AddOptionalAssertionEnvelope(assertionEnvelope);
    }

    /// <summary>
    /// Returns a new envelope with multiple assertion envelopes added.
    /// </summary>
    /// <param name="assertions">An array of assertion envelopes to add.</param>
    /// <returns>A new envelope with all assertions added.</returns>
    /// <exception cref="EnvelopeException">Thrown if any envelope is not a valid assertion.</exception>
    public Envelope AddAssertionEnvelopes(IEnumerable<Envelope> assertions)
    {
        var e = this;
        foreach (var assertion in assertions)
            e = e.AddAssertionEnvelope(assertion);
        return e;
    }

    /// <summary>
    /// Adds an optional assertion envelope. If null, returns the envelope unchanged.
    /// Duplicate assertions (same digest) are not added.
    /// </summary>
    /// <param name="assertion">An optional assertion envelope to add.</param>
    /// <returns>A new envelope with the assertion added, or the original if null or duplicate.</returns>
    /// <exception cref="EnvelopeException">Thrown if the envelope is not a valid assertion.</exception>
    public Envelope AddOptionalAssertionEnvelope(Envelope? assertion)
    {
        if (assertion is null)
            return this;

        if (!assertion.IsSubjectAssertion && !assertion.IsSubjectObscured)
            throw EnvelopeException.InvalidFormat();

        if (_case is EnvelopeCase.NodeCase n)
        {
            // Check for duplicate by digest
            var assertionDigest = assertion.GetDigest();
            foreach (var existing in n.Assertions)
            {
                if (existing.GetDigest() == assertionDigest)
                    return this;
            }

            var assertions = new List<Envelope>(n.Assertions) { assertion };
            return CreateWithUncheckedAssertions(n.Subject, assertions);
        }

        return CreateWithUncheckedAssertions(Subject, new List<Envelope> { assertion });
    }

    /// <summary>
    /// Adds an assertion with the given predicate and optional object.
    /// If the object is null, returns the envelope unchanged.
    /// </summary>
    public Envelope AddOptionalAssertion(object predicate, object? @object)
    {
        if (@object is null)
            return this;
        return AddAssertionEnvelope(CreateAssertion(predicate, @object));
    }

    /// <summary>
    /// Adds an assertion with the given predicate and string value, but only
    /// if the string is non-empty.
    /// </summary>
    public Envelope AddNonemptyStringAssertion(object predicate, string str)
    {
        if (string.IsNullOrEmpty(str))
            return this;
        return AddAssertion(predicate, str);
    }

    /// <summary>
    /// Returns a new envelope with the given array of assertion envelopes added.
    /// </summary>
    public Envelope AddAssertions(IEnumerable<Envelope> envelopes)
    {
        var e = this;
        foreach (var envelope in envelopes)
            e = e.AddAssertionEnvelope(envelope);
        return e;
    }

    /// <summary>
    /// Adds an assertion envelope only if the provided condition is true.
    /// </summary>
    public Envelope AddAssertionEnvelopeIf(bool condition, Envelope assertionEnvelope)
    {
        if (condition)
            return AddAssertionEnvelope(assertionEnvelope);
        return this;
    }

    // ===== Salted Assertions =====

    /// <summary>
    /// Returns the result of adding the given assertion, optionally salting it.
    /// </summary>
    public Envelope AddAssertionSalted(object predicate, object @object, bool salted)
    {
        var assertion = CreateAssertion(predicate, @object);
        return AddOptionalAssertionEnvelopeSalted(assertion, salted);
    }

    /// <summary>
    /// Adds an assertion envelope, optionally salting it.
    /// </summary>
    public Envelope AddAssertionEnvelopeSalted(Envelope assertionEnvelope, bool salted)
    {
        return AddOptionalAssertionEnvelopeSalted(assertionEnvelope, salted);
    }

    /// <summary>
    /// If the optional assertion is present, adds it to the envelope, optionally salting it.
    /// </summary>
    public Envelope AddOptionalAssertionEnvelopeSalted(Envelope? assertion, bool salted)
    {
        if (assertion is null)
            return this;

        if (!assertion.IsSubjectAssertion && !assertion.IsSubjectObscured)
            throw EnvelopeException.InvalidFormat();

        var envelope2 = salted ? assertion.AddSalt() : assertion;

        if (_case is EnvelopeCase.NodeCase n)
        {
            var assertionDigest = envelope2.GetDigest();
            foreach (var existing in n.Assertions)
            {
                if (existing.GetDigest() == assertionDigest)
                    return this;
            }

            var assertions = new List<Envelope>(n.Assertions) { envelope2 };
            return CreateWithUncheckedAssertions(n.Subject, assertions);
        }

        return CreateWithUncheckedAssertions(Subject, new List<Envelope> { envelope2 });
    }

    /// <summary>
    /// Adds multiple assertions, optionally salting each.
    /// </summary>
    public Envelope AddAssertionsSalted(IEnumerable<Envelope> assertions, bool salted)
    {
        var e = this;
        foreach (var assertion in assertions)
            e = e.AddAssertionEnvelopeSalted(assertion, salted);
        return e;
    }

    // ===== Remove / Replace =====

    /// <summary>
    /// Returns a new envelope with the given assertion removed.
    /// </summary>
    /// <param name="target">The assertion envelope to remove (matched by digest).</param>
    /// <returns>A new envelope without the specified assertion.</returns>
    public Envelope RemoveAssertion(Envelope target)
    {
        var assertions = new List<Envelope>(Assertions);
        var targetDigest = target.GetDigest();

        int index = -1;
        for (int i = 0; i < assertions.Count; i++)
        {
            if (assertions[i].GetDigest() == targetDigest)
            {
                index = i;
                break;
            }
        }

        if (index >= 0)
        {
            assertions.RemoveAt(index);
            if (assertions.Count == 0)
                return Subject;
            return CreateWithUncheckedAssertions(Subject, assertions);
        }

        return this;
    }

    /// <summary>
    /// Returns a new envelope with the given assertion replaced by a new one.
    /// </summary>
    public Envelope ReplaceAssertion(Envelope assertion, Envelope newAssertion)
    {
        return RemoveAssertion(assertion).AddAssertionEnvelope(newAssertion);
    }

    /// <summary>
    /// Returns a new envelope with its subject replaced by the provided one.
    /// All assertions from the original envelope are preserved.
    /// </summary>
    public Envelope ReplaceSubject(Envelope subject)
    {
        var e = subject;
        foreach (var assertion in Assertions)
            e = e.AddAssertionEnvelope(assertion);
        return e;
    }
}

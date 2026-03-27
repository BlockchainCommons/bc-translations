using BlockchainCommons.BCComponents;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Signature operations for Gordian Envelopes.
/// </summary>
/// <remarks>
/// Provides methods for digitally signing envelopes and verifying signatures.
/// Supports both simple signatures and signatures with metadata, as well as
/// multi-signature and threshold-signature scenarios.
/// </remarks>
public partial class Envelope
{
    // ---- Adding Signatures ----

    /// <summary>
    /// Creates a signature for the envelope's subject and returns a new
    /// envelope with a <c>'signed': Signature</c> assertion.
    /// </summary>
    /// <param name="signer">The signer to use.</param>
    /// <returns>The signed envelope.</returns>
    public Envelope AddSignature(ISigner signer)
        => AddSignatureOpt(signer, null, null);

    /// <summary>
    /// Creates a signature for the envelope's subject with optional signing
    /// options and metadata.
    /// </summary>
    /// <param name="signer">The signer to use.</param>
    /// <param name="options">Optional signing options.</param>
    /// <param name="metadata">Optional metadata for the signature.</param>
    /// <returns>The signed envelope.</returns>
    public Envelope AddSignatureOpt(
        ISigner signer,
        SigningOptions? options,
        SignatureMetadata? metadata)
    {
        var digestData = Subject.GetDigest().Data;
        var signature = EnvelopeExtensions.ToEnvelope(
            signer.SignWithOptions(digestData, options));

        if (metadata is not null && metadata.HasAssertions)
        {
            var signatureWithMetadata = signature;

            foreach (var assertion in metadata.Assertions)
            {
                signatureWithMetadata = signatureWithMetadata
                    .AddAssertionEnvelope(EnvelopeExtensions.ToEnvelope(assertion));
            }

            signatureWithMetadata = signatureWithMetadata.Wrap();

            var outerSignature = EnvelopeExtensions.ToEnvelope(
                signer.SignWithOptions(
                    signatureWithMetadata.GetDigest().Data,
                    options));

            signature = signatureWithMetadata
                .AddAssertion(KnownValuesRegistry.Signed, outerSignature);
        }

        return AddAssertion(KnownValuesRegistry.Signed, signature);
    }

    /// <summary>
    /// Creates several signatures for the envelope's subject.
    /// </summary>
    /// <param name="signers">An array of signers.</param>
    /// <returns>The signed envelope.</returns>
    public Envelope AddSignatures(IEnumerable<ISigner> signers)
    {
        var result = this;
        foreach (var signer in signers)
        {
            result = result.AddSignature(signer);
        }
        return result;
    }

    /// <summary>
    /// Creates several signatures with optional options and metadata.
    /// </summary>
    /// <param name="signers">
    /// An array of tuples containing a signer, optional options, and optional metadata.
    /// </param>
    /// <returns>The signed envelope.</returns>
    public Envelope AddSignaturesOpt(
        IEnumerable<(ISigner Signer, SigningOptions? Options, SignatureMetadata? Metadata)> signers)
    {
        var result = this;
        foreach (var (signer, options, metadata) in signers)
        {
            result = result.AddSignatureOpt(signer, options, metadata);
        }
        return result;
    }

    // ---- Signature Verification ----

    /// <summary>
    /// Returns whether the given signature is valid for this envelope's subject.
    /// </summary>
    /// <param name="signature">The signature to check.</param>
    /// <param name="verifier">The verifier (public key).</param>
    /// <returns><c>true</c> if the signature is valid; <c>false</c> otherwise.</returns>
    public bool IsVerifiedSignature(Signature signature, IVerifier verifier)
        => IsSignatureFromKey(signature, verifier);

    /// <summary>
    /// Verifies the given signature, throwing on failure.
    /// </summary>
    /// <param name="signature">The signature to verify.</param>
    /// <param name="verifier">The verifier (public key).</param>
    /// <returns>This envelope if verification succeeds.</returns>
    /// <exception cref="EnvelopeException">Thrown if verification fails.</exception>
    public Envelope VerifySignature(Signature signature, IVerifier verifier)
    {
        if (!IsSignatureFromKey(signature, verifier))
            throw EnvelopeException.UnverifiedSignature();
        return this;
    }

    /// <summary>
    /// Returns whether the envelope has at least one valid signature from the given verifier.
    /// </summary>
    /// <param name="verifier">The verifier (public key).</param>
    /// <returns><c>true</c> if a valid signature is found.</returns>
    public bool HasSignatureFrom(IVerifier verifier)
        => HasSomeSignatureFromKeyReturningMetadata(verifier) is not null;

    /// <summary>
    /// Returns the signature metadata envelope if a valid signature from the
    /// given verifier is found; otherwise <c>null</c>.
    /// </summary>
    /// <param name="verifier">The verifier (public key).</param>
    /// <returns>The metadata envelope, or <c>null</c>.</returns>
    public Envelope? HasSignatureFromReturningMetadata(IVerifier verifier)
        => HasSomeSignatureFromKeyReturningMetadata(verifier);

    /// <summary>
    /// Verifies the envelope has a valid signature from the given verifier,
    /// throwing on failure.
    /// </summary>
    /// <param name="verifier">The verifier (public key).</param>
    /// <returns>This envelope if verification succeeds.</returns>
    /// <exception cref="EnvelopeException">Thrown if no valid signature is found.</exception>
    public Envelope VerifySignatureFrom(IVerifier verifier)
    {
        if (!HasSignatureFrom(verifier))
            throw EnvelopeException.UnverifiedSignature();
        return this;
    }

    /// <summary>
    /// Verifies the signature and returns the metadata envelope.
    /// </summary>
    /// <param name="verifier">The verifier (public key).</param>
    /// <returns>The metadata envelope.</returns>
    /// <exception cref="EnvelopeException">Thrown if no valid signature is found.</exception>
    public Envelope VerifySignatureFromReturningMetadata(IVerifier verifier)
    {
        return HasSomeSignatureFromKeyReturningMetadata(verifier)
            ?? throw EnvelopeException.UnverifiedSignature();
    }

    // ---- Multi-signature ----

    /// <summary>
    /// Checks whether the envelope has valid signatures from all given verifiers.
    /// </summary>
    /// <param name="verifiers">The verifiers (public keys).</param>
    /// <returns><c>true</c> if all verifiers have matching signatures.</returns>
    public bool HasSignaturesFrom(IEnumerable<IVerifier> verifiers)
        => HasSignaturesFromThreshold(verifiers, null);

    /// <summary>
    /// Checks whether the envelope meets a threshold of valid signatures.
    /// </summary>
    /// <param name="verifiers">The verifiers (public keys).</param>
    /// <param name="threshold">
    /// The minimum number of valid signatures required.
    /// If <c>null</c>, all verifiers must have signed.
    /// </param>
    /// <returns><c>true</c> if the threshold is met.</returns>
    public bool HasSignaturesFromThreshold(IEnumerable<IVerifier> verifiers, int? threshold)
    {
        var verifierList = verifiers as IList<IVerifier> ?? verifiers.ToList();
        var required = threshold ?? verifierList.Count;
        var count = 0;
        foreach (var key in verifierList)
        {
            if (HasSomeSignatureFromKeyReturningMetadata(key) is not null)
            {
                count++;
                if (count >= required) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Verifies a threshold of signatures, throwing on failure.
    /// </summary>
    /// <param name="verifiers">The verifiers (public keys).</param>
    /// <param name="threshold">
    /// The minimum number of valid signatures required.
    /// If <c>null</c>, all verifiers must have signed.
    /// </param>
    /// <returns>This envelope if the threshold is met.</returns>
    /// <exception cref="EnvelopeException">Thrown if the threshold is not met.</exception>
    public Envelope VerifySignaturesFromThreshold(
        IEnumerable<IVerifier> verifiers, int? threshold)
    {
        if (!HasSignaturesFromThreshold(verifiers, threshold))
            throw EnvelopeException.UnverifiedSignature();
        return this;
    }

    /// <summary>
    /// Verifies all signatures, throwing on failure.
    /// </summary>
    /// <param name="verifiers">The verifiers (public keys).</param>
    /// <returns>This envelope if all signatures verify.</returns>
    /// <exception cref="EnvelopeException">Thrown if any verification fails.</exception>
    public Envelope VerifySignaturesFrom(IEnumerable<IVerifier> verifiers)
        => VerifySignaturesFromThreshold(verifiers, null);

    // ---- Convenience methods (wrap + sign, verify + unwrap) ----

    /// <summary>
    /// Signs the entire envelope by wrapping it first, then adding a signature.
    /// </summary>
    /// <param name="signer">The signer to use.</param>
    /// <returns>A new envelope with the wrapped envelope as subject and a signature assertion.</returns>
    public Envelope Sign(ISigner signer)
        => SignOpt(signer, null);

    /// <summary>
    /// Signs the entire envelope with options by wrapping it first, then adding a signature.
    /// </summary>
    /// <param name="signer">The signer to use.</param>
    /// <param name="options">Optional signing options.</param>
    /// <returns>A new envelope with the wrapped envelope as subject and a signature assertion.</returns>
    public Envelope SignOpt(ISigner signer, SigningOptions? options)
        => Wrap().AddSignatureOpt(signer, options, null);

    /// <summary>
    /// Verifies the signature from the given verifier and unwraps the envelope.
    /// </summary>
    /// <param name="verifier">The verifier (public key).</param>
    /// <returns>The unwrapped envelope.</returns>
    /// <exception cref="EnvelopeException">Thrown if verification or unwrapping fails.</exception>
    public Envelope Verify(IVerifier verifier)
        => VerifySignatureFrom(verifier).TryUnwrap();

    /// <summary>
    /// Verifies the signature and returns both the unwrapped envelope and
    /// signature metadata.
    /// </summary>
    /// <param name="verifier">The verifier (public key).</param>
    /// <returns>A tuple of the unwrapped envelope and the metadata envelope.</returns>
    /// <exception cref="EnvelopeException">Thrown if verification or unwrapping fails.</exception>
    public (Envelope Envelope, Envelope Metadata) VerifyReturningMetadata(IVerifier verifier)
    {
        var metadata = VerifySignatureFromReturningMetadata(verifier);
        return (TryUnwrap(), metadata);
    }

    // ---- Internal helpers ----

    private bool IsSignatureFromKey(Signature signature, IVerifier key)
        => key.Verify(signature, Subject.GetDigest().Data);

    private Envelope? HasSomeSignatureFromKeyReturningMetadata(IVerifier key)
    {
        // Valid signature objects are either:
        // - Signature objects, or
        // - Signature objects with additional metadata assertions, wrapped
        //   and then signed by the same key.
        var signatureObjects = ObjectsForPredicate(KnownValuesRegistry.Signed);

        foreach (var signatureObject in signatureObjects)
        {
            var signatureObjectSubject = signatureObject.Subject;

            if (signatureObjectSubject.IsWrapped)
            {
                // Wrapped signature with metadata
                Envelope outerSignatureObject;
                try
                {
                    outerSignatureObject = signatureObject.ObjectForPredicate(
                        KnownValuesRegistry.Signed);
                }
                catch { continue; }

                Signature outerSignature;
                try
                {
                    outerSignature = outerSignatureObject.ExtractSubject<Signature>();
                }
                catch
                {
                    throw EnvelopeException.InvalidOuterSignatureType();
                }

                if (!signatureObjectSubject.IsSignatureFromKey(outerSignature, key))
                    continue;

                var signatureMetadataEnvelope = signatureObjectSubject.TryUnwrap();

                Signature innerSignature;
                try
                {
                    innerSignature = signatureMetadataEnvelope.ExtractSubject<Signature>();
                }
                catch
                {
                    throw EnvelopeException.InvalidInnerSignatureType();
                }

                if (!IsSignatureFromKey(innerSignature, key))
                    throw EnvelopeException.UnverifiedInnerSignature();

                return signatureMetadataEnvelope;
            }
            else
            {
                // Simple signature (not wrapped)
                Signature signature;
                try
                {
                    signature = signatureObject.ExtractSubject<Signature>();
                }
                catch
                {
                    throw EnvelopeException.InvalidSignatureType();
                }

                if (IsSignatureFromKey(signature, key))
                    return signatureObject;
            }
        }

        return null;
    }
}

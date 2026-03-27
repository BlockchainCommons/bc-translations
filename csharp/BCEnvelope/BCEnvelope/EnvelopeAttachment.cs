using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Attachment support for Gordian Envelopes.
/// </summary>
/// <remarks>
/// <para>
/// Attachments allow vendor-specific data to be included in an envelope
/// without interfering with the main data structure or with other attachments.
/// </para>
/// <para>
/// Each attachment has a payload (arbitrary data), a required vendor identifier
/// (typically a reverse domain name), and an optional <c>conformsTo</c> URI
/// indicating the format.
/// </para>
/// </remarks>
public partial class Assertion
{
    /// <summary>
    /// Creates a new attachment assertion.
    /// </summary>
    /// <remarks>
    /// An attachment assertion consists of the predicate <c>attachment</c> and
    /// an object that is a wrapped envelope containing the payload, a required
    /// <c>vendor</c> assertion, and an optional <c>conformsTo</c> assertion.
    /// </remarks>
    /// <param name="payload">The content of the attachment.</param>
    /// <param name="vendor">A string identifying the vendor (typically a reverse domain name).</param>
    /// <param name="conformsTo">An optional URI identifying the format of the attachment.</param>
    /// <returns>A new attachment assertion.</returns>
    public static Assertion NewAttachment(object payload, string vendor, string? conformsTo = null)
    {
        var payloadEnvelope = Envelope.Create(payload)
            .Wrap()
            .AddAssertion(KnownValuesRegistry.Vendor, vendor);
        if (conformsTo != null)
        {
            payloadEnvelope = payloadEnvelope.AddAssertion(KnownValuesRegistry.ConformsTo, conformsTo);
        }
        return new Assertion(
            Envelope.Create(KnownValuesRegistry.Attachment),
            payloadEnvelope);
    }

    /// <summary>
    /// Returns the payload of this attachment assertion.
    /// </summary>
    /// <returns>The payload envelope.</returns>
    public Envelope AttachmentPayload()
    {
        return Object.TryUnwrap();
    }

    /// <summary>
    /// Returns the vendor identifier of this attachment assertion.
    /// </summary>
    /// <returns>The vendor string.</returns>
    public string AttachmentVendor()
    {
        return Object.ExtractObjectForPredicate<string>(KnownValuesRegistry.Vendor);
    }

    /// <summary>
    /// Returns the optional <c>conformsTo</c> URI of this attachment assertion.
    /// </summary>
    /// <returns>The conformsTo string if present, or <c>null</c>.</returns>
    public string? AttachmentConformsTo()
    {
        return Object.ExtractOptionalObjectForPredicate<string>(KnownValuesRegistry.ConformsTo);
    }

    /// <summary>
    /// Validates that this assertion is a proper attachment assertion.
    /// </summary>
    /// <exception cref="EnvelopeException">Thrown if the assertion is not a valid attachment.</exception>
    public void ValidateAttachment()
    {
        var payload = AttachmentPayload();
        var vendor = AttachmentVendor();
        var conformsTo = AttachmentConformsTo();
        var assertion = Assertion.NewAttachment(payload, vendor, conformsTo);
        var e = assertion.ToEnvelope();
        if (!e.IsEquivalentTo(this.ToEnvelope()))
            throw EnvelopeException.InvalidAttachment();
    }
}

/// <summary>
/// Envelope methods for creating and accessing attachments.
/// </summary>
public partial class Envelope
{
    /// <summary>
    /// Creates a new envelope with an attachment as its subject.
    /// </summary>
    /// <param name="payload">The content of the attachment.</param>
    /// <param name="vendor">A string identifying the vendor.</param>
    /// <param name="conformsTo">An optional URI identifying the format.</param>
    /// <returns>A new envelope with the attachment as its subject.</returns>
    public static Envelope NewAttachment(object payload, string vendor, string? conformsTo = null)
    {
        return Assertion.NewAttachment(payload, vendor, conformsTo).ToEnvelope();
    }

    /// <summary>
    /// Returns a new envelope with an added attachment assertion.
    /// </summary>
    /// <param name="payload">The content of the attachment.</param>
    /// <param name="vendor">A string identifying the vendor.</param>
    /// <param name="conformsTo">An optional URI identifying the format.</param>
    /// <returns>A new envelope with the attachment assertion added.</returns>
    public Envelope AddAttachment(object payload, string vendor, string? conformsTo = null)
    {
        return AddAssertionEnvelope(
            Assertion.NewAttachment(payload, vendor, conformsTo).ToEnvelope());
    }

    /// <summary>
    /// Returns the payload of an attachment envelope.
    /// </summary>
    /// <returns>The payload envelope.</returns>
    /// <exception cref="EnvelopeException">Thrown if the envelope is not a valid attachment.</exception>
    public Envelope AttachmentPayload()
    {
        if (Case is EnvelopeCase.AssertionCase assertionCase)
            return assertionCase.Assertion.AttachmentPayload();
        throw EnvelopeException.InvalidAttachment();
    }

    /// <summary>
    /// Returns the vendor identifier of an attachment envelope.
    /// </summary>
    /// <returns>The vendor string.</returns>
    /// <exception cref="EnvelopeException">Thrown if the envelope is not a valid attachment.</exception>
    public string AttachmentVendor()
    {
        if (Case is EnvelopeCase.AssertionCase assertionCase)
            return assertionCase.Assertion.AttachmentVendor();
        throw EnvelopeException.InvalidAttachment();
    }

    /// <summary>
    /// Returns the optional <c>conformsTo</c> URI of an attachment envelope.
    /// </summary>
    /// <returns>The conformsTo string if present, or <c>null</c>.</returns>
    /// <exception cref="EnvelopeException">Thrown if the envelope is not a valid attachment.</exception>
    public string? AttachmentConformsTo()
    {
        if (Case is EnvelopeCase.AssertionCase assertionCase)
            return assertionCase.Assertion.AttachmentConformsTo();
        throw EnvelopeException.InvalidAttachment();
    }

    /// <summary>
    /// Returns all attachments in the envelope.
    /// </summary>
    /// <returns>A list of all attachment envelopes.</returns>
    public List<Envelope> Attachments()
    {
        return AttachmentsWithVendorAndConformsTo(null, null);
    }

    /// <summary>
    /// Returns attachments matching the given vendor and/or <c>conformsTo</c>.
    /// </summary>
    /// <param name="vendor">Optional vendor identifier to match.</param>
    /// <param name="conformsTo">Optional conformsTo URI to match.</param>
    /// <returns>A list of matching attachment envelopes.</returns>
    public List<Envelope> AttachmentsWithVendorAndConformsTo(string? vendor, string? conformsTo)
    {
        var assertions = AssertionsWithPredicate(KnownValuesRegistry.Attachment);
        foreach (var assertion in assertions)
        {
            assertion.ValidateAttachment();
        }
        return assertions
            .Where(assertion =>
            {
                if (vendor != null)
                {
                    try
                    {
                        if (assertion.AttachmentVendor() != vendor)
                            return false;
                    }
                    catch
                    {
                        return false;
                    }
                }
                if (conformsTo != null)
                {
                    try
                    {
                        var c = assertion.AttachmentConformsTo();
                        if (c != conformsTo)
                            return false;
                    }
                    catch
                    {
                        return false;
                    }
                }
                return true;
            })
            .ToList();
    }

    /// <summary>
    /// Returns the single attachment matching the criteria, or throws.
    /// </summary>
    /// <param name="vendor">Optional vendor identifier to match.</param>
    /// <param name="conformsTo">Optional conformsTo URI to match.</param>
    /// <returns>The matching attachment envelope.</returns>
    /// <exception cref="EnvelopeException">
    /// Thrown if no attachments match (<see cref="EnvelopeException.NonexistentAttachment"/>)
    /// or more than one matches (<see cref="EnvelopeException.AmbiguousAttachment"/>).
    /// </exception>
    public Envelope AttachmentWithVendorAndConformsTo(string? vendor, string? conformsTo)
    {
        var attachments = AttachmentsWithVendorAndConformsTo(vendor, conformsTo);
        if (attachments.Count == 0)
            throw EnvelopeException.NonexistentAttachment();
        if (attachments.Count > 1)
            throw EnvelopeException.AmbiguousAttachment();
        return attachments[0];
    }

    /// <summary>
    /// Validates that this envelope is a proper attachment envelope.
    /// </summary>
    /// <exception cref="EnvelopeException">Thrown if the envelope is not a valid attachment.</exception>
    public void ValidateAttachment()
    {
        if (Case is EnvelopeCase.AssertionCase assertionCase)
        {
            assertionCase.Assertion.ValidateAttachment();
            return;
        }
        throw EnvelopeException.InvalidAttachment();
    }
}

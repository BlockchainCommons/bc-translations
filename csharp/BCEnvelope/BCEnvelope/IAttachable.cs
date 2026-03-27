using BlockchainCommons.BCComponents;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Interface for types that can have metadata attachments.
/// </summary>
/// <remarks>
/// <see cref="IAttachable"/> provides a consistent interface for working with
/// metadata attachments. Types implementing this interface can store and retrieve
/// vendor-specific data without modifying their core structure.
/// </remarks>
public interface IAttachable
{
    /// <summary>
    /// Returns a reference to the attachments container.
    /// </summary>
    Attachments AttachmentsContainer { get; }

    /// <summary>
    /// Adds a new attachment with the specified payload and metadata.
    /// </summary>
    /// <param name="payload">The data to attach.</param>
    /// <param name="vendor">A string identifying the vendor.</param>
    /// <param name="conformsTo">An optional URI identifying the format.</param>
    void AddAttachment(object payload, string vendor, string? conformsTo = null)
    {
        AttachmentsContainer.Add(payload, vendor, conformsTo);
    }

    /// <summary>
    /// Retrieves an attachment by its digest.
    /// </summary>
    /// <param name="digest">The digest of the attachment to retrieve.</param>
    /// <returns>The envelope if found, or <c>null</c>.</returns>
    Envelope? GetAttachment(Digest digest)
    {
        return AttachmentsContainer.Get(digest);
    }

    /// <summary>
    /// Removes an attachment by its digest.
    /// </summary>
    /// <param name="digest">The digest of the attachment to remove.</param>
    /// <returns>The removed envelope if found, or <c>null</c>.</returns>
    Envelope? RemoveAttachment(Digest digest)
    {
        return AttachmentsContainer.Remove(digest);
    }

    /// <summary>
    /// Removes all attachments.
    /// </summary>
    void ClearAttachments()
    {
        AttachmentsContainer.Clear();
    }

    /// <summary>
    /// Returns <c>true</c> if the object has any attachments.
    /// </summary>
    bool HasAttachments => !AttachmentsContainer.IsEmpty;
}

using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Compression and decompression operations for Gordian Envelopes.
/// </summary>
/// <remarks>
/// Compression preserves the envelope's digest tree structure, so signatures
/// and other cryptographic artifacts remain valid even when parts of the
/// envelope are compressed.
/// </remarks>
public partial class Envelope
{
    /// <summary>
    /// Returns a compressed version of this envelope.
    /// </summary>
    /// <returns>A compressed envelope with the same digest.</returns>
    /// <exception cref="EnvelopeException">
    /// Thrown if the envelope is already encrypted or elided.
    /// </exception>
    public Envelope Compress()
    {
        switch (Case)
        {
            case EnvelopeCase.CompressedCase:
                return this;
            case EnvelopeCase.EncryptedCase:
                throw EnvelopeException.AlreadyEncrypted();
            case EnvelopeCase.ElidedCase:
                throw EnvelopeException.AlreadyElided();
            default:
            {
                var data = TaggedCbor().ToCborData();
                var compressed = Compressed.FromDecompressedData(data, GetDigest());
                return CreateWithCompressed(compressed);
            }
        }
    }

    /// <summary>
    /// Returns the decompressed variant of this envelope.
    /// </summary>
    /// <returns>A decompressed envelope.</returns>
    /// <exception cref="EnvelopeException">
    /// Thrown if the envelope is not compressed, is missing a digest,
    /// or the decompressed data does not match the expected digest.
    /// </exception>
    public Envelope Decompress()
    {
        if (Case is not EnvelopeCase.CompressedCase compressedCase)
            throw EnvelopeException.NotCompressed();

        var compressed = compressedCase.Compressed;
        var digest = compressed.Digest ?? throw EnvelopeException.MissingDigest();

        if (digest != GetDigest())
            throw EnvelopeException.InvalidDigest();

        var decompressedData = compressed.Decompress();
        var envelope = FromCborData(decompressedData);

        if (envelope.GetDigest() != digest)
            throw EnvelopeException.InvalidDigest();

        return envelope;
    }

    /// <summary>
    /// Returns this envelope with its subject compressed.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="Compress"/> which compresses the entire envelope,
    /// this method only compresses the subject, leaving assertions readable.
    /// </remarks>
    /// <returns>A new envelope with a compressed subject.</returns>
    public Envelope CompressSubject()
    {
        if (Subject.IsCompressed) return this;
        var compressedSubject = Subject.Compress();
        return ReplaceSubject(compressedSubject);
    }

    /// <summary>
    /// Returns this envelope with its subject decompressed.
    /// </summary>
    /// <returns>A new envelope with a decompressed subject.</returns>
    public Envelope DecompressSubject()
    {
        if (!Subject.IsCompressed) return this;
        var decompressedSubject = Subject.Decompress();
        return ReplaceSubject(decompressedSubject);
    }
}

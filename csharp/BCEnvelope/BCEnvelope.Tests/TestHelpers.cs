using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.DCbor;
using Xunit;

namespace BlockchainCommons.BCEnvelope.Tests;

/// <summary>
/// Test helpers for CBOR encoding verification and assertion utilities.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Checks round-trip CBOR encoding of an Envelope:
    /// 1. Encodes to tagged CBOR
    /// 2. Decodes back
    /// 3. Verifies digests match
    /// Returns the original envelope on success.
    /// </summary>
    public static Envelope CheckEncoding(this Envelope envelope)
    {
        var cbor = envelope.TaggedCbor();
        Envelope restored;
        try
        {
            restored = Envelope.FromTaggedCbor(cbor);
        }
        catch
        {
            throw new Exception(
                $"=== EXPECTED\n{envelope.Format()}\n=== GOT\n{cbor.Diagnostic()}\n===\nInvalid format");
        }

        if (envelope.GetDigest() != restored.GetDigest())
        {
            throw new Exception(
                $"=== EXPECTED\n{envelope.Format()}\n=== GOT\n{restored.Format()}\n===\nDigest mismatch");
        }

        return envelope;
    }
}

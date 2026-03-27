using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// The core structural variants of a Gordian Envelope.
/// </summary>
/// <remarks>
/// Each variant represents a different structural form that an envelope can
/// take, as defined in the Gordian Envelope specification.
/// </remarks>
public abstract class EnvelopeCase
{
    private EnvelopeCase() { }

    /// <summary>
    /// An envelope with a subject and one or more assertions.
    /// </summary>
    public sealed class NodeCase : EnvelopeCase
    {
        public Envelope Subject { get; }
        public IReadOnlyList<Envelope> Assertions { get; }
        public Digest Digest { get; }

        public NodeCase(Envelope subject, IReadOnlyList<Envelope> assertions, Digest digest)
        {
            Subject = subject;
            Assertions = assertions;
            Digest = digest;
        }
    }

    /// <summary>
    /// An envelope containing a primitive CBOR value.
    /// </summary>
    public sealed class LeafCase : EnvelopeCase
    {
        public Cbor Cbor { get; }
        public Digest Digest { get; }

        public LeafCase(Cbor cbor, Digest digest)
        {
            Cbor = cbor;
            Digest = digest;
        }
    }

    /// <summary>
    /// An envelope that wraps another envelope.
    /// </summary>
    public sealed class WrappedCase : EnvelopeCase
    {
        public Envelope Envelope { get; }
        public Digest Digest { get; }

        public WrappedCase(Envelope envelope, Digest digest)
        {
            Envelope = envelope;
            Digest = digest;
        }
    }

    /// <summary>
    /// A predicate-object assertion.
    /// </summary>
    public sealed class AssertionCase : EnvelopeCase
    {
        public Assertion Assertion { get; }

        public AssertionCase(Assertion assertion)
        {
            Assertion = assertion;
        }
    }

    /// <summary>
    /// An envelope that has been elided, leaving only its digest.
    /// </summary>
    public sealed class ElidedCase : EnvelopeCase
    {
        public Digest Digest { get; }

        public ElidedCase(Digest digest)
        {
            Digest = digest;
        }
    }

    /// <summary>
    /// A known value from a namespace of unsigned integers used for ontological concepts.
    /// </summary>
    public sealed class KnownValueCase : EnvelopeCase
    {
        public KnownValue Value { get; }
        public Digest Digest { get; }

        public KnownValueCase(KnownValue value, Digest digest)
        {
            Value = value;
            Digest = digest;
        }
    }

    /// <summary>
    /// An envelope that has been encrypted.
    /// </summary>
    public sealed class EncryptedCase : EnvelopeCase
    {
        public EncryptedMessage EncryptedMessage { get; }

        public EncryptedCase(EncryptedMessage encryptedMessage)
        {
            EncryptedMessage = encryptedMessage;
        }
    }

    /// <summary>
    /// An envelope that has been compressed.
    /// </summary>
    public sealed class CompressedCase : EnvelopeCase
    {
        public Compressed Compressed { get; }

        public CompressedCase(Compressed compressed)
        {
            Compressed = compressed;
        }
    }
}

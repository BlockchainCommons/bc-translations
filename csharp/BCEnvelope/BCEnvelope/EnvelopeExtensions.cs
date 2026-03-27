using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// Extension methods and conversion utilities for creating envelopes from various types.
/// </summary>
public static class EnvelopeExtensions
{
    /// <summary>
    /// Converts any supported value to an <see cref="Envelope"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>An envelope representing the value.</returns>
    /// <exception cref="ArgumentException">Thrown if the type is not supported.</exception>
    public static Envelope ToEnvelope(object value)
    {
        return value switch
        {
            Envelope e => e,
            Assertion a => Envelope.CreateWithAssertion(a),
            KnownValue kv => Envelope.CreateWithKnownValue(kv),
            Cbor cbor => Envelope.CreateLeaf(cbor),
            string s => Envelope.CreateLeaf(Cbor.FromString(s)),
            bool b => Envelope.CreateLeaf(Cbor.FromBool(b)),
            byte v => Envelope.CreateLeaf(Cbor.FromUInt(v)),
            ushort v => Envelope.CreateLeaf(Cbor.FromUInt(v)),
            uint v => Envelope.CreateLeaf(Cbor.FromUInt(v)),
            ulong v => Envelope.CreateLeaf(Cbor.FromUInt(v)),
            sbyte v => Envelope.CreateLeaf(Cbor.FromInt(v)),
            short v => Envelope.CreateLeaf(Cbor.FromInt(v)),
            int v => Envelope.CreateLeaf(Cbor.FromInt(v)),
            long v => Envelope.CreateLeaf(Cbor.FromInt(v)),
            float v => Envelope.CreateLeaf(Cbor.FromFloat(v)),
            double v => Envelope.CreateLeaf(Cbor.FromDouble(v)),
            ByteString bs => Envelope.CreateLeaf(Cbor.FromByteString(bs)),
            CborDate d => Envelope.CreateLeaf(d.TaggedCbor()),
            CborMap m => Envelope.CreateLeaf(new Cbor(CborCase.Map(m))),
            Digest d => Envelope.CreateLeaf(d.TaggedCbor()),
            Salt s => Envelope.CreateLeaf(s.TaggedCbor()),
            Nonce n => Envelope.CreateLeaf(n.TaggedCbor()),
            ARID a => Envelope.CreateLeaf(a.TaggedCbor()),
            BCComponents.URI u => Envelope.CreateLeaf(u.TaggedCbor()),
            BCComponents.UUID u => Envelope.CreateLeaf(u.TaggedCbor()),
            XID x => Envelope.CreateLeaf(x.TaggedCbor()),
            Reference r => Envelope.CreateLeaf(r.TaggedCbor()),
            PublicKeys pk => Envelope.CreateLeaf(pk.TaggedCbor()),
            PrivateKeys pk => Envelope.CreateLeaf(pk.TaggedCbor()),
            PrivateKeyBase pkb => Envelope.CreateLeaf(pkb.TaggedCbor()),
            SealedMessage sm => Envelope.CreateLeaf(sm.TaggedCbor()),
            EncryptedMessage em => CreateFromEncryptedMessage(em),
            EncryptedKey ek => Envelope.CreateLeaf(ek.TaggedCbor()),
            Signature sig => Envelope.CreateLeaf(sig.TaggedCbor()),
            SSKRShare share => Envelope.CreateLeaf(share.TaggedCbor()),
            Compressed comp => CreateFromCompressed(comp),
            Json j => Envelope.CreateLeaf(j.TaggedCbor()),
            Function f => Envelope.CreateLeaf(f.TaggedCbor()),
            Parameter p => Envelope.CreateLeaf(p.TaggedCbor()),
            _ => throw new ArgumentException($"Unsupported type for envelope conversion: {value.GetType().Name}", nameof(value)),
        };
    }

    private static Envelope CreateFromEncryptedMessage(EncryptedMessage em)
    {
        if (em.HasDigest)
            return Envelope.CreateWithEncrypted(em);
        return Envelope.CreateLeaf(em.TaggedCbor());
    }

    private static Envelope CreateFromCompressed(Compressed comp)
    {
        if (comp.HasDigest)
            return Envelope.CreateWithCompressed(comp);
        return Envelope.CreateLeaf(comp.TaggedCbor());
    }

    // ===== Typed extension methods =====

    /// <summary>Converts a string to an envelope.</summary>
    public static Envelope ToEnvelope(this string value) => Envelope.CreateLeaf(Cbor.FromString(value));

    /// <summary>Converts a boolean to an envelope.</summary>
    public static Envelope ToEnvelope(this bool value) => Envelope.CreateLeaf(Cbor.FromBool(value));

    /// <summary>Converts a byte to an envelope.</summary>
    public static Envelope ToEnvelope(this byte value) => Envelope.CreateLeaf(Cbor.FromUInt(value));

    /// <summary>Converts a ushort to an envelope.</summary>
    public static Envelope ToEnvelope(this ushort value) => Envelope.CreateLeaf(Cbor.FromUInt(value));

    /// <summary>Converts a uint to an envelope.</summary>
    public static Envelope ToEnvelope(this uint value) => Envelope.CreateLeaf(Cbor.FromUInt(value));

    /// <summary>Converts a ulong to an envelope.</summary>
    public static Envelope ToEnvelope(this ulong value) => Envelope.CreateLeaf(Cbor.FromUInt(value));

    /// <summary>Converts an sbyte to an envelope.</summary>
    public static Envelope ToEnvelope(this sbyte value) => Envelope.CreateLeaf(Cbor.FromInt(value));

    /// <summary>Converts a short to an envelope.</summary>
    public static Envelope ToEnvelope(this short value) => Envelope.CreateLeaf(Cbor.FromInt(value));

    /// <summary>Converts an int to an envelope.</summary>
    public static Envelope ToEnvelope(this int value) => Envelope.CreateLeaf(Cbor.FromInt(value));

    /// <summary>Converts a long to an envelope.</summary>
    public static Envelope ToEnvelope(this long value) => Envelope.CreateLeaf(Cbor.FromInt(value));

    /// <summary>Converts a float to an envelope.</summary>
    public static Envelope ToEnvelope(this float value) => Envelope.CreateLeaf(Cbor.FromFloat(value));

    /// <summary>Converts a double to an envelope.</summary>
    public static Envelope ToEnvelope(this double value) => Envelope.CreateLeaf(Cbor.FromDouble(value));

    /// <summary>Converts a ByteString to an envelope.</summary>
    public static Envelope ToEnvelope(this ByteString value) => Envelope.CreateLeaf(Cbor.FromByteString(value));

    /// <summary>Converts a CborDate to an envelope.</summary>
    public static Envelope ToEnvelope(this CborDate value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts a Digest to an envelope.</summary>
    public static Envelope ToEnvelope(this Digest value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts a Salt to an envelope.</summary>
    public static Envelope ToEnvelope(this Salt value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts an ARID to an envelope.</summary>
    public static Envelope ToEnvelope(this ARID value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts a URI to an envelope.</summary>
    public static Envelope ToEnvelope(this BCComponents.URI value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts a UUID to an envelope.</summary>
    public static Envelope ToEnvelope(this BCComponents.UUID value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts an XID to an envelope.</summary>
    public static Envelope ToEnvelope(this XID value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts a Reference to an envelope.</summary>
    public static Envelope ToEnvelope(this Reference value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts PublicKeys to an envelope.</summary>
    public static Envelope ToEnvelope(this PublicKeys value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts PrivateKeys to an envelope.</summary>
    public static Envelope ToEnvelope(this PrivateKeys value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts a PrivateKeyBase to an envelope.</summary>
    public static Envelope ToEnvelope(this PrivateKeyBase value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts a SealedMessage to an envelope.</summary>
    public static Envelope ToEnvelope(this SealedMessage value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts an EncryptedKey to an envelope.</summary>
    public static Envelope ToEnvelope(this EncryptedKey value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts a Signature to an envelope.</summary>
    public static Envelope ToEnvelope(this Signature value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts an SSKRShare to an envelope.</summary>
    public static Envelope ToEnvelope(this SSKRShare value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts a KnownValue to an envelope.</summary>
    public static Envelope ToEnvelope(this KnownValue value) => Envelope.CreateWithKnownValue(value);

    /// <summary>Converts an Assertion to an envelope.</summary>
    public static Envelope ToEnvelope(this Assertion value) => Envelope.CreateWithAssertion(value);

    /// <summary>Converts a Json value to an envelope.</summary>
    public static Envelope ToEnvelope(this Json value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts a CborMap to an envelope.</summary>
    public static Envelope ToEnvelope(this CborMap value) => Envelope.CreateLeaf(new Cbor(CborCase.Map(value)));

    /// <summary>Converts a Function to an envelope.</summary>
    public static Envelope ToEnvelope(this Function value) => Envelope.CreateLeaf(value.TaggedCbor());

    /// <summary>Converts a Parameter to an envelope.</summary>
    public static Envelope ToEnvelope(this Parameter value) => Envelope.CreateLeaf(value.TaggedCbor());
}

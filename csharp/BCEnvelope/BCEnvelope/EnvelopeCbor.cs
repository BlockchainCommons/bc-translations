using BlockchainCommons.BCComponents;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope;

/// <summary>
/// CBOR encoding and decoding support for <see cref="Envelope"/>.
/// </summary>
public sealed partial class Envelope : ICborTaggedEncodable, ICborTaggedDecodable
{
    /// <summary>
    /// Returns the CBOR tags associated with the Envelope type.
    /// </summary>
    public static IReadOnlyList<Tag> CborTags =>
        GlobalTags.TagsForValues(BcTags.TagEnvelope);

    /// <summary>
    /// Returns the untagged CBOR representation of this envelope.
    /// </summary>
    /// <remarks>
    /// The CBOR encoding varies by case:
    /// <list type="bullet">
    /// <item>Node: CBOR array (subject followed by assertions)</item>
    /// <item>Leaf: #6.24 tagged encoded-CBOR (or #6.201 leaf tag)</item>
    /// <item>Wrapped: envelope tag wrapping the inner envelope</item>
    /// <item>Assertion: single-entry CBOR map</item>
    /// <item>Elided: 32-byte byte string (the digest)</item>
    /// <item>KnownValue: unsigned integer (untagged)</item>
    /// <item>Encrypted: tagged encrypted message</item>
    /// <item>Compressed: tagged compressed data</item>
    /// </list>
    /// </remarks>
    public Cbor UntaggedCbor()
    {
        return _case switch
        {
            EnvelopeCase.NodeCase n => EncodeNode(n),
            EnvelopeCase.LeafCase l => Cbor.ToTaggedValue(BcTags.TagLeaf, l.Cbor),
            EnvelopeCase.WrappedCase w => w.Envelope.TaggedCbor(),
            EnvelopeCase.AssertionCase a => a.Assertion.ToCbor(e => e.UntaggedCbor()),
            EnvelopeCase.ElidedCase e => e.Digest.UntaggedCbor(),
            EnvelopeCase.KnownValueCase kv => kv.Value.UntaggedCbor(),
            EnvelopeCase.EncryptedCase enc => enc.EncryptedMessage.TaggedCbor(),
            EnvelopeCase.CompressedCase comp => comp.Compressed.TaggedCbor(),
            _ => throw new InvalidOperationException(),
        };
    }

    private static Cbor EncodeNode(EnvelopeCase.NodeCase n)
    {
        var elements = new List<Cbor> { n.Subject.UntaggedCbor() };
        foreach (var assertion in n.Assertions)
            elements.Add(assertion.UntaggedCbor());
        return new Cbor(CborCase.Array(elements));
    }

    /// <summary>
    /// Returns the tagged CBOR representation of this envelope.
    /// </summary>
    public Cbor TaggedCbor() => Cbor.ToTaggedValue(CborTags[0], UntaggedCbor());

    /// <summary>
    /// Returns the tagged CBOR representation.
    /// </summary>
    public Cbor ToCbor() => TaggedCbor();

    /// <summary>
    /// Returns the tagged CBOR data as bytes.
    /// </summary>
    public byte[] TaggedCborData() => TaggedCbor().ToCborData();

    /// <summary>
    /// Decodes an <see cref="Envelope"/> from untagged CBOR.
    /// </summary>
    /// <param name="cbor">The untagged CBOR value.</param>
    /// <returns>A decoded envelope.</returns>
    /// <exception cref="CborException">Thrown if the CBOR cannot be decoded as an envelope.</exception>
    public static Envelope FromUntaggedCbor(Cbor cbor)
    {
        switch (cbor.Case)
        {
            case CborCase.TaggedCase tg:
                return DecodeTagged(tg.Tag, tg.Item, cbor);

            case CborCase.ByteStringCase bs:
                return CreateElided(Digest.FromData(bs.Value.ToArray()));

            case CborCase.ArrayCase arr:
                return DecodeArray(arr.Value);

            case CborCase.MapCase m:
                var assertion = Assertion.FromCborMap(m.Value, FromUntaggedCbor);
                return CreateWithAssertion(assertion);

            case CborCase.UnsignedCase u:
                var knownValue = new KnownValue(u.Value);
                return CreateWithKnownValue(knownValue);

            default:
                throw new CborException("invalid envelope");
        }
    }

    private static Envelope DecodeTagged(Tag tag, Cbor item, Cbor originalCbor)
    {
        switch (tag.Value)
        {
            case BcTags.TagLeaf:
            case BcTags.TagEncodedCbor:
                return CreateLeaf(item);

            case BcTags.TagEnvelope:
                var innerEnvelope = FromTaggedCbor(originalCbor);
                return CreateWrapped(innerEnvelope);

            case BcTags.TagEncrypted:
                var encrypted = EncryptedMessage.FromUntaggedCbor(item);
                return CreateWithEncrypted(encrypted);

            case BcTags.TagCompressed:
                var compressed = Compressed.FromUntaggedCbor(item);
                return CreateWithCompressed(compressed);

            default:
                throw new CborException($"unknown envelope tag: {tag.Value}");
        }
    }

    private static Envelope DecodeArray(IReadOnlyList<Cbor> elements)
    {
        if (elements.Count < 2)
            throw new CborException("node must have at least two elements");

        var subject = FromUntaggedCbor(elements[0]);
        var assertions = new List<Envelope>();
        for (int i = 1; i < elements.Count; i++)
            assertions.Add(FromUntaggedCbor(elements[i]));

        return CreateWithAssertions(subject, assertions);
    }

    /// <summary>
    /// Decodes an <see cref="Envelope"/> from tagged CBOR.
    /// </summary>
    /// <param name="cbor">The tagged CBOR value.</param>
    /// <returns>A decoded envelope.</returns>
    /// <exception cref="CborException">Thrown if the tag does not match or the data is invalid.</exception>
    public static Envelope FromTaggedCbor(Cbor cbor)
    {
        foreach (var tag in CborTags)
        {
            try
            {
                var item = cbor.TryIntoExpectedTaggedValue(tag);
                return FromUntaggedCbor(item);
            }
            catch (CborWrongTagException) { }
            catch (CborWrongTypeException) { }
        }
        throw new CborWrongTypeException();
    }
}

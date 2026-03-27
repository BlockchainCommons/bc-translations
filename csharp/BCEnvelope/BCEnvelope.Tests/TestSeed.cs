using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.BCTags;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope.Tests;

/// <summary>
/// Seed domain object used in envelope tests.
/// </summary>
public sealed class Seed
{
    public byte[] Data { get; }
    public string Name { get; set; }
    public string Note { get; set; }
    public CborDate? CreationDate { get; set; }

    public Seed(byte[] data)
        : this(data, "", "", null) { }

    public Seed(byte[] data, string name, string note, CborDate? creationDate)
    {
        Data = (byte[])data.Clone();
        Name = name;
        Note = note;
        CreationDate = creationDate;
    }

    // --- CBOR encoding ---

    public Cbor TaggedCbor()
    {
        var map = new CborMap();
        map.Insert(Cbor.FromInt(1), Cbor.ToByteString(Data));
        if (CreationDate is { } cd)
            map.Insert(Cbor.FromInt(2), cd.TaggedCbor());
        if (!string.IsNullOrEmpty(Name))
            map.Insert(Cbor.FromInt(3), Cbor.FromString(Name));
        if (!string.IsNullOrEmpty(Note))
            map.Insert(Cbor.FromInt(4), Cbor.FromString(Note));
        var untagged = new Cbor(CborCase.Map(map));
        var tags = GlobalTags.TagsForValues(BcTags.TagSeed);
        return Cbor.ToTaggedValue(tags[0], untagged);
    }

    public static Seed FromTaggedCbor(Cbor cbor)
    {
        var tags = GlobalTags.TagsForValues(BcTags.TagSeed);
        var untagged = cbor.TryIntoExpectedTaggedValue(tags[0]);
        return FromUntaggedCbor(untagged);
    }

    public static Seed FromUntaggedCbor(Cbor cbor)
    {
        var map = cbor.TryIntoMap();
        var dataEntry = map.Extract(Cbor.FromInt(1));
        var data = dataEntry.TryIntoByteString();

        CborDate? creationDate = null;
        var dateEntry = map.GetValue(Cbor.FromInt(2));
        if (dateEntry != null)
            creationDate = CborDate.FromTaggedCbor(dateEntry);

        var nameEntry = map.GetValue(Cbor.FromInt(3));
        var name = nameEntry?.TryIntoText() ?? "";

        var noteEntry = map.GetValue(Cbor.FromInt(4));
        var note = noteEntry?.TryIntoText() ?? "";

        return new Seed(data, name, note, creationDate);
    }

    // --- Envelope encoding ---

    public Envelope ToEnvelope()
    {
        var e = Envelope.Create(new ByteString(Data))
            .AddType(KnownValuesRegistry.SeedType);

        if (CreationDate is not null)
            e = e.AddAssertion(KnownValuesRegistry.Date, CreationDate);

        if (!string.IsNullOrEmpty(Name))
            e = e.AddAssertion(KnownValuesRegistry.Name, Name);

        if (!string.IsNullOrEmpty(Note))
            e = e.AddAssertion(KnownValuesRegistry.Note, Note);

        return e;
    }

    public static Seed FromEnvelope(Envelope envelope)
    {
        envelope.CheckTypeValue(KnownValuesRegistry.SeedType);
        var data = envelope.Subject.TryLeaf().TryIntoByteString();
        var name = envelope.ExtractOptionalObjectForPredicate<string>(KnownValuesRegistry.Name) ?? "";
        var note = envelope.ExtractOptionalObjectForPredicate<string>(KnownValuesRegistry.Note) ?? "";
        var creationDate = envelope.ExtractOptionalObjectForPredicate<CborDate>(KnownValuesRegistry.Date);
        return new Seed(data, name, note, creationDate);
    }
}

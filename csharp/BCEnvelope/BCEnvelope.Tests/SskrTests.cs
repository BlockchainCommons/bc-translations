using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class SskrTests
{
    [Fact]
    public void TestSskr()
    {
        TagsRegistry.RegisterTags();

        // Dan has a cryptographic seed he wants to backup using a social
        // recovery scheme.
        var danSeed = new Seed(
            Convert.FromHexString("59f2293a5bce7d4de59e71b4207ac5d2"),
            "Dark Purple Aqua Love",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
            CborDate.FromString("2021-02-24"));

        // Dan encrypts the seed and then splits the content key into a single
        // group 2-of-3.
        var contentKey = SymmetricKey.New();
        var seedEnvelope = danSeed.ToEnvelope();
        var encryptedSeedEnvelope = seedEnvelope.Wrap().EncryptSubject(contentKey);

        var group = BlockchainCommons.SSKR.GroupSpec.Create(2, 3);
        var spec = BlockchainCommons.SSKR.Spec.Create(1,
            new[] { group });
        var envelopes = encryptedSeedEnvelope.SskrSplit(spec, contentKey);

        // Flattening the array of arrays gives just a single array of all
        // the envelopes to be distributed.
        var sentEnvelopes = envelopes.SelectMany(g => g).ToList();
        var sentUrs = sentEnvelopes
            .Select(e => BCUR.UR.Create("envelope", e.TaggedCbor()))
            .ToList();

        var expectedFormat =
            "ENCRYPTED [\n" +
            "    'sskrShare': SSKRShare\n" +
            "]";
        Assert.Equal(expectedFormat, sentEnvelopes[0].Format());

        // Dan sends one envelope to each of Alice, Bob, and Carol.
        // At some future point, Dan retrieves two of the three envelopes.
        var bobUr = sentUrs[1];
        var carolUr = sentUrs[2];
        var bobEnvelope = Envelope.FromTaggedCbor(
            BCUR.UR.FromUrString(bobUr.ToUrString()).Cbor);
        var carolEnvelope = Envelope.FromTaggedCbor(
            BCUR.UR.FromUrString(carolUr.ToUrString()).Cbor);

        var recoveredEnvelopes = new[] { bobEnvelope, carolEnvelope };
        var recoveredSeedEnvelope =
            Envelope.SskrJoin(recoveredEnvelopes).TryUnwrap();

        var recoveredSeed = Seed.FromEnvelope(recoveredSeedEnvelope);

        // The recovered seed is correct.
        Assert.Equal(danSeed.Data, recoveredSeed.Data);
        Assert.Equal(danSeed.Name, recoveredSeed.Name);
        Assert.Equal(danSeed.Note, recoveredSeed.Note);

        // Attempting to recover with only one of the envelopes won't work.
        Assert.Throws<EnvelopeException>(() =>
            Envelope.SskrJoin(new[] { bobEnvelope }));
    }
}

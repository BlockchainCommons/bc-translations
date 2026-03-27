using System.Text;
using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;
using BlockchainCommons.DCbor;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class MultiPermitTests
{
    [Fact]
    public void TestMultiPermit()
    {
        TagsRegistry.RegisterTags();

        //
        // Alice composes a poem.
        //
        var poemText =
            "At midnight, the clocks sang lullabies to the wandering teacups.";

        //
        // Alice creates a new envelope.
        //
        var originalEnvelope = Envelope.Create(poemText)
            .AddType("poem")
            .AddAssertion("title", "A Song of Ice Cream")
            .AddAssertion("author", "Plonkus the Iridescent")
            .AddAssertion(KnownValuesRegistry.Date, CborDate.FromYmd(2025, 5, 15));

        //
        // Alice signs the envelope with her private key.
        //
        var (alicePrivateKeys, alicePublicKeys) = Keypair.Generate();
        var signedEnvelope = originalEnvelope.Sign(alicePrivateKeys);

        var expectedSignedFormat =
            "{\n" +
            "    \"At midnight, the clocks sang lullabies to the wandering teacups.\" [\n" +
            "        'isA': \"poem\"\n" +
            "        \"author\": \"Plonkus the Iridescent\"\n" +
            "        \"title\": \"A Song of Ice Cream\"\n" +
            "        'date': 2025-05-15\n" +
            "    ]\n" +
            "} [\n" +
            "    'signed': Signature\n" +
            "]";
        Assert.Equal(expectedSignedFormat, signedEnvelope.Format());

        //
        // Alice picks a random symmetric "content key" and uses it to encrypt.
        //
        var contentKey = SymmetricKey.New();
        var encryptedEnvelope = signedEnvelope.Encrypt(contentKey);
        Assert.Equal("ENCRYPTED", encryptedEnvelope.Format());

        //
        // Alice adds a password-based permit.
        //
        var password = Encoding.UTF8.GetBytes("unicorns_dance_on_mars_while_eating_pizza");
        var lockedEnvelope = encryptedEnvelope
            .AddSecret(KeyDerivationMethod.Argon2id, password, contentKey);

        var expectedLockedFormat =
            "ENCRYPTED [\n" +
            "    'hasSecret': EncryptedKey(Argon2id)\n" +
            "]";
        Assert.Equal(expectedLockedFormat, lockedEnvelope.Format());

        //
        // Alice adds recipient permits.
        //
        var (bobPrivateKeys, bobPublicKeys) = Keypair.Generate();
        lockedEnvelope = lockedEnvelope
            .AddRecipient(alicePublicKeys, contentKey)
            .AddRecipient(bobPublicKeys, contentKey);

        var expectedRecipientsFormat =
            "ENCRYPTED [\n" +
            "    'hasRecipient': SealedMessage\n" +
            "    'hasRecipient': SealedMessage\n" +
            "    'hasSecret': EncryptedKey(Argon2id)\n" +
            "]";
        Assert.Equal(expectedRecipientsFormat, lockedEnvelope.Format());

        //
        // Alice creates 2-of-3 SSKR shares.
        //
        var sskrGroup = BlockchainCommons.SSKR.GroupSpec.Create(2, 3);
        var spec = BlockchainCommons.SSKR.Spec.Create(1,
            new[] { sskrGroup });
        var shardedEnvelopes =
            lockedEnvelope.SskrSplitFlattened(spec, contentKey);

        var expectedShardFormat =
            "ENCRYPTED [\n" +
            "    'hasRecipient': SealedMessage\n" +
            "    'hasRecipient': SealedMessage\n" +
            "    'hasSecret': EncryptedKey(Argon2id)\n" +
            "    'sskrShare': SSKRShare\n" +
            "]";
        Assert.Equal(expectedShardFormat, shardedEnvelopes[0].Format());

        //
        // Method 1: Using the content key.
        //
        var receivedEnvelope = shardedEnvelopes[0];
        var unlockedEnvelope = receivedEnvelope.Decrypt(contentKey);
        Assert.Equal(signedEnvelope, unlockedEnvelope);

        //
        // Method 2: Using the password.
        //
        unlockedEnvelope = receivedEnvelope.Unlock(password);
        Assert.Equal(signedEnvelope, unlockedEnvelope);

        //
        // Method 3: Using Alice's private key.
        //
        unlockedEnvelope = receivedEnvelope.DecryptToRecipient(alicePrivateKeys);
        Assert.Equal(signedEnvelope, unlockedEnvelope);

        //
        // Method 4: Using Bob's private key.
        //
        unlockedEnvelope = receivedEnvelope.DecryptToRecipient(bobPrivateKeys);
        Assert.Equal(signedEnvelope, unlockedEnvelope);

        //
        // Method 5: Using SSKR shares (2 of 3).
        //
        unlockedEnvelope = Envelope.SskrJoin(
            new[] { shardedEnvelopes[0], shardedEnvelopes[2] })
            .TryUnwrap();
        Assert.Equal(signedEnvelope, unlockedEnvelope);

        unlockedEnvelope.Verify(alicePublicKeys);
    }
}

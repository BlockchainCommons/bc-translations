using BlockchainCommons.BCComponents;
using BlockchainCommons.BCEnvelope;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class ObscuringTests
{
    private const string PlaintextHello = "Hello.";

    [Fact]
    public void TestObscuring()
    {
        var key = SymmetricKey.New();

        var envelope = Envelope.Create(PlaintextHello);
        Assert.False(envelope.IsObscured);

        var encrypted = envelope.EncryptSubject(key);
        Assert.True(encrypted.IsObscured);

        var elided = envelope.Elide();
        Assert.True(elided.IsObscured);

        var compressed = envelope.Compress();
        Assert.True(compressed.IsObscured);

        // ENCRYPTION

        // Cannot encrypt an encrypted envelope.
        Assert.ThrowsAny<Exception>(() => encrypted.EncryptSubject(key));

        // Cannot encrypt an elided envelope.
        Assert.ThrowsAny<Exception>(() => elided.EncryptSubject(key));

        // OK to encrypt a compressed envelope.
        var encryptedCompressed = compressed.EncryptSubject(key);
        Assert.True(encryptedCompressed.IsEncrypted);

        // ELISION

        // OK to elide an encrypted envelope.
        var elidedEncrypted = encrypted.Elide();
        Assert.True(elidedEncrypted.IsElided);

        // Eliding an elided envelope is idempotent.
        var elidedElided = elided.Elide();
        Assert.True(elidedElided.IsElided);

        // OK to elide a compressed envelope.
        var elidedCompressed = compressed.Elide();
        Assert.True(elidedCompressed.IsElided);

        // COMPRESSION

        // Cannot compress an encrypted envelope.
        Assert.ThrowsAny<Exception>(() => encrypted.Compress());

        // Cannot compress an elided envelope.
        Assert.ThrowsAny<Exception>(() => elided.Compress());

        // Compressing a compressed envelope is idempotent.
        var compressedCompressed = compressed.Compress();
        Assert.True(compressedCompressed.IsCompressed);
    }

    [Fact]
    public void TestNodesMatching()
    {
        var envelope = Envelope.Create("Alice")
            .AddAssertion("knows", "Bob")
            .AddAssertion("age", 30)
            .AddAssertion("city", "Boston");

        // Get some digests for targeting
        var knowsAssertion = envelope.AssertionWithPredicate("knows");
        var knowsDigest = knowsAssertion.GetDigest();

        var ageAssertion = envelope.AssertionWithPredicate("age");
        var ageDigest = ageAssertion.GetDigest();

        // Elide one assertion, compress another
        var elideTarget = new HashSet<Digest> { knowsDigest };

        var compressTarget = new HashSet<Digest> { ageDigest };

        var obscured = envelope.ElideRemovingSet(elideTarget);

        obscured = obscured.ElideRemovingSetWithAction(
            compressTarget,
            ObscureAction.Compress);

        // Verify the structure with elided and compressed nodes
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"city\": \"Boston\"\n" +
            "    COMPRESSED\n" +
            "    ELIDED\n" +
            "]",
            obscured.Format());

        // Test finding elided nodes
        var elidedNodes = obscured.NodesMatching(null, ObscureType.Elided);
        Assert.Contains(knowsDigest, elidedNodes);

        // Test finding compressed nodes
        var compressedNodes = obscured.NodesMatching(null, ObscureType.Compressed);
        Assert.Contains(ageDigest, compressedNodes);

        // Test finding with target filter
        var targetFilter = new HashSet<Digest> { knowsDigest };
        var filtered = obscured.NodesMatching(targetFilter, ObscureType.Elided);
        Assert.Single(filtered);
        Assert.Contains(knowsDigest, filtered);

        // Test finding all obscured nodes (no type filter)
        var allInTarget = obscured.NodesMatching(elideTarget);
        Assert.Single(allInTarget);
        Assert.Contains(knowsDigest, allInTarget);

        // Test with no matches
        var noMatchTarget = new HashSet<Digest> { Digest.FromImage("nonexistent"u8.ToArray()) };
        var noMatches = obscured.NodesMatching(noMatchTarget, ObscureType.Elided);
        Assert.Empty(noMatches);
    }

    [Fact]
    public void TestWalkUnelide()
    {
        var alice = Envelope.Create("Alice");
        var bob = Envelope.Create("Bob");
        var carol = Envelope.Create("Carol");

        var envelope = Envelope.Create("Alice")
            .AddAssertion("knows", "Bob")
            .AddAssertion("friend", "Carol");

        // Elide multiple parts
        var elided = envelope
            .ElideRemovingTarget(alice)
            .ElideRemovingTarget(bob);

        // Verify parts are elided
        Assert.Equal(
            "ELIDED [\n" +
            "    \"friend\": \"Carol\"\n" +
            "    \"knows\": ELIDED\n" +
            "]",
            elided.Format());

        // Restore with walk_unelide
        var restored = elided.WalkUnelide(new[] { alice, bob, carol });

        // The restored envelope should match original
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"friend\": \"Carol\"\n" +
            "    \"knows\": \"Bob\"\n" +
            "]",
            restored.Format());

        // Test with partial restoration (only some envelopes provided)
        var partial = elided.WalkUnelide(new[] { alice });
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"friend\": \"Carol\"\n" +
            "    \"knows\": ELIDED\n" +
            "]",
            partial.Format());

        // Test with no matching envelopes
        var unchanged = elided.WalkUnelide(Array.Empty<Envelope>());
        Assert.True(unchanged.IsIdenticalTo(elided));
    }

    [Fact]
    public void TestWalkDecrypt()
    {
        var key1 = SymmetricKey.New();
        var key2 = SymmetricKey.New();
        var key3 = SymmetricKey.New();

        var envelope = Envelope.Create("Alice")
            .AddAssertion("knows", "Bob")
            .AddAssertion("age", 30)
            .AddAssertion("city", "Boston");

        // Encrypt different parts with different keys
        var knowsAssertion = envelope.AssertionWithPredicate("knows");
        var ageAssertion = envelope.AssertionWithPredicate("age");

        var encrypt1Target = new HashSet<Digest> { knowsAssertion.GetDigest() };
        var encrypt2Target = new HashSet<Digest> { ageAssertion.GetDigest() };

        var encrypted = envelope
            .ElideRemovingSetWithAction(
                encrypt1Target,
                ObscureAction.Encrypt(key1))
            .ElideRemovingSetWithAction(
                encrypt2Target,
                ObscureAction.Encrypt(key2));

        // Verify parts are encrypted
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"city\": \"Boston\"\n" +
            "    ENCRYPTED (2)\n" +
            "]",
            encrypted.Format());

        // Decrypt with all keys
        var decrypted = encrypted.WalkDecrypt(new[] { key1, key2 });
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"age\": 30\n" +
            "    \"city\": \"Boston\"\n" +
            "    \"knows\": \"Bob\"\n" +
            "]",
            decrypted.Format());

        // Decrypt with only one key (partial decryption)
        var partial = encrypted.WalkDecrypt(new[] { key1 });
        Assert.False(partial.IsIdenticalTo(encrypted));
        // Note: partial is still equivalent because encrypted nodes preserve
        // digests
        Assert.True(partial.IsEquivalentTo(envelope));

        // There should still be one encrypted node remaining
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"city\": \"Boston\"\n" +
            "    \"knows\": \"Bob\"\n" +
            "    ENCRYPTED\n" +
            "]",
            partial.Format());

        // Decrypt with wrong key (should be unchanged)
        var unchanged = encrypted.WalkDecrypt(new[] { key3 });
        Assert.True(unchanged.IsIdenticalTo(encrypted));
    }

    [Fact]
    public void TestWalkDecompress()
    {
        var envelope = Envelope.Create("Alice")
            .AddAssertion("knows", "Bob")
            .AddAssertion("bio", new string('A', 1000))
            .AddAssertion("description", new string('B', 1000));

        // Compress multiple parts
        var bioAssertion = envelope.AssertionWithPredicate("bio");
        var descAssertion = envelope.AssertionWithPredicate("description");

        var bioDigest = bioAssertion.GetDigest();
        var descDigest = descAssertion.GetDigest();

        var compressTarget = new HashSet<Digest> { bioDigest, descDigest };

        var compressed = envelope.ElideRemovingSetWithAction(
            compressTarget,
            ObscureAction.Compress);

        // Verify parts are compressed
        Assert.Equal(
            "\"Alice\" [\n" +
            "    \"knows\": \"Bob\"\n" +
            "    COMPRESSED (2)\n" +
            "]",
            compressed.Format());

        // decompress all
        var decompressed = compressed.WalkDecompress();
        Assert.True(decompressed.IsEquivalentTo(envelope));

        // Decompress with target filter (only one node)
        var target = new HashSet<Digest> { bioDigest };

        var partialDecomp = compressed.WalkDecompress(target);
        Assert.False(partialDecomp.IsIdenticalTo(compressed));
        // Note: partial is still equivalent because compressed nodes preserve
        // digests
        Assert.True(partialDecomp.IsEquivalentTo(envelope));

        // Bio should be decompressed but description still compressed
        var stillCompressed = partialDecomp.NodesMatching(null, ObscureType.Compressed);
        Assert.Contains(descDigest, stillCompressed);
        Assert.DoesNotContain(bioDigest, stillCompressed);

        // Decompress with non-matching target (should be unchanged)
        var noMatch = new HashSet<Digest> { Digest.FromImage("nonexistent"u8.ToArray()) };
        var unchangedDecomp = compressed.WalkDecompress(noMatch);
        Assert.True(unchangedDecomp.IsIdenticalTo(compressed));
    }

    [Fact]
    public void TestMixedObscurationOperations()
    {
        var key = SymmetricKey.New();

        var envelope = Envelope.Create("Alice")
            .AddAssertion("knows", "Bob")
            .AddAssertion("age", 30)
            .AddAssertion("bio", new string('A', 1000));

        var knowsAssertion = envelope.AssertionWithPredicate("knows");
        var ageAssertion = envelope.AssertionWithPredicate("age");
        var bioAssertion = envelope.AssertionWithPredicate("bio");

        var knowsDigest = knowsAssertion.GetDigest();
        var ageDigest = ageAssertion.GetDigest();
        var bioDigest = bioAssertion.GetDigest();

        // Apply different obscuration types
        var elideTarget = new HashSet<Digest> { knowsDigest };
        var encryptTarget = new HashSet<Digest> { ageDigest };
        var compressTarget = new HashSet<Digest> { bioDigest };

        var obscured = envelope
            .ElideRemovingSet(elideTarget)
            .ElideRemovingSetWithAction(
                encryptTarget,
                ObscureAction.Encrypt(key))
            .ElideRemovingSetWithAction(
                compressTarget,
                ObscureAction.Compress);

        // Verify different obscuration types
        var elidedNodes = obscured.NodesMatching(null, ObscureType.Elided);
        var encryptedNodes = obscured.NodesMatching(null, ObscureType.Encrypted);
        var compressedNodes = obscured.NodesMatching(null, ObscureType.Compressed);

        Assert.Contains(knowsDigest, elidedNodes);
        Assert.Contains(ageDigest, encryptedNodes);
        Assert.Contains(bioDigest, compressedNodes);

        // Restore everything
        var restored = obscured
            .WalkUnelide(new[] { knowsAssertion })
            .WalkDecrypt(new[] { key })
            .WalkDecompress();

        Assert.True(restored.IsEquivalentTo(envelope));
    }
}
